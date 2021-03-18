using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BiangLibrary;
using BiangLibrary.GameDataFormat;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.GamePlay.UI;
using BiangLibrary.Messenger;
using BiangLibrary.Singleton;
using UnityEngine;

public partial class BattleManager : TSingletonBaseManager<BattleManager>
{
    public SRandom SRandom = new SRandom(12345);
    public Messenger BattleMessenger = new Messenger();

    internal PlayerActor[] MainPlayers = new PlayerActor[2];
    internal PlayerActor Player1 => MainPlayers[(int) PlayerNumber.Player1];
    internal PlayerActor Player2 => MainPlayers[(int) PlayerNumber.Player2];

    internal List<EnemyActor> Enemies = new List<EnemyActor>();
    internal SortedDictionary<uint, Actor> ActorDict = new SortedDictionary<uint, Actor>(); // Key: ActorGUID

    #region 分模组记录模组所属的Actor信息

    internal Dictionary<uint, HashSet<uint>> WorldModuleActorDict = new Dictionary<uint, HashSet<uint>>(); // Key: WorldModuleGUID, Value: HashSet<ActorGUID>

    private void RegisterEnemyToWorldModule(uint worldModuleGUID, uint actorGUID)
    {
        if (!WorldModuleActorDict.ContainsKey(worldModuleGUID))
        {
            WorldModuleActorDict.Add(worldModuleGUID, new HashSet<uint>());
        }

        WorldModuleActorDict[worldModuleGUID].Add(actorGUID);
    }

    private void UnregisterEnemyFromWorldModule(uint actorGUID)
    {
        if (ActorDict.TryGetValue(actorGUID, out Actor actor))
        {
            if (WorldModuleActorDict.TryGetValue(actor.InitWorldModuleGUID, out HashSet<uint> dict))
            {
                if (dict.Remove(actorGUID))
                {
                    if (dict.Count == 0)
                    {
                        ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, $"WorldModule_{actor.InitWorldModuleGUID}_EnemyClear");
                        WorldModuleActorDict.Remove(actor.InitWorldModuleGUID);
                    }
                }
                else
                {
                    Debug.LogError("角色未注册进世界模组字典");
                }
            }
            else
            {
                Debug.LogError("未注册过的世界模组GUID");
            }
        }
        else
        {
            Debug.LogError("未注册过角色");
        }
    }

    #endregion

    public Transform ActorContainerRoot;
    public Transform NavTrackMarkerRoot;

    public void Clear()
    {
        foreach (EnemyActor enemy in Enemies.ToArray().ToList())
        {
            enemy.PoolRecycle();
        }

        Enemies.Clear();

        for (int index = 0; index < MainPlayers.Length; index++)
        {
            MainPlayers[index]?.PoolRecycle();
            MainPlayers[index] = null;
        }

        ActorDict.Clear();
        WorldModuleActorDict.Clear();

        BattleMessenger.Cleanup();
        BattleStateBoolDict.Clear();
    }

    public override void Awake()
    {
        Clear();
        ActorContainerRoot = new GameObject("ActorContainerRoot").transform;
        NavTrackMarkerRoot = new GameObject("NavTrackMarkerRoot").transform;
    }

    public bool IsStart = false;

    public override void Start()
    {
    }

    public override void Update(float deltaTime)
    {
    }

    public void StartBattle()
    {
        LoadActors();
        GameStateManager.Instance.SetState(GameState.Fighting);
        ClientGameManager.Instance.BattleMessenger.AddListener<string>((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, OnTriggerLevelEventCreateActor);
        IsStart = true;
    }

    private void LoadActors()
    {
        CameraManager.Instance.FieldCamera.InitFocus();
    }

    private void OnTriggerLevelEventCreateActor(string eventAlias)
    {
        WorldBornPointGroupData_Runtime data = WorldManager.Instance.CurrentWorld.WorldData.WorldBornPointGroupData_Runtime;
        if (data.EnemyBornPointDataAliasDict.TryGetValue(eventAlias, out BornPointData bp))
        {
            CreateActorByBornPointData(bp);
        }
    }

    public IEnumerator CreateActorByBornPointDataList(List<BornPointData> bps, bool ignorePlayer = true)
    {
        foreach (BornPointData bp in bps)
        {
            if (ignorePlayer && bp.ActorCategory == ActorCategory.Player) continue;
            CreateActorByBornPointData(bp);
            yield return null;
        }
    }

    public void CreateActorByBornPointData(BornPointData bpd)
    {
        if (bpd.RawEntityExtraSerializeData != null)
        {
            foreach (EntityPassiveSkill eps in bpd.RawEntityExtraSerializeData.EntityPassiveSkills)
            {
                if (eps is EntityPassiveSkill_ProbablyShow ps)
                {
                    if (!ps.ShowProbabilityPercent.ProbabilityBool()) return;
                }
            }
        }

        if (bpd.ActorCategory == ActorCategory.Player)
        {
            PlayerNumber playerNumber = (PlayerNumber) Enum.Parse(typeof(PlayerNumber), bpd.ActorTypeName);
            PlayerActor player = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.Player].AllocateGameObject<PlayerActor>(ActorContainerRoot);
            GridPos3D.ApplyGridPosToLocalTrans(bpd.WorldGP, player.transform, 1);
            player.BornPointDataGUID = bpd.GUID;
            player.WorldGP = bpd.WorldGP;
            player.Setup(bpd.ActorTypeName, bpd.ActorCategory, playerNumber, 0);
            BattleMessenger.Broadcast((uint) Enum_Events.OnPlayerLoaded, (Actor) player);
            MainPlayers[(int) playerNumber] = player;
            AddActor(null, player);
            UIManager.Instance.ShowUIForms<PlayerStatHUDPanel>().Initialize();
            UIManager.Instance.CloseUIForm<PlayerStatHUDPanel>();
        }
        else
        {
            WorldModule worldModule = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(bpd.WorldGP);
            if (worldModule != null)
            {
                ushort enemyTypeIndex = ConfigManager.GetEnemyTypeIndex(bpd.ActorTypeName);
                EnemyActor enemy = GameObjectPoolManager.Instance.EnemyDict[enemyTypeIndex].AllocateGameObject<EnemyActor>(ActorContainerRoot);
                GridPos3D.ApplyGridPosToLocalTrans(bpd.WorldGP, enemy.transform, 1);
                enemy.BornPointDataGUID = bpd.GUID;
                enemy.WorldGP = bpd.WorldGP;
                enemy.Setup(bpd.ActorTypeName, bpd.ActorCategory, bpd.ActorOrientation, worldModule.GUID);
                enemy.ApplyBoxExtraSerializeData(bpd.RawEntityExtraSerializeData);
                Enemies.Add(enemy);
                AddActor(worldModule, enemy);
            }
        }
    }

    /// <summary>
    /// 按地理位置销毁角色，非死亡
    /// </summary>
    /// <param name="moduleGP"></param>
    public void DestroyActorByModuleGP_OpenWorldModuleRecycle(GridPos3D moduleGP)
    {
        WorldModule module = WorldManager.Instance.CurrentWorld.WorldModuleMatrix[moduleGP.x, moduleGP.y, moduleGP.z];
        List<Actor> cachedDyingActors = new List<Actor>(32);
        foreach (EnemyActor enemy in Enemies)
        {
            GridPos3D actorModuleGP = WorldManager.Instance.CurrentWorld.GetModuleGPByWorldGP(enemy.WorldGP);
            if (actorModuleGP == moduleGP) cachedDyingActors.Add(enemy);
        }

        foreach (Actor cachedDyingActor in cachedDyingActors)
        {
            if (cachedDyingActor == Player1 || cachedDyingActor == Player2) continue;
            cachedDyingActor.ActorBattleHelper.DestroyActor(null, true);
        }

        cachedDyingActors.Clear();
    }

    private void AddActor(WorldModule worldModule, Actor actor)
    {
        if (actor is EnemyActor enemy) RegisterEnemyToWorldModule(worldModule.GUID, enemy.GUID);
        ActorDict.Add(actor.GUID, actor);
    }

    /// <summary>
    /// 死亡
    /// </summary>
    /// <param name="actor"></param>
    public void RemoveActor(Actor actor)
    {
        if (actor is EnemyActor enemy)
        {
            UnregisterEnemyFromWorldModule(enemy.GUID);
            Enemies.Remove(enemy);
        }

        ActorDict.Remove(actor.GUID);
    }

    public void SetAllActorShown(bool shown)
    {
        for (int i = 0; i < MainPlayers.Length; i++)
        {
            MainPlayers[i]?.SetShown(shown);
        }

        foreach (EnemyActor enemy in Enemies)
        {
            enemy.SetShown(shown);
        }
    }

    private List<Actor> cachedSearchActorList = new List<Actor>(32);

    public Actor SearchNearestActor(Vector3 center, Camp executeCamp, float radius, RelativeCamp effectiveOnRelativeCamp, string actorTypeName = "")
    {
        float minDist = float.MaxValue;
        Actor nearestActor = null;
        int layerMask = LayerManager.Instance.GetTargetEntityLayerMask(executeCamp, effectiveOnRelativeCamp);
        Collider[] colliders = Physics.OverlapSphere(center, radius, layerMask);
        foreach (Collider collider in colliders)
        {
            Actor actor = collider.gameObject.GetComponentInParent<Actor>();
            if (actor.IsNotNullAndAlive())
            {
                if (!string.IsNullOrWhiteSpace(actorTypeName))
                {
                    if (actor.ActorType != actorTypeName)
                    {
                        continue;
                    }
                }

                float dist = (actor.EntityBaseCenter - center).magnitude;
                if (minDist > dist)
                {
                    minDist = dist;
                    nearestActor = actor;
                }
            }
        }

        return nearestActor;
    }

    public List<Actor> SearchActors(Vector3 center, Camp executeCamp, float radius, RelativeCamp effectiveOnRelativeCamp)
    {
        cachedSearchActorList.Clear();
        int layerMask = LayerManager.Instance.GetTargetEntityLayerMask(executeCamp, effectiveOnRelativeCamp);
        Collider[] colliders = Physics.OverlapSphere(center, radius, layerMask);
        foreach (Collider collider in colliders)
        {
            Actor actor = collider.gameObject.GetComponentInParent<Actor>();
            if (actor.IsNotNullAndAlive()) cachedSearchActorList.Add(actor);
        }

        return cachedSearchActorList;
    }

    #region Buff

    public void AddBuffToEntities(Vector3 center, Camp executeCamp, float radius, bool exactGPDistance, RelativeCamp effectiveOnRelativeCamp, List<EntityBuff> entityBuffs, HashSet<uint> entityGUIDSet)
    {
        int layerMask = LayerManager.Instance.GetTargetEntityLayerMask(executeCamp, effectiveOnRelativeCamp);
        Collider[] colliders = Physics.OverlapSphere(exactGPDistance ? center.ToGridPos3D() : center, radius, layerMask);
        foreach (Collider collider in colliders)
        {
            if (exactGPDistance)
            {
                if ((collider.transform.position - center.ToGridPos3D()).magnitude > radius)
                    continue;
            }

            Entity entity = collider.gameObject.GetComponentInParent<Entity>();
            if (entity.IsNotNullAndAlive() && !entityGUIDSet.Contains(entity.GUID))
            {
                entityGUIDSet.Add(entity.GUID);
                foreach (EntityBuff entityBuff in entityBuffs)
                {
                    if (entity.IsNotNullAndAlive())
                    {
                        if (!entity.EntityBuffHelper.AddBuff(entityBuff.Clone()))
                        {
                            //Debug.Log($"Failed to AddBuff: {entityBuff.GetType().Name} to {entity.name}");
                        }
                    }
                }
            }
        }
    }

    #endregion

    public void LoseGame()
    {
        ClientGameManager.Instance.StartCoroutine(Co_LoseGame());
    }

    public void WinGame()
    {
        ClientGameManager.Instance.StartCoroutine(Co_WinGame());
    }

    IEnumerator Co_LoseGame()
    {
        Player1.ForbidAction = true;
        WinLosePanel panel = UIManager.Instance.ShowUIForms<WinLosePanel>();
        yield return panel.Co_LoseGame();
        if (WorldManager.Instance.CurrentWorld is OpenWorld openWorld)
        {
            openWorld.ReturnToOpenWorldFormMicroWorld(true);
        }
        else
        {
            Player1.ForbidAction = true;
            ClientGameManager.Instance.ReloadGame();
        }
    }

    IEnumerator Co_WinGame()
    {
        WinLosePanel panel = UIManager.Instance.ShowUIForms<WinLosePanel>();
        yield return panel.Co_WinGame();
        ClientGameManager.Instance.ReloadGame();
    }

    public override void ShutDown()
    {
        base.ShutDown();
        IsStart = false;
        Clear();
    }
}
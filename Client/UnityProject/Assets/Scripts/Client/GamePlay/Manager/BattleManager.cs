using System.Collections;
using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.GamePlay.UI;
using BiangLibrary.Messenger;
using BiangLibrary.Singleton;
using UnityEngine;

public partial class BattleManager : TSingletonBaseManager<BattleManager>
{
    public Messenger BattleMessenger = new Messenger();

    internal Actor[] MainPlayers = new Actor[2];
    internal Actor Player1 => MainPlayers[(int) PlayerNumber.Player1];
    internal Actor Player2 => MainPlayers[(int) PlayerNumber.Player2];

    internal List<Actor> Enemies = new List<Actor>();
    internal SortedDictionary<uint, Actor> ActorDict = new SortedDictionary<uint, Actor>(); // Key: ActorGUID

    #region 战斗状态

    internal Dictionary<uint, CombatState> ActorCombatStateDict = new Dictionary<uint, CombatState>(); // Key: ActorGUID

    public void SetActorInCombat(uint actorGUID, CombatState newCombatState)
    {
        void RefreshCombatState()
        {
            CombatState = CombatState.Exploring;
            foreach (KeyValuePair<uint, CombatState> kv in ActorCombatStateDict)
            {
                if (kv.Value > CombatState)
                {
                    CombatState = kv.Value;
                }
            }

            WwiseAudioManager.Instance.WwiseBGMConfiguration.SetCombatState(CombatState);
        }

        if (ActorCombatStateDict.TryGetValue(actorGUID, out CombatState oldCombatState))
        {
            if (newCombatState == CombatState.Exploring)
            {
                ActorCombatStateDict.Remove(actorGUID);
                WwiseAudioManager.Instance.WwiseBGMConfiguration.CombatEnemyNumber.SetGlobalValue(ActorCombatStateDict.Count);
            }
            if (newCombatState != oldCombatState)
            {
                ActorCombatStateDict[actorGUID] = newCombatState;
                if (newCombatState >= CombatState || oldCombatState >= CombatState)
                {
                    RefreshCombatState();
                }
            }
        }
        else
        {
            if (newCombatState == CombatState.Exploring) return;
            ActorCombatStateDict.Add(actorGUID, newCombatState);
            WwiseAudioManager.Instance.WwiseBGMConfiguration.CombatEnemyNumber.SetGlobalValue(ActorCombatStateDict.Count);
            if (newCombatState > CombatState)
            {
                RefreshCombatState();
            }
        }
    }

    public CombatState CombatState;

    #endregion

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
        Enemies.Clear();

        for (int index = 0; index < MainPlayers.Length; index++)
        {
            MainPlayers[index] = null;
        }

        ActorDict.Clear();
        ActorCombatStateDict.Clear();
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

    private bool isStart = false;

    public bool IsStart
    {
        get { return isStart; }
        set
        {
            if (isStart != value)
            {
                //Debug.Log($"BattleManager.IsStart = {value}");
                isStart = value;
                SetAllActorForbidAction(!isStart);
            }
        }
    }

    public override void Start()
    {
    }

    public override void Update(float deltaTime)
    {
    }

    public void StartBattle()
    {
        WorldManager.Instance.CurrentWorld.ApplyWorldVisualEffectSettings(WorldManager.Instance.CurrentWorld.WorldData);
        if (WorldManager.Instance.CurrentWorld.WorldData.UseSpecialPlayerEnterESPS)
        {
            Player1.ReloadESPS(WorldManager.Instance.CurrentWorld.WorldData.Raw_PlayerEnterESPS);
        }

        GameStateManager.Instance.SetState(GameState.Fighting);
        IsStart = true;
    }

    public void CreatePlayerByBornPointData(BornPointData bpd)
    {
        if (MainPlayers[0] != null) return;
        Actor player = GameObjectPoolManager.Instance.ActorDict[ConfigManager.Actor_PlayerIndex].AllocateGameObject<Actor>(ActorContainerRoot);
        GridPos3D.ApplyGridPosToLocalTrans(bpd.WorldGP, player.transform, 1);
        player.Setup(new EntityData(ConfigManager.Actor_PlayerIndex, GridPosR.Orientation.Up), bpd.WorldGP, 0);
        AddActor(null, player);
        PlayerDefaultActorSkillLearningData = player.ActorSkillLearningHelper.ActorSkillLearningData.Clone();
        PlayerCurrentActorSkillLearningData = player.ActorSkillLearningHelper.ActorSkillLearningData;
    }

    public void AddActor(WorldModule worldModule, Actor actor)
    {
        if (actor.ActorCategory == ActorCategory.Player)
        {
            BattleMessenger.Broadcast((uint) Enum_Events.OnPlayerLoaded, (Actor) actor);
            MainPlayers[0] = actor;
            ClientGameManager.Instance.PlayerStatHUDPanel.Initialize();
        }
        else if (actor.ActorCategory == ActorCategory.Creature)
        {
            Enemies.Add(actor);
            RegisterEnemyToWorldModule(worldModule.GUID, actor.GUID);
        }

        ActorDict.Add(actor.GUID, actor);
    }

    /// <summary>
    /// 死亡
    /// </summary>
    /// <param name="actor"></param>
    public void RemoveActor(Actor actor)
    {
        if (actor.ActorCategory == ActorCategory.Creature)
        {
            UnregisterEnemyFromWorldModule(actor.GUID);
            Enemies.Remove(actor);
        }

        ActorDict.Remove(actor.GUID);
        ActorCombatStateDict.Remove(actor.GUID);
    }

    private void SetAllActorForbidAction(bool forbidAction)
    {
        foreach (KeyValuePair<uint, Actor> kv in ActorDict)
        {
            kv.Value.ForbidAction = forbidAction;
        }
    }

    public void SetAllActorShown(bool shown)
    {
        for (int i = 0; i < MainPlayers.Length; i++)
        {
            MainPlayers[i]?.SetShown(shown);
        }

        foreach (Actor enemy in Enemies)
        {
            enemy.SetShown(shown);
        }
    }

    private List<Actor> cachedSearchActorList = new List<Actor>(32);
    private Collider[] cachedColliders = new Collider[128];

    public Actor SearchNearestActor(Vector3 center, Camp executeCamp, float radius, RelativeCamp effectiveOnRelativeCamp, List<TerrainType> validTerrainTypes, string actorTypeName = "")
    {
        float minDist = float.MaxValue;
        Actor nearestActor = null;
        int layerMask = LayerManager.Instance.GetTargetEntityLayerMask(executeCamp, effectiveOnRelativeCamp);
        int count = Physics.OverlapSphereNonAlloc(center, radius, cachedColliders, layerMask);
        for (int i = 0; i < count; i++)
        {
            Collider collider = cachedColliders[i];
            if (collider != null)
            {
                Actor actor = collider.gameObject.GetComponentInParent<Actor>();
                if (actor.IsNotNullAndAlive() && actor.CanBeThreatened)
                {
                    if (!string.IsNullOrWhiteSpace(actorTypeName))
                    {
                        if (actor.ActorType != actorTypeName)
                        {
                            continue;
                        }
                    }

                    bool terrainValid = WorldManager.Instance.CurrentWorld.TerrainValid(actor.WorldGP, validTerrainTypes);
                    if (terrainValid)
                    {
                        float dist = (actor.EntityBaseCenter - center).magnitude;
                        if (minDist > dist)
                        {
                            minDist = dist;
                            nearestActor = actor;
                        }
                    }
                }
            }
        }

        return nearestActor;
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

    #region Skills

    public ActorSkillLearningData PlayerDefaultActorSkillLearningData; // 玩家初始化后克隆出来的备份数据
    public ActorSkillLearningData PlayerCurrentActorSkillLearningData; // 实时引用

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
        IsStart = false;
        WwiseAudioManager.Instance.Trigger_PlayerDeath.Post(WwiseAudioManager.Instance.WwiseBGMConfiguration.gameObject);
        SetAllActorForbidAction(true);
        WinLosePanel panel = UIManager.Instance.ShowUIForms<WinLosePanel>();
        yield return panel.Co_LoseGame();
        if (WorldManager.Instance.CurrentWorld is OpenWorld openWorld)
        {
            if (openWorld.IsInsideMicroWorld)
            {
                openWorld.RestartMicroWorld(true);
            }
            else
            {
                openWorld.ReturnToOpenWorldFormMicroWorld(true);
            }
        }
        else
        {
            yield return ClientGameManager.Instance.ReloadGame();
        }
    }

    IEnumerator Co_WinGame()
    {
        WinLosePanel panel = UIManager.Instance.ShowUIForms<WinLosePanel>();
        yield return panel.Co_WinGame();
        yield return ClientGameManager.Instance.ReloadGame();
    }

    public override void ShutDown()
    {
        base.ShutDown();
        IsStart = false;
        Clear();
    }
}
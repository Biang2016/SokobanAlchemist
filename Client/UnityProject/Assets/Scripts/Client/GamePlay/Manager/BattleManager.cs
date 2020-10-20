using System;
using System.Collections;
using System.Collections.Generic;
using BiangStudio.GameDataFormat.Grid;
using BiangStudio.GamePlay.UI;
using BiangStudio.Messenger;
using BiangStudio.Singleton;
using UnityEngine;

public class BattleManager : TSingletonBaseManager<BattleManager>
{
    public Messenger BattleMessenger = new Messenger();

    internal PlayerActor[] MainPlayers = new PlayerActor[2];
    internal PlayerActor Player1 => MainPlayers[(int) PlayerNumber.Player1];
    internal PlayerActor Player2 => MainPlayers[(int) PlayerNumber.Player2];

    internal List<EnemyActor> Enemies = new List<EnemyActor>();
    internal SortedDictionary<uint, Actor> ActorDict = new SortedDictionary<uint, Actor>();

    public Transform ActorContainerRoot;
    public Transform NavTrackMarkerRoot;

    public NoticePanel NoticePanel;

    public void Clear()
    {
        foreach (EnemyActor enemy in Enemies)
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

        BattleMessenger.Cleanup();
    }

    public override void Awake()
    {
        Clear();
        ActorContainerRoot = new GameObject("ActorContainerRoot").transform;
        NavTrackMarkerRoot = new GameObject("NavTrackMarkerRoot").transform;
    }

    public override void Start()
    {
    }

    public override void Update(float deltaTime)
    {
    }

    public void StartBattle()
    {
        LoadActors();
        NoticePanel = UIManager.Instance.ShowUIForms<NoticePanel>();
        UIManager.Instance.ShowUIForms<PlayerHUDPanel>().Initialize();
        GameStateManager.Instance.SetState(GameState.Fighting);
    }

    private void LoadActors()
    {
        CameraManager.Instance.FieldCamera.InitFocus();
    }

    public void CreateActorsByBornPointGroupData(BornPointGroupData dataGroup, WorldModule parentModule = null)
    {
        foreach (BornPointData bpd in dataGroup.BornPoints)
        {
            if (string.IsNullOrEmpty(bpd.SpawnLevelEventAlias))
            {
                CreateActorByBornPointData(bpd, parentModule);
            }
            else
            {
                ClientGameManager.Instance.BattleMessenger.AddListener((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, (string eventAlias) =>
                {
                    if (eventAlias == bpd.SpawnLevelEventAlias)
                    {
                        CreateActorByBornPointData(bpd, parentModule);
                    }
                });
            }
        }
    }

    public void CreateActorByBornPointData(BornPointData bpd, WorldModule parentModule = null)
    {
        if (bpd.ActorCategory == ActorCategory.Player)
        {
            PlayerNumber playerNumber = (PlayerNumber) Enum.Parse(typeof(PlayerNumber), bpd.ActorType);
            PlayerActor player = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.Player].AllocateGameObject<PlayerActor>(ActorContainerRoot);
            GridPos3D worldGP = parentModule ? parentModule.LocalGPToWorldGP(bpd.LocalGP) : bpd.WorldGP;
            GridPos3D.ApplyGridPosToLocalTrans(worldGP, player.transform, 1);
            player.Initialize(bpd.ActorType, bpd.ActorCategory, playerNumber);
            BattleMessenger.Broadcast((uint) Enum_Events.OnPlayerLoaded, (Actor) player);
            MainPlayers[(int) playerNumber] = player;
            AddActor(player);
        }
        else
        {
            ushort enemyTypeIndex = ConfigManager.GetEnemyTypeIndex(bpd.ActorType);
            EnemyActor enemy = GameObjectPoolManager.Instance.EnemyDict[enemyTypeIndex].AllocateGameObject<EnemyActor>(ActorContainerRoot);
            GridPos3D worldGP = parentModule ? parentModule.LocalGPToWorldGP(bpd.LocalGP) : bpd.WorldGP;
            GridPos3D.ApplyGridPosToLocalTrans(worldGP, enemy.transform, 1);
            enemy.Initialize(bpd.ActorType, bpd.ActorCategory);
            Enemies.Add(enemy);
            AddActor(enemy);
        }
    }

    private void AddActor(Actor actor)
    {
        ActorDict.Add(actor.GUID, actor);
    }

    private void RemoveActor(Actor actor)
    {
        ActorDict.Remove(actor.GUID);
    }

    public Actor FindActor(uint actorGUID)
    {
        ActorDict.TryGetValue(actorGUID, out Actor actor);
        return actor;
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
        WinLosePanel panel = UIManager.Instance.ShowUIForms<WinLosePanel>();
        yield return panel.Co_LoseGame();
        ClientGameManager.Instance.ReloadGame();
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
        Clear();
    }
}
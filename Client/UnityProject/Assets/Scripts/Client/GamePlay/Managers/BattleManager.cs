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
        UIManager.Instance.ShowUIForms<PlayerHUDPanel>().Initialize();
        GameStateManager.Instance.SetState(GameState.Fighting);
    }

    private void LoadActors()
    {
        foreach (BornPointData bpd in WorldManager.Instance.CurrentWorld.WorldData.WorldActorData.BornPoints)
        {
            if (bpd.BornPointType == BornPointType.Player)
            {
                PlayerActor player = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.Player].AllocateGameObject<PlayerActor>(ActorContainerRoot);
                GridPos3D.ApplyGridPosToLocalTrans(bpd.GridPos3D, player.transform, 1);
                player.Initialize(bpd.PlayerNumber);
                BattleMessenger.Broadcast((uint) Enum_Events.OnPlayerLoaded, (Actor) player);
                MainPlayers[(int) bpd.PlayerNumber] = player;
                AddActor(player);
            }

            if (bpd.BornPointType == BornPointType.Enemy)
            {
                ushort enemyTypeIndex = ConfigManager.GetEnemyTypeIndex(bpd.EnemyName);
                EnemyActor enemy = GameObjectPoolManager.Instance.EnemyDict[enemyTypeIndex].AllocateGameObject<EnemyActor>(ActorContainerRoot);
                GridPos3D.ApplyGridPosToLocalTrans(bpd.GridPos3D, enemy.transform, 1);
                enemy.Initialize();
                Enemies.Add(enemy);
                AddActor(enemy);
            }
        }

        CameraManager.Instance.FieldCamera.InitFocus();
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
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
    internal List<EnemyActor> Enemies = new List<EnemyActor>();

    public Transform ActorContainerRoot;

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
    }

    public override void Awake()
    {
        ActorContainerRoot = new GameObject("ActorContainerRoot").transform;
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
                player.Initialize(bpd.PlayerNumber);
                GridPos3D.ApplyGridPosToLocalTrans(bpd.GridPos3D, player.transform, 1);
                BattleMessenger.Broadcast((uint) Enum_Events.OnPlayerLoaded, (Actor) player);
                MainPlayers[(int) bpd.PlayerNumber] = player;
            }

            if (bpd.BornPointType == BornPointType.Enemy)
            {
                byte enemyTypeIndex = ConfigManager.GetEnemyTypeIndex(bpd.EnemyName);
                EnemyActor enemy = GameObjectPoolManager.Instance.EnemyDict[enemyTypeIndex].AllocateGameObject<EnemyActor>(ActorContainerRoot);
                GridPos3D.ApplyGridPosToLocalTrans(bpd.GridPos3D, enemy.transform, 1);
                Enemies.Add(enemy);
            }
        }
    }

    public void ResetBattle()
    {
        for (int i = 0; i < MainPlayers.Length; i++)
        {
            MainPlayers[i]?.ActorBattleHelper.ResetState();
        }
    }

    private void AddActor(Actor actor)
    {
    }

    private void RemoveActor(Actor actor)
    {
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
}
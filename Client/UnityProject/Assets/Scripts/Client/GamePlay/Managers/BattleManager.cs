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
    internal SortedDictionary<uint, Actor> ActorDict = new SortedDictionary<uint, Actor>();

    public Transform ActorContainerRoot;

    public void Clear()
    {
        foreach (KeyValuePair<uint, Actor> kv in ActorDict)
        {
            kv.Value.PoolRecycle();
        }

        ActorDict.Clear();

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

    public Actor FindActor(uint guid)
    {
        ActorDict.TryGetValue(guid, out Actor actor);
        return actor;
    }

    public void SetAllActorShown(bool shown)
    {
        foreach (KeyValuePair<uint, Actor> kv in ActorDict)
        {
            kv.Value.SetShown(shown);
        }
    }

    public void SetAllEnemyShown(bool shown)
    {
        foreach (KeyValuePair<uint, Actor> kv in ActorDict)
        {
            if (!kv.Value.IsPlayer)
            {
                kv.Value.SetShown(shown);
            }
        }
    }
}
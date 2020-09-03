using System.Collections.Generic;
using BiangStudio.GameDataFormat.Grid;
using BiangStudio.GamePlay.UI;
using BiangStudio.Messenger;
using BiangStudio.Singleton;
using UnityEngine;

public class BattleManager : TSingletonBaseManager<BattleManager>
{
    public Messenger BattleMessenger = new Messenger();

    internal PlayerActor MainPlayer1;
    internal PlayerActor MainPlayer2;
    internal SortedDictionary<uint, Actor> ActorDict = new SortedDictionary<uint, Actor>();

    public Transform ActorContainerRoot;

    public void Clear()
    {
        foreach (KeyValuePair<uint, Actor> kv in ActorDict)
        {
            kv.Value.PoolRecycle();
        }

        ActorDict.Clear();
        MainPlayer1?.PoolRecycle();
        MainPlayer1 = null;
        MainPlayer2?.PoolRecycle();
        MainPlayer2 = null;
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
        MainPlayer1 = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.Player].AllocateGameObject<PlayerActor>(ActorContainerRoot);
        MainPlayer1.Initialize(PlayerNumber.Player1);
        GridPos3D.ApplyGridPosToLocalTrans(WorldManager.Instance.CurrentWorld.WorldData.WorldActorData.Player1BornPoint, MainPlayer1.transform, 1);
        BattleMessenger.Broadcast((uint) Enum_Events.OnPlayerLoaded, (Actor) MainPlayer1);

        MainPlayer2 = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.Player].AllocateGameObject<PlayerActor>(ActorContainerRoot);
        MainPlayer2.Initialize(PlayerNumber.Player2);
        GridPos3D.ApplyGridPosToLocalTrans(WorldManager.Instance.CurrentWorld.WorldData.WorldActorData.Player2BornPoint, MainPlayer2.transform, 1);
        BattleMessenger.Broadcast((uint) Enum_Events.OnPlayerLoaded, (Actor) MainPlayer2);

        UIManager.Instance.ShowUIForms<PlayerHUDPanel>().Initialize();

        GameStateManager.Instance.SetState(GameState.Fighting);
    }

    public void ResetBattle()
    {
        MainPlayer1.ActorBattleHelper.ResetState();
        MainPlayer2.ActorBattleHelper.ResetState();
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
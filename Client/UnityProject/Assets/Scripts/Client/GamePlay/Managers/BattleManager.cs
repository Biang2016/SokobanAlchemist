using System.Collections.Generic;
using BiangStudio.GameDataFormat.Grid;
using BiangStudio.Messenger;
using BiangStudio.Singleton;
using UnityEngine;

public class BattleManager : TSingletonBaseManager<BattleManager>
{
    public Messenger BattleMessenger = new Messenger();

    internal Actor MainPlayer;
    internal SortedDictionary<uint, Actor> ActorDict = new SortedDictionary<uint, Actor>();

    public Transform ActorContainerRoot;

    public void Clear()
    {
        foreach (KeyValuePair<uint, Actor> kv in ActorDict)
        {
            kv.Value.PoolRecycle();
        }

        ActorDict.Clear();
        MainPlayer?.PoolRecycle();
        MainPlayer = null;
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
        MainPlayer = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.Player].AllocateGameObject<PlayerActor>(ActorContainerRoot);
        GridPos3D.ApplyGridPosToLocalTrans(WorldManager.Instance.CurrentWorld.WorldData.WorldActorData.PlayerBornPoint, MainPlayer.transform, 1);
        BattleMessenger.Broadcast((uint) Enum_Events.OnPlayerLoaded, MainPlayer);
        GameStateManager.Instance.SetState(GameState.Fighting);
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
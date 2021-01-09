using System.Collections.Generic;
using BiangLibrary.Singleton;
using UnityEngine;

public class WorldManager : TSingletonBaseManager<WorldManager>
{
    internal Transform WorldRoot;
    internal Transform BattleIndicatorRoot;
    public World CurrentWorld;

    /// <summary>
    /// 不属于世界管辖范围内的Box
    /// </summary>
    public SortedDictionary<uint, Box> OtherBoxDict = new SortedDictionary<uint, Box>();

    public void Clear()
    {
        foreach (KeyValuePair<uint, Box> kv in OtherBoxDict)
        {
            kv.Value.PoolRecycle();
        }

        OtherBoxDict.Clear();
        CurrentWorld?.Clear();
        CurrentWorld = null;
    }

    public override void Awake()
    {
        WorldRoot = new GameObject("WorldRoot").transform;
        BattleIndicatorRoot = new GameObject("BattleIndicatorRoot").transform;
    }

    public override void Start()
    {
        base.Start();
        if (string.IsNullOrEmpty(ClientGameManager.DebugChangeWorldName))
        {
            Initialize(ConfigManager.GetWorldDataConfig(ConfigManager.GetWorldTypeIndex(ClientGameManager.Instance.StartWorldName)));
        }
        else
        {
            Initialize(ConfigManager.GetWorldDataConfig(ConfigManager.GetWorldTypeIndex(ClientGameManager.DebugChangeWorldName)));
        }
    }

    public void Initialize(WorldData worldData)
    {
        CurrentWorld = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.World].AllocateGameObject<World>(WorldRoot);
        CurrentWorld.name = worldData.WorldTypeName;
        CurrentWorld.Initialize(worldData);
    }

    public override void Update(float deltaTime)
    {
    }

    public override void ShutDown()
    {
        base.ShutDown();
        Clear();
    }
}
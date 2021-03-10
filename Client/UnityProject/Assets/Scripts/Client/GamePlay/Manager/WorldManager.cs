using System.Collections;
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
    }

    public IEnumerator StartGame()
    {
        if (string.IsNullOrEmpty(ClientGameManager.DebugChangeWorldName))
        {
            yield return Initialize(ConfigManager.GetWorldDataConfig(ConfigManager.GetWorldTypeIndex(ClientGameManager.Instance.StartWorldName)));
        }
        else
        {
            yield return Initialize(ConfigManager.GetWorldDataConfig(ConfigManager.GetWorldTypeIndex(ClientGameManager.DebugChangeWorldName)));
        }
    }

    public IEnumerator Initialize(WorldData worldData)
    {
        if (worldData.WorldFeature.HasFlag(WorldFeature.OpenWorld))
        {
            CurrentWorld = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.OpenWorld].AllocateGameObject<OpenWorld>(WorldRoot);
        }
        else
        {
            CurrentWorld = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.World].AllocateGameObject<World>(WorldRoot);
        }

        CurrentWorld.name = worldData.WorldTypeName;
        yield return CurrentWorld.Initialize(worldData);
    }

    public IEnumerator OnAfterStartGame()
    {
        if (CurrentWorld is OpenWorld openWorld)
        {
            yield return openWorld.OnAfterInitialize();
        }
    }

    public override void Update(float deltaTime)
    {
    }

    public override void ShutDown()
    {
        CurrentWorld.ShutDown();
        base.ShutDown();
        Clear();
    }
}
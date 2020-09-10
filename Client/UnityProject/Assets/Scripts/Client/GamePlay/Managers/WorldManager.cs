using System.Collections.Generic;
using BiangStudio.Singleton;
using UnityEngine;

public class WorldManager : TSingletonBaseManager<WorldManager>
{
    public static byte DeadZoneIndex;

    public Transform WorldRoot;
    public World CurrentWorld;

    public void Clear()
    {
    }

    public override void Awake()
    {
        DeadZoneIndex = ConfigManager.GetWorldModuleTypeIndex("WorldModule_DeadZone");
        WorldRoot = new GameObject("WorldRoot").transform;
    }

    public override void Start()
    {
        base.Start();
        if (string.IsNullOrEmpty(ClientGameManager.DebugChangeWorldName))
        {
            Initialize(ConfigManager.GetWorldDataConfig(ClientGameManager.Instance.StartWorldName));
        }
        else
        {
            Initialize(ConfigManager.GetWorldDataConfig(ClientGameManager.DebugChangeWorldName));
        }
    }

    public void Initialize(WorldData worldData)
    {
        CurrentWorld = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.World].AllocateGameObject<World>(WorldRoot);
        CurrentWorld.name = worldData.WorldName;
        CurrentWorld.Initialize(worldData);
    }

    public override void Update(float deltaTime)
    {
    }
}
using System.Collections.Generic;
using BiangStudio.Singleton;
using UnityEngine;

public class WorldManager : TSingletonBaseManager<WorldManager>
{
    public Transform WorldRoot;
    public World CurrentWorld;

    public void Clear()
    {
    }

    public override void Awake()
    {
        WorldRoot = new GameObject("WorldRoot").transform;
    }

    public override void Start()
    {
        base.Start();
        Initialize(ConfigManager.GetWorldDataConfig(ClientGameManager.Instance.StartWorldName));
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
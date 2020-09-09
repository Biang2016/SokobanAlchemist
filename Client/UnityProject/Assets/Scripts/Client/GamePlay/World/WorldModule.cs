using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ObjectPool;
using UnityEngine;

public class WorldModule : PoolObject
{
    public const int MODULE_SIZE = 16;
    public World World;

    /// <summary>
    /// 按16格为一单位的坐标
    /// </summary>
    public GridPos3D ModuleGP;

    public WorldModuleData WorldModuleData;

    [HideInInspector]
    public Box[,,] BoxMatrix = new Box[MODULE_SIZE, MODULE_SIZE, MODULE_SIZE];

    public void Initialize(WorldModuleData worldModuleData, GridPos3D moduleGP, World world)
    {
        ModuleGP = moduleGP;
        World = world;
        WorldModuleData = worldModuleData;
        if (WorldModuleData.WorldModuleFeature.HasFlag(WorldModuleFeature.DeadZone))
        {
            WorldDeadZoneTrigger deadZone = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.WorldDeadZoneTrigger].AllocateGameObject<WorldDeadZoneTrigger>(transform);
            deadZone.name = $"{nameof(WorldDeadZoneTrigger)}_{ModuleGP}";
            deadZone.Initialize(moduleGP);
        }

        if (WorldModuleData.WorldModuleFeature.HasFlag(WorldModuleFeature.Wall))
        {
            WorldWallCollider wallCollider = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.WorldWallCollider].AllocateGameObject<WorldWallCollider>(transform);
            wallCollider.name = $"{nameof(WorldWallCollider)}_{ModuleGP}";
            wallCollider.Initialize(moduleGP);
        }

        if (WorldModuleData.WorldModuleFeature.HasFlag(WorldModuleFeature.Ground))
        {
            WorldGroundCollider groundCollider = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.WorldGroundCollider].AllocateGameObject<WorldGroundCollider>(transform);
            groundCollider.name = $"{nameof(WorldGroundCollider)}_{ModuleGP}";
            groundCollider.Initialize(moduleGP);
        }

        for (int x = 0; x < worldModuleData.BoxMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < worldModuleData.BoxMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < worldModuleData.BoxMatrix.GetLength(2); z++)
                {
                    byte boxTypeIndex = worldModuleData.BoxMatrix[x, y, z];
                    if (boxTypeIndex != 0)
                    {
                        Box box = GameObjectPoolManager.Instance.BoxDict[boxTypeIndex].AllocateGameObject<Box>(transform);
                        string boxName = ConfigManager.GetBoxTypeName(boxTypeIndex);
                        box.BoxTypeIndex = boxTypeIndex;
                        GridPos3D gp = new GridPos3D(x, y, z);
                        box.Initialize(gp, this, 0, !IsAccessible);
                        box.name = $"{boxName}_{gp}";
                        BoxMatrix[x, y, z] = box;
                    }
                }
            }
        }
    }

    public bool IsAccessible => !WorldModuleData.WorldModuleFeature.HasFlag(WorldModuleFeature.DeadZone)
                                && !WorldModuleData.WorldModuleFeature.HasFlag(WorldModuleFeature.Wall)
                                && !WorldModuleData.WorldModuleFeature.HasFlag(WorldModuleFeature.Ground);

    void Start()
    {
    }

    void Update()
    {
    }

    public void ExportModuleData()
    {
    }
}
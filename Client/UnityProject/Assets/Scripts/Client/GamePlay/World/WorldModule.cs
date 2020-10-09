using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ObjectPool;
using UnityEditor;
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

    public WorldDeadZoneTrigger WorldDeadZoneTrigger;
    public WorldWallCollider WorldWallCollider;
    public WorldGroundCollider WorldGroundCollider;

    [HideInInspector]
    public Box[,,] BoxMatrix = new Box[MODULE_SIZE, MODULE_SIZE, MODULE_SIZE];

    public void Clear()
    {
        for (int x = 0; x < BoxMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < BoxMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < BoxMatrix.GetLength(2); z++)
                {
                    BoxMatrix[x, y, z]?.PoolRecycle();
                    BoxMatrix[x, y, z] = null;
                }
            }
        }

        World = null;
        WorldModuleData = null;
        WorldDeadZoneTrigger?.PoolRecycle();
        WorldDeadZoneTrigger = null;
        WorldWallCollider?.PoolRecycle();
        WorldWallCollider = null;
        WorldGroundCollider?.PoolRecycle();
        WorldGroundCollider = null;
    }

    public void Initialize(WorldModuleData worldModuleData, GridPos3D moduleGP, World world)
    {
        ModuleGP = moduleGP;
        World = world;
        WorldModuleData = worldModuleData;
        if (WorldModuleData.WorldModuleFeature.HasFlag(WorldModuleFeature.DeadZone))
        {
            WorldDeadZoneTrigger = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.WorldDeadZoneTrigger].AllocateGameObject<WorldDeadZoneTrigger>(transform);
            WorldDeadZoneTrigger.name = $"{nameof(WorldDeadZoneTrigger)}_{ModuleGP}";
            WorldDeadZoneTrigger.Initialize(moduleGP);
        }

        if (WorldModuleData.WorldModuleFeature.HasFlag(WorldModuleFeature.Wall))
        {
            WorldWallCollider = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.WorldWallCollider].AllocateGameObject<WorldWallCollider>(transform);
            WorldWallCollider.name = $"{nameof(WorldWallCollider)}_{ModuleGP}";
            WorldWallCollider.Initialize(moduleGP);
        }

        if (WorldModuleData.WorldModuleFeature.HasFlag(WorldModuleFeature.Ground))
        {
            WorldGroundCollider = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.WorldGroundCollider].AllocateGameObject<WorldGroundCollider>(transform);
            WorldGroundCollider.name = $"{nameof(WorldGroundCollider)}_{ModuleGP}";
            WorldGroundCollider.Initialize(moduleGP);
        }

        for (int x = 0; x < worldModuleData.BoxMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < worldModuleData.BoxMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < worldModuleData.BoxMatrix.GetLength(2); z++)
                {
                    ushort boxTypeIndex = worldModuleData.BoxMatrix[x, y, z];
                    if (boxTypeIndex != 0)
                    {
                        GenerateBox(boxTypeIndex, x, y, z);
                    }
                }
            }
        }
    }

    public void GenerateBox(ushort boxTypeIndex, GridPos3D localGP)
    {
        GenerateBox(boxTypeIndex, localGP.x, localGP.y, localGP.z);
    }

    public void GenerateBox(ushort boxTypeIndex, int x, int y, int z)
    {
        Box box = GameObjectPoolManager.Instance.BoxDict[boxTypeIndex].AllocateGameObject<Box>(transform);
        string boxName = ConfigManager.GetBoxTypeName(boxTypeIndex);
        GridPos3D gp = new GridPos3D(x, y, z);
        box.Setup(boxTypeIndex);
        box.Initialize(gp, this, 0, !IsAccessible, Box.LerpType.Create);
        box.name = $"{boxName}_{gp}";
        BoxMatrix[x, y, z] = box;
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

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (Selection.Contains(gameObject))
        {
            if (WorldModuleData != null && WorldModuleData.WorldModuleTypeIndex == WorldManager.DeadZoneIndex)
            {
                Gizmos.color = new Color(1f, 0, 0, 0.7f);
                Gizmos.DrawSphere(transform.position + Vector3.one * (MODULE_SIZE - 1) * 0.5f, 3f);
            }
        }
    }
#endif
}
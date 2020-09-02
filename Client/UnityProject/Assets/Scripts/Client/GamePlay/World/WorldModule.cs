using System.Collections.Generic;
using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ObjectPool;
using Sirenix.OdinInspector;
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
    public Box[,,] BoxMatrix = new Box[MODULE_SIZE, MODULE_SIZE, MODULE_SIZE];

    public void Initialize(WorldModuleData worldModuleData, GridPos3D moduleGP, World world)
    {
        ModuleGP = moduleGP;
        World = world;
        WorldModuleData = worldModuleData;
        for (int x = 0; x < worldModuleData.BoxMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < worldModuleData.BoxMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < worldModuleData.BoxMatrix.GetLength(2); z++)
                {
                    BoxType boxType = (BoxType) worldModuleData.BoxMatrix[x, y, z];
                    if (boxType != BoxType.None)
                    {
                        Box box = GameObjectPoolManager.Instance.BoxDict[boxType].AllocateGameObject<Box>(transform);
                        GridPos3D gp = new GridPos3D(x, y, z);
                        box.Initialize(gp, this, 0);
                        box.name = $"{boxType}({x}, {y}, {z})";
                        BoxMatrix[x, y, z] = box;
                    }
                }
            }
        }
    }

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

public enum WorldModuleType
{
    None,
    SampleWorldModule = 1,
    GroundWorldModule = 2,

    BorderWorldModule_Up = 101,
    BorderWorldModule_Down = 102,
    BorderWorldModule_Left = 103,
    BorderWorldModule_Right = 104,

    MAX = 255,
}
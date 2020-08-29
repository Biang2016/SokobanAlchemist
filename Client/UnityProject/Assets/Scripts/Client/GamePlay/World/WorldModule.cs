using System.Collections.Generic;
using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ObjectPool;
using Sirenix.OdinInspector;
using UnityEngine;

public class WorldModule : PoolObject
{
    public const int MODULE_SIZE = 16;

    public WorldModuleData WorldModuleData;

    public List<BoxBase> Boxes = new List<BoxBase>();

    public void Initialize(WorldModuleData worldModuleData)
    {
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
                        BoxBase box = GameObjectPoolManager.Instance.BoxDict[boxType].AllocateGameObject<BoxBase>(transform);
                        GridPos3D.ApplyGridPosToLocalTrans(new GridPos3D(x, y, z), box.transform, 1);
                        box.name = $"{boxType}({x}, {y}, {z})";
                        Boxes.Add(box);
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

    MAX = 255,
}
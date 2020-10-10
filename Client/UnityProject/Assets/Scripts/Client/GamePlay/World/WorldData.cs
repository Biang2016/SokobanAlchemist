using System;
using System.Collections.Generic;
using BiangStudio.CloneVariant;
using Sirenix.OdinInspector;

public class WorldData : IClone<WorldData>
{
    public string WorldName;

    public WorldFeature WorldFeature;

    /// <summary>
    /// 世界制作规范，世界最大范围为16x16x8个模组
    /// </summary>
    public ushort[,,] ModuleMatrix = new ushort[World.WORLD_SIZE, World.WORLD_HEIGHT, World.WORLD_SIZE];

    public WorldActorData WorldActorData = new WorldActorData();
    public WorldCameraPOIData WorldCameraPOIData = new WorldCameraPOIData();
    public WorldLevelTriggerData WorldLevelTriggerData = new WorldLevelTriggerData();
    public List<Box.WorldSpecialBoxData> WorldSpecialBoxDataList = new List<Box.WorldSpecialBoxData>();
    public List<Box.BoxExtraSerializeData>[,,] ModuleBoxExtraSerializeDataMatrix = new List<Box.BoxExtraSerializeData>[World.WORLD_SIZE, World.WORLD_HEIGHT, World.WORLD_SIZE];

    public WorldData()
    {
        for (int x = 0; x < ModuleMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < ModuleMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < ModuleMatrix.GetLength(2); z++)
                {
                    ModuleBoxExtraSerializeDataMatrix[x, y, z] = new List<Box.BoxExtraSerializeData>();
                }
            }
        }
    }

    public WorldData Clone()
    {
        WorldData data = new WorldData();
        data.WorldName = WorldName;
        data.WorldFeature = WorldFeature;
        for (int x = 0; x < ModuleMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < ModuleMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < ModuleMatrix.GetLength(2); z++)
                {
                    data.ModuleMatrix[x, y, z] = ModuleMatrix[x, y, z];
                    data.ModuleBoxExtraSerializeDataMatrix[x, y, z] = ModuleBoxExtraSerializeDataMatrix[x, y, z].Clone();
                }
            }
        }

        data.WorldActorData = WorldActorData.Clone();
        data.WorldCameraPOIData = WorldCameraPOIData.Clone();
        data.WorldLevelTriggerData = WorldLevelTriggerData.Clone();
        data.WorldSpecialBoxDataList = WorldSpecialBoxDataList.Clone();
        return data;
    }
}

[Flags]
public enum WorldFeature
{
    None = 0,

    [LabelText("玩家无敌")]
    PlayerImmune = 1 << 0,

    [LabelText("PVP")]
    PVP = 1 << 1,
}
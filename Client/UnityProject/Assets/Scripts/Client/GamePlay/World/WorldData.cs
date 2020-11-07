using System;
using System.Collections.Generic;
using BiangStudio.CloneVariant;
using BiangStudio.GameDataFormat.Grid;
using Sirenix.OdinInspector;

public class WorldData : IClone<WorldData>
{
    public ushort WorldTypeIndex;
    public string WorldTypeName;

    public WorldFeature WorldFeature;

    public string DefaultWorldActorBornPointAlias;

    /// <summary>
    /// 世界制作规范，世界最大范围为16x16x8个模组
    /// </summary>
    public ushort[,,] ModuleMatrix = new ushort[World.WORLD_SIZE, World.WORLD_HEIGHT, World.WORLD_SIZE];

    public WorldBornPointGroupData WorldBornPointGroupData = new WorldBornPointGroupData();
    public WorldCameraPOIData WorldCameraPOIData = new WorldCameraPOIData();
    public LevelTriggerGroupData WorldLevelTriggerGroupData = new LevelTriggerGroupData();
    public List<Box.WorldSpecialBoxData> WorldSpecialBoxDataList = new List<Box.WorldSpecialBoxData>(); // LevelEventTriggerAppear的箱子不记录到此列表中
    public List<Box.BoxExtraSerializeData>[,,] ModuleBoxExtraSerializeDataMatrix = new List<Box.BoxExtraSerializeData>[World.WORLD_SIZE, World.WORLD_HEIGHT, World.WORLD_SIZE];
    public List<BoxFunction_LevelEventTriggerAppear.Data> EventTriggerAppearBoxDataList = new List<BoxFunction_LevelEventTriggerAppear.Data>(); // 覆盖模组特例的世界特例不序列化在此，序列化在ModuleBoxExtraSerializeDataMatrix中

    public List<GridPos3D> WorldModuleGPOrder = new List<GridPos3D>();

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
        data.WorldTypeIndex = WorldTypeIndex;
        data.WorldTypeName = WorldTypeName;
        data.WorldFeature = WorldFeature;
        data.DefaultWorldActorBornPointAlias = DefaultWorldActorBornPointAlias;
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

        data.WorldBornPointGroupData = WorldBornPointGroupData.Clone();
        data.WorldCameraPOIData = WorldCameraPOIData.Clone();
        data.WorldLevelTriggerGroupData = WorldLevelTriggerGroupData.Clone();
        data.WorldSpecialBoxDataList = WorldSpecialBoxDataList.Clone();
        data.EventTriggerAppearBoxDataList = EventTriggerAppearBoxDataList.Clone();
        data.WorldModuleGPOrder = WorldModuleGPOrder.Clone();
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
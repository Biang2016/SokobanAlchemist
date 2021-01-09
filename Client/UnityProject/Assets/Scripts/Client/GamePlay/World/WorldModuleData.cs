using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;

public class WorldModuleData : IClone<WorldModuleData>
{
    #region ConfigData

    public ushort WorldModuleTypeIndex;
    public string WorldModuleTypeName;
    public string WorldModuleFlowAssetPath;

    public WorldModuleFeature WorldModuleFeature;

    /// <summary>
    /// 世界模组制作规范，一个模组容量为16x16x16
    /// 模组上下层叠，底部模组Y为0，顶部Y为15
    /// </summary>
    public ushort[,,] BoxMatrix = new ushort[WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE];

    public BornPointGroupData WorldModuleBornPointGroupData = new BornPointGroupData();
    public LevelTriggerGroupData WorldModuleLevelTriggerGroupData = new LevelTriggerGroupData();
    public Box.BoxExtraSerializeData[,,] BoxExtraSerializeDataMatrix = new Box.BoxExtraSerializeData[WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE]; // 不含LevelEventTriggerBoxPassiveSkill，但含该Box的其他BF信息
    public List<BoxPassiveSkill_LevelEventTriggerAppear.Data> EventTriggerAppearBoxDataList = new List<BoxPassiveSkill_LevelEventTriggerAppear.Data>();

    public WorldModuleData Clone()
    {
        WorldModuleData data = new WorldModuleData();
        data.WorldModuleTypeIndex = WorldModuleTypeIndex;
        data.WorldModuleTypeName = WorldModuleTypeName;
        data.WorldModuleFlowAssetPath = WorldModuleFlowAssetPath;
        data.WorldModuleFeature = WorldModuleFeature;
        data.WorldModuleBornPointGroupData = WorldModuleBornPointGroupData.Clone();
        data.WorldModuleLevelTriggerGroupData = WorldModuleLevelTriggerGroupData.Clone();
        for (int x = 0; x < BoxMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < BoxMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < BoxMatrix.GetLength(2); z++)
                {
                    data.BoxMatrix[x, y, z] = BoxMatrix[x, y, z];
                    if (BoxExtraSerializeDataMatrix[x, y, z] != null)
                    {
                        data.BoxExtraSerializeDataMatrix[x, y, z] = BoxExtraSerializeDataMatrix[x, y, z].Clone();
                    }
                }
            }
        }

        data.EventTriggerAppearBoxDataList = EventTriggerAppearBoxDataList.Clone();
        return data;
    }

    #endregion
}

[Flags]
public enum WorldModuleFeature
{
    None = 0,

    [LabelText("墙壁")]
    Wall = 1 << 0,

    [LabelText("死亡面")]
    DeadZone = 1 << 1,

    [LabelText("地面")]
    Ground = 1 << 2,
}
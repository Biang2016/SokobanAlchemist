using System;
using System.Collections.Generic;
using BiangStudio.CloneVariant;
using Sirenix.OdinInspector;

public class WorldModuleData : IClone<WorldModuleData>
{
    #region ConfigData

    public ushort WorldModuleTypeIndex;
    public string WorldModuleTypeName;

    public WorldModuleFeature WorldModuleFeature;

    /// <summary>
    /// 世界模组制作规范，一个模组容量为16x16x16
    /// 模组上下层叠，底部模组Y为0，顶部Y为15
    /// </summary>
    public ushort[,,] BoxMatrix = new ushort[WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE];
    public WorldModuleLevelTriggerData WorldModuleLevelTriggerData = new WorldModuleLevelTriggerData();
    public Box.BoxExtraSerializeData[,,] BoxExtraSerializeDataMatrix = new Box.BoxExtraSerializeData[WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE];

    public WorldModuleData Clone()
    {
        WorldModuleData data = new WorldModuleData();
        data.WorldModuleTypeIndex = WorldModuleTypeIndex;
        data.WorldModuleTypeName = WorldModuleTypeName;
        data.WorldModuleFeature = WorldModuleFeature;
        data.WorldModuleLevelTriggerData = WorldModuleLevelTriggerData.Clone();
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
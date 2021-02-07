using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.ObjectPool;
using Sirenix.OdinInspector;

public class WorldModuleData : IClone<WorldModuleData>, IClassPoolObject<WorldModuleData>
{
    public static ClassObjectPool<WorldModuleData> WorldModuleDataFactory = new ClassObjectPool<WorldModuleData>(16);

    #region ConfigData

    public ushort WorldModuleTypeIndex;
    public string WorldModuleTypeName;
    public string WorldModuleFlowAssetPath;

    public WorldModuleFeature WorldModuleFeature;

    /// <summary>
    /// 世界模组制作规范，一个模组容量为16x16x16
    /// 模组上下层叠，底部模组Y为0，顶部Y为15
    /// </summary>
    public ushort[,,] BoxMatrix = new ushort[WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE]; // 仅作为箱子核心格的位置记录，不代表每一格实际是否有占用。（因为有Mega箱子尺寸不止一格）

    public GridPosR.Orientation[,,] BoxOrientationMatrix; // 箱子朝向矩阵记录，仅引用于对应核心格的箱子
    public ushort[,,] BoxMatrix_Temp_CheckOverlap; // 导出时临时使用，为了检查箱子重叠

    public BornPointGroupData WorldModuleBornPointGroupData = new BornPointGroupData();
    public LevelTriggerGroupData WorldModuleLevelTriggerGroupData = new LevelTriggerGroupData();
    public Box_LevelEditor.BoxExtraSerializeData[,,] BoxExtraSerializeDataMatrix; // 不含LevelEventTriggerBoxPassiveSkill，但含该Box的其他BF信息
    public List<BoxPassiveSkill_LevelEventTriggerAppear.Data> EventTriggerAppearBoxDataList;

    /// <summary>
    /// 初始化序列化出的模组数据（开放世界模组考虑性能，不对这些数据初始化）
    /// </summary>
    public void InitNormalModuleData()
    {
        BoxOrientationMatrix = new GridPosR.Orientation[WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE];
        BoxMatrix_Temp_CheckOverlap = new ushort[WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE];
        BoxExtraSerializeDataMatrix = new Box_LevelEditor.BoxExtraSerializeData[WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE];
        EventTriggerAppearBoxDataList = new List<BoxPassiveSkill_LevelEventTriggerAppear.Data>();
    }

    public WorldModuleData Clone()
    {
        WorldModuleData data = WorldModuleDataFactory.Alloc();
        data.InitNormalModuleData();
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
                    if (BoxOrientationMatrix != null) data.BoxOrientationMatrix[x, y, z] = BoxOrientationMatrix[x, y, z];
                    if (BoxExtraSerializeDataMatrix?[x, y, z] != null)
                    {
                        data.BoxExtraSerializeDataMatrix[x, y, z] = BoxExtraSerializeDataMatrix[x, y, z].Clone();
                    }
                }
            }
        }

        if (EventTriggerAppearBoxDataList != null) data.EventTriggerAppearBoxDataList = EventTriggerAppearBoxDataList.Clone();
        return data;
    }

    #endregion

    #region IClassPoolObject

    public WorldModuleData Create()
    {
        return WorldModuleDataFactory.Alloc();
    }

    public void OnUsed()
    {
        for (int x = 0; x < BoxMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < BoxMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < BoxMatrix.GetLength(2); z++)
                {
                    BoxMatrix[x, y, z] = 0;
                    if (BoxOrientationMatrix != null) BoxOrientationMatrix[x, y, z] = GridPosR.Orientation.Up;
                }
            }
        }

        WorldModuleBornPointGroupData = new BornPointGroupData();
        WorldModuleLevelTriggerGroupData = new LevelTriggerGroupData();
    }

    public void OnRelease()
    {
        WorldModuleBornPointGroupData = null;
        WorldModuleLevelTriggerGroupData = null;
    }

    public void Release()
    {
        WorldModuleDataFactory.Release(this);
    }

    private int PoolIndex;

    public void SetPoolIndex(int index)
    {
        PoolIndex = index;
    }

    public int GetPoolIndex()
    {
        return PoolIndex;
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
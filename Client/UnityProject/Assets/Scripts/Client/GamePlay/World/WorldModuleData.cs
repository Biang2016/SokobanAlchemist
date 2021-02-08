using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.ObjectPool;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public class WorldModuleData : IClone<WorldModuleData>, IClassPoolObject<WorldModuleData>
{
    public static ClassObjectPool<WorldModuleData> WorldModuleDataFactory = new ClassObjectPool<WorldModuleData>(16);

    #region ConfigData

    public ushort WorldModuleTypeIndex;
    public string WorldModuleTypeName;
    public string WorldModuleFlowAssetPath;

    public WorldModuleFeature WorldModuleFeature;

    public ushort[,,] RawBoxMatrix = new ushort[WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE];

    /// <summary>
    /// 世界模组制作规范，一个模组容量为16x16x16
    /// 模组上下层叠，底部模组Y为0，顶部Y为15
    /// </summary>
    public ushort[,,] BoxMatrix = new ushort[WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE]; // 仅作为箱子核心格的位置记录，不代表每一格实际是否有占用。（因为有Mega箱子尺寸不止一格）

    public GridPosR.Orientation[,,] RawBoxOrientationMatrix = new GridPosR.Orientation[WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE]; 
    public GridPosR.Orientation[,,] BoxOrientationMatrix = new GridPosR.Orientation[WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE]; // 箱子朝向矩阵记录，仅引用于对应核心格的箱子
    public ushort[,,] BoxMatrix_Temp_CheckOverlap; // 导出时临时使用，为了检查箱子重叠

    public BornPointGroupData WorldModuleBornPointGroupData = new BornPointGroupData();
    public LevelTriggerGroupData WorldModuleLevelTriggerGroupData = new LevelTriggerGroupData();
    public Box_LevelEditor.BoxExtraSerializeData[,,] BoxExtraSerializeDataMatrix; // 不含LevelEventTriggerBoxPassiveSkill，但含该Box的其他BF信息
    public List<BoxPassiveSkill_LevelEventTriggerAppear.Data> EventTriggerAppearBoxDataList;

    public WorldModuleDataModification Modification;

    /// <summary>
    /// 初始化序列化出的模组数据（开放世界模组考虑性能，不对这些数据初始化）
    /// </summary>
    public void InitNormalModuleData()
    {
        BoxMatrix_Temp_CheckOverlap = new ushort[WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE];
        BoxExtraSerializeDataMatrix = new Box_LevelEditor.BoxExtraSerializeData[WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE];
        EventTriggerAppearBoxDataList = new List<BoxPassiveSkill_LevelEventTriggerAppear.Data>();
    }

    /// <summary>
    /// 开放世界模组专用，记录模组信息变化
    /// </summary>
    public void InitOpenWorldModuleData(bool needSaveModification)
    {
        if (Modification == null)
        {
            Modification = new WorldModuleDataModification();
        }

        Modification.Enable = needSaveModification;
    }

    public WorldModuleData Clone() // 理论上只有NormalModule会用到，开放世界模组不能用此Clone，否则会造成不必要的内存占用
    {
        WorldModuleData data = WorldModuleDataFactory.Alloc();
        data.InitNormalModuleData(); // 这里有内存分配
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
                    data.RawBoxMatrix[x, y, z] = RawBoxMatrix[x, y, z];
                    data.BoxMatrix[x, y, z] = RawBoxMatrix[x, y, z];
                    data.RawBoxOrientationMatrix[x, y, z] = RawBoxOrientationMatrix[x, y, z];
                    data.BoxOrientationMatrix[x, y, z] = RawBoxOrientationMatrix[x, y, z];
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

    public void OnUsed()
    {
        for (int x = 0; x < WorldModule.MODULE_SIZE; x++)
        {
            for (int y = 0; y < WorldModule.MODULE_SIZE; y++)
            {
                for (int z = 0; z < WorldModule.MODULE_SIZE; z++)
                {
                    RawBoxMatrix[x, y, z] = 0;
                    BoxMatrix[x, y, z] = 0;
                    RawBoxOrientationMatrix[x, y, z] = GridPosR.Orientation.Up;
                    BoxOrientationMatrix[x, y, z] = GridPosR.Orientation.Up;
                }
            }
        }

        WorldModuleFeature = WorldModuleFeature.None;
        WorldModuleBornPointGroupData = new BornPointGroupData();
        WorldModuleLevelTriggerGroupData = new LevelTriggerGroupData();
    }

    public void OnRelease()
    {
        Modification?.Clear();
        WorldModuleBornPointGroupData = null;
        WorldModuleLevelTriggerGroupData = null;
        WorldModuleFeature = WorldModuleFeature.None;
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

public class WorldModuleDataModification
{
    public bool Enable = true; // 对不需要记录更改的模组不生效

    public struct BoxModification
    {
        public ushort BoxTypeIndex;
        public GridPosR.Orientation BoxOrientation;

        public BoxModification(ushort boxTypeIndex, GridPosR.Orientation boxOrientation)
        {
            BoxTypeIndex = boxTypeIndex;
            BoxOrientation = boxOrientation;
        }
    }

    public Dictionary<GridPos3D, BoxModification> ModificationDict = new Dictionary<GridPos3D, BoxModification>(16);

    public void SaveData(GridPos3D moduleGP)
    {
        if (ModificationDict.Count > 0)
        {
            GameSaveManager.Instance.SaveData(WorldManager.Instance.CurrentWorld.WorldGUID, moduleGP.ToString(), GameSaveManager.SaveDataType.GameProgress, this, DataFormat.JSON);
        }
    }

    public static WorldModuleDataModification LoadData(GridPos3D moduleGP)
    {
        WorldModuleDataModification mod = GameSaveManager.Instance.LoadData<WorldModuleDataModification>(WorldManager.Instance.CurrentWorld.WorldGUID, moduleGP.ToString(), GameSaveManager.SaveDataType.GameProgress, DataFormat.JSON);
        return mod;
    }

    public void Clear()
    {
        ModificationDict.Clear();
    }
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
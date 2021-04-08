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

    public Dictionary<TypeDefineType, EntityData[,,]>.KeyCollection EntityDataMatrixKeys => EntityDataMatrix.Keys;

    /// <summary>
    /// 世界模组制作规范，一个模组容量为16x16x16
    /// 模组上下层叠，底部模组Y为0，顶部Y为15
    /// 仅作为Entity核心格的位置记录，不代表每一格实际是否有占用。（因为有大型实体尺寸不止一格）
    /// </summary>
    private Dictionary<TypeDefineType, EntityData[,,]> EntityDataMatrix = new Dictionary<TypeDefineType, EntityData[,,]>
    {
        {TypeDefineType.Box, new EntityData[WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE]},
        {TypeDefineType.Actor, new EntityData[WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE]}
    };

#if UNITY_EDITOR

    public EntityData[,,] EntityDataMatrix_Temp_CheckOverlap_BetweenBoxes; // 导出时临时使用，为了检查重叠
    public EntityData[,,] EntityDataMatrix_Temp_CheckOverlap_BoxAndActor; // 导出时临时使用，为了检查重叠. Box写入时只写入非Passable的，这样就允许Actor和Passable的Box初始可以重叠
#endif

    public Grid3DBounds BoxBounds;

    public BornPointGroupData WorldModuleBornPointGroupData = new BornPointGroupData();
    public LevelTriggerGroupData WorldModuleLevelTriggerGroupData = new LevelTriggerGroupData();
    public List<EntityPassiveSkill_LevelEventTriggerAppear.Data> EventTriggerAppearEntityDataList;

    public EntityData this[TypeDefineType entityType, GridPos3D localGP]
    {
        get => EntityDataMatrix[entityType][localGP.x, localGP.y, localGP.z];
        set => EntityDataMatrix[entityType][localGP.x, localGP.y, localGP.z] = value;
    }

    /// <summary>
    /// 初始化序列化出的模组数据（开放世界模组考虑性能，不对这些数据初始化）
    /// </summary>
    public void InitNormalModuleData()
    {
#if UNITY_EDITOR
        EntityDataMatrix_Temp_CheckOverlap_BetweenBoxes = new EntityData[WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE];
        EntityDataMatrix_Temp_CheckOverlap_BoxAndActor = new EntityData[WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE];
#endif
        EventTriggerAppearEntityDataList = new List<EntityPassiveSkill_LevelEventTriggerAppear.Data>();
    }

    /// <summary>
    /// 开放世界模组专用，记录模组信息变化
    /// </summary>
    public void InitOpenWorldModuleData(bool needSaveModification)
    {
        EventTriggerAppearEntityDataList = new List<EntityPassiveSkill_LevelEventTriggerAppear.Data>();
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
        for (int x = 0; x < WorldModule.MODULE_SIZE; x++)
        {
            for (int y = 0; y < WorldModule.MODULE_SIZE; y++)
            {
                for (int z = 0; z < WorldModule.MODULE_SIZE; z++)
                {
                    data.EntityDataMatrix[TypeDefineType.Box][x, y, z] = EntityDataMatrix[TypeDefineType.Box][x, y, z].Clone();
                }
            }
        }

        for (int x = 0; x < WorldModule.MODULE_SIZE; x++)
        {
            for (int y = 0; y < WorldModule.MODULE_SIZE; y++)
            {
                for (int z = 0; z < WorldModule.MODULE_SIZE; z++)
                {
                    data.EntityDataMatrix[TypeDefineType.Actor][x, y, z] = EntityDataMatrix[TypeDefineType.Actor][x, y, z].Clone();
                }
            }
        }

        data.BoxBounds = BoxBounds;

        if (EventTriggerAppearEntityDataList != null) data.EventTriggerAppearEntityDataList = EventTriggerAppearEntityDataList.Clone();
        return data;
    }

    #endregion

    #region IClassPoolObject

    public void OnUsed()
    {
        foreach (KeyValuePair<TypeDefineType, EntityData[,,]> kv in EntityDataMatrix)
        {
            for (int x = 0; x < WorldModule.MODULE_SIZE; x++)
            {
                for (int y = 0; y < WorldModule.MODULE_SIZE; y++)
                {
                    for (int z = 0; z < WorldModule.MODULE_SIZE; z++)
                    {
                        kv.Value[x, y, z] = null;
                    }
                }
            }
        }

        WorldModuleFeature = WorldModuleFeature.None;
        WorldModuleBornPointGroupData = new BornPointGroupData();
        WorldModuleLevelTriggerGroupData = new LevelTriggerGroupData();
    }

    public void OnRelease()
    {
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
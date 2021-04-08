﻿using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class GenerateLayerData
{
    public abstract string Description { get; }

    [LabelText("生效")]
    [PropertyOrder(-10)]
    public bool Enable = true;

    [LabelText("生成算法")]
    [PropertyOrder(-10)]
    public GenerateAlgorithm m_GenerateAlgorithm = GenerateAlgorithm.CellularAutomata;

    public abstract TypeSelectHelper TypeName { get; }

    #region 白名单黑名单

    [SerializeField]
    [LabelText("允许覆盖的箱子类型")]
    [ListDrawerSettings(ListElementLabelName = "TypeName")]
    [PropertyOrder(10)]
    private List<TypeSelectHelper> AllowReplacedBoxTypeNames = new List<TypeSelectHelper>();

    [HideInInspector]
    public HashSet<string> AllowReplacedBoxTypeNameSet = new HashSet<string>();

    [LabelText("只允许覆盖上述箱子上")]
    [PropertyOrder(10)]
    public bool OnlyOverrideAnyBox = false;

    [LabelText("只允许放置某类地形上")]
    [PropertyOrder(10)]
    public bool OnlyAllowPutOnTerrain = false;

    [SerializeField]
    [ShowIf("OnlyAllowPutOnTerrain")]
    [LabelText("允许放置的地形类型")]
    [PropertyOrder(10)]
    private List<TerrainType> AllowPlaceOnTerrainTypes = new List<TerrainType>();

    [HideInInspector]
    public HashSet<TerrainType> AllowPlaceOnTerrainTypeSet = new HashSet<TerrainType>();

    [SerializeField]
    [HideIf("OnlyAllowPutOnTerrain")]
    [LabelText("禁止放置的地形类型")]
    [PropertyOrder(10)]
    private List<TerrainType> ForbidPlaceOnTerrainTypes = new List<TerrainType>();

    [HideInInspector]
    public HashSet<TerrainType> ForbidPlaceOnTerrainTypeSet = new HashSet<TerrainType>();

    #endregion

    [LabelText("是否考虑静态布局的影响")]
    [PropertyOrder(10)]
    [ShowIf("m_GenerateAlgorithm", GenerateAlgorithm.CellularAutomata)]
    public bool ConsiderStaticLayout = false;

    [LabelText("数量确定")]
    [PropertyOrder(10)]
    [ShowIf("m_GenerateAlgorithm", GenerateAlgorithm.Random)]
    public bool CertainNumber = false;

    [ShowIf("CertainNumber")]
    [ShowIf("m_GenerateAlgorithm", GenerateAlgorithm.Random)]
    [LabelText("数量")]
    [PropertyOrder(10)]
    public int Count = 1;

    [HideIf("CertainNumber")]
    [ShowIf("m_GenerateAlgorithm", GenerateAlgorithm.Random)]
    [LabelText("比率：每万格约有多少个")]
    [PropertyOrder(10)]
    public float CountPer10KGrid = 20;

    public void Init()
    {
        foreach (TypeSelectHelper allowReplacedBoxTypeName in AllowReplacedBoxTypeNames)
        {
            AllowReplacedBoxTypeNameSet.Add(allowReplacedBoxTypeName.TypeName);
        }

        foreach (TerrainType allowPlaceOnTerrainType in AllowPlaceOnTerrainTypes)
        {
            AllowPlaceOnTerrainTypeSet.Add(allowPlaceOnTerrainType);
        }

        foreach (TerrainType forbidPlaceOnTerrainType in ForbidPlaceOnTerrainTypes)
        {
            ForbidPlaceOnTerrainTypeSet.Add(forbidPlaceOnTerrainType);
        }
    }
}

[Serializable]
public class GenerateStaticLayoutLayerData : GenerateLayerData
{
    public override TypeSelectHelper TypeName => StaticLayoutTypeName;

    [LabelText("@\"静态布局类型\t\"+StaticLayoutTypeName")]
    public TypeSelectHelper StaticLayoutTypeName = new TypeSelectHelper {TypeDefineType = TypeDefineType.StaticLayout};

    [LabelText("支持放置破损布局")]
    public bool AllowFragment = false;

    [LabelText("保证布局完整不受其他布局破坏")]
    [HideIf("AllowFragment")]
    public bool LayoutIntactForOtherStaticLayout = false;

    [LabelText("保证布局内不受随机Box植入")]
    public bool LayoutIntactForOtherBoxes = false;

    [LabelText("决定玩家出生点")]
    public bool DeterminePlayerBP = false;

    public override string Description => $"{StaticLayoutTypeName}\t\t{m_GenerateAlgorithm}";
}

[Serializable]
public class GenerateBoxLayerData : GenerateLayerData
{
    public override TypeSelectHelper TypeName => BoxTypeName;

    [LabelText("@\"箱子类型\t\"+BoxTypeName")]
    public TypeSelectHelper BoxTypeName = new TypeSelectHelper {TypeDefineType = TypeDefineType.Box};

    [ShowIf("m_GenerateAlgorithm", GenerateAlgorithm.CellularAutomata)]
    [LabelText("初始填充比率")]
    public int FillPercent = 40;

    [ShowIf("m_GenerateAlgorithm", GenerateAlgorithm.CellularAutomata)]
    [LabelText("洞穴联通率")]
    public int CaveConnectPercent = 0;

    [ShowIf("m_GenerateAlgorithm", GenerateAlgorithm.CellularAutomata)]
    [LabelText("迭代次数")]
    public int SmoothTimes = 4;

    [ShowIf("m_GenerateAlgorithm", GenerateAlgorithm.CellularAutomata)]
    [LabelText("空地生墙迭代次数")]
    public int SmoothTimes_GenerateWallInOpenSpace = 3;

    [ShowIf("m_GenerateAlgorithm", GenerateAlgorithm.CellularAutomata)]
    [LabelText("是否自动融合为大Box")]
    public bool MergeBoxesIntoMegaBox = false;

    [ShowIf("m_GenerateAlgorithm", GenerateAlgorithm.CellularAutomata)]
    [ShowIf("MergeBoxesIntoMegaBox")]
    [ListDrawerSettings(ListElementLabelName = "TypeName")]
    public List<TypeSelectHelper> MergeBoxesIntoMegaBoxConfigList = new List<TypeSelectHelper>();

    public override string Description => $"{BoxTypeName}\t\t{m_GenerateAlgorithm}";
}

[Serializable]
public class GenerateActorLayerData : GenerateLayerData
{
    public override TypeSelectHelper TypeName => ActorTypeName;

    [LabelText("@\"Actor类型\t\"+ActorTypeName")]
    public TypeSelectHelper ActorTypeName = new TypeSelectHelper {TypeDefineType = TypeDefineType.Enemy};

    public override string Description => $"{ActorTypeName}\t\t{m_GenerateAlgorithm}";
}

public enum GenerateAlgorithm
{
    CellularAutomata,
    PerlinNoise,
    Random,
    Around,
}
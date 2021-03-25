using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class GenerateTerrainData
{
    [SerializeReference]
    [LabelText("处理流程")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<TerrainProcessPass> ProcessingPassList = new List<TerrainProcessPass>();
}

[Serializable]
public abstract class TerrainProcessPass
{
    public abstract string Description { get; }
}

[Serializable]
public class TerrainProcessPass_RandomFill : TerrainProcessPass
{
    public override string Description
    {
        get
        {
            string desc = OnlyOverrideSomeTerrain ? $"对于{OverrideTerrainType}地貌的格子" : "任何格子";
            string prob = $"有{FillPercent}%概率填充为{TerrainType}";
            return desc + prob;
        }
    }

    [LabelText("使用PerlinNoise来控制几率")]
    public bool ControlFillPercentWithPerlinNoise = false;

    [LabelText("填充几率")]
    public int FillPercent = 40;

    [LabelText("填充地形类型")]
    public TerrainType TerrainType = TerrainType.Earth;

    [LabelText("只覆盖某地形")]
    public bool OnlyOverrideSomeTerrain = false;

    [LabelText("覆盖哪种地形")]
    [ShowIf("OnlyOverrideSomeTerrain")]
    public TerrainType OverrideTerrainType = TerrainType.Earth;
}

[Serializable]
public class TerrainProcessPass_Smooth : TerrainProcessPass
{
    public override string Description => $"细胞自动机平滑{SmoothTimes}次";

    [LabelText("迭代次数")]
    public int SmoothTimes = 1;

    [LabelText("迭代规则")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<NeighborIteration> NeighborIterations = new List<NeighborIteration>();

    [Serializable]
    public class NeighborIteration
    {
        public string Description
        {
            get
            {
                string self = LimitSelfType ? SelfTerrainType.ToString() : "任意地形";
                string oper = "";
                switch (Operator)
                {
                    case Operator.LessEquals:
                    {
                        oper = "小等于";
                        break;
                    }
                    case Operator.Equals:
                    {
                        oper = "等于";
                        break;
                    }
                    case Operator.GreaterEquals:
                    {
                        oper = "大等于";
                        break;
                    }
                }

                return $"{self}的{NeighborTerrainType.ToString()}邻居{oper}{Threshold}个时->{ChangeTerrainTypeTo.ToString()}";
            }
        }

        [LabelText("限制本身类型")]
        public bool LimitSelfType = false;

        [LabelText("本身类型")]
        [ShowIf("LimitSelfType")]
        public TerrainType SelfTerrainType = TerrainType.Earth;

        [LabelText("邻居类型")]
        public TerrainType NeighborTerrainType = TerrainType.Earth;

        [HideLabel]
        [EnumToggleButtons]
        public Operator Operator;

        [LabelText("参考阈值")]
        public int Threshold = 4;

        [LabelText("变换地形")]
        public TerrainType ChangeTerrainTypeTo = TerrainType.Earth;
    }

    public enum Operator
    {
        LessEquals,
        Equals,
        GreaterEquals,
    }
}
using System;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class SkillRankWithProbability : Probability, IClone<SkillRankWithProbability>
{
    public string Description => $"{SkillRankType} * {Probability}";

    [LabelText("技能阶级")]
    public SkillRankType SkillRankType;

    [SerializeField]
    private int probability;

    public int Probability
    {
        get { return probability; }
        set { probability = value; }
    }

    [SerializeField]
    private bool isSingleton;

    public bool IsSingleton
    {
        get { return isSingleton; }
        set { isSingleton = value; }
    }

    public Probability ProbabilityClone()
    {
        SkillRankWithProbability newData = new SkillRankWithProbability();
        newData.SkillRankType = SkillRankType;
        newData.probability = probability;
        newData.isSingleton = isSingleton;
        return newData;
    }

    public SkillRankWithProbability Clone()
    {
        return (SkillRankWithProbability) ProbabilityClone();
    }

    public void CopyDataFrom(SkillRankWithProbability src)
    {
        SkillRankType = src.SkillRankType;
        probability = src.probability;
        isSingleton = src.isSingleton;
    }
}

[Flags]
public enum SkillRankType
{
    None = 0,
    Rank_1 = 1 << 0,
    Rank_2 = 1 << 1,
    Rank_3 = 1 << 2,
    Rank_4 = 1 << 3,
    Rank_5 = 1 << 4,
    Rank_6 = 1 << 5,
    Rank_7 = 1 << 6,
}
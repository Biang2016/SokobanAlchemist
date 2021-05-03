using System;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class SkillCategoryWithProbability : Probability, IClone<SkillCategoryWithProbability>
{
    public string Description => $"{SkillCategoryType} * {Probability}";

    [LabelText("技能分类")]
    public SkillCategoryType SkillCategoryType;

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
        SkillCategoryWithProbability newData = new SkillCategoryWithProbability();
        newData.SkillCategoryType = SkillCategoryType;
        newData.probability = probability;
        newData.isSingleton = isSingleton;
        return newData;
    }

    public SkillCategoryWithProbability Clone()
    {
        return (SkillCategoryWithProbability) ProbabilityClone();
    }

    public void CopyDataFrom(SkillCategoryWithProbability src)
    {
        SkillCategoryType = src.SkillCategoryType;
        probability = src.probability;
        isSingleton = src.isSingleton;
    }
}

[Flags]
public enum SkillCategoryType
{
    None = 0,
    Fire = 1 << 0,
    Ice = 1 << 1,
    Lightning = 1 << 2,
    Action = 1 << 3,
}
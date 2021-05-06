using System;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class SkillNameWithProbability : Probability, IClone<SkillNameWithProbability>
{
    public string Description
    {
        get
        {
            if (EntitySkillSO != null && EntitySkillSO.EntitySkill != null)
            {
                return $"{EntitySkillSO.EntitySkill.SkillAlias} * {Probability}";
            }

            return $"Null * {Probability}";
        }
    }

    [LabelText("技能")]
    [OnValueChanged("RefreshSkillGUID")]
    public EntitySkillSO EntitySkillSO;

    [ReadOnly]
    [LabelText("技能GUID")]
    public string SkillGUID = "";

    public void RefreshSkillGUID()
    {
        if (EntitySkillSO != null)
        {
            SkillGUID = EntitySkillSO.EntitySkill.SkillGUID;
        }
        else
        {
            SkillGUID = "";
        }
    }

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
        SkillNameWithProbability newData = new SkillNameWithProbability();
        newData.SkillGUID = SkillGUID;
        newData.probability = probability;
        newData.isSingleton = isSingleton;
        return newData;
    }

    public SkillNameWithProbability Clone()
    {
        return (SkillNameWithProbability) ProbabilityClone();
    }

    public void CopyDataFrom(SkillNameWithProbability src)
    {
        SkillGUID = src.SkillGUID;
        probability = src.probability;
        isSingleton = src.isSingleton;
    }
}
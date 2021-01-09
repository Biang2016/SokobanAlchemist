using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class SubSkillAliasWithProbability : Probability, IClone<SubSkillAliasWithProbability>
{
    [LabelText("子技能花名")]
    public string SubSkillAlias;

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
        SubSkillAliasWithProbability newData = new SubSkillAliasWithProbability();
        newData.SubSkillAlias = SubSkillAlias;
        newData.probability = probability;
        newData.isSingleton = isSingleton;
        return newData;
    }

    public SubSkillAliasWithProbability Clone()
    {
        return (SubSkillAliasWithProbability) ProbabilityClone();
    }
}
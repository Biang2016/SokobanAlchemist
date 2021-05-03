using System;
using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;

[Serializable]
public class SkillScrollProbability : IClone<SkillScrollProbability>
{
    public string Description => $"{ProbabilityMin}~{ProbabilityMax}";

    [LabelText("掉落概率小值")]
    public float ProbabilityMin = 0;

    [LabelText("掉落概率大值")]
    public float ProbabilityMax = 0;

    [LabelText("技能分类概率分布")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<SkillCategoryWithProbability> SkillCategoryWithProbabilityList = new List<SkillCategoryWithProbability>();

    [LabelText("技能阶级概率分布")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<SkillRankWithProbability> SkillRankWithProbabilityList = new List<SkillRankWithProbability>();

    public EntitySkill GetRandomRawSkill()
    {
        SkillCategoryWithProbability categoryP = CommonUtils.GetRandomWithProbabilityFromList(SkillCategoryWithProbabilityList);
        SkillCategoryType category = categoryP.SkillCategoryType;
        SkillRankWithProbability rankP = CommonUtils.GetRandomWithProbabilityFromList(SkillRankWithProbabilityList);
        SkillRankType rank = rankP.SkillRankType;
        EntitySkill rawEntitySkill = ConfigManager.GetRawEntitySkillByFilter(category, rank, true);
        return rawEntitySkill;
    }

    public SkillScrollProbability Clone()
    {
        SkillScrollProbability cloneData = new SkillScrollProbability();
        cloneData.ProbabilityMin = ProbabilityMin;
        cloneData.ProbabilityMax = ProbabilityMax;
        cloneData.SkillCategoryWithProbabilityList = SkillCategoryWithProbabilityList.Clone<SkillCategoryWithProbability, SkillCategoryWithProbability>();
        cloneData.SkillRankWithProbabilityList = SkillRankWithProbabilityList.Clone<SkillRankWithProbability, SkillRankWithProbability>();
        return cloneData;
    }
}
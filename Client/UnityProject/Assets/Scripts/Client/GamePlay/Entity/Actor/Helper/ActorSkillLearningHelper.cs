using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using UnityEngine;

/// <summary>
/// 此类作为角色技能学习和遗忘的数据存储类，不对外产生副作用
/// </summary>
public class ActorSkillLearningHelper : ActorMonoHelper
{
    public ActorSkillLearningData ActorSkillLearningData = new ActorSkillLearningData();

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
        ActorSkillLearningData.Clear();
    }

    /// <summary>
    /// 将Prefab原版配置的技能加载进来
    /// </summary>
    public void LoadInitSkills()
    {
        foreach (EntitySkillSO so in Entity.RawEntityPassiveSkillSOs)
        {
            ActorSkillLearningData.LearnedSkillGUIDs.Add(so.EntitySkill.SkillGUID);
            ActorSkillLearningData.LearnedPassiveSkillGUIDs.Add(so.EntitySkill.SkillGUID);
        }

        foreach (EntitySkillSO so in Entity.RawEntityActiveSkillSOs)
        {
            ActorSkillLearningData.LearnedSkillGUIDs.Add(so.EntitySkill.SkillGUID);
            ActorSkillLearningData.LearnedActiveSkillGUIDs.Add(so.EntitySkill.SkillGUID);
        }

        if (Entity is Actor actor)
        {
            if (actor.ActorControllerHelper is PlayerControllerHelper pch)
            {
                ActorSkillLearningData.SkillKeyMappings = pch.SkillKeyMappings.Clone<List<EntitySkillIndex>, List<EntitySkillIndex>>();
            }
        }
    }

    public void LearnActiveSkill(string skillGUID, EntitySkillIndex skillIndex, int keyBind, bool clearAllSkillOnKeyBind)
    {
        ActorSkillLearningData.LearnedActiveSkillGUIDs.Add(skillGUID);
        ActorSkillLearningData.LearnedSkillGUIDs.Add(skillGUID);
        if (clearAllSkillOnKeyBind) ActorSkillLearningData.SkillKeyMappings[keyBind].Clear();
        ActorSkillLearningData.SkillKeyMappings[keyBind].Add(skillIndex);
    }

    public void LearnPassiveSkill(string skillGUID)
    {
        ActorSkillLearningData.LearnedPassiveSkillGUIDs.Add(skillGUID);
        ActorSkillLearningData.LearnedSkillGUIDs.Add(skillGUID);
    }

    public void ForgetActiveSkill(string skillGUID, EntitySkillIndex skillIndex)
    {
        ActorSkillLearningData.LearnedActiveSkillGUIDs.Remove(skillGUID);
        ActorSkillLearningData.LearnedSkillGUIDs.Remove(skillGUID);
        foreach (List<EntitySkillIndex> entitySkillIndices in ActorSkillLearningData.SkillKeyMappings)
        {
            entitySkillIndices.Remove(skillIndex);
        }
    }

    public void ForgetPassiveSkill(string skillGUID)
    {
        ActorSkillLearningData.LearnedPassiveSkillGUIDs.Remove(skillGUID);
        ActorSkillLearningData.LearnedSkillGUIDs.Remove(skillGUID);
    }
}

public class ActorSkillLearningData : IClone<ActorSkillLearningData>
{
    public List<string> LearnedSkillGUIDs = new List<string>();

    public List<string> LearnedPassiveSkillGUIDs = new List<string>(); // 顺序重要，如果未来有重置、遗忘、或技能顺序联动的关系
    public List<string> LearnedActiveSkillGUIDs = new List<string>(); // 顺序重要，如果未来有重置、遗忘、或技能顺序联动的关系

    public List<List<EntitySkillIndex>> SkillKeyMappings = new List<List<EntitySkillIndex>>();

    public void Clear()
    {
        LearnedSkillGUIDs.Clear();
        LearnedPassiveSkillGUIDs.Clear();
        LearnedActiveSkillGUIDs.Clear();
        SkillKeyMappings.Clear();
    }

    public ActorSkillLearningData Clone()
    {
        ActorSkillLearningData cloneData = new ActorSkillLearningData();
        cloneData.LearnedSkillGUIDs = LearnedSkillGUIDs.Clone<string, string>();
        cloneData.LearnedPassiveSkillGUIDs = LearnedPassiveSkillGUIDs.Clone<string, string>();
        cloneData.LearnedActiveSkillGUIDs = LearnedActiveSkillGUIDs.Clone<string, string>();
        return cloneData;
    }
}
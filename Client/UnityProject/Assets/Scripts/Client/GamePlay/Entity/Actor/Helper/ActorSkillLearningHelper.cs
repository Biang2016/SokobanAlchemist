using System;
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
        ActorSkillLearningData.Init();
    }

    /// <summary>
    /// 将Prefab原版配置的技能加载进来
    /// </summary>
    public void LoadInitSkills()
    {
        foreach (EntitySkillSO so in Entity.RawEntityPassiveSkillSOs)
        {
            ActorSkillLearningData.LearnedPassiveSkillGUIDs.Add(so.EntitySkill.SkillGUID);
        }

        for (int skillIndexInt = 0; skillIndexInt < Entity.RawEntityActiveSkillSOs.Count; skillIndexInt++)
        {
            EntitySkillIndex skillIndex = (EntitySkillIndex) skillIndexInt;
            EntitySkillSO so = Entity.RawEntityActiveSkillSOs[skillIndexInt];
            ActorSkillLearningData.LearnedActiveSkillGUIDs.Add(so.EntitySkill.SkillGUID);
            ActorSkillLearningData.LearnedActiveSkillDict.Add(so.EntitySkill.SkillGUID, skillIndex);
        }

        if (Entity is Actor actor)
        {
            if (actor.ActorControllerHelper is PlayerControllerHelper pch)
            {
                foreach (KeyValuePair<PlayerControllerHelper.KeyBind, List<EntitySkillIndex>> kv in pch.SkillKeyMappings)
                {
                    ActorSkillLearningData.SkillKeyMappings[kv.Key] = kv.Value.Clone<EntitySkillIndex, EntitySkillIndex>();
                }
            }
        }
    }

    public void LearnActiveSkill(string skillGUID, EntitySkillIndex skillIndex, PlayerControllerHelper.KeyBind keyBind, bool clearAllSkillOnKeyBind)
    {
        ActorSkillLearningData.LearnedActiveSkillGUIDs.Add(skillGUID);
        ActorSkillLearningData.LearnedActiveSkillDict.Add(skillGUID, skillIndex);
        if (clearAllSkillOnKeyBind) ActorSkillLearningData.SkillKeyMappings[keyBind].Clear();
        ActorSkillLearningData.SkillKeyMappings[keyBind].Add(skillIndex);
    }

    public void BindActiveSkillToKey(EntitySkillIndex skillIndex, PlayerControllerHelper.KeyBind keyBind, bool clearAllExistedSkillInKeyBind)
    {
        if (clearAllExistedSkillInKeyBind) ActorSkillLearningData.SkillKeyMappings[keyBind].Clear();
        ActorSkillLearningData.SkillKeyMappings[keyBind].Add(skillIndex);
    }

    public void LearnPassiveSkill(string skillGUID)
    {
        ActorSkillLearningData.LearnedPassiveSkillGUIDs.Add(skillGUID);
    }

    public void ForgetActiveSkill(string skillGUID, EntitySkillIndex skillIndex)
    {
        ActorSkillLearningData.LearnedActiveSkillGUIDs.Remove(skillGUID);
        ActorSkillLearningData.LearnedActiveSkillDict.Remove(skillGUID);
        foreach (KeyValuePair<PlayerControllerHelper.KeyBind, List<EntitySkillIndex>> kv in ActorSkillLearningData.SkillKeyMappings)
        {
            kv.Value.Remove(skillIndex);
        }
    }

    public void ForgetPassiveSkill(string skillGUID)
    {
        ActorSkillLearningData.LearnedPassiveSkillGUIDs.Remove(skillGUID);
    }
}

public class ActorSkillLearningData : IClone<ActorSkillLearningData>
{
    public List<string> LearnedPassiveSkillGUIDs = new List<string>(); // 顺序重要，如果未来有重置、遗忘、或技能顺序联动的关系
    public List<string> LearnedActiveSkillGUIDs = new List<string>(); // 顺序重要，如果未来有重置、遗忘、或技能顺序联动的关系
    public Dictionary<string, EntitySkillIndex> LearnedActiveSkillDict = new Dictionary<string, EntitySkillIndex>();
    public SortedDictionary<PlayerControllerHelper.KeyBind, List<EntitySkillIndex>> SkillKeyMappings = new SortedDictionary<PlayerControllerHelper.KeyBind, List<EntitySkillIndex>>();

    public void Init()
    {
        Clear();
        foreach (PlayerControllerHelper.KeyBind keyBind in Enum.GetValues(typeof(PlayerControllerHelper.KeyBind)))
        {
            if (!SkillKeyMappings.ContainsKey(keyBind))
            {
                SkillKeyMappings.Add(keyBind, new List<EntitySkillIndex>());
            }
        }
    }

    public void Clear()
    {
        LearnedPassiveSkillGUIDs.Clear();
        LearnedActiveSkillGUIDs.Clear();
        LearnedActiveSkillDict.Clear();

        foreach (KeyValuePair<PlayerControllerHelper.KeyBind, List<EntitySkillIndex>> kv in SkillKeyMappings)
        {
            kv.Value.Clear();
        }
    }

    public ActorSkillLearningData Clone()
    {
        ActorSkillLearningData cloneData = new ActorSkillLearningData();
        cloneData.LearnedPassiveSkillGUIDs = LearnedPassiveSkillGUIDs.Clone<string, string>();
        cloneData.LearnedActiveSkillGUIDs = LearnedActiveSkillGUIDs.Clone<string, string>();
        cloneData.LearnedActiveSkillDict = LearnedActiveSkillDict.Clone<string, EntitySkillIndex, string, EntitySkillIndex>();

        foreach (KeyValuePair<PlayerControllerHelper.KeyBind, List<EntitySkillIndex>> kv in SkillKeyMappings)
        {
            cloneData.SkillKeyMappings[kv.Key] = kv.Value.Clone<EntitySkillIndex, EntitySkillIndex>();
        }

        return cloneData;
    }
}
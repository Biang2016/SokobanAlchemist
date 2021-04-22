using System.Collections.Generic;
using UnityEngine;

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
    }

    public void LearnSkill(string skillGUID, int keyBind)
    {
        if (!ActorSkillLearningData.LearnedSkillGUIDSet.Contains(skillGUID))
        {
            ActorSkillLearningData.LearnedSkillGUIDSet.Add(skillGUID);
            EntitySkill entitySkill = ConfigManager.GetEntitySkill(skillGUID);
            if (entitySkill is EntityActiveSkill activeSkill)
            {
                ActorSkillLearningData.LearnedActiveSkillGUIDs.Add(skillGUID);
                Entity.AddNewActiveSkill(activeSkill, keyBind);
            }
            else if (entitySkill is EntityPassiveSkill passiveSkill)
            {
                ActorSkillLearningData.LearnedPassiveSkillGUIDs.Add(skillGUID);
                Entity.AddNewPassiveSkill(passiveSkill);
            }
        }
    }
}

public class ActorSkillLearningData
{
    public List<string> LearnedSkillGUIDs = new List<string>();
    public HashSet<string> LearnedSkillGUIDSet = new HashSet<string>();

    public List<string> LearnedPassiveSkillGUIDs = new List<string>(); // 顺序重要，如果未来有重置、遗忘、或技能顺序联动的关系
    public List<string> LearnedActiveSkillGUIDs = new List<string>(); // 顺序重要，如果未来有重置、遗忘、或技能顺序联动的关系
}
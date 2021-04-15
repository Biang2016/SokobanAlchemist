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

    public void LearnPassiveSkill(string skillGUID)
    {
        if (!ActorSkillLearningData.LearnedPassiveSkillGUIDSet.Contains(skillGUID))
        {
            ActorSkillLearningData.LearnedPassiveSkillGUIDs.Add(skillGUID);
            ActorSkillLearningData.LearnedPassiveSkillGUIDSet.Add(skillGUID);
            EntityPassiveSkill eps = (EntityPassiveSkill)ConfigManager.GetEntitySkill(skillGUID);
            Entity.AddNewPassiveSkill(eps);
        }
    }
}

public class ActorSkillLearningData
{
    public List<string> LearnedPassiveSkillGUIDs = new List<string>();
    public HashSet<string> LearnedPassiveSkillGUIDSet = new HashSet<string>();
}
using System;
using UnityEngine;

[Serializable]
public abstract class ActorPassiveSkillAction : EntityPassiveSkillAction
{
    protected override string Description => "Actor被动技能行为基类";

    public Actor Actor
    {
        get
        {
            if (Entity is Actor actor) return actor;
            else
            {
                Debug.LogError($"{Entity.name}上非法添加了Actor专用的被动技能行为{GetType().Name}");
                return null;
            }
        }
    }
}
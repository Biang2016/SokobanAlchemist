using System;
using UnityEngine;

[Serializable]
public abstract class ActorPassiveSkill : EntityPassiveSkill
{
    protected override string Description => "Actor被动技能基类";

    public Actor Actor
    {
        get
        {
            if (Entity is Actor actor) return actor;
            else
            {
                Debug.LogError($"{Entity.name}上非法添加了Actor专用的被动技能{GetType().Name}");
                return null;
            }
        }
    }
}
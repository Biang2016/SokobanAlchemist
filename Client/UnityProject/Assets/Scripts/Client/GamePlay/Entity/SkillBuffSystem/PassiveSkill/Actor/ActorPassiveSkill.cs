using System;
using UnityEngine;

[Serializable]
public abstract class ActorPassiveSkill : EntityPassiveSkill
{
    protected override string Description => "Actor被动技能基类";

    private Actor m_actor;

    public Actor Actor
    {
        get
        {
            if (m_actor != null) return m_actor;
            if (Entity is Actor actor)
            {
                m_actor = actor;
                return m_actor;
            }
            else
            {
                Debug.LogError($"{Entity.name}上非法添加了Actor专用的被动技能{GetType().Name}");
                return null;
            }
        }
    }
}
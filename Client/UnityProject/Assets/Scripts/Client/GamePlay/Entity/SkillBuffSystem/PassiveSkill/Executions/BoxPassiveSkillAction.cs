using System;
using UnityEngine;

[Serializable]
public abstract class BoxPassiveSkillAction : EntityPassiveSkillAction
{
    protected override string Description => "Box被动技能行为基类";

    private Box m_box;

    public Box Box
    {
        get
        {
            if (m_box != null) return m_box;
            if (Entity is Box box)
            {
                m_box = box;
                return m_box;
            }
            else
            {
                Debug.LogError($"{Entity.name}上非法添加了Box专用的被动技能行为{GetType().Name}");
                return null;
            }
        }
    }
}
using System;
using UnityEngine;

[Serializable]
public abstract class BoxPassiveSkill : EntityPassiveSkill
{
    protected override string Description => "box被动技能基类";

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
                Debug.LogError($"{Entity.name}上非法添加了box专用的被动技能{GetType().Name}");
                return null;
            }
        }
    }
}
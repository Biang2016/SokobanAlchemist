using System;
using UnityEngine;

[Serializable]
public abstract class BoxPassiveSkillAction : EntityPassiveSkillAction
{
    protected override string Description => "Box被动技能行为基类";

    public Box Box
    {
        get
        {
            if (Entity is Box box) return box;
            else
            {
                Debug.LogError($"{Entity.name}上非法添加了Box专用的被动技能行为{GetType().Name}");
                return null;
            }
        }
    }
}
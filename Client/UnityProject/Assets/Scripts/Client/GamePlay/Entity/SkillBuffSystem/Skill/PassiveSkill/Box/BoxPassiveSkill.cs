using System;
using UnityEngine;

[Serializable]
public abstract class BoxPassiveSkill : EntityPassiveSkill
{
    protected override string Description => "box被动技能基类";

    public Box Box
    {
        get
        {
            if (Entity is Box box) return box;
            else
            {
                Debug.LogError($"{Entity.name}上非法添加了box专用的被动技能{GetType().Name}");
                return null;
            }
        }
    }
}
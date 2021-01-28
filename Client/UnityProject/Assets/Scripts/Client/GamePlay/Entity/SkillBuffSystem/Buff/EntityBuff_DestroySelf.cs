using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntityBuff_DestroySelf : EntityBuff
{
    protected override string Description => "毁灭自己";

    public override void OnAdded(Entity entity)
    {
        base.OnAdded(entity);
        if (!entity.IsNotNullAndAlive()) return;
        entity.PassiveSkillMarkAsDestroyed = true;
    }

    protected override void ChildClone(EntityBuff newBuff)
    {
        base.ChildClone(newBuff);
        EntityBuff_DestroySelf buff = ((EntityBuff_DestroySelf) newBuff);
    }
}
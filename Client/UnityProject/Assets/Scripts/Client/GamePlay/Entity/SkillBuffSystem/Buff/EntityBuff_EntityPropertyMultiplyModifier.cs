using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntityBuff_EntityPropertyMultiplyModifier : EntityBuff
{
    protected override string Description => "属性乘法修正Buff, buff结束后消除该修正值";

    public EntityBuff_EntityPropertyMultiplyModifier()
    {
        MultiplyModifier = new Property.MultiplyModifier {Percent = Percent};
    }

    [LabelText("增加比率%")]
    public int Percent;

    [LabelText("在持续时间内线性衰减至0")]
    [HideIf("IsPermanent")]
    public bool LinearDecayInDuration;

    [LabelText("属性类型")]
    public EntityPropertyType EntityPropertyType;

    internal Property.MultiplyModifier MultiplyModifier;

    public override void OnAdded(Entity entity)
    {
        base.OnAdded(entity);
        if (!entity.IsNotNullAndAlive()) return;
        if (entity.EntityStatPropSet.PropertyDict.TryGetValue(EntityPropertyType, out EntityProperty property))
        {
            property.AddModifier(MultiplyModifier);
        }
    }

    public override void OnFixedUpdate(Entity entity, float passedTime, float remainTime)
    {
        base.OnFixedUpdate(entity, passedTime, remainTime);
        if (!entity.IsNotNullAndAlive()) return;
        if (!IsPermanent && LinearDecayInDuration)
        {
            MultiplyModifier.Percent = Mathf.RoundToInt(Percent * remainTime / Duration);
        }
    }

    public override void OnRemoved(Entity entity)
    {
        base.OnRemoved(entity);
        if (!entity.IsNotNullAndAlive()) return;
        if (entity.EntityStatPropSet.PropertyDict.TryGetValue(EntityPropertyType, out EntityProperty property))
        {
            if (!property.RemoveModifier(MultiplyModifier))
            {
                Debug.Log($"RemoveMultiplyModifier: {EntityPropertyType} failed from {entity.name}");
            }
        }

        MultiplyModifier = null;
    }

    protected override void ChildClone(EntityBuff newBuff)
    {
        base.ChildClone(newBuff);
        EntityBuff_EntityPropertyMultiplyModifier buff = ((EntityBuff_EntityPropertyMultiplyModifier) newBuff);
        buff.Percent = Percent;
        buff.LinearDecayInDuration = LinearDecayInDuration;
        buff.EntityPropertyType = EntityPropertyType;
        buff.MultiplyModifier = new Property.MultiplyModifier {Percent = Percent};
    }
}
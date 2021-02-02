using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntityBuff_EntityPropertyPlusModifier : EntityBuff
{
    protected override string Description => "属性加法修正Buff, buff结束后消除该修正值";

    public EntityBuff_EntityPropertyPlusModifier()
    {
        PlusModifier = new Property.PlusModifier {Delta = Delta};
    }

    [LabelText("属性类型")]
    public EntityPropertyType EntityPropertyType;

    [LabelText("变化量")]
    public int Delta;

    [LabelText("在持续时间内线性衰减至0")]
    [HideIf("IsPermanent")]
    public bool LinearDecayInDuration;

    internal Property.PlusModifier PlusModifier;

    public override void OnAdded(Entity entity, string extraInfo)
    {
        base.OnAdded(entity);
        if (!entity.IsNotNullAndAlive()) return;
        if (entity.EntityStatPropSet.PropertyDict.TryGetValue(EntityPropertyType, out EntityProperty property))
        {
            property.AddModifier(PlusModifier);
        }
    }

    public override void OnFixedUpdate(Entity entity, float passedTime, float remainTime)
    {
        base.OnFixedUpdate(entity, passedTime, remainTime);
        if (!entity.IsNotNullAndAlive()) return;
        if (!IsPermanent && LinearDecayInDuration)
        {
            PlusModifier.Delta = Mathf.RoundToInt(Delta * remainTime / Duration);
        }
    }

    public override void OnRemoved(Entity entity)
    {
        base.OnRemoved(entity);
        if (!entity.IsNotNullAndAlive()) return;
        if (entity.EntityStatPropSet.PropertyDict.TryGetValue(EntityPropertyType, out EntityProperty property))
        {
            if (!property.RemoveModifier(PlusModifier))
            {
                Debug.LogError($"Failed to RemovePlusModifier: {EntityPropertyType} from {entity.name}");
            }
        }

        PlusModifier = null;
    }

    protected override void ChildClone(EntityBuff newBuff)
    {
        base.ChildClone(newBuff);
        EntityBuff_EntityPropertyPlusModifier buff = ((EntityBuff_EntityPropertyPlusModifier) newBuff);
        buff.Delta = Delta;
        buff.EntityPropertyType = EntityPropertyType;
        buff.LinearDecayInDuration = LinearDecayInDuration;
        buff.PlusModifier = new Property.PlusModifier {Delta = Delta};
    }
}
using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class BoxBuff : EntityBuff
{
    public override void OnAdded(Entity entity)
    {
        base.OnAdded(entity);
    }

    public override void OnFixedUpdate(Entity entity, float passedTime, float remainTime)
    {
    }

    public override void OnRemoved(Entity entity)
    {
    }
}

[Serializable]
public class BoxBuff_BoxPropertyMultiplyModifier : BoxBuff
{
    protected override string Description => "Box属性乘法修正Buff, 必须是延时buff, buff结束后消除该修正值";

    public BoxBuff_BoxPropertyMultiplyModifier()
    {
        MultiplyModifier = new Property.MultiplyModifier {Percent = Percent};
    }

    [LabelText("增加比率%")]
    public int Percent;

    [LabelText("在持续时间内线性衰减至0")]
    [HideIf("IsPermanent")]
    public bool LinearDecayInDuration;

    [LabelText("Box属性类型")]
    public BoxPropertyType PropertyType;

    internal Property.MultiplyModifier MultiplyModifier;

    public override void OnAdded(Entity entity)
    {
        base.OnAdded(entity);
        Box box = (Box) entity;
        if (box.IsRecycled) return;
        if (box.BoxStatPropSet.PropertyDict.TryGetValue(PropertyType, out BoxProperty property))
        {
            property.AddModifier(MultiplyModifier);
        }
    }

    public override void OnFixedUpdate(Entity entity, float passedTime, float remainTime)
    {
        base.OnFixedUpdate(entity, passedTime, remainTime);
        Box box = (Box) entity;
        if (box.IsRecycled) return;
        if (!IsPermanent && LinearDecayInDuration)
        {
            MultiplyModifier.Percent = Mathf.RoundToInt(Percent * remainTime / Duration);
        }
    }

    public override void OnRemoved(Entity entity)
    {
        base.OnRemoved(entity);
        Box box = (Box) entity;
        if (box.IsRecycled) return;
        if (box.BoxStatPropSet.PropertyDict.TryGetValue(PropertyType, out BoxProperty property))
        {
            if (!property.RemoveModifier(MultiplyModifier))
            {
                Debug.Log($"RemoveMultiplyModifier: {PropertyType} failed from {box.name}");
            }
        }

        MultiplyModifier = null;
    }

    protected override bool ValidateBuffAttribute(BuffAttribute buffAttribute)
    {
        if (buffAttribute == BuffAttribute.InstantEffect)
        {
            validateBuffAttributeInfo = "本Buff不支持【瞬时效果】标签";
            return false;
        }

        return true;
    }

    protected override void ChildClone(EntityBuff newBuff)
    {
        base.ChildClone(newBuff);
        BoxBuff_BoxPropertyMultiplyModifier buff = ((BoxBuff_BoxPropertyMultiplyModifier) newBuff);
        buff.Percent = Percent;
        buff.LinearDecayInDuration = LinearDecayInDuration;
        buff.PropertyType = PropertyType;
        buff.MultiplyModifier = new Property.MultiplyModifier {Percent = Percent};
    }
}

[Serializable]
public class BoxBuff_BoxPropertyPlusModifier : BoxBuff
{
    protected override string Description => "角色属性加法修正Buff, 必须是延时buff, buff结束后消除该修正值";

    public BoxBuff_BoxPropertyPlusModifier()
    {
        PlusModifier = new Property.PlusModifier {Delta = Delta};
    }

    [LabelText("角色属性类型")]
    public BoxPropertyType PropertyType;

    [LabelText("变化量")]
    public int Delta;

    [LabelText("在持续时间内线性衰减至0")]
    [HideIf("IsPermanent")]
    public bool LinearDecayInDuration;

    internal Property.PlusModifier PlusModifier;

    public override void OnAdded(Entity entity)
    {
        base.OnAdded(entity);
        Box box = (Box) entity;
        if (box.IsRecycled) return;
        if (box.BoxStatPropSet.PropertyDict.TryGetValue(PropertyType, out BoxProperty property))
        {
            property.AddModifier(PlusModifier);
        }
    }

    public override void OnFixedUpdate(Entity entity, float passedTime, float remainTime)
    {
        base.OnFixedUpdate(entity, passedTime, remainTime);
        Box box = (Box) entity;
        if (box.IsRecycled) return;
        if (!IsPermanent && LinearDecayInDuration)
        {
            PlusModifier.Delta = Mathf.RoundToInt(Delta * remainTime / Duration);
        }
    }

    public override void OnRemoved(Entity entity)
    {
        base.OnRemoved(entity);
        Box box = (Box) entity;
        if (box.IsRecycled) return;
        if (box.BoxStatPropSet.PropertyDict.TryGetValue(PropertyType, out BoxProperty property))
        {
            if (!property.RemoveModifier(PlusModifier))
            {
                Debug.LogError($"Failed to RemovePlusModifier: {PropertyType} from {box.name}");
            }
        }

        PlusModifier = null;
    }

    protected override bool ValidateBuffAttribute(BuffAttribute buffAttribute)
    {
        if (buffAttribute == BuffAttribute.InstantEffect)
        {
            validateBuffAttributeInfo = "本Buff不支持【瞬时效果】标签";
            return false;
        }

        return true;
    }

    protected override void ChildClone(EntityBuff newBuff)
    {
        base.ChildClone(newBuff);
        BoxBuff_BoxPropertyPlusModifier buff = ((BoxBuff_BoxPropertyPlusModifier) newBuff);
        buff.Delta = Delta;
        buff.PropertyType = PropertyType;
        buff.LinearDecayInDuration = LinearDecayInDuration;
        buff.PlusModifier = new Property.PlusModifier {Delta = Delta};
    }
}

[Serializable]
public class BoxBuff_ChangeBoxStatInstantly : BoxBuff
{
    protected override string Description => "瞬间更改Box异常状态累积值, 必须是【瞬时效果】. buff施加后, 不残留在角色身上, 无移除的概念。但此buff有可能被既有buff免疫或抵消等";

    [LabelText("Box属性类型")]
    [ValidateInput("ValidateStatType", "请选择异常状态累积值")]
    public BoxStatType StatType;

    private bool ValidateStatType(BoxStatType statType)
    {
        if (statType == BoxStatType.FiringValue || statType == BoxStatType.FrozenValue)
        {
            return true;
        }

        return false;
    }

    [LabelText("变化量")]
    public int Delta;

    [LabelText("增加比率%")]
    public int Percent;

    public override void OnAdded(Entity entity)
    {
        base.OnAdded(entity);
        Box box = (Box) entity;
        if (box.IsRecycled) return;
        float valueBefore = box.BoxStatPropSet.StatDict[StatType].Value;
        valueBefore += Delta;
        valueBefore *= (100 + Percent) / 100f;
        box.BoxStatPropSet.StatDict[StatType].Value = Mathf.RoundToInt(valueBefore);
    }

    protected override bool ValidateBuffAttribute(BuffAttribute boxBuffAttribute)
    {
        if (boxBuffAttribute != BuffAttribute.InstantEffect)
        {
            validateBuffAttributeInfo = "本Buff仅支持【瞬时效果】标签";
            return false;
        }

        return true;
    }

    protected override void ChildClone(EntityBuff newBuff)
    {
        base.ChildClone(newBuff);
        BoxBuff_ChangeBoxStatInstantly buff = ((BoxBuff_ChangeBoxStatInstantly) newBuff);
        buff.StatType = StatType;
        buff.Delta = Delta;
        buff.Percent = Percent;
    }
}

[Serializable]
public class BoxBuff_InstantDamage : BoxBuff
{
    protected override string Description => "瞬间伤害buff, 必须是【瞬时效果】. buff施加后, 不残留在Box身上, 无移除的概念。但此buff有可能被既有buff免疫或抵消等";

    [LabelText("伤害")]
    public int Damage;

    public override void OnAdded(Entity entity)
    {
        base.OnAdded(entity);
        Box box = (Box) entity;
        if (box.IsRecycled) return;
        box.BoxStatPropSet.CommonDurability.Value -= Damage;
    }

    protected override bool ValidateBuffAttribute(BuffAttribute boxBuffAttribute)
    {
        if (boxBuffAttribute != BuffAttribute.InstantEffect)
        {
            validateBuffAttributeInfo = "本Buff仅支持【瞬时效果】标签";
            return false;
        }

        return true;
    }

    protected override void ChildClone(EntityBuff newBuff)
    {
        base.ChildClone(newBuff);
        BoxBuff_InstantDamage buff = ((BoxBuff_InstantDamage) newBuff);
        buff.Damage = Damage;
    }
}
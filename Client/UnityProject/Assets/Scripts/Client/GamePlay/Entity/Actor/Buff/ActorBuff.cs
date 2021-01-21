using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class ActorBuff : EntityBuff
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
public class ActorBuff_AttributeLabel : ActorBuff
{
    protected override string Description => "本Buff仅提供标签作用";
}

[Serializable]
public class ActorBuff_ActorPropertyMultiplyModifier : ActorBuff
{
    protected override string Description => "角色属性乘法修正Buff, 必须是延时buff, buff结束后消除该修正值";

    public ActorBuff_ActorPropertyMultiplyModifier()
    {
        MultiplyModifier = new Property.MultiplyModifier {Percent = Percent};
    }

    [LabelText("增加比率%")]
    public int Percent;

    [LabelText("在持续时间内线性衰减至0")]
    [HideIf("IsPermanent")]
    public bool LinearDecayInDuration;

    [LabelText("角色属性类型")]
    public ActorPropertyType PropertyType;

    internal Property.MultiplyModifier MultiplyModifier;

    public override void OnAdded(Entity entity)
    {
        base.OnAdded(entity);
        Actor actor = (Actor) entity;
        if (actor.IsRecycled) return;
        if (actor.ActorStatPropSet.PropertyDict.TryGetValue(PropertyType, out ActorProperty property))
        {
            property.AddModifier(MultiplyModifier);
        }
    }

    public override void OnFixedUpdate(Entity entity, float passedTime, float remainTime)
    {
        base.OnFixedUpdate(entity, passedTime, remainTime);
        Actor actor = (Actor) entity;
        if (actor.IsRecycled) return;
        if (!IsPermanent && LinearDecayInDuration)
        {
            MultiplyModifier.Percent = Mathf.RoundToInt(Percent * remainTime / Duration);
        }
    }

    public override void OnRemoved(Entity entity)
    {
        base.OnRemoved(entity);
        Actor actor = (Actor) entity;
        if (actor.IsRecycled) return;
        if (actor.ActorStatPropSet.PropertyDict.TryGetValue(PropertyType, out ActorProperty property))
        {
            if (!property.RemoveModifier(MultiplyModifier))
            {
                Debug.Log($"RemoveMultiplyModifier: {PropertyType} failed from {actor.name}");
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
        ActorBuff_ActorPropertyMultiplyModifier buff = ((ActorBuff_ActorPropertyMultiplyModifier) newBuff);
        buff.Percent = Percent;
        buff.LinearDecayInDuration = LinearDecayInDuration;
        buff.PropertyType = PropertyType;
        buff.MultiplyModifier = new Property.MultiplyModifier {Percent = Percent};
    }
}

[Serializable]
public class ActorBuff_ActorPropertyPlusModifier : ActorBuff
{
    protected override string Description => "角色属性加法修正Buff, 必须是延时buff, buff结束后消除该修正值";

    public ActorBuff_ActorPropertyPlusModifier()
    {
        PlusModifier = new Property.PlusModifier {Delta = Delta};
    }

    [LabelText("角色属性类型")]
    public ActorPropertyType PropertyType;

    [LabelText("变化量")]
    public int Delta;

    [LabelText("在持续时间内线性衰减至0")]
    [HideIf("IsPermanent")]
    public bool LinearDecayInDuration;

    internal Property.PlusModifier PlusModifier;

    public override void OnAdded(Entity entity)
    {
        base.OnAdded(entity);
        Actor actor = (Actor) entity;
        if (actor.IsRecycled) return;
        if (actor.ActorStatPropSet.PropertyDict.TryGetValue(PropertyType, out ActorProperty property))
        {
            property.AddModifier(PlusModifier);
        }
    }

    public override void OnFixedUpdate(Entity entity, float passedTime, float remainTime)
    {
        base.OnFixedUpdate(entity, passedTime, remainTime);
        Actor actor = (Actor) entity;
        if (actor.IsRecycled) return;
        if (!IsPermanent && LinearDecayInDuration)
        {
            PlusModifier.Delta = Mathf.RoundToInt(Delta * remainTime / Duration);
        }
    }

    public override void OnRemoved(Entity entity)
    {
        base.OnRemoved(entity);
        Actor actor = (Actor) entity;
        if (actor.IsRecycled) return;
        if (actor.ActorStatPropSet.PropertyDict.TryGetValue(PropertyType, out ActorProperty property))
        {
            if (!property.RemoveModifier(PlusModifier))
            {
                Debug.LogError($"Failed to RemovePlusModifier: {PropertyType} from {actor.name}");
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
        ActorBuff_ActorPropertyPlusModifier buff = ((ActorBuff_ActorPropertyPlusModifier) newBuff);
        buff.Delta = Delta;
        buff.PropertyType = PropertyType;
        buff.LinearDecayInDuration = LinearDecayInDuration;
        buff.PlusModifier = new Property.PlusModifier {Delta = Delta};
    }
}

[Serializable]
public class ActorBuff_InstantDamage : ActorBuff
{
    protected override string Description => "瞬间伤害buff, 必须是【瞬时效果】. buff施加后, 不残留在角色身上, 无移除的概念。但此buff有可能被既有buff免疫或抵消等";

    [LabelText("伤害")]
    public int Damage;

    public override void OnAdded(Entity entity)
    {
        base.OnAdded(entity);
        Actor actor = (Actor) entity;
        if (actor.IsRecycled) return;
        actor.ActorBattleHelper.Damage(actor, Damage); // 此处施加对象是自己，暂时有点奇怪
    }

    protected override bool ValidateBuffAttribute(BuffAttribute actorBuffAttribute)
    {
        if (actorBuffAttribute != BuffAttribute.InstantEffect)
        {
            validateBuffAttributeInfo = "本Buff仅支持【瞬时效果】标签";
            return false;
        }

        return true;
    }

    protected override void ChildClone(EntityBuff newBuff)
    {
        base.ChildClone(newBuff);
        ActorBuff_InstantDamage buff = ((ActorBuff_InstantDamage) newBuff);
        buff.Damage = Damage;
    }
}

[Serializable]
public class ActorBuff_InstantHeal : ActorBuff
{
    protected override string Description => "瞬间治疗buff, 必须是【瞬时效果】. buff施加后, 不残留在角色身上, 无移除的概念。但此buff有可能被既有buff免疫或抵消等";

    [LabelText("治疗量")]
    public int Health;

    public override void OnAdded(Entity entity)
    {
        base.OnAdded(entity);
        Actor actor = (Actor) entity;
        if (actor.IsRecycled) return;
        actor.ActorBattleHelper.Heal(actor, Health); // 此处施加对象是自己，暂时有点奇怪
    }

    protected override bool ValidateBuffAttribute(BuffAttribute actorBuffAttribute)
    {
        if (actorBuffAttribute != BuffAttribute.InstantEffect)
        {
            validateBuffAttributeInfo = "本Buff仅支持【瞬时效果】标签";
            return false;
        }

        return true;
    }

    protected override void ChildClone(EntityBuff newBuff)
    {
        base.ChildClone(newBuff);
        ActorBuff_InstantHeal buff = ((ActorBuff_InstantHeal) newBuff);
        buff.Health = Health;
    }
}

[Serializable]
public class ActorBuff_ChangeActorStatInstantly : ActorBuff
{
    protected override string Description => "瞬间更改角色异常状态累积值, 必须是【瞬时效果】. buff施加后, 不残留在角色身上, 无移除的概念。但此buff有可能被既有buff免疫或抵消等";

    [LabelText("角色属性类型")]
    [ValidateInput("ValidateStatType", "请选择异常状态累积值")]
    public ActorStatType StatType;

    private bool ValidateStatType(ActorStatType statType)
    {
        if (statType == ActorStatType.FrozenValue || statType == ActorStatType.FiringValue)
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
        Actor actor = (Actor) entity;
        if (actor.IsRecycled) return;
        float valueBefore = actor.ActorStatPropSet.StatDict[StatType].Value;
        valueBefore += Delta;
        valueBefore *= (100 + Percent) / 100f;
        actor.ActorStatPropSet.StatDict[StatType].Value = Mathf.RoundToInt(valueBefore);
    }

    protected override bool ValidateBuffAttribute(BuffAttribute actorBuffAttribute)
    {
        if (actorBuffAttribute != BuffAttribute.InstantEffect)
        {
            validateBuffAttributeInfo = "本Buff仅支持【瞬时效果】标签";
            return false;
        }

        return true;
    }

    protected override void ChildClone(EntityBuff newBuff)
    {
        base.ChildClone(newBuff);
        ActorBuff_ChangeActorStatInstantly buff = ((ActorBuff_ChangeActorStatInstantly) newBuff);
        buff.StatType = StatType;
        buff.Delta = Delta;
        buff.Percent = Percent;
    }
}

public enum BuffAttribute
{
    [LabelText("瞬时效果")]
    InstantEffect = 0,

    [LabelText("加速")]
    SpeedUp = 1,

    [LabelText("行动力")]
    ActionPoint = 2,

    [LabelText("减速")]
    SlowDown = 3,

    [LabelText("眩晕")]
    Stun = 4,

    [LabelText("眩晕免疫")]
    StunImmune = 5,

    [LabelText("无敌")]
    Invincible = 6,

    [LabelText("隐身")]
    Hiding = 7,

    [LabelText("中毒")]
    Poison = 8,

    [LabelText("最大血量")]
    MaxHealth = 9,

    [LabelText("击退")]
    Repulse = 10,
}

public enum BuffAttributeRelationship
{
    [LabelText("相容")]
    Compatible, // Buff相容

    [LabelText("互斥")]
    Mutex, // 直接替换

    [LabelText("排斥")]
    Repel, // 后者无法添加

    [LabelText("抵消")]
    SetOff, // 两者同时消失

    [LabelText("大值优先")]
    MaxDominant, // 仅针对同种ActorBuffAttribute，允许多buff共存但同一时刻仅最大值生效，各buff分别计时
}
using System;
using System.Collections.Generic;
using BiangStudio.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class ActorBuff : IClone<ActorBuff>
{
    #region GUID

    [ReadOnly]
    [HideInEditorMode]
    public uint GUID;

    private static uint guidGenerator = (uint) ConfigManager.GUID_Separator.Buff;

    private uint GetGUID()
    {
        return guidGenerator++;
    }

    #endregion

    protected abstract string ActorBuffDisplayName { get; }

    [InfoBox("@ActorBuffDisplayName")]
    [LabelText("Buff标签")]
    [ValidateInput("ValidateActorBuffAttribute", "$validateActorBuffAttributeInfo")]
    public ActorBuffAttribute ActorBuffAttribute;

    protected string validateActorBuffAttributeInfo = "";

    protected virtual bool ValidateActorBuffAttribute(ActorBuffAttribute actorBuffAttribute)
    {
        return true;
    }

    [LabelText("永久Buff")]
    [HideIf("ActorBuffAttribute", ActorBuffAttribute.InstantEffect)]
    public bool IsPermanent;

    [LabelText("Buff持续时间")]
    [HideIf("IsPermanent")]
    [HideIf("ActorBuffAttribute", ActorBuffAttribute.InstantEffect)]
    [ValidateInput("ValidateDuration", "若非【瞬时效果】或永久Buff，持续时间不可为0")]
    public float Duration;

    private bool ValidateDuration(float duration)
    {
        if (!IsPermanent && duration.Equals(0))
        {
            return false;
        }

        return true;
    }

    [ValueDropdown("GetAllFXTypeNames")]
    [LabelText("Buff特效")]
    public string BuffFX;

    [LabelText("Buff特效尺寸")]
    public float BuffFXScale = 1.0f;

    protected ActorBuff()
    {
        GUID = GetGUID();
    }

    public virtual void OnAdded(Actor actor)
    {
    }

    public virtual void OnFixedUpdate(Actor actor, float passedTime, float remainTime)
    {
    }

    public virtual void OnRemoved(Actor actor)
    {
    }

    public ActorBuff Clone()
    {
        Type type = GetType();
        ActorBuff newBuff = (ActorBuff) Activator.CreateInstance(type);
        newBuff.ActorBuffAttribute = ActorBuffAttribute;
        newBuff.IsPermanent = IsPermanent;
        newBuff.Duration = Duration;
        newBuff.BuffFX = BuffFX;
        newBuff.BuffFXScale = BuffFXScale;
        ChildClone(newBuff);
        return newBuff;
    }

    protected virtual void ChildClone(ActorBuff newBuff)
    {
    }

    #region Utils

    private IEnumerable<string> GetAllBoxTypeNames => ConfigManager.GetAllBoxTypeNames();

    private IEnumerable<string> GetAllFXTypeNames => ConfigManager.GetAllFXTypeNames();

    #endregion
}

[Serializable]
public class ActorBuff_AttributeLabel : ActorBuff
{
    protected override string ActorBuffDisplayName => "本Buff仅提供标签作用";
}

[Serializable]
public class ActorBuff_ActorPropertyMultiplyModifier : ActorBuff
{
    protected override string ActorBuffDisplayName => "角色属性乘法修正Buff, 必须是延时buff, buff结束后消除该修正值";

    public ActorBuff_ActorPropertyMultiplyModifier()
    {
        MultiplyModifier = new ActorProperty.MultiplyModifier {Percent = Percent};
    }

    [LabelText("增加比率%")]
    public int Percent;

    [LabelText("在持续时间内线性衰减至0")]
    [HideIf("IsPermanent")]
    public bool LinearDecayInDuration;

    [LabelText("角色属性类型")]
    public ActorProperty.PropertyType PropertyType;

    internal ActorProperty.MultiplyModifier MultiplyModifier;

    public override void OnAdded(Actor actor)
    {
        base.OnAdded(actor);
        if (actor.ActorStatPropSet.PropertyDict.TryGetValue(PropertyType, out ActorProperty property))
        {
            property.AddModifier(MultiplyModifier);
        }
    }

    public override void OnFixedUpdate(Actor actor, float passedTime, float remainTime)
    {
        base.OnFixedUpdate(actor, passedTime, remainTime);
        if (!IsPermanent && LinearDecayInDuration)
        {
            MultiplyModifier.Percent = Mathf.RoundToInt(Percent * remainTime / Duration);
        }
    }

    public override void OnRemoved(Actor actor)
    {
        base.OnRemoved(actor);
        if (actor.ActorStatPropSet.PropertyDict.TryGetValue(PropertyType, out ActorProperty property))
        {
            if (!property.RemoveModifier(MultiplyModifier))
            {
                Debug.Log($"RemoveMultiplyModifier: {PropertyType} failed from {actor.name}");
            }
        }

        MultiplyModifier = null;
    }

    protected override bool ValidateActorBuffAttribute(ActorBuffAttribute actorBuffAttribute)
    {
        if (actorBuffAttribute == ActorBuffAttribute.InstantEffect)
        {
            validateActorBuffAttributeInfo = "本Buff不支持【瞬时效果】标签";
            return false;
        }

        return true;
    }

    protected override void ChildClone(ActorBuff newBuff)
    {
        base.ChildClone(newBuff);
        ActorBuff_ActorPropertyMultiplyModifier buff = ((ActorBuff_ActorPropertyMultiplyModifier) newBuff);
        buff.Percent = Percent;
        buff.LinearDecayInDuration = LinearDecayInDuration;
        buff.PropertyType = PropertyType;
        buff.MultiplyModifier = new ActorProperty.MultiplyModifier {Percent = Percent};
    }
}

[Serializable]
public class ActorBuff_ActorPropertyPlusModifier : ActorBuff
{
    protected override string ActorBuffDisplayName => "角色属性加法修正Buff, 必须是延时buff, buff结束后消除该修正值";

    public ActorBuff_ActorPropertyPlusModifier()
    {
        PlusModifier = new ActorProperty.PlusModifier {Delta = Delta};
    }

    [LabelText("角色属性类型")]
    public ActorProperty.PropertyType PropertyType;

    [LabelText("变化量")]
    public int Delta;

    [LabelText("在持续时间内线性衰减至0")]
    [HideIf("IsPermanent")]
    public bool LinearDecayInDuration;

    internal ActorProperty.PlusModifier PlusModifier;

    public override void OnAdded(Actor actor)
    {
        base.OnAdded(actor);
        if (actor.ActorStatPropSet.PropertyDict.TryGetValue(PropertyType, out ActorProperty property))
        {
            property.AddModifier(PlusModifier);
        }
    }

    public override void OnFixedUpdate(Actor actor, float passedTime, float remainTime)
    {
        base.OnFixedUpdate(actor, passedTime, remainTime);
        if (!IsPermanent && LinearDecayInDuration)
        {
            PlusModifier.Delta = Mathf.RoundToInt(Delta * remainTime / Duration);
        }
    }

    public override void OnRemoved(Actor actor)
    {
        base.OnRemoved(actor);
        if (actor.ActorStatPropSet.PropertyDict.TryGetValue(PropertyType, out ActorProperty property))
        {
            if (!property.RemoveModifier(PlusModifier))
            {
                Debug.LogError($"Failed to RemovePlusModifier: {PropertyType} from {actor.name}");
            }
        }

        PlusModifier = null;
    }

    protected override bool ValidateActorBuffAttribute(ActorBuffAttribute actorBuffAttribute)
    {
        if (actorBuffAttribute == ActorBuffAttribute.InstantEffect)
        {
            validateActorBuffAttributeInfo = "本Buff不支持【瞬时效果】标签";
            return false;
        }

        return true;
    }

    protected override void ChildClone(ActorBuff newBuff)
    {
        base.ChildClone(newBuff);
        ActorBuff_ActorPropertyPlusModifier buff = ((ActorBuff_ActorPropertyPlusModifier) newBuff);
        buff.Delta = Delta;
        buff.PropertyType = PropertyType;
        buff.LinearDecayInDuration = LinearDecayInDuration;
        buff.PlusModifier = new ActorProperty.PlusModifier {Delta = Delta};
    }
}

[Serializable]
public class ActorBuff_InstantDamage : ActorBuff
{
    protected override string ActorBuffDisplayName => "瞬间伤害buff, 必须是【瞬时效果】. buff施加后, 不残留在角色身上, 无移除的概念。但此buff有可能被既有buff免疫或抵消等";

    [LabelText("伤害")]
    public int Damage;

    public override void OnAdded(Actor actor)
    {
        base.OnAdded(actor);
        actor.ActorBattleHelper.Damage(actor, Damage); // 此处施加对象是自己，暂时有点奇怪
    }

    protected override bool ValidateActorBuffAttribute(ActorBuffAttribute actorBuffAttribute)
    {
        if (actorBuffAttribute != ActorBuffAttribute.InstantEffect)
        {
            validateActorBuffAttributeInfo = "本Buff仅支持【瞬时效果】标签";
            return false;
        }

        return true;
    }

    protected override void ChildClone(ActorBuff newBuff)
    {
        base.ChildClone(newBuff);
        ActorBuff_InstantDamage buff = ((ActorBuff_InstantDamage) newBuff);
        buff.Damage = Damage;
    }
}

[Serializable]
public class ActorBuff_InstantHeal : ActorBuff
{
    protected override string ActorBuffDisplayName => "瞬间治疗buff, 必须是【瞬时效果】. buff施加后, 不残留在角色身上, 无移除的概念。但此buff有可能被既有buff免疫或抵消等";

    [LabelText("治疗量")]
    public int Health;

    public override void OnAdded(Actor actor)
    {
        base.OnAdded(actor);
        actor.ActorBattleHelper.Heal(actor, Health); // 此处施加对象是自己，暂时有点奇怪
    }

    protected override bool ValidateActorBuffAttribute(ActorBuffAttribute actorBuffAttribute)
    {
        if (actorBuffAttribute != ActorBuffAttribute.InstantEffect)
        {
            validateActorBuffAttributeInfo = "本Buff仅支持【瞬时效果】标签";
            return false;
        }

        return true;
    }

    protected override void ChildClone(ActorBuff newBuff)
    {
        base.ChildClone(newBuff);
        ActorBuff_InstantHeal buff = ((ActorBuff_InstantHeal) newBuff);
        buff.Health = Health;
    }
}

[Serializable]
public class ActorBuff_ChangeActorStatInstantly : ActorBuff
{
    protected override string ActorBuffDisplayName => "瞬间更改角色异常状态累积值, 必须是【瞬时效果】. buff施加后, 不残留在角色身上, 无移除的概念。但此buff有可能被既有buff免疫或抵消等";

    [LabelText("角色属性类型")]
    [ValidateInput("ValidateStatType", "请选择异常状态累积值")]
    public ActorStat.StatType StatType;

    private bool ValidateStatType(ActorStat.StatType statType)
    {
        if (statType == ActorStat.StatType.Health || statType == ActorStat.StatType.Life)
        {
            return false;
        }

        return true;
    }

    [LabelText("变化量")]
    public int Delta;

    [LabelText("增加比率%")]
    public int Percent;

    public override void OnAdded(Actor actor)
    {
        base.OnAdded(actor);
        float valueBefore = actor.ActorStatPropSet.StatDict[StatType].Value;
        valueBefore += Delta;
        valueBefore *= (100 + Percent) / 100f;
        actor.ActorStatPropSet.StatDict[StatType].Value = Mathf.RoundToInt(valueBefore);
    }

    protected override bool ValidateActorBuffAttribute(ActorBuffAttribute actorBuffAttribute)
    {
        if (actorBuffAttribute != ActorBuffAttribute.InstantEffect)
        {
            validateActorBuffAttributeInfo = "本Buff仅支持【瞬时效果】标签";
            return false;
        }

        return true;
    }

    protected override void ChildClone(ActorBuff newBuff)
    {
        base.ChildClone(newBuff);
        ActorBuff_ChangeActorStatInstantly buff = ((ActorBuff_ChangeActorStatInstantly) newBuff);
        buff.StatType = StatType;
        buff.Delta = Delta;
        buff.Percent = Percent;
    }
}

public enum ActorBuffAttribute
{
    [LabelText("瞬时效果")]
    InstantEffect,

    [LabelText("加速")]
    SpeedUp,

    [LabelText("行动力")]
    ActionPoint,

    [LabelText("减速")]
    SlowDown,

    [LabelText("冰冻")]
    Frozen,

    [LabelText("灼烧")]
    Firing,

    [LabelText("眩晕")]
    Stun,

    [LabelText("眩晕免疫")]
    StunImmune,

    [LabelText("无敌")]
    Invincible,

    [LabelText("隐身")]
    Hiding,

    [LabelText("中毒")]
    Poison,

    [LabelText("最大血量")]
    MaxHealth,
}

public enum ActorBuffAttributeRelationship
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
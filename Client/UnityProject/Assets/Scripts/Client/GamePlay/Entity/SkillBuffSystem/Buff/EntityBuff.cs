using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class EntityBuff : IClone<EntityBuff>
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

    [LabelText("Buff描述")]
    [ShowInInspector]
    [PropertyOrder(-1)]
    protected abstract string Description { get; }

    [LabelText("Buff标签")]
    [ValidateInput("ValidateBuffAttribute", "$validateBuffAttributeInfo")]
    public EntityBuffAttribute EntityBuffAttribute;

    protected string validateBuffAttributeInfo = "";

    protected virtual bool ValidateBuffAttribute(EntityBuffAttribute buffAttribute)
    {
        return true;
    }

    [LabelText("永久Buff")]
    [HideIf("EntityBuffAttribute", EntityBuffAttribute.InstantEffect)]
    public bool IsPermanent;

    [LabelText("Buff持续时间")]
    [HideIf("IsPermanent")]
    [HideIf("EntityBuffAttribute", EntityBuffAttribute.InstantEffect)]
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

    protected EntityBuff()
    {
        GUID = GetGUID();
    }

    public virtual void OnAdded(Entity entity)
    {
    }

    public virtual void OnFixedUpdate(Entity entity, float passedTime, float remainTime)
    {
    }

    public virtual void OnRemoved(Entity entity)
    {
    }

    public EntityBuff Clone()
    {
        Type type = GetType();
        EntityBuff newBuff = (EntityBuff) Activator.CreateInstance(type);
        newBuff.EntityBuffAttribute = EntityBuffAttribute;
        newBuff.IsPermanent = IsPermanent;
        newBuff.Duration = Duration;
        newBuff.BuffFX = BuffFX;
        newBuff.BuffFXScale = BuffFXScale;
        ChildClone(newBuff);
        return newBuff;
    }

    protected virtual void ChildClone(EntityBuff newBuff)
    {
    }

    #region Utils

    private IEnumerable<string> GetAllBoxTypeNames => ConfigManager.GetAllBoxTypeNames();

    private IEnumerable<string> GetAllFXTypeNames => ConfigManager.GetAllFXTypeNames();

    #endregion
}

[Serializable]
public class EntityBuff_AttributeLabel : EntityBuff
{
    protected override string Description => "本Buff仅提供标签作用";
}

[Serializable]
public class EntityBuff_EntityPropertyMultiplyModifier : EntityBuff
{
    protected override string Description => "属性乘法修正Buff, 必须是延时buff, buff结束后消除该修正值";

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
        if (entity.IsRecycled) return;
        if (entity.EntityStatPropSet.PropertyDict.TryGetValue(EntityPropertyType, out EntityProperty property))
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
        if (entity.IsRecycled) return;
        if (entity.EntityStatPropSet.PropertyDict.TryGetValue(EntityPropertyType, out EntityProperty property))
        {
            if (!property.RemoveModifier(MultiplyModifier))
            {
                Debug.Log($"RemoveMultiplyModifier: {EntityPropertyType} failed from {entity.name}");
            }
        }

        MultiplyModifier = null;
    }

    protected override bool ValidateBuffAttribute(EntityBuffAttribute buffAttribute)
    {
        if (buffAttribute == EntityBuffAttribute.InstantEffect)
        {
            validateBuffAttributeInfo = "本Buff不支持【瞬时效果】标签";
            return false;
        }

        return true;
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

[Serializable]
public class EntityBuff_EntityPropertyPlusModifier : EntityBuff
{
    protected override string Description => "属性加法修正Buff, 必须是延时buff, buff结束后消除该修正值";

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

    public override void OnAdded(Entity entity)
    {
        base.OnAdded(entity);
        if (entity.IsRecycled) return;
        if (entity.EntityStatPropSet.PropertyDict.TryGetValue(EntityPropertyType, out EntityProperty property))
        {
            property.AddModifier(PlusModifier);
        }
    }

    public override void OnFixedUpdate(Entity entity, float passedTime, float remainTime)
    {
        base.OnFixedUpdate(entity, passedTime, remainTime);
        if (entity.IsRecycled) return;
        if (!IsPermanent && LinearDecayInDuration)
        {
            PlusModifier.Delta = Mathf.RoundToInt(Delta * remainTime / Duration);
        }
    }

    public override void OnRemoved(Entity entity)
    {
        base.OnRemoved(entity);
        if (entity.IsRecycled) return;
        if (entity.EntityStatPropSet.PropertyDict.TryGetValue(EntityPropertyType, out EntityProperty property))
        {
            if (!property.RemoveModifier(PlusModifier))
            {
                Debug.LogError($"Failed to RemovePlusModifier: {EntityPropertyType} from {entity.name}");
            }
        }

        PlusModifier = null;
    }

    protected override bool ValidateBuffAttribute(EntityBuffAttribute attribute)
    {
        if (attribute == EntityBuffAttribute.InstantEffect)
        {
            validateBuffAttributeInfo = "本Buff不支持【瞬时效果】标签";
            return false;
        }

        return true;
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

[Serializable]
public class EntityBuff_ChangeActorStatInstantly : EntityBuff
{
    protected override string Description => "瞬间更改状态值, 必须是【瞬时效果】. buff施加后, 不残留在Entity身上, 无移除的概念。但此buff有可能被既有buff免疫或抵消等";

    [LabelText("状态值类型")]
    public EntityStatType EntityStatType;

    [LabelText("变化量")]
    public int Delta;

    [LabelText("增加比率%")]
    public int Percent;

    public override void OnAdded(Entity entity)
    {
        base.OnAdded(entity);
        if (entity.IsRecycled) return;
        float valueBefore = entity.EntityStatPropSet.StatDict[EntityStatType].Value;
        valueBefore += Delta;
        valueBefore *= (100 + Percent) / 100f;
        entity.EntityStatPropSet.StatDict[EntityStatType].Value = Mathf.RoundToInt(valueBefore);
    }

    protected override bool ValidateBuffAttribute(EntityBuffAttribute attribute)
    {
        if (attribute != EntityBuffAttribute.InstantEffect)
        {
            validateBuffAttributeInfo = "本Buff仅支持【瞬时效果】标签";
            return false;
        }

        return true;
    }

    protected override void ChildClone(EntityBuff newBuff)
    {
        base.ChildClone(newBuff);
        EntityBuff_ChangeActorStatInstantly buff = ((EntityBuff_ChangeActorStatInstantly) newBuff);
        buff.EntityStatType = EntityStatType;
        buff.Delta = Delta;
        buff.Percent = Percent;
    }
}
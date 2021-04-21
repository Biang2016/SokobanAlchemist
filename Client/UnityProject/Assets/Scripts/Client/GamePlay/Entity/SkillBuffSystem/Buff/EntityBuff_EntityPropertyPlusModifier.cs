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

    [LabelText("属性分类")]
    [EnumToggleButtons]
    public PropertyCategory PropertyCategory;

    [LabelText("属性类型")]
    public EntityPropertyType EntityPropertyType;

    [LabelText("技能GUID")]
    [ShowIf("PropertyCategory", PropertyCategory.EntitySkillPropertyType)]
    public string SkillGUID = "";

    [LabelText("属性类型")]
    [ShowIf("PropertyCategory", PropertyCategory.EntitySkillPropertyType)]
    public EntitySkillPropertyType EntitySkillPropertyType;

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
        switch (PropertyCategory)
        {
            case PropertyCategory.EntityPropertyType:
            {
                if (entity.EntityStatPropSet.PropertyDict.TryGetValue(EntityPropertyType, out EntityProperty property))
                {
                    property.AddModifier(PlusModifier);
                }

                break;
            }
            case PropertyCategory.EntitySkillPropertyType:
            {
                if (entity.EntityActiveSkillGUIDDict.TryGetValue(SkillGUID, out EntityActiveSkill eas))
                {
                    eas.SkillsPropertyCollection.PropertyDict[EntitySkillPropertyType].AddModifier(PlusModifier);
                }

                break;
            }
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

        switch (PropertyCategory)
        {
            case PropertyCategory.EntityPropertyType:
            {
                if (entity.EntityStatPropSet.PropertyDict.TryGetValue(EntityPropertyType, out EntityProperty property))
                {
                    if (!property.RemoveModifier(PlusModifier))
                    {
                        Debug.LogError($"Failed to RemovePlusModifier: {EntityPropertyType} from {entity.name}");
                    }
                }

                break;
            }
            case PropertyCategory.EntitySkillPropertyType:
            {
                if (entity.EntityActiveSkillGUIDDict.TryGetValue(SkillGUID, out EntityActiveSkill eas))
                {
                    if (!eas.SkillsPropertyCollection.PropertyDict[EntitySkillPropertyType].RemoveModifier(PlusModifier))
                    {
                        Debug.LogError($"Failed to RemoveSkillPlusModifier: {EntityPropertyType} from {entity.name} {eas.SkillAlias}");
                    }
                }

                break;
            }
        }

        PlusModifier = null;
    }

    protected override void ChildClone(EntityBuff newBuff)
    {
        base.ChildClone(newBuff);
        EntityBuff_EntityPropertyPlusModifier buff = ((EntityBuff_EntityPropertyPlusModifier) newBuff);
        buff.PropertyCategory = PropertyCategory;
        buff.EntityPropertyType = EntityPropertyType;
        buff.SkillGUID = SkillGUID;
        buff.EntitySkillPropertyType = EntitySkillPropertyType;
        buff.Delta = Delta;
        buff.LinearDecayInDuration = LinearDecayInDuration;
        buff.PlusModifier = new Property.PlusModifier {Delta = Delta};
    }

    public override void CopyDataFrom(EntityBuff srcData)
    {
        base.CopyDataFrom(srcData);
        EntityBuff_EntityPropertyPlusModifier srcBuff = ((EntityBuff_EntityPropertyPlusModifier) srcData);
        PropertyCategory = srcBuff.PropertyCategory;
        EntityPropertyType = srcBuff.EntityPropertyType;
        SkillGUID = srcBuff.SkillGUID;
        EntitySkillPropertyType = srcBuff.EntitySkillPropertyType;
        Delta = srcBuff.Delta;
        LinearDecayInDuration = srcBuff.LinearDecayInDuration;
        PlusModifier = new Property.PlusModifier {Delta = srcBuff.Delta};
    }
}
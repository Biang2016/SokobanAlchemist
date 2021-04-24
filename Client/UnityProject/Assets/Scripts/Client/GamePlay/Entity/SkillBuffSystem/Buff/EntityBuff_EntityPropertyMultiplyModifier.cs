using Sirenix.OdinInspector;
using System;
using UnityEngine;

[Serializable]
public class EntityBuff_EntityPropertyMultiplyModifier : EntityBuff
{
    protected override string Description => "属性乘法修正Buff, buff结束后消除该修正值";

    public EntityBuff_EntityPropertyMultiplyModifier()
    {
        MultiplyModifier = new Property.MultiplyModifier {Percent = Percent};
    }

    [LabelText("属性分类")]
    [EnumToggleButtons]
    public PropertyCategory PropertyCategory;

    [LabelText("属性类型")]
    [ShowIf("PropertyCategory", PropertyCategory.EntityPropertyType)]
    public EntityPropertyType EntityPropertyType;

    [LabelText("技能")]
    [ShowIf("PropertyCategory", PropertyCategory.EntitySkillPropertyType)]
    [OnValueChanged("RefreshSkillGUID")]
    public EntitySkillSO EntitySkillSO;

    public void RefreshSkillGUID()
    {
        if (EntitySkillSO != null)
        {
            SkillGUID = EntitySkillSO.EntitySkill.SkillGUID;
        }
        else
        {
            SkillGUID = "";
        }
    }

    [ReadOnly]
    [LabelText("技能GUID")]
    [ShowIf("PropertyCategory", PropertyCategory.EntitySkillPropertyType)]
    public string SkillGUID = "";

    [LabelText("属性类型")]
    [ShowIf("PropertyCategory", PropertyCategory.EntitySkillPropertyType)]
    public EntitySkillPropertyType EntitySkillPropertyType;

    [LabelText("增加比率%")]
    public int Percent;

    [LabelText("在持续时间内线性衰减至0")]
    [HideIf("IsPermanent")]
    public bool LinearDecayInDuration;

    internal Property.MultiplyModifier MultiplyModifier;

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
                    property.AddModifier(MultiplyModifier);
                }

                break;
            }
            case PropertyCategory.EntitySkillPropertyType:
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                if (ConfigManager.GetRawEntitySkill(SkillGUID) == null)
                {
                    Debug.LogError($"EntitySkillSO Link invalid: {SkillGUID}");
                }
#endif

                if (entity.EntityActiveSkillGUIDDict.TryGetValue(SkillGUID, out EntityActiveSkill eas))
                {
                    eas.SkillsPropertyCollection.PropertyDict[EntitySkillPropertyType].AddModifier(MultiplyModifier);
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
            MultiplyModifier.Percent = Mathf.RoundToInt(Percent * remainTime / Duration);
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
                    if (!property.RemoveModifier(MultiplyModifier))
                    {
                        Debug.Log($"RemoveMultiplyModifier: {EntityPropertyType} failed from {entity.name}");
                    }
                }

                break;
            }
            case PropertyCategory.EntitySkillPropertyType:
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                if (ConfigManager.GetRawEntitySkill(SkillGUID) == null)
                {
                    Debug.LogError($"EntitySkillSO Link invalid: {SkillGUID}");
                }
#endif
                if (entity.EntityActiveSkillGUIDDict.TryGetValue(SkillGUID, out EntityActiveSkill eas))
                {
                    if (!eas.SkillsPropertyCollection.PropertyDict[EntitySkillPropertyType].RemoveModifier(MultiplyModifier))
                    {
                        Debug.Log($"RemoveSkillMultiplyModifier: {EntityPropertyType} failed from {entity.name} {eas.SkillAlias}");
                    }
                }

                break;
            }
        }

        MultiplyModifier = null;
    }

    protected override void ChildClone(EntityBuff newBuff)
    {
        base.ChildClone(newBuff);
        EntityBuff_EntityPropertyMultiplyModifier buff = ((EntityBuff_EntityPropertyMultiplyModifier) newBuff);
        buff.PropertyCategory = PropertyCategory;
        buff.EntityPropertyType = EntityPropertyType;
        buff.SkillGUID = SkillGUID;
        buff.EntitySkillPropertyType = EntitySkillPropertyType;
        buff.Percent = Percent;
        buff.LinearDecayInDuration = LinearDecayInDuration;
        buff.MultiplyModifier = new Property.MultiplyModifier {Percent = Percent};
    }

    public override void CopyDataFrom(EntityBuff srcData)
    {
        base.CopyDataFrom(srcData);
        EntityBuff_EntityPropertyMultiplyModifier srcBuff = ((EntityBuff_EntityPropertyMultiplyModifier) srcData);
        PropertyCategory = srcBuff.PropertyCategory;
        EntityPropertyType = srcBuff.EntityPropertyType;
        SkillGUID = srcBuff.SkillGUID;
        EntitySkillPropertyType = srcBuff.EntitySkillPropertyType;
        Percent = srcBuff.Percent;
        LinearDecayInDuration = srcBuff.LinearDecayInDuration;
        MultiplyModifier = new Property.MultiplyModifier {Percent = srcBuff.Percent};
    }
}
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class SkillPropertyCollection
{
    [HideInInspector]
    public Dictionary<EntitySkillPropertyType, EntityProperty> PropertyDict = new Dictionary<EntitySkillPropertyType, EntityProperty>();

    [LabelText("@\"施法正方形范围边长\t\"+CastingRadius")]
    public EntityProperty CastingRadius = new EntityProperty(EntitySkillPropertyType.CastingRadius);

    [LabelText("@\"冷却时间/ms\t\"+Cooldown")]
    public EntityProperty Cooldown = new EntityProperty(EntitySkillPropertyType.Cooldown);

    [LabelText("@\"前摇/ms\t\"+WingUp")]
    public EntityProperty WingUp = new EntityProperty(EntitySkillPropertyType.WingUp);

    [LabelText("@\"施法时间/ms\t\"+CastDuration")]
    public EntityProperty CastDuration = new EntityProperty(EntitySkillPropertyType.CastDuration);

    [LabelText("@\"后摇/ms\t\"+Recovery")]
    public EntityProperty Recovery = new EntityProperty(EntitySkillPropertyType.Recovery);

    [LabelText("@\"震屏伤害当量\t\"+CameraShakeEquivalentDamage")]
    public EntityProperty CameraShakeEquivalentDamage = new EntityProperty(EntitySkillPropertyType.CameraShakeEquivalentDamage);

    [LabelText("@\"消耗行动力点数\t\"+ConsumeActionPoint")]
    public EntityProperty ConsumeActionPoint = new EntityProperty(EntitySkillPropertyType.ConsumeActionPoint);

    [LabelText("@\"消耗火元素碎片量\t\"+ConsumeFireElementFragment")]
    public EntityProperty ConsumeFireElementFragment = new EntityProperty(EntitySkillPropertyType.ConsumeFireElementFragment);

    [LabelText("@\"消耗冰元素碎片量\t\"+ConsumeIceElementFragment")]
    public EntityProperty ConsumeIceElementFragment = new EntityProperty(EntitySkillPropertyType.ConsumeIceElementFragment);

    [LabelText("@\"消耗电元素碎片量\t\"+ConsumeLightningElementFragment")]
    public EntityProperty ConsumeLightningElementFragment = new EntityProperty(EntitySkillPropertyType.ConsumeLightningElementFragment);

    public void Init()
    {
        if (PropertyDict.Count == 0)
        {
            PropertyDict.Add(EntitySkillPropertyType.CastingRadius, CastingRadius);
            PropertyDict.Add(EntitySkillPropertyType.Cooldown, Cooldown);
            PropertyDict.Add(EntitySkillPropertyType.WingUp, WingUp);
            PropertyDict.Add(EntitySkillPropertyType.CastDuration, CastDuration);
            PropertyDict.Add(EntitySkillPropertyType.Recovery, Recovery);
            PropertyDict.Add(EntitySkillPropertyType.CameraShakeEquivalentDamage, CameraShakeEquivalentDamage);
            PropertyDict.Add(EntitySkillPropertyType.ConsumeActionPoint, ConsumeActionPoint);
            PropertyDict.Add(EntitySkillPropertyType.ConsumeFireElementFragment, ConsumeFireElementFragment);
            PropertyDict.Add(EntitySkillPropertyType.ConsumeIceElementFragment, ConsumeIceElementFragment);
            PropertyDict.Add(EntitySkillPropertyType.ConsumeLightningElementFragment, ConsumeLightningElementFragment);
        }

        foreach (KeyValuePair<EntitySkillPropertyType, EntityProperty> kv in PropertyDict)
        {
            kv.Value.Initialize();
        }
    }

    public void OnRecycled()
    {
        foreach (KeyValuePair<EntitySkillPropertyType, EntityProperty> kv in PropertyDict)
        {
            kv.Value.OnRecycled();
        }
    }

    public void ApplyDataTo(SkillPropertyCollection target)
    {
        CastingRadius.ApplyDataTo(target.CastingRadius);
        Cooldown.ApplyDataTo(target.Cooldown);
        WingUp.ApplyDataTo(target.WingUp);
        CastDuration.ApplyDataTo(target.CastDuration);
        Recovery.ApplyDataTo(target.Recovery);
        CameraShakeEquivalentDamage.ApplyDataTo(target.CameraShakeEquivalentDamage);
        ConsumeActionPoint.ApplyDataTo(target.ConsumeActionPoint);
        ConsumeFireElementFragment.ApplyDataTo(target.ConsumeFireElementFragment);
        ConsumeIceElementFragment.ApplyDataTo(target.ConsumeIceElementFragment);
        ConsumeLightningElementFragment.ApplyDataTo(target.ConsumeLightningElementFragment);
    }
}
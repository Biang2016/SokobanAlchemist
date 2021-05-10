using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public class EntitySkillAction_LightningShield : EntitySkillAction, EntitySkillAction.IBuffAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "电盾";

    public void ExecuteOnBuff(EntityBuff buff)
    {
        if (buff is EntityBuff_ChangeEntityStatInstantly damageBuff)
        {
            int damage = -damageBuff.Delta; // 伤害Delta默认是负数，这里damage取正
            int lightningElementLeft = Entity.EntityStatPropSet.LightningElementFragment.Value;
            bool playSound = false;
            if (damage * LightningElementConsumptionPerDamage <= lightningElementLeft)
            {
                Entity.EntityStatPropSet.LightningElementFragment.SetValue(lightningElementLeft - damage * LightningElementConsumptionPerDamage);
                damageBuff.Delta = 0;
                playSound = true;
            }
            else
            {
                int decreaseDamage = lightningElementLeft / LightningElementConsumptionPerDamage;
                playSound = decreaseDamage > 0;
                Entity.EntityStatPropSet.LightningElementFragment.SetValue(lightningElementLeft - decreaseDamage * LightningElementConsumptionPerDamage);
                damageBuff.Delta += decreaseDamage;
            }

            if (playSound) Entity.EntityWwiseHelper.OnLightningShieldBlockDamage?.Post(Entity.gameObject);
        }
    }

    [LabelText("每点伤害消耗多少电能")]
    public int LightningElementConsumptionPerDamage;

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_LightningShield action = ((EntitySkillAction_LightningShield) newAction);
        action.LightningElementConsumptionPerDamage = LightningElementConsumptionPerDamage;
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_LightningShield action = ((EntitySkillAction_LightningShield) srcData);
        LightningElementConsumptionPerDamage = action.LightningElementConsumptionPerDamage;
    }
}
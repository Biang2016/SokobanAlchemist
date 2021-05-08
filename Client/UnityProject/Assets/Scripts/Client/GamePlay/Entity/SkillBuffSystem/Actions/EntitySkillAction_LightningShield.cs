using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

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
            int damage = damageBuff.Delta;
            int lightningElementLeft = Entity.EntityStatPropSet.LightningElementFragment.Value;
            if (damage * LightningElementConsumptionPerDamage <= lightningElementLeft )
            {
                Entity.EntityStatPropSet.LightningElementFragment.SetValue(lightningElementLeft - damage * LightningElementConsumptionPerDamage);
                damageBuff.EntityStatType = EntityStatType.LightningElementFragment;
            }
            else
            {
                int decreaseDamage = lightningElementLeft / LightningElementConsumptionPerDamage;
                Entity.EntityStatPropSet.LightningElementFragment.SetValue(lightningElementLeft - decreaseDamage * LightningElementConsumptionPerDamage);
                damageBuff.Delta -= decreaseDamage;
            }
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
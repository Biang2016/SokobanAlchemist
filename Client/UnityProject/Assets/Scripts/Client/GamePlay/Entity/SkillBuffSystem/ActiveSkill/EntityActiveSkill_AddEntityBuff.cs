using System;
using System.Collections;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntityActiveSkill_AddEntityBuff : EntityActiveSkill_AreaCast
{
    protected override string Description => "给区域内Entity施加Buff";

    [BoxGroup("Buff")]
    [LabelText("Buff列表")]
    [SerializeReference]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<EntityBuff> RawEntityBuffs = new List<EntityBuff>(); // 干数据，禁修改

    public override void OnInit()
    {
        base.OnInit();
    }

    public override void OnUnInit()
    {
        base.OnUnInit();
    }

    protected override IEnumerator Cast(float castDuration)
    {
        foreach (Entity entity in GetTargetEntities())
        {
            entity.EntityBuffHelper.Damage(GetValue(EntitySkillPropertyType.Damage), EntityBuffAttribute.AttackDamage);

            entity.EntityStatPropSet.FiringValue.SetValue(entity.EntityStatPropSet.FiringValue.Value + GetValue(EntitySkillPropertyType.Attach_FiringValue), "AddEntityBuffDamageCast");
            entity.EntityStatPropSet.FrozenValue.SetValue(entity.EntityStatPropSet.FrozenValue.Value + GetValue(EntitySkillPropertyType.Attach_FrozenValue), "AddEntityBuffDamageCast");
            foreach (EntityBuff buff in RawEntityBuffs)
            {
                entity.EntityBuffHelper.AddBuff(buff.Clone());
            }
        }

        yield return base.Cast(castDuration);
    }

    protected override void ChildClone(EntityActiveSkill cloneData)
    {
        base.ChildClone(cloneData);
        EntityActiveSkill_AddEntityBuff newAAS = (EntityActiveSkill_AddEntityBuff) cloneData;
        newAAS.RawEntityBuffs = RawEntityBuffs.Clone();
    }

    public override void CopyDataFrom(EntityActiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        EntityActiveSkill_AddEntityBuff srcAAS = (EntityActiveSkill_AddEntityBuff) srcData;
        RawEntityBuffs = srcAAS.RawEntityBuffs.Clone();
    }
}
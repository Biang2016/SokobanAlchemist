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

    protected override IEnumerator Cast(TargetEntityType targetEntityType, float castDuration)
    {
        foreach (Entity entity in GetTargetEntities())
        {
            foreach (EntityBuff buff in RawEntityBuffs)
            {
                entity.EntityBuffHelper.AddBuff(buff.Clone());
            }
        }

        yield return base.Cast(targetEntityType, castDuration);
    }

    protected override void ChildClone(EntitySkill cloneData)
    {
        base.ChildClone(cloneData);
        EntityActiveSkill_AddEntityBuff newAAS = (EntityActiveSkill_AddEntityBuff) cloneData;
        newAAS.RawEntityBuffs = RawEntityBuffs.Clone<EntityBuff, EntityBuff>();
    }

    public override void CopyDataFrom(EntitySkill srcData)
    {
        base.CopyDataFrom(srcData);
        EntityActiveSkill_AddEntityBuff srcAAS = (EntityActiveSkill_AddEntityBuff) srcData;
        if (RawEntityBuffs.Count != srcAAS.RawEntityBuffs.Count)
        {
            Debug.LogError("EntityActiveSkill_AddEntityBuff CopyDataFrom RawEntityBuffs数量不一致");
        }
        else
        {
            for (int i = 0; i < RawEntityBuffs.Count; i++)
            {
                RawEntityBuffs[i].CopyDataFrom(srcAAS.RawEntityBuffs[i]);
            }
        }
    }
}
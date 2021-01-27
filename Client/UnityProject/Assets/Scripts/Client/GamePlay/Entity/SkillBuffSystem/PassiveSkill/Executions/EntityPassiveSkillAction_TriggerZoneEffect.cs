using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntityPassiveSkillAction_TriggerZoneEffect : EntityPassiveSkillAction, EntityPassiveSkillAction.ITriggerAction
{
    protected override string Description => "箱子范围触发效果";

    [LabelText("生效于相对阵营")]
    public RelativeCamp EffectiveOnRelativeCamp;

    [SerializeReference]
    [LabelText("进入")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<EntityBuff> RawEntityBuffs_Enter = new List<EntityBuff>();

    [SerializeReference]
    [LabelText("停留")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<EntityBuff> RawEntityBuffs_Stay = new List<EntityBuff>();

    [LabelText("停留时重复生效间隔时间/s")]
    public float EffectInterval = 1f;

    [SerializeReference]
    [LabelText("离开")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<EntityBuff> RawEntityBuffs_Exit = new List<EntityBuff>();

    public void OnTriggerEnter(Collider collider)
    {
        if (LayerManager.Instance.CheckLayerValid(Entity.Camp, EffectiveOnRelativeCamp, collider.gameObject.layer))
        {
            Entity target = collider.GetComponentInParent<Entity>();
            if (target != null)
            {
                if (!Entity.EntityTriggerZoneHelper.ActorStayTimeDict.ContainsKey(target.GUID))
                {
                    Entity.EntityTriggerZoneHelper.ActorStayTimeDict.Add(target.GUID, 0);
                    foreach (EntityBuff buff in RawEntityBuffs_Enter)
                    {
                        target.EntityBuffHelper.AddBuff(buff.Clone());
                    }
                }
            }
        }
    }

    public void OnTriggerStay(Collider collider)
    {
        if (LayerManager.Instance.CheckLayerValid(Entity.Camp, EffectiveOnRelativeCamp, collider.gameObject.layer))
        {
            Entity target = collider.GetComponentInParent<Entity>();
            if (target != null)
            {
                if (Entity.EntityTriggerZoneHelper.ActorStayTimeDict.TryGetValue(target.GUID, out float duration))
                {
                    if (duration > EffectInterval)
                    {
                        foreach (EntityBuff buff in RawEntityBuffs_Stay)
                        {
                            target.EntityBuffHelper.AddBuff(buff.Clone());
                        }

                        Entity.EntityTriggerZoneHelper.ActorStayTimeDict[target.GUID] = 0;
                    }
                    else
                    {
                        Entity.EntityTriggerZoneHelper.ActorStayTimeDict[target.GUID] += Time.fixedDeltaTime;
                    }
                }
            }
        }
    }

    public void OnTriggerExit(Collider collider)
    {
        if (LayerManager.Instance.CheckLayerValid(Entity.Camp, EffectiveOnRelativeCamp, collider.gameObject.layer))
        {
            Entity target = collider.GetComponentInParent<Entity>();
            if (target != null)
            {
                if (Entity.EntityTriggerZoneHelper.ActorStayTimeDict.ContainsKey(target.GUID))
                {
                    Entity.EntityTriggerZoneHelper.ActorStayTimeDict.Remove(target.GUID);
                    foreach (EntityBuff buff in RawEntityBuffs_Exit)
                    {
                        target.EntityBuffHelper.AddBuff(buff.Clone());
                    }
                }
            }
        }
    }

    protected override void ChildClone(EntityPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        EntityPassiveSkillAction_TriggerZoneEffect action = ((EntityPassiveSkillAction_TriggerZoneEffect) newAction);
        action.EffectiveOnRelativeCamp = EffectiveOnRelativeCamp;
        action.RawEntityBuffs_Enter = RawEntityBuffs_Enter.Clone();
        action.RawEntityBuffs_Stay = RawEntityBuffs_Stay.Clone();
        action.EffectInterval = EffectInterval;
        action.RawEntityBuffs_Exit = RawEntityBuffs_Exit.Clone();
    }

    public override void CopyDataFrom(EntityPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntityPassiveSkillAction_TriggerZoneEffect action = ((EntityPassiveSkillAction_TriggerZoneEffect) srcData);
        EffectiveOnRelativeCamp = action.EffectiveOnRelativeCamp;
        RawEntityBuffs_Enter = action.RawEntityBuffs_Enter.Clone();
        RawEntityBuffs_Stay = action.RawEntityBuffs_Stay.Clone();
        EffectInterval = action.EffectInterval;
        RawEntityBuffs_Exit = action.RawEntityBuffs_Exit.Clone();
    }
}
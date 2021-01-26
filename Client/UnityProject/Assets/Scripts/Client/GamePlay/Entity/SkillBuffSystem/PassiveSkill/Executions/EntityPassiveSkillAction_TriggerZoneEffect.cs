using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntityPassiveSkillAction_TriggerZoneEffect : EntityPassiveSkillAction, EntityPassiveSkillAction.ITriggerEnterAction, EntityPassiveSkillAction.ITriggerStayAction, EntityPassiveSkillAction.ITriggerExitAction
{
    protected override string Description => "箱子范围触发效果";

    [SerializeReference]
    [LabelText("作用效果")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<EntityBuff> EntityBuffs = new List<EntityBuff>();

    [LabelText("间隔时间/s")]
    public float EffectInterval = 1f;

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Player || collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Enemy)
        {
            Entity target = collider.GetComponentInParent<Entity>();
            if (target != null && target.IsOpponentOrNeutralCampOf(Entity))
            {
                if (!Entity.EntityTriggerZoneHelper.ActorStayTimeDict.ContainsKey(target.GUID))
                {
                    foreach (EntityBuff buff in EntityBuffs)
                    {
                        target.EntityBuffHelper.AddBuff(buff.Clone());
                    }

                    Entity.EntityTriggerZoneHelper.ActorStayTimeDict.Add(target.GUID, 0);
                }
            }
        }
    }

    public void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Player || collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Enemy)
        {
            Entity target = collider.GetComponentInParent<Entity>();
            if (target != null && target.IsOpponentOrNeutralCampOf(Entity))
            {
                if (Entity.EntityTriggerZoneHelper.ActorStayTimeDict.TryGetValue(target.GUID, out float duration))
                {
                    if (duration > EffectInterval)
                    {
                        foreach (EntityBuff buff in EntityBuffs)
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
        if (collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Player || collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Enemy)
        {
            Entity target = collider.GetComponentInParent<Entity>();
            if (target != null && target.IsOpponentOrNeutralCampOf(Entity))
            {
                if (Entity.EntityTriggerZoneHelper.ActorStayTimeDict.ContainsKey(target.GUID))
                {
                    Entity.EntityTriggerZoneHelper.ActorStayTimeDict.Remove(target.GUID);
                }
            }
        }
    }

    protected override void ChildClone(EntityPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        EntityPassiveSkillAction_TriggerZoneEffect action = ((EntityPassiveSkillAction_TriggerZoneEffect) newAction);
        action.EntityBuffs = EntityBuffs.Clone();
        action.EffectInterval = EffectInterval;
    }

    public override void CopyDataFrom(EntityPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntityPassiveSkillAction_TriggerZoneEffect action = ((EntityPassiveSkillAction_TriggerZoneEffect) srcData);
        EntityBuffs = action.EntityBuffs.Clone();
        EffectInterval = action.EffectInterval;
    }
}
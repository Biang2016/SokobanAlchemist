using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class BoxPassiveSkillAction_TriggerZoneEffect : BoxPassiveSkillAction, BoxPassiveSkillAction.ITriggerEnterAction, BoxPassiveSkillAction.ITriggerStayAction, BoxPassiveSkillAction.ITriggerExitAction
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
            Actor actor = collider.GetComponentInParent<Actor>();
            if (actor != null && actor.IsEnemy)
            {
                if (!Box.BoxTriggerZoneHelper.ActorStayTimeDict.ContainsKey(actor.GUID))
                {
                    foreach (EntityBuff buff in EntityBuffs)
                    {
                        actor.ActorBuffHelper.AddBuff(buff.Clone());
                    }

                    Box.BoxTriggerZoneHelper.ActorStayTimeDict.Add(actor.GUID, 0);
                }
            }
        }
    }

    public void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Player || collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Enemy)
        {
            Actor actor = collider.GetComponentInParent<Actor>();
            if (actor != null && actor.IsEnemy)
            {
                if (Box.BoxTriggerZoneHelper.ActorStayTimeDict.TryGetValue(actor.GUID, out float duration))
                {
                    if (duration > EffectInterval)
                    {
                        foreach (EntityBuff buff in EntityBuffs)
                        {
                            actor.ActorBuffHelper.AddBuff(buff.Clone());
                        }

                        Box.BoxTriggerZoneHelper.ActorStayTimeDict[actor.GUID] = 0;
                    }
                    else
                    {
                        Box.BoxTriggerZoneHelper.ActorStayTimeDict[actor.GUID] += Time.fixedDeltaTime;
                    }
                }
            }
        }
    }

    public void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Player || collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Enemy)
        {
            Actor actor = collider.GetComponentInParent<Actor>();
            if (actor != null && actor.IsEnemy)
            {
                if (Box.BoxTriggerZoneHelper.ActorStayTimeDict.ContainsKey(actor.GUID))
                {
                    Box.BoxTriggerZoneHelper.ActorStayTimeDict.Remove(actor.GUID);
                }
            }
        }
    }

    protected override void ChildClone(BoxPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        BoxPassiveSkillAction_TriggerZoneEffect action = ((BoxPassiveSkillAction_TriggerZoneEffect) newAction);
        action.EntityBuffs = EntityBuffs.Clone();
        action.EffectInterval = EffectInterval;
    }

    public override void CopyDataFrom(BoxPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        BoxPassiveSkillAction_TriggerZoneEffect action = ((BoxPassiveSkillAction_TriggerZoneEffect) srcData);
        EntityBuffs = action.EntityBuffs.Clone();
        EffectInterval = action.EffectInterval;
    }
}
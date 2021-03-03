﻿using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntityPassiveSkillAction_TriggerZoneAction : EntityPassiveSkillAction, EntityPassiveSkillAction.ITriggerAction
{
    public Dictionary<uint, float> ActorStayTimeDict = new Dictionary<uint, float>();

    public override void OnRecycled()
    {
        ActorStayTimeDict.Clear();
    }

    protected override string Description => "箱子范围触发行为";

    [LabelText("生效于相对阵营")]
    public RelativeCamp EffectiveOnRelativeCamp;

    [SerializeReference]
    [LabelText("进入触发事件")]
    public List<IPureAction> EntityActions_Enter = new List<IPureAction>();

    [SerializeReference]
    [LabelText("停留触发事件")]
    public List<IPureAction> EntityActions_Stay = new List<IPureAction>();

    [LabelText("停留时重复触发间隔时间/s")]
    public float ActionInterval = 1f;

    [SerializeReference]
    [LabelText("离开触发事件")]
    public List<IPureAction> EntityActions_Exit = new List<IPureAction>();

    public void OnTriggerEnter(Collider collider)
    {
        if (LayerManager.Instance.CheckLayerValid(Entity.Camp, EffectiveOnRelativeCamp, collider.gameObject.layer))
        {
            Entity target = collider.GetComponentInParent<Entity>();
            if (target.IsNotNullAndAlive())
            {
                if (!ActorStayTimeDict.ContainsKey(target.GUID))
                {
                    ActorStayTimeDict.Add(target.GUID, 0);
                    foreach (IPureAction action in EntityActions_Enter)
                    {
                        action.Execute();
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
            if (target.IsNotNullAndAlive())
            {
                if (ActorStayTimeDict.TryGetValue(target.GUID, out float duration))
                {
                    if (duration > ActionInterval)
                    {
                        foreach (IPureAction action in EntityActions_Stay)
                        {
                            action.Execute();
                        }

                        ActorStayTimeDict[target.GUID] = 0;
                    }
                    else
                    {
                        ActorStayTimeDict[target.GUID] += Time.fixedDeltaTime;
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
            if (target.IsNotNullAndAlive())
            {
                if (ActorStayTimeDict.ContainsKey(target.GUID))
                {
                    ActorStayTimeDict.Remove(target.GUID);
                    foreach (IPureAction action in EntityActions_Exit)
                    {
                        action.Execute();
                    }
                }
            }
        }
    }

    protected override void ChildClone(EntityPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        EntityPassiveSkillAction_TriggerZoneAction action = ((EntityPassiveSkillAction_TriggerZoneAction) newAction);
        action.EffectiveOnRelativeCamp = EffectiveOnRelativeCamp;
        action.EntityActions_Enter = EntityActions_Enter.Clone();
        action.EntityActions_Stay = EntityActions_Stay.Clone();
        action.ActionInterval = ActionInterval;
        action.EntityActions_Exit = EntityActions_Exit.Clone();
    }

    public override void CopyDataFrom(EntityPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntityPassiveSkillAction_TriggerZoneAction action = ((EntityPassiveSkillAction_TriggerZoneAction) srcData);
        EffectiveOnRelativeCamp = action.EffectiveOnRelativeCamp;
        EntityActions_Enter = action.EntityActions_Enter.Clone();
        EntityActions_Stay = action.EntityActions_Stay.Clone();
        ActionInterval = action.ActionInterval;
        EntityActions_Exit = action.EntityActions_Exit.Clone();
    }
}
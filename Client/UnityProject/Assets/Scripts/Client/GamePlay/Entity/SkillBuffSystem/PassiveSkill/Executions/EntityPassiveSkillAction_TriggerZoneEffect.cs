using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntityPassiveSkillAction_TriggerZoneEffect : EntityPassiveSkillAction, EntityPassiveSkillAction.ITriggerAction
{
    public Dictionary<uint, float> ActorStayTimeDict = new Dictionary<uint, float>();

    public override void OnRecycled()
    {
        ActorStayTimeDict.Clear();
        EntityBuffs_Enter.Clear();
        EntityBuffs_Stay.Clear();
    }

    protected override string Description => "箱子范围触发效果";

    [LabelText("生效于相对阵营")]
    public RelativeCamp EffectiveOnRelativeCamp;

    [SerializeReference]
    [LabelText("进入施加Buff")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<EntityBuff> RawEntityBuffs_Enter = new List<EntityBuff>();

    [LabelText("离开时移除进入buff")]
    public bool RemoveEnterBuffWhenExit;

    private Dictionary<uint, List<EntityBuff>> EntityBuffs_Enter = new Dictionary<uint, List<EntityBuff>>();

    [SerializeReference]
    [LabelText("停留施加Buff")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<EntityBuff> RawEntityBuffs_Stay = new List<EntityBuff>();

    [LabelText("离开时移除停留buff")]
    public bool RemoveStayBuffWhenExit;

    private Dictionary<uint, List<EntityBuff>> EntityBuffs_Stay = new Dictionary<uint, List<EntityBuff>>();

    [LabelText("停留时重复生效间隔时间/s")]
    public float EffectInterval = 1f;

    [SerializeReference]
    [LabelText("离开施加Buff")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<EntityBuff> RawEntityBuffs_Exit = new List<EntityBuff>();

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
                    foreach (EntityBuff buff in RawEntityBuffs_Enter)
                    {
                        EntityBuff newBuff = buff.Clone();
                        if (target.EntityBuffHelper.AddBuff(newBuff) && RemoveEnterBuffWhenExit)
                        {
                            if (!EntityBuffs_Enter.ContainsKey(target.GUID))
                            {
                                EntityBuffs_Enter.Add(target.GUID, new List<EntityBuff>());
                            }

                            EntityBuffs_Enter[target.GUID].Add(newBuff);
                        }
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
                    if (duration > EffectInterval)
                    {
                        foreach (EntityBuff buff in RawEntityBuffs_Stay)
                        {
                            EntityBuff newBuff = buff.Clone();
                            if (target.EntityBuffHelper.AddBuff(newBuff) && RemoveStayBuffWhenExit)
                            {
                                if (!EntityBuffs_Stay.ContainsKey(target.GUID))
                                {
                                    EntityBuffs_Stay.Add(target.GUID, new List<EntityBuff>());
                                }

                                EntityBuffs_Stay[target.GUID].Add(newBuff);
                            }
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
                    foreach (EntityBuff buff in RawEntityBuffs_Exit)
                    {
                        target.EntityBuffHelper.AddBuff(buff.Clone());
                    }

                    if (RemoveEnterBuffWhenExit && EntityBuffs_Enter.TryGetValue(target.GUID, out List<EntityBuff> enterBuffs))
                    {
                        foreach (EntityBuff enterBuff in enterBuffs)
                        {
                            target.EntityBuffHelper.RemoveBuff(enterBuff);
                        }

                        EntityBuffs_Enter.Remove(target.GUID);
                    }

                    if (RemoveStayBuffWhenExit && EntityBuffs_Stay.TryGetValue(target.GUID, out List<EntityBuff> stayBuffs))
                    {
                        foreach (EntityBuff stayBuff in stayBuffs)
                        {
                            target.EntityBuffHelper.RemoveBuff(stayBuff);
                        }

                        EntityBuffs_Stay.Remove(target.GUID);
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
        action.RemoveEnterBuffWhenExit = RemoveEnterBuffWhenExit;
        action.RawEntityBuffs_Stay = RawEntityBuffs_Stay.Clone();
        action.RemoveStayBuffWhenExit = RemoveStayBuffWhenExit;
        action.EffectInterval = EffectInterval;
        action.RawEntityBuffs_Exit = RawEntityBuffs_Exit.Clone();
    }

    public override void CopyDataFrom(EntityPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntityPassiveSkillAction_TriggerZoneEffect action = ((EntityPassiveSkillAction_TriggerZoneEffect) srcData);
        EffectiveOnRelativeCamp = action.EffectiveOnRelativeCamp;
        RawEntityBuffs_Enter = action.RawEntityBuffs_Enter.Clone();
        RemoveEnterBuffWhenExit = action.RemoveEnterBuffWhenExit;
        RawEntityBuffs_Stay = action.RawEntityBuffs_Stay.Clone();
        RemoveStayBuffWhenExit = action.RemoveStayBuffWhenExit;
        EffectInterval = action.EffectInterval;
        RawEntityBuffs_Exit = action.RawEntityBuffs_Exit.Clone();
    }
}
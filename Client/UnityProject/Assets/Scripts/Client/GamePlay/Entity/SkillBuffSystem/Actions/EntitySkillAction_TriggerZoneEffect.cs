using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntitySkillAction_TriggerZoneEffect : EntitySkillAction, EntitySkillAction.ITriggerAction
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

    public void ExecuteOnTriggerEnter(Collider collider)
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

    public void ExecuteOnTriggerStay(Collider collider)
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

    public void ExecuteOnTriggerExit(Collider collider)
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

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_TriggerZoneEffect action = ((EntitySkillAction_TriggerZoneEffect) newAction);
        action.EffectiveOnRelativeCamp = EffectiveOnRelativeCamp;
        action.RawEntityBuffs_Enter = RawEntityBuffs_Enter.Clone<EntityBuff, EntityBuff>();
        action.RemoveEnterBuffWhenExit = RemoveEnterBuffWhenExit;
        action.RawEntityBuffs_Stay = RawEntityBuffs_Stay.Clone<EntityBuff, EntityBuff>();
        action.RemoveStayBuffWhenExit = RemoveStayBuffWhenExit;
        action.EffectInterval = EffectInterval;
        action.RawEntityBuffs_Exit = RawEntityBuffs_Exit.Clone<EntityBuff, EntityBuff>();
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_TriggerZoneEffect action = ((EntitySkillAction_TriggerZoneEffect) srcData);
        EffectiveOnRelativeCamp = action.EffectiveOnRelativeCamp;

        if (RawEntityBuffs_Enter.Count != action.RawEntityBuffs_Enter.Count)
        {
            Debug.LogError("EntitySkillAction_TriggerZoneEffect CopyDataFrom RawEntityBuffs_Enter数量不一致");
        }
        else
        {
            for (int i = 0; i < RawEntityBuffs_Enter.Count; i++)
            {
                RawEntityBuffs_Enter[i].CopyDataFrom(action.RawEntityBuffs_Enter[i]);
            }
        }

        RemoveEnterBuffWhenExit = action.RemoveEnterBuffWhenExit;

        if (RawEntityBuffs_Stay.Count != action.RawEntityBuffs_Stay.Count)
        {
            Debug.LogError("EntitySkillAction_TriggerZoneEffect CopyDataFrom RawEntityBuffs_Stay数量不一致");
        }
        else
        {
            for (int i = 0; i < RawEntityBuffs_Stay.Count; i++)
            {
                RawEntityBuffs_Stay[i].CopyDataFrom(action.RawEntityBuffs_Stay[i]);
            }
        }

        RemoveStayBuffWhenExit = action.RemoveStayBuffWhenExit;
        EffectInterval = action.EffectInterval;
        if (RawEntityBuffs_Exit.Count != action.RawEntityBuffs_Exit.Count)
        {
            Debug.LogError("EntitySkillAction_TriggerZoneEffect CopyDataFrom RawEntityBuffs_Exit数量不一致");
        }
        else
        {
            for (int i = 0; i < RawEntityBuffs_Exit.Count; i++)
            {
                RawEntityBuffs_Exit[i].CopyDataFrom(action.RawEntityBuffs_Exit[i]);
            }
        }
    }
}
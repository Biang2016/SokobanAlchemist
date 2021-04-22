using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntitySkillAction_TriggerZoneAction : EntitySkillAction, EntitySkillAction.ITriggerAction
{
    public Dictionary<uint, float> ActorStayTimeDict = new Dictionary<uint, float>();

    public override void OnRecycled()
    {
        ActorStayTimeDict.Clear();
    }

    public override void Init(Entity entity)
    {
        base.Init(entity);
        foreach (IPureAction pureAction in EntityActions_Enter)
        {
            if (pureAction is EntitySkillAction action)
            {
                action.Init(Entity);
            }
        }

        foreach (IPureAction pureAction in EntityActions_Stay)
        {
            if (pureAction is EntitySkillAction action)
            {
                action.Init(Entity);
            }
        }

        foreach (IPureAction pureAction in EntityActions_Exit)
        {
            if (pureAction is EntitySkillAction action)
            {
                action.Init(Entity);
            }
        }
    }

    public override void UnInit()
    {
        base.UnInit();
        foreach (IPureAction pureAction in EntityActions_Enter)
        {
            if (pureAction is EntitySkillAction action)
            {
                action.UnInit();
            }
        }

        foreach (IPureAction pureAction in EntityActions_Stay)
        {
            if (pureAction is EntitySkillAction action)
            {
                action.UnInit();
            }
        }

        foreach (IPureAction pureAction in EntityActions_Exit)
        {
            if (pureAction is EntitySkillAction action)
            {
                action.UnInit();
            }
        }
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

    [LabelText("停留时按交互键触发")]
    public bool EffectiveWhenInteractiveKeyDown = false;

    [LabelText("交互提示语")]
    public string InteractiveKeyNotice = "";

    [LabelText("交互提示语位置")]
    public NoticePanel.TipPositionType TipPositionType = NoticePanel.TipPositionType.RightCenter;

    [LabelText("交互提示语持续时间")]
    public float InteractiveKeyNoticeDuration = 0.5f;

    [LabelText("停留时重复触发间隔时间/s")]
    [HideIf("EffectiveWhenInteractiveKeyDown")]
    public float ActionInterval = 1f;

    [SerializeReference]
    [LabelText("离开触发事件")]
    public List<IPureAction> EntityActions_Exit = new List<IPureAction>();

    public void ExecuteOnTriggerEnter(Collider collider)
    {
        if (LayerManager.Instance.CheckLayerValid(Entity.Camp, EffectiveOnRelativeCamp, collider.gameObject.layer))
        {
            Entity target = collider.GetComponentInParent<Entity>();
            if (target.IsNotNullAndAlive())
            {
                if (EffectiveWhenInteractiveKeyDown)
                {
                    ClientGameManager.Instance.NoticePanel.ShowTip(InteractiveKeyNotice, TipPositionType, InteractiveKeyNoticeDuration);
                }

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

    public void ExecuteOnTriggerStay(Collider collider)
    {
        if (LayerManager.Instance.CheckLayerValid(Entity.Camp, EffectiveOnRelativeCamp, collider.gameObject.layer))
        {
            Entity target = collider.GetComponentInParent<Entity>();
            if (target.IsNotNullAndAlive())
            {
                if (EffectiveWhenInteractiveKeyDown)
                {
                    if (ControlManager.Instance.Common_InteractiveKey.Down)
                    {
                        foreach (IPureAction action in EntityActions_Stay)
                        {
                            action.Execute();
                        }
                    }
                }
                else
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
    }

    public void ExecuteOnTriggerExit(Collider collider)
    {
        if (LayerManager.Instance.CheckLayerValid(Entity.Camp, EffectiveOnRelativeCamp, collider.gameObject.layer))
        {
            Entity target = collider.GetComponentInParent<Entity>();
            if (target.IsNotNullAndAlive())
            {
                if (EffectiveWhenInteractiveKeyDown)
                {
                    ClientGameManager.Instance.NoticePanel.HideTip();
                }

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

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_TriggerZoneAction action = ((EntitySkillAction_TriggerZoneAction) newAction);
        action.EffectiveOnRelativeCamp = EffectiveOnRelativeCamp;
        action.EntityActions_Enter = EntityActions_Enter.Clone<IPureAction, IPureAction>();
        action.EntityActions_Stay = EntityActions_Stay.Clone<IPureAction, IPureAction>();
        action.EffectiveWhenInteractiveKeyDown = EffectiveWhenInteractiveKeyDown;
        action.InteractiveKeyNotice = InteractiveKeyNotice;
        action.TipPositionType = TipPositionType;
        action.InteractiveKeyNoticeDuration = InteractiveKeyNoticeDuration;
        action.ActionInterval = ActionInterval;
        action.EntityActions_Exit = EntityActions_Exit.Clone<IPureAction, IPureAction>();
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_TriggerZoneAction action = ((EntitySkillAction_TriggerZoneAction) srcData);
        EffectiveOnRelativeCamp = action.EffectiveOnRelativeCamp;
        EntityActions_Enter = action.EntityActions_Enter.Clone<IPureAction, IPureAction>();
        EntityActions_Stay = action.EntityActions_Stay.Clone<IPureAction, IPureAction>();
        EffectiveWhenInteractiveKeyDown = action.EffectiveWhenInteractiveKeyDown;
        InteractiveKeyNotice = action.InteractiveKeyNotice;
        TipPositionType = action.TipPositionType;
        InteractiveKeyNoticeDuration = action.InteractiveKeyNoticeDuration;
        ActionInterval = action.ActionInterval;
        EntityActions_Exit = action.EntityActions_Exit.Clone<IPureAction, IPureAction>();
    }
}
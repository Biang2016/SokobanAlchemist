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
        foreach (EntitySkillAction esa in EntityActions_Enter)
        {
            esa.Init(Entity);
        }

        foreach (EntitySkillAction esa in EntityActions_Stay)
        {
            esa.Init(Entity);
        }

        foreach (EntitySkillAction esa in EntityActions_Exit)
        {
            esa.Init(Entity);
        }
    }

    public override void UnInit()
    {
        base.UnInit();
        foreach (EntitySkillAction esa in EntityActions_Enter)
        {
            esa.UnInit();
        }

        foreach (EntitySkillAction esa in EntityActions_Stay)
        {
            esa.UnInit();
        }

        foreach (EntitySkillAction esa in EntityActions_Exit)
        {
            esa.UnInit();
        }
    }

    protected override string Description => "箱子范围触发行为";

    [LabelText("生效于相对阵营")]
    public RelativeCamp EffectiveOnRelativeCamp;

    [LabelText("生效于特定种类Entity")]
    public bool EffectiveOnSpecificEntity;

    [LabelText("Entity种类")]
    [ShowIf("EffectiveOnSpecificEntity")]
    public TypeSelectHelper EffectiveOnSpecificEntityType = new TypeSelectHelper {TypeDefineType = TypeDefineType.Box};

    [SerializeReference]
    [LabelText("进入触发事件")]
    public List<EntitySkillAction> EntityActions_Enter = new List<EntitySkillAction>();

    [SerializeReference]
    [LabelText("停留触发事件")]
    public List<EntitySkillAction> EntityActions_Stay = new List<EntitySkillAction>();

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
    public List<EntitySkillAction> EntityActions_Exit = new List<EntitySkillAction>();

    public void ExecuteOnTriggerEnter(Collider collider)
    {
        if (LayerManager.Instance.CheckLayerValid(Entity.Camp, EffectiveOnRelativeCamp, collider.gameObject.layer))
        {
            Entity target = collider.GetComponentInParent<Entity>();
            if (target.IsNotNullAndAlive())
            {
                if (EffectiveOnSpecificEntity)
                {
                    if (target.EntityTypeIndex != ConfigManager.GetTypeIndex(EffectiveOnSpecificEntityType.TypeDefineType, EffectiveOnSpecificEntityType.TypeName))
                    {
                        return;
                    }
                }

                if (EffectiveWhenInteractiveKeyDown)
                {
                    ClientGameManager.Instance.NoticePanel.ShowTip(InteractiveKeyNotice, TipPositionType, InteractiveKeyNoticeDuration);
                }

                if (!ActorStayTimeDict.ContainsKey(target.GUID))
                {
                    ActorStayTimeDict.Add(target.GUID, 0);
                    foreach (EntitySkillAction action in EntityActions_Enter)
                    {
                        if (action is IPureAction pureAction)
                        {
                            pureAction.Execute();
                        }

                        if (action is IEntityAction entityAction)
                        {
                            entityAction.ExecuteOnEntity(target);
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
                if (EffectiveOnSpecificEntity)
                {
                    if (target.EntityTypeIndex != ConfigManager.GetTypeIndex(EffectiveOnSpecificEntityType.TypeDefineType, EffectiveOnSpecificEntityType.TypeName))
                    {
                        return;
                    }
                }

                if (EffectiveWhenInteractiveKeyDown)
                {
                    if (ControlManager.Instance.Common_InteractiveKey.Down)
                    {
                        foreach (EntitySkillAction action in EntityActions_Stay)
                        {
                            if (action is IPureAction pureAction)
                            {
                                pureAction.Execute();
                            }

                            if (action is IEntityAction entityAction)
                            {
                                entityAction.ExecuteOnEntity(target);
                            }
                        }
                    }
                }
                else
                {
                    if (ActorStayTimeDict.TryGetValue(target.GUID, out float duration))
                    {
                        if (duration > ActionInterval)
                        {
                            foreach (EntitySkillAction action in EntityActions_Stay)
                            {
                                if (action is IPureAction pureAction)
                                {
                                    pureAction.Execute();
                                }

                                if (action is IEntityAction entityAction)
                                {
                                    entityAction.ExecuteOnEntity(target);
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
    }

    public void ExecuteOnTriggerExit(Collider collider)
    {
        if (LayerManager.Instance.CheckLayerValid(Entity.Camp, EffectiveOnRelativeCamp, collider.gameObject.layer))
        {
            Entity target = collider.GetComponentInParent<Entity>();
            if (target.IsNotNullAndAlive())
            {
                if (EffectiveOnSpecificEntity)
                {
                    if (target.EntityTypeIndex != ConfigManager.GetTypeIndex(EffectiveOnSpecificEntityType.TypeDefineType, EffectiveOnSpecificEntityType.TypeName))
                    {
                        return;
                    }
                }

                if (EffectiveWhenInteractiveKeyDown)
                {
                    ClientGameManager.Instance.NoticePanel.HideTip();
                }

                if (ActorStayTimeDict.ContainsKey(target.GUID))
                {
                    ActorStayTimeDict.Remove(target.GUID);
                    foreach (EntitySkillAction action in EntityActions_Exit)
                    {
                        if (action is IPureAction pureAction)
                        {
                            pureAction.Execute();
                        }

                        if (action is IEntityAction entityAction)
                        {
                            entityAction.ExecuteOnEntity(target);
                        }
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
        action.EffectiveOnSpecificEntity = EffectiveOnSpecificEntity;
        action.EffectiveOnSpecificEntityType = EffectiveOnSpecificEntityType.Clone();
        action.EntityActions_Enter = EntityActions_Enter.Clone<EntitySkillAction, EntitySkillAction>();
        action.EntityActions_Stay = EntityActions_Stay.Clone<EntitySkillAction, EntitySkillAction>();
        action.EffectiveWhenInteractiveKeyDown = EffectiveWhenInteractiveKeyDown;
        action.InteractiveKeyNotice = InteractiveKeyNotice;
        action.TipPositionType = TipPositionType;
        action.InteractiveKeyNoticeDuration = InteractiveKeyNoticeDuration;
        action.ActionInterval = ActionInterval;
        action.EntityActions_Exit = EntityActions_Exit.Clone<EntitySkillAction, EntitySkillAction>();
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_TriggerZoneAction action = ((EntitySkillAction_TriggerZoneAction) srcData);
        EffectiveOnRelativeCamp = action.EffectiveOnRelativeCamp;
        EffectiveOnSpecificEntity = action.EffectiveOnSpecificEntity;
        EffectiveOnSpecificEntityType = action.EffectiveOnSpecificEntityType.Clone();
        EntityActions_Enter = action.EntityActions_Enter.Clone<EntitySkillAction, EntitySkillAction>();
        EntityActions_Stay = action.EntityActions_Stay.Clone<EntitySkillAction, EntitySkillAction>();
        EffectiveWhenInteractiveKeyDown = action.EffectiveWhenInteractiveKeyDown;
        InteractiveKeyNotice = action.InteractiveKeyNotice;
        TipPositionType = action.TipPositionType;
        InteractiveKeyNoticeDuration = action.InteractiveKeyNoticeDuration;
        ActionInterval = action.ActionInterval;
        EntityActions_Exit = action.EntityActions_Exit.Clone<EntitySkillAction, EntitySkillAction>();
    }
}
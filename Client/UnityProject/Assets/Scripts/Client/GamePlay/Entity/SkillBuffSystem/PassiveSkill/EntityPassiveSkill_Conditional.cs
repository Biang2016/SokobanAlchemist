using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntityPassiveSkill_Conditional : EntityPassiveSkill
{
    protected override string Description => "条件触发的箱子被动技能";

    [Flags]
    public enum PassiveSkillConditionType
    {
        None = 0,
        OnInit = 1 << 0,
        OnTick = 1 << 1,
        OnBeingLift = 1 << 2,
        OnBeingKicked = 1 << 3,
        OnFlyingCollisionEnter = 1 << 4,
        OnBeingKickedCollisionEnter = 1 << 5,
        OnDroppingFromAirCollisionEnter = 1 << 6,
        OnEntityTriggerZone = 1 << 7,
        OnEntityGrindTriggerZone = 1 << 8,

        OnBeforeDestroyEntity = 1 << 10,
        OnDestroyEntity = 1 << 11,
        OnLevelEvent = 1 << 12,
        OnBeforeMergeBox = 1 << 13,
        OnMergeBox = 1 << 14,
    }

    public PassiveSkillConditionType PassiveSkillCondition;

    #region Conditions

    public override void OnInit()
    {
        base.OnInit();
        InitBoxPassiveSkillActions();
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnInit))
        {
            foreach (EntityPassiveSkillAction action in EntityPassiveSkillActions)
            {
                if (action is EntityPassiveSkillAction.IPureAction pureAction)
                {
                    pureAction.Execute();
                }
            }
        }
    }

    public override void OnTick(float deltaTime)
    {
        base.OnTick(deltaTime);
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnTick))
        {
            foreach (EntityPassiveSkillAction action in EntityPassiveSkillActions)
            {
                if (action is EntityPassiveSkillAction.IPureAction pureAction)
                {
                    pureAction.Execute();
                }
            }
        }
    }

    #region Level Event Trigger

    private bool IsEventTrigger => PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnLevelEvent);

    [ShowIf("IsEventTrigger")]
    [LabelText("多个事件联合触发")]
    public bool MultiEventTrigger = false;

    [ShowIf("IsEventTrigger")]
    [ShowIf("MultiEventTrigger")]
    [LabelText("监听关卡事件花名列表(联合触发)")]
    public List<string> ListenLevelEventAliasList = new List<string>();

    // 联合触发记录
    private List<bool> multiTriggerFlags = new List<bool>();

    [HideIf("MultiEventTrigger")]
    [ShowIf("IsEventTrigger")]
    [LabelText("监听关卡事件花名(单个触发)")]
    public string ListenLevelEventAlias;

    [ShowIf("IsEventTrigger")]
    [LabelText("最大触发次数")]
    public int MaxTriggeredTimes = 1;

    private int triggeredTimes = 0;

    public override void OnRegisterLevelEventID()
    {
        base.OnRegisterLevelEventID();
        triggeredTimes = 0;
        ClientGameManager.Instance.BattleMessenger.AddListener<string>((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, OnEvent);
        multiTriggerFlags.Clear();
        foreach (string alias in ListenLevelEventAliasList)
        {
            multiTriggerFlags.Add(false);
        }
    }

    public override void OnUnRegisterLevelEventID()
    {
        base.OnUnRegisterLevelEventID();
        triggeredTimes = 0;
        ClientGameManager.Instance.BattleMessenger.RemoveListener<string>((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, OnEvent);
        multiTriggerFlags.Clear();
    }

    private void OnEvent(string eventAlias)
    {
        if (MultiEventTrigger)
        {
            for (int index = 0; index < ListenLevelEventAliasList.Count; index++)
            {
                string alias = ListenLevelEventAliasList[index];
                if (eventAlias == alias && !multiTriggerFlags[index])
                {
                    multiTriggerFlags[index] = true;
                }
            }

            bool trigger = true;
            foreach (bool flag in multiTriggerFlags)
            {
                if (!flag) trigger = false;
            }

            if (trigger)
            {
                ExecuteFunction();
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(ListenLevelEventAlias))
            {
                if (ListenLevelEventAlias.Equals(eventAlias))
                {
                    ExecuteFunction();
                }
            }
        }
    }

    private void ExecuteFunction()
    {
        if (triggeredTimes < MaxTriggeredTimes)
        {
            OnEventExecute();
            if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnLevelEvent))
            {
                foreach (EntityPassiveSkillAction action in EntityPassiveSkillActions)
                {
                    if (action is EntityPassiveSkillAction.IPureAction pureAction)
                    {
                        pureAction.Execute();
                    }
                }
            }

            triggeredTimes++;
            for (int i = 0; i < multiTriggerFlags.Count; i++)
            {
                multiTriggerFlags[i] = false;
            }
        }
    }

    protected virtual void OnEventExecute()
    {
    }

    #endregion

    public override void OnBeingLift(Actor actor)
    {
        base.OnBeingLift(actor);
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnBeingLift))
        {
            foreach (EntityPassiveSkillAction action in EntityPassiveSkillActions)
            {
                if (action is EntityPassiveSkillAction.IPureAction pureAction)
                {
                    pureAction.Execute();
                }

                if (action is EntityPassiveSkillAction.IActorOperationAction actorOperationAction)
                {
                    actorOperationAction.OnOperation(actor);
                }
            }
        }
    }

    public override void OnBeingKicked(Actor actor)
    {
        base.OnBeingKicked(actor);
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnBeingKicked))
        {
            foreach (EntityPassiveSkillAction action in EntityPassiveSkillActions)
            {
                if (action is EntityPassiveSkillAction.IPureAction pureAction)
                {
                    pureAction.Execute();
                }

                if (action is EntityPassiveSkillAction.IActorOperationAction actorOperationAction)
                {
                    actorOperationAction.OnOperation(actor);
                }
            }
        }
    }

    public override void OnFlyingCollisionEnter(Collision collision)
    {
        base.OnFlyingCollisionEnter(collision);
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnFlyingCollisionEnter))
        {
            foreach (EntityPassiveSkillAction action in EntityPassiveSkillActions)
            {
                if (action is EntityPassiveSkillAction.IPureAction pureAction)
                {
                    pureAction.Execute();
                }

                if (action is EntityPassiveSkillAction.ICollideAction collideAction)
                {
                    collideAction.OnCollide(collision);
                }
            }
        }
    }

    public override void OnBeingKickedCollisionEnter(Collision collision, Box.KickLocalAxis kickLocalAxis)
    {
        base.OnBeingKickedCollisionEnter(collision, kickLocalAxis);
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnBeingKickedCollisionEnter))
        {
            foreach (EntityPassiveSkillAction action in EntityPassiveSkillActions)
            {
                if (action is EntityPassiveSkillAction.IPureAction pureAction)
                {
                    pureAction.Execute();
                }

                if (action is EntityPassiveSkillAction.ICollideAction collideAction)
                {
                    collideAction.OnCollide(collision);
                }
            }
        }
    }

    public override void OnDroppingFromAirCollisionEnter(Collision collision)
    {
        base.OnDroppingFromAirCollisionEnter(collision);
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnDroppingFromAirCollisionEnter))
        {
            foreach (EntityPassiveSkillAction action in EntityPassiveSkillActions)
            {
                if (action is EntityPassiveSkillAction.IPureAction pureAction)
                {
                    pureAction.Execute();
                }

                if (action is EntityPassiveSkillAction.ICollideAction collideAction)
                {
                    collideAction.OnCollide(collision);
                }
            }
        }
    }

    public override void OnTriggerZoneEnter(Collider collider)
    {
        base.OnTriggerZoneEnter(collider);
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnEntityTriggerZone))
        {
            foreach (EntityPassiveSkillAction action in EntityPassiveSkillActions)
            {
                if (action is EntityPassiveSkillAction.ITriggerAction collideAction)
                {
                    collideAction.OnTriggerEnter(collider);
                }
            }
        }
    }

    public override void OnTriggerZoneStay(Collider collider)
    {
        base.OnTriggerZoneStay(collider);
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnEntityTriggerZone))
        {
            foreach (EntityPassiveSkillAction action in EntityPassiveSkillActions)
            {
                if (action is EntityPassiveSkillAction.ITriggerAction collideAction)
                {
                    collideAction.OnTriggerStay(collider);
                }
            }
        }
    }

    public override void OnTriggerZoneExit(Collider collider)
    {
        base.OnTriggerZoneExit(collider);
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnEntityTriggerZone))
        {
            foreach (EntityPassiveSkillAction action in EntityPassiveSkillActions)
            {
                if (action is EntityPassiveSkillAction.ITriggerAction collideAction)
                {
                    collideAction.OnTriggerExit(collider);
                }
            }
        }
    }

    public override void OnGrindTriggerZoneEnter(Collider collider)
    {
        base.OnTriggerZoneEnter(collider);
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnEntityGrindTriggerZone))
        {
            foreach (EntityPassiveSkillAction action in EntityPassiveSkillActions)
            {
                if (action is EntityPassiveSkillAction.ITriggerAction collideAction)
                {
                    collideAction.OnTriggerEnter(collider);
                }
            }
        }
    }

    public override void OnGrindTriggerZoneStay(Collider collider)
    {
        base.OnTriggerZoneStay(collider);
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnEntityGrindTriggerZone))
        {
            foreach (EntityPassiveSkillAction action in EntityPassiveSkillActions)
            {
                if (action is EntityPassiveSkillAction.ITriggerAction collideAction)
                {
                    collideAction.OnTriggerStay(collider);
                }
            }
        }
    }

    public override void OnGrindTriggerZoneExit(Collider collider)
    {
        base.OnTriggerZoneExit(collider);
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnEntityGrindTriggerZone))
        {
            foreach (EntityPassiveSkillAction action in EntityPassiveSkillActions)
            {
                if (action is EntityPassiveSkillAction.ITriggerAction collideAction)
                {
                    collideAction.OnTriggerExit(collider);
                }
            }
        }
    }

    public override void OnBeforeDestroyEntity()
    {
        base.OnBeforeDestroyEntity();
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnBeforeDestroyEntity))
        {
            foreach (EntityPassiveSkillAction action in EntityPassiveSkillActions)
            {
                if (action is EntityPassiveSkillAction.IPureAction pureAction)
                {
                    pureAction.Execute();
                }
            }
        }
    }

    public override void OnDestroyEntity()
    {
        base.OnDestroyEntity();
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnDestroyEntity))
        {
            foreach (EntityPassiveSkillAction action in EntityPassiveSkillActions)
            {
                if (action is EntityPassiveSkillAction.IPureAction pureAction)
                {
                    pureAction.Execute();
                }
            }
        }

        UnInitPassiveSkillActions();
    }

    public override void OnBeforeMergeBox()
    {
        base.OnBeforeMergeBox();
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnBeforeMergeBox))
        {
            foreach (EntityPassiveSkillAction action in EntityPassiveSkillActions)
            {
                if (action is EntityPassiveSkillAction.IPureAction pureAction)
                {
                    pureAction.Execute();
                }
            }
        }
    }

    public override void OnMergeBox()
    {
        base.OnMergeBox();
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnMergeBox))
        {
            foreach (EntityPassiveSkillAction action in EntityPassiveSkillActions)
            {
                if (action is EntityPassiveSkillAction.IPureAction pureAction)
                {
                    pureAction.Execute();
                }
            }
        }

        UnInitPassiveSkillActions();
    }

    #endregion

    #region 箱子被动技能行为部分

    [SerializeReference]
    [LabelText("具体内容")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<EntityPassiveSkillAction> RawEntityPassiveSkillActions = new List<EntityPassiveSkillAction>(); // 干数据，禁修改

    [HideInInspector]
    public List<EntityPassiveSkillAction> EntityPassiveSkillActions = new List<EntityPassiveSkillAction>(); // 湿数据，每个Entity生命周期开始前从干数据拷出，结束后清除

    internal bool EntityPassiveSkillActionsMarkAsDeleted = false;

    private void InitBoxPassiveSkillActions()
    {
        foreach (EntityPassiveSkillAction epsa in EntityPassiveSkillActions) // 放在Init里清空，避免在UnInit里出现Entity死亡而modify collection的情况
        {
            epsa.OnRecycled();
        }

        EntityPassiveSkillActions.Clear();

        foreach (EntityPassiveSkillAction rawAction in RawEntityPassiveSkillActions)
        {
            EntityPassiveSkillActions.Add(rawAction.Clone());
        }

        EntityPassiveSkillActionsMarkAsDeleted = false;
        foreach (EntityPassiveSkillAction action in EntityPassiveSkillActions)
        {
            action.Entity = Entity;
        }
    }

    private void UnInitPassiveSkillActions()
    {
        EntityPassiveSkillActionsMarkAsDeleted = false;
    }

    #endregion

    protected override void ChildClone(EntityPassiveSkill newPS)
    {
        base.ChildClone(newPS);
        EntityPassiveSkill_Conditional newPSC = (EntityPassiveSkill_Conditional) newPS;
        newPSC.MultiEventTrigger = MultiEventTrigger;
        newPSC.ListenLevelEventAliasList = ListenLevelEventAliasList.Clone();
        newPSC.ListenLevelEventAlias = ListenLevelEventAlias;
        newPSC.MaxTriggeredTimes = MaxTriggeredTimes;

        newPSC.PassiveSkillCondition = PassiveSkillCondition;
        foreach (EntityPassiveSkillAction rawBoxPassiveSkillAction in RawEntityPassiveSkillActions)
        {
            newPSC.RawEntityPassiveSkillActions.Add(rawBoxPassiveSkillAction.Clone());
        }
    }

    public override void CopyDataFrom(EntityPassiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        EntityPassiveSkill_Conditional srcPSC = (EntityPassiveSkill_Conditional) srcData;
        MultiEventTrigger = srcPSC.MultiEventTrigger;
        ListenLevelEventAliasList = srcPSC.ListenLevelEventAliasList.Clone();
        ListenLevelEventAlias = srcPSC.ListenLevelEventAlias;
        MaxTriggeredTimes = srcPSC.MaxTriggeredTimes;

        PassiveSkillCondition = srcPSC.PassiveSkillCondition;
        RawEntityPassiveSkillActions.Clear();
        foreach (EntityPassiveSkillAction rawBoxPassiveSkillAction in srcPSC.RawEntityPassiveSkillActions)
        {
            RawEntityPassiveSkillActions.Add(rawBoxPassiveSkillAction.Clone());
        }
    }
}
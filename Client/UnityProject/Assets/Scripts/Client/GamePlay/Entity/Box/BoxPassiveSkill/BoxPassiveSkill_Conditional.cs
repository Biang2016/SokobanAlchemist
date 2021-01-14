using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public class BoxPassiveSkill_Conditional : BoxPassiveSkill
{
    protected override string Description => "条件触发的箱子被动技能";

    [Flags]
    public enum BoxPassiveSkillConditionType
    {
        None = 0,
        OnInit = 1 << 0,
        OnTick = 1 << 1,
        OnBeingLift = 1 << 2,
        OnBeingKicked = 1 << 3,
        OnFlyingCollisionEnter = 1 << 4,
        OnBeingKickedCollisionEnter = 1 << 5,
        OnDroppingFromAirCollisionEnter = 1 << 6,
        OnBoxThornTrapTriggerEnter = 1 << 7,
        OnBoxThornTrapTriggerStay = 1 << 8,
        OnBoxThornTrapTriggerExit = 1 << 9,
        OnBeforeDestroyBox = 1 << 10,
        OnDestroyBox = 1 << 11,
        OnLevelEvent = 1 << 12,
    }

    public BoxPassiveSkillConditionType BoxPassiveSkillCondition;

    #region Conditions

    public override void OnInit()
    {
        base.OnInit();
        InitBoxPassiveSkillActions();
        if (BoxPassiveSkillCondition.HasFlag(BoxPassiveSkillConditionType.OnInit))
        {
            foreach (BoxPassiveSkillAction action in BoxPassiveSkillActions)
            {
                if (action is BoxPassiveSkillAction.IPureAction pureAction)
                {
                    pureAction.Execute();
                }
            }
        }
    }

    public override void OnTick(float deltaTime)
    {
        base.OnTick(deltaTime);
        if (BoxPassiveSkillCondition.HasFlag(BoxPassiveSkillConditionType.OnTick))
        {
            foreach (BoxPassiveSkillAction action in BoxPassiveSkillActions)
            {
                if (action is BoxPassiveSkillAction.IPureAction pureAction)
                {
                    pureAction.Execute();
                }
            }
        }
    }

    #region Level Event Trigger

    private bool IsEventTrigger => BoxPassiveSkillCondition.HasFlag(BoxPassiveSkillConditionType.OnLevelEvent);

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
            if (BoxPassiveSkillCondition.HasFlag(BoxPassiveSkillConditionType.OnLevelEvent))
            {
                foreach (BoxPassiveSkillAction action in BoxPassiveSkillActions)
                {
                    if (action is BoxPassiveSkillAction.IPureAction pureAction)
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
        if (BoxPassiveSkillCondition.HasFlag(BoxPassiveSkillConditionType.OnBeingLift))
        {
            foreach (BoxPassiveSkillAction action in BoxPassiveSkillActions)
            {
                if (action is BoxPassiveSkillAction.IPureAction pureAction)
                {
                    pureAction.Execute();
                }

                if (action is BoxPassiveSkillAction.IActorOperationAction actorOperationAction)
                {
                    actorOperationAction.OnOperation(actor);
                }
            }
        }
    }

    public override void OnBeingKicked(Actor actor)
    {
        base.OnBeingKicked(actor);
        if (BoxPassiveSkillCondition.HasFlag(BoxPassiveSkillConditionType.OnBeingKicked))
        {
            foreach (BoxPassiveSkillAction action in BoxPassiveSkillActions)
            {
                if (action is BoxPassiveSkillAction.IPureAction pureAction)
                {
                    pureAction.Execute();
                }

                if (action is BoxPassiveSkillAction.IActorOperationAction actorOperationAction)
                {
                    actorOperationAction.OnOperation(actor);
                }
            }
        }
    }

    public override void OnFlyingCollisionEnter(Collision collision)
    {
        base.OnFlyingCollisionEnter(collision);
        if (BoxPassiveSkillCondition.HasFlag(BoxPassiveSkillConditionType.OnFlyingCollisionEnter))
        {
            foreach (BoxPassiveSkillAction action in BoxPassiveSkillActions)
            {
                if (action is BoxPassiveSkillAction.IPureAction pureAction)
                {
                    pureAction.Execute();
                }

                if (action is BoxPassiveSkillAction.ICollideAction collideAction)
                {
                    collideAction.OnCollide(collision);
                }
            }
        }
    }

    public override void OnBeingKickedCollisionEnter(Collision collision)
    {
        base.OnBeingKickedCollisionEnter(collision);
        if (BoxPassiveSkillCondition.HasFlag(BoxPassiveSkillConditionType.OnBeingKickedCollisionEnter))
        {
            foreach (BoxPassiveSkillAction action in BoxPassiveSkillActions)
            {
                if (action is BoxPassiveSkillAction.IPureAction pureAction)
                {
                    pureAction.Execute();
                }

                if (action is BoxPassiveSkillAction.ICollideAction collideAction)
                {
                    collideAction.OnCollide(collision);
                }
            }
        }
    }

    public override void OnDroppingFromAirCollisionEnter(Collision collision)
    {
        base.OnDroppingFromAirCollisionEnter(collision);
        if (BoxPassiveSkillCondition.HasFlag(BoxPassiveSkillConditionType.OnDroppingFromAirCollisionEnter))
        {
            foreach (BoxPassiveSkillAction action in BoxPassiveSkillActions)
            {
                if (action is BoxPassiveSkillAction.IPureAction pureAction)
                {
                    pureAction.Execute();
                }

                if (action is BoxPassiveSkillAction.ICollideAction collideAction)
                {
                    collideAction.OnCollide(collision);
                }
            }
        }
    }

    public override void OnBoxThornTrapTriggerEnter(Collider collider)
    {
        base.OnBoxThornTrapTriggerEnter(collider);
        if (BoxPassiveSkillCondition.HasFlag(BoxPassiveSkillConditionType.OnBoxThornTrapTriggerEnter))
        {
            foreach (BoxPassiveSkillAction action in BoxPassiveSkillActions)
            {
                if (action is BoxPassiveSkillAction.IPureAction pureAction)
                {
                    pureAction.Execute();
                }

                if (action is BoxPassiveSkillAction.ITriggerEnterAction collideAction)
                {
                    collideAction.OnTriggerEnter(collider);
                }
            }
        }
    }

    public override void OnBoxThornTrapTriggerStay(Collider collider)
    {
        base.OnBoxThornTrapTriggerStay(collider);
        if (BoxPassiveSkillCondition.HasFlag(BoxPassiveSkillConditionType.OnBoxThornTrapTriggerStay))
        {
            foreach (BoxPassiveSkillAction action in BoxPassiveSkillActions)
            {
                if (action is BoxPassiveSkillAction.IPureAction pureAction)
                {
                    pureAction.Execute();
                }

                if (action is BoxPassiveSkillAction.ITriggerStayAction collideAction)
                {
                    collideAction.OnTriggerStay(collider);
                }
            }
        }
    }

    public override void OnBoxThornTrapTriggerExit(Collider collider)
    {
        base.OnBoxThornTrapTriggerExit(collider);
        if (BoxPassiveSkillCondition.HasFlag(BoxPassiveSkillConditionType.OnBoxThornTrapTriggerExit))
        {
            foreach (BoxPassiveSkillAction action in BoxPassiveSkillActions)
            {
                if (action is BoxPassiveSkillAction.IPureAction pureAction)
                {
                    pureAction.Execute();
                }

                if (action is BoxPassiveSkillAction.ITriggerExitAction collideAction)
                {
                    collideAction.OnTriggerExit(collider);
                }
            }
        }
    }

    public override void OnBeforeDestroyBox()
    {
        base.OnBeforeDestroyBox();
        if (BoxPassiveSkillCondition.HasFlag(BoxPassiveSkillConditionType.OnBeforeDestroyBox))
        {
            foreach (BoxPassiveSkillAction action in BoxPassiveSkillActions)
            {
                if (action is BoxPassiveSkillAction.IPureAction pureAction)
                {
                    pureAction.Execute();
                }
            }
        }
    }

    public override void OnDestroyBox()
    {
        base.OnDestroyBox();
        if (BoxPassiveSkillCondition.HasFlag(BoxPassiveSkillConditionType.OnDestroyBox))
        {
            foreach (BoxPassiveSkillAction action in BoxPassiveSkillActions)
            {
                if (action is BoxPassiveSkillAction.IPureAction pureAction)
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
    public List<BoxPassiveSkillAction> RawBoxPassiveSkillActions = new List<BoxPassiveSkillAction>(); // 干数据，禁修改

    [HideInInspector]
    public List<BoxPassiveSkillAction> BoxPassiveSkillActions = new List<BoxPassiveSkillAction>(); // 湿数据，每个Box生命周期开始前从干数据拷出，结束后清除

    internal bool BoxPassiveSkillActionsMarkAsDeleted = false;

    private void InitBoxPassiveSkillActions()
    {
        BoxPassiveSkillActions.Clear();
        foreach (BoxPassiveSkillAction rawAction in RawBoxPassiveSkillActions)
        {
            BoxPassiveSkillActions.Add(rawAction.Clone());
        }

        BoxPassiveSkillActionsMarkAsDeleted = false;
        foreach (BoxPassiveSkillAction action in BoxPassiveSkillActions)
        {
            action.Box = Box;
        }
    }

    private void UnInitPassiveSkillActions()
    {
        BoxPassiveSkillActionsMarkAsDeleted = false;
    }

    #endregion

    protected override void ChildClone(BoxPassiveSkill newPS)
    {
        base.ChildClone(newPS);
        BoxPassiveSkill_Conditional newPSC = (BoxPassiveSkill_Conditional) newPS;
        newPSC.MultiEventTrigger = MultiEventTrigger;
        newPSC.ListenLevelEventAliasList = ListenLevelEventAliasList.Clone();
        newPSC.ListenLevelEventAlias = ListenLevelEventAlias;
        newPSC.MaxTriggeredTimes = MaxTriggeredTimes;

        newPSC.BoxPassiveSkillCondition = BoxPassiveSkillCondition;
        foreach (BoxPassiveSkillAction rawBoxPassiveSkillAction in RawBoxPassiveSkillActions)
        {
            newPSC.RawBoxPassiveSkillActions.Add(rawBoxPassiveSkillAction.Clone());
        }
    }

    public override void CopyDataFrom(BoxPassiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        BoxPassiveSkill_Conditional srcPSC = (BoxPassiveSkill_Conditional) srcData;
        MultiEventTrigger = srcPSC.MultiEventTrigger;
        ListenLevelEventAliasList = srcPSC.ListenLevelEventAliasList.Clone();
        ListenLevelEventAlias = srcPSC.ListenLevelEventAlias;
        MaxTriggeredTimes = srcPSC.MaxTriggeredTimes;

        BoxPassiveSkillCondition = srcPSC.BoxPassiveSkillCondition;
        RawBoxPassiveSkillActions.Clear();
        foreach (BoxPassiveSkillAction rawBoxPassiveSkillAction in srcPSC.RawBoxPassiveSkillActions)
        {
            RawBoxPassiveSkillActions.Add(rawBoxPassiveSkillAction.Clone());
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntityPassiveSkill_Conditional : EntityPassiveSkill
{
    protected override string Description => "条件触发的箱子被动技能";

    [ReadOnly]
    [HideInEditorMode]
    public uint InitWorldModuleGUID; // 创建时所属的世界模组GUID

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
        OnDestroyEntityByElementDamage = 1 << 15,
    }

    [LabelText("触发时机")]
    public PassiveSkillConditionType PassiveSkillCondition;

    [LabelText("触发概率%")]
    public uint TriggerProbabilityPercent = 100;

    [ShowIf("PassiveSkillCondition", PassiveSkillConditionType.OnDestroyEntityByElementDamage)]
    [LabelText("死亡原因")]
    [ValueDropdown("GetAllBuffAttributeTypes", IsUniqueList = true, DropdownTitle = "选择死亡原因", DrawDropdownForListElements = true)]
    public List<EntityBuffAttribute> DestroyEntityByElementDamageTypeList = new List<EntityBuffAttribute>();

    private HashSet<EntityBuffAttribute> DestroyEntityByElementDamageTypeSet = new HashSet<EntityBuffAttribute>();

    private IEnumerable GetAllBuffAttributeTypes => ConfigManager.GetAllBuffAttributeTypes();

    #region Conditions

    public override void OnInit()
    {
        base.OnInit();
        InitWorldModuleGUID = Entity.InitWorldModuleGUID;
        InitPassiveSkillActions();
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnInit))
        {
            if (TriggerProbabilityPercent.ProbabilityBool())
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

        if (DestroyEntityByElementDamageTypeSet.Count == 0)
        {
            foreach (EntityBuffAttribute attribute in DestroyEntityByElementDamageTypeList)
            {
                DestroyEntityByElementDamageTypeSet.Add(attribute);
            }
        }
    }

    public override void OnTick(float deltaTime)
    {
        base.OnTick(deltaTime);
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnTick))
        {
            if (TriggerProbabilityPercent.ProbabilityBool())
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
        if (IsEventTrigger)
        {
            triggeredTimes = 0;
            ClientGameManager.Instance.BattleMessenger.AddListener<string>((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, OnEvent);
            multiTriggerFlags.Clear();
            foreach (string alias in ListenLevelEventAliasList)
            {
                multiTriggerFlags.Add(false);
            }
        }
    }

    public override void OnUnRegisterLevelEventID()
    {
        base.OnUnRegisterLevelEventID();
        if (IsEventTrigger)
        {
            triggeredTimes = 0;
            ClientGameManager.Instance.BattleMessenger.RemoveListener<string>((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, OnEvent);
            multiTriggerFlags.Clear();
        }
    }

    private void OnEvent(string incomingEventAlias)
    {
        bool CheckEventAlias(string waitingEventAlias)
        {
            if (waitingEventAlias.Contains("{WorldModule}"))
            {
                string formatWaitingEventAlias = waitingEventAlias.Replace("{WorldModule}", InitWorldModuleGUID.ToString());
                return formatWaitingEventAlias.Equals(incomingEventAlias);
            }
            else
            {
                return !string.IsNullOrEmpty(waitingEventAlias) && waitingEventAlias.Equals(incomingEventAlias);
            }
        }

        if (MultiEventTrigger)
        {
            for (int index = 0; index < ListenLevelEventAliasList.Count; index++)
            {
                string alias = ListenLevelEventAliasList[index];
                if (CheckEventAlias(alias) && !multiTriggerFlags[index])
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
                if (TriggerProbabilityPercent.ProbabilityBool())
                {
                    ExecuteFunction();
                }
            }
        }
        else
        {
            if (CheckEventAlias(ListenLevelEventAlias))
            {
                if (TriggerProbabilityPercent.ProbabilityBool())
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
                if (TriggerProbabilityPercent.ProbabilityBool())
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
            if (TriggerProbabilityPercent.ProbabilityBool())
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
    }

    public override void OnBeingKicked(Actor actor)
    {
        base.OnBeingKicked(actor);
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnBeingKicked))
        {
            if (TriggerProbabilityPercent.ProbabilityBool())
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
    }

    public override void OnFlyingCollisionEnter(Collision collision)
    {
        base.OnFlyingCollisionEnter(collision);
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnFlyingCollisionEnter))
        {
            if (TriggerProbabilityPercent.ProbabilityBool())
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
    }

    public override void OnBeingKickedCollisionEnter(Collision collision, Box.KickAxis kickLocalAxis)
    {
        base.OnBeingKickedCollisionEnter(collision, kickLocalAxis);
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnBeingKickedCollisionEnter))
        {
            if (TriggerProbabilityPercent.ProbabilityBool())
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
    }

    public override void OnDroppingFromAirCollisionEnter(Collision collision)
    {
        base.OnDroppingFromAirCollisionEnter(collision);
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnDroppingFromAirCollisionEnter))
        {
            if (TriggerProbabilityPercent.ProbabilityBool())
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
    }

    public override void OnTriggerZoneEnter(Collider collider)
    {
        base.OnTriggerZoneEnter(collider);
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnEntityTriggerZone))
        {
            if (TriggerProbabilityPercent.ProbabilityBool())
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
    }

    public override void OnTriggerZoneStay(Collider collider)
    {
        base.OnTriggerZoneStay(collider);
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnEntityTriggerZone))
        {
            if (TriggerProbabilityPercent.ProbabilityBool())
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
    }

    public override void OnTriggerZoneExit(Collider collider)
    {
        base.OnTriggerZoneExit(collider);
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnEntityTriggerZone))
        {
            if (TriggerProbabilityPercent.ProbabilityBool())
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
    }

    public override void OnGrindTriggerZoneEnter(Collider collider)
    {
        base.OnTriggerZoneEnter(collider);
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnEntityGrindTriggerZone))
        {
            if (TriggerProbabilityPercent.ProbabilityBool())
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
    }

    public override void OnGrindTriggerZoneStay(Collider collider)
    {
        base.OnTriggerZoneStay(collider);
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnEntityGrindTriggerZone))
        {
            if (TriggerProbabilityPercent.ProbabilityBool())
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
    }

    public override void OnGrindTriggerZoneExit(Collider collider)
    {
        base.OnTriggerZoneExit(collider);
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnEntityGrindTriggerZone))
        {
            if (TriggerProbabilityPercent.ProbabilityBool())
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
    }

    public override void OnBeforeDestroyEntity()
    {
        base.OnBeforeDestroyEntity();
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnBeforeDestroyEntity))
        {
            if (TriggerProbabilityPercent.ProbabilityBool())
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
    }

    public override void OnDestroyEntity()
    {
        base.OnDestroyEntity();
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnDestroyEntity))
        {
            if (TriggerProbabilityPercent.ProbabilityBool())
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

        UnInitPassiveSkillActions();
    }

    public override void OnBeforeMergeBox()
    {
        base.OnBeforeMergeBox();
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnBeforeMergeBox))
        {
            if (TriggerProbabilityPercent.ProbabilityBool())
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
    }

    public override void OnMergeBox()
    {
        base.OnMergeBox();
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnMergeBox))
        {
            if (TriggerProbabilityPercent.ProbabilityBool())
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

        UnInitPassiveSkillActions();
    }

    public override void OnDestroyEntityByElementDamage(EntityBuffAttribute entityBuffAttribute)
    {
        base.OnMergeBox();
        if (PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnDestroyEntityByElementDamage))
        {
            if (DestroyEntityByElementDamageTypeSet.Contains(entityBuffAttribute))
            {
                if (TriggerProbabilityPercent.ProbabilityBool())
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
        }
    }

    #endregion

    #region 被动技能行为部分

    [SerializeReference]
    [LabelText("具体内容")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<EntityPassiveSkillAction> RawEntityPassiveSkillActions = new List<EntityPassiveSkillAction>(); // 干数据，禁修改

    [HideInInspector]
    public List<EntityPassiveSkillAction> EntityPassiveSkillActions = new List<EntityPassiveSkillAction>(); // 湿数据，每个Entity生命周期开始前从干数据数据拷贝，数量永远和干数据相等

    internal bool EntityPassiveSkillActionsMarkAsDeleted = false;

    private void InitPassiveSkillActions()
    {
        if (EntityPassiveSkillActions.Count > 0)
        {
            foreach (EntityPassiveSkillAction epsa in EntityPassiveSkillActions) // 放在Init里清空，避免在UnInit里出现Entity死亡而modify collection的情况
            {
                epsa.OnRecycled();
            }

            if (EntityPassiveSkillActions.Count == RawEntityPassiveSkillActions.Count)
            {
                for (int i = 0; i < RawEntityPassiveSkillActions.Count; i++)
                {
                    EntityPassiveSkillAction epsa = EntityPassiveSkillActions[i];
                    epsa.Entity = Entity;
                    epsa.CopyDataFrom(RawEntityPassiveSkillActions[i]);
                    epsa.Init();
                }
            }
            else
            {
                Debug.Log("EntityPassiveSkillActions的数量和RawEntityPassiveSkillActions不一致，请检查临时EPSA添加情况");
            }
        }
        else
        {
            foreach (EntityPassiveSkillAction rawAction in RawEntityPassiveSkillActions)
            {
                EntityPassiveSkillAction epsa = rawAction.Clone();
                epsa.Entity = Entity;
                EntityPassiveSkillActions.Add(epsa);
                epsa.Init();
            }
        }

        EntityPassiveSkillActionsMarkAsDeleted = false;
    }

    private void UnInitPassiveSkillActions()
    {
        EntityPassiveSkillActionsMarkAsDeleted = false;
        foreach (EntityPassiveSkillAction action in EntityPassiveSkillActions)
        {
            action.Entity = null;
            action.UnInit();
        }
    }

    #endregion

    protected override void ChildClone(EntityPassiveSkill newPS)
    {
        base.ChildClone(newPS);
        EntityPassiveSkill_Conditional newPSC = (EntityPassiveSkill_Conditional) newPS;
        newPSC.TriggerProbabilityPercent = TriggerProbabilityPercent;
        newPSC.MultiEventTrigger = MultiEventTrigger;
        newPSC.ListenLevelEventAliasList = ListenLevelEventAliasList.Clone();
        newPSC.ListenLevelEventAlias = ListenLevelEventAlias;
        newPSC.MaxTriggeredTimes = MaxTriggeredTimes;

        newPSC.PassiveSkillCondition = PassiveSkillCondition;
        newPSC.DestroyEntityByElementDamageTypeList = DestroyEntityByElementDamageTypeList.Clone();
        foreach (EntityPassiveSkillAction rawBoxPassiveSkillAction in RawEntityPassiveSkillActions)
        {
            EntityPassiveSkillAction newEPSA = rawBoxPassiveSkillAction.Clone();
            newPSC.RawEntityPassiveSkillActions.Add(newEPSA);
        }
    }

    public override void CopyDataFrom(EntityPassiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        EntityPassiveSkill_Conditional srcPSC = (EntityPassiveSkill_Conditional) srcData;
        TriggerProbabilityPercent = srcPSC.TriggerProbabilityPercent;
        MultiEventTrigger = srcPSC.MultiEventTrigger;
        ListenLevelEventAliasList.Clear();
        foreach (string s in srcPSC.ListenLevelEventAliasList)
        {
            ListenLevelEventAliasList.Add(s);
        }

        ListenLevelEventAlias = srcPSC.ListenLevelEventAlias;
        MaxTriggeredTimes = srcPSC.MaxTriggeredTimes;

        PassiveSkillCondition = srcPSC.PassiveSkillCondition;
        DestroyEntityByElementDamageTypeList = srcPSC.DestroyEntityByElementDamageTypeList.Clone();
        if (srcPSC.RawEntityPassiveSkillActions.Count != RawEntityPassiveSkillActions.Count)
        {
            Debug.LogError("EPS_Conditional CopyDataFrom() Action数量不一致");
        }
        else
        {
            for (int i = 0; i < srcPSC.RawEntityPassiveSkillActions.Count; i++)
            {
                RawEntityPassiveSkillActions[i].CopyDataFrom(srcPSC.RawEntityPassiveSkillActions[i]);
            }
        }
    }
}
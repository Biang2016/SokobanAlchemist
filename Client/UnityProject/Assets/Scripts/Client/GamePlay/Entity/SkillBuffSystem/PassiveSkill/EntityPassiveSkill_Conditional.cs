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
        OnBeingFueled = 1 << 16,
        OnEntityStatValueChange = 1 << 17,
        OnEntityPropertyValueChange = 1 << 18,
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
        void LevelEventExecuteFunction()
        {
            if (triggeredTimes < MaxTriggeredTimes)
            {
                OnEventExecute();
                if (CheckEPSCondition() && PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnLevelEvent))
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

        if (MultiEventTrigger)
        {
            for (int index = 0; index < ListenLevelEventAliasList.Count; index++)
            {
                string alias = ListenLevelEventAliasList[index];
                if (alias.CheckEventAliasOrStateBool(incomingEventAlias, InitWorldModuleGUID) && !multiTriggerFlags[index])
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
                    LevelEventExecuteFunction();
                }
            }
        }
        else
        {
            if (ListenLevelEventAlias.CheckEventAliasOrStateBool(incomingEventAlias, InitWorldModuleGUID))
            {
                if (TriggerProbabilityPercent.ProbabilityBool())
                {
                    LevelEventExecuteFunction();
                }
            }
        }
    }

    protected virtual void OnEventExecute()
    {
    }

    #endregion

    #region Stat/Property OnChange

    [ShowIf("PassiveSkillCondition", PassiveSkillConditionType.OnEntityStatValueChange)]
    [LabelText("状态值种类")]
    public EntityStatType EntityStatChangeType;

    [ShowIf("PassiveSkillCondition", PassiveSkillConditionType.OnEntityStatValueChange)]
    [LabelText("状态值阈值")]
    public int EntityStatChangeThreshold;

    [ShowIf("PassiveSkillCondition", PassiveSkillConditionType.OnEntityStatValueChange)]
    [HideLabel]
    [EnumToggleButtons]
    public ValueChangeOverThresholdType EntityStatChangeThresholdType;

    [ShowIf("PassiveSkillCondition", PassiveSkillConditionType.OnEntityPropertyValueChange)]
    [LabelText("属性值种类")]
    public EntityPropertyType EntityPropertyChangeType;

    [ShowIf("PassiveSkillCondition", PassiveSkillConditionType.OnEntityPropertyValueChange)]
    [LabelText("属性值阈值")]
    public int EntityPropertyChangeThreshold;

    [ShowIf("PassiveSkillCondition", PassiveSkillConditionType.OnEntityPropertyValueChange)]
    [HideLabel]
    [EnumToggleButtons]
    public ValueChangeOverThresholdType EntityPropertyChangeThresholdType;

    #endregion

    #region OnEntityStatPropertyValue

    [LabelText("持续状态条件")]
    public List<EPSConditional> EPSConditionals = new List<EPSConditional>();

    #endregion

    private IEnumerable GetAllBuffAttributeTypes => ConfigManager.GetAllBuffAttributeTypes();

    #region Conditions

    public override void OnInit()
    {
        base.OnInit();
        InitWorldModuleGUID = Entity.InitWorldModuleGUID;
        foreach (EPSConditional epsConditional in EPSConditionals)
        {
            epsConditional.OnInit(Entity);
        }

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
        foreach (EPSConditional epsConditional in EPSConditionals)
        {
            epsConditional.OnTick(deltaTime);
        }

        if (CheckEPSCondition() && PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnTick))
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

    private bool CheckEPSCondition()
    {
        foreach (EPSConditional epsConditional in EPSConditionals)
        {
            if (!epsConditional.Condition) return false;
        }

        return true;
    }

    public override void OnBeingLift(Actor actor)
    {
        base.OnBeingLift(actor);
        if (CheckEPSCondition() && PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnBeingLift))
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
        if (CheckEPSCondition() && PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnBeingKicked))
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
        if (CheckEPSCondition() && PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnFlyingCollisionEnter))
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
        if (CheckEPSCondition() && PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnBeingKickedCollisionEnter))
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
        if (CheckEPSCondition() && PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnDroppingFromAirCollisionEnter))
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
        if (CheckEPSCondition() && PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnEntityTriggerZone))
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
        if (CheckEPSCondition() && PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnEntityTriggerZone))
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
        if (CheckEPSCondition() && PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnEntityTriggerZone))
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
        base.OnGrindTriggerZoneEnter(collider);
        if (CheckEPSCondition() && PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnEntityGrindTriggerZone))
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
        base.OnGrindTriggerZoneStay(collider);
        if (CheckEPSCondition() && PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnEntityGrindTriggerZone))
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
        base.OnGrindTriggerZoneExit(collider);
        if (CheckEPSCondition() && PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnEntityGrindTriggerZone))
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
        if (CheckEPSCondition() && PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnBeforeDestroyEntity))
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
        if (CheckEPSCondition() && PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnDestroyEntity))
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
        if (CheckEPSCondition() && PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnBeforeMergeBox))
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
        if (CheckEPSCondition() && PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnMergeBox))
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
        base.OnDestroyEntityByElementDamage(entityBuffAttribute);
        if (CheckEPSCondition() && PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnDestroyEntityByElementDamage))
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

    public override void OnBeingFueled()
    {
        base.OnBeingFueled();
        if (CheckEPSCondition() && PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnBeingFueled))
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

    public override void OnEntityStatValueChange(EntityStatType entityStatType, int before, int after)
    {
        base.OnEntityStatValueChange(entityStatType, before, after);
        if (CheckEPSCondition() && PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnEntityStatValueChange))
        {
            if (EntityStatChangeType == entityStatType)
            {
                bool trigger = false;
                switch (EntityStatChangeThresholdType)
                {
                    case ValueChangeOverThresholdType.LE_to_G:
                    {
                        trigger = before <= EntityStatChangeThreshold && after > EntityStatChangeThreshold;
                        break;
                    }
                    case ValueChangeOverThresholdType.L_to_GE:
                    {
                        trigger = before < EntityStatChangeThreshold && after >= EntityStatChangeThreshold;
                        break;
                    }
                    case ValueChangeOverThresholdType.GE_to_L:
                    {
                        trigger = before >= EntityStatChangeThreshold && after < EntityStatChangeThreshold;
                        break;
                    }
                    case ValueChangeOverThresholdType.G_to_LE:
                    {
                        trigger = before > EntityStatChangeThreshold && after <= EntityStatChangeThreshold;
                        break;
                    }
                }

                if (trigger && TriggerProbabilityPercent.ProbabilityBool())
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

    public override void OnEntityPropertyValueChange(EntityPropertyType entityPropertyType, int before, int after)
    {
        base.OnEntityPropertyValueChange(entityPropertyType, before, after);
        if (CheckEPSCondition() && PassiveSkillCondition.HasFlag(PassiveSkillConditionType.OnEntityPropertyValueChange))
        {
            if (EntityPropertyChangeType == entityPropertyType)
            {
                bool trigger = false;
                switch (EntityPropertyChangeThresholdType)
                {
                    case ValueChangeOverThresholdType.LE_to_G:
                    {
                        trigger = before <= EntityPropertyChangeThreshold && after > EntityPropertyChangeThreshold;
                        break;
                    }
                    case ValueChangeOverThresholdType.L_to_GE:
                    {
                        trigger = before < EntityPropertyChangeThreshold && after >= EntityPropertyChangeThreshold;
                        break;
                    }
                    case ValueChangeOverThresholdType.GE_to_L:
                    {
                        trigger = before >= EntityPropertyChangeThreshold && after < EntityPropertyChangeThreshold;
                        break;
                    }
                    case ValueChangeOverThresholdType.G_to_LE:
                    {
                        trigger = before > EntityPropertyChangeThreshold && after <= EntityPropertyChangeThreshold;
                        break;
                    }
                }

                if (trigger && TriggerProbabilityPercent.ProbabilityBool())
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
                    epsa.Init(InitWorldModuleGUID);
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
                epsa.Init(InitWorldModuleGUID);
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

    protected override void ChildClone(EntitySkill cloneData)
    {
        base.ChildClone(cloneData);
        EntityPassiveSkill_Conditional newPSC = (EntityPassiveSkill_Conditional) cloneData;
        newPSC.TriggerProbabilityPercent = TriggerProbabilityPercent;

        newPSC.PassiveSkillCondition = PassiveSkillCondition;
        newPSC.DestroyEntityByElementDamageTypeList = DestroyEntityByElementDamageTypeList.Clone<EntityBuffAttribute, EntityBuffAttribute>();

        // Level Event Trigger
        newPSC.MultiEventTrigger = MultiEventTrigger;
        newPSC.ListenLevelEventAliasList = ListenLevelEventAliasList.Clone<string, string>();
        newPSC.ListenLevelEventAlias = ListenLevelEventAlias;
        newPSC.MaxTriggeredTimes = MaxTriggeredTimes;

        // OnEntityStatPropertyChange
        newPSC.EntityStatChangeType = EntityStatChangeType;
        newPSC.EntityStatChangeThreshold = EntityStatChangeThreshold;
        newPSC.EntityStatChangeThresholdType = EntityStatChangeThresholdType;
        newPSC.EntityPropertyChangeType = EntityPropertyChangeType;
        newPSC.EntityPropertyChangeThreshold = EntityPropertyChangeThreshold;
        newPSC.EntityPropertyChangeThresholdType = EntityPropertyChangeThresholdType;

        // EPSConditionals
        newPSC.EPSConditionals = EPSConditionals.Clone<EPSConditional, EPSConditional>();

        // Actions
        foreach (EntityPassiveSkillAction rawBoxPassiveSkillAction in RawEntityPassiveSkillActions)
        {
            EntityPassiveSkillAction newEPSA = rawBoxPassiveSkillAction.Clone();
            newPSC.RawEntityPassiveSkillActions.Add(newEPSA);
        }
    }

    public override void CopyDataFrom(EntitySkill srcData)
    {
        base.CopyDataFrom(srcData);
        EntityPassiveSkill_Conditional srcPSC = (EntityPassiveSkill_Conditional) srcData;
        PassiveSkillCondition = srcPSC.PassiveSkillCondition;
        DestroyEntityByElementDamageTypeList = srcPSC.DestroyEntityByElementDamageTypeList.Clone<EntityBuffAttribute, EntityBuffAttribute>();

        // Level Event Trigger
        TriggerProbabilityPercent = srcPSC.TriggerProbabilityPercent;
        MultiEventTrigger = srcPSC.MultiEventTrigger;
        ListenLevelEventAliasList.Clear();
        foreach (string s in srcPSC.ListenLevelEventAliasList)
        {
            ListenLevelEventAliasList.Add(s);
        }

        ListenLevelEventAlias = srcPSC.ListenLevelEventAlias;
        MaxTriggeredTimes = srcPSC.MaxTriggeredTimes;

        // OnEntityStatPropertyChange
        EntityStatChangeType = srcPSC.EntityStatChangeType;
        EntityStatChangeThreshold = srcPSC.EntityStatChangeThreshold;
        EntityStatChangeThresholdType = srcPSC.EntityStatChangeThresholdType;
        EntityPropertyChangeType = srcPSC.EntityPropertyChangeType;
        EntityPropertyChangeThreshold = srcPSC.EntityPropertyChangeThreshold;
        EntityPropertyChangeThresholdType = srcPSC.EntityPropertyChangeThresholdType;

        // EPSConditionals
        for (int i = 0; i < srcPSC.EPSConditionals.Count; i++)
        {
            EPSConditionals[i].CopyDataFrom(srcPSC.EPSConditionals[i]);
        }

        // Actions
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

[Serializable]
public class EPSConditional : IClone<EPSConditional>
{
    [EnumToggleButtons]
    [LabelText("状态或属性")]
    public ConditionType m_ConditionType = ConditionType.Stat;

    [LabelText("状态值种类")]
    [ShowIf("m_ConditionType", ConditionType.Stat)]
    public EntityStatType EntityStatType;

    [LabelText("状态值阈值")]
    [ShowIf("m_ConditionType", ConditionType.Stat)]
    public int EntityStatThreshold;

    private bool StatOrProperty => m_ConditionType == ConditionType.Stat || m_ConditionType == ConditionType.Property;

    [LabelText("属性值种类")]
    [ShowIf("m_ConditionType", ConditionType.Property)]
    public EntityPropertyType EntityPropertyType;

    [LabelText("属性值阈值")]
    [ShowIf("m_ConditionType", ConditionType.Property)]
    public int EntityPropertyThreshold;

    [HideLabel]
    [EnumToggleButtons]
    [ShowIf("StatOrProperty")]
    public Operator ThresholdOperator;

    [LabelText("战场状态")]
    [ShowIf("m_ConditionType", ConditionType.BattleStateBool)]
    public string BattleStateBool = "";

    [LabelText("满足条件持续时间/s")]
    public float SatisfiedDuration = 0f;

    private float satisfiedDurationTick = 0f;

    private Entity Entity;

    public void OnInit(Entity entity)
    {
        Entity = entity;
        satisfiedDurationTick = 0f;
    }

    public void OnTick(float tickInterval)
    {
        if (Check())
        {
            satisfiedDurationTick += tickInterval;
        }
        else
        {
            satisfiedDurationTick = 0;
        }
    }

    private bool Check()
    {
        switch (m_ConditionType)
        {
            case ConditionType.Stat:
            {
                EntityStat stat = Entity.EntityStatPropSet.StatDict[EntityStatType];
                bool trigger = false;
                switch (ThresholdOperator)
                {
                    case Operator.LessEquals:
                    {
                        trigger = stat.Value <= EntityStatThreshold;
                        break;
                    }
                    case Operator.Equals:
                    {
                        trigger = stat.Value == EntityStatThreshold;
                        break;
                    }
                    case Operator.GreaterEquals:
                    {
                        trigger = stat.Value >= EntityStatThreshold;
                        break;
                    }
                }

                return trigger;
            }
            case ConditionType.Property:
            {
                EntityProperty property = Entity.EntityStatPropSet.PropertyDict[EntityPropertyType];
                bool trigger = false;
                switch (ThresholdOperator)
                {
                    case Operator.LessEquals:
                    {
                        trigger = property.GetModifiedValue <= EntityStatThreshold;
                        break;
                    }
                    case Operator.Equals:
                    {
                        trigger = property.GetModifiedValue == EntityStatThreshold;
                        break;
                    }
                    case Operator.GreaterEquals:
                    {
                        trigger = property.GetModifiedValue >= EntityStatThreshold;
                        break;
                    }
                }

                return trigger;
            }
            case ConditionType.BattleStateBool:
            {
                return BattleManager.Instance.GetStateBool(BattleStateBool);
            }
        }

        return false;
    }

    public bool Condition => satisfiedDurationTick >= SatisfiedDuration;

    public EPSConditional Clone()
    {
        EPSConditional cloneData = new EPSConditional();
        cloneData.m_ConditionType = m_ConditionType;
        cloneData.EntityStatType = EntityStatType;
        cloneData.EntityStatThreshold = EntityStatThreshold;
        cloneData.EntityPropertyType = EntityPropertyType;
        cloneData.EntityPropertyThreshold = EntityPropertyThreshold;
        cloneData.ThresholdOperator = ThresholdOperator;
        cloneData.BattleStateBool = BattleStateBool;
        cloneData.SatisfiedDuration = SatisfiedDuration;
        return cloneData;
    }

    public void CopyDataFrom(EPSConditional srcData)
    {
        m_ConditionType = srcData.m_ConditionType;
        EntityStatType = srcData.EntityStatType;
        EntityStatThreshold = srcData.EntityStatThreshold;
        EntityPropertyType = srcData.EntityPropertyType;
        EntityPropertyThreshold = srcData.EntityPropertyThreshold;
        ThresholdOperator = srcData.ThresholdOperator;
        BattleStateBool = srcData.BattleStateBool;
        SatisfiedDuration = srcData.SatisfiedDuration;
    }
}

public enum Operator
{
    LessEquals,
    Equals,
    GreaterEquals,
}

public enum ValueChangeOverThresholdType
{
    LE_to_G,
    L_to_GE,
    GE_to_L,
    G_to_LE,
}

public enum ConditionType
{
    Stat = 0,
    Property = 1,
    BattleStateBool = 2,
}
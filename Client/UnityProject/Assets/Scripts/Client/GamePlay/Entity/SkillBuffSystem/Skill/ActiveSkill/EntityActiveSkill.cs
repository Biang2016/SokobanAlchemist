using System;
using System.Collections;
using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public abstract class EntityActiveSkill : EntitySkill
{
    internal EntityActiveSkill ParentActiveSkill;

    [LabelText("作用阵营")]
    public RelativeCamp TargetCamp;

    #region 绑定角色技能参数

    internal EntitySkillIndex EntitySkillIndex;

    [BoxGroup("技能参数")]
    [InlineProperty]
    [HideLabel]
    [SerializeField]
    public SkillPropertyCollection SkillsPropertyCollection;

    [LabelText("Miss时仍触发的概率%")]
    public int TriggerWhenMissProbabilityPercent = 100;

    protected int GetValue(EntitySkillPropertyType type)
    {
        return SkillsPropertyCollection.PropertyDict[type].GetModifiedValue;
    }

    #endregion

    #region 释放合法性判断条件

    [LabelText("释放合法性判断条件")]
    [SerializeReference]
    public List<EntitySkillCondition> EntitySkillConditions = new List<EntitySkillCondition>(); // 干数据，禁修改


    #endregion

    [LabelText("前摇可移动")]
    public bool WingUpCanMove;

    [LabelText("施法中可移动")]
    public bool CastCanMove;

    [LabelText("后摇可移动")]
    public bool RecoverCanMove;

    public bool CurrentAllowMove
    {
        get
        {
            switch (SkillPhase)
            {
                case ActiveSkillPhase.WingingUp:
                {
                    return WingUpCanMove;
                }
                case ActiveSkillPhase.Casting:
                {
                    return CastCanMove;
                }
                case ActiveSkillPhase.Recovering:
                {
                    return RecoverCanMove;
                }
            }

            return true;
        }
    }

    #region 子技能

    [SerializeReference]
    [LabelText("子技能列表")]
    public List<EntityActiveSkill> RawSubActiveSkillList = new List<EntityActiveSkill>(); // 干数据，运行时使用副本来触发技能

    private Dictionary<string, EntityActiveSkill> SubActiveSkillDict = new Dictionary<string, EntityActiveSkill>();

    [LabelText("子技能触发逻辑配置(依序执行)")]
    public List<SubActiveSkillTriggerLogic> SubActiveSkillTriggerLogicList = new List<SubActiveSkillTriggerLogic>();

    [LabelText("母技能被打断同时打断子技能")]
    public bool InterruptSubActiveSkillsWhenInterrupted;

    private List<EntityActiveSkill> RunningSubActiveSkillList = new List<EntityActiveSkill>();

    [Serializable]
    public class SubActiveSkillTriggerLogic : IClone<SubActiveSkillTriggerLogic> // 子技能触发逻辑配置
    {
        [LabelText("触发逻辑类型")]
        public TriggerType M_TriggerType;

        private bool NeedSequence => M_TriggerType == TriggerType.TriggerInSequence;
        private bool NeedProbabilityList => M_TriggerType == TriggerType.RandomTriggerTimeDuringPeriod || M_TriggerType == TriggerType.UniformMultipleTriggerDuringPeriod;
        private bool NeedDurationConfig => M_TriggerType == TriggerType.RandomTriggerTimeDuringPeriod || M_TriggerType == TriggerType.UniformMultipleTriggerDuringPeriod;
        private bool NeedTimesConfig => M_TriggerType == TriggerType.RandomTriggerTimeDuringPeriod || M_TriggerType == TriggerType.UniformMultipleTriggerDuringPeriod;

        [Serializable]
        public struct SkillTimePoint
        {
            [LabelText("子技能花名")]
            public string SubSkillAlias;

            [LabelText("子技能触发时间点ms")]
            public int SubActiveSkillTimePoint;
        }

        [LabelText("子技能顺序触发序列(按时间升序排列)")]
        [ShowIf("NeedSequence")]
        public List<SkillTimePoint> SubSkillTriggerSequence = new List<SkillTimePoint>();

        [LabelText("持续时长ms")]
        [ShowIf("NeedDurationConfig")]
        public int Duration;

        [LabelText("触发次数")]
        [ShowIf("NeedTimesConfig")]
        public int Times;

        [LabelText("子技能随机配比")]
        [ShowIf("NeedProbabilityList")]
        public List<SubSkillAliasWithProbability> SubSkillAliasWithProbabilityList = new List<SubSkillAliasWithProbability>();

        public enum TriggerType
        {
            [LabelText("按序列触发")]
            TriggerInSequence = 0, // 按序列触发

            [LabelText("持续时间内随机选取n个触发时间点播放随机子技能")]
            RandomTriggerTimeDuringPeriod = 1,

            [LabelText("持续时间内按相同时间间隔均匀播放n次随机子技能")]
            UniformMultipleTriggerDuringPeriod = 2,
        }

        public SubActiveSkillTriggerLogic Clone()
        {
            SubActiveSkillTriggerLogic newLogic = new SubActiveSkillTriggerLogic();
            newLogic.M_TriggerType = M_TriggerType;
            newLogic.SubSkillTriggerSequence = SubSkillTriggerSequence.Clone<SkillTimePoint, SkillTimePoint>();
            newLogic.Duration = Duration;
            newLogic.Times = Times;
            newLogic.SubSkillAliasWithProbabilityList = SubSkillAliasWithProbabilityList.Clone<SubSkillAliasWithProbability, SubSkillAliasWithProbability>();
            return newLogic;
        }

        public void CopyDataFrom(SubActiveSkillTriggerLogic src)
        {
            M_TriggerType = src.M_TriggerType;
            SubSkillTriggerSequence = src.SubSkillTriggerSequence.Clone<SkillTimePoint, SkillTimePoint>();
            Duration = src.Duration;
            Times = src.Times;
            if (SubSkillAliasWithProbabilityList.Count != src.SubSkillAliasWithProbabilityList.Count)
            {
                Debug.LogError("SubActiveSkillTriggerLogic CopyDataFrom SubSkillAliasWithProbabilityList数量不一致");
            }
            else
            {
                for (int i = 0; i < SubSkillAliasWithProbabilityList.Count; i++)
                {
                    SubSkillAliasWithProbabilityList[i].CopyDataFrom(src.SubSkillAliasWithProbabilityList[i]);
                }
            }
        }
    }

    #endregion

    [HideInInspector]
    public UnityAction<ActiveSkillPhase, ActiveSkillPhase> OnSkillPhaseChanged;

    private ActiveSkillPhase skillPhase;

    public ActiveSkillPhase SkillPhase
    {
        get { return skillPhase; }
        set
        {
            if (skillPhase != value)
            {
                skillPhase = value;
                OnSkillPhaseChanged?.Invoke(skillPhase, value);
            }
        }
    }

    internal float cooldownTimeTick = 0f;
    internal float currentExecutingCooldownTime = 0f; // 本次技能释放时取用的冷却时间

    internal UnityAction OnValidateFailed;
    internal UnityAction OnWingUpPhaseCompleteCallback;
    internal UnityAction OnCastPhaseCompleteCallback;
    internal UnityAction OnRecoveryPhaseCompleteCallback;
    internal UnityAction OnSkillFinishedCallback;

    public override void OnInit()
    {
        base.OnInit();
        OnSkillPhaseChanged = null;
        skillPhase = ActiveSkillPhase.Ready;

        cooldownTimeTick = 0;

        SkillsPropertyCollection.Init();
        if (SubActiveSkillDict == null) SubActiveSkillDict = new Dictionary<string, EntityActiveSkill>();
        SubActiveSkillDict.Clear();
        foreach (EntityActiveSkill subEAS in RawSubActiveSkillList)
        {
            if (string.IsNullOrWhiteSpace(subEAS.SkillAlias) || SubActiveSkillDict.ContainsKey(subEAS.SkillAlias))
            {
                Debug.LogError($"[{Entity.name}]的[{SkillAlias}]的子技能[{subEAS.SkillAlias}]花名为空或与其他子技能花名重复");
                continue;
            }

            SubActiveSkillDict.Add(subEAS.SkillAlias, subEAS);
            subEAS.Entity = Entity;
            subEAS.ParentActiveSkill = this;
            subEAS.OnInit();
        }

        foreach (EntitySkillCondition condition in EntitySkillConditions)
        {
            condition.OnInit(Entity);
        }

        if (RunningSubActiveSkillList == null) RunningSubActiveSkillList = new List<EntityActiveSkill>();
        RunningSubActiveSkillList.Clear();

        if (SubSkillManageCoroutines_CanNotInterrupt == null) SubSkillManageCoroutines_CanNotInterrupt = new List<Coroutine>();
        SubSkillManageCoroutines_CanNotInterrupt.Clear();
    }

    public override void OnUnInit()
    {
        base.OnUnInit();
        Interrupt();

        foreach (EntityActiveSkill subEAS in RawSubActiveSkillList)
        {
            subEAS.OnUnInit();
        }

        foreach (EntitySkillCondition condition in EntitySkillConditions)
        {
            condition.OnUnInit();
        }

        SkillsPropertyCollection.OnRecycled();

        SubActiveSkillDict.Clear();

        foreach (Coroutine coroutine in SubSkillManageCoroutines_CanNotInterrupt)
        {
            if (coroutine != null) ActiveSkillAgent.Instance.StopCoroutine(coroutine);
        }

        SubSkillManageCoroutines_CanNotInterrupt.Clear();

        foreach (EntityActiveSkill subEAS in RunningSubActiveSkillList)
        {
            subEAS.OnUnInit();
        }

        RunningSubActiveSkillList.Clear();

        Entity = null;
        ParentActiveSkill = null;

        OnValidateFailed = null;
        OnWingUpPhaseCompleteCallback = null;
        OnCastPhaseCompleteCallback = null;
        OnRecoveryPhaseCompleteCallback = null;
        OnSkillFinishedCallback = null;
    }

    public bool CheckCanTriggerSkill(TargetEntityType targetEntityType, int triggerWhenMissProbabilityPercent)
    {
        if (!ValidateSkill_CD()) return false;
        if (!ValidateSkillTrigger_Subject(targetEntityType)) return false;
        if (!ValidateSkillTrigger_Resources())
        {
            OnValidateFailed?.Invoke();
            return false;
        }
        else
        {
            if (!ValidateSkillTrigger_HitProbability(targetEntityType))
            {
                if (triggerWhenMissProbabilityPercent < 0) triggerWhenMissProbabilityPercent = TriggerWhenMissProbabilityPercent; // 如果不指定值则取技能默认值
                bool triggerAnyway = triggerWhenMissProbabilityPercent.ProbabilityBool(); // 当确定会miss时，有多少概率仍然发动
                if (!triggerAnyway) return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 触发技能的CD条件
    /// </summary>
    /// <returns></returns>
    protected virtual bool ValidateSkill_CD()
    {
        if (skillPhase != ActiveSkillPhase.Ready || SkillCoroutine != null) return false;
        return true;
    }

    /// <summary>
    /// 触发技能的主观条件
    /// </summary>
    /// <returns></returns>
    protected virtual bool ValidateSkillTrigger_Subject(TargetEntityType targetEntityType)
    {
        foreach (EntitySkillCondition condition in EntitySkillConditions)
        {
            if (condition is EntitySkillCondition.IPureCondition pureCondition)
            {
                if (!pureCondition.OnCheckCondition()) return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 触发技能的命中条件
    /// </summary>
    /// <returns></returns>
    protected virtual bool ValidateSkillTrigger_HitProbability(TargetEntityType targetEntityType)
    {
        PrepareSkillInfo(targetEntityType);
        bool hit = false;
        foreach (KeyValuePair<string, EntityActiveSkill> kv in SubActiveSkillDict)
        {
            hit |= kv.Value.ValidateSkillTrigger_HitProbability(targetEntityType);
            if (hit) return true;
        }

        return false;
    }

    /// <summary>
    /// 触发技能的资源条件
    /// </summary>
    /// <returns></returns>
    protected virtual bool ValidateSkillTrigger_Resources()
    {
        bool enoughResources = true;
        if (Entity.EntityStatPropSet.StatDict[EntityStatType.ActionPoint].Value < SkillsPropertyCollection.ConsumeActionPoint.GetModifiedValue)
        {
            Entity.EntityStatPropSet.StatDict[EntityStatType.ActionPoint].m_NotifyActionSet.OnValueNotEnoughWarning?.Invoke();
            enoughResources = false;
        }

        if (Entity.EntityStatPropSet.StatDict[EntityStatType.FireElementFragment].Value < SkillsPropertyCollection.ConsumeFireElementFragment.GetModifiedValue)
        {
            Entity.EntityStatPropSet.StatDict[EntityStatType.FireElementFragment].m_NotifyActionSet.OnValueNotEnoughWarning?.Invoke();
            enoughResources = false;
        }

        if (Entity.EntityStatPropSet.StatDict[EntityStatType.IceElementFragment].Value < SkillsPropertyCollection.ConsumeIceElementFragment.GetModifiedValue)
        {
            Entity.EntityStatPropSet.StatDict[EntityStatType.IceElementFragment].m_NotifyActionSet.OnValueNotEnoughWarning?.Invoke();
            enoughResources = false;
        }

        if (Entity.EntityStatPropSet.StatDict[EntityStatType.LightningElementFragment].Value < SkillsPropertyCollection.ConsumeLightningElementFragment.GetModifiedValue)
        {
            Entity.EntityStatPropSet.StatDict[EntityStatType.LightningElementFragment].m_NotifyActionSet.OnValueNotEnoughWarning?.Invoke();
            enoughResources = false;
        }

        return enoughResources;
    }

    public virtual void TriggerActiveSkill(TargetEntityType targetEntityType)
    {
        SkillCoroutine = Entity.StartCoroutine(Co_CastSkill(
            targetEntityType,
            GetValue(EntitySkillPropertyType.WingUp),
            GetValue(EntitySkillPropertyType.CastDuration),
            GetValue(EntitySkillPropertyType.Recovery),
            GetValue(EntitySkillPropertyType.Cooldown)));
    }

    private Coroutine SkillCoroutine;
    private Coroutine SubSkillManageCoroutine_CanInterrupt; // 技能被打断时或角色被销毁时停止该协程
    private List<Coroutine> SubSkillManageCoroutines_CanNotInterrupt = new List<Coroutine>(); // 角色被销毁时停止该协程

    // 技能进度，用于美术表现
    protected float WingUpRatio;
    protected float CastRatio;
    protected float RecoveryRatio;
    protected float CooldownRatio;

    protected GridPos3D CastEntityWorldGP; // 打出技能瞬间的世界坐标
    protected GridPosR.Orientation CastEntityOrientation; // 打出技能瞬间的朝向

    // 技能主协程
    IEnumerator Co_CastSkill(TargetEntityType targetEntityType, float wingUpTime, float castDuration, float recoveryTime, float cooldownTime)
    {
        currentExecutingCooldownTime = cooldownTime;
        PrepareSkillInfo(targetEntityType);
        yield return WingUp(wingUpTime);
        OnWingUpPhaseComplete();
        OnWingUpPhaseCompleteCallback?.Invoke();
        yield return Cast(targetEntityType, castDuration);
        OnCastPhaseComplete();
        OnCastPhaseCompleteCallback?.Invoke();
        yield return Recover(recoveryTime);
        OnRecoveryPhaseComplete();
        OnRecoveryPhaseCompleteCallback?.Invoke();
        SkillPhase = ActiveSkillPhase.CoolingDown;
        SkillCoroutine = null;
        OnSkillFinishedCallback?.Invoke();
    }

    protected virtual void PrepareSkillInfo(TargetEntityType targetEntityType)
    {
        if (ParentActiveSkill != null)
        {
            CastEntityWorldGP = ParentActiveSkill.CastEntityWorldGP;
            CastEntityOrientation = ParentActiveSkill.CastEntityOrientation;
        }
        else
        {
            CastEntityWorldGP = Entity.WorldGP;
            CastEntityOrientation = Entity.EntityOrientation;
        }
    }

    protected virtual IEnumerator WingUp(float wingUpTime)
    {
        Entity.EntityStatPropSet.StatDict[EntityStatType.ActionPoint].SetValue(Entity.EntityStatPropSet.StatDict[EntityStatType.ActionPoint].Value - SkillsPropertyCollection.ConsumeActionPoint.GetModifiedValue);
        Entity.EntityStatPropSet.StatDict[EntityStatType.FireElementFragment].SetValue(Entity.EntityStatPropSet.StatDict[EntityStatType.FireElementFragment].Value - SkillsPropertyCollection.ConsumeFireElementFragment.GetModifiedValue);
        Entity.EntityStatPropSet.StatDict[EntityStatType.IceElementFragment].SetValue(Entity.EntityStatPropSet.StatDict[EntityStatType.IceElementFragment].Value - SkillsPropertyCollection.ConsumeIceElementFragment.GetModifiedValue);
        Entity.EntityStatPropSet.StatDict[EntityStatType.LightningElementFragment].SetValue(Entity.EntityStatPropSet.StatDict[EntityStatType.LightningElementFragment].Value - SkillsPropertyCollection.ConsumeLightningElementFragment.GetModifiedValue);

        Entity.EntityWwiseHelper.OnSkillPreparing[(int) EntitySkillIndex].Post(Entity.gameObject);
        SkillPhase = ActiveSkillPhase.WingingUp;
        // todo Entity 前摇animation， 且按时间缩放
        WingUpRatio = 0;
        float wingUpTick = 0f;
        while (wingUpTick < wingUpTime / 1000f)
        {
            wingUpTick += Time.deltaTime;
            WingUpRatio = wingUpTick / (wingUpTime / 1000f);
            yield return null;
        }

        WingUpRatio = 0;
    }

    public virtual void OnWingUpPhaseComplete()
    {
    }

    protected virtual IEnumerator Cast(TargetEntityType targetEntityType, float castDuration)
    {
        Entity.EntityWwiseHelper.OnSkillCast[(int) EntitySkillIndex].Post(Entity.gameObject);
        SkillPhase = ActiveSkillPhase.Casting;
        CastRatio = 0;
        // todo Entity 施法animation， 且按时间缩放

        if (RawSubActiveSkillList.Count > 0)
        {
            if (InterruptSubActiveSkillsWhenInterrupted)
            {
                SubSkillManageCoroutine_CanInterrupt = Entity.StartCoroutine(CastSubActiveSkills(targetEntityType));
            }
            else
            {
                if (ActiveSkillAgent.Instance != null)
                {
                    SubSkillManageCoroutines_CanNotInterrupt.Add(ActiveSkillAgent.Instance.StartCoroutine(CastSubActiveSkills(targetEntityType)));
                }
            }
        }

        float castTick = 0f;
        while (castTick < castDuration / 1000f)
        {
            castTick += Time.deltaTime;
            CastRatio = castTick / (castDuration / 1000f);
            yield return null;
        }

        cooldownTimeTick = currentExecutingCooldownTime; // 从施法完毕瞬间开始计算冷却
        CastRatio = 0;
    }

    protected virtual IEnumerator CastSubActiveSkills(TargetEntityType targetEntityType)
    {
        void CloneSubSkillAndTrigger(EntityActiveSkill subSkill)
        {
            EntityActiveSkill subSkillClone = (EntityActiveSkill) subSkill.Clone();
            RunningSubActiveSkillList.Add(subSkillClone);
            subSkillClone.OnValidateFailed += () =>
            {
                RunningSubActiveSkillList.Remove(subSkillClone);
                subSkillClone.OnUnInit();
            };
            subSkillClone.OnSkillFinishedCallback += () =>
            {
                RunningSubActiveSkillList.Remove(subSkillClone);
                subSkillClone.OnUnInit();
            };

            subSkillClone.Entity = subSkill.Entity;
            subSkillClone.ParentActiveSkill = subSkill.ParentActiveSkill;
            subSkillClone.OnInit();
            if (subSkillClone.CheckCanTriggerSkill(targetEntityType, -1))
            {
                subSkillClone.TriggerActiveSkill(targetEntityType);
            }
        }

        foreach (SubActiveSkillTriggerLogic logic in SubActiveSkillTriggerLogicList)
        {
            switch (logic.M_TriggerType)
            {
                case SubActiveSkillTriggerLogic.TriggerType.TriggerInSequence:
                {
                    float timeTick = 0f;
                    foreach (SubActiveSkillTriggerLogic.SkillTimePoint timePoint in logic.SubSkillTriggerSequence)
                    {
                        while (timeTick < timePoint.SubActiveSkillTimePoint / 1000f)
                        {
                            yield return null;
                            timeTick += Time.deltaTime;
                        }

                        if (SubActiveSkillDict.TryGetValue(timePoint.SubSkillAlias, out EntityActiveSkill subSkill))
                        {
                            CloneSubSkillAndTrigger(subSkill);
                        }
                        else
                        {
                            Debug.LogError($"[子技能顺序触发序列] 未配置花名为[{timePoint.SubSkillAlias}]的子技能");
                        }
                    }

                    break;
                }
                case SubActiveSkillTriggerLogic.TriggerType.RandomTriggerTimeDuringPeriod:
                {
                    float timeTick = 0f;
                    List<SubSkillAliasWithProbability> randomResults = CommonUtils.GetRandomWithProbabilityFromList(logic.SubSkillAliasWithProbabilityList, logic.Times);
                    List<float> randomTriggerTimePoints = new List<float>();
                    for (int i = 0; i < randomResults.Count; i++)
                    {
                        randomTriggerTimePoints.Add(UnityEngine.Random.Range(0, logic.Duration));
                    }

                    for (int index = 0; index < randomResults.Count; index++)
                    {
                        SubSkillAliasWithProbability skillAlias = randomResults[index];
                        float triggerTimePoint = randomTriggerTimePoints[index];
                        while (timeTick < triggerTimePoint)
                        {
                            yield return null;
                            timeTick += Time.deltaTime;
                        }

                        if (SubActiveSkillDict.TryGetValue(skillAlias.SubSkillAlias, out EntityActiveSkill subSkill))
                        {
                            CloneSubSkillAndTrigger(subSkill);
                        }
                        else
                        {
                            Debug.LogError($"[子技能随机触发序列] 未配置花名为[{skillAlias.SubSkillAlias}]的子技能");
                        }
                    }

                    break;
                }
                case SubActiveSkillTriggerLogic.TriggerType.UniformMultipleTriggerDuringPeriod:
                {
                    float interval = logic.Duration / 1000f / logic.Times;
                    WaitForSeconds wfs = new WaitForSeconds(interval);
                    List<SubSkillAliasWithProbability> randomResults = CommonUtils.GetRandomWithProbabilityFromList(logic.SubSkillAliasWithProbabilityList, logic.Times);
                    foreach (SubSkillAliasWithProbability skillAlias in randomResults)
                    {
                        if (SubActiveSkillDict.TryGetValue(skillAlias.SubSkillAlias, out EntityActiveSkill subSkill))
                        {
                            CloneSubSkillAndTrigger(subSkill);
                            yield return wfs;
                        }
                        else
                        {
                            Debug.LogError($"[子技能均匀随机触发序列] 未配置花名为[{skillAlias.SubSkillAlias}]的子技能");
                        }
                    }

                    break;
                }
            }
        }

        if (InterruptSubActiveSkillsWhenInterrupted) SubSkillManageCoroutine_CanInterrupt = null;
    }

    public virtual void OnCastPhaseComplete()
    {
        int cameraShakeEquivalentDamage = GetValue(EntitySkillPropertyType.CameraShakeEquivalentDamage);
        if (cameraShakeEquivalentDamage > 0)
        {
            CameraManager.Instance.FieldCamera.CameraShake(cameraShakeEquivalentDamage, (Entity.transform.position - BattleManager.Instance.Player1.transform.position).magnitude);
        }
    }

    protected virtual IEnumerator Recover(float recoveryTime)
    {
        SkillPhase = ActiveSkillPhase.Recovering;
        RecoveryRatio = 0;
        // todo Entity 后摇animation， 且按时间缩放
        float recoveryTick = 0f;
        while (recoveryTick < recoveryTime / 1000f)
        {
            recoveryTick += Time.deltaTime;
            RecoveryRatio = recoveryTick / (recoveryTime / 1000f);
            yield return null;
        }

        RecoveryRatio = 0;
    }

    public virtual void OnRecoveryPhaseComplete()
    {
    }

    // 被打断时，进打断当前母技能的主协程及子技能管理携程，已释放的子技能不打断，未释放的子技能由于子技能管理携程被打断无法发出
    public virtual void Interrupt()
    {
        if (SkillCoroutine != null)
        {
            Entity.StopCoroutine(SkillCoroutine);
            SkillCoroutine = null;
        }

        if (SubSkillManageCoroutine_CanInterrupt != null) Entity.StopCoroutine(SubSkillManageCoroutine_CanInterrupt);
        switch (SkillPhase)
        {
            case ActiveSkillPhase.Ready:
            {
                cooldownTimeTick = 0f;
                SkillPhase = ActiveSkillPhase.Ready;
                break;
            }
            case ActiveSkillPhase.WingingUp:
            {
                cooldownTimeTick = 0f;
                SkillPhase = ActiveSkillPhase.Ready;
                break;
            }
            case ActiveSkillPhase.Casting:
            {
                cooldownTimeTick = currentExecutingCooldownTime;
                SkillPhase = ActiveSkillPhase.CoolingDown;
                break;
            }
            case ActiveSkillPhase.Recovering:
            {
                SkillPhase = ActiveSkillPhase.CoolingDown;
                break;
            }
            case ActiveSkillPhase.CoolingDown:
            {
                break;
            }
        }

        // todo Entity reset animation
        OnRecoveryPhaseComplete();
    }

    public virtual void OnFixedUpdate(float fixedDeltaTime)
    {
        foreach (EntityActiveSkill subEAS in RunningSubActiveSkillList)
        {
            subEAS.OnFixedUpdate(fixedDeltaTime);
        }

        if (skillPhase == ActiveSkillPhase.Recovering || skillPhase == ActiveSkillPhase.CoolingDown)
        {
            cooldownTimeTick -= fixedDeltaTime * 1000f;
            CooldownRatio = cooldownTimeTick / currentExecutingCooldownTime;
            // 冷却时间结束，且目前后摇已完成并进入冷却阶段，才能将状态置为Ready
            if (cooldownTimeTick <= 0 && skillPhase == ActiveSkillPhase.CoolingDown)
            {
                cooldownTimeTick = 0;
                SkillPhase = ActiveSkillPhase.Ready;
            }
        }
    }

    public override void OnTick(float tickInterval)
    {
        foreach (EntityActiveSkill subEAS in RunningSubActiveSkillList)
        {
            subEAS.OnTick(tickInterval);
        }

        foreach (EntitySkillCondition condition in EntitySkillConditions)
        {
            condition.OnTick(tickInterval);
        }
    }

    protected override void ChildClone(EntitySkill cloneData)
    {
        base.ChildClone(cloneData);
        EntityActiveSkill newEAS = (EntityActiveSkill) cloneData;
        newEAS.SkillsPropertyCollection = SkillsPropertyCollection.Clone();
        newEAS.TargetCamp = TargetCamp;
        newEAS.TriggerWhenMissProbabilityPercent = TriggerWhenMissProbabilityPercent;
        newEAS.EntitySkillConditions = EntitySkillConditions.Clone<EntitySkillCondition, EntitySkillCondition>();
        newEAS.WingUpCanMove = WingUpCanMove;
        newEAS.CastCanMove = CastCanMove;
        newEAS.RecoverCanMove = RecoverCanMove;
        //newEAS.EntitySkillIndex = EntitySkillIndex; // 这条不抄，没有意义
        newEAS.RawSubActiveSkillList = RawSubActiveSkillList.Clone<EntityActiveSkill, EntitySkill>();
        newEAS.SubActiveSkillTriggerLogicList = SubActiveSkillTriggerLogicList.Clone<SubActiveSkillTriggerLogic, SubActiveSkillTriggerLogic>();
        newEAS.InterruptSubActiveSkillsWhenInterrupted = InterruptSubActiveSkillsWhenInterrupted;
    }

    public override void CopyDataFrom(EntitySkill srcData)
    {
        EntityActiveSkill srcEAS = (EntityActiveSkill) srcData;
        srcEAS.SkillsPropertyCollection.ApplyDataTo(SkillsPropertyCollection);
        TargetCamp = srcEAS.TargetCamp;
        TriggerWhenMissProbabilityPercent = srcEAS.TriggerWhenMissProbabilityPercent;
        if (EntitySkillConditions.Count != srcEAS.EntitySkillConditions.Count)
        {
            Debug.LogError("EAS CopyDataFrom EntitySkillConditions数量不一致");
        }
        else
        {
            for (int i = 0; i < EntitySkillConditions.Count; i++)
            {
                EntitySkillConditions[i].CopyDataFrom(srcEAS.EntitySkillConditions[i]);
            }
        }

        WingUpCanMove = srcEAS.WingUpCanMove;
        CastCanMove = srcEAS.CastCanMove;
        RecoverCanMove = srcEAS.RecoverCanMove;
        //EntitySkillIndex = srcEAS.EntitySkillIndex; // 这条不抄！！！
        if (RawSubActiveSkillList.Count != srcEAS.RawSubActiveSkillList.Count)
        {
            Debug.LogError("EAS CopyDataFrom RawSubActiveSkillList数量不一致");
        }
        else
        {
            for (int i = 0; i < RawSubActiveSkillList.Count; i++)
            {
                RawSubActiveSkillList[i].CopyDataFrom(srcEAS.RawSubActiveSkillList[i]);
            }
        }

        if (SubActiveSkillTriggerLogicList.Count != srcEAS.SubActiveSkillTriggerLogicList.Count)
        {
            Debug.LogError("EAS CopyDataFrom SubActiveSkillTriggerLogicList数量不一致");
        }
        else
        {
            for (int i = 0; i < SubActiveSkillTriggerLogicList.Count; i++)
            {
                SubActiveSkillTriggerLogicList[i].CopyDataFrom(srcEAS.SubActiveSkillTriggerLogicList[i]);
            }
        }

        InterruptSubActiveSkillsWhenInterrupted = srcEAS.InterruptSubActiveSkillsWhenInterrupted;
    }
}

public enum ActiveSkillPhase
{
    CoolingDown,
    Ready,
    WingingUp,
    Casting,
    Recovering,
}
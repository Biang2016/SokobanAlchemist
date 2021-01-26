using System;
using System.Collections;
using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public abstract class EntityActiveSkill : IClone<EntityActiveSkill>
{
    internal EntityActiveSkill ParentActiveSkill;
    internal Entity Entity;

    [LabelText("作用阵营")]
    public RelativeCamp TargetCamp;

    #region 绑定角色技能参数

    // 本技能为母技能时，该编号必须和其他母技能互斥
    // 子技能不受此限制，可绑定任意技能参数
    [LabelText("绑定技能号")]
    public EntitySkillIndex EntitySkillIndex;

    internal Dictionary<EntitySkillPropertyType, EntityPropertyValue> EntityPropertyValueDict = new Dictionary<EntitySkillPropertyType, EntityPropertyValue>();

    protected int GetValue(EntitySkillPropertyType type)
    {
        return EntityPropertyValueDict[type].Value;
    }

    public class EntityPropertyValue
    {
        public int Value;

        public void OnValueChangedHandle(int before, int after)
        {
            Value = after;
        }
    }

    protected void BindEntityProperty(EntityPropertyValue epv, EntitySkillPropertyType entitySkillPropertyType)
    {
        EntityProperty ep = Entity.EntityStatPropSet.SkillsPropertyCollections[(int) EntitySkillIndex].PropertyDict[entitySkillPropertyType];
        epv.Value = ep.GetModifiedValue;
        ep.OnValueChanged += epv.OnValueChangedHandle;
    }

    protected void UnBindActorProperty(EntityPropertyValue epv, EntitySkillPropertyType actorSkillPropertyType)
    {
        EntityProperty ep = Entity.EntityStatPropSet.SkillsPropertyCollections[(int) EntitySkillIndex].PropertyDict[actorSkillPropertyType];
        ep.OnValueChanged -= epv.OnValueChangedHandle;
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

    [LabelText("技能描述")]
    [ShowInInspector]
    [PropertyOrder(-1)]
    protected abstract string Description { get; }

    [LabelText("技能花名")]
    [InfoBox("必填且不得与并列技能重复")]
    [Required]
    public string SkillAlias;

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
    [FoldoutGroup("子技能")]
    [LabelText("子技能列表")]
    public List<EntityActiveSkill> RawSubActiveSkillList = new List<EntityActiveSkill>(); // 干数据，运行时使用副本来触发技能

    private Dictionary<string, EntityActiveSkill> SubActiveSkillDict = new Dictionary<string, EntityActiveSkill>();

    [FoldoutGroup("子技能")]
    [LabelText("子技能触发逻辑配置(依序执行)")]
    public List<SubActiveSkillTriggerLogic> SubActiveSkillTriggerLogicList = new List<SubActiveSkillTriggerLogic>();

    [FoldoutGroup("子技能")]
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
            newLogic.SubSkillTriggerSequence = SubSkillTriggerSequence.Clone();
            newLogic.Duration = Duration;
            newLogic.Times = Times;
            newLogic.SubSkillAliasWithProbabilityList = SubSkillAliasWithProbabilityList.Clone();
            return newLogic;
        }
    }

    #endregion

    internal UnityAction OnValidateFailed;
    internal UnityAction OnWingUpPhaseCompleteCallback;
    internal UnityAction OnCastPhaseCompleteCallback;
    internal UnityAction OnRecoveryPhaseCompleteCallback;
    internal UnityAction OnSkillFinishedCallback;

    public virtual void OnInit()
    {
        OnSkillPhaseChanged = null;
        skillPhase = ActiveSkillPhase.Ready;

        cooldownTimeTick = 0;

        EntityPropertyValueDict.Clear();
        foreach (EntitySkillPropertyType espt in Enum.GetValues(typeof(EntitySkillPropertyType)))
        {
            EntityPropertyValue apv = new EntityPropertyValue();
            BindEntityProperty(apv, espt);
            EntityPropertyValueDict.Add(espt, apv);
        }

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

        RunningSubActiveSkillList.Clear();

        SubSkillManageCoroutines_CanNotInterrupt.Clear();
    }

    public virtual void OnUnInit()
    {
        Interrupt();

        foreach (EntityActiveSkill subEAS in RawSubActiveSkillList)
        {
            subEAS.OnUnInit();
        }

        foreach (KeyValuePair<EntitySkillPropertyType, EntityPropertyValue> kv in EntityPropertyValueDict)
        {
            UnBindActorProperty(kv.Value, kv.Key);
        }

        SubActiveSkillDict.Clear();

        foreach (EntityActiveSkill subEAS in RunningSubActiveSkillList)
        {
            subEAS.OnUnInit();
        }

        RunningSubActiveSkillList.Clear();

        foreach (Coroutine coroutine in SubSkillManageCoroutines_CanNotInterrupt)
        {
            if (coroutine != null) ActiveSkillAgent.Instance.StopCoroutine(coroutine);
        }

        SubSkillManageCoroutines_CanNotInterrupt.Clear();

        Entity = null;
        ParentActiveSkill = null;

        OnValidateFailed = null;
        OnWingUpPhaseCompleteCallback = null;
        OnCastPhaseCompleteCallback = null;
        OnRecoveryPhaseCompleteCallback = null;
        OnSkillFinishedCallback = null;
    }

    public virtual bool TriggerActiveSkill()
    {
        if (!ValidateSkillTrigger())
        {
            OnValidateFailed?.Invoke();
            return false;
        }

        SkillCoroutine = Entity.StartCoroutine(Co_CastSkill(
            GetValue(EntitySkillPropertyType.WingUp),
            GetValue(EntitySkillPropertyType.CastDuration),
            GetValue(EntitySkillPropertyType.Recovery),
            GetValue(EntitySkillPropertyType.Cooldown)));
        return true;
    }

    protected virtual bool ValidateSkillTrigger()
    {
        if (skillPhase != ActiveSkillPhase.Ready || SkillCoroutine != null) return false;
        return true;
    }

    private Coroutine SkillCoroutine;
    private Coroutine SubSkillManageCoroutine_CanInterrupt; // 技能被打断时或角色被销毁时停止该协程
    private List<Coroutine> SubSkillManageCoroutines_CanNotInterrupt = new List<Coroutine>(); // 角色被销毁时停止该协程

    // 技能进度，用于美术表现
    protected float WingUpRatio;
    protected float CastRatio;
    protected float RecoveryRatio;
    protected float CooldownRatio;

    // 技能主协程
    IEnumerator Co_CastSkill(float wingUpTime, float castDuration, float recoveryTime, float cooldownTime)
    {
        currentExecutingCooldownTime = cooldownTime;
        PrepareSkillInfo();
        yield return WingUp(wingUpTime);
        OnWingUpPhaseComplete();
        OnWingUpPhaseCompleteCallback?.Invoke();
        yield return Cast(castDuration);
        OnCastPhaseComplete();
        OnCastPhaseCompleteCallback?.Invoke();
        yield return Recover(recoveryTime);
        OnRecoveryPhaseComplete();
        OnRecoveryPhaseCompleteCallback?.Invoke();
        SkillPhase = ActiveSkillPhase.CoolingDown;
        SkillCoroutine = null;
        OnSkillFinishedCallback?.Invoke();
    }

    protected virtual void PrepareSkillInfo()
    {
    }

    protected virtual IEnumerator WingUp(float wingUpTime)
    {
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

    protected virtual IEnumerator Cast(float castDuration)
    {
        SkillPhase = ActiveSkillPhase.Casting;
        CastRatio = 0;
        // todo Entity 施法animation， 且按时间缩放

        if (RawSubActiveSkillList.Count > 0)
        {
            if (InterruptSubActiveSkillsWhenInterrupted)
            {
                SubSkillManageCoroutine_CanInterrupt = Entity.StartCoroutine(CastSubActiveSkills());
            }
            else
            {
                SubSkillManageCoroutines_CanNotInterrupt.Add(ActiveSkillAgent.Instance.StartCoroutine(CastSubActiveSkills()));
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

    protected virtual IEnumerator CastSubActiveSkills()
    {
        void CloneSubSkillAndTrigger(EntityActiveSkill subSkill)
        {
            EntityActiveSkill subSkillClone = subSkill.Clone();
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
            subSkillClone.Entity = Entity;
            subSkillClone.ParentActiveSkill = ParentActiveSkill;
            subSkillClone.OnInit();
            subSkillClone.TriggerActiveSkill();
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
        if (SkillCoroutine != null) Entity.StopCoroutine(SkillCoroutine);
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

    public virtual void OnTick(float tickDeltaTime)
    {
        foreach (EntityActiveSkill subEAS in RunningSubActiveSkillList)
        {
            subEAS.OnTick(tickDeltaTime);
        }
    }

    public EntityActiveSkill Clone()
    {
        Type type = GetType();
        EntityActiveSkill newEAS = (EntityActiveSkill) Activator.CreateInstance(type);
        newEAS.TargetCamp = TargetCamp;
        newEAS.EntitySkillIndex = EntitySkillIndex;
        newEAS.SkillAlias = SkillAlias;
        newEAS.RawSubActiveSkillList = RawSubActiveSkillList.Clone();
        newEAS.SubActiveSkillTriggerLogicList = SubActiveSkillTriggerLogicList.Clone();
        newEAS.InterruptSubActiveSkillsWhenInterrupted = InterruptSubActiveSkillsWhenInterrupted;
        ChildClone(newEAS);
        return newEAS;
    }

    protected virtual void ChildClone(EntityActiveSkill cloneData)
    {
    }

    public virtual void CopyDataFrom(EntityActiveSkill srcData)
    {
        TargetCamp = srcData.TargetCamp;
        EntitySkillIndex = srcData.EntitySkillIndex;
        SkillAlias = srcData.SkillAlias;
        RawSubActiveSkillList = srcData.RawSubActiveSkillList.Clone();
        SubActiveSkillTriggerLogicList = srcData.SubActiveSkillTriggerLogicList.Clone();
        InterruptSubActiveSkillsWhenInterrupted = srcData.InterruptSubActiveSkillsWhenInterrupted;
    }

    #region Utils

    private IEnumerable<string> GetAllBoxTypeNames => ConfigManager.GetAllBoxTypeNames();

    private IEnumerable<string> GetAllEnemyNames => ConfigManager.GetAllEnemyNames();

    private IEnumerable<string> GetAllFXTypeNames => ConfigManager.GetAllFXTypeNames();

    private IEnumerable<string> GetAllBattleIndicatorNames => ConfigManager.GetAllBattleIndicatorTypeNames();

    #endregion
}

public enum ActiveSkillPhase
{
    CoolingDown,
    Ready,
    WingingUp,
    Casting,
    Recovering,
}
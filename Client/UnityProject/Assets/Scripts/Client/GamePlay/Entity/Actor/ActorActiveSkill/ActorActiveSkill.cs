using System;
using System.Collections;
using System.Collections.Generic;
using BiangStudio;
using BiangStudio.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public abstract class ActorActiveSkill : IClone<ActorActiveSkill>
{
    internal ActorActiveSkill ParentActiveSkill;
    internal Actor Actor;

    [LabelText("作用阵营")]
    public RelativeCamp TargetCamp;

    #region 绑定角色技能参数

    // 本技能为母技能时，该编号必须和其他母技能互斥
    // 子技能不受此限制，可绑定任意技能参数
    [LabelText("绑定角色技能号")]
    public ActorSkillIndex ActorSkillIndex;

    internal Dictionary<ActorSkillPropertyType, ActorPropertyValue> ActorPropertyValueDict = new Dictionary<ActorSkillPropertyType, ActorPropertyValue>();

    protected int GetValue(ActorSkillPropertyType type)
    {
        return ActorPropertyValueDict[type].Value;
    }

    public class ActorPropertyValue
    {
        public int Value;

        public void OnValueChangedHandle(int before, int after)
        {
            Value = after;
        }
    }

    protected void BindActorProperty(ActorPropertyValue apv, ActorSkillPropertyType actorSkillPropertyType)
    {
        ActorProperty ap = Actor.ActorStatPropSet.SkillsPropertyCollections[(int) ActorSkillIndex].PropertyDict[actorSkillPropertyType];
        apv.Value = ap.GetModifiedValue;
        ap.OnValueChanged += apv.OnValueChangedHandle;
    }

    protected void UnBindActorProperty(ActorPropertyValue apv, ActorSkillPropertyType actorSkillPropertyType)
    {
        ActorProperty ap = Actor.ActorStatPropSet.SkillsPropertyCollections[(int) ActorSkillIndex].PropertyDict[actorSkillPropertyType];
        ap.OnValueChanged -= apv.OnValueChangedHandle;
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

    [FoldoutGroup("子技能")]
    [LabelText("子技能列表")]
    public List<ActorActiveSkill> RawSubActiveSkillList = new List<ActorActiveSkill>(); // 干数据，运行时使用副本来触发技能

    private Dictionary<string, ActorActiveSkill> SubActiveSkillDict = new Dictionary<string, ActorActiveSkill>();

    [FoldoutGroup("子技能")]
    [LabelText("子技能触发逻辑配置(依序执行)")]
    public List<SubActiveSkillTriggerLogic> SubActiveSkillTriggerLogicList = new List<SubActiveSkillTriggerLogic>();

    [FoldoutGroup("子技能")]
    [LabelText("母技能被打断同时打断子技能")]
    public bool InterruptSubActiveSkillsWhenInterrupted;

    private List<ActorActiveSkill> RunningSubActiveSkillList = new List<ActorActiveSkill>();

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

        ActorPropertyValueDict.Clear();
        foreach (ActorSkillPropertyType aspt in Enum.GetValues(typeof(ActorSkillPropertyType)))
        {
            ActorPropertyValue apv = new ActorPropertyValue();
            BindActorProperty(apv, aspt);
            ActorPropertyValueDict.Add(aspt, apv);
        }

        SubActiveSkillDict.Clear();
        foreach (ActorActiveSkill subAAS in RawSubActiveSkillList)
        {
            if (string.IsNullOrWhiteSpace(subAAS.SkillAlias) || SubActiveSkillDict.ContainsKey(subAAS.SkillAlias))
            {
                Debug.LogError($"{SkillAlias}的子技能[{subAAS.SkillAlias}]花名为空或与其他子技能花名重复");
                continue;
            }

            SubActiveSkillDict.Add(subAAS.SkillAlias, subAAS);
            subAAS.Actor = Actor;
            subAAS.ParentActiveSkill = this;
            subAAS.OnInit();
        }

        RunningSubActiveSkillList.Clear();

        SubSkillManageCoroutines_CanNotInterrupt.Clear();
    }

    public virtual void OnUnInit()
    {
        Interrupt();

        foreach (ActorActiveSkill subAAS in RawSubActiveSkillList)
        {
            subAAS.OnUnInit();
        }

        foreach (KeyValuePair<ActorSkillPropertyType, ActorPropertyValue> kv in ActorPropertyValueDict)
        {
            UnBindActorProperty(kv.Value, kv.Key);
        }

        SubActiveSkillDict.Clear();

        foreach (ActorActiveSkill subAAS in RunningSubActiveSkillList)
        {
            subAAS.OnUnInit();
        }

        RunningSubActiveSkillList.Clear();

        foreach (Coroutine coroutine in SubSkillManageCoroutines_CanNotInterrupt)
        {
            if (coroutine != null) ActiveSkillAgent.Instance.StopCoroutine(coroutine);
        }

        SubSkillManageCoroutines_CanNotInterrupt.Clear();

        Actor = null;
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

        SkillCoroutine = Actor.StartCoroutine(Co_CastSkill(
            GetValue(ActorSkillPropertyType.WingUp),
            GetValue(ActorSkillPropertyType.CastDuration),
            GetValue(ActorSkillPropertyType.Recovery),
            GetValue(ActorSkillPropertyType.Cooldown)));
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
        // todo Actor 前摇animation， 且按时间缩放
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
        // todo Actor 施法animation， 且按时间缩放

        if (RawSubActiveSkillList.Count > 0)
        {
            if (InterruptSubActiveSkillsWhenInterrupted)
            {
                SubSkillManageCoroutine_CanInterrupt = Actor.StartCoroutine(CastSubActiveSkills());
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
        void CloneSubSkillAndTrigger(ActorActiveSkill subSkill)
        {
            ActorActiveSkill subSkillClone = subSkill.Clone();
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
            subSkillClone.Actor = Actor;
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

                        if (SubActiveSkillDict.TryGetValue(timePoint.SubSkillAlias, out ActorActiveSkill subSkill))
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

                        if (SubActiveSkillDict.TryGetValue(skillAlias.SubSkillAlias, out ActorActiveSkill subSkill))
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
                        if (SubActiveSkillDict.TryGetValue(skillAlias.SubSkillAlias, out ActorActiveSkill subSkill))
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
        // todo Actor 后摇animation， 且按时间缩放
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
        if (SkillCoroutine != null) Actor.StopCoroutine(SkillCoroutine);
        if (SubSkillManageCoroutine_CanInterrupt != null) Actor.StopCoroutine(SubSkillManageCoroutine_CanInterrupt);

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

        // todo Actor reset animation
        OnRecoveryPhaseComplete();
    }

    public virtual void OnFixedUpdate(float fixedDeltaTime)
    {
        foreach (ActorActiveSkill subAAS in RunningSubActiveSkillList)
        {
            subAAS.OnFixedUpdate(fixedDeltaTime);
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
        foreach (ActorActiveSkill subAAS in RunningSubActiveSkillList)
        {
            subAAS.OnTick(tickDeltaTime);
        }
    }

    public ActorActiveSkill Clone()
    {
        Type type = GetType();
        ActorActiveSkill newAS = (ActorActiveSkill) Activator.CreateInstance(type);
        newAS.TargetCamp = TargetCamp;
        newAS.ActorSkillIndex = ActorSkillIndex;
        newAS.SkillAlias = SkillAlias;
        newAS.RawSubActiveSkillList = RawSubActiveSkillList.Clone();
        newAS.SubActiveSkillTriggerLogicList = SubActiveSkillTriggerLogicList.Clone();
        newAS.InterruptSubActiveSkillsWhenInterrupted = InterruptSubActiveSkillsWhenInterrupted;
        ChildClone(newAS);
        return newAS;
    }

    protected virtual void ChildClone(ActorActiveSkill cloneData)
    {
    }

    public virtual void CopyDataFrom(ActorActiveSkill srcData)
    {
        TargetCamp = srcData.TargetCamp;
        ActorSkillIndex = srcData.ActorSkillIndex;
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
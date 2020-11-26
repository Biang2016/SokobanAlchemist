using System;
using System.Collections;
using System.Collections.Generic;
using BiangStudio.CloneVariant;
using BiangStudio.GamePlay;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[Serializable]
public abstract class ActorActiveSkill : IClone<ActorActiveSkill>
{
    internal Actor Actor;

    #region SkillReady

    [LabelText("作用阵营")]
    public RelativeCamp TargetCamp;

    [LabelText("绑定角色技能号")]
    [FormerlySerializedAs("ActorSkillName")]
    public ActorSkillIndex ActorSkillIndex;

    internal Dictionary<ActorSkillPropertyType, ActorPropertyValue> ActorPropertyValueDict = new Dictionary<ActorSkillPropertyType, ActorPropertyValue>();

    protected int GetValue(ActorSkillPropertyType type)
    {
        return ActorPropertyValueDict[type].Value;
    }

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

    public class ActorPropertyValue
    {
        public int Value;

        public void OnValueChangedHandle(int before, int after)
        {
            Value = after;
        }
    }

    internal float cooldownTimeTick = 0f;

    #endregion

    [LabelText("技能描述")]
    [ShowInInspector]
    [PropertyOrder(-1)]
    protected abstract string Description { get; }

    private IEnumerable<string> GetAllBoxTypeNames => ConfigManager.GetAllBoxTypeNames();

    private IEnumerable<string> GetAllEnemyNames => ConfigManager.GetAllEnemyNames();

    private IEnumerable<string> GetAllFXTypeNames => ConfigManager.GetAllFXTypeNames();

    private IEnumerable<string> GetAllBattleIndicatorNames => ConfigManager.GetAllBattleIndicatorTypeNames();

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
    }

    public virtual void OnUnInit()
    {
        Interrupt();

        foreach (KeyValuePair<ActorSkillPropertyType, ActorPropertyValue> kv in ActorPropertyValueDict)
        {
            UnBindActorProperty(kv.Value, kv.Key);
        }

        Clear();
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

    public virtual bool TriggerActiveSkill()
    {
        if (skillPhase != ActiveSkillPhase.Ready || SkillCoroutine != null) return false;
        if (ValidateSkillTrigger())
        {
            SkillCoroutine = Actor.StartCoroutine(Co_CastSkill(GetValue(ActorSkillPropertyType.WingUp),
                GetValue(ActorSkillPropertyType.CastDuration),
                GetValue(ActorSkillPropertyType.Recovery),
                GetValue(ActorSkillPropertyType.SkillCastTimes)));
        }

        return true;
    }

    protected abstract bool ValidateSkillTrigger();

    public virtual void OnTick(float tickDeltaTime)
    {
        if (skillPhase == ActiveSkillPhase.CoolingDown)
        {
            cooldownTimeTick -= tickDeltaTime * 1000f;
            if (cooldownTimeTick <= 0)
            {
                cooldownTimeTick = 0;
                SkillPhase = ActiveSkillPhase.Ready;
            }
        }
    }

    private Coroutine SkillCoroutine;

    protected float WingUpRatio;
    protected float CastRatio;
    protected float RecoveryRatio;

    IEnumerator Co_CastSkill(float wingUpTime, float castDuration, float recoveryTime, int skillTriggerTimes)
    {
        WingUpRatio = 0;
        CastRatio = 0;
        RecoveryRatio = 0;

        SkillPhase = ActiveSkillPhase.WingingUp;
        // todo Actor 前摇animation， 且按时间缩放
        WingUp();
        float wingUpTick = 0f;
        while (wingUpTick < wingUpTime / 1000f)
        {
            wingUpTick += Time.deltaTime;
            WingUpRatio = wingUpTick / (wingUpTime / 1000f);
            yield return null;
        }

        SkillPhase = ActiveSkillPhase.Casting;
        // todo Actor 攻击animation， 且按时间缩放
        // todo Actor 攻击逻辑
        for (int count = 0; count < skillTriggerTimes; count++)
        {
            Cast();
            float castTick = 0f;
            while (castTick < castDuration / 1000f)
            {
                castTick += Time.deltaTime;
                CastRatio = castTick / (castDuration / 1000f);
                yield return null;
            }
        }

        SkillPhase = ActiveSkillPhase.Recovering;
        // todo Actor 后摇animation， 且按时间缩放
        Recover();
        float recoveryTick = 0f;
        while (recoveryTick < recoveryTime / 1000f)
        {
            recoveryTick += Time.deltaTime;
            RecoveryRatio = recoveryTick / (recoveryTime / 1000f);
            yield return null;
        }

        SkillPhase = ActiveSkillPhase.CoolingDown;

        WingUpRatio = 0;
        CastRatio = 0;
        RecoveryRatio = 0;

        SkillCoroutine = null;
    }

    public virtual void Clear()
    {
    }

    protected virtual void WingUp()
    {
    }

    protected virtual void Cast()
    {
        cooldownTimeTick = GetValue(ActorSkillPropertyType.Cooldown);
    }

    protected virtual void Recover()
    {
    }

    public void Interrupt()
    {
        if (SkillCoroutine == null) return;
        Actor.StopCoroutine(SkillCoroutine);
        if (SkillPhase == ActiveSkillPhase.Casting)
        {
            cooldownTimeTick = GetValue(ActorSkillPropertyType.Cooldown);
            SkillPhase = ActiveSkillPhase.CoolingDown;
        }
        else
        {
            cooldownTimeTick = 0f;
            SkillPhase = ActiveSkillPhase.Ready;
        }

        // todo Actor reset animation
    }

    public ActorActiveSkill Clone()
    {
        Type type = GetType();
        ActorActiveSkill newAS = (ActorActiveSkill) Activator.CreateInstance(type);
        newAS.TargetCamp = TargetCamp;
        newAS.ActorSkillIndex = ActorSkillIndex;
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
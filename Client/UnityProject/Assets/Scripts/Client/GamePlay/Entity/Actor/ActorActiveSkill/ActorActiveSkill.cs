using System;
using System.Collections;
using System.Collections.Generic;
using BiangStudio.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public abstract class ActorActiveSkill : IClone<ActorActiveSkill>
{
    internal Actor Actor;

    #region SkillReady

    [LabelText("作用阵营")]
    public RelativeCamp TargetCamp;

    [LabelText("冷却时间")]
    public ActorPropertyType APT_Cooldown;

    internal ActorPropertyValue CooldownTime = new ActorPropertyValue();

    [LabelText("施法时间")]
    public ActorPropertyType APT_CastDuration;

    internal ActorPropertyValue CastDuration = new ActorPropertyValue();

    [LabelText("前摇")]
    public ActorPropertyType APT_WingUp;

    internal ActorPropertyValue WingUpTime = new ActorPropertyValue();

    [LabelText("后摇")]
    public ActorPropertyType APT_Recovery;

    internal ActorPropertyValue RecoveryTime = new ActorPropertyValue();

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

    private IEnumerable<string> GetAllBattleIndicatorNames => ConfigManager.GetAllBattleIndicatorTypeNames();

    public virtual void OnInit()
    {
        OnSkillPhaseChanged = null;
        skillPhase = ActiveSkillPhase.Ready;

        cooldownTimeTick = 0;

        BindActorProperty(CooldownTime, APT_Cooldown);
        BindActorProperty(CastDuration, APT_CastDuration);
        BindActorProperty(WingUpTime, APT_WingUp);
        BindActorProperty(RecoveryTime, APT_Recovery);
    }

    public virtual void OnUnInit()
    {
        Interrupt();
        Clear();
        UnBindActorProperty(CooldownTime, APT_Cooldown);
        UnBindActorProperty(CastDuration, APT_CastDuration);
        UnBindActorProperty(WingUpTime, APT_WingUp);
        UnBindActorProperty(RecoveryTime, APT_Recovery);
    }

    protected void BindActorProperty(ActorPropertyValue apv, ActorPropertyType actorPropertyType)
    {
        ActorProperty ap = Actor.ActorStatPropSet.PropertyDict[actorPropertyType];
        apv.Value = ap.GetModifiedValue;
        ap.OnValueChanged += apv.OnValueChangedHandle;
    }

    protected void UnBindActorProperty(ActorPropertyValue apv, ActorPropertyType actorPropertyType)
    {
        ActorProperty ap = Actor.ActorStatPropSet.PropertyDict[actorPropertyType];
        ap.OnValueChanged -= apv.OnValueChangedHandle;
    }

    public virtual bool TriggerActiveSkill()
    {
        if (skillPhase != ActiveSkillPhase.Ready || SkillCoroutine != null) return false;
        if (ValidateSkillTrigger())
        {
            SkillCoroutine = Actor.StartCoroutine(Co_CastSkill(WingUpTime.Value, CastDuration.Value, RecoveryTime.Value));
        }

        return true;
    }

    protected virtual bool ValidateSkillTrigger()
    {
        return true;
    }

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

    IEnumerator Co_CastSkill(float wingUpTime, float castDuration, float recoveryTime)
    {
        SkillPhase = ActiveSkillPhase.WingingUp;
        // todo Actor 前摇animation， 且按时间缩放
        WingUp();
        yield return new WaitForSeconds(wingUpTime / 1000f);

        SkillPhase = ActiveSkillPhase.Casting;
        // todo Actor 攻击animation， 且按时间缩放
        // todo Actor 攻击逻辑
        Cast();
        yield return new WaitForSeconds(castDuration / 1000f);

        SkillPhase = ActiveSkillPhase.Recovering;
        // todo Actor 后摇animation， 且按时间缩放
        Recover();
        yield return new WaitForSeconds(recoveryTime / 1000f);

        SkillPhase = ActiveSkillPhase.CoolingDown;
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
        cooldownTimeTick = CooldownTime.Value;
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
            cooldownTimeTick = CooldownTime.Value;
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
        newAS.APT_Cooldown = APT_Cooldown;
        newAS.APT_CastDuration = APT_CastDuration;
        newAS.APT_WingUp = APT_WingUp;
        newAS.APT_Recovery = APT_Recovery;
        ChildClone(newAS);
        return newAS;
    }

    protected virtual void ChildClone(ActorActiveSkill newAS)
    {
    }

    public virtual void CopyDataFrom(ActorActiveSkill srcData)
    {
        TargetCamp = srcData.TargetCamp;
        APT_Cooldown = srcData.APT_Cooldown;
        APT_CastDuration = srcData.APT_CastDuration;
        APT_WingUp = srcData.APT_WingUp;
        APT_Recovery = srcData.APT_Recovery;
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
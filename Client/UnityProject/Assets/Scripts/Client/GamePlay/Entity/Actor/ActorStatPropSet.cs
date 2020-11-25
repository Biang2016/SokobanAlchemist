using System;
using System.Collections.Generic;
using BiangStudio.CloneVariant;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public class ActorStatPropSet : IClone<ActorStatPropSet>, ISerializationCallbackReceiver
{
    internal Actor Actor;

    public Dictionary<ActorStatType, ActorStat> StatDict = new Dictionary<ActorStatType, ActorStat>();
    public Dictionary<ActorPropertyType, ActorProperty> PropertyDict = new Dictionary<ActorPropertyType, ActorProperty>();

    [BoxGroup("生命")]
    [LabelText("@\"血量\t\"+Health")]
    public ActorStat Health = new ActorStat(ActorStatType.Health);

    [BoxGroup("生命")]
    [LabelText("@\"血量上限\t\"+MaxHealth")]
    public ActorProperty MaxHealth = new ActorProperty(ActorPropertyType.MaxHealth);

    [BoxGroup("生命")]
    [LabelText("@\"回血速度*100\t\"+HealthRecovery")]
    public ActorProperty HealthRecovery = new ActorProperty(ActorPropertyType.HealthRecovery);

    [BoxGroup("生命")]
    [LabelText("@\"生命数\t\"+Life")]
    public ActorStat Life = new ActorStat(ActorStatType.Life);

    [BoxGroup("操作")]
    [LabelText("@\"移动速度\t\"+MoveSpeed")]
    public ActorProperty MoveSpeed = new ActorProperty(ActorPropertyType.MoveSpeed);

    [BoxGroup("操作")]
    [LabelText("@\"最大行动力\t\"+MaxActionPoint")]
    public ActorProperty MaxActionPoint = new ActorProperty(ActorPropertyType.MaxActionPoint);

    [BoxGroup("操作")]
    [LabelText("@\"行动力\t\"+ActionPoint")]
    public ActorStat ActionPoint = new ActorStat(ActorStatType.ActionPoint);

    [BoxGroup("操作")]
    [LabelText("@\"行动力回复速度\t\"+ActionPointRecovery")]
    public ActorProperty ActionPointRecovery = new ActorProperty(ActorPropertyType.ActionPointRecovery);

    [BoxGroup("操作")]
    [LabelText("@\"Kick消耗行动力\t\"+KickConsumeActionPoint")]
    public ActorProperty KickConsumeActionPoint = new ActorProperty(ActorPropertyType.KickConsumeActionPoint);

    [BoxGroup("操作")]
    [LabelText("@\"Dash消耗行动力\t\"+DashConsumeActionPoint")]
    public ActorProperty DashConsumeActionPoint = new ActorProperty(ActorPropertyType.DashConsumeActionPoint);

    [BoxGroup("操作")]
    [LabelText("@\"Vault消耗行动力\t\"+VaultConsumeActionPoint")]
    public ActorProperty VaultConsumeActionPoint = new ActorProperty(ActorPropertyType.VaultConsumeActionPoint);

    [BoxGroup("冰冻")]
    [LabelText("@\"冰冻抗性\t\"+FrozenResistance")]
    public ActorProperty FrozenResistance = new ActorProperty(ActorPropertyType.FrozenResistance);

    [BoxGroup("冰冻")]
    [LabelText("@\"冰冻恢复率\t\"+FrozenRecovery")]
    public ActorProperty FrozenRecovery = new ActorProperty(ActorPropertyType.FrozenRecovery);

    [BoxGroup("冰冻")]
    [LabelText("@\"冰冻累积值\t\"+FrozenValue")]
    public ActorStat FrozenValue = new ActorStat(ActorStatType.FrozenValue);

    [BoxGroup("冰冻")]
    [LabelText("冰冻等级")]
    public ActorStat FrozenLevel = new ActorStat(ActorStatType.FrozenLevel);

    internal int FrozenValuePerLevel => Mathf.RoundToInt(((float) FrozenValue.MaxValue / FrozenLevel.MaxValue));

    public bool IsFrozen => FrozenLevel.Value > 0;

    [BoxGroup("冰冻")]
    [LabelText("冰冻持续特效")]
    [ValueDropdown("GetAllFXTypeNames", DropdownTitle = "选择FX类型")]
    public string FrozenFX;

    [BoxGroup("冰冻")]
    [LabelText("冰冻持续特效尺寸(x->冰冻等级")]
    public AnimationCurve FrozenFXScaleCurve;

    [BoxGroup("燃烧")]
    [LabelText("@\"燃烧抗性\t\"+FiringResistance")]
    public ActorProperty FiringResistance = new ActorProperty(ActorPropertyType.FiringResistance);

    [BoxGroup("燃烧")]
    [LabelText("@\"燃烧恢复率\t\"+FiringRecovery")]
    public ActorProperty FiringRecovery = new ActorProperty(ActorPropertyType.FiringRecovery);

    [BoxGroup("燃烧")]
    [LabelText("@\"燃烧增长率\t\"+FiringGrowthPercent")]
    public ActorProperty FiringGrowthPercent = new ActorProperty(ActorPropertyType.FiringGrowthPercent);

    [BoxGroup("燃烧")]
    [LabelText("@\"燃烧累积值\t\"+FiringValue")]
    public ActorStat FiringValue = new ActorStat(ActorStatType.FiringValue);

    [BoxGroup("燃烧")]
    [LabelText("燃烧等级")]
    public ActorStat FiringLevel = new ActorStat(ActorStatType.FiringLevel);

    internal int FiringValuePerLevel => Mathf.RoundToInt(((float) FiringValue.MaxValue / FiringLevel.MaxValue));

    [BoxGroup("燃烧")]
    [LabelText("燃烧持续特效")]
    [ValueDropdown("GetAllFXTypeNames", DropdownTitle = "选择FX类型")]
    public string FiringFX;

    [BoxGroup("燃烧")]
    [LabelText("燃烧持续特效尺寸(x->燃烧等级")]
    public AnimationCurve FiringFXScaleCurve;

    #region 技能

    #region 普通攻击

    [BoxGroup("攻击")]
    [LabelText("@\"攻击伤害\t\"+AttackDamage")]
    public ActorProperty AttackDamage = new ActorProperty(ActorPropertyType.AttackDamage);

    [BoxGroup("攻击")]
    [LabelText("@\"攻击附加燃烧值\t\"+AttackAttach_FiringValue")]
    public ActorProperty AttackAttach_FiringValue = new ActorProperty(ActorPropertyType.AttackAttach_FiringValue);

    [BoxGroup("攻击")]
    [LabelText("@\"攻击附加冰冻值\t\"+AttackAttach_FrozenValue")]
    public ActorProperty AttackAttach_FrozenValue = new ActorProperty(ActorPropertyType.AttackAttach_FrozenValue);

    [BoxGroup("攻击")]
    [LabelText("@\"攻击范围\t\"+AttackRange")]
    public ActorProperty AttackRange = new ActorProperty(ActorPropertyType.AttackRange);

    [BoxGroup("攻击")]
    [LabelText("@\"攻击CD/ms\t\"+AttackCooldown")]
    public ActorProperty AttackCooldown = new ActorProperty(ActorPropertyType.AttackCooldown);

    [BoxGroup("攻击")]
    [LabelText("@\"攻击施法时间/ms\t\"+AttackCastDuration")]
    public ActorProperty AttackCastDuration = new ActorProperty(ActorPropertyType.AttackCastDuration);

    [BoxGroup("攻击")]
    [LabelText("@\"攻击前摇/ms\t\"+AttackWingUp")]
    public ActorProperty AttackWingUp = new ActorProperty(ActorPropertyType.AttackWingUp);

    [BoxGroup("攻击")]
    [LabelText("@\"攻击后摇/ms\t\"+AttackRecovery")]
    public ActorProperty AttackRecovery = new ActorProperty(ActorPropertyType.AttackRecovery);

    #endregion

    #endregion

    [NonSerialized]
    [ShowInInspector]
    [BoxGroup("Buff")]
    [LabelText("角色自带Buff")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<ActorBuff> RawActorDefaultBuffs = new List<ActorBuff>(); // 干数据，禁修改

    [HideInInspector]
    public byte[] RawActorDefaultBuffData;

    public void OnBeforeSerialize()
    {
        if (RawActorDefaultBuffs == null) RawActorDefaultBuffs = new List<ActorBuff>();
        RawActorDefaultBuffData = SerializationUtility.SerializeValue(RawActorDefaultBuffs, DataFormat.JSON);
    }

    public void OnAfterDeserialize()
    {
        RawActorDefaultBuffs = SerializationUtility.DeserializeValue<List<ActorBuff>>(RawActorDefaultBuffData, DataFormat.JSON);
    }

    public void Initialize(Actor actor)
    {
        Actor = actor;

        MaxHealth.Initialize();
        HealthRecovery.Initialize();
        MoveSpeed.Initialize();
        MaxActionPoint.Initialize();
        ActionPointRecovery.Initialize();
        KickConsumeActionPoint.Initialize();
        DashConsumeActionPoint.Initialize();
        VaultConsumeActionPoint.Initialize();
        FrozenResistance.Initialize();
        FrozenRecovery.Initialize();
        FiringResistance.Initialize();
        FiringRecovery.Initialize();
        FiringGrowthPercent.Initialize();
        AttackDamage.Initialize();
        AttackAttach_FiringValue.Initialize();
        AttackAttach_FrozenValue.Initialize();
        AttackRange.Initialize();
        AttackCooldown.Initialize();
        AttackCastDuration.Initialize();
        AttackWingUp.Initialize();
        AttackRecovery.Initialize();

        Health.MaxValue = MaxHealth.GetModifiedValue;
        Health.OnValueReachZero += () => { Life.Value--; };
        Health.Recovery = HealthRecovery.GetModifiedValue / 100f;
        StatDict.Add(ActorStatType.Health, Health);

        MaxHealth.OnValueChanged += (before, after) => { Health.MaxValue = after; };
        PropertyDict.Add(ActorPropertyType.MaxHealth, MaxHealth);

        HealthRecovery.OnValueChanged += (before, after) => { Health.Recovery = after / 100f; };
        PropertyDict.Add(ActorPropertyType.HealthRecovery, HealthRecovery);

        Life.OnValueDecrease += (_) => { Health.ChangeValueWithoutNotify(Health.MaxValue - Health.Value); };
        StatDict.Add(ActorStatType.Life, Life);

        PropertyDict.Add(ActorPropertyType.MoveSpeed, MoveSpeed);

        MaxActionPoint.OnValueChanged += (before, after) => { ActionPoint.MaxValue = after; };
        PropertyDict.Add(ActorPropertyType.MaxActionPoint, MaxActionPoint);

        ActionPoint.Recovery = ActionPointRecovery.GetModifiedValue;
        StatDict.Add(ActorStatType.ActionPoint, ActionPoint);

        ActionPointRecovery.OnValueChanged += (before, after) => { ActionPoint.Recovery = after; };
        PropertyDict.Add(ActorPropertyType.ActionPointRecovery, ActionPointRecovery);

        PropertyDict.Add(ActorPropertyType.KickConsumeActionPoint, KickConsumeActionPoint);

        PropertyDict.Add(ActorPropertyType.DashConsumeActionPoint, DashConsumeActionPoint);

        PropertyDict.Add(ActorPropertyType.VaultConsumeActionPoint, VaultConsumeActionPoint);

        #region Frozen

        FrozenResistance.OnValueChanged += (before, after) => { FrozenValue.AbnormalStatResistance = after; };
        PropertyDict.Add(ActorPropertyType.FrozenResistance, FrozenResistance);

        FrozenRecovery.OnValueChanged += (before, after) => { FrozenValue.Recovery = -after; };
        PropertyDict.Add(ActorPropertyType.FrozenRecovery, FrozenRecovery);

        FrozenValue.Recovery = -FrozenRecovery.GetModifiedValue;
        FrozenValue.AbnormalStatResistance = FrozenResistance.GetModifiedValue;
        FrozenValue.OnValueChanged += (before, after) =>
        {
            FrozenLevel.Value = after / FrozenValuePerLevel;
            if (FrozenLevel.Value > 0) Actor.ActorBuffHelper.PlayAbnormalStatFX((int) ActorStatType.FrozenValue, FrozenFX, FrozenFXScaleCurve.Evaluate(FrozenLevel.Value)); // 冰冻值变化时，播放一次特效
        };
        StatDict.Add(ActorStatType.FrozenValue, FrozenValue);

        FrozenLevel.OnValueChanged += Actor.ActorFrozenHelper.FrozeIntoIceBlock;
        StatDict.Add(ActorStatType.FrozenLevel, FrozenLevel);

        #endregion

        #region Firing

        FiringResistance.OnValueChanged += (before, after) => { FiringValue.AbnormalStatResistance = after; };
        PropertyDict.Add(ActorPropertyType.FiringResistance, FiringResistance);

        FiringRecovery.OnValueChanged += (before, after) => { FiringValue.Recovery = -after; };
        PropertyDict.Add(ActorPropertyType.FiringRecovery, FiringRecovery);

        FiringGrowthPercent.OnValueChanged += (before, after) => { FiringValue.GrowthPercent = after; };
        PropertyDict.Add(ActorPropertyType.FiringGrowthPercent, FiringGrowthPercent);

        FiringValue.Recovery = -FiringResistance.GetModifiedValue;
        FiringValue.AbnormalStatResistance = FiringResistance.GetModifiedValue;
        FiringValue.GrowthPercent = FiringGrowthPercent.GetModifiedValue;
        FiringValue.OnValueChanged += (before, after) =>
        {
            FiringLevel.Value = after / FiringValuePerLevel;
            if (FiringLevel.Value > 0) Actor.ActorBuffHelper.PlayAbnormalStatFX((int) ActorStatType.FiringValue, FiringFX, FiringFXScaleCurve.Evaluate(FiringLevel.Value)); // 燃烧值变化时，播放一次特效
        };
        StatDict.Add(ActorStatType.FiringValue, FiringValue);

        StatDict.Add(ActorStatType.FiringLevel, FiringLevel);

        #endregion

        #region AttackDamage

        PropertyDict.Add(ActorPropertyType.AttackDamage, AttackDamage);
        PropertyDict.Add(ActorPropertyType.AttackAttach_FiringValue, AttackAttach_FiringValue);
        PropertyDict.Add(ActorPropertyType.AttackAttach_FrozenValue, AttackAttach_FrozenValue);
        PropertyDict.Add(ActorPropertyType.AttackRange, AttackRange);
        PropertyDict.Add(ActorPropertyType.AttackCooldown, AttackCooldown);
        PropertyDict.Add(ActorPropertyType.AttackCastDuration, AttackCastDuration);
        PropertyDict.Add(ActorPropertyType.AttackWingUp, AttackWingUp);
        PropertyDict.Add(ActorPropertyType.AttackRecovery, AttackRecovery);

        #endregion

        foreach (ActorBuff rawActorDefaultBuff in RawActorDefaultBuffs)
        {
            actor.ActorBuffHelper.AddBuff((ActorBuff) rawActorDefaultBuff.Clone());
        }
    }

    public void OnRecycled()
    {
        foreach (KeyValuePair<ActorStatType, ActorStat> kv in StatDict)
        {
            kv.Value.ClearCallBacks();
        }

        StatDict.Clear();
        foreach (KeyValuePair<ActorPropertyType, ActorProperty> kv in PropertyDict)
        {
            kv.Value.ClearCallBacks();
        }

        PropertyDict.Clear();
    }

    private float abnormalStateAutoTick = 0f;
    private int abnormalStateAutoTickInterval = 1; // 异常状态值每秒降低

    public void FixedUpdate(float fixedDeltaTime)
    {
        foreach (KeyValuePair<ActorStatType, ActorStat> kv in StatDict)
        {
            kv.Value.FixedUpdate(fixedDeltaTime);
        }

        abnormalStateAutoTick += fixedDeltaTime;
        if (abnormalStateAutoTick > abnormalStateAutoTickInterval)
        {
            abnormalStateAutoTick -= abnormalStateAutoTickInterval;

            Actor.ActorBattleHelper.Damage(Actor, FiringLevel.Value);
        }
    }

    public ActorStatPropSet Clone()
    {
        ActorStatPropSet newStatPropSet = new ActorStatPropSet();
        newStatPropSet.Health = (ActorStat) Health.Clone();
        newStatPropSet.MaxHealth = (ActorProperty) MaxHealth.Clone();
        newStatPropSet.HealthRecovery = (ActorProperty) HealthRecovery.Clone();
        newStatPropSet.Life = (ActorStat) Life.Clone();
        newStatPropSet.MoveSpeed = (ActorProperty) MoveSpeed.Clone();
        newStatPropSet.MaxActionPoint = (ActorProperty) MaxActionPoint.Clone();
        newStatPropSet.ActionPoint = (ActorStat) ActionPoint.Clone();
        newStatPropSet.ActionPointRecovery = (ActorProperty) ActionPointRecovery.Clone();
        newStatPropSet.KickConsumeActionPoint = (ActorProperty) KickConsumeActionPoint.Clone();
        newStatPropSet.DashConsumeActionPoint = (ActorProperty) DashConsumeActionPoint.Clone();
        newStatPropSet.VaultConsumeActionPoint = (ActorProperty) VaultConsumeActionPoint.Clone();
        newStatPropSet.FrozenResistance = (ActorProperty) FrozenResistance.Clone();
        newStatPropSet.FrozenRecovery = (ActorProperty) FrozenRecovery.Clone();
        newStatPropSet.FrozenValue = (ActorStat) FrozenValue.Clone();
        newStatPropSet.FrozenLevel = (ActorStat) FrozenLevel.Clone();
        newStatPropSet.FrozenFX = FrozenFX;
        newStatPropSet.FrozenFXScaleCurve = FrozenFXScaleCurve; // 风险，此处没有深拷贝
        newStatPropSet.FiringResistance = (ActorProperty) FiringResistance.Clone();
        newStatPropSet.FiringRecovery = (ActorProperty) FiringRecovery.Clone();
        newStatPropSet.FiringGrowthPercent = (ActorProperty) FiringGrowthPercent.Clone();
        newStatPropSet.FiringValue = (ActorStat) FiringValue.Clone();
        newStatPropSet.FiringLevel = (ActorStat) FiringLevel.Clone();
        newStatPropSet.FiringFX = FiringFX;
        newStatPropSet.FiringFXScaleCurve = FiringFXScaleCurve; // 风险，此处没有深拷贝
        newStatPropSet.AttackDamage = (ActorProperty) AttackDamage.Clone();
        newStatPropSet.AttackAttach_FiringValue = (ActorProperty) AttackAttach_FiringValue.Clone();
        newStatPropSet.AttackAttach_FrozenValue = (ActorProperty) AttackAttach_FrozenValue.Clone();
        newStatPropSet.AttackRange = (ActorProperty) AttackRange.Clone();
        newStatPropSet.AttackCooldown = (ActorProperty) AttackCooldown.Clone();
        newStatPropSet.AttackCastDuration = (ActorProperty)AttackCastDuration.Clone();
        newStatPropSet.AttackWingUp = (ActorProperty) AttackWingUp.Clone();
        newStatPropSet.AttackRecovery = (ActorProperty) AttackRecovery.Clone();

        newStatPropSet.RawActorDefaultBuffs = RawActorDefaultBuffs.Clone();
        return newStatPropSet;
    }

    #region Utils

    private IEnumerable<string> GetAllFXTypeNames => ConfigManager.GetAllFXTypeNames();

    #endregion
}

/// <summary>
/// Property是根据加或乘Modifier来增减、倍增的量，如最大生命值、速度、攻击等
/// </summary>
[Serializable]
public class ActorProperty : Property
{
    public ActorProperty()
    {
    }

    public ActorProperty(ActorPropertyType propertyType)
    {
        m_PropertyType = propertyType;
    }

    internal ActorPropertyType m_PropertyType;

    protected override void ChildClone(Property newProp)
    {
        base.ChildClone(newProp);
        ActorProperty newActorProp = (ActorProperty) newProp;
        newActorProp.m_PropertyType = m_PropertyType;
    }
}

[Serializable]
public class ActorStat : Stat
{
    public ActorStat()
    {
    }

    public ActorStat(ActorStatType statType)
    {
        m_StatType = statType;
    }

    internal ActorStatType m_StatType;
    public override bool IsAbnormalStat => m_StatType == ActorStatType.FiringValue || m_StatType == ActorStatType.FrozenValue;

    protected override void ChildClone(Stat newStat)
    {
        base.ChildClone(newStat);
        ActorStat newActorStat = (ActorStat) newStat;
        newActorStat.m_StatType = m_StatType;
    }
}

public enum ActorStatType
{
    [LabelText("血量")]
    Health = 0,

    [LabelText("生命数")]
    Life = 1,

    [LabelText("行动力")]
    ActionPoint = 20,

    [LabelText("冰冻累积值")]
    FrozenValue = 100,

    [LabelText("冰冻等级")]
    FrozenLevel = 120,

    [LabelText("燃烧累积值")]
    FiringValue = 101,

    [LabelText("燃烧等级")]
    FiringLevel = 121,
}

public enum ActorPropertyType
{
    [LabelText("血量上限")]
    MaxHealth = 0,

    [LabelText("回血速度")]
    HealthRecovery = 1,

    [LabelText("移动速度")]
    MoveSpeed = 10,

    [LabelText("最大行动力")]
    MaxActionPoint = 20,

    [LabelText("行动力回复速度")]
    ActionPointRecovery = 21,

    [LabelText("Kick消耗行动力")]
    KickConsumeActionPoint = 80,

    [LabelText("Dash消耗行动力")]
    DashConsumeActionPoint = 81,

    [LabelText("Vault消耗行动力")]
    VaultConsumeActionPoint = 82,

    [LabelText("冰冻抗性")]
    FrozenResistance = 100,

    [LabelText("燃烧抗性")]
    FiringResistance = 101,

    [LabelText("冰冻恢复率")]
    FrozenRecovery = 200,

    [LabelText("燃烧恢复率")]
    FiringRecovery = 201,

    [LabelText("燃烧增长率")]
    FiringGrowthPercent = 301,

    [LabelText("攻击伤害")]
    AttackDamage = 400,

    [LabelText("攻击附加燃烧值")]
    AttackAttach_FiringValue = 401,

    [LabelText("攻击附加冰冻值")]
    AttackAttach_FrozenValue = 402,

    [LabelText("攻击范围")]
    AttackRange = 500,

    [LabelText("攻击CD")]
    AttackCooldown = 600,

    [LabelText("攻击施法时间")]
    AttackCastDuration = 601,

    [LabelText("攻击前摇")]
    AttackWingUp = 602,

    [LabelText("攻击后摇")]
    AttackRecovery = 603,
}
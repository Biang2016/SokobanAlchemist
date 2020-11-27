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

    public bool IsFiring => FiringLevel.Value > 0;

    [BoxGroup("燃烧")]
    [LabelText("燃烧持续特效")]
    [ValueDropdown("GetAllFXTypeNames", DropdownTitle = "选择FX类型")]
    public string FiringFX;

    [BoxGroup("燃烧")]
    [LabelText("燃烧持续特效尺寸(x->燃烧等级")]
    public AnimationCurve FiringFXScaleCurve;

    #region 技能

    [LabelText("技能参数列表")]
    public List<SkillPropertyCollection> SkillsPropertyCollections = new List<SkillPropertyCollection>();

    [Serializable]
    public class SkillPropertyCollection : IClone<SkillPropertyCollection>
    {
        public Dictionary<ActorSkillPropertyType, ActorProperty> PropertyDict = new Dictionary<ActorSkillPropertyType, ActorProperty>();

        [SerializeField]
        [LabelText("技能名备注")]
        private string skillAlias = "";

        [LabelText("@\"伤害\t\"+Damage")]
        public ActorProperty Damage = new ActorProperty(ActorSkillPropertyType.Damage);

        [LabelText("@\"附加燃烧值\t\"+Attach_FiringValue")]
        public ActorProperty Attach_FiringValue = new ActorProperty(ActorSkillPropertyType.Attach_FiringValue);

        [LabelText("@\"附加冰冻值\t\"+Attach_FrozenValue")]
        public ActorProperty Attach_FrozenValue = new ActorProperty(ActorSkillPropertyType.Attach_FrozenValue);

        [LabelText("@\"目标数量\t\"+MaxTargetCount")]
        public ActorProperty MaxTargetCount = new ActorProperty(ActorSkillPropertyType.MaxTargetCount);

        [LabelText("@\"施法正方形范围边长\t\"+CastingRadius")]
        public ActorProperty CastingRadius = new ActorProperty(ActorSkillPropertyType.CastingRadius);

        [LabelText("@\"效果半径\t\"+EffectRadius")]
        public ActorProperty EffectRadius = new ActorProperty(ActorSkillPropertyType.EffectRadius);

        [LabelText("@\"宽度X\t\"+Width")]
        public ActorProperty Width = new ActorProperty(ActorSkillPropertyType.Width);

        [LabelText("@\"深度Z\t\"+Depth")]
        public ActorProperty Depth = new ActorProperty(ActorSkillPropertyType.Depth);

        [LabelText("@\"冷却时间/ms\t\"+Cooldown")]
        public ActorProperty Cooldown = new ActorProperty(ActorSkillPropertyType.Cooldown);

        [LabelText("@\"前摇/ms\t\"+WingUp")]
        public ActorProperty WingUp = new ActorProperty(ActorSkillPropertyType.WingUp);

        [LabelText("@\"施法时间/ms\t\"+CastDuration")]
        public ActorProperty CastDuration = new ActorProperty(ActorSkillPropertyType.CastDuration);

        [LabelText("@\"后摇/ms\t\"+Recovery")]
        public ActorProperty Recovery = new ActorProperty(ActorSkillPropertyType.Recovery);

        public void Initialize()
        {
            PropertyDict.Add(ActorSkillPropertyType.Damage, Damage);
            PropertyDict.Add(ActorSkillPropertyType.Attach_FiringValue, Attach_FiringValue);
            PropertyDict.Add(ActorSkillPropertyType.Attach_FrozenValue, Attach_FrozenValue);
            PropertyDict.Add(ActorSkillPropertyType.MaxTargetCount, MaxTargetCount);
            PropertyDict.Add(ActorSkillPropertyType.CastingRadius, CastingRadius);
            PropertyDict.Add(ActorSkillPropertyType.EffectRadius, EffectRadius);
            PropertyDict.Add(ActorSkillPropertyType.Width, Width);
            PropertyDict.Add(ActorSkillPropertyType.Depth, Depth);
            PropertyDict.Add(ActorSkillPropertyType.Cooldown, Cooldown);
            PropertyDict.Add(ActorSkillPropertyType.WingUp, WingUp);
            PropertyDict.Add(ActorSkillPropertyType.CastDuration, CastDuration);
            PropertyDict.Add(ActorSkillPropertyType.Recovery, Recovery);

            foreach (KeyValuePair<ActorSkillPropertyType, ActorProperty> kv in PropertyDict)
            {
                kv.Value.Initialize();
            }
        }

        public void OnRecycled()
        {
            foreach (KeyValuePair<ActorSkillPropertyType, ActorProperty> kv in PropertyDict)
            {
                kv.Value.ClearCallBacks();
            }

            PropertyDict.Clear();
        }

        public SkillPropertyCollection Clone()
        {
            SkillPropertyCollection newSPC = new SkillPropertyCollection();
            newSPC.skillAlias = skillAlias;
            newSPC.Damage = (ActorProperty) Damage.Clone();
            newSPC.Attach_FiringValue = (ActorProperty) Attach_FiringValue.Clone();
            newSPC.Attach_FrozenValue = (ActorProperty) Attach_FrozenValue.Clone();
            newSPC.MaxTargetCount = (ActorProperty) MaxTargetCount.Clone();
            newSPC.CastingRadius = (ActorProperty) CastingRadius.Clone();
            newSPC.EffectRadius = (ActorProperty) EffectRadius.Clone();
            newSPC.Width = (ActorProperty) Width.Clone();
            newSPC.Depth = (ActorProperty) Depth.Clone();
            newSPC.Cooldown = (ActorProperty) Cooldown.Clone();
            newSPC.WingUp = (ActorProperty) WingUp.Clone();
            newSPC.CastDuration = (ActorProperty) CastDuration.Clone();
            newSPC.Recovery = (ActorProperty) Recovery.Clone();
            return newSPC;
        }
    }

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

        foreach (SkillPropertyCollection spc in SkillsPropertyCollections)
        {
            spc.Initialize();
        }

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

        foreach (SkillPropertyCollection spc in SkillsPropertyCollections)
        {
            spc.OnRecycled();
        }
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

        newStatPropSet.SkillsPropertyCollections = SkillsPropertyCollections.Clone();
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
        m_PropertyType = (int) propertyType;
    }

    public ActorProperty(ActorSkillPropertyType propertyType)
    {
        m_PropertyType = (int) propertyType;
    }

    internal int m_PropertyType;

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

public enum ActorSkillIndex
{
    [LabelText("技能0")]
    Skill_0 = 0,

    [LabelText("技能1")]
    Skill_1 = 1,

    [LabelText("技能2")]
    Skill_2 = 2,

    [LabelText("技能3")]
    Skill_3 = 3,

    [LabelText("技能4")]
    Skill_4 = 4,

    [LabelText("技能5")]
    Skill_5 = 5,

    [LabelText("技能6")]
    Skill_6 = 6,

    [LabelText("技能7")]
    Skill_7 = 7,

    [LabelText("技能8")]
    Skill_8 = 8,
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
}

public enum ActorSkillPropertyType
{
    [LabelText("伤害")]
    Damage = 10001,

    [LabelText("附加燃烧值")]
    Attach_FiringValue = 10011,

    [LabelText("附加冰冻值")]
    Attach_FrozenValue = 10012,

    [LabelText("目标数量")]
    MaxTargetCount = 10021,

    [LabelText("施法正方形范围边长")]
    CastingRadius = 10022,

    [LabelText("效果半径")]
    EffectRadius = 10023,

    [LabelText("宽度X")]
    Width = 10024,

    [LabelText("深度Z")]
    Depth = 10025,

    [LabelText("冷却时间")]
    Cooldown = 10031,

    [LabelText("前摇")]
    WingUp = 10032,

    [LabelText("施法时间")]
    CastDuration = 10033,

    [LabelText("后摇")]
    Recovery = 10034,
}
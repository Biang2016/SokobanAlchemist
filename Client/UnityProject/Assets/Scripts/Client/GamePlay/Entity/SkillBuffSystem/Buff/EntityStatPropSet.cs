using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntityStatPropSet
{
    internal Entity Entity;

    public Dictionary<EntityStatType, EntityStat> StatDict = new Dictionary<EntityStatType, EntityStat>();
    public Dictionary<EntityPropertyType, EntityProperty> PropertyDict = new Dictionary<EntityPropertyType, EntityProperty>();

    #region 耐久

    [BoxGroup("耐久")]
    [LabelText("@\"血量耐久\t\"+HealthDurability")]
    public EntityStat HealthDurability = new EntityStat(EntityStatType.HealthDurability);

    [BoxGroup("耐久")]
    [LabelText("@\"血量耐久上限\t\"+MaxHealthDurability")]
    public EntityProperty MaxHealthDurability = new EntityProperty(EntityPropertyType.MaxHealthDurability);

    [BoxGroup("耐久")]
    [LabelText("@\"血量耐久回复速度*100\t\"+HealthDurabilityRecovery")]
    public EntityProperty HealthDurabilityRecovery = new EntityProperty(EntityPropertyType.HealthDurabilityRecovery);

    [BoxGroup("耐久")]
    [LabelText("@\"燃烧耐久\t\"+FiringDurability")]
    public EntityStat FiringDurability = new EntityStat(EntityStatType.FiringDurability);

    [BoxGroup("耐久")]
    [LabelText("@\"燃烧耐久上限\t\"+MaxFiringDurability")]
    public EntityProperty MaxFiringDurability = new EntityProperty(EntityPropertyType.MaxFiringDurability);

    [BoxGroup("耐久")]
    [LabelText("@\"燃烧耐久回复速度*100\t\"+FiringDurabilityRecovery")]
    public EntityProperty FiringDurabilityRecovery = new EntityProperty(EntityPropertyType.FiringDurabilityRecovery);

    [BoxGroup("耐久")]
    [LabelText("@\"爆炸耐久\t\"+ExplodeDurability")]
    public EntityStat ExplodeDurability = new EntityStat(EntityStatType.ExplodeDurability);

    [BoxGroup("耐久")]
    [LabelText("@\"爆炸耐久上限\t\"+MaxExplodeDurability")]
    public EntityProperty MaxExplodeDurability = new EntityProperty(EntityPropertyType.MaxExplodeDurability);

    [BoxGroup("耐久")]
    [LabelText("@\"爆炸耐久回复速度*100\t\"+FiringDurabilityRecovery")]
    public EntityProperty ExplodeDurabilityRecovery = new EntityProperty(EntityPropertyType.ExplodeDurabilityRecovery);

    [LabelText("@\"坠落留存率%\t\"+DropFromAirSurviveProbabilityPercent")]
    public EntityStat DropFromAirSurviveProbabilityPercent = new EntityStat(EntityStatType.DropFromAirSurviveProbabilityPercent);

    #endregion

    #region 操作

    [BoxGroup("操作")]
    [LabelText("@\"移动速度\t\"+MoveSpeed")]
    public EntityProperty MoveSpeed = new EntityProperty(EntityPropertyType.MoveSpeed);

    [BoxGroup("操作")]
    [LabelText("@\"最大行动力\t\"+MaxActionPoint")]
    public EntityProperty MaxActionPoint = new EntityProperty(EntityPropertyType.MaxActionPoint);

    [BoxGroup("操作")]
    [LabelText("@\"行动力\t\"+ActionPoint")]
    public EntityStat ActionPoint = new EntityStat(EntityStatType.ActionPoint);

    [BoxGroup("操作")]
    [LabelText("@\"行动力回复速度\t\"+ActionPointRecovery")]
    public EntityProperty ActionPointRecovery = new EntityProperty(EntityPropertyType.ActionPointRecovery);

    [BoxGroup("操作")]
    [LabelText("@\"Kick消耗行动力\t\"+KickConsumeActionPoint")]
    public EntityProperty KickConsumeActionPoint = new EntityProperty(EntityPropertyType.KickConsumeActionPoint);

    [BoxGroup("操作")]
    [LabelText("@\"Dash消耗行动力\t\"+DashConsumeActionPoint")]
    public EntityProperty DashConsumeActionPoint = new EntityProperty(EntityPropertyType.DashConsumeActionPoint);

    [BoxGroup("操作")]
    [LabelText("@\"Vault消耗行动力\t\"+VaultConsumeActionPoint")]
    public EntityProperty VaultConsumeActionPoint = new EntityProperty(EntityPropertyType.VaultConsumeActionPoint);

    #endregion

    #region 冰冻

    [BoxGroup("冰冻")]
    [LabelText("@\"冰冻抗性\t\"+FrozenResistance")]
    public EntityProperty FrozenResistance = new EntityProperty(EntityPropertyType.FrozenResistance);

    [BoxGroup("冰冻")]
    [LabelText("@\"冰冻恢复率\t\"+FrozenRecovery")]
    public EntityProperty FrozenRecovery = new EntityProperty(EntityPropertyType.FrozenRecovery);

    [BoxGroup("冰冻")]
    [LabelText("@\"冰冻累积值\t\"+FrozenValue")]
    public EntityStat FrozenValue = new EntityStat(EntityStatType.FrozenValue);

    [BoxGroup("冰冻")]
    [LabelText("冰冻等级")]
    public EntityStat FrozenLevel = new EntityStat(EntityStatType.FrozenLevel);

    internal int FrozenValuePerLevel => Mathf.RoundToInt(((float) FrozenValue.MaxValue / FrozenLevel.MaxValue));

    public bool IsFrozen => FrozenLevel.Value > 1;

    [BoxGroup("冰冻")]
    [LabelText("冰冻持续特效")]
    [ValueDropdown("GetAllFXTypeNames", DropdownTitle = "选择FX类型")]
    public string FrozenFX;

    [BoxGroup("冰冻")]
    [LabelText("冰冻持续特效尺寸(x->冰冻等级")]
    public AnimationCurve FrozenFXScaleCurve;

    #endregion

    #region 燃烧

    [BoxGroup("燃烧")]
    [LabelText("@\"燃烧抗性\t\"+FiringResistance")]
    public EntityProperty FiringResistance = new EntityProperty(EntityPropertyType.FiringResistance);

    [BoxGroup("燃烧")]
    [LabelText("@\"燃烧恢复率\t\"+FiringRecovery")]
    public EntityProperty FiringRecovery = new EntityProperty(EntityPropertyType.FiringRecovery);

    [BoxGroup("燃烧")]
    [LabelText("@\"燃烧增长率\t\"+FiringGrowthPercent")]
    public EntityProperty FiringGrowthPercent = new EntityProperty(EntityPropertyType.FiringGrowthPercent);

    [BoxGroup("燃烧")]
    [LabelText("@\"燃烧累积值\t\"+FiringValue")]
    public EntityStat FiringValue = new EntityStat(EntityStatType.FiringValue);

    [BoxGroup("燃烧")]
    [LabelText("燃烧等级")]
    public EntityStat FiringLevel = new EntityStat(EntityStatType.FiringLevel);

    internal int FiringValuePerLevel => Mathf.RoundToInt(((float) FiringValue.MaxValue / FiringLevel.MaxValue));

    public bool IsFiring => FiringLevel.Value > 0;

    [BoxGroup("燃烧")]
    [LabelText("燃烧触发特效")]
    [ValueDropdown("GetAllFXTypeNames", DropdownTitle = "选择FX类型")]
    public string StartFiringFX;

    [BoxGroup("燃烧")]
    [LabelText("燃烧触发特效尺寸")]
    public float StartFiringFXScale;

    [BoxGroup("燃烧")]
    [LabelText("燃烧持续特效")]
    [ValueDropdown("GetAllFXTypeNames", DropdownTitle = "选择FX类型")]
    public string FiringFX;

    [BoxGroup("燃烧")]
    [LabelText("燃烧持续特效尺寸(x->燃烧等级")]
    public AnimationCurve FiringFXScaleCurve;

    [BoxGroup("燃烧")]
    [LabelText("燃烧毁坏特效")]
    [ValueDropdown("GetAllFXTypeNames", DropdownTitle = "选择FX类型")]
    public string FiringBreakFX;

    [BoxGroup("燃烧")]
    [LabelText("燃烧毁坏特效尺寸")]
    public float FiringBreakFXScale;

    #endregion

    #region 技能

    [LabelText("技能参数列表")]
    public List<SkillPropertyCollection> SkillsPropertyCollections = new List<SkillPropertyCollection>();

    [Serializable]
    public class SkillPropertyCollection : IClone<SkillPropertyCollection>
    {
        public Dictionary<EntitySkillPropertyType, EntityProperty> PropertyDict = new Dictionary<EntitySkillPropertyType, EntityProperty>();

        [SerializeField]
        [LabelText("技能名备注")]
        private string skillAlias = "";

        [LabelText("@\"伤害\t\"+Damage")]
        public EntityProperty Damage = new EntityProperty(EntitySkillPropertyType.Damage);

        [LabelText("@\"附加燃烧值\t\"+Attach_FiringValue")]
        public EntityProperty Attach_FiringValue = new EntityProperty(EntitySkillPropertyType.Attach_FiringValue);

        [LabelText("@\"附加冰冻值\t\"+Attach_FrozenValue")]
        public EntityProperty Attach_FrozenValue = new EntityProperty(EntitySkillPropertyType.Attach_FrozenValue);

        [LabelText("@\"施法正方形范围边长\t\"+CastingRadius")]
        public EntityProperty CastingRadius = new EntityProperty(EntitySkillPropertyType.CastingRadius);

        [LabelText("@\"效果半径\t\"+EffectRadius")]
        public EntityProperty EffectRadius = new EntityProperty(EntitySkillPropertyType.EffectRadius);

        [LabelText("@\"宽度X\t\"+Width")]
        public EntityProperty Width = new EntityProperty(EntitySkillPropertyType.Width);

        [LabelText("@\"深度Z\t\"+Depth")]
        public EntityProperty Depth = new EntityProperty(EntitySkillPropertyType.Depth);

        [LabelText("@\"冷却时间/ms\t\"+Cooldown")]
        public EntityProperty Cooldown = new EntityProperty(EntitySkillPropertyType.Cooldown);

        [LabelText("@\"前摇/ms\t\"+WingUp")]
        public EntityProperty WingUp = new EntityProperty(EntitySkillPropertyType.WingUp);

        [LabelText("@\"施法时间/ms\t\"+CastDuration")]
        public EntityProperty CastDuration = new EntityProperty(EntitySkillPropertyType.CastDuration);

        [LabelText("@\"后摇/ms\t\"+Recovery")]
        public EntityProperty Recovery = new EntityProperty(EntitySkillPropertyType.Recovery);

        [Button("初始化")]
        public void Initialize()
        {
            PropertyDict.Add(EntitySkillPropertyType.Damage, Damage);
            PropertyDict.Add(EntitySkillPropertyType.Attach_FiringValue, Attach_FiringValue);
            PropertyDict.Add(EntitySkillPropertyType.Attach_FrozenValue, Attach_FrozenValue);
            PropertyDict.Add(EntitySkillPropertyType.CastingRadius, CastingRadius);
            PropertyDict.Add(EntitySkillPropertyType.EffectRadius, EffectRadius);
            PropertyDict.Add(EntitySkillPropertyType.Width, Width);
            PropertyDict.Add(EntitySkillPropertyType.Depth, Depth);
            PropertyDict.Add(EntitySkillPropertyType.Cooldown, Cooldown);
            PropertyDict.Add(EntitySkillPropertyType.WingUp, WingUp);
            PropertyDict.Add(EntitySkillPropertyType.CastDuration, CastDuration);
            PropertyDict.Add(EntitySkillPropertyType.Recovery, Recovery);

            foreach (KeyValuePair<EntitySkillPropertyType, EntityProperty> kv in PropertyDict)
            {
                kv.Value.Initialize();
            }
        }

        public void OnRecycled()
        {
            foreach (KeyValuePair<EntitySkillPropertyType, EntityProperty> kv in PropertyDict)
            {
                kv.Value.OnRecycled();
            }

            PropertyDict.Clear();
        }

        public SkillPropertyCollection Clone()
        {
            SkillPropertyCollection newSPC = new SkillPropertyCollection();
            newSPC.skillAlias = skillAlias;
            Damage.ApplyDataTo(newSPC.Damage);
            Attach_FiringValue.ApplyDataTo(newSPC.Attach_FiringValue);
            Attach_FrozenValue.ApplyDataTo(newSPC.Attach_FrozenValue);
            CastingRadius.ApplyDataTo(newSPC.CastingRadius);
            EffectRadius.ApplyDataTo(newSPC.EffectRadius);
            Width.ApplyDataTo(newSPC.Width);
            Depth.ApplyDataTo(newSPC.Depth);
            Cooldown.ApplyDataTo(newSPC.Cooldown);
            WingUp.ApplyDataTo(newSPC.WingUp);
            CastDuration.ApplyDataTo(newSPC.CastDuration);
            Recovery.ApplyDataTo(newSPC.Recovery);
            return newSPC;
        }
    }

    #endregion

    [SerializeReference]
    [BoxGroup("Buff")]
    [LabelText("自带Buff")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<EntityBuff> RawEntityDefaultBuffs = new List<EntityBuff>(); // 干数据，禁修改

    public void Initialize(Entity entity)
    {
        Entity = entity;

        // Property Initialize
        MaxHealthDurability.Initialize();
        HealthDurabilityRecovery.Initialize();
        MaxFiringDurability.Initialize();
        FiringDurabilityRecovery.Initialize();
        MaxExplodeDurability.Initialize();
        ExplodeDurabilityRecovery.Initialize();
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

        #region 耐久

        HealthDurability.MaxValue = MaxHealthDurability.GetModifiedValue;
        HealthDurability.OnValueReachZero += () =>
        {
            entity.EntityPassiveSkillMarkAsDeleted = true;
            if (entity is Actor actor) actor.ActorBattleHelper.Die();
        };
        HealthDurability.Recovery = HealthDurabilityRecovery.GetModifiedValue / 100f;
        StatDict.Add(EntityStatType.HealthDurability, HealthDurability);
        MaxHealthDurability.OnValueChanged += (before, after) => { HealthDurability.MaxValue = after; };
        PropertyDict.Add(EntityPropertyType.MaxHealthDurability, MaxHealthDurability);
        HealthDurabilityRecovery.OnValueChanged += (before, after) => { HealthDurability.Recovery = after / 100f; };
        PropertyDict.Add(EntityPropertyType.HealthDurabilityRecovery, HealthDurabilityRecovery);

        FiringDurability.MaxValue = MaxFiringDurability.GetModifiedValue;
        FiringDurability.OnValueReachZero += () =>
        {
            if (entity is Box box)
            {
                foreach (GridPos3D offset in box.GetBoxOccupationGPs_Rotated())
                {
                    FXManager.Instance.PlayFX(FiringBreakFX, box.transform.position + offset, 1.5f);
                }
            }

            entity.EntityPassiveSkillMarkAsDeleted = true;
            if (entity is Actor actor) actor.ActorBattleHelper.Die();
        };
        FiringDurability.Recovery = FiringDurabilityRecovery.GetModifiedValue / 100f;
        StatDict.Add(EntityStatType.FiringDurability, FiringDurability);
        MaxFiringDurability.OnValueChanged += (before, after) => { FiringDurability.MaxValue = after; };
        PropertyDict.Add(EntityPropertyType.MaxFiringDurability, MaxFiringDurability);
        FiringDurabilityRecovery.OnValueChanged += (before, after) => { FiringDurability.Recovery = after / 100f; };
        PropertyDict.Add(EntityPropertyType.FiringDurabilityRecovery, FiringDurabilityRecovery);

        ExplodeDurability.MaxValue = MaxExplodeDurability.GetModifiedValue;
        ExplodeDurability.OnValueReachZero += () =>
        {
            entity.EntityPassiveSkillMarkAsDeleted = true;
            if (entity is Actor actor) actor.ActorBattleHelper.Die();
        };
        ExplodeDurability.Recovery = ExplodeDurabilityRecovery.GetModifiedValue / 100f;
        StatDict.Add(EntityStatType.ExplodeDurability, ExplodeDurability);
        MaxExplodeDurability.OnValueChanged += (before, after) => { ExplodeDurability.MaxValue = after; };
        PropertyDict.Add(EntityPropertyType.MaxExplodeDurability, MaxExplodeDurability);
        ExplodeDurabilityRecovery.OnValueChanged += (before, after) => { ExplodeDurability.Recovery = after / 100f; };
        PropertyDict.Add(EntityPropertyType.ExplodeDurabilityRecovery, ExplodeDurabilityRecovery);

        StatDict.Add(EntityStatType.DropFromAirSurviveProbabilityPercent, DropFromAirSurviveProbabilityPercent);

        #endregion

        #region 操作

        PropertyDict.Add(EntityPropertyType.MoveSpeed, MoveSpeed);

        MaxActionPoint.OnValueChanged += (before, after) => { ActionPoint.MaxValue = after; };
        PropertyDict.Add(EntityPropertyType.MaxActionPoint, MaxActionPoint);

        ActionPoint.Recovery = ActionPointRecovery.GetModifiedValue;
        StatDict.Add(EntityStatType.ActionPoint, ActionPoint);

        ActionPointRecovery.OnValueChanged += (before, after) => { ActionPoint.Recovery = after; };
        PropertyDict.Add(EntityPropertyType.ActionPointRecovery, ActionPointRecovery);

        PropertyDict.Add(EntityPropertyType.KickConsumeActionPoint, KickConsumeActionPoint);

        PropertyDict.Add(EntityPropertyType.DashConsumeActionPoint, DashConsumeActionPoint);

        PropertyDict.Add(EntityPropertyType.VaultConsumeActionPoint, VaultConsumeActionPoint);

        #endregion

        #region 冰冻

        FrozenResistance.OnValueChanged += (before, after) => { FrozenValue.AbnormalStatResistance = after; };
        PropertyDict.Add(EntityPropertyType.FrozenResistance, FrozenResistance);

        FrozenRecovery.OnValueChanged += (before, after) => { FrozenValue.Recovery = -after; };
        PropertyDict.Add(EntityPropertyType.FrozenRecovery, FrozenRecovery);

        FrozenValue.Recovery = -FrozenRecovery.GetModifiedValue;
        FrozenValue.AbnormalStatResistance = FrozenResistance.GetModifiedValue;
        FrozenValue.OnValueChanged += (before, after) =>
        {
            FrozenLevel.Value = after / FrozenValuePerLevel;
            if (FrozenLevel.Value > 0) Entity.EntityBuffHelper.PlayAbnormalStatFX((int) EntityStatType.FrozenValue, FrozenFX, FrozenFXScaleCurve.Evaluate(FrozenLevel.Value)); // 冰冻值变化时，播放一次特效
        };
        StatDict.Add(EntityStatType.FrozenValue, FrozenValue);

        FrozenLevel.OnValueChanged += Entity.EntityFrozenHelper.FrozeIntoIceBlock;
        StatDict.Add(EntityStatType.FrozenLevel, FrozenLevel);

        #endregion

        #region Firing

        FiringResistance.OnValueChanged += (before, after) => { FiringValue.AbnormalStatResistance = after; };
        PropertyDict.Add(EntityPropertyType.FiringResistance, FiringResistance);

        FiringRecovery.OnValueChanged += (before, after) => { FiringValue.Recovery = -after; };
        PropertyDict.Add(EntityPropertyType.FiringRecovery, FiringRecovery);

        FiringGrowthPercent.OnValueChanged += (before, after) => { FiringValue.GrowthPercent = after; };
        PropertyDict.Add(EntityPropertyType.FiringGrowthPercent, FiringGrowthPercent);

        FiringValue.Recovery = -FiringResistance.GetModifiedValue;
        FiringValue.AbnormalStatResistance = FiringResistance.GetModifiedValue;
        FiringValue.GrowthPercent = FiringGrowthPercent.GetModifiedValue;
        FiringValue.OnValueChanged += (before, after) =>
        {
            FiringLevel.Value = after / FiringValuePerLevel;
            if (FiringLevel.Value > 0) Entity.EntityBuffHelper.PlayAbnormalStatFX((int) EntityStatType.FiringValue, FiringFX, FiringFXScaleCurve.Evaluate(FiringLevel.Value)); // 燃烧值变化时，播放一次特效
            else if (after == 0) Entity.EntityBuffHelper.RemoveAbnormalStatFX((int) EntityStatType.FiringValue);
        };
        StatDict.Add(EntityStatType.FiringValue, FiringValue);

        StatDict.Add(EntityStatType.FiringLevel, FiringLevel);

        #endregion

        foreach (SkillPropertyCollection spc in SkillsPropertyCollections)
        {
            spc.Initialize();
        }

        foreach (EntityBuff rawEntityDefaultBuff in RawEntityDefaultBuffs)
        {
            Entity.EntityBuffHelper.AddBuff(rawEntityDefaultBuff.Clone());
        }
    }

    public void OnRecycled()
    {
        foreach (KeyValuePair<EntityStatType, EntityStat> kv in StatDict)
        {
            kv.Value.OnRecycled();
        }

        StatDict.Clear();
        foreach (KeyValuePair<EntityPropertyType, EntityProperty> kv in PropertyDict)
        {
            kv.Value.OnRecycled();
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
        foreach (KeyValuePair<EntityStatType, EntityStat> kv in StatDict)
        {
            kv.Value.FixedUpdate(fixedDeltaTime);
        }

        abnormalStateAutoTick += fixedDeltaTime;
        if (abnormalStateAutoTick > abnormalStateAutoTickInterval)
        {
            abnormalStateAutoTick -= abnormalStateAutoTickInterval;

            if (Entity is Actor actor)
            {
                actor.ActorBattleHelper.Damage(actor, FiringLevel.Value);
            }
        }

        // 燃烧蔓延
        if (FiringLevel.Value >= 1)
        {
            foreach (Box adjacentBox in WorldManager.Instance.CurrentWorld.GetAdjacentBox(Entity.WorldGP))
            {
                int diff = FiringValue.Value - adjacentBox.EntityStatPropSet.FiringValue.Value;
                if (diff > 0)
                {
                    adjacentBox.EntityStatPropSet.FiringValue.Value += Mathf.RoundToInt(diff * 0.66f);
                }
            }

            FiringDurability.Value -= FiringLevel.Value;
        }
    }

    public void ApplyDataTo(EntityStatPropSet target)
    {
        #region 耐久

        HealthDurability.ApplyDataTo(target.HealthDurability);
        MaxHealthDurability.ApplyDataTo(target.MaxHealthDurability);
        HealthDurabilityRecovery.ApplyDataTo(target.HealthDurabilityRecovery);
        FiringDurability.ApplyDataTo(target.FiringDurability);
        MaxFiringDurability.ApplyDataTo(target.MaxFiringDurability);
        FiringDurabilityRecovery.ApplyDataTo(target.FiringDurabilityRecovery);
        ExplodeDurability.ApplyDataTo(target.ExplodeDurability);
        MaxExplodeDurability.ApplyDataTo(target.MaxExplodeDurability);
        ExplodeDurabilityRecovery.ApplyDataTo(target.ExplodeDurabilityRecovery);
        DropFromAirSurviveProbabilityPercent.ApplyDataTo(target.DropFromAirSurviveProbabilityPercent);

        #endregion

        #region 操作

        MoveSpeed.ApplyDataTo(target.MoveSpeed);
        MaxActionPoint.ApplyDataTo(target.MaxActionPoint);
        ActionPoint.ApplyDataTo(target.ActionPoint);
        ActionPointRecovery.ApplyDataTo(target.ActionPointRecovery);
        KickConsumeActionPoint.ApplyDataTo(target.KickConsumeActionPoint);
        DashConsumeActionPoint.ApplyDataTo(target.DashConsumeActionPoint);
        VaultConsumeActionPoint.ApplyDataTo(target.VaultConsumeActionPoint);

        #endregion

        #region 冰冻

        FrozenResistance.ApplyDataTo(target.FrozenResistance);
        FrozenRecovery.ApplyDataTo(target.FrozenRecovery);
        FrozenValue.ApplyDataTo(target.FrozenValue);
        FrozenLevel.ApplyDataTo(target.FrozenLevel);
        target.FrozenFX = FrozenFX;
        target.FrozenFXScaleCurve = FrozenFXScaleCurve; // 风险，此处没有深拷贝

        #endregion

        #region 燃烧

        FiringResistance.ApplyDataTo(target.FiringResistance);
        FiringRecovery.ApplyDataTo(target.FiringRecovery);
        FiringGrowthPercent.ApplyDataTo(target.FiringGrowthPercent);
        FiringValue.ApplyDataTo(target.FiringValue);
        FiringLevel.ApplyDataTo(target.FiringLevel);
        target.StartFiringFX = StartFiringFX;
        target.StartFiringFXScale = StartFiringFXScale;
        target.FiringFX = FiringFX;
        target.FiringFXScaleCurve = FiringFXScaleCurve; // 风险，此处没有深拷贝
        target.FiringBreakFX = FiringBreakFX;
        target.FiringBreakFXScale = FiringBreakFXScale;

        #endregion

        target.SkillsPropertyCollections = SkillsPropertyCollections.Clone();
        target.RawEntityDefaultBuffs = RawEntityDefaultBuffs; // 由于是干数据，此处不克隆！
    }

    #region Utils

    private IEnumerable<string> GetAllFXTypeNames => ConfigManager.GetAllFXTypeNames();

    #endregion
}

public enum EntitySkillIndex
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
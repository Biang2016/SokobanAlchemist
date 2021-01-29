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
    [LabelText("@\"坠落留存率%\t\"+DropFromAirSurviveProbabilityPercent")]
    public EntityStat DropFromAirSurviveProbabilityPercent = new EntityStat(EntityStatType.DropFromAirSurviveProbabilityPercent);

    #endregion

    #region 碰撞

    [BoxGroup("碰撞")]
    [LabelText("@\"角色碰撞伤害抵消\t\"+ActorCollideDamageDefense")]
    public EntityProperty ActorCollideDamageDefense = new EntityProperty(EntityPropertyType.ActorCollideDamageDefense);

    [BoxGroup("碰撞")]
    [LabelText("@\"箱子碰撞减少自身耐久\t\"+BoxCollideDamageSelf")]
    public EntityProperty BoxCollideDamageSelf = new EntityProperty(EntityPropertyType.BoxCollideDamageSelf);

    [BoxGroup("碰撞")]
    [LabelText("@\"碰撞伤害\t\"+CollideDamage")]
    public EntityProperty CollideDamage = new EntityProperty(EntityPropertyType.CollideDamage);

    #endregion

    #region 爆炸

    [BoxGroup("爆炸")]
    [LabelText("@\"爆炸伤害抵消\t\"+ExplodeDamageDefense")]
    public EntityProperty ExplodeDamageDefense = new EntityProperty(EntityPropertyType.ExplodeDamageDefense);

    #endregion

    #region 冰冻

    [BoxGroup("冰冻")]
    [LabelText("@\"冰冻抗性\t\"+FrozenResistance")]
    public EntityProperty FrozenResistance = new EntityProperty(EntityPropertyType.FrozenResistance);

    [BoxGroup("冰冻")]
    [LabelText("@\"冰冻累积值\t\"+FrozenValue")]
    public EntityStat FrozenValue = new EntityStat(EntityStatType.FrozenValue);

    [BoxGroup("冰冻")]
    [LabelText("冰冻等级")]
    public EntityStat FrozenLevel = new EntityStat(EntityStatType.FrozenLevel);

    internal int FrozenValuePerLevel => Mathf.RoundToInt(((float) FrozenValue.MaxValue / FrozenLevel.MaxValue));

    public bool IsFrozen => FrozenLevel.Value > 1; // 1级不算冰冻， 2级~4级冰冻对应三个模型

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
    [LabelText("@\"燃烧累积值\t\"+FiringValue")]
    public EntityStat FiringValue = new EntityStat(EntityStatType.FiringValue);

    [BoxGroup("燃烧")]
    [LabelText("燃烧等级")]
    public EntityStat FiringLevel = new EntityStat(EntityStatType.FiringLevel);

    internal int FiringValuePerLevel => Mathf.RoundToInt(((float) FiringValue.MaxValue / FiringLevel.MaxValue));

    public bool IsFiring => FiringLevel.Value > 0;

    [BoxGroup("燃烧")]
    [LabelText("@\"燃烧蔓延率%/s\t\"+FiringSpreadPercent")]
    public EntityProperty FiringSpreadPercent = new EntityProperty(EntityPropertyType.FiringSpreadPercent);

    [BoxGroup("燃烧")]
    [LabelText("@\"燃烧伤害抵消\t\"+FiringDamageDefense")]
    public EntityProperty FiringDamageDefense = new EntityProperty(EntityPropertyType.FiringDamageDefense);

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
    [LabelText("@\"行动力恢复速度/100\t\"+ActionPointRecovery")]
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

    #region 技能

    [LabelText("技能参数列表")]
    [ListDrawerSettings(ListElementLabelName = "skillAlias")]
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

        #region Property初始化

        #region 耐久

        MaxHealthDurability.Initialize();

        #endregion

        #region 碰撞

        ActorCollideDamageDefense.Initialize();
        BoxCollideDamageSelf.Initialize();
        CollideDamage.Initialize();

        #endregion

        #region 爆炸

        ExplodeDamageDefense.Initialize();

        #endregion

        #region 冰冻

        FrozenResistance.Initialize();

        #endregion

        #region 燃烧

        FiringResistance.Initialize();
        FiringSpreadPercent.Initialize();
        FiringDamageDefense.Initialize();

        #endregion

        #region 操作

        MoveSpeed.Initialize();
        MaxActionPoint.Initialize();
        ActionPointRecovery.Initialize();
        KickConsumeActionPoint.Initialize();
        DashConsumeActionPoint.Initialize();
        VaultConsumeActionPoint.Initialize();

        #endregion

        #endregion

        #region 耐久

        HealthDurability.MaxValue = MaxHealthDurability.GetModifiedValue;
        HealthDurability.OnValueDecrease += (decrease) =>
        {
            if (entity is Actor actor)
            {
                actor.ActorBattleHelper.ShowDamageNumFX(decrease);
            }
        };
        HealthDurability.OnValueIncrease += (increase) =>
        {
            if (entity is Actor actor) actor.ActorBattleHelper.ShowHealNumFX(increase);
        };
        HealthDurability.OnValueReachZero += () =>
        {
            entity.PassiveSkillMarkAsDestroyed = true;
            if (entity is Actor actor) actor.ActorBattleHelper.Die();
        };
        StatDict.Add(EntityStatType.HealthDurability, HealthDurability);
        MaxHealthDurability.OnValueChanged += (before, after) => { HealthDurability.MaxValue = after; };
        PropertyDict.Add(EntityPropertyType.MaxHealthDurability, MaxHealthDurability);

        StatDict.Add(EntityStatType.DropFromAirSurviveProbabilityPercent, DropFromAirSurviveProbabilityPercent);

        #endregion

        #region 碰撞

        PropertyDict.Add(EntityPropertyType.ActorCollideDamageDefense, ActorCollideDamageDefense);
        PropertyDict.Add(EntityPropertyType.BoxCollideDamageSelf, BoxCollideDamageSelf);
        PropertyDict.Add(EntityPropertyType.CollideDamage, CollideDamage);

        #endregion

        #region 爆炸

        PropertyDict.Add(EntityPropertyType.ExplodeDamageDefense, ExplodeDamageDefense);

        #endregion

        #region 冰冻

        FrozenResistance.OnValueChanged += (before, after) => { FrozenValue.AbnormalStatResistance = after; };
        PropertyDict.Add(EntityPropertyType.FrozenResistance, FrozenResistance);

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

        #region 燃烧

        FiringResistance.OnValueChanged += (before, after) => { FiringValue.AbnormalStatResistance = after; };
        PropertyDict.Add(EntityPropertyType.FiringResistance, FiringResistance);

        FiringValue.AbnormalStatResistance = FiringResistance.GetModifiedValue;
        FiringValue.OnValueChanged += (before, after) =>
        {
            FiringLevel.Value = after / FiringValuePerLevel;
            if (FiringLevel.Value > 0) Entity.EntityBuffHelper.PlayAbnormalStatFX((int) EntityStatType.FiringValue, FiringFX, FiringFXScaleCurve.Evaluate(FiringLevel.Value)); // 燃烧值变化时，播放一次特效
            else if (after == 0) Entity.EntityBuffHelper.RemoveAbnormalStatFX((int) EntityStatType.FiringValue);
        };
        StatDict.Add(EntityStatType.FiringValue, FiringValue);

        FiringLevel.OnValueChanged += (before, after) =>
        {
            if (before == 0 && after > 0)
            {
                if (entity is Box box)
                {
                    foreach (GridPos3D offset in box.GetBoxOccupationGPs_Rotated())
                    {
                        FX fx = FXManager.Instance.PlayFX(StartFiringFX, box.transform.position + Vector3.up * 0.5f + offset, StartFiringFXScale);
                        if (fx) fx.transform.parent = box.transform;
                    }
                }
            }
        };
        StatDict.Add(EntityStatType.FiringLevel, FiringLevel);
        PropertyDict.Add(EntityPropertyType.FiringSpreadPercent, FiringSpreadPercent);
        PropertyDict.Add(EntityPropertyType.FiringDamageDefense, FiringDamageDefense);

        #endregion

        #region 操作

        PropertyDict.Add(EntityPropertyType.MoveSpeed, MoveSpeed);

        MaxActionPoint.OnValueChanged += (before, after) => { ActionPoint.MaxValue = after; };
        PropertyDict.Add(EntityPropertyType.MaxActionPoint, MaxActionPoint);

        ActionPoint.MaxValue = MaxActionPoint.GetModifiedValue;
        ActionPoint.AutoChange = ActionPointRecovery.GetModifiedValue / 100f;
        StatDict.Add(EntityStatType.ActionPoint, ActionPoint);

        ActionPointRecovery.OnValueChanged += (before, after) => { ActionPoint.AutoChange = after / 100f; };
        PropertyDict.Add(EntityPropertyType.ActionPointRecovery, ActionPointRecovery);

        PropertyDict.Add(EntityPropertyType.KickConsumeActionPoint, KickConsumeActionPoint);

        PropertyDict.Add(EntityPropertyType.DashConsumeActionPoint, DashConsumeActionPoint);

        PropertyDict.Add(EntityPropertyType.VaultConsumeActionPoint, VaultConsumeActionPoint);

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

    public void FixedUpdate(float fixedDeltaTime) // Actor更新频率为每帧，Box更新频率为每秒
    {
        foreach (KeyValuePair<EntityStatType, EntityStat> kv in StatDict)
        {
            kv.Value.FixedUpdate(fixedDeltaTime);
        }

        abnormalStateAutoTick += fixedDeltaTime;
        if (abnormalStateAutoTick > abnormalStateAutoTickInterval)
        {
            abnormalStateAutoTick -= abnormalStateAutoTickInterval;
            if (FiringLevel.Value >= 1)
            {
                // 燃烧蔓延
                foreach (Box adjacentBox in WorldManager.Instance.CurrentWorld.GetAdjacentBox(Entity.WorldGP))
                {
                    int diff = FiringValue.Value - adjacentBox.EntityStatPropSet.FiringValue.Value;
                    if (diff > 0)
                    {
                        adjacentBox.EntityStatPropSet.FiringValue.Value += Mathf.RoundToInt(diff * FiringSpreadPercent.GetModifiedValue / 100f);
                    }
                }

                Entity.EntityBuffHelper.Damage(FiringLevel.Value, EntityBuffAttribute.FiringDamage);
            }
        }
    }

    public void ApplyDataTo(EntityStatPropSet target)
    {
        #region 耐久

        HealthDurability.ApplyDataTo(target.HealthDurability);
        MaxHealthDurability.ApplyDataTo(target.MaxHealthDurability);
        DropFromAirSurviveProbabilityPercent.ApplyDataTo(target.DropFromAirSurviveProbabilityPercent);

        #endregion

        #region 碰撞

        ActorCollideDamageDefense.ApplyDataTo(target.ActorCollideDamageDefense);
        BoxCollideDamageSelf.ApplyDataTo(target.BoxCollideDamageSelf);
        CollideDamage.ApplyDataTo(target.CollideDamage);

        #endregion

        #region 爆炸

        ExplodeDamageDefense.ApplyDataTo(target.ExplodeDamageDefense);

        #endregion

        #region 冰冻

        FrozenResistance.ApplyDataTo(target.FrozenResistance);
        FrozenValue.ApplyDataTo(target.FrozenValue);
        FrozenLevel.ApplyDataTo(target.FrozenLevel);
        target.FrozenFX = FrozenFX;
        target.FrozenFXScaleCurve = FrozenFXScaleCurve; // 风险，此处没有深拷贝

        #endregion

        #region 燃烧

        FiringResistance.ApplyDataTo(target.FiringResistance);
        FiringValue.ApplyDataTo(target.FiringValue);
        FiringLevel.ApplyDataTo(target.FiringLevel);
        FiringSpreadPercent.ApplyDataTo(target.FiringSpreadPercent);
        FiringDamageDefense.ApplyDataTo(target.FiringDamageDefense);
        target.StartFiringFX = StartFiringFX;
        target.StartFiringFXScale = StartFiringFXScale;
        target.FiringFX = FiringFX;
        target.FiringFXScaleCurve = FiringFXScaleCurve; // 风险，此处没有深拷贝
        target.FiringBreakFX = FiringBreakFX;
        target.FiringBreakFXScale = FiringBreakFXScale;

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
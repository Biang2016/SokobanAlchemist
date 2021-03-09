using System;
using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;

[Serializable]
public class EntityStatPropSet
{
    public EntityStatPropSet()
    {
        InitDelegates();
    }

    internal Entity Entity;

    public Dictionary<EntityStatType, EntityStat> StatDict = new Dictionary<EntityStatType, EntityStat>(20);
    public Dictionary<EntityPropertyType, EntityProperty> PropertyDict = new Dictionary<EntityPropertyType, EntityProperty>(20);

    #region 财产

    [BoxGroup("耐久")]
    [LabelText("@\"当前金子\t\"+Gold")]
    public EntityStat Gold = new EntityStat(EntityStatType.Gold);

    #endregion

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

    [BoxGroup("碰撞")]
    [LabelText("@\"X轴碰撞伤害\t\"+CollideDamageX")]
    public EntityProperty CollideDamageX = new EntityProperty(EntityPropertyType.CollideDamageX);

    [BoxGroup("碰撞")]
    [LabelText("@\"Z轴碰撞伤害\t\"+CollideDamageZ")]
    public EntityProperty CollideDamageZ = new EntityProperty(EntityPropertyType.CollideDamageZ);

    [BoxGroup("碰撞")]
    [LabelText("@\"被碰撞硬直ms\t\"+BeCollidedHitStopDuration")]
    public EntityProperty BeCollidedHitStopDuration = new EntityProperty(EntityPropertyType.BeCollidedHitStopDuration);

    public EntityProperty GetCollideDamageByAxis(Box.KickAxis axis)
    {
        if (axis == Box.KickAxis.X) return CollideDamageX;
        if (axis == Box.KickAxis.Z) return CollideDamageZ;
        else return CollideDamage;
    }

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
    [LabelText("@\"冰冻伤害抵消\t\"+FrozenDamageDefense")]
    public EntityProperty FrozenDamageDefense = new EntityProperty(EntityPropertyType.FrozenDamageDefense);

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

    internal float FiringValuePerLevel => ((float) FiringValue.MaxValue / FiringLevel.MaxValue);

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
    public class SkillPropertyCollection
    {
        public Dictionary<EntitySkillPropertyType, EntityProperty> PropertyDict = new Dictionary<EntitySkillPropertyType, EntityProperty>();

        [SerializeField]
        [LabelText("技能名备注")]
        private string skillAlias = "";

        [LabelText("@\"施法正方形范围边长\t\"+CastingRadius")]
        public EntityProperty CastingRadius = new EntityProperty(EntitySkillPropertyType.CastingRadius);

        [LabelText("@\"效果半径\t\"+EffectRadius")]
        public EntityProperty EffectRadius = new EntityProperty(EntitySkillPropertyType.EffectRadius);

        [LabelText("释放区域偏移")]
        public GridPos3D Offset = GridPos3D.Zero;

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

        public void Setup()
        {
            if (PropertyDict.Count == 0)
            {
                PropertyDict.Add(EntitySkillPropertyType.CastingRadius, CastingRadius);
                PropertyDict.Add(EntitySkillPropertyType.EffectRadius, EffectRadius);
                PropertyDict.Add(EntitySkillPropertyType.Width, Width);
                PropertyDict.Add(EntitySkillPropertyType.Depth, Depth);
                PropertyDict.Add(EntitySkillPropertyType.Cooldown, Cooldown);
                PropertyDict.Add(EntitySkillPropertyType.WingUp, WingUp);
                PropertyDict.Add(EntitySkillPropertyType.CastDuration, CastDuration);
                PropertyDict.Add(EntitySkillPropertyType.Recovery, Recovery);
            }
        }

        public void Initialize()
        {
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
        }

        public void ApplyDataTo(SkillPropertyCollection target)
        {
            CastingRadius.ApplyDataTo(target.CastingRadius);
            target.Offset = Offset;
            EffectRadius.ApplyDataTo(target.EffectRadius);
            Width.ApplyDataTo(target.Width);
            Depth.ApplyDataTo(target.Depth);
            Cooldown.ApplyDataTo(target.Cooldown);
            WingUp.ApplyDataTo(target.WingUp);
            CastDuration.ApplyDataTo(target.CastDuration);
            Recovery.ApplyDataTo(target.Recovery);
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
        Profiler.BeginSample("ESPS #1");
        Entity = entity;

        #region Property初始化

        #region 耐久

        MaxHealthDurability.Initialize();

        #endregion

        #region 碰撞

        ActorCollideDamageDefense.Initialize();
        BoxCollideDamageSelf.Initialize();
        CollideDamage.Initialize();
        CollideDamageX.Initialize();
        CollideDamageZ.Initialize();
        BeCollidedHitStopDuration.Initialize();

        #endregion

        #region 爆炸

        ExplodeDamageDefense.Initialize();

        #endregion

        #region 冰冻

        FrozenResistance.Initialize();
        FrozenDamageDefense.Initialize();

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

        Profiler.EndSample();
        Profiler.BeginSample("ESPS #2");

        Profiler.BeginSample("ESPS #2-1");

        #region 财产

        StatDict.Add(EntityStatType.Gold, Gold);
        if (entity is Actor) Gold.OnValueIncrease = OnGoldIncreaseAction;

        #endregion

        Profiler.EndSample();

        Profiler.BeginSample("ESPS #2-2");

        #region 耐久

        HealthDurability.MaxValue = MaxHealthDurability.GetModifiedValue;
        if (entity is Actor)
        {
            HealthDurability.OnValueDecrease = OnHealthDurabilityDecreaseAction;
            HealthDurability.OnValueIncrease = OnHealthDurabilityIncreaseAction;
        }

        HealthDurability.OnValueReachZero = OnHealthDurabilityReachZeroAction;
        StatDict.Add(EntityStatType.HealthDurability, HealthDurability);
        MaxHealthDurability.OnValueChanged = OnMaxHealthDurabilityChangedAction;
        PropertyDict.Add(EntityPropertyType.MaxHealthDurability, MaxHealthDurability);

        StatDict.Add(EntityStatType.DropFromAirSurviveProbabilityPercent, DropFromAirSurviveProbabilityPercent);

        #endregion

        Profiler.EndSample();

        Profiler.BeginSample("ESPS #2-3");

        #region 碰撞

        PropertyDict.Add(EntityPropertyType.ActorCollideDamageDefense, ActorCollideDamageDefense);
        PropertyDict.Add(EntityPropertyType.BoxCollideDamageSelf, BoxCollideDamageSelf);
        PropertyDict.Add(EntityPropertyType.CollideDamage, CollideDamage);
        PropertyDict.Add(EntityPropertyType.CollideDamageX, CollideDamageX);
        PropertyDict.Add(EntityPropertyType.CollideDamageZ, CollideDamageZ);
        PropertyDict.Add(EntityPropertyType.BeCollidedHitStopDuration, BeCollidedHitStopDuration);

        #endregion

        Profiler.EndSample();

        Profiler.BeginSample("ESPS #2-4");

        #region 爆炸

        PropertyDict.Add(EntityPropertyType.ExplodeDamageDefense, ExplodeDamageDefense);

        #endregion

        Profiler.EndSample();

        Profiler.BeginSample("ESPS #2-5");

        #region 冰冻

        FrozenResistance.OnValueChanged = OnFrozenResistanceChangedAction;
        PropertyDict.Add(EntityPropertyType.FrozenResistance, FrozenResistance);
        PropertyDict.Add(EntityPropertyType.FrozenDamageDefense, FrozenDamageDefense);

        FrozenValue.AbnormalStatResistance = FrozenResistance.GetModifiedValue;
        if (FrozenValue.AbnormalStatResistance < 200)
        {
            FrozenValue.OnValueChanged = OnFrozenValueChangedAction;
            FrozenValue.OnValueIncrease = OnFrozenValueIncreaseAction;
            FrozenLevel.OnValueChanged = Entity.EntityFrozenHelper.OnFrozeIntoIceBlockAction;
        }

        StatDict.Add(EntityStatType.FrozenValue, FrozenValue);
        StatDict.Add(EntityStatType.FrozenLevel, FrozenLevel);

        #endregion

        Profiler.EndSample();

        Profiler.BeginSample("ESPS #2-6");

        #region 燃烧

        FiringResistance.OnValueChanged = OnFiringResistanceChangedAction;
        PropertyDict.Add(EntityPropertyType.FiringResistance, FiringResistance);

        FiringValue.AbnormalStatResistance = FiringResistance.GetModifiedValue;
        if (FiringValue.AbnormalStatResistance < 200)
        {
            FiringValue.OnValueChanged = OnFiringValueChangedAction;
            FiringValue.OnValueIncrease = OnFiringValueIncreaseAction;
            FiringLevel.OnValueChanged = OnFiringLevelChangedAction;
        }

        StatDict.Add(EntityStatType.FiringValue, FiringValue);
        StatDict.Add(EntityStatType.FiringLevel, FiringLevel);
        PropertyDict.Add(EntityPropertyType.FiringSpreadPercent, FiringSpreadPercent);
        PropertyDict.Add(EntityPropertyType.FiringDamageDefense, FiringDamageDefense);

        #endregion

        Profiler.EndSample();

        Profiler.BeginSample("ESPS #2-7");

        #region 操作

        PropertyDict.Add(EntityPropertyType.MoveSpeed, MoveSpeed);

        if (entity is Actor) MaxActionPoint.OnValueChanged = OnMaxActionPointChanged;
        PropertyDict.Add(EntityPropertyType.MaxActionPoint, MaxActionPoint);

        ActionPoint.MaxValue = MaxActionPoint.GetModifiedValue;
        ActionPoint.AutoChange = ActionPointRecovery.GetModifiedValue / 100f;
        StatDict.Add(EntityStatType.ActionPoint, ActionPoint);

        if (entity is Actor) ActionPointRecovery.OnValueChanged = OnActionPointRecoveryChangedAction;
        PropertyDict.Add(EntityPropertyType.ActionPointRecovery, ActionPointRecovery);

        PropertyDict.Add(EntityPropertyType.KickConsumeActionPoint, KickConsumeActionPoint);

        PropertyDict.Add(EntityPropertyType.DashConsumeActionPoint, DashConsumeActionPoint);

        PropertyDict.Add(EntityPropertyType.VaultConsumeActionPoint, VaultConsumeActionPoint);

        #endregion

        Profiler.EndSample();
        Profiler.EndSample();

        Profiler.BeginSample("ESPS #3");
        foreach (SkillPropertyCollection spc in SkillsPropertyCollections)
        {
            spc.Initialize();
        }

        Profiler.EndSample();

        Profiler.BeginSample("ESPS #4");
        if (Entity.name.Contains("LightningEnemy"))
        {
            int a = 0;
        }

        foreach (EntityBuff rawEntityDefaultBuff in RawEntityDefaultBuffs)
        {
            Entity.EntityBuffHelper.AddBuff(rawEntityDefaultBuff.Clone());
        }

        Profiler.EndSample();
    }

    #region Delegates

    private void InitDelegates()
    {
        OnGoldIncreaseAction = OnGoldIncrease;
        OnHealthDurabilityDecreaseAction = OnHealthDurabilityDecrease;
        OnHealthDurabilityIncreaseAction = OnHealthDurabilityIncrease;
        OnHealthDurabilityReachZeroAction = OnHealthDurabilityReachZero;
        OnMaxHealthDurabilityChangedAction = OnMaxHealthDurabilityChanged;
        OnFrozenResistanceChangedAction = OnFrozenResistanceChanged;
        OnFrozenValueChangedAction = OnFrozenValueChanged;
        OnFrozenValueIncreaseAction = OnFrozenValueIncrease;
        OnFiringResistanceChangedAction = OnFiringResistanceChanged;
        OnFiringValueChangedAction = OnFiringValueChanged;
        OnFiringValueIncreaseAction = OnFiringValueIncrease;
        OnFiringLevelChangedAction = OnFiringLevelChanged;
        OnActionPointRecoveryChangedAction = OnActionPointRecoveryChanged;
        OnMaxActionPointChangedAction = OnMaxActionPointChanged;
    }

    private UnityAction<int> OnGoldIncreaseAction;

    private void OnGoldIncrease(int increase)
    {
        if (Entity is Actor actor)
        {
            actor.ActorBattleHelper.ShowGainGoldNumFX(increase);
        }
    }

    private UnityAction<int> OnHealthDurabilityDecreaseAction;

    private void OnHealthDurabilityDecrease(int decrease)
    {
        if (Entity is Actor actor)
        {
            actor.ActorBattleHelper.ShowDamageNumFX(decrease);
        }
    }

    private UnityAction<int> OnHealthDurabilityIncreaseAction;

    private void OnHealthDurabilityIncrease(int increase)
    {
        if (Entity is Actor actor)
        {
            actor.ActorBattleHelper.ShowHealNumFX(increase);
        }
    }

    private UnityAction<string> OnHealthDurabilityReachZeroAction;

    private void OnHealthDurabilityReachZero(string changeInfo)
    {
        Entity.PassiveSkillMarkAsDestroyed = true;
        foreach (EntityBuffAttribute attribute in Enum.GetValues(typeof(EntityBuffAttribute)))
        {
            if (changeInfo.Contains($"Damage-{attribute}"))
            {
                foreach (EntityPassiveSkill eps in Entity.EntityPassiveSkills)
                {
                    eps.OnDestroyEntityByElementDamage(attribute);
                }
            }
        }

        if (Entity is Actor actor) actor.ActorBattleHelper.DestroyActor();
    }

    private UnityAction<int, int> OnMaxHealthDurabilityChangedAction;

    private void OnMaxHealthDurabilityChanged(int before, int after)
    {
        HealthDurability.MaxValue = after;
    }

    private UnityAction<int, int> OnFrozenResistanceChangedAction;

    private void OnFrozenResistanceChanged(int before, int after)
    {
        FrozenValue.AbnormalStatResistance = after;
    }

    private UnityAction<int, int> OnFrozenValueChangedAction;

    private void OnFrozenValueChanged(int before, int after)
    {
        FrozenLevel.SetValue(after / FrozenValuePerLevel, "FrozenValueChange");
        if (FrozenLevel.Value > 0) Entity.EntityBuffHelper.PlayAbnormalStatFX((int) EntityStatType.FrozenValue, FrozenFX, FrozenFXScaleCurve.Evaluate(FrozenLevel.Value)); // 冰冻值变化时，播放一次特效
    }

    private UnityAction<int> OnFrozenValueIncreaseAction;

    private void OnFrozenValueIncrease(int increase)
    {
        FiringValue.SetValue(0);
    }

    private UnityAction<int, int> OnFiringResistanceChangedAction;

    private void OnFiringResistanceChanged(int before, int after)
    {
        FiringValue.AbnormalStatResistance = after;
    }

    private UnityAction<int, int> OnFiringValueChangedAction;

    private void OnFiringValueChanged(int before, int after)
    {
        FiringLevel.SetValue(Mathf.RoundToInt(after / FiringValuePerLevel), "FiringValueChange");
        if (FiringLevel.Value > 0)
            Entity.EntityBuffHelper.PlayAbnormalStatFX((int) EntityStatType.FiringValue, FiringFX, FiringFXScaleCurve.Evaluate(FiringLevel.Value)); // 燃烧值变化时，播放一次特效
        else if (after == 0) Entity.EntityBuffHelper.RemoveAbnormalStatFX((int) EntityStatType.FiringValue);
    }

    private UnityAction<int> OnFiringValueIncreaseAction;

    private void OnFiringValueIncrease(int increase)
    {
        FrozenValue.SetValue(FrozenValue.Value - increase);
    }

    private UnityAction<int, int> OnFiringLevelChangedAction;

    private void OnFiringLevelChanged(int before, int after)
    {
        if (before == 0 && after > 0)
        {
            if (Entity is Box box)
            {
                foreach (GridPos3D offset in box.GetEntityOccupationGPs_Rotated())
                {
                    FX fx = FXManager.Instance.PlayFX(StartFiringFX, box.transform.position + Vector3.up * 0.5f + offset, StartFiringFXScale);
                    if (fx) fx.transform.parent = box.transform;
                }
            }
        }
    }

    private UnityAction<int, int> OnActionPointRecoveryChangedAction;

    private void OnActionPointRecoveryChanged(int before, int after)
    {
        ActionPoint.AutoChange = after / 100f;
    }

    private UnityAction<int, int> OnMaxActionPointChangedAction;

    private void OnMaxActionPointChanged(int before, int after)
    {
        ActionPoint.MaxValue = after;
    }

    #endregion

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

    public void FixedUpdate(float fixedDeltaTime) // Actor更新频率为每帧，Box更新频率为每秒, 带SlowTick标签的Box更新频率为每2秒
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
                        adjacentBox.EntityStatPropSet.FiringValue.SetValue(adjacentBox.EntityStatPropSet.FiringValue.Value + Mathf.RoundToInt(diff * FiringSpreadPercent.GetModifiedValue / 100f), "FiringSpread");
                    }
                }

                Entity.EntityBuffHelper.Damage(FiringLevel.Value, EntityBuffAttribute.FiringDamage);
            }
        }
    }

    public void ApplyDataTo(EntityStatPropSet target)
    {
        #region 财产

        Gold.ApplyDataTo(target.Gold);

        #endregion

        #region 耐久

        HealthDurability.ApplyDataTo(target.HealthDurability);
        MaxHealthDurability.ApplyDataTo(target.MaxHealthDurability);
        DropFromAirSurviveProbabilityPercent.ApplyDataTo(target.DropFromAirSurviveProbabilityPercent);

        #endregion

        #region 碰撞

        ActorCollideDamageDefense.ApplyDataTo(target.ActorCollideDamageDefense);
        BoxCollideDamageSelf.ApplyDataTo(target.BoxCollideDamageSelf);
        CollideDamage.ApplyDataTo(target.CollideDamage);
        CollideDamageX.ApplyDataTo(target.CollideDamageX);
        CollideDamageZ.ApplyDataTo(target.CollideDamageZ);
        BeCollidedHitStopDuration.ApplyDataTo(target.BeCollidedHitStopDuration);

        #endregion

        #region 爆炸

        ExplodeDamageDefense.ApplyDataTo(target.ExplodeDamageDefense);

        #endregion

        #region 冰冻

        FrozenResistance.ApplyDataTo(target.FrozenResistance);
        FrozenValue.ApplyDataTo(target.FrozenValue);
        FrozenLevel.ApplyDataTo(target.FrozenLevel);
        FrozenDamageDefense.ApplyDataTo(target.FrozenDamageDefense);
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

        // 性能考虑，进行数据拷贝

        if (target.SkillsPropertyCollections.Count == 0)
        {
            for (int i = 0; i < SkillsPropertyCollections.Count; i++)
            {
                SkillPropertyCollection targetSRC = new SkillPropertyCollection();
                target.SkillsPropertyCollections.Add(targetSRC);
                SkillsPropertyCollections[i].Setup();
                targetSRC.Setup();
                SkillsPropertyCollections[i].ApplyDataTo(targetSRC);
            }
        }
        else
        {
            if (target.SkillsPropertyCollections.Count == SkillsPropertyCollections.Count)
            {
                // 性能考虑，进行数据拷贝
                for (int i = 0; i < SkillsPropertyCollections.Count; i++)
                {
                    SkillPropertyCollection targetSRC = target.SkillsPropertyCollections[i];
                    SkillsPropertyCollections[i].Setup();
                    targetSRC.Setup();
                    SkillsPropertyCollections[i].ApplyDataTo(targetSRC);
                }
            }
            else
            {
                Debug.Log("技能属性数据，源数据和目标数据数量不符");
            }
        }

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
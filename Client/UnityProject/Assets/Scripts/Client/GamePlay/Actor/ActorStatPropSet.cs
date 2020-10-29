using System;
using System.Collections.Generic;
using System.Text;
using BiangStudio.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class ActorStatPropSet : IClone<ActorStatPropSet>
{
    internal Actor Actor;

    public Dictionary<ActorStat.StatType, ActorStat> StatDict = new Dictionary<ActorStat.StatType, ActorStat>();
    public Dictionary<ActorProperty.PropertyType, ActorProperty> PropertyDict = new Dictionary<ActorProperty.PropertyType, ActorProperty>();

    [LabelText("@\"血量\t\"+Health")]
    public ActorStat Health = new ActorStat(ActorStat.StatType.Health);

    [LabelText("@\"血量上限\t\"+MaxHealth")]
    public ActorProperty MaxHealth = new ActorProperty(ActorProperty.PropertyType.MaxHealth);

    [LabelText("@\"生命数\t\"+Life")]
    public ActorStat Life = new ActorStat(ActorStat.StatType.Life);

    [LabelText("@\"移动速度\t\"+MoveSpeed")]
    public ActorProperty MoveSpeed = new ActorProperty(ActorProperty.PropertyType.MoveSpeed);

    [BoxGroup("冰冻")]
    [LabelText("@\"冰冻抗性\t\"+FrozenResistance")]
    public ActorProperty FrozenResistance = new ActorProperty(ActorProperty.PropertyType.FrozenResistance);

    [BoxGroup("冰冻")]
    [LabelText("@\"冰冻累积值\t\"+FrozenValue")]
    public ActorStat FrozenValue = new ActorStat(ActorStat.StatType.FrozenValue);

    [BoxGroup("冰冻")]
    [LabelText("冰冻等级")]
    public ActorStat FrozenLevel = new ActorStat(ActorStat.StatType.FrozenLevel);

    [BoxGroup("冰冻")]
    [LabelText("冰冻持续特效")]
    [ValueDropdown("GetAllFXTypeNames", DropdownTitle = "选择FX类型")]
    public string FrozenFX;

    [BoxGroup("冰冻")]
    [LabelText("冰冻持续特效尺寸(x->冰冻等级")]
    public AnimationCurve FrozenFXScaleCurve;

    [BoxGroup("灼烧")]
    [LabelText("@\"灼烧抗性\t\"+FiringResistance")]
    public ActorProperty FiringResistance = new ActorProperty(ActorProperty.PropertyType.FiringResistance);

    [BoxGroup("灼烧")]
    [LabelText("@\"灼烧累积值\t\"+FiringValue")]
    public ActorStat FiringValue = new ActorStat(ActorStat.StatType.FiringValue);

    [BoxGroup("灼烧")]
    [LabelText("灼烧等级")]
    public ActorStat FiringLevel = new ActorStat(ActorStat.StatType.FiringLevel);

    [BoxGroup("灼烧")]
    [LabelText("灼烧持续特效")]
    [ValueDropdown("GetAllFXTypeNames", DropdownTitle = "选择FX类型")]
    public string FiringFX;

    [BoxGroup("灼烧")]
    [LabelText("灼烧持续特效尺寸(x->灼烧等级")]
    public AnimationCurve FiringFXScaleCurve;

    public void Initialize(Actor actor)
    {
        Actor = actor;

        Health.OnValueReachZero += () => { Life.Value--; };
        StatDict.Add(ActorStat.StatType.Health, Health);

        MaxHealth.Initialize();
        MaxHealth.OnValueChanged += (before, after) => { Health.MaxValue = after; };
        PropertyDict.Add(ActorProperty.PropertyType.MaxHealth, MaxHealth);

        Life.OnValueDecrease += (_) => { Health.ChangeValueWithoutNotify(Health.MaxValue - Health.Value); };
        StatDict.Add(ActorStat.StatType.Life, Life);

        MoveSpeed.Initialize();
        PropertyDict.Add(ActorProperty.PropertyType.MoveSpeed, MoveSpeed);

        FrozenResistance.Initialize();
        FrozenResistance.OnValueChanged += (before, after) => { FrozenValue.AbnormalResistance = after; };
        PropertyDict.Add(ActorProperty.PropertyType.FrozenResistance, FrozenResistance);

        FrozenValue.AbnormalResistance = FrozenResistance.GetModifiedValue;
        FrozenValue.OnValueChanged += (before, after) =>
        {
            FrozenLevel.Value = Mathf.FloorToInt(after / ((float) FrozenValue.MaxValue / FrozenLevel.MaxValue));
            if (FrozenLevel.Value > 0) Actor.ActorBuffHelper.PlayAbnormalStatFX(ActorStat.StatType.FrozenValue, FrozenFX, FrozenFXScaleCurve.Evaluate(FrozenLevel.Value)); // 冰冻值变化时，播放一次特效
        };
        StatDict.Add(ActorStat.StatType.FrozenValue, FrozenValue);

        FrozenLevel.OnValueChanged += (before, after) => { ; }; // todo 冰冻等级变化时，角色冰块形态发生变化
        StatDict.Add(ActorStat.StatType.FrozenLevel, FrozenLevel);

        FiringResistance.Initialize();
        FiringResistance.OnValueChanged += (before, after) => { FiringValue.AbnormalResistance = after; };
        PropertyDict.Add(ActorProperty.PropertyType.FiringResistance, FiringResistance);

        FiringValue.AbnormalResistance = FiringResistance.GetModifiedValue;
        FiringValue.OnValueChanged += (before, after) =>
        {
            FiringLevel.Value = Mathf.FloorToInt(after / ((float) FiringValue.MaxValue / FiringLevel.MaxValue));
            if (FiringLevel.Value > 0) Actor.ActorBuffHelper.PlayAbnormalStatFX(ActorStat.StatType.FiringValue, FiringFX, FiringFXScaleCurve.Evaluate(FiringLevel.Value)); // 灼烧值变化时，播放一次特效
        };
        StatDict.Add(ActorStat.StatType.FiringValue, FiringValue);

        StatDict.Add(ActorStat.StatType.FiringLevel, FiringLevel);
    }

    public void OnRecycled()
    {
        foreach (KeyValuePair<ActorStat.StatType, ActorStat> kv in StatDict)
        {
            kv.Value.ClearCallBacks();
        }

        StatDict.Clear();
        foreach (KeyValuePair<ActorProperty.PropertyType, ActorProperty> kv in PropertyDict)
        {
            kv.Value.ClearCallBacks();
        }

        PropertyDict.Clear();
    }

    private float abnormalStateAutoTick = 0f;
    private int abnormalStateAutoTickInterval = 1; // 异常状态值每秒降低

    public void FixedUpdate(float fixedDeltaTime)
    {
        foreach (KeyValuePair<ActorStat.StatType, ActorStat> kv in StatDict)
        {
            kv.Value.AbnormalStatFixedUpdate(fixedDeltaTime);
        }

        abnormalStateAutoTick += fixedDeltaTime;
        if (abnormalStateAutoTick > abnormalStateAutoTickInterval)
        {
            abnormalStateAutoTick -= abnormalStateAutoTickInterval;

            Actor.ActorBattleHelper.Damage(Actor, FiringLevel.Value);
        }

        foreach (KeyValuePair<ActorProperty.PropertyType, ActorProperty> kv in PropertyDict)
        {
            kv.Value.RefreshModifiedValue();
        }
    }

    public ActorStatPropSet Clone()
    {
        ActorStatPropSet newStatPropSet = new ActorStatPropSet();
        newStatPropSet.Health = Health.Clone();
        newStatPropSet.MaxHealth = MaxHealth.Clone();
        newStatPropSet.Life = Life.Clone();
        newStatPropSet.MoveSpeed = MoveSpeed.Clone();
        newStatPropSet.FrozenResistance = FrozenResistance.Clone();
        newStatPropSet.FrozenValue = FrozenValue.Clone();
        newStatPropSet.FrozenLevel = FrozenLevel.Clone();
        newStatPropSet.FrozenFX = FrozenFX;
        newStatPropSet.FrozenFXScaleCurve = FrozenFXScaleCurve; // 风险，此处没有深拷贝
        newStatPropSet.FiringResistance = FiringResistance.Clone();
        newStatPropSet.FiringValue = FiringValue.Clone();
        newStatPropSet.FiringLevel = FiringLevel.Clone();
        newStatPropSet.FiringFX = FiringFX;
        newStatPropSet.FiringFXScaleCurve = FiringFXScaleCurve; // 风险，此处没有深拷贝
        return newStatPropSet;
    }

    #region Utils

    private IEnumerable<string> GetAllFXTypeNames => ConfigManager.GetAllFXTypeNames();

    #endregion
}

/// <summary>
/// ActorProperty是根据加或乘Modifier来增减、倍增的量，如最大生命值、速度、攻击等
/// </summary>
[Serializable]
public class ActorProperty : IClone<ActorProperty>
{
    public enum PropertyType
    {
        [LabelText("血量上限")]
        MaxHealth = 0,

        [LabelText("移动速度")]
        MoveSpeed = 1,

        [LabelText("冰冻抗性")]
        FrozenResistance = 100,

        [LabelText("灼烧抗性")]
        FiringResistance = 101,
    }

    public ActorProperty(PropertyType propertyType)
    {
        m_PropertyType = propertyType;
    }

    public void Initialize()
    {
        RefreshModifiedValue();
    }

    public void ClearCallBacks()
    {
        OnChanged = null;
        OnValueChanged = null;
        OnValueIncrease = null;
        OnValueDecrease = null;
    }

    public UnityAction<int, int, int> OnChanged;

    public UnityAction<int, int> OnValueChanged;
    public UnityAction<int> OnValueIncrease;
    public UnityAction<int> OnValueDecrease;

    internal PropertyType m_PropertyType;

    #region Modifiers

    private SortedDictionary<uint, PlusModifier> PlusModifiers_Value = new SortedDictionary<uint, PlusModifier>();
    private SortedDictionary<uint, MultiplyModifier> MultiplyModifiers_Value = new SortedDictionary<uint, MultiplyModifier>();

    public bool AddModifier(PlusModifier modifier)
    {
        if (!PlusModifiers_Value.ContainsKey(modifier.GUID))
        {
            PlusModifiers_Value.Add(modifier.GUID, modifier);
            RefreshModifiedValue();
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool RemoveModifier(PlusModifier modifier)
    {
        if (PlusModifiers_Value.Remove(modifier.GUID))
        {
            RefreshModifiedValue();
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool AddModifier(MultiplyModifier modifier)
    {
        if (!MultiplyModifiers_Value.ContainsKey(modifier.GUID))
        {
            MultiplyModifiers_Value.Add(modifier.GUID, modifier);
            RefreshModifiedValue();
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool RemoveModifier(MultiplyModifier modifier)
    {
        if (MultiplyModifiers_Value.Remove(modifier.GUID))
        {
            foreach (KeyValuePair<uint, MultiplyModifier> kv in MultiplyModifiers_Value)
            {
                kv.Value.CoverModifiersGUID.Remove(modifier.GUID);
            }

            RefreshModifiedValue();
            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion

    [SerializeField]
    [LabelText("基准值")]
    private int BaseValue;

    public int GetBaseValue => BaseValue;

    [SerializeField]
    [LabelText("修正值")]
    [HideInEditorMode]
    private int ModifiedValue;

    public int GetModifiedValue => ModifiedValue;

    public void RefreshModifiedValue()
    {
        float finalValue = BaseValue;
        foreach (KeyValuePair<uint, PlusModifier> kv in PlusModifiers_Value)
        {
            if (kv.Value.Covered) continue;
            finalValue += kv.Value.Delta;
        }

        foreach (KeyValuePair<uint, MultiplyModifier> kv in MultiplyModifiers_Value)
        {
            if (kv.Value.Covered) continue;
            finalValue *= (100 + kv.Value.Percent) / 100f;
        }

        int finalValue_Int = Mathf.RoundToInt(finalValue);
        finalValue_Int = Mathf.Clamp(finalValue_Int, MinValue, MaxValue);
        if (ModifiedValue != finalValue_Int)
        {
            int before = ModifiedValue;
            ModifiedValue = finalValue_Int;
            OnValueChanged?.Invoke(before, ModifiedValue);
            if (before < ModifiedValue) OnValueIncrease?.Invoke(ModifiedValue - before);
            if (before > ModifiedValue) OnValueDecrease?.Invoke(before - ModifiedValue);
            OnChanged?.Invoke(ModifiedValue, MinValue, MaxValue);
        }
    }

    [SerializeField]
    [LabelText("下限")]
    private int MinValue;

    [SerializeField]
    [LabelText("上限")]
    private int MaxValue;

    public ActorProperty Clone()
    {
        ActorProperty newProp = new ActorProperty(m_PropertyType);
        newProp.BaseValue = BaseValue;
        newProp.MinValue = MinValue;
        newProp.MaxValue = MaxValue;
        return newProp;
    }

    private StringBuilder sb;

    public override string ToString()
    {
        if (sb == null) sb = new StringBuilder();
        sb.Clear();
        if (PlusModifiers_Value.Count > 0) sb.Append("(");
        sb.Append(BaseValue);
        if (PlusModifiers_Value.Count > 0)
        {
            foreach (KeyValuePair<uint, PlusModifier> kv in PlusModifiers_Value)
            {
                sb.Append(kv.Value);
            }

            sb.Append(")");
        }

        foreach (KeyValuePair<uint, MultiplyModifier> kv in MultiplyModifiers_Value)
        {
            sb.Append(kv.Value);
        }

        sb.Append($" = {ModifiedValue} ~ [{MinValue}, {MaxValue}]");
        return sb.ToString();
    }

    [Serializable]
    public abstract class Modifier
    {
        #region GUID

        [ReadOnly]
        [HideInEditorMode]
        public uint GUID;

        private static uint guidGenerator = (uint) ConfigManager.GUID_Separator.PropertyModifier;

        protected uint GetGUID()
        {
            return guidGenerator++;
        }

        #endregion

        protected Modifier()
        {
            GUID = GetGUID();
        }

        internal bool Covered => CoverModifiersGUID.Count > 0;

        /// <summary>
        /// 此Modifier被哪些Modifier用<see cref="ActorBuffAttributeRelationship.MaxDominant"/>屏蔽了
        /// </summary>
        internal HashSet<uint> CoverModifiersGUID = new HashSet<uint>();

        public abstract bool CanCover(Modifier target);
    }

    [Serializable]
    public class MultiplyModifier : Modifier
    {
        public int Percent;

        public override string ToString()
        {
            return $" * {(Covered ? "(Covered:" : "")}{100 + Percent}%{(Covered ? ")" : "")}";
        }

        public override bool CanCover(Modifier target)
        {
            MultiplyModifier targetMultiplyModifier = (MultiplyModifier) target;
            return Mathf.Abs(Percent) >= Mathf.Abs(targetMultiplyModifier.Percent);
        }
    }

    [Serializable]
    public class PlusModifier : Modifier
    {
        public int Delta;

        public override string ToString()
        {
            if (Delta >= 0)
            {
                return $" + {(Covered ? "(Covered:" : "")}{Delta}{(Covered ? ")" : "")}";
            }
            else
            {
                return $" - {(Covered ? "(Covered:" : "")}{-Delta}{(Covered ? ")" : "")}";
            }
        }

        public override bool CanCover(Modifier target)
        {
            PlusModifier targetPlusModifier = (PlusModifier) target;
            return Mathf.Abs(Delta) >= Mathf.Abs(targetPlusModifier.Delta);
        }
    }
}

/// <summary>
/// ActorStat是可自由消耗、增加的量，如生命值，命数等
/// </summary>
[Serializable]
public class ActorStat : IClone<ActorStat>
{
    public enum StatType
    {
        [LabelText("血量")]
        Health = 0,

        [LabelText("生命数")]
        Life = 1,

        [LabelText("冰冻累积值")]
        FrozenValue = 100,

        [LabelText("冰冻等级")]
        FrozenLevel = 120,

        [LabelText("灼烧累积值")]
        FiringValue = 101,

        [LabelText("灼烧等级")]
        FiringLevel = 121,
    }

    public ActorStat(StatType statType)
    {
        m_StatType = statType;
    }

    public void ClearCallBacks()
    {
        OnChanged = null;
        OnValueChanged = null;
        OnValueIncrease = null;
        OnValueDecrease = null;
        OnValueReachMin = null;
        OnValueReachMax = null;
        OnValueReachZero = null;
        OnMinValueChanged = null;
        OnMaxValueChanged = null;
    }

    public UnityAction<int, int, int> OnChanged;

    public UnityAction<int, int> OnValueChanged;
    public UnityAction<int> OnValueIncrease;
    public UnityAction<int> OnValueDecrease;
    public UnityAction<int> OnValueReachMin;
    public UnityAction<int> OnValueReachMax;
    public UnityAction OnValueReachZero;

    internal StatType m_StatType;

    #region 异常状态

    internal int AbnormalResistance = 100; // 抗性，取值范围0~200，仅用于累积值的Property，如冰冻累积值，0为2倍弱冰，100为正常，150为50%免疫，200为100%免疫
    public bool IsAbnormalStat => m_StatType == StatType.FiringValue || m_StatType == StatType.FrozenValue;
    private float abnormalStateAutoTick = 0f;

    [LabelText("异常状态值自动衰减时间间隔/s")]
    [ShowIf("IsAbnormalStat")]
    public float AbnormalStateAutoTickInterval = 1f;

    public void AbnormalStatFixedUpdate(float fixedDeltaTime)
    {
        if (IsAbnormalStat)
        {
            abnormalStateAutoTick += fixedDeltaTime;
            if (abnormalStateAutoTick > AbnormalStateAutoTickInterval)
            {
                abnormalStateAutoTick -= AbnormalStateAutoTickInterval;
                Value -= Mathf.RoundToInt((AbnormalResistance == 0 ? 10 : AbnormalResistance) * AbnormalStateAutoTickInterval); // 保底衰减率为10/s
            }
        }
    }

    #endregion

    [SerializeField]
    [LabelText("当前值")]
    private int _value;

    public int Value
    {
        get { return _value; }
        set
        {
            value = Mathf.Clamp(value, _minValue, _maxValue);
            if (_value != value)
            {
                // 对异常状态累积值而言:
                // 抗性低则积累快，抗性高则积累慢，免疫则不积累。消退速度不受抗性影响
                if (IsAbnormalStat)
                {
                    if (_value < value)
                    {
                        value = Mathf.RoundToInt((value - _value) * (200f - AbnormalResistance) / 100f) + _value;
                        value = Mathf.Clamp(value, _minValue, _maxValue);
                    }
                }

                int before = _value;
                _value = value;
                OnValueChanged?.Invoke(before, _value);
                if (_value == _minValue) OnValueReachMin?.Invoke(_value);
                if (_value == _maxValue) OnValueReachMax?.Invoke(_value);
                if (_value == 0) OnValueReachZero?.Invoke();
                if (before < _value) OnValueIncrease?.Invoke(_value - before);
                if (before > _value) OnValueDecrease?.Invoke(before - _value);
                OnChanged?.Invoke(_value, _minValue, _maxValue);
            }
        }
    }

    public void ChangeValueWithoutNotify(int delta)
    {
        _value = Mathf.Clamp(_value + delta, _minValue, _maxValue);
    }

    public UnityAction<int, int> OnMinValueChanged;

    [SerializeField]
    [LabelText("最小值")]
    private int _minValue;

    public int MinValue
    {
        get { return _minValue; }
        set
        {
            value = Mathf.Clamp(value, 0, _maxValue);
            if (_minValue != value)
            {
                if (_value < value) Value = value;
                int before = _minValue;
                _minValue = value;
                OnMinValueChanged?.Invoke(before, _minValue);
                OnChanged?.Invoke(_value, _minValue, _maxValue);
            }
        }
    }

    public UnityAction<int, int> OnMaxValueChanged;

    [SerializeField]
    [LabelText("最大值")]
    private int _maxValue;

    public int MaxValue
    {
        get { return _maxValue; }
        set
        {
            value = Mathf.Clamp(value, _minValue, int.MaxValue);
            if (_maxValue != value)
            {
                if (_value > value) _value = value;
                int before = _maxValue;
                _maxValue = value;
                OnMaxValueChanged?.Invoke(before, _maxValue);
                OnChanged?.Invoke(_value, _minValue, _maxValue);
            }
        }
    }

    public ActorStat Clone()
    {
        ActorStat newStat = new ActorStat(m_StatType);
        newStat._value = _value;
        newStat._minValue = _minValue;
        newStat._maxValue = _maxValue;
        return newStat;
    }

    public override string ToString()
    {
        return $"{_value} ~ [{_minValue}, {_maxValue}]";
    }
}
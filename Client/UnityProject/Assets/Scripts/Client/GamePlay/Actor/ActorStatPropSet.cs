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

    [LabelText("@\"冰冻抗性\t\"+FrozenResistance")]
    public ActorProperty FrozenResistance = new ActorProperty(ActorProperty.PropertyType.FrozenResistance);

    [LabelText("@\"冰冻累积值\t\"+FrozenValue")]
    public ActorStat FrozenValue = new ActorStat(ActorStat.StatType.FrozenValue);

    [LabelText("@\"灼烧抗性\t\"+FiringResistance")]
    public ActorProperty FiringResistance = new ActorProperty(ActorProperty.PropertyType.FiringResistance);

    [LabelText("@\"灼烧累积值\t\"+FiringValue")]
    public ActorStat FiringValue = new ActorStat(ActorStat.StatType.FiringValue);

    public void Initialize()
    {
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
        FrozenResistance.OnValueChanged += (before, after) => { FrozenValue.Resistance = after; };
        PropertyDict.Add(ActorProperty.PropertyType.FrozenResistance, FrozenResistance);

        StatDict.Add(ActorStat.StatType.FrozenValue, FrozenValue);

        FiringResistance.Initialize();
        FiringResistance.OnValueChanged += (before, after) => { FiringValue.Resistance = after; };
        PropertyDict.Add(ActorProperty.PropertyType.FiringResistance, FiringResistance);

        StatDict.Add(ActorStat.StatType.FiringValue, FiringValue);
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

    public void FixedUpdate()
    {
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
        newStatPropSet.FiringResistance = FiringResistance.Clone();
        newStatPropSet.FiringValue = FiringValue.Clone();
        return newStatPropSet;
    }
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

        [LabelText("灼烧累积值")]
        FiringValue = 101,
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

    internal int Resistance = 100; // 抗性，取值范围0~200，仅用于累积值的Property，如冰冻累积值，0为2倍弱冰，100为正常，150为50%免疫，200为100%免疫

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
                // 对生命值不适用
                if (_value < value)
                {
                    value = Mathf.RoundToInt((value - _value) * (200f - Resistance) / 100f) + _value;
                    value = Mathf.Clamp(value, _minValue, _maxValue);
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
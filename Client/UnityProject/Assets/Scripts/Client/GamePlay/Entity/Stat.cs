﻿using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Stat是可自由消耗、增加的量，如生命值，命数等
/// </summary>
[Serializable]
public abstract class Stat
{
    public void OnRecycled()
    {
        Recovery = 0;
        GrowthPercent = 0;
        accumulatedRecovery = 0;
        AbnormalStatResistance = 100;
        _value = 0;
        _minValue = 0;
        _maxValue = 0;
        ClearCallBacks();
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

    #region 自动恢复

    /// <summary>
    /// 每秒恢复率
    /// </summary>
    internal float Recovery = 0;

    /// <summary>
    /// 每秒增长率percent
    /// </summary>
    internal int GrowthPercent = 0;

    private float accumulatedRecovery = 0;

    public void FixedUpdate(float fixedDeltaTime)
    {
        if (Value != 0 && GrowthPercent != 0)
        {
            int a = 0;
        }
        accumulatedRecovery += fixedDeltaTime * Recovery;
        accumulatedRecovery += fixedDeltaTime * (Value * GrowthPercent / 100f);

        if (accumulatedRecovery > 1)
        {
            int round = Mathf.FloorToInt(accumulatedRecovery);
            Value += round;
            accumulatedRecovery -= round;
        }
        else if (accumulatedRecovery < -1)
        {
            int round = Mathf.CeilToInt(accumulatedRecovery);
            Value += round;
            accumulatedRecovery -= round;
        }
    }

    #endregion

    #region 异常属性抗性

    public abstract bool IsAbnormalStat { get; }

    /// <summary>
    /// 异常属性累积值抗性，取值范围0~200。如冰冻累积值，0为2倍弱冰，100为正常，150为50%免疫，200为100%免疫
    /// </summary>
    internal int AbnormalStatResistance = 100;

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
                        value = Mathf.RoundToInt((value - _value) * (200f - AbnormalStatResistance) / 100f) + _value;
                        value = Mathf.Clamp(value, _minValue, _maxValue);
                    }
                }

                int before = _value;
                _value = value;
                if (_value == _minValue) OnValueReachMin?.Invoke(_value);
                if (_value == _maxValue) OnValueReachMax?.Invoke(_value);
                OnValueChanged?.Invoke(before, _value);
                OnChanged?.Invoke(_value, _minValue, _maxValue);
                if (before < _value) OnValueIncrease?.Invoke(_value - before);
                if (before > _value) OnValueDecrease?.Invoke(before - _value);
                if (before > 0 && _value <= 0) OnValueReachZero?.Invoke();
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

    public void ApplyDataTo(Stat target)
    {
        target._value = _value;
        target._minValue = _minValue;
        target._maxValue = _maxValue;
        ChildApplyDataTo(target);
    }

    protected virtual void ChildApplyDataTo(Stat target)
    {
    }

    public override string ToString()
    {
        return $"{_value} ~ [{_minValue}, {_maxValue}]";
    }
}
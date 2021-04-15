using System;
using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Property是根据加或乘Modifier来增减、倍增的量，如最大生命值、速度、攻击等
/// </summary>
[Serializable]
public abstract class Property
{
    private bool isDirty = true;

    public void Initialize()
    {
        if (isDirty)
        {
            RefreshModifiedValue();
        }

        isDirty = false;
    }

    public void OnRecycled()
    {
        BaseValue = 0;
        ModifiedValue = 0;
        foreach (PlusModifier pm in PlusModifiers_Value)
        {
            pm.OnRecycled();
        }

        PlusModifiers_Value.Clear();

        foreach (MultiplyModifier mm in MultiplyModifiers_Value)
        {
            mm.OnRecycled();
        }

        MultiplyModifiers_Value.Clear();
        ClearCallBacks();
    }

    public void ClearCallBacks()
    {
        m_NotifyActionSet.ClearCallBacks();
    }

    [HideInInspector]
    public NotifyActionSet m_NotifyActionSet = new NotifyActionSet();

    public class NotifyActionSet
    {
        public UnityAction<int, int, int> OnChanged;
        public UnityAction<int, int> OnValueChanged;
        public UnityAction<int> OnValueIncrease;
        public UnityAction<int> OnValueDecrease;

        public void RegisterCallBacks(NotifyActionSet target)
        {
            OnChanged = target.OnChanged;
            OnValueChanged = target.OnValueChanged;
            OnValueIncrease = target.OnValueIncrease;
            OnValueDecrease = target.OnValueDecrease;
        }

        public void ClearCallBacks()
        {
            OnChanged = null;
            OnValueChanged = null;
            OnValueIncrease = null;
            OnValueDecrease = null;
        }
    }

    #region Modifiers

    [OdinSerialize]
    private List<PlusModifier> PlusModifiers_Value = new List<PlusModifier>();

    [OdinSerialize]
    private List<MultiplyModifier> MultiplyModifiers_Value = new List<MultiplyModifier>();

    public bool AddModifier(PlusModifier modifier)
    {
        isDirty = true;
        PlusModifiers_Value.Add(modifier);
        modifier.OnValueChanged += (before, after) => { RefreshModifiedValue(); };
        RefreshModifiedValue();
        return true;
    }

    public bool RemoveModifier(PlusModifier modifier)
    {
        isDirty = true;
        if (PlusModifiers_Value.Remove(modifier))
        {
            modifier.OnValueChanged = null;
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
        isDirty = true;
        MultiplyModifiers_Value.Add(modifier);
        modifier.OnValueChanged += (before, after) => { RefreshModifiedValue(); };
        RefreshModifiedValue();
        return true;
    }

    public bool RemoveModifier(MultiplyModifier modifier)
    {
        isDirty = true;
        if (MultiplyModifiers_Value.Remove(modifier))
        {
            foreach (MultiplyModifier mm in MultiplyModifiers_Value)
            {
                mm.CoverModifiersGUID.Remove(modifier.GUID);
            }

            modifier.OnValueChanged = null;
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
        foreach (PlusModifier pm in PlusModifiers_Value)
        {
            if (pm.Covered) continue;
            finalValue += pm.Delta;
        }

        foreach (MultiplyModifier mm in MultiplyModifiers_Value)
        {
            if (mm.Covered) continue;
            finalValue *= (100 + mm.Percent) / 100f;
        }

        int finalValue_Int = Mathf.RoundToInt(finalValue);
        finalValue_Int = Mathf.Clamp(finalValue_Int, MinValue, MaxValue);
        if (ModifiedValue != finalValue_Int)
        {
            int before = ModifiedValue;
            ModifiedValue = finalValue_Int;
            m_NotifyActionSet.OnValueChanged?.Invoke(before, ModifiedValue);
            if (before < ModifiedValue) m_NotifyActionSet.OnValueIncrease?.Invoke(ModifiedValue - before);
            if (before > ModifiedValue) m_NotifyActionSet.OnValueDecrease?.Invoke(before - ModifiedValue);
            m_NotifyActionSet.OnChanged?.Invoke(ModifiedValue, MinValue, MaxValue);
        }
    }

    [SerializeField]
    [LabelText("下限")]
    private int MinValue;

    [SerializeField]
    [LabelText("上限")]
    private int MaxValue;

    public void ApplyDataTo(Property target)
    {
        target.BaseValue = BaseValue;
        target.MinValue = MinValue;
        target.MaxValue = MaxValue;
        if (target.PlusModifiers_Value.Count == 0 && target.MultiplyModifiers_Value.Count == 0)
        {
            target.ModifiedValue = target.BaseValue;
            target.isDirty = false;
        }
        else
        {
            target.isDirty = true;
        }

        ChildApplyDataTo(target);
    }

    protected virtual void ChildApplyDataTo(Property target)
    {
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
            foreach (PlusModifier pm in PlusModifiers_Value)
            {
                sb.Append(pm);
            }

            sb.Append(")");
        }

        foreach (MultiplyModifier mm in MultiplyModifiers_Value)
        {
            sb.Append(mm);
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

        public UnityAction<int, int> OnValueChanged;

        internal bool Covered => CoverModifiersGUID.Count > 0;

        /// <summary>
        /// 此Modifier被哪些Modifier用<see cref="EntityBuffAttributeRelationship.MaxDominant"/>屏蔽了
        /// </summary>
        internal HashSet<uint> CoverModifiersGUID = new HashSet<uint>();

        public abstract bool CanCover(Modifier target);

        public virtual void OnRecycled()
        {
            OnValueChanged = null;
            CoverModifiersGUID.Clear();
        }
    }

    [Serializable]
    public class MultiplyModifier : Modifier
    {
        private int percent;

        public int Percent
        {
            get { return percent; }
            set
            {
                if (percent != value)
                {
                    OnValueChanged?.Invoke(percent, value);
                    percent = value;
                }
            }
        }

        public override string ToString()
        {
            return $" * {(Covered ? "(Covered:" : "")}{100 + Percent}%{(Covered ? ")" : "")}";
        }

        public override bool CanCover(Modifier target)
        {
            MultiplyModifier targetMultiplyModifier = (MultiplyModifier) target;
            return Mathf.Abs(Percent) >= Mathf.Abs(targetMultiplyModifier.Percent);
        }

        public override void OnRecycled()
        {
            base.OnRecycled();
            percent = 0;
        }
    }

    [Serializable]
    public class PlusModifier : Modifier
    {
        private int delta;

        public int Delta
        {
            get { return delta; }
            set
            {
                if (delta != value)
                {
                    OnValueChanged?.Invoke(delta, value);
                    delta = value;
                }
            }
        }

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

        public override void OnRecycled()
        {
            base.OnRecycled();
            delta = 0;
        }
    }
}
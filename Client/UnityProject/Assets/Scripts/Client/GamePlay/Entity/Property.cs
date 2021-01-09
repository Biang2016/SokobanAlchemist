using System;
using System.Collections.Generic;
using System.Text;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Property是根据加或乘Modifier来增减、倍增的量，如最大生命值、速度、攻击等
/// </summary>
[Serializable]
public abstract class Property : IClone<Property>
{
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

    #region Modifiers

    private SortedDictionary<uint, PlusModifier> PlusModifiers_Value = new SortedDictionary<uint, PlusModifier>();
    private SortedDictionary<uint, MultiplyModifier> MultiplyModifiers_Value = new SortedDictionary<uint, MultiplyModifier>();

    public bool AddModifier(PlusModifier modifier)
    {
        if (!PlusModifiers_Value.ContainsKey(modifier.GUID))
        {
            PlusModifiers_Value.Add(modifier.GUID, modifier);
            modifier.OnValueChanged += (before, after) => { RefreshModifiedValue(); };
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
        if (!MultiplyModifiers_Value.ContainsKey(modifier.GUID))
        {
            MultiplyModifiers_Value.Add(modifier.GUID, modifier);
            modifier.OnValueChanged += (before, after) => { RefreshModifiedValue(); };
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

    public Property Clone()
    {
        Type type = GetType();
        Property newProp = (Property) Activator.CreateInstance(type);
        newProp.BaseValue = BaseValue;
        newProp.MinValue = MinValue;
        newProp.MaxValue = MaxValue;
        ChildClone(newProp);
        return newProp;
    }

    protected virtual void ChildClone(Property newProp)
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

        public UnityAction<int, int> OnValueChanged;

        internal bool Covered => CoverModifiersGUID.Count > 0;

        /// <summary>
        /// 此Modifier被哪些Modifier用<see cref="BuffAttributeRelationship.MaxDominant"/>屏蔽了
        /// </summary>
        internal HashSet<uint> CoverModifiersGUID = new HashSet<uint>();

        public abstract bool CanCover(Modifier target);
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
    }
}
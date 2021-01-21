using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public class BoxStatPropSet : IClone<BoxStatPropSet>, ISerializationCallbackReceiver
{
    internal Box Box;

    public Dictionary<BoxStatType, BoxStat> StatDict = new Dictionary<BoxStatType, BoxStat>();
    public Dictionary<BoxPropertyType, BoxProperty> PropertyDict = new Dictionary<BoxPropertyType, BoxProperty>();

    [BoxGroup("耐久")]
    [LabelText("@\"公共碰撞耐久\t\"+CommonDurability")]
    public BoxStat CommonDurability = new BoxStat(BoxStatType.CommonDurability);

    [BoxGroup("耐久")]
    [LabelText("@\"撞击箱子损坏耐久\t\"+CollideWithBoxDurability")]
    public BoxStat CollideWithBoxDurability = new BoxStat(BoxStatType.CollideWithBoxDurability);

    [BoxGroup("耐久")]
    [LabelText("@\"撞击角色损坏耐久\t\"+CollideWithActorDurability")]
    public BoxStat CollideWithActorDurability = new BoxStat(BoxStatType.CollideWithActorDurability);

    [BoxGroup("耐久")]
    [LabelText("@\"坠落留存率%\t\"+DropFromAirSurviveProbabilityPercent")]
    public BoxStat DropFromAirSurviveProbabilityPercent = new BoxStat(BoxStatType.DropFromAirSurviveProbabilityPercent);

    [BoxGroup("冰冻")]
    [LabelText("@\"冰冻抗性\t\"+FrozenResistance")]
    public BoxProperty FrozenResistance = new BoxProperty(BoxPropertyType.FrozenResistance);

    [BoxGroup("冰冻")]
    [LabelText("@\"冰冻恢复率\t\"+FrozenRecovery")]
    public BoxProperty FrozenRecovery = new BoxProperty(BoxPropertyType.FrozenRecovery);

    [BoxGroup("冰冻")]
    [LabelText("@\"冰冻累积值\t\"+FrozenValue")]
    public BoxStat FrozenValue = new BoxStat(BoxStatType.FrozenValue);

    [BoxGroup("冰冻")]
    [LabelText("冰冻等级")]
    public BoxStat FrozenLevel = new BoxStat(BoxStatType.FrozenLevel);

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
    public BoxProperty FiringResistance = new BoxProperty(BoxPropertyType.FiringResistance);

    [BoxGroup("燃烧")]
    [LabelText("@\"燃烧恢复率\t\"+FiringRecovery")]
    public BoxProperty FiringRecovery = new BoxProperty(BoxPropertyType.FiringRecovery);

    [BoxGroup("燃烧")]
    [LabelText("@\"燃烧增长率Percent\t\"+FiringGrowthPercent")]
    public BoxProperty FiringGrowthPercent = new BoxProperty(BoxPropertyType.FiringGrowthPercent);

    [BoxGroup("燃烧")]
    [LabelText("@\"燃烧累积值\t\"+FiringValue")]
    public BoxStat FiringValue = new BoxStat(BoxStatType.FiringValue);

    [BoxGroup("燃烧")]
    [LabelText("@\"燃烧耐久度\t\"+FiringDurability")]
    public BoxStat FiringDurability = new BoxStat(BoxStatType.FiringDurability);

    [BoxGroup("燃烧")]
    [LabelText("燃烧等级")]
    public BoxStat FiringLevel = new BoxStat(BoxStatType.FiringLevel);

    internal int FiringValuePerLevel => Mathf.RoundToInt(((float) FiringValue.MaxValue / FiringLevel.MaxValue));

    [BoxGroup("燃烧")]
    [LabelText("燃烧触发特效")]
    [ValueDropdown("GetAllFXTypeNames", DropdownTitle = "选择FX类型")]
    public string StartFiringFX;

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

    [NonSerialized]
    [ShowInInspector]
    [BoxGroup("Buff")]
    [LabelText("箱子自带Buff")]
    public List<BoxBuff> RawBoxDefaultBuffs = new List<BoxBuff>(); // 干数据，禁修改

    [HideInInspector]
    public byte[] RawBoxDefaultBuffData;

    public void OnBeforeSerialize()
    {
        if (RawBoxDefaultBuffs == null) RawBoxDefaultBuffs = new List<BoxBuff>();
        RawBoxDefaultBuffData = SerializationUtility.SerializeValue(RawBoxDefaultBuffs, DataFormat.JSON);
    }

    public void OnAfterDeserialize()
    {
        RawBoxDefaultBuffs = SerializationUtility.DeserializeValue<List<BoxBuff>>(RawBoxDefaultBuffData, DataFormat.JSON);
    }

    public void Initialize(Box box)
    {
        Box = box;

        CommonDurability.OnValueReachZero += () => { box.BoxPassiveSkillMarkAsDeleted = true; };
        CollideWithBoxDurability.OnValueReachZero += () => { box.BoxPassiveSkillMarkAsDeleted = true; };
        CollideWithActorDurability.OnValueReachZero += () => { box.BoxPassiveSkillMarkAsDeleted = true; };
        StatDict.Add(BoxStatType.CommonDurability, CommonDurability);
        StatDict.Add(BoxStatType.CollideWithBoxDurability, CollideWithBoxDurability);
        StatDict.Add(BoxStatType.CollideWithActorDurability, CollideWithActorDurability);
        StatDict.Add(BoxStatType.DropFromAirSurviveProbabilityPercent, DropFromAirSurviveProbabilityPercent);

        #region Frozen

        FrozenResistance.Initialize();
        FrozenResistance.OnValueChanged += (before, after) => { FrozenValue.AbnormalStatResistance = FrozenResistance.GetModifiedValue; };
        PropertyDict.Add(BoxPropertyType.FrozenResistance, FrozenResistance);

        FrozenRecovery.Initialize();
        FrozenRecovery.OnValueChanged += (before, after) => { FrozenValue.Recovery = -after; };
        PropertyDict.Add(BoxPropertyType.FrozenRecovery, FrozenRecovery);

        FrozenValue.Recovery = -FrozenRecovery.GetModifiedValue;
        FrozenValue.AbnormalStatResistance = FrozenResistance.GetModifiedValue;
        FrozenValue.OnValueChanged += (before, after) =>
        {
            FrozenLevel.Value = after / FrozenValuePerLevel;
            if (FrozenLevel.Value > 0) Box.BoxBuffHelper.PlayAbnormalStatFX((int) BoxStatType.FrozenValue, FrozenFX, FrozenFXScaleCurve.Evaluate(FrozenLevel.Value)); // 冰冻值变化时，播放一次特效
        };
        StatDict.Add(BoxStatType.FrozenValue, FrozenValue);

        FrozenLevel.OnValueChanged += Box.BoxFrozenHelper.FrozeIntoIceBlock;
        StatDict.Add(BoxStatType.FrozenLevel, FrozenLevel);

        #endregion

        #region Firing

        FiringResistance.Initialize();
        FiringResistance.OnValueChanged += (before, after) => { FiringValue.AbnormalStatResistance = after; };
        PropertyDict.Add(BoxPropertyType.FiringResistance, FiringResistance);

        FiringRecovery.Initialize();
        FiringRecovery.OnValueChanged += (before, after) => { FiringValue.Recovery = -after; };
        PropertyDict.Add(BoxPropertyType.FiringRecovery, FiringRecovery);

        FiringGrowthPercent.Initialize();
        FiringGrowthPercent.OnValueChanged += (before, after) => { FiringValue.GrowthPercent = after; };
        PropertyDict.Add(BoxPropertyType.FiringGrowthPercent, FiringGrowthPercent);

        FiringValue.Recovery = -FiringRecovery.GetModifiedValue;
        FiringValue.GrowthPercent = FiringGrowthPercent.GetModifiedValue;
        FiringValue.AbnormalStatResistance = FiringResistance.GetModifiedValue;
        FiringValue.OnValueChanged += (before, after) =>
        {
            FiringLevel.Value = after / FiringValuePerLevel;
            if (FiringLevel.Value > 0) Box.BoxBuffHelper.PlayAbnormalStatFX((int) BoxStatType.FiringValue, FiringFX, FiringFXScaleCurve.Evaluate(FiringLevel.Value)); // 燃烧值变化时，播放一次特效
            else if (after == 0) Box.BoxBuffHelper.RemoveAbnormalStatFX((int) BoxStatType.FiringValue);
        };
        StatDict.Add(BoxStatType.FiringValue, FiringValue);

        FiringDurability.OnValueChanged += (before, after) =>
        {
            if (after == 0)
            {
                foreach (GridPos3D offset in Box.GetBoxOccupationGPs_Rotated())
                {
                    FXManager.Instance.PlayFX(FiringBreakFX, Box.transform.position + offset, 1.5f);
                }

                box.DestroyBox();
            }
        };
        StatDict.Add(BoxStatType.FiringDurability, FiringDurability);

        FiringLevel.OnValueChanged += (before, after) =>
        {
            if (before == 0 && after > 0)
            {
                foreach (GridPos3D offset in Box.GetBoxOccupationGPs_Rotated())
                {
                    FX fx = FXManager.Instance.PlayFX(StartFiringFX, Box.transform.position + Vector3.up * 0.5f + offset, 1f);
                    if (fx) fx.transform.parent = Box.transform;
                }
            }
        };
        StatDict.Add(BoxStatType.FiringLevel, FiringLevel);

        #endregion

        foreach (BoxBuff rawBoxDefaultBuff in RawBoxDefaultBuffs)
        {
            box.BoxBuffHelper.AddBuff(rawBoxDefaultBuff.Clone());
        }
    }

    public void OnRecycled()
    {
        foreach (KeyValuePair<BoxStatType, BoxStat> kv in StatDict)
        {
            kv.Value.ClearCallBacks();
        }

        StatDict.Clear();
        foreach (KeyValuePair<BoxPropertyType, BoxProperty> kv in PropertyDict)
        {
            kv.Value.ClearCallBacks();
        }

        PropertyDict.Clear();
    }

    public void FixedUpdate(float fixedDeltaTime)
    {
        foreach (KeyValuePair<BoxStatType, BoxStat> kv in StatDict)
        {
            kv.Value.FixedUpdate(fixedDeltaTime);
        }

        // 燃烧蔓延
        if (FiringLevel.Value >= 1)
        {
            foreach (Box adjacentBox in WorldManager.Instance.CurrentWorld.GetAdjacentBox(Box.WorldGP))
            {
                int diff = FiringValue.Value - adjacentBox.BoxStatPropSet.FiringValue.Value;
                if (diff > 0)
                {
                    adjacentBox.BoxStatPropSet.FiringValue.Value += Mathf.RoundToInt(diff * 0.66f);
                }
            }

            FiringDurability.Value -= FiringLevel.Value;
        }
    }

    public BoxStatPropSet Clone()
    {
        BoxStatPropSet newStatPropSet = new BoxStatPropSet();
        newStatPropSet.CommonDurability = (BoxStat) CommonDurability.Clone();
        newStatPropSet.CollideWithBoxDurability = (BoxStat) CollideWithBoxDurability.Clone();
        newStatPropSet.CollideWithActorDurability = (BoxStat) CollideWithActorDurability.Clone();
        newStatPropSet.DropFromAirSurviveProbabilityPercent = (BoxStat) DropFromAirSurviveProbabilityPercent.Clone();
        newStatPropSet.FrozenResistance = (BoxProperty) FrozenResistance.Clone();
        newStatPropSet.FrozenRecovery = (BoxProperty) FrozenRecovery.Clone();
        newStatPropSet.FrozenValue = (BoxStat) FrozenValue.Clone();
        newStatPropSet.FrozenLevel = (BoxStat) FrozenLevel.Clone();
        newStatPropSet.FrozenFX = FrozenFX;
        newStatPropSet.FrozenFXScaleCurve = FrozenFXScaleCurve; // 风险，此处没有深拷贝
        newStatPropSet.FiringResistance = (BoxProperty) FiringResistance.Clone();
        newStatPropSet.FiringRecovery = (BoxProperty) FiringRecovery.Clone();
        newStatPropSet.FiringGrowthPercent = (BoxProperty) FiringGrowthPercent.Clone();
        newStatPropSet.FiringValue = (BoxStat) FiringValue.Clone();
        newStatPropSet.FiringDurability = (BoxStat) FiringDurability.Clone();
        newStatPropSet.FiringLevel = (BoxStat) FiringLevel.Clone();
        newStatPropSet.StartFiringFX = StartFiringFX;
        newStatPropSet.FiringFX = FiringFX;
        newStatPropSet.FiringFXScaleCurve = FiringFXScaleCurve; // 风险，此处没有深拷贝
        newStatPropSet.FiringBreakFX = FiringBreakFX;

        newStatPropSet.RawBoxDefaultBuffs = RawBoxDefaultBuffs.Clone();
        return newStatPropSet;
    }

    #region Utils

    private IEnumerable<string> GetAllFXTypeNames => ConfigManager.GetAllFXTypeNames();

    #endregion
}

[Serializable]
public class BoxProperty : Property
{
    public BoxProperty()
    {
    }

    public BoxProperty(BoxPropertyType propertyType)
    {
        m_PropertyType = propertyType;
    }

    internal BoxPropertyType m_PropertyType;

    protected override void ChildClone(Property newProp)
    {
        base.ChildClone(newProp);
        BoxProperty newBoxProp = (BoxProperty) newProp;
        newBoxProp.m_PropertyType = m_PropertyType;
    }
}

[Serializable]
public class BoxStat : Stat
{
    public BoxStat()
    {
    }

    public BoxStat(BoxStatType statType)
    {
        m_StatType = statType;
    }

    internal BoxStatType m_StatType;
    public override bool IsAbnormalStat => m_StatType == BoxStatType.FiringValue || m_StatType == BoxStatType.FrozenValue;

    protected override void ChildClone(Stat newStat)
    {
        base.ChildClone(newStat);
        BoxStat newBoxStat = (BoxStat) newStat;
        newBoxStat.m_StatType = m_StatType;
    }
}

public enum BoxStatType
{
    [LabelText("公共碰撞耐久(-1无限)")]
    CommonDurability = 0,

    [LabelText("撞击箱子损坏耐久(-1无限)")]
    CollideWithBoxDurability = 1,

    [LabelText("撞击角色损坏耐久(-1无限)")]
    CollideWithActorDurability = 2,

    [LabelText("坠落留存率%")]
    DropFromAirSurviveProbabilityPercent = 3,

    [LabelText("冰冻累积值")]
    FrozenValue = 100,

    [LabelText("冰冻等级")]
    FrozenLevel = 120,

    [LabelText("燃烧累积值")]
    FiringValue = 101,

    [LabelText("燃烧耐久度")]
    FiringDurability = 102,

    [LabelText("燃烧等级")]
    FiringLevel = 121,
}

public enum BoxPropertyType
{
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
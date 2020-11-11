using System;
using System.Collections.Generic;
using BiangStudio.CloneVariant;
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

    [LabelText("@\"冰冻抗性\t\"+FrozenResistance")]
    public BoxProperty FrozenResistance = new BoxProperty(BoxPropertyType.FrozenResistance);

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

    [BoxGroup("灼烧")]
    [LabelText("@\"灼烧抗性\t\"+FiringResistance")]
    public BoxProperty FiringResistance = new BoxProperty(BoxPropertyType.FiringResistance);

    [BoxGroup("灼烧")]
    [LabelText("@\"灼烧累积值\t\"+FiringValue")]
    public BoxStat FiringValue = new BoxStat(BoxStatType.FiringValue);

    [BoxGroup("灼烧")]
    [LabelText("灼烧等级")]
    public BoxStat FiringLevel = new BoxStat(BoxStatType.FiringLevel);

    internal int FiringValuePerLevel => Mathf.RoundToInt(((float) FiringValue.MaxValue / FiringLevel.MaxValue));

    [BoxGroup("灼烧")]
    [LabelText("灼烧持续特效")]
    [ValueDropdown("GetAllFXTypeNames", DropdownTitle = "选择FX类型")]
    public string FiringFX;

    [BoxGroup("灼烧")]
    [LabelText("灼烧持续特效尺寸(x->灼烧等级")]
    public AnimationCurve FiringFXScaleCurve;

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

        FrozenResistance.Initialize();
        FiringResistance.Initialize();

        CommonDurability.OnValueReachZero += () => { box.BoxFunctionMarkAsDeleted = true; };
        CollideWithBoxDurability.OnValueReachZero += () => { box.BoxFunctionMarkAsDeleted = true; };
        CollideWithActorDurability.OnValueReachZero += () => { box.BoxFunctionMarkAsDeleted = true; };
        StatDict.Add(BoxStatType.CommonDurability, CommonDurability);
        StatDict.Add(BoxStatType.CollideWithBoxDurability, CollideWithBoxDurability);
        StatDict.Add(BoxStatType.CollideWithActorDurability, CollideWithActorDurability);

        FrozenResistance.OnValueChanged += (before, after) =>
        {
            FrozenValue.AbnormalStatResistance = after;
            FrozenValue.Recovery = -after;
        };
        PropertyDict.Add(BoxPropertyType.FrozenResistance, FrozenResistance);

        FrozenValue.Recovery = -FrozenResistance.GetModifiedValue;
        FrozenValue.AbnormalStatResistance = FrozenResistance.GetModifiedValue;
        FrozenValue.OnValueChanged += (before, after) =>
        {
            FrozenLevel.Value = after / FrozenValuePerLevel;
            if (FrozenLevel.Value > 0) Box.BoxBuffHelper.PlayAbnormalStatFX((int) BoxStatType.FrozenValue, FrozenFX, FrozenFXScaleCurve.Evaluate(FrozenLevel.Value)); // 冰冻值变化时，播放一次特效
        };
        StatDict.Add(BoxStatType.FrozenValue, FrozenValue);

        FrozenLevel.OnValueChanged += Box.BoxFrozenHelper.FrozeIntoIceBlock;
        StatDict.Add(BoxStatType.FrozenLevel, FrozenLevel);

        FiringResistance.OnValueChanged += (before, after) =>
        {
            FiringValue.AbnormalStatResistance = after;
            FiringValue.Recovery = -after;
        };
        PropertyDict.Add(BoxPropertyType.FiringResistance, FiringResistance);

        FiringValue.Recovery = -FiringResistance.GetModifiedValue;
        FiringValue.AbnormalStatResistance = FiringResistance.GetModifiedValue;
        FiringValue.OnValueChanged += (before, after) =>
        {
            FiringLevel.Value = after / FiringValuePerLevel;
            if (FiringLevel.Value > 0) Box.BoxBuffHelper.PlayAbnormalStatFX((int) BoxStatType.FiringValue, FiringFX, FiringFXScaleCurve.Evaluate(FiringLevel.Value)); // 灼烧值变化时，播放一次特效
        };
        StatDict.Add(BoxStatType.FiringValue, FiringValue);

        StatDict.Add(BoxStatType.FiringLevel, FiringLevel);

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

    private float abnormalStateAutoTick = 0f;
    private int abnormalStateAutoTickInterval = 1; // 异常状态值每秒降低

    public void FixedUpdate(float fixedDeltaTime)
    {
        foreach (KeyValuePair<BoxStatType, BoxStat> kv in StatDict)
        {
            kv.Value.FixedUpdate(fixedDeltaTime);
        }

        abnormalStateAutoTick += fixedDeltaTime;
        if (abnormalStateAutoTick > abnormalStateAutoTickInterval)
        {
            abnormalStateAutoTick -= abnormalStateAutoTickInterval;

            // todo
            //Box.BoxBattleHelper.Damage(Box, FiringLevel.Value);
        }
    }

    public BoxStatPropSet Clone()
    {
        BoxStatPropSet newStatPropSet = new BoxStatPropSet();
        newStatPropSet.CommonDurability = (BoxStat)CommonDurability.Clone();
        newStatPropSet.CollideWithBoxDurability = (BoxStat)CollideWithBoxDurability.Clone();
        newStatPropSet.CollideWithActorDurability = (BoxStat)CollideWithActorDurability.Clone();
        newStatPropSet.FrozenResistance = (BoxProperty) FrozenResistance.Clone();
        newStatPropSet.FrozenValue = (BoxStat) FrozenValue.Clone();
        newStatPropSet.FrozenLevel = (BoxStat) FrozenLevel.Clone();
        newStatPropSet.FrozenFX = FrozenFX;
        newStatPropSet.FrozenFXScaleCurve = FrozenFXScaleCurve; // 风险，此处没有深拷贝
        newStatPropSet.FiringResistance = (BoxProperty) FiringResistance.Clone();
        newStatPropSet.FiringValue = (BoxStat) FiringValue.Clone();
        newStatPropSet.FiringLevel = (BoxStat) FiringLevel.Clone();
        newStatPropSet.FiringFX = FiringFX;
        newStatPropSet.FiringFXScaleCurve = FiringFXScaleCurve; // 风险，此处没有深拷贝

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

    [LabelText("冰冻累积值")]
    FrozenValue = 100,

    [LabelText("冰冻等级")]
    FrozenLevel = 120,

    [LabelText("灼烧累积值")]
    FiringValue = 101,

    [LabelText("灼烧等级")]
    FiringLevel = 121,
}

public enum BoxPropertyType
{
    [LabelText("冰冻抗性")]
    FrozenResistance = 100,

    [LabelText("灼烧抗性")]
    FiringResistance = 101,
}
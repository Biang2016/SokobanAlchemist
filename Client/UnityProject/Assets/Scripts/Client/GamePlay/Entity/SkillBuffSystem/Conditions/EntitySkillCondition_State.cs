using System;
using Sirenix.OdinInspector;

[Serializable]
public class EntitySkillCondition_State : EntitySkillCondition, EntitySkillCondition.IPureCondition
{
    [EnumToggleButtons]
    [LabelText("状态或属性")]
    public ConditionType m_ConditionType = ConditionType.Stat;

    [LabelText("状态值种类")]
    [ShowIf("m_ConditionType", ConditionType.Stat)]
    public EntityStatType EntityStatType;

    [LabelText("状态值阈值")]
    [ShowIf("m_ConditionType", ConditionType.Stat)]
    public int EntityStatThreshold;

    private bool StatOrProperty => m_ConditionType == ConditionType.Stat || m_ConditionType == ConditionType.Property;

    [LabelText("属性值种类")]
    [ShowIf("m_ConditionType", ConditionType.Property)]
    public EntityPropertyType EntityPropertyType;

    [LabelText("属性值阈值")]
    [ShowIf("m_ConditionType", ConditionType.Property)]
    public int EntityPropertyThreshold;

    [HideLabel]
    [EnumToggleButtons]
    [ShowIf("StatOrProperty")]
    public Operator ThresholdOperator;

    [LabelText("战场状态")]
    [ShowIf("m_ConditionType", ConditionType.BattleStateBool)]
    public string BattleStateBool = "";

    [LabelText("满足条件持续时间/s")]
    public float SatisfiedDuration = 0f;

    private float satisfiedDurationTick = 0f;

    public bool OnCheckCondition()
    {
        return satisfiedDurationTick >= SatisfiedDuration;
    }

    public override void OnInit(Entity entity)
    {
        base.OnInit(entity);
        satisfiedDurationTick = 0f;
    }

    public override void OnTick(float tickInterval)
    {
        base.OnTick(tickInterval);
        if (CheckPerTick())
        {
            satisfiedDurationTick += tickInterval;
        }
        else
        {
            satisfiedDurationTick = 0;
        }
    }

    private bool CheckPerTick()
    {
        switch (m_ConditionType)
        {
            case ConditionType.Stat:
            {
                if (Entity == null || Entity.EntityStatPropSet == null || Entity.EntityStatPropSet.StatDict == null)
                {
                    int a = 0;
                }

                EntityStat stat = Entity.EntityStatPropSet.StatDict[EntityStatType];
                bool trigger = false;
                switch (ThresholdOperator)
                {
                    case Operator.LessEquals:
                    {
                        trigger = stat.Value <= EntityStatThreshold;
                        break;
                    }
                    case Operator.Equals:
                    {
                        trigger = stat.Value == EntityStatThreshold;
                        break;
                    }
                    case Operator.GreaterEquals:
                    {
                        trigger = stat.Value >= EntityStatThreshold;
                        break;
                    }
                }

                return trigger;
            }
            case ConditionType.Property:
            {
                EntityProperty property = Entity.EntityStatPropSet.PropertyDict[EntityPropertyType];
                bool trigger = false;
                switch (ThresholdOperator)
                {
                    case Operator.LessEquals:
                    {
                        trigger = property.GetModifiedValue <= EntityStatThreshold;
                        break;
                    }
                    case Operator.Equals:
                    {
                        trigger = property.GetModifiedValue == EntityStatThreshold;
                        break;
                    }
                    case Operator.GreaterEquals:
                    {
                        trigger = property.GetModifiedValue >= EntityStatThreshold;
                        break;
                    }
                }

                return trigger;
            }
            case ConditionType.BattleStateBool:
            {
                return BattleManager.Instance.GetStateBool(BattleStateBool);
            }
        }

        return false;
    }

    protected override void ChildClone(EntitySkillCondition cloneData)
    {
        EntitySkillCondition_State cloneCondition = (EntitySkillCondition_State) cloneData;
        cloneCondition.m_ConditionType = m_ConditionType;
        cloneCondition.EntityStatType = EntityStatType;
        cloneCondition.EntityStatThreshold = EntityStatThreshold;
        cloneCondition.EntityPropertyType = EntityPropertyType;
        cloneCondition.EntityPropertyThreshold = EntityPropertyThreshold;
        cloneCondition.ThresholdOperator = ThresholdOperator;
        cloneCondition.BattleStateBool = BattleStateBool;
        cloneCondition.SatisfiedDuration = SatisfiedDuration;
    }

    public override void CopyDataFrom(EntitySkillCondition srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillCondition_State srcCondition = (EntitySkillCondition_State) srcData;
        m_ConditionType = srcCondition.m_ConditionType;
        EntityStatType = srcCondition.EntityStatType;
        EntityStatThreshold = srcCondition.EntityStatThreshold;
        EntityPropertyType = srcCondition.EntityPropertyType;
        EntityPropertyThreshold = srcCondition.EntityPropertyThreshold;
        ThresholdOperator = srcCondition.ThresholdOperator;
        BattleStateBool = srcCondition.BattleStateBool;
        SatisfiedDuration = srcCondition.SatisfiedDuration;
    }
}

public enum Operator
{
    LessEquals,
    Equals,
    GreaterEquals,
}

public enum ValueChangeOverThresholdType
{
    LE_to_G,
    L_to_GE,
    GE_to_L,
    G_to_LE,
}

public enum ConditionType
{
    Stat = 0,
    Property = 1,
    BattleStateBool = 2,
}
﻿using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntitySkillCondition_State : EntitySkillCondition, EntitySkillCondition.IPureCondition
{
    [EnumToggleButtons]
    [LabelText("状态或属性")]
    public ConditionType m_ConditionType = ConditionType.Stat;

    [LabelText("状态值种类")]
    [ShowIf("m_ConditionType", ConditionType.Stat)]
    public EntityStatType EntityStatType;

    [LabelText("使用百分比")]
    [ShowIf("m_ConditionType", ConditionType.Stat)]
    public bool EntityStatThreshold_UsePercent = false;

    [LabelText("状态值阈值")]
    [ShowIf("m_ConditionType", ConditionType.Stat)]
    [HideIf("EntityStatThreshold_UsePercent")]
    public int EntityStatThreshold;

    [LabelText("状态值阈值%")]
    [ShowIf("m_ConditionType", ConditionType.Stat)]
    [ShowIf("EntityStatThreshold_UsePercent")]
    public int EntityStatThresholdPercent;

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

    [LabelText("战场状态")]
    [ShowIf("m_ConditionType", ConditionType.BattleStateBool)]
    public bool BattleStateBoolValue = false;

    [LabelText("满足条件持续时间/s")]
    public float SatisfiedDuration = 0f;

    private float satisfiedDurationTick = 0f;

    public bool OnCheckCondition()
    {
        return satisfiedDurationTick > SatisfiedDuration;
    }

    public override void OnInit(Entity entity)
    {
        base.OnInit(entity);
        satisfiedDurationTick = 0f;
    }

    public override void OnUnInit()
    {
        base.OnUnInit();
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
                EntityStat stat = Entity.EntityStatPropSet.StatDict[EntityStatType];
                bool trigger = false;
                int threshold = EntityStatThreshold;
                if (EntityStatThreshold_UsePercent)
                {
                    threshold = Mathf.RoundToInt(EntityStatThresholdPercent / 100f * stat.MaxValue);
                }

                switch (ThresholdOperator)
                {
                    case Operator.LessEquals:
                    {
                        trigger = stat.Value <= threshold;
                        break;
                    }
                    case Operator.Equals:
                    {
                        trigger = stat.Value == threshold;
                        break;
                    }
                    case Operator.GreaterEquals:
                    {
                        trigger = stat.Value >= threshold;
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
                return BattleManager.Instance.GetStateBool(BattleStateBool) == BattleStateBoolValue;
            }
        }

        return false;
    }

    protected override void ChildClone(EntitySkillCondition cloneData)
    {
        EntitySkillCondition_State cloneCondition = (EntitySkillCondition_State) cloneData;
        cloneCondition.m_ConditionType = m_ConditionType;
        cloneCondition.EntityStatType = EntityStatType;
        cloneCondition.EntityStatThreshold_UsePercent = EntityStatThreshold_UsePercent;
        cloneCondition.EntityStatThreshold = EntityStatThreshold;
        cloneCondition.EntityStatThresholdPercent = EntityStatThresholdPercent;
        cloneCondition.EntityPropertyType = EntityPropertyType;
        cloneCondition.EntityPropertyThreshold = EntityPropertyThreshold;
        cloneCondition.ThresholdOperator = ThresholdOperator;
        cloneCondition.BattleStateBool = BattleStateBool;
        cloneCondition.BattleStateBoolValue = BattleStateBoolValue;
        cloneCondition.SatisfiedDuration = SatisfiedDuration;
    }

    public override void CopyDataFrom(EntitySkillCondition srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillCondition_State srcCondition = (EntitySkillCondition_State) srcData;
        m_ConditionType = srcCondition.m_ConditionType;
        EntityStatType = srcCondition.EntityStatType;
        EntityStatThreshold_UsePercent = srcCondition.EntityStatThreshold_UsePercent;
        EntityStatThreshold = srcCondition.EntityStatThreshold;
        EntityStatThresholdPercent = srcCondition.EntityStatThresholdPercent;
        EntityPropertyType = srcCondition.EntityPropertyType;
        EntityPropertyThreshold = srcCondition.EntityPropertyThreshold;
        ThresholdOperator = srcCondition.ThresholdOperator;
        BattleStateBool = srcCondition.BattleStateBool;
        BattleStateBoolValue = srcCondition.BattleStateBoolValue;
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
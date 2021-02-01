using System;
using Sirenix.OdinInspector;

[Serializable]
public class EntityStat : Stat
{
    public EntityStat()
    {
    }

    public EntityStat(EntityStatType statType)
    {
        m_StatType = statType;
    }

    internal EntityStatType m_StatType;
    public override bool IsAbnormalStat => m_StatType == EntityStatType.FiringValue || m_StatType == EntityStatType.FrozenValue;

    protected override void ChildApplyDataTo(Stat target)
    {
        base.ChildApplyDataTo(target);
        EntityStat newStat = (EntityStat) target;
        newStat.m_StatType = m_StatType;
    }
}

public class EntityStatAttribute : Attribute
{
}

public class BoxStatAttribute : Attribute
{
}

public class ActorStatAttribute : Attribute
{
}

public enum EntityStatType
{
    [EntityStat]
    [LabelText("血量耐久")]
    HealthDurability = 0,

    [ActorStat]
    [LabelText("当前行动力")]
    ActionPoint = 30,

    [EntityStat]
    [LabelText("冰冻累积值")]
    FrozenValue = 100,

    [EntityStat]
    [LabelText("冰冻等级")]
    FrozenLevel = 120,

    [EntityStat]
    [LabelText("燃烧累积值")]
    FiringValue = 101,

    [EntityStat]
    [LabelText("燃烧等级")]
    FiringLevel = 121,

    [BoxStat]
    [LabelText("坠落留存率%")]
    DropFromAirSurviveProbabilityPercent = 500,

    [EntityStat]
    [LabelText("当前金子")]
    Gold = 1000,
}
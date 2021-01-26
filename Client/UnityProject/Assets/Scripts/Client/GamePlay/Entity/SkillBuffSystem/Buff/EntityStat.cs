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

public class BoxStatAttribute : EntityStatAttribute
{
}

public class ActorStatAttribute : EntityStatAttribute
{
}

public enum EntityStatType
{
    [EntityStat]
    [LabelText("血量耐久")]
    HealthDurability = 0,

    [EntityStat]
    [LabelText("爆炸耐久")]
    ExplodeDurability = 1,

    [EntityStat]
    [LabelText("燃烧耐久")]
    FiringDurability = 2,

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
}
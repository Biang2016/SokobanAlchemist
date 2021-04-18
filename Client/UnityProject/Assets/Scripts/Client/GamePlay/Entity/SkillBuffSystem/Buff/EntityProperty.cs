using System;
using Sirenix.OdinInspector;

[Serializable]
public class EntityProperty : Property
{
    public EntityProperty()
    {
    }

    public EntityProperty(EntityPropertyType propertyType)
    {
        m_PropertyType = (int) propertyType;
    }

    public EntityProperty(EntitySkillPropertyType propertyType)
    {
        m_PropertyType = (int) propertyType;
    }

    internal int m_PropertyType;

    protected override void ChildApplyDataTo(Property target)
    {
        base.ChildApplyDataTo(target);
        EntityProperty newProp = (EntityProperty) target;
        newProp.m_PropertyType = m_PropertyType;
    }
}

public class EntityPropertyAttribute : Attribute
{
}

public class BoxPropertyAttribute : Attribute
{
}

public class ActorPropertyAttribute : Attribute
{
}

public enum EntityPropertyType
{
    [EntityProperty]
    [LabelText("血量耐久上限")]
    MaxHealthDurability = 0,

    [EntityProperty]
    [LabelText("燃烧伤害抵消")]
    FiringDamageDefense = 1,

    [EntityProperty]
    [LabelText("爆炸伤害抵消")]
    ExplodeDamageDefense = 2,

    [ActorProperty]
    [LabelText("角色碰撞伤害抵消")]
    ActorCollideDamageDefense = 3,

    [ActorProperty]
    [LabelText("冰冻伤害抵消")]
    FrozenDamageDefense = 4,

    [BoxProperty]
    [LabelText("箱子碰撞减少自身耐久")]
    BoxCollideDamageSelf = 13,

    [EntityProperty]
    [LabelText("碰撞伤害")]
    CollideDamage = 14,

    [EntityProperty]
    [LabelText("X轴碰撞伤害")]
    CollideDamageX = 15,

    [EntityProperty]
    [LabelText("Z轴碰撞伤害")]
    CollideDamageZ = 16,

    [EntityProperty]
    [LabelText("被碰撞硬直")]
    BeCollidedHitStopDuration = 17,

    [EntityProperty]
    [LabelText("碰撞伤害")]
    FrozenBeCollideDamage = 18,

    [ActorProperty]
    [LabelText("移动速度")]
    MoveSpeed = 30,

    [ActorProperty]
    [LabelText("最大行动力")]
    MaxActionPoint = 31,

    [ActorProperty]
    [LabelText("行动力回复速度")]
    ActionPointRecovery = 32,

    [ActorProperty]
    [LabelText("Kick消耗行动力")]
    KickConsumeActionPoint = 33,

    [ActorProperty]
    [LabelText("Dash消耗行动力")]
    DashConsumeActionPoint = 34,

    [ActorProperty]
    [LabelText("Vault消耗行动力")]
    VaultConsumeActionPoint = 35,

    [EntityProperty]
    [LabelText("冰冻抗性")]
    FrozenResistance = 100,

    [EntityProperty]
    [LabelText("燃烧抗性")]
    FiringResistance = 101,

    //[EntityProperty]
    //[LabelText("冰冻恢复率")]
    //FrozenRecovery = 200,

    //[EntityProperty]
    //[LabelText("燃烧恢复率")]
    ////FiringRecovery = 201,

    //[EntityProperty]
    //[LabelText("燃烧增长率")]
    //FiringGrowthPercent = 301,

    [EntityProperty]
    [LabelText("燃烧蔓延率")]
    FiringSpreadPercent = 401,

    [EntityProperty]
    [LabelText("拾取吸附半径")]
    CollectDetectRadius = 499,

    [EntityProperty]
    [LabelText("火元素碎片上限")]
    MaxFireElementFragment = 500,

    [EntityProperty]
    [LabelText("冰元素碎片上限")]
    MaxIceElementFragment = 501,

    [EntityProperty]
    [LabelText("电元素碎片上限")]
    MaxLightningElementFragment = 502,
}

public enum EntitySkillPropertyType
{
    [LabelText("施法正方形范围边长")]
    CastingRadius = 10110,

    [LabelText("冷却时间")]
    Cooldown = 10120,

    [LabelText("前摇")]
    WingUp = 10121,

    [LabelText("施法时间")]
    CastDuration = 10122,

    [LabelText("后摇")]
    Recovery = 10123,

    [LabelText("后摇震屏伤害当量")]
    CameraShakeEquivalentDamage = 10124,

    [LabelText("消耗行动力点数")]
    ConsumeActionPoint = 10199,

    [LabelText("消耗火元素碎片量")]
    ConsumeFireElementFragment = 10200,

    [LabelText("消耗冰元素碎片量")]
    ConsumeIceElementFragment = 10201,

    [LabelText("消耗电元素碎片量")]
    ConsumeLightningElementFragment = 10202,
}
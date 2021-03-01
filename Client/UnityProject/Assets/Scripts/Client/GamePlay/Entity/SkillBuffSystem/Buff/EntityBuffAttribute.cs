using Sirenix.OdinInspector;

public enum EntityBuffAttribute
{
    [LabelText("空")]
    None,

    [LabelText("加速")]
    SpeedUp,

    [LabelText("减速")]
    SlowDown,

    [LabelText("定身")]
    Immobile,

    [LabelText("活跃(行动力)")]
    ActionPointPositive,

    [LabelText("疲劳(行动力)")]
    ActionPointNegative,

    [LabelText("眩晕")]
    Stun,

    [LabelText("眩晕免疫")]
    StunImmune,

    [LabelText("伤害免疫")]
    DamageImmune,

    [LabelText("异常状态免疫")]
    AbnormalImmune,

    [LabelText("增益无效")]
    PositiveBuffImmune,

    [LabelText("无敌")]
    Invincible,

    [LabelText("隐身")]
    Hiding,

    [LabelText("治疗")]
    Healing,

    [LabelText("禁疗")]
    ForbidHealing,

    [LabelText("强壮(最大血量)")]
    MaxHealthPositive,

    [LabelText("虚弱(最大血量)")]
    MaxHealthNegative,

    [LabelText("燃烧")]
    Firing,

    [LabelText("燃烧伤害")]
    FiringDamage,

    [LabelText("燃烧免疫")]
    FiringImmune,

    [LabelText("冰冻")]
    Frozen,

    [LabelText("冰冻伤害")]
    FrozenDamage,

    [LabelText("冰冻免疫")]
    FrozenImmune,

    [LabelText("爆炸")]
    Explode,

    [LabelText("爆炸伤害")]
    ExplodeDamage,

    [LabelText("爆炸免疫")]
    ExplodeImmune,

    [LabelText("碰撞")]
    Collide,

    [LabelText("碰撞伤害")]
    CollideDamage,

    [LabelText("碰撞击退")]
    Repulse,

    [LabelText("碰撞免疫/虚无")]
    CollideImmune,

    [LabelText("攻击")]
    Attack,

    [LabelText("攻击伤害")]
    AttackDamage,

    [LabelText("攻击免疫")]
    AttackImmune,

    [LabelText("荆棘")]
    Thorn,

    [LabelText("荆棘伤害")]
    ThornDamage,

    [LabelText("荆棘免疫")]
    ThornImmune,

    [LabelText("中毒")]
    Poison,

    [LabelText("毒伤害")]
    PoisonousDamage,

    [LabelText("中毒免疫")]
    PoisonImmune,

    [LabelText("电击")]
    Shocking,

    [LabelText("电击伤害")]
    ShockDamage,

    [LabelText("电击免疫")]
    ShockImmune,

    [LabelText("碾压")]
    Grind,

    [LabelText("碾压伤害")]
    GrindDamage,

    [LabelText("碾压免疫")]
    GrindImmune,
}

public static class EntityBuffAttributeExtension
{
    public static bool IsDamageBuff(this EntityBuffAttribute attribute)
    {
        if (
            attribute == EntityBuffAttribute.FiringDamage
            || attribute == EntityBuffAttribute.FrozenDamage
            || attribute == EntityBuffAttribute.ExplodeDamage
            || attribute == EntityBuffAttribute.CollideDamage
            || attribute == EntityBuffAttribute.AttackDamage
            || attribute == EntityBuffAttribute.ThornDamage
            || attribute == EntityBuffAttribute.PoisonousDamage
            || attribute == EntityBuffAttribute.ShockDamage
            || attribute == EntityBuffAttribute.GrindDamage
        ) return true;
        else
        {
            return false;
        }
    }
}
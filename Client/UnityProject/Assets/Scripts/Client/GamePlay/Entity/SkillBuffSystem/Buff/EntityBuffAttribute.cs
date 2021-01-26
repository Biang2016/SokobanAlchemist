using Sirenix.OdinInspector;

public enum EntityBuffAttribute
{
    [LabelText("瞬时效果")]
    InstantEffect = 0,

    [LabelText("加速")]
    SpeedUp = 1,

    [LabelText("行动力")]
    ActionPoint = 2,

    [LabelText("减速")]
    SlowDown = 3,

    [LabelText("眩晕")]
    Stun = 4,

    [LabelText("眩晕免疫")]
    StunImmune = 5,

    [LabelText("无敌")]
    Invincible = 6,

    [LabelText("隐身")]
    Hiding = 7,

    [LabelText("中毒")]
    Poison = 8,

    [LabelText("最大血量")]
    MaxHealth = 9,

    [LabelText("击退")]
    Repulse = 10,
}
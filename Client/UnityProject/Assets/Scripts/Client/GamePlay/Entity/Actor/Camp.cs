using System;

public enum Camp
{
    Neutral = 0,
    Player = 1,
    Enemy = 2,
    Friend = 3,
    // todo Box的阵营划分目前先放在Neutral里面，待以后想清楚了再改
}

[Flags]
public enum RelativeCamp
{
    None = 0,
    FriendCamp = 1 << 0,
    OpponentCamp = 1 << 1,
    NeutralCamp = 1 << 2,
    AllCamp = FriendCamp | OpponentCamp | NeutralCamp,
}
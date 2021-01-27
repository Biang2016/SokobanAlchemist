using System;

public enum Camp
{
    Neutral = 0,
    Player = 1,
    Enemy = 2,
    Friend = 3,
    Box = 4,
}

[Flags]
public enum RelativeCamp
{
    None = 0,
    FriendCamp = 1 << 0,
    OpponentCamp = 1 << 1,
    NeutralCamp = 1 << 2,
    BoxCamp = 1 << 3,
    AllActorCamp = FriendCamp | OpponentCamp | NeutralCamp,
    AllCamp = AllActorCamp | BoxCamp,
}
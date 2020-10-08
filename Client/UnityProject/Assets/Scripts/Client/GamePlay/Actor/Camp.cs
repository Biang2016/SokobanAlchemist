using System;

public enum Camp
{
    None = 0,
    Player = 1,
    Enemy = 2,
    Friend = 3,
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
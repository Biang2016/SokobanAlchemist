﻿using BiangLibrary.Singleton;
using UnityEngine;

public class LayerManager : TSingletonBaseManager<LayerManager>
{
    public int LayerMask_UI;
    public int LayerMask_Player;
    public int LayerMask_Enemy;
    public int LayerMask_Friend;
    public int LayerMask_BoxIndicator; // 用于各种操作识别Box位置
    public int LayerMask_HitBox_Player;
    public int LayerMask_HitBox_Enemy;
    public int LayerMask_HitBox_Friend;
    public int LayerMask_HitBox_Box; // 用于物理碰撞和各种伤害检测
    public int LayerMask_BoxOnlyDynamicCollider;
    public int LayerMask_Ground;
    public int LayerMask_Wall;
    public int LayerMask_BattleTips;
    public int LayerMask_BoxTriggerZone;

    public int Layer_UI;
    public int Layer_Player;
    public int Layer_Enemy;
    public int Layer_Friend;
    public int Layer_BoxIndicator;
    public int Layer_HitBox_Player;
    public int Layer_HitBox_Enemy;
    public int Layer_HitBox_Friend;
    public int Layer_HitBox_Box;
    public int Layer_BoxOnlyDynamicCollider;
    public int Layer_Ground;
    public int Layer_Wall;
    public int Layer_BattleTips;
    public int Layer_BoxTriggerZone;

    public override void Awake()
    {
        LayerMask_UI = LayerMask.GetMask("UI");
        LayerMask_Player = LayerMask.GetMask("Player");
        LayerMask_Enemy = LayerMask.GetMask("Enemy");
        LayerMask_Friend = LayerMask.GetMask("Friend");
        LayerMask_BoxIndicator = LayerMask.GetMask("BoxIndicator");
        LayerMask_HitBox_Player = LayerMask.GetMask("HitBox_Player");
        LayerMask_HitBox_Enemy = LayerMask.GetMask("HitBox_Enemy");
        LayerMask_HitBox_Friend = LayerMask.GetMask("HitBox_Friend");
        LayerMask_HitBox_Box = LayerMask.GetMask("HitBox_Box");
        LayerMask_BoxOnlyDynamicCollider = LayerMask.GetMask("BoxOnlyDynamicCollider");
        LayerMask_Ground = LayerMask.GetMask("Ground");
        LayerMask_Wall = LayerMask.GetMask("Wall");
        LayerMask_BattleTips = LayerMask.GetMask("BattleTips");
        LayerMask_BoxTriggerZone = LayerMask.GetMask("BoxTriggerZone");

        Layer_UI = LayerMask.NameToLayer("UI");
        Layer_Player = LayerMask.NameToLayer("Player");
        Layer_Enemy = LayerMask.NameToLayer("Enemy");
        Layer_Friend = LayerMask.NameToLayer("Friend");
        Layer_BoxIndicator = LayerMask.NameToLayer("BoxIndicator");
        Layer_HitBox_Player = LayerMask.NameToLayer("HitBox_Player");
        Layer_HitBox_Enemy = LayerMask.NameToLayer("HitBox_Enemy");
        Layer_HitBox_Friend = LayerMask.NameToLayer("HitBox_Friend");
        Layer_HitBox_Box = LayerMask.NameToLayer("HitBox_Box");
        Layer_BoxOnlyDynamicCollider = LayerMask.NameToLayer("BoxOnlyDynamicCollider");
        Layer_Ground = LayerMask.NameToLayer("Ground");
        Layer_Wall = LayerMask.NameToLayer("Wall");
        Layer_BattleTips = LayerMask.NameToLayer("BattleTips");
        Layer_BoxTriggerZone = LayerMask.NameToLayer("BoxTriggerZone");
    }

    public bool CheckLayerValid(Camp selfCamp, RelativeCamp relativeCamp, int layer)
    {
        if (relativeCamp == RelativeCamp.None) return false;
        bool valid = false;
        if (relativeCamp == RelativeCamp.AllActorCamp) return layer == Layer_HitBox_Enemy || layer == Layer_HitBox_Player || layer == Layer_HitBox_Friend;
        if (relativeCamp == RelativeCamp.AllCamp) return layer == Layer_HitBox_Enemy || layer == Layer_HitBox_Player || layer == Layer_HitBox_Friend || layer == Layer_BoxIndicator;
        if (relativeCamp.HasFlag(RelativeCamp.BoxCamp)) valid |= layer == Layer_BoxIndicator;
        switch (selfCamp)
        {
            case Camp.Neutral:
            {
                if (relativeCamp.HasFlag(RelativeCamp.FriendCamp)) valid |= layer == Layer_HitBox_Friend;
                if (relativeCamp.HasFlag(RelativeCamp.OpponentCamp)) valid = valid;
                if (relativeCamp.HasFlag(RelativeCamp.NeutralCamp)) valid |= layer == Layer_HitBox_Enemy || layer == Layer_HitBox_Player || layer == Layer_HitBox_Friend;
                break;
            }
            case Camp.Player:
            case Camp.Friend:
            case Camp.Box:
            {
                if (relativeCamp.HasFlag(RelativeCamp.FriendCamp)) valid |= layer == Layer_HitBox_Player;
                if (relativeCamp.HasFlag(RelativeCamp.OpponentCamp)) valid |= layer == Layer_HitBox_Enemy;
                if (relativeCamp.HasFlag(RelativeCamp.NeutralCamp)) valid |= layer == Layer_HitBox_Friend;
                break;
            }
            case Camp.Enemy:
            {
                if (relativeCamp.HasFlag(RelativeCamp.FriendCamp)) valid |= layer == Layer_HitBox_Enemy;
                if (relativeCamp.HasFlag(RelativeCamp.OpponentCamp)) valid |= layer == Layer_HitBox_Player;
                if (relativeCamp.HasFlag(RelativeCamp.NeutralCamp)) valid |= layer == Layer_HitBox_Friend;
                break;
            }
        }

        return valid;
    }

    public int GetTargetEntityLayerMask(Camp selfCamp, RelativeCamp relativeCamp)
    {
        if (relativeCamp == RelativeCamp.None) return 0;
        int layerMask = 0;
        if (relativeCamp == RelativeCamp.AllActorCamp) return LayerMask_HitBox_Enemy | LayerMask_HitBox_Player | LayerMask_HitBox_Friend;
        if (relativeCamp == RelativeCamp.AllCamp) return LayerMask_HitBox_Enemy | LayerMask_HitBox_Player | LayerMask_HitBox_Friend | LayerMask_BoxIndicator;
        if (relativeCamp.HasFlag(RelativeCamp.BoxCamp)) layerMask |= LayerMask_BoxIndicator;
        switch (selfCamp)
        {
            case Camp.Neutral:
            {
                if (relativeCamp.HasFlag(RelativeCamp.FriendCamp)) layerMask |= LayerMask_HitBox_Friend;
                if (relativeCamp.HasFlag(RelativeCamp.OpponentCamp)) layerMask |= 0;
                if (relativeCamp.HasFlag(RelativeCamp.NeutralCamp)) layerMask |= LayerMask_HitBox_Enemy | LayerMask_HitBox_Player | LayerMask_HitBox_Friend;
                break;
            }
            case Camp.Player:
            case Camp.Friend:
            case Camp.Box:
            {
                if (relativeCamp.HasFlag(RelativeCamp.FriendCamp)) layerMask |= LayerMask_HitBox_Player;
                if (relativeCamp.HasFlag(RelativeCamp.OpponentCamp)) layerMask |= LayerMask_HitBox_Enemy;
                if (relativeCamp.HasFlag(RelativeCamp.NeutralCamp)) layerMask |= LayerMask_HitBox_Friend;
                break;
            }
            case Camp.Enemy:
            {
                if (relativeCamp.HasFlag(RelativeCamp.FriendCamp)) layerMask |= LayerMask_HitBox_Enemy;
                if (relativeCamp.HasFlag(RelativeCamp.OpponentCamp)) layerMask |= LayerMask_HitBox_Player;
                if (relativeCamp.HasFlag(RelativeCamp.NeutralCamp)) layerMask |= LayerMask_HitBox_Friend;
                break;
            }
        }

        return layerMask;
    }
}
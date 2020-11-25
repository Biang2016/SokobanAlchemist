using BiangStudio.Singleton;
using UnityEngine;

public class LayerManager : TSingletonBaseManager<LayerManager>
{
    public int LayerMask_UI;
    public int LayerMask_Player;
    public int LayerMask_Enemy;
    public int LayerMask_BoxIndicator; // 用于各种操作识别Box位置
    public int LayerMask_HitBox_Player;
    public int LayerMask_HitBox_Enemy;
    public int LayerMask_HitBox_Box; // 用于物理碰撞和各种伤害检测
    public int LayerMask_BoxOnlyDynamicCollider;
    public int LayerMask_Ground;
    public int LayerMask_Wall;
    public int LayerMask_BattleTips;
    public int LayerMask_BoxTriggerZone;

    public int Layer_UI;
    public int Layer_Player;
    public int Layer_Enemy;
    public int Layer_BoxIndicator;
    public int Layer_HitBox_Player;
    public int Layer_HitBox_Enemy;
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
        LayerMask_BoxIndicator = LayerMask.GetMask("BoxIndicator");
        LayerMask_HitBox_Player = LayerMask.GetMask("HitBox_Player");
        LayerMask_HitBox_Enemy = LayerMask.GetMask("HitBox_Enemy");
        LayerMask_HitBox_Box = LayerMask.GetMask("HitBox_Box");
        LayerMask_BoxOnlyDynamicCollider = LayerMask.GetMask("BoxOnlyDynamicCollider");
        LayerMask_Ground = LayerMask.GetMask("Ground");
        LayerMask_Wall = LayerMask.GetMask("Wall");
        LayerMask_BattleTips = LayerMask.GetMask("BattleTips");
        LayerMask_BoxTriggerZone = LayerMask.GetMask("BoxTriggerZone");

        Layer_UI = LayerMask.NameToLayer("UI");
        Layer_Player = LayerMask.NameToLayer("Player");
        Layer_Enemy = LayerMask.NameToLayer("Enemy");
        Layer_BoxIndicator = LayerMask.NameToLayer("BoxIndicator");
        Layer_HitBox_Player = LayerMask.NameToLayer("HitBox_Player");
        Layer_HitBox_Enemy = LayerMask.NameToLayer("HitBox_Enemy");
        Layer_HitBox_Box = LayerMask.NameToLayer("HitBox_Box");
        Layer_BoxOnlyDynamicCollider = LayerMask.NameToLayer("BoxOnlyDynamicCollider");
        Layer_Ground = LayerMask.NameToLayer("Ground");
        Layer_Wall = LayerMask.NameToLayer("Wall");
        Layer_BattleTips = LayerMask.NameToLayer("BattleTips");
        Layer_BoxTriggerZone = LayerMask.NameToLayer("BoxTriggerZone");
    }

    public int GetTargetActorLayerMask(Camp selfCamp, RelativeCamp relativeCamp)
    {
        switch (selfCamp)
        {
            case Camp.None:
            {
                return 0;
            }
            case Camp.Player:
            {
                switch (relativeCamp)
                {
                    case RelativeCamp.None: return 0;
                    case RelativeCamp.FriendCamp: return LayerMask_HitBox_Player;
                    case RelativeCamp.OpponentCamp: return LayerMask_HitBox_Enemy;
                    case RelativeCamp.NeutralCamp: return LayerMask_HitBox_Enemy;
                }

                break;
            }
            case Camp.Enemy:
            {
                switch (relativeCamp)
                {
                    case RelativeCamp.None: return 0;
                    case RelativeCamp.FriendCamp: return LayerMask_HitBox_Enemy;
                    case RelativeCamp.OpponentCamp: return LayerMask_HitBox_Player;
                    case RelativeCamp.NeutralCamp: return LayerMask_HitBox_Enemy;
                }

                break;
            }
            case Camp.Friend:
            {
                switch (relativeCamp)
                {
                    case RelativeCamp.None: return 0;
                    case RelativeCamp.FriendCamp: return LayerMask_HitBox_Player;
                    case RelativeCamp.OpponentCamp: return LayerMask_HitBox_Enemy;
                    case RelativeCamp.NeutralCamp: return LayerMask_HitBox_Enemy;
                }

                break;
            }
        }

        return 0;
    }
}
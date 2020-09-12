using BiangStudio.Singleton;
using UnityEngine;

public class LayerManager : TSingletonBaseManager<LayerManager>
{
    public int LayerMask_UI;
    public int LayerMask_Player;
    public int LayerMask_Enemy;
    public int LayerMask_HitBox_Player;
    public int LayerMask_HitBox_Enemy;
    public int LayerMask_Box;
    public int LayerMask_Ground;
    public int LayerMask_ItemDropped;
    public int LayerMask_BattleTips;

    public int Layer_UI;
    public int Layer_Player;
    public int Layer_Enemy;
    public int Layer_HitBox_Player;
    public int Layer_HitBox_Enemy;
    public int Layer_Box;
    public int Layer_Ground;
    public int Layer_ItemDropped;
    public int Layer_BattleTips;

    public override void Awake()
    {
        LayerMask_UI = LayerMask.GetMask("UI");
        LayerMask_Player = LayerMask.GetMask("Player");
        LayerMask_Enemy = LayerMask.GetMask("Enemy");
        LayerMask_HitBox_Player = LayerMask.GetMask("HitBox_Player");
        LayerMask_HitBox_Enemy = LayerMask.GetMask("HitBox_Enemy");
        LayerMask_Box = LayerMask.GetMask("Box");
        LayerMask_Ground = LayerMask.GetMask("Ground");
        LayerMask_ItemDropped = LayerMask.GetMask("ItemDropped");
        LayerMask_BattleTips = LayerMask.GetMask("BattleTips");

        Layer_UI = LayerMask.NameToLayer("UI");
        Layer_Player = LayerMask.NameToLayer("Player");
        Layer_Enemy = LayerMask.NameToLayer("Enemy");
        Layer_HitBox_Player = LayerMask.NameToLayer("HitBox_Player");
        Layer_HitBox_Enemy = LayerMask.NameToLayer("HitBox_Enemy");
        Layer_Box = LayerMask.NameToLayer("Box");
        Layer_Ground = LayerMask.NameToLayer("Ground");
        Layer_ItemDropped = LayerMask.NameToLayer("ItemDropped");
        Layer_BattleTips = LayerMask.NameToLayer("BattleTips");
    }
}
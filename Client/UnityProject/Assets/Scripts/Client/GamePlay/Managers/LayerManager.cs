using BiangStudio.Singleton;
using UnityEngine;

public class LayerManager : TSingletonBaseManager<LayerManager>
{
    public int LayerMask_UI;
    public int LayerMask_HitBox_Player;
    public int LayerMask_HitBox_Enemy;
    public int LayerMask_ItemDropped;

    public int Layer_UI;
    public int Layer_HitBox_Player;
    public int Layer_HitBox_Enemy;
    public int Layer_ItemDropped;

    public override void Awake()
    {
        LayerMask_UI = LayerMask.GetMask("UI");
        LayerMask_HitBox_Player = LayerMask.GetMask("HitBox_Player");
        LayerMask_HitBox_Enemy = LayerMask.GetMask("HitBox_Enemy");
        LayerMask_ItemDropped = LayerMask.GetMask("ItemDropped");

        Layer_UI = LayerMask.NameToLayer("UI");
        Layer_HitBox_Player = LayerMask.NameToLayer("HitBox_Player");
        Layer_HitBox_Enemy = LayerMask.NameToLayer("HitBox_Enemy");
        Layer_ItemDropped = LayerMask.NameToLayer("ItemDropped");
    }

    public int GetLayerByMechaCamp(Camp camp)
    {
        switch (camp)
        {
            case Camp.None:
            {
                return 0;
            }
            case Camp.Friend:
            {
                return Layer_HitBox_Player;
            }
            case Camp.Player:
            {
                return Layer_HitBox_Player;
            }
            case Camp.Enemy:
            {
                return Layer_HitBox_Enemy;
            }
        }

        return 0;
    }
}
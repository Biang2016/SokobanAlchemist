using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class BoxFunction_CollideBreakable : BoxFunctionBase
{
    protected override string BoxFunctionDisplayName => "撞击损坏耐久";

    [InfoBox("备注: 任一耐久值为0时箱子损坏")]
    [LabelText("公共碰撞耐久(-1无限)")]
    public int CommonDurability = -1;

    [LabelText("撞击箱子损坏耐久(-1无限)")]
    public int CollideWithBoxDurability = -1;

    [LabelText("撞击角色损坏耐久(-1无限)")]
    public int CollideWithActorDurability = -1;

    [ReadOnly]
    [ShowInInspector]
    [HideInEditorMode]
    [LabelText("公共碰撞剩余耐久")]
    private int remainCommonDurability;

    [ReadOnly]
    [ShowInInspector]
    [HideInEditorMode]
    [LabelText("撞击箱子损坏剩余耐久")]
    private int remainDurabilityCollideWithBox;

    [ReadOnly]
    [ShowInInspector]
    [HideInEditorMode]
    [LabelText("撞击角色损坏剩余耐久")]
    private int remainDurabilityCollideWithActor;

    public override void OnInit()
    {
        base.OnInit();
        remainCommonDurability = CommonDurability;
        remainDurabilityCollideWithBox = CollideWithBoxDurability;
        remainDurabilityCollideWithActor = CollideWithActorDurability;
    }

    public override void OnBeingKickedCollisionEnter(Collision collision)
    {
        base.OnBeingKickedCollisionEnter(collision);
        bool playCollideBehavior = CollideCalculate(collision);
        if (playCollideBehavior) kickCollideBehavior();

        void kickCollideBehavior()
        {
        }
    }

    public override void OnFlyingCollisionEnter(Collision collision)
    {
        base.OnFlyingCollisionEnter(collision);
        bool playCollideBehavior = CollideCalculate(collision);
        if (playCollideBehavior) flyCollideBehavior();

        void flyCollideBehavior()
        {
            Box box = collision.gameObject.GetComponentInParent<Box>();
            if (box && !box.BoxFeature.HasFlag(BoxFeature.IsBorder))
            {
                Box.Rigidbody.drag = Box.Throw_Drag * ConfigManager.BoxThrowDragFactor_Cheat;
            }
        }
    }

    private bool CollideCalculate(Collision collision)
    {
        bool playCollideBehavior = false;
        if (remainDurabilityCollideWithBox > 0 && collision.gameObject.layer == LayerManager.Instance.Layer_HitBox_Box)
        {
            Box box = collision.gameObject.GetComponentInParent<Box>();
            if (box != null)
            {
                remainDurabilityCollideWithBox--;
                if (remainDurabilityCollideWithBox == 0)
                {
                    Break();
                }
                else
                {
                    playCollideBehavior = true;
                }
            }
        }

        if (remainDurabilityCollideWithActor > 0 &&
            (collision.gameObject.layer == LayerManager.Instance.Layer_HitBox_Player ||
             collision.gameObject.layer == LayerManager.Instance.Layer_Player ||
             collision.gameObject.layer == LayerManager.Instance.Layer_HitBox_Enemy ||
             collision.gameObject.layer == LayerManager.Instance.Layer_Enemy))
        {
            Actor actor = collision.gameObject.GetComponentInParent<Actor>();
            if (actor != null)
            {
                if (Box.LastTouchActor != null && Box.LastTouchActor.IsOpponentCampOf(actor))
                {
                    remainDurabilityCollideWithActor--;
                    if (remainDurabilityCollideWithActor == 0)
                    {
                        Break();
                    }
                    else
                    {
                        playCollideBehavior = true;
                    }
                }
            }
        }

        remainCommonDurability--;
        if (remainCommonDurability == 0)
        {
            Break();
        }
        else
        {
            playCollideBehavior = true;
        }

        return playCollideBehavior;
    }

    private void Break()
    {
        Box.BoxFunctionMarkAsDeleted = true;
    }

    protected override void ChildClone(BoxFunctionBase newBF)
    {
        base.ChildClone(newBF);
        BoxFunction_CollideBreakable bf = ((BoxFunction_CollideBreakable) newBF);
        bf.CommonDurability = CommonDurability;
        bf.CollideWithBoxDurability = CollideWithBoxDurability;
        bf.CollideWithActorDurability = CollideWithActorDurability;
    }

    public override void CopyDataFrom(BoxFunctionBase srcData)
    {
        base.CopyDataFrom(srcData);
        BoxFunction_CollideBreakable bf = ((BoxFunction_CollideBreakable) srcData);
        CommonDurability = bf.CommonDurability;
        CollideWithBoxDurability = bf.CollideWithBoxDurability;
        CollideWithActorDurability = bf.CollideWithActorDurability;
    }
}
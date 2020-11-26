using UnityEngine;

public partial class Box
{
    public void OnBeingKickedCollisionEnter(Collision collision)
    {
        bool playCollideBehavior = CollideCalculate(collision, out bool validCollision);
        if (playCollideBehavior) kickCollideBehavior();

        void kickCollideBehavior()
        {
        }
    }

    public void OnFlyingCollisionEnter(Collision collision)
    {
        bool playCollideBehavior = CollideCalculate(collision, out bool validCollision);
        if (playCollideBehavior) flyCollideBehavior();

        void flyCollideBehavior()
        {
            Box box = collision.gameObject.GetComponentInParent<Box>();
            if (box && !box.BoxFeature.HasFlag(BoxFeature.IsBorder))
            {
                Rigidbody.drag = Throw_Drag * ConfigManager.BoxThrowDragFactor_Cheat;
            }
        }
    }

    public void OnDroppingFromAirCollisionEnter(Collision collision)
    {
        bool playCollideBehavior = CollideCalculate(collision, out bool validCollision);
        if (validCollision) CameraManager.Instance.FieldCamera.CameraShake(0.1f, 0.8f, (transform.position - BattleManager.Instance.Player1.transform.position).magnitude);
        if (playCollideBehavior) dropFromAirCollideBehavior();

        void dropFromAirCollideBehavior()
        {
        }
    }

    private bool CollideCalculate(Collision collision, out bool validCollision)
    {
        bool requireCommonDurabilityReduce = false;
        validCollision = false;
        if (BoxStatPropSet.CollideWithBoxDurability.Value > 0 && collision.gameObject.layer == LayerManager.Instance.Layer_HitBox_Box)
        {
            Box box = collision.gameObject.GetComponentInParent<Box>();
            if (box != null)
            {
                validCollision = true;
                if (BoxStatPropSet.FrozenLevel.Value >= 1)
                {
                    BoxStatPropSet.FrozenValue.Value = 0;
                }
                else
                {
                    BoxStatPropSet.CollideWithBoxDurability.Value--;
                    requireCommonDurabilityReduce = true;
                }
            }
        }

        if (BoxStatPropSet.CollideWithActorDurability.Value > 0 &&
            (collision.gameObject.layer == LayerManager.Instance.Layer_HitBox_Player ||
             collision.gameObject.layer == LayerManager.Instance.Layer_Player ||
             collision.gameObject.layer == LayerManager.Instance.Layer_HitBox_Enemy ||
             collision.gameObject.layer == LayerManager.Instance.Layer_Enemy))
        {
            Actor actor = collision.gameObject.GetComponentInParent<Actor>();
            if (actor != null)
            {
                if (LastTouchActor != null && LastTouchActor.IsOpponentCampOf(actor))
                {
                    validCollision = true;
                    if (BoxStatPropSet.FrozenLevel.Value >= 1)
                    {
                        BoxStatPropSet.FrozenValue.Value = 0;
                    }
                    else
                    {
                        BoxStatPropSet.CollideWithActorDurability.Value--;
                        requireCommonDurabilityReduce = true;
                    }
                }
            }
        }

        if (requireCommonDurabilityReduce) BoxStatPropSet.CommonDurability.Value--;
        return BoxStatPropSet.CollideWithBoxDurability.Value > 0 && BoxStatPropSet.CollideWithActorDurability.Value > 0 && BoxStatPropSet.CommonDurability.Value > 0;
    }
}
using UnityEngine;

public partial class Box
{
    public void OnBeingKickedCollisionEnter(Collision collision)
    {
        bool playCollideBehavior = CollideCalculate(collision);
        if (playCollideBehavior) kickCollideBehavior();

        void kickCollideBehavior()
        {
        }
    }

    public void OnFlyingCollisionEnter(Collision collision)
    {
        bool playCollideBehavior = CollideCalculate(collision);
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

    private bool CollideCalculate(Collision collision)
    {
        if (BoxStatPropSet.CollideWithBoxDurability.Value > 0 && collision.gameObject.layer == LayerManager.Instance.Layer_HitBox_Box)
        {
            Box box = collision.gameObject.GetComponentInParent<Box>();
            if (box != null)
            {
                BoxStatPropSet.CollideWithBoxDurability.Value--;
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
                    BoxStatPropSet.CollideWithActorDurability.Value--;
                }
            }
        }

        BoxStatPropSet.CommonDurability.Value--;
        return BoxStatPropSet.CollideWithBoxDurability.Value > 0 && BoxStatPropSet.CollideWithActorDurability.Value > 0 && BoxStatPropSet.CommonDurability.Value > 0;
    }
}
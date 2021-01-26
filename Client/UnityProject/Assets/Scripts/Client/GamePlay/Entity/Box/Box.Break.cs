using BiangLibrary;
using UnityEngine;

public partial class Box
{
    public enum BoxCollideType
    {
        Kick,
        Fly,
        DropFromAir
    }

    public void OnBeingKickedCollisionEnter(Collision collision)
    {
        bool playCollideBehavior = CollideCalculate(
            collision.collider,
            out bool validCollision,
            BoxCollideType.Kick);
        if (playCollideBehavior) kickCollideBehavior();

        void kickCollideBehavior()
        {
        }
    }

    public void OnFlyingCollisionEnter(Collision collision)
    {
        bool playCollideBehavior = CollideCalculate(
            collision.collider,
            out bool validCollision,
            BoxCollideType.Fly);
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
        bool playCollideBehavior = CollideCalculate(
            collision.collider,
            out bool validCollision,
            BoxCollideType.DropFromAir);
        if (validCollision) CameraManager.Instance.FieldCamera.CameraShake(0.1f, 0.4f, (transform.position - BattleManager.Instance.Player1.transform.position).magnitude);
        if (playCollideBehavior) dropFromAirCollideBehavior();

        void dropFromAirCollideBehavior()
        {
        }
    }

    private bool CollideCalculate(Collider collider, out bool validCollision, BoxCollideType collideType)
    {
        validCollision = false;

        // 和一般箱子相撞
        if (EntityStatPropSet.HealthDurability.Value > 0 &&
            (collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Box ||
             collider.gameObject.layer == LayerManager.Instance.Layer_BoxOnlyDynamicCollider)
        )
        {
            Box box = collider.gameObject.gameObject.GetComponentInParent<Box>();
            if (box != null)
            {
                validCollision = true;
                if (EntityStatPropSet.FrozenLevel.Value >= 1)
                {
                    EntityStatPropSet.FrozenValue.Value = 0;
                }
                else
                {
                    EntityStatPropSet.HealthDurability.Value--;
                }
            }
        }

        // 和世界碰撞体相撞
        if (EntityStatPropSet.HealthDurability.Value > 0 &&
            (collider.gameObject.layer == LayerManager.Instance.Layer_Wall)
        )
        {
            validCollision = true;
            if (EntityStatPropSet.FrozenLevel.Value >= 1)
            {
                EntityStatPropSet.FrozenValue.Value = 0;
            }
            else
            {
                EntityStatPropSet.HealthDurability.Value--;
            }
        }

        // 和角色碰撞体相撞
        if (EntityStatPropSet.HealthDurability.Value > 0 &&
            (collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Player ||
             collider.gameObject.layer == LayerManager.Instance.Layer_Player ||
             collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Enemy ||
             collider.gameObject.layer == LayerManager.Instance.Layer_Enemy))
        {
            Actor actor = collider.gameObject.GetComponentInParent<Actor>();
            if (actor != null)
            {
                if (LastTouchActor != null && LastTouchActor.IsOpponentCampOf(actor))
                {
                    validCollision = true;
                    if (EntityStatPropSet.FrozenLevel.Value >= 1)
                    {
                        EntityStatPropSet.FrozenValue.Value = 0;
                    }
                    else
                    {
                        EntityStatPropSet.HealthDurability.Value--;
                    }
                }
            }
        }

        if (collideType == BoxCollideType.DropFromAir) // 坠落有一定几率直接消失
        {
            if (!(EntityStatPropSet.DropFromAirSurviveProbabilityPercent.Value / 100f).ProbabilityBool())
            {
                EntityStatPropSet.HealthDurability.Value = 0;
            }
        }

        return EntityStatPropSet.HealthDurability.Value > 0;
    }
}
using System;
using UnityEngine;

[Serializable]
public class BoxPassiveSkill_EnemyFrozenBox : BoxPassiveSkill
{
    protected override string Description => "冻住敌人变成的箱子，勿乱用，(仅箱子生效)";

    public override void OnFlyingCollisionEnter(Collision collision)
    {
        base.OnFlyingCollisionEnter(collision);
    }

    public override void OnBeingKickedCollisionEnter(Collision collision, Box.KickAxis kickLocalAxis)
    {
        base.OnBeingKickedCollisionEnter(collision, kickLocalAxis);
        if (Box.FrozenActor.IsNotNullAndAlive())
        {
            if (collision.gameObject.layer == LayerManager.Instance.Layer_Enemy)
            {
                Actor actor = collision.gameObject.GetComponentInParent<Actor>();
                actor.EntityBuffHelper.Damage(Box.FrozenActor.EntityStatPropSet.GetCollideDamageByAxis(kickLocalAxis).GetModifiedValue, EntityBuffAttribute.CollideDamage);
                Box.FrozenActor.EntityBuffHelper.Damage(Box.FrozenActor.EntityStatPropSet.FrozenBeCollideDamage.GetModifiedValue, EntityBuffAttribute.CollideDamage);
            }
            else if (collision.gameObject.layer == LayerManager.Instance.Layer_HitBox_Box || collision.gameObject.layer == LayerManager.Instance.Layer_BoxOnlyDynamicCollider)
            {
                Box targetBox = collision.gameObject.GetComponentInParent<Box>();
                if (targetBox.FrozenActor.IsNotNullAndAlive())
                {
                    targetBox.FrozenActor.EntityBuffHelper.Damage(Box.FrozenActor.EntityStatPropSet.GetCollideDamageByAxis(kickLocalAxis).GetModifiedValue, EntityBuffAttribute.CollideDamage);
                    Box.FrozenActor.EntityBuffHelper.Damage(targetBox.FrozenActor.EntityStatPropSet.GetCollideDamageByAxis(kickLocalAxis).GetModifiedValue, EntityBuffAttribute.CollideDamage);
                }
                else
                {
                    Box.FrozenActor.EntityBuffHelper.Damage(Box.FrozenActor.EntityStatPropSet.FrozenBeCollideDamage.GetModifiedValue, EntityBuffAttribute.CollideDamage);
                }
            }
            else if (collision.gameObject.layer == LayerManager.Instance.Layer_Wall ||
                     collision.gameObject.layer == LayerManager.Instance.Layer_Ground)
            {
                Box.FrozenActor.EntityBuffHelper.Damage(1, EntityBuffAttribute.CollideDamage);
            }

            if (Box.FrozenActor.IsNotNullAndAlive())
            {
                Box.FrozenActor.EntityStatPropSet.FrozenValue.SetValue(Box.FrozenActor.EntityStatPropSet.FrozenValue.Value - Box.FrozenActor.EntityStatPropSet.FrozenValuePerLevel, "FrozenValueDecreaseByCollide");
            }
        }
    }

    public override void OnDestroyEntity()
    {
        base.OnDestroyEntity();
        Box.FrozenActor = null;
    }

    public override void CopyDataFrom(EntityPassiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
    }
}
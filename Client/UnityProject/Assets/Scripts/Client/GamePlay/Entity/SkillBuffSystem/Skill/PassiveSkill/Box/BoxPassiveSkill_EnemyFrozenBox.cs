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
                int damageOut = Box.FrozenActor.EntityStatPropSet.GetCollideDamageByAxis(kickLocalAxis).GetModifiedValue;
                int damageIn = Box.FrozenActor.EntityStatPropSet.FrozenBeCollideDamage.GetModifiedValue;
                Entity damageSource = Box.LastInteractEntity;
                actor.EntityBuffHelper.Damage(damageOut, EntityBuffAttribute.CollideDamage, damageSource);
                Box.FrozenActor.EntityBuffHelper.Damage(damageIn, EntityBuffAttribute.CollideDamage, damageSource);
            }
            else if (collision.gameObject.layer == LayerManager.Instance.Layer_Box || collision.gameObject.layer == LayerManager.Instance.Layer_BoxOnlyDynamicCollider)
            {
                Box targetBox = collision.gameObject.GetComponentInParent<Box>();
                if (targetBox.FrozenActor.IsNotNullAndAlive())
                {
                    int damageOut = Box.FrozenActor.EntityStatPropSet.GetCollideDamageByAxis(kickLocalAxis).GetModifiedValue;
                    int damageIn = targetBox.FrozenActor.EntityStatPropSet.GetCollideDamageByAxis(kickLocalAxis).GetModifiedValue;
                    Entity damageSource = Box.LastInteractEntity;
                    targetBox.FrozenActor.EntityBuffHelper.Damage(damageOut, EntityBuffAttribute.CollideDamage, damageSource);
                    Box.FrozenActor.EntityBuffHelper.Damage(damageIn, EntityBuffAttribute.CollideDamage, damageSource);
                }
                else
                {
                    int damageIn = Box.FrozenActor.EntityStatPropSet.FrozenBeCollideDamage.GetModifiedValue;
                    Entity damageSource = Box.LastInteractEntity;
                    Box.FrozenActor.EntityBuffHelper.Damage(damageIn, EntityBuffAttribute.CollideDamage, damageSource);
                }
            }
            else if (collision.gameObject.layer == LayerManager.Instance.Layer_Wall ||
                     collision.gameObject.layer == LayerManager.Instance.Layer_Ground)
            {
                int damageIn = Box.FrozenActor.EntityStatPropSet.FrozenBeCollideDamage.GetModifiedValue;
                Entity damageSource = Box.LastInteractEntity;
                Box.FrozenActor.EntityBuffHelper.Damage(damageIn, EntityBuffAttribute.CollideDamage, damageSource);
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
        if (Box.FrozenActor != null)
        {
            Box.FrozenActor.EntityFrozenHelper.FrozeIntoIceBlock(
                Box.FrozenActor.EntityStatPropSet.FrozenLevel.Value,
                0,
                Box.FrozenActor.EntityStatPropSet.FrozenLevel.MinValue,
                Box.FrozenActor.EntityStatPropSet.FrozenLevel.MaxValue
            ); // 此举为了避免Box因非战斗原因先于Actor销毁时，要先将Actor释放出来，否则Actor无法进入正常销毁逻辑
        }

        Box.FrozenActor = null;
    }
}
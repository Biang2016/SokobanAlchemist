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

    public override void OnBeingKickedCollisionEnter(Collision collision)
    {
        base.OnBeingKickedCollisionEnter(collision);
        if (Box.FrozenActor != null)
        {
            if (collision.gameObject.layer == LayerManager.Instance.Layer_Enemy)
            {
                Actor actor = collision.gameObject.GetComponentInParent<Actor>();
                actor.ActorBattleHelper.Damage(Box.FrozenActor, Box.FrozenActor.CollideDamage);
                Box.FrozenActor.ActorBattleHelper.Damage(Box.FrozenActor, 1);
            }
            else if (collision.gameObject.layer == LayerManager.Instance.Layer_HitBox_Box || collision.gameObject.layer == LayerManager.Instance.Layer_BoxOnlyDynamicCollider)
            {
                Box.FrozenActor.ActorBattleHelper.Damage(Box.FrozenActor, 1);
                Box targetBox = collision.gameObject.GetComponentInParent<Box>();
                if (targetBox.FrozenActor != null)
                {
                    targetBox.FrozenActor.ActorBattleHelper.Damage(Box.FrozenActor, 1);
                }
            }
            else if (collision.gameObject.layer == LayerManager.Instance.Layer_Wall ||
                     collision.gameObject.layer == LayerManager.Instance.Layer_Ground)
            {
                Box.FrozenActor.ActorBattleHelper.Damage(Box.FrozenActor, 1);
            }

            if (Box.FrozenActor != null && !Box.FrozenActor.IsRecycled)
            {
                Box.FrozenActor.EntityStatPropSet.FrozenValue.Value -= Box.FrozenActor.EntityStatPropSet.FrozenValuePerLevel;
            }
        }
    }

    public override void OnDestroyEntity()
    {
        base.OnDestroyEntity();
        Box.FrozenActor = null;
    }
}
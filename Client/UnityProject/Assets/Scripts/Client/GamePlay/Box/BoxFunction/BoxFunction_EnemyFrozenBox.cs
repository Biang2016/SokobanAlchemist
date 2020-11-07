using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class BoxFunction_EnemyFrozenBox : BoxFunctionBase
{
    protected override string BoxFunctionDisplayName => "冻住敌人变成的箱子，勿乱用";

    public override void OnFlyingCollisionEnter(Collision collision)
    {
        base.OnFlyingCollisionEnter(collision);
    }

    public override void OnBeingKickedCollisionEnter(Collision collision)
    {
        base.OnBeingKickedCollisionEnter(collision);
        if (Box.FrozenActor != null)
        {
            if (collision.gameObject.layer == LayerManager.Instance.Layer_HitBox_Enemy)
            {
                Actor actor = collision.gameObject.GetComponentInParent<Actor>();
                actor.ActorBattleHelper.Damage(Box.FrozenActor, Box.FrozenActor.CollideDamage);
            }
            else if (collision.gameObject.layer == LayerManager.Instance.Layer_HitBox_Box ||
                     collision.gameObject.layer == LayerManager.Instance.Layer_Wall ||
                     collision.gameObject.layer == LayerManager.Instance.Layer_Ground)
            {
                Box.FrozenActor.ActorBattleHelper.Damage(Box.FrozenActor, 1);
            }

            if (Box.FrozenActor != null)
            {
                Box.FrozenActor.ActorStatPropSet.FrozenValue.Value -= Box.FrozenActor.ActorStatPropSet.FrozenValuePerLevel;
            }
        }
    }

    public override void OnDeleteBox()
    {
        base.OnDeleteBox();
        Box.FrozenActor = null;
    }
}
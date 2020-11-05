using UnityEngine;
using System.Collections;

public class EnemyDamageBoxHelper : ActorMonoHelper
{
    private void OnTriggerEnter(Collider collider)
    {
        if (Actor.IsRecycled) return;
        if (Actor.ActorStatPropSet.IsFrozen) return;
        if (collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Player)
        {
            PlayerActor player = collider.gameObject.GetComponentInParent<PlayerActor>();
            if (player)
            {
                player.ActorBattleHelper.Damage(Actor, Actor.CollideDamage);
            }
        }
    }
}
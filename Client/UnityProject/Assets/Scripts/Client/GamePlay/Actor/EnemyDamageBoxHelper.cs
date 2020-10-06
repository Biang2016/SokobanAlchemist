using UnityEngine;
using System.Collections;

public class EnemyDamageBoxHelper : ActorHelper
{

    private void OnTriggerEnter(Collider collider)
    {
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

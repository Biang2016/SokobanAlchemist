using UnityEngine;
using System.Collections;

public class BoxShooter : MonoBehaviour
{
    private Box Box;

    public Transform ShooterDummy;

    public float ProjectileVelocity;

    public ProjectileType ProjectileType;

    private void Awake()
    {
        Box = GetComponentInParent<Box>();
    }

    private float ShootTick = 0;
    public float ShootInterval = 0.5f;
    public float ProjectileScale = 0.5f;

    private void FixedUpdate()
    {
        if (!Box.IsRecycled)
        {
            ShootTick += Time.fixedDeltaTime;
            if (ShootTick > ShootInterval)
            {
                Shoot();
                ShootTick -= ShootInterval;
            }
        }
        else
        {
            ShootTick = 0;
        }
    }

    public void Shoot()
    {
        //PlayerNumber playerNumber = PlayerNumber.Player1;
        //if (Box.BoxFeature.HasFlag(BoxFeature.BelongToPlayer1))
        //{
        //    playerNumber = PlayerNumber.Player1;
        //}
        //else if (Box.BoxFeature.HasFlag(BoxFeature.BelongToPlayer2))
        //{
        //    playerNumber = PlayerNumber.Player2;
        //}
        //else
        //{
        //    return;
        //}

        //ProjectileManager.Instance.EmitProjectile(ShooterDummy, ProjectileType, playerNumber, ProjectileVelocity, ProjectileScale);
    }
}
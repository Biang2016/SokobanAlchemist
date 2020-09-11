using BiangStudio.ObjectPool;
using UnityEngine;

public class Projectile : PoolObject
{
    private Rigidbody Rigidbody;
    private ParticleSystem ParticleSystem;

    private ProjectileColliderRoot ProjectileColliderRoot;

    private float Velocity;
    private float Scale;
    private ProjectileType ProjectileType;
    private PlayerNumber PlayerNumber;

    public override void OnRecycled()
    {
        StopSelfEffect();
        ProjectileColliderRoot.OnRecycled();
        Rigidbody.velocity = Vector3.zero;
        Rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;
        recycleByDurationTick = 0;
        base.OnRecycled();
    }

    void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        ProjectileColliderRoot = GetComponentInChildren<ProjectileColliderRoot>();
        ParticleSystem = GetComponentInChildren<ParticleSystem>();
    }

    void Start()
    {
    }

    public void Initialize(float velocity, Vector3 forward, ProjectileType projectileType, PlayerNumber playerNumber, float scale)
    {
        ProjectileColliderRoot.Init();
        Velocity = velocity;
        Scale = scale;
        ProjectileType = projectileType;
        PlayerNumber = playerNumber;
        transform.forward = forward;
        transform.localScale = Vector3.one * Scale;
        recycleByDurationTick = 0;
    }

    public void Launch(Transform dummyPos)
    {
        Rigidbody.constraints = RigidbodyConstraints.None;
        PlayFlashEffect(dummyPos);
        PlaySelfEffect();
        PoolRecycle(ParticleSystem.main.duration);
    }

    private float recycleByDurationTick = 0;

    void FixedUpdate()
    {
        if (!IsRecycled)
        {
            // 速度
            Rigidbody.velocity = transform.forward * Velocity;

            recycleByDurationTick += Time.fixedDeltaTime;
            bool recycleByDuration = recycleByDurationTick > 10f;

            if (recycleByDuration)
            {
                PoolRecycle();
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsRecycled)
        {
            ContactPoint contact = collision.contacts[0];
            Box box = collision.collider.GetComponentInParent<Box>();
            if (box)
            {
                if (box.BoxFeature.HasFlag(BoxFeature.ChessKing))
                {
                    if (box.BoxFeature.HasFlag(BoxFeature.BelongToPlayer1) && PlayerNumber == PlayerNumber.Player2)
                    {
                        BattleManager.Instance.Player1?.ActorBattleHelper.Damage(1);
                    }
                    else if (box.BoxFeature.HasFlag(BoxFeature.BelongToPlayer2) && PlayerNumber == PlayerNumber.Player1)
                    {
                        BattleManager.Instance.Player2?.ActorBattleHelper.Damage(1);
                    }
                }
            }

            PlayHitEffect(contact.point, contact.normal);
            PoolRecycle();
        }
    }

    private void PlaySelfEffect()
    {
        ParticleSystem.Play(true);
    }

    private void StopSelfEffect()
    {
        ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void PlayFlashEffect(Transform dummyPos)
    {
        if (GameObjectPoolManager.Instance.ProjectileFlashDict.TryGetValue(ProjectileType, out GameObjectPool flashPool))
        {
            ProjectileFlash flash = flashPool.AllocateGameObject<ProjectileFlash>(dummyPos);
            flash.transform.position = dummyPos.position;
            flash.transform.rotation = Quaternion.identity;
            flash.transform.localScale = Vector3.one * Scale;
            flash.transform.forward = dummyPos.forward;
            flash.Play();
        }
    }

    public void PlayHitEffect(Vector3 position, Vector3 direction)
    {
        ProjectileHit hit = ProjectileManager.Instance.PlayProjectileHit(ProjectileType, position);
        if (hit)
        {
            hit.transform.localScale = Vector3.one * Scale;
            hit.transform.LookAt(position + direction);
        }
    }
}

public enum ProjectileType
{
    Projectile_Leaves,
    Projectile_BloodBlade,
    Projectile_WhiteLightening,
    Projectile_Fire,
    Projectile_SnowFlake,
    Projectile_PurpleSmoke,
    Projectile_WhiteFlash,
    Projectile_PurpleGravBoom,
    Projectile_InterlacedRays,
    Projectile_GreenPoisonous,
    Projectile_BubbleBlade,
    Projectile_CyanSlight,
    Projectile_YellowLightening,
    Projectile_WaterBall,
    Projectile_FlyCutter,
    Projectile_SpiralDrill,
    Projectile_LoveHeart,
    Projectile_BlueArrowSmoke,
    Projectile_YellowLighteningHotBall,
    Projectile_EvilBigGravBall,
    Projectile_FastGreenBoom,
    Projectile_TwinkleLittleWhite,
    Projectile_Mushroom,
    Projectile_Butter,
    Projectile_ArrowsFly,
}
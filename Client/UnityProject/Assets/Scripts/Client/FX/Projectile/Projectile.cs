using BiangLibrary.ObjectPool;
using Sirenix.OdinInspector;
using UnityEngine;

public class Projectile : PoolObject
{
    private Rigidbody Rigidbody;
    private ParticleSystem ParticleSystem;

    private ProjectileColliderRoot ProjectileColliderRoot;

    private float Velocity;
    private float Scale;

    public string FlashFXName;
    public string HitFXName;

    [Button("生成Hit和Flash名称")]
    private void GenerateHitAndFlashName()
    {
        FlashFXName = name.Replace("Projectile_", "Flash_");
        HitFXName = name.Replace("Projectile_", "Hit_");
    }

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

    public void Initialize(float velocity, Vector3 forward, float scale)
    {
        ProjectileColliderRoot.Init();
        Velocity = velocity;
        Scale = scale;
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
        FX fx = FXManager.Instance.PlayFX(FlashFXName, dummyPos.position);
        if (fx)
        {
            fx.transform.parent = dummyPos;
            fx.transform.localScale = Vector3.one * Scale;
            fx.transform.forward = dummyPos.forward;
            fx.Play();
        }
    }

    public void PlayHitEffect(Vector3 position, Vector3 direction)
    {
        FX fx = FXManager.Instance.PlayFX(HitFXName, position);
        if (fx)
        {
            fx.transform.localScale = Vector3.one * Scale;
            fx.transform.LookAt(position + direction);
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

    Projectile_BlackDarkGreenExplode,
    Projectile_BloodyLightExplode,
    Projectile_BlueStarSmoke,
    Projectile_DarkBlueStar,
    Projectile_FireExplodeSmoke,
    Projectile_GreenPoisonBubbleExplode,
    Projectile_LightBlueLightningShock,
    Projectile_LightBlueQuickExplode,
    Projectile_LittleFireUp,
    Projectile_PinkSwirl,
    Projectile_RainbowFlowerExplode,
    Projectile_RainbowWaveFlower,
    Projectile_WaterBubbleExplode,
    Projectile_WhiteArrowLightning,
    Projectile_YellowFireStar,
}
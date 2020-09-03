using BiangStudio.ObjectPool;
using UnityEngine;

public class ProjectileFlash : PoolObject
{
    private ParticleSystem ParticleSystem;

    public override void OnRecycled()
    {
        Stop();
        base.OnRecycled();
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;
    }

    void Awake()
    {
        ParticleSystem = GetComponentInChildren<ParticleSystem>();
    }

    void Update()
    {
        if (!IsRecycled && ParticleSystem.isStopped)
        {
            PoolRecycle();
        }
    }

    public void Play()
    {
        ParticleSystem.Play(true);
    }

    public void Stop()
    {
        ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
using BiangStudio.ObjectPool;
using UnityEngine;

public class FX : PoolObject
{
    private ParticleSystem ParticleSystem;

    void Awake()
    {
        ParticleSystem = GetComponent<ParticleSystem>();
        if (!ParticleSystem)
        {
            ParticleSystem = GetComponentInChildren<ParticleSystem>();
        }
    }

    public override void PoolRecycle()
    {
        ParticleSystem.Stop(true);
        base.PoolRecycle();
    }

    public void Play()
    {
        ParticleSystem.Play(true);
        PoolRecycle(5f);
    }
}
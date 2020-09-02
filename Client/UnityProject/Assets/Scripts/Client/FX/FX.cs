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

    public override void OnRecycled()
    {
        ParticleSystem.Stop(true);
        base.OnRecycled();
    }

    public void Play()
    {
        ParticleSystem.Play(true);
        PoolRecycle(5f);
    }
}
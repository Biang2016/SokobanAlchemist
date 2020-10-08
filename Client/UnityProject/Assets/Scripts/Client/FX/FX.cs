using BiangStudio.ObjectPool;
using UnityEngine;
using UnityEngine.Events;

public class FX : PoolObject
{
    private ParticleSystem ParticleSystem;

    public UnityAction OnFXEnd;

    public override void OnRecycled()
    {
        Stop();
        OnFXEnd?.Invoke();
        base.OnRecycled();
        OnFXEnd = null;
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
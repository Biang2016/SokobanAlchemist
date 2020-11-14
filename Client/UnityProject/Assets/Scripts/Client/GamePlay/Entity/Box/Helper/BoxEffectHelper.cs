using BiangStudio.ObjectPool;
using UnityEngine;

public class BoxEffectHelper : PoolObject, IBoxHelper
{
    public override void OnRecycled()
    {
        Stop();
        base.OnRecycled();
    }

    public void OnBoxUsed()
    {
    }

    public void OnBoxPoolRecycled()
    {
        PoolRecycle();
    }

    public ParticleSystem ImpulseParticleSystem;

    public void Play()
    {
        if (!ImpulseParticleSystem.isPlaying)
        {
            ImpulseParticleSystem.Play(true);
        }
    }

    public void Stop()
    {
        ImpulseParticleSystem.Stop(true);
    }
}
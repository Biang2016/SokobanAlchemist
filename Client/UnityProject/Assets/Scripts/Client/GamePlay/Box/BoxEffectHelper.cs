using UnityEngine;
using BiangStudio.ObjectPool;

public class BoxEffectHelper : PoolObject
{
    public override void OnRecycled()
    {
        base.OnRecycled();
        Stop();
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
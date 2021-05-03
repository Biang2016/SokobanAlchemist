using BiangLibrary.ObjectPool;
using UnityEngine;

public class BoxTrail : PoolObject
{
    public ParticleSystem ImpulseParticleSystem;

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

    public void Play()
    {
        if (!ImpulseParticleSystem.isPlaying)
        {
            ImpulseParticleSystem.gameObject.SetActive(true);
            ImpulseParticleSystem.Play(true);
        }
    }

    public void Stop()
    {
        ImpulseParticleSystem.Stop(true);
        ImpulseParticleSystem.gameObject.SetActive(false);
    }
}
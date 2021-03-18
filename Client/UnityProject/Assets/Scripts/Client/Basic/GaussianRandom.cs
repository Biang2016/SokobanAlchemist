using System.Collections;
using UnityEngine;

public class GaussianRandom
{
    private ulong Seed;

    public GaussianRandom()
    {
        Seed = 61829450 + (ulong) Random.Range(-1000000, 1000000);
    }

    public GaussianRandom(ulong seed)
    {
        Seed = seed;
    }

    public float Range(float mean, float p100)
    {
        double sum = 0;
        for (int i = 0; i < 3; i++)
        {
            ulong holdSeed = Seed;
            Seed ^= Seed << 13;
            Seed ^= Seed >> 17;
            Seed ^= Seed << 5;
            long r = (long) (holdSeed + Seed);
            sum += (double) r * (1.0f / long.MaxValue);
        }

        return (float) sum / 3 * p100 + mean;
    }
}
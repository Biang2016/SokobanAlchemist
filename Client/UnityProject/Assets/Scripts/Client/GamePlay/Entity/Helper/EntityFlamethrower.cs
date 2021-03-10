using UnityEngine;

public class EntityFlamethrower : MonoBehaviour
{
    public ParticleSystem FirePS; // Main
    public ParticleSystem SmokePS;
    public ParticleSystem SparkPS;
    public Light FireLight;

    public EntityTriggerZone EntityTriggerZone_Flame;
}
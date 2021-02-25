using System;
using System.Collections;
using BiangLibrary.ObjectPool;
using UnityEngine;

public class EntityLightning : PoolObject
{
    public ParticleSystem LightningPS; // Main
    public ParticleSystem Sparks_Left;
    public ParticleSystem Sparks_Right;
    public ParticleSystem Sparks_Line;
    public Light LightningLight;

    internal EntityLightningGeneratorHelper StartGeneratorHelper;
    internal EntityLightningGeneratorHelper EndGeneratorHelper;

    public EntityTriggerZone EntityTriggerZone_Lightning;

    public override void OnRecycled()
    {
        base.OnRecycled();
        LightningPS.Stop(true);
        LightningLight.gameObject.SetActive(false);
        StartGeneratorHelper = null;
        EndGeneratorHelper = null;
    }

    public override void OnUsed()
    {
        base.OnUsed();
    }

    private Transform StartPivot;
    private Transform EndPivot;

    public void Initialize(Transform startPivot, Transform endPivot)
    {
        StartPivot = startPivot;
        EndPivot = endPivot;
        LightningPS.Play(true);
        LightningLight.gameObject.SetActive(true);
    }

    private void FixedUpdate()
    {
        if (!IsRecycled)
        {
            if (StartPivot != null && EndPivot != null)
            {
                transform.position = (StartPivot.position + EndPivot.position) / 2f;
                float scale = (StartPivot.position - EndPivot.position).magnitude / 13f;
                LightningPS.transform.localScale = new Vector3(scale / LightningPS.transform.parent.lossyScale.x, scale / LightningPS.transform.parent.lossyScale.y, scale / LightningPS.transform.parent.lossyScale.z);
                EntityTriggerZone_Lightning.transform.localScale = new Vector3(scale / EntityTriggerZone_Lightning.transform.parent.lossyScale.x, scale / EntityTriggerZone_Lightning.transform.parent.lossyScale.y, scale / EntityTriggerZone_Lightning.transform.parent.lossyScale.z);
                transform.LookAt(EndPivot);
            }
        }
    }
}
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

                // 闪电及伤害框尺寸不随Model节点缩放
                float scale = (StartPivot.position - EndPivot.position).magnitude;
                float lightningPSDefaultScale = 13f;
                LightningPS.transform.localScale = new Vector3(scale / LightningPS.transform.parent.lossyScale.x / lightningPSDefaultScale, scale / LightningPS.transform.parent.lossyScale.y / lightningPSDefaultScale, scale / LightningPS.transform.parent.lossyScale.z / lightningPSDefaultScale);
                BoxCollider boxCollider = (BoxCollider) EntityTriggerZone_Lightning.Collider;
                boxCollider.size = new Vector3(0.2f / EntityTriggerZone_Lightning.transform.lossyScale.x, 0.2f / EntityTriggerZone_Lightning.transform.lossyScale.y, scale / EntityTriggerZone_Lightning.transform.lossyScale.z);
                transform.LookAt(EndPivot);
            }
        }
    }
}
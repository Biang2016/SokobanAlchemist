﻿using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.ObjectPool;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class CollectableItem : PoolObject
{
    [SerializeReference]
    [LabelText("允许Collect的条件")]
    public List<EntitySkillCondition_State> EntitySkillConditions_ToCollect = new List<EntitySkillCondition_State>();

    [SerializeReference]
    [LabelText("Collect的效果")]
    public EntitySkillAction EntitySkillAction_OnCollect;

    [SerializeField]
    private Rigidbody Rigidbody;

    [SerializeField]
    private Collider Trigger;

    [SerializeField]
    private Collider Collider;

    [SerializeField]
    private ParticleSystem ParticleSystem;

    [SerializeField]
    private ParticleSystem TrailParticleSystem;

    [SerializeField]
    private Animator ModelAnimator;

    private Transform ChasingTarget;

    private UnityAction ChasedCallback;

    [SerializeField]
    private float ChasingForce;

    [SerializeField]
    private float ChasingTimeAccelerate;

    [SerializeField]
    private FXConfig ConsumeFX;

    internal WorldModule WorldModule;

    public enum Status
    {
        None,
        Flying,
        Floating,
        Chasing,
    }

    public Status CurrentStatus = Status.None;

    public override void OnRecycled()
    {
        base.OnRecycled();
        StopAllCoroutines();
        Rigidbody.isKinematic = true;
        Rigidbody.velocity = Vector3.zero;
        Trigger.enabled = false;
        Collider.enabled = false;
        ModelAnimator.SetBool("Floating", false);
        ChasingTarget = null;
        CurrentStatus = Status.None;
        TrailParticleSystem.gameObject.SetActive(false);
        ChasedCallback = null;
        WorldModule = null;
    }

    public override void OnUsed()
    {
        base.OnUsed();
        TrailParticleSystem.gameObject.SetActive(false);
        CurrentStatus = Status.None;
    }

    public void Initialize(WorldModule worldModule)
    {
        WorldModule = worldModule;
        ModelAnimator.SetBool("Floating", false);
    }

    public void ThrowFrom(Vector3 origin, Vector3 force)
    {
        if (CurrentStatus == Status.None)
        {
            CurrentStatus = Status.Flying;
            transform.position = origin;
            Rigidbody.isKinematic = false;
            Rigidbody.useGravity = true;
            Rigidbody.AddForce(force, ForceMode.VelocityChange);
            Collider.enabled = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (CurrentStatus == Status.Flying)
        {
            if (collision.gameObject.layer == LayerManager.Instance.Layer_Box)
            {
                bool isGrounded = WorldManager.Instance.CurrentWorld.CheckIsGroundByPos(transform.position, 1f, true, out GridPos3D _);
                if (isGrounded)
                {
                    CurrentStatus = Status.Floating;
                    StopAllCoroutines();
                    Rigidbody.useGravity = false;
                    Rigidbody.velocity = Vector3.zero;
                    Collider.enabled = false;
                    Trigger.enabled = true;
                    ModelAnimator.SetBool("Floating", true);
                }
            }
        }
    }

    public void SetChasingTarget(Transform target, UnityAction callBack)
    {
        if (CurrentStatus == Status.Flying || CurrentStatus == Status.Floating)
        {
            WorldModule.WorldModuleCollectableItems.Remove(this);
            WorldModule = null;
            CurrentStatus = Status.Chasing;
            StopAllCoroutines();
            Rigidbody.velocity = Vector3.zero;
            Rigidbody.useGravity = false;
            Collider.enabled = false;
            Trigger.enabled = false;
            ModelAnimator.SetBool("Floating", false);

            chasingTime = 0;
            ChasingTarget = target;
            if (TrailParticleSystem != null) TrailParticleSystem.gameObject.SetActive(true);
            ChasedCallback = callBack;
        }
    }

    private float chasingTime = 0f;

    private float destroyBecauseNotInAnyModuleTick = 0f;
    private float DestroyBecauseNotInAnyModuleThreshold = 1.5f;

    void FixedUpdate()
    {
        if (CurrentStatus == Status.Chasing && ChasingTarget != null)
        {
            chasingTime += Time.fixedDeltaTime;
            Rigidbody.AddForce((ChasingTarget.transform.position - transform.position).normalized * (ChasingForce + chasingTime * ChasingTimeAccelerate), ForceMode.Force);
            if ((transform.position - ChasingTarget.transform.position).magnitude < 0.7f)
            {
                FXManager.Instance.PlayFX(ConsumeFX, transform.position);
                ChasedCallback?.Invoke();
                PoolRecycle();
            }
        }

        if (BattleManager.Instance.IsStart)
        {
            if (WorldManager.Instance != null)
            {
                WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(transform.position.ToGridPos3D(), false);
                if (module == null)
                {
                    destroyBecauseNotInAnyModuleTick += Time.fixedDeltaTime;
                    if (destroyBecauseNotInAnyModuleTick > DestroyBecauseNotInAnyModuleThreshold)
                    {
                        destroyBecauseNotInAnyModuleTick = 0;
                        PoolRecycle();
                    }
                }
            }
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ObjectPool;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class Actor : PoolObject
{
    public Rigidbody RigidBody;
    public ActorPushHelper ActorPushHelper;
    public ActorFaceHelper ActorFaceHelper;
    public ActorSkinHelper ActorSkinHelper;
    public ActorLaunchArcRendererHelper ActorLaunchArcRendererHelper;
    public Transform LiftBoxPivot;

    [ReadOnly]
    [DisplayAsString]
    [LabelText("移动倾向")]
    public Vector3 CurMoveAttempt;

    [ReadOnly]
    [DisplayAsString]
    [LabelText("上一帧移动倾向")]
    public Vector3 LastMoveAttempt;

    protected Vector3 CurForward = Vector3.forward;

    [DisplayAsString]
    [LabelText("世界坐标")]
    public GridPos3D CurGP;

    [LabelText("阵营")]
    public Camp Camp;

    [LabelText("踢箱子力量")]
    public float KickForce = 5;

    [LabelText("起步速度")]
    public float Accelerate = 10f;

    [LabelText("移动速度")]
    public float MoveSpeed = 10f;

    [LabelText("扔箱子力量")]
    public float ThrowForce = 100;

    [LabelText("扔箱子蓄力速度曲线(X为重量Y为蓄力速度)")]
    public AnimationCurve ThrowChargeSpeedCurveByWeight;

    [ReadOnly]
    [LabelText("扔箱子蓄力速度因子(作弊调整)")]
    public float ThrowChargeSpeedFactor_Cheat = 1f;

    [LabelText("扔箱子蓄力曲线(X为重量Y为蓄力上限)")]
    public AnimationCurve ThrowChargeMaxCurveByWeight;

    [ReadOnly]
    [LabelText("扔箱子蓄力曲线因子(作弊调整)")]
    public float ThrowChargeMaxCurveFactor_Cheat = 1f;

    private List<SmoothMove> SmoothMoves = new List<SmoothMove>();

    internal Box CurrentLiftBox = null;

    public bool IsPlayer => Camp == Camp.Player;

    public enum MovementStates
    {
        Static,
        Moving,
    }

    [ReadOnly]
    [LabelText("移动状态")]
    public MovementStates MovementState = MovementStates.Static;

    public enum PushStates
    {
        None,
        Pushing,
    }

    [ReadOnly]
    [LabelText("推箱子状态")]
    public PushStates PushState = PushStates.None;

    public enum ThrowStates
    {
        None,
        Raising,
        Lifting,
        ThrowCharging,
    }

    [ReadOnly]
    [LabelText("扔技能状态")]
    public ThrowStates ThrowState = ThrowStates.None;

    void Awake()
    {
        SmoothMoves = GetComponentsInChildren<SmoothMove>().ToList();
        foreach (SmoothMove sm in SmoothMoves)
        {
            sm.enabled = false;
        }

        ClientGameManager.Instance.BattleMessenger.AddListener<Actor>((uint) Enum_Events.OnPlayerLoaded, OnLoaded);
    }

    private void Update()
    {
        UpdateThrowParabolaLine();
    }

    public void OnLoaded(Actor actor)
    {
        if (actor == this)
        {
            foreach (SmoothMove sm in SmoothMoves)
            {
                sm.enabled = true;
            }
        }
    }

    public void SetShown(bool shown)
    {
    }

    protected virtual void FixedUpdate()
    {
        RigidBody.angularVelocity = Vector3.zero;
        CurGP = GridPos3D.GetGridPosByTrans(transform, 1);

        #region Move

        if (CurMoveAttempt.magnitude > 0)
        {
            if (!CurMoveAttempt.x.Equals(0) && !CurMoveAttempt.z.Equals(0))
            {
                if (!LastMoveAttempt.x.Equals(0))
                {
                    CurMoveAttempt.z = 0;
                }
                else if (!LastMoveAttempt.z.Equals(0))
                {
                    CurMoveAttempt.x = 0;
                }
                else
                {
                    CurMoveAttempt.z = 0;
                }
            }

            if (CurMoveAttempt.x.Equals(0)) RigidBody.velocity = new Vector3(0, RigidBody.velocity.y, RigidBody.velocity.z);
            if (CurMoveAttempt.z.Equals(0)) RigidBody.velocity = new Vector3(RigidBody.velocity.x, RigidBody.velocity.y, 0);

            MovementState = MovementStates.Moving;
            RigidBody.drag = 0;
            RigidBody.AddForce(CurMoveAttempt);

            if (RigidBody.velocity.magnitude > MoveSpeed)
            {
                RigidBody.AddForce(-RigidBody.velocity.normalized * (RigidBody.velocity.magnitude - MoveSpeed), ForceMode.VelocityChange);
            }

            transform.forward = CurMoveAttempt;
            CurForward = transform.forward;
            ActorPushHelper.PushTriggerOut();
        }
        else
        {
            transform.forward = CurForward;
            MovementState = MovementStates.Static;
            RigidBody.drag = 100f;
            ActorPushHelper.PushTriggerReset();
        }

        InternalFixedUpdate();

        LastMoveAttempt = CurMoveAttempt;

        #endregion
    }

    protected virtual void InternalFixedUpdate()
    {

    }

    #region Skills

    public void Kick()
    {
        ActorPushHelper.ActorPushHelperTrigger.curPushingBox?.Kick(CurForward, KickForce);
    }

    public void Lift()
    {
        if (CurrentLiftBox) return;
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 1.3f, LayerManager.Instance.LayerMask_Box, QueryTriggerInteraction.Collide))
        {
            Box box = hit.collider.gameObject.GetComponentInParent<Box>();
            if (box && box.Liftable())
            {
                if (box.BeingLift())
                {
                    CurrentLiftBox = box;
                    ThrowState = ThrowStates.Raising;
                    ActorFaceHelper.FacingBoxList.Remove(box);
                    box.transform.parent = LiftBoxPivot.transform.parent;
                    box.transform.DOLocalMove(LiftBoxPivot.transform.localPosition, 0.2f).OnComplete(() =>
                    {
                        box.State = Box.States.Lifted;
                        ThrowState = ThrowStates.Lifting;
                    });
                }
            }
        }
    }

    private float ThrowChargeTick = 0;
    private float FinalThrowForce => ThrowChargeTick * ThrowForce;

    protected void ThrowCharge()
    {
        if (!CurrentLiftBox)
        {
            ThrowChargeTick = 0;
            return;
        }

        if (ThrowState == ThrowStates.Lifting)
        {
            ThrowState = ThrowStates.ThrowCharging;
        }

        if (ThrowState == ThrowStates.ThrowCharging)
        {
            float max = ThrowChargeMaxCurveByWeight.Evaluate(CurrentLiftBox.FinalWeight) * ThrowChargeMaxCurveFactor_Cheat;
            ThrowChargeTick += Time.fixedDeltaTime * ThrowChargeSpeedCurveByWeight.Evaluate(CurrentLiftBox.FinalWeight) * ThrowChargeSpeedFactor_Cheat;
            if (ThrowChargeTick > max) ThrowChargeTick = max;
        }
    }

    private void UpdateThrowParabolaLine()
    {
        bool isCharging = CurrentLiftBox && ThrowState == ThrowStates.ThrowCharging;
        ActorLaunchArcRendererHelper.SetShown(isCharging);
        if (isCharging)
        {
            float velocity = GetThrowBoxVelocity(CurrentLiftBox);
            ActorLaunchArcRendererHelper.Initialize(velocity, 45, 2, 3f);
        }
    }

    public void Throw()
    {
        if (CurrentLiftBox && ThrowState == ThrowStates.ThrowCharging)
        {
            float velocity = GetThrowBoxVelocity(CurrentLiftBox);
            ThrowState = ThrowStates.None;
            Vector3 throwVel = transform.TransformDirection(new Vector3(0, 1, 1));
            CurrentLiftBox.Throw(throwVel, velocity);
            CurrentLiftBox = null;
            ThrowChargeTick = 0;
        }
    }

    private float GetThrowBoxVelocity(Box box)
    {
        return 3.5f + FinalThrowForce * Time.fixedDeltaTime / box.FinalWeight;
    }

    #endregion
}
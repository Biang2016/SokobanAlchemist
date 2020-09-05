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
    public ActorBattleHelper ActorBattleHelper;
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
    [BoxGroup("战斗状态")]
    public GridPos3D CurGP;

    [LabelText("阵营")]
    [BoxGroup("战斗状态")]
    [DisableInPlayMode]
    public Camp Camp;

    [LabelText("总血量")]
    [BoxGroup("战斗状态")]
    [SerializeField]
    [DisableInPlayMode]
    private int MaxHealth;

    [LabelText("总命数")]
    [BoxGroup("战斗状态")]
    [SerializeField]
    [DisableInPlayMode]
    private int TotalLife;

    [ShowInInspector]
    [LabelText("剩余命数")]
    [BoxGroup("战斗状态")]
    public int Life => ActorBattleHelper ? ActorBattleHelper.Life : 0;

    [ShowInInspector]
    [LabelText("剩余生命值")]
    [BoxGroup("战斗状态")]
    public int Health => ActorBattleHelper ? ActorBattleHelper.Health : 0;

    [BoxGroup("配置")]
    [LabelText("起步速度")]
    public float Accelerate = 10f;

    [BoxGroup("配置")]
    [LabelText("移动速度")]
    public float MoveSpeed = 10f;

    [BoxGroup("配置")]
    [LabelText("会踢箱子")]
    public bool CanKickBox = true;

    [BoxGroup("配置")]
    [LabelText("踢箱子力量")]
    [ShowIf("CanKickBox")]
    public float KickForce = 5;

    [BoxGroup("配置")]
    [LabelText("会扔箱子")]
    public bool CanThrowBox = true;

    [BoxGroup("配置")]
    [LabelText("扔箱子力量")]
    [ShowIf("CanThrowBox")]
    public float ThrowForce = 100;

    [BoxGroup("配置")]
    [LabelText("扔箱子蓄力速度曲线(X为重量Y为蓄力速度)")]
    [ShowIf("CanThrowBox")]
    public AnimationCurve ThrowChargeSpeedCurveByWeight;

    [ReadOnly]
    [BoxGroup("配置")]
    [LabelText("扔箱子蓄力速度因子(作弊调整)")]
    [ShowIf("CanThrowBox")]
    public float ThrowChargeSpeedFactor_Cheat = 1f;

    [BoxGroup("配置")]
    [LabelText("扔箱子蓄力曲线(X为重量Y为蓄力上限)")]
    [ShowIf("CanThrowBox")]
    public AnimationCurve ThrowChargeMaxCurveByWeight;

    [ReadOnly]
    [BoxGroup("配置")]
    [LabelText("扔箱子蓄力曲线因子(作弊调整)")]
    [ShowIf("CanThrowBox")]
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
    [BoxGroup("战斗状态")]
    [LabelText("移动状态")]
    public MovementStates MovementState = MovementStates.Static;

    public enum PushStates
    {
        None,
        Pushing,
    }

    [ReadOnly]
    [BoxGroup("战斗状态")]
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
    [BoxGroup("战斗状态")]
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
        ActorBattleHelper.Initialize(TotalLife, MaxHealth);
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
    }

    #region Skills

    public void KickDoubleClick()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        Debug.DrawRay(ray.origin, ray.direction, Color.red, 0.3f);
        if (Physics.Raycast(ray, out RaycastHit hit, 1.5f, LayerManager.Instance.LayerMask_Box, QueryTriggerInteraction.Collide))
        {
            Box box = hit.collider.gameObject.GetComponentInParent<Box>();
            if (box && box.Pushable())
            {
                box.Kick(CurForward, KickForce, this);
            }
        }
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
                if (box.BeingLift(this))
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
            CurrentLiftBox.Throw(throwVel, velocity, this);
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
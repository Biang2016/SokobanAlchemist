using System.Collections.Generic;
using System.Linq;
using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ObjectPool;
using DG.Tweening;
using NodeCanvas.BehaviourTrees;
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

    internal BehaviourTreeOwner BehaviourTreeOwner;
    internal ActorAIAgent ActorAIAgent;

    [ReadOnly]
    [DisplayAsString]
    [LabelText("移动倾向")]
    public Vector3 CurMoveAttempt;

    [ReadOnly]
    [DisplayAsString]
    [LabelText("上一帧移动倾向")]
    public Vector3 LastMoveAttempt;

    [ReadOnly]
    [DisplayAsString]
    [LabelText("扔箱子瞄准点移动倾向")]
    public Vector3 CurThrowMoveAttempt;

    [ReadOnly]
    [DisplayAsString]
    [LabelText("扔箱子瞄准点偏移")]
    public Vector3 CurThrowPointOffset;

    internal Vector3 CurForward = Vector3.forward;

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

    [BoxGroup("特效")]
    [LabelText("死亡特效")]
    public ProjectileType DieFX;

    [BoxGroup("特效")]
    [LabelText("死亡特效尺寸")]
    public float DieFXScale = 1f;

    [BoxGroup("配置")]
    [LabelText("起步速度")]
    public float Accelerate = 10f;

    [BoxGroup("配置")]
    [LabelText("移动速度")]
    public float MoveSpeed = 10f;

    [BoxGroup("配置")]
    [LabelText("瞄准点移动速度")]
    public float ThrowAimMoveSpeed = 10f;

    private float ThrowRadiusMin = 0.75f;

    [BoxGroup("配置")]
    [LabelText("扔半径")]
    public float ThrowRadius = 10f;

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

    private List<SmoothMove> SmoothMoves = new List<SmoothMove>();

    [DisableInEditorMode]
    [ShowInInspector]
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

    public override void OnRecycled()
    {
        BehaviourTreeOwner.StopBehaviour();
        ActorAIAgent.Stop();
        CurMoveAttempt = Vector3.zero;
        LastMoveAttempt = Vector3.zero;
        CurThrowMoveAttempt = Vector3.zero;
        CurThrowPointOffset = Vector3.zero;
        CurForward = Vector3.forward;
        CurGP = GridPos3D.Zero;
        MovementState = MovementStates.Static;
        PushState = PushStates.None;
        ThrowState = ThrowStates.None;
        ThrowWhenDie();
        ActorPushHelper.OnRecycled();
        ActorFaceHelper.OnRecycled();
        ActorSkinHelper.OnRecycled();
        ActorLaunchArcRendererHelper.OnRecycled();
        ActorBattleHelper.OnRecycled();
        RigidBody.drag = 100f;
        RigidBody.velocity = Vector3.zero;
        RigidBody.angularVelocity = Vector3.zero;
        base.OnRecycled();
    }

    void Awake()
    {
        ActorAIAgent = new ActorAIAgent(this);
        BehaviourTreeOwner = GetComponent<BehaviourTreeOwner>();
        SmoothMoves = GetComponentsInChildren<SmoothMove>().ToList();
        foreach (SmoothMove sm in SmoothMoves)
        {
            sm.enabled = false;
        }

        ClientGameManager.Instance.BattleMessenger.AddListener<Actor>((uint) Enum_Events.OnPlayerLoaded, OnLoaded);
        ActorBattleHelper.Initialize(TotalLife, MaxHealth);
    }

    public void Initialize()
    {
        CurGP = GridPos3D.GetGridPosByTrans(transform, 1);
        ActorAIAgent.Start();
    }

    private void Update()
    {
        if (!IsRecycled)
        {
            UpdateThrowParabolaLine();
        }
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
        if (!IsRecycled)
        {
            RigidBody.angularVelocity = Vector3.zero;
            CurGP = GridPos3D.GetGridPosByTrans(transform, 1);
        }
    }

    private Tweener TransTweener_X;
    private Tweener TransTweener_Z;

    protected virtual void MoveInternal()
    {
        if (CurMoveAttempt.magnitude > 0)
        {
            if (CurMoveAttempt.x.Equals(0)) RigidBody.velocity = new Vector3(0, RigidBody.velocity.y, RigidBody.velocity.z);
            if (CurMoveAttempt.z.Equals(0)) RigidBody.velocity = new Vector3(RigidBody.velocity.x, RigidBody.velocity.y, 0);
            MovementState = MovementStates.Moving;
            RigidBody.drag = 0;
            RigidBody.AddForce(CurMoveAttempt * Time.fixedDeltaTime * Accelerate);
            if (RigidBody.velocity.magnitude > MoveSpeed)
            {
                RigidBody.AddForce(-RigidBody.velocity.normalized * (RigidBody.velocity.magnitude - MoveSpeed), ForceMode.VelocityChange);
            }

            transform.forward = CurMoveAttempt;
            CurForward = CurMoveAttempt.normalized;
            ActorPushHelper.PushTriggerOut();
        }
        else
        {
            transform.forward = CurForward;
            MovementState = MovementStates.Static;
            RigidBody.drag = 100f;
            ActorPushHelper.PushTriggerReset();
        }

        if (CurMoveAttempt.x.Equals(0))
        {
            transform.position = new Vector3(CurGP.x, transform.position.y, transform.position.z);
        }

        if (CurMoveAttempt.z.Equals(0))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, CurGP.z);
        }

        LastMoveAttempt = CurMoveAttempt;
    }

    private float ThrowChargeTick;
    internal float ThrowChargeMax = 1.5f;

    protected virtual void ThrowChargeAimInternal()
    {
        if (ThrowState == ThrowStates.ThrowCharging)
        {
            if (ThrowChargeTick < ThrowChargeMax)
            {
                ThrowChargeTick += Time.fixedDeltaTime;
            }
            else
            {
                ThrowChargeTick = ThrowChargeMax;
            }

            float radius = ThrowRadius * ThrowChargeTick / ThrowChargeMax + ThrowRadiusMin;
            if (CurThrowPointOffset.magnitude > radius)
            {
                CurThrowPointOffset = CurThrowPointOffset.normalized * radius;
            }
        }
        else
        {
            CurThrowPointOffset = Vector3.zero;
            ThrowChargeTick = 0;
        }
    }

    #region Skills

    public void Kick()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        Debug.DrawRay(ray.origin, ray.direction, Color.red, 0.3f);
        if (Physics.Raycast(ray, out RaycastHit hit, 1.5f, LayerManager.Instance.LayerMask_Box, QueryTriggerInteraction.Collide))
        {
            Box box = hit.collider.gameObject.GetComponentInParent<Box>();
            if (box && box.Kickable)
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
            if (box && box.Liftable)
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

    protected void ThrowCharge()
    {
        if (!CurrentLiftBox) return;
        if (ThrowState == ThrowStates.Lifting)
        {
            ThrowState = ThrowStates.ThrowCharging;
            CurThrowPointOffset = transform.forward * ThrowRadiusMin;
        }
    }

    private void UpdateThrowParabolaLine()
    {
        bool isCharging = CurrentLiftBox && ThrowState == ThrowStates.ThrowCharging;
        ActorLaunchArcRendererHelper.SetShown(isCharging);
        if (isCharging)
        {
            ActorLaunchArcRendererHelper.InitializeByOffset(CurThrowPointOffset, 45, 1, 3f);
        }
    }

    public void Throw()
    {
        if (CurrentLiftBox && ThrowState == ThrowStates.ThrowCharging)
        {
            float velocity = ActorLaunchArcRendererHelper.CalculateVelocityByOffset(CurThrowPointOffset, 45);
            ThrowState = ThrowStates.None;
            Vector3 throwVel = (CurThrowPointOffset.normalized + Vector3.up) * velocity;
            CurrentLiftBox.Throw(throwVel, velocity, this);
            CurrentLiftBox = null;
        }
    }

    private void ThrowWhenDie()
    {
        if (CurrentLiftBox)
        {
            ThrowState = ThrowStates.None;
            Vector3 throwVel = Vector3.up * 3;
            CurrentLiftBox.Throw(throwVel, 3, this);
            CurrentLiftBox = null;
        }
    }

    #endregion
}
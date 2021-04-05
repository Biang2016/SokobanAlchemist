using System.Collections.Generic;
using System.Linq;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.GamePlay.UI;
using DG.Tweening;
using NodeCanvas.Framework;
using Sirenix.OdinInspector;
using UnityEngine;

public class Actor : Entity
{
    #region GUID

    [ReadOnly]
    [HideInEditorMode]
    public uint BornPointDataGUID; // 出生点GUID

    #endregion

    public static bool ENABLE_ACTOR_MOVE_LOG =
#if UNITY_EDITOR
        false;
#else
        false;
#endif

    private bool forbidAction = false;

    internal bool ForbidAction
    {
        get { return forbidAction; }
        set
        {
            if (forbidAction != value)
            {
                forbidAction = value;
                if (forbidAction)
                {
                    RemoveRigidbody();
                }
                else
                {
                    AddRigidbody();
                }
            }
        }
    }

    internal bool HasRigidbody = true;

    [FoldoutGroup("组件")]
    public Rigidbody RigidBody;

    [FoldoutGroup("组件")]
    public ActorCommonHelpers ActorCommonHelpers;

    public Vector3 ArtPos => ActorSkinHelper.MainArtTransform.position;

    internal override EntityModelHelper EntityModelHelper => ActorCommonHelpers.EntityModelHelper;
    internal override EntityIndicatorHelper EntityIndicatorHelper => ActorCommonHelpers.EntityIndicatorHelper;
    internal override EntityBuffHelper EntityBuffHelper => ActorCommonHelpers.EntityBuffHelper;
    internal override EntityFrozenHelper EntityFrozenHelper => ActorFrozenHelper;
    internal override EntityTriggerZoneHelper EntityTriggerZoneHelper => ActorCommonHelpers.EntityTriggerZoneHelper;
    internal override EntityGrindTriggerZoneHelper EntityGrindTriggerZoneHelper => ActorCommonHelpers.EntityGrindTriggerZoneHelper;
    internal override List<EntityFlamethrowerHelper> EntityFlamethrowerHelpers => ActorCommonHelpers.EntityFlamethrowerHelpers;
    internal override List<EntityLightningGeneratorHelper> EntityLightningGeneratorHelpers => ActorCommonHelpers.ActorLightningGeneratorHelpers;
    internal GameObject ActorMoveColliderRoot => ActorCommonHelpers.ActorMoveColliderRoot;
    internal ActorArtHelper ActorArtHelper => ActorCommonHelpers.ActorArtHelper;
    internal ActorPushHelper ActorPushHelper => ActorCommonHelpers.ActorPushHelper;
    internal ActorFaceHelper ActorFaceHelper => ActorCommonHelpers.ActorFaceHelper;
    internal ActorSkinHelper ActorSkinHelper => ActorCommonHelpers.ActorSkinHelper;
    internal ActorLaunchArcRendererHelper ActorLaunchArcRendererHelper => ActorCommonHelpers.ActorLaunchArcRendererHelper;
    internal ActorBattleHelper ActorBattleHelper => ActorCommonHelpers.ActorBattleHelper;
    internal ActorBoxInteractHelper ActorBoxInteractHelper => ActorCommonHelpers.ActorBoxInteractHelper;
    internal ActorFrozenHelper ActorFrozenHelper => ActorCommonHelpers.ActorFrozenHelper;
    internal Transform LiftBoxPivot => ActorCommonHelpers.LiftBoxPivot;

    internal GraphOwner GraphOwner;
    internal ActorAIAgent ActorAIAgent;

    #region 状态

    [ReadOnly]
    [DisplayAsString]
    [FoldoutGroup("状态")]
    [LabelText("角色分类")]
    public ActorCategory ActorCategory;

    [ReadOnly]
    [DisplayAsString]
    [FoldoutGroup("状态")]
    [LabelText("角色类型")]
    public string ActorType;

    [ReadOnly]
    [DisplayAsString]
    [FoldoutGroup("状态")]
    [LabelText("移动倾向")]
    public Vector3 CurMoveAttempt;

    [ReadOnly]
    [DisplayAsString]
    [FoldoutGroup("状态")]
    [LabelText("上一帧移动倾向")]
    public Vector3 LastMoveAttempt;

    [ReadOnly]
    [DisplayAsString]
    [FoldoutGroup("状态")]
    [LabelText("扔箱子瞄准点移动倾向")]
    public Vector3 CurThrowMoveAttempt;

    [ReadOnly]
    [DisplayAsString]
    [FoldoutGroup("状态")]
    [LabelText("扔箱子瞄准点偏移")]
    public Vector3 CurThrowPointOffset;

    [DisplayAsString]
    [ShowInInspector]
    [FoldoutGroup("状态")]
    [LabelText("世界坐标")]
    public override GridPos3D WorldGP
    {
        get { return curWorldGP; }
        set
        {
            if (curWorldGP != value)
            {
                WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(value, true);
                if (module) curWorldGP = value; // 此处判空可防止角色走入空模组
            }
        }
    }

    private GridPos3D curWorldGP;

    /// <summary>
    /// 这是寻路专用节点坐标，和世界坐标略有偏差，Y坐标为角色基底那层的Y坐标
    /// </summary>
    [DisplayAsString]
    [HideInEditorMode]
    [FoldoutGroup("状态")]
    [LabelText("寻路节点坐标")]
    public GridPos3D WorldGP_PF
    {
        get
        {
            int xmin = int.MaxValue;
            int ymin = int.MaxValue;
            int zmin = int.MaxValue;
            foreach (GridPos3D offset in GetEntityOccupationGPs_Rotated())
            {
                if (offset.x < xmin) xmin = offset.x;
                if (offset.y < ymin) ymin = offset.y;
                if (offset.z < zmin) zmin = offset.z;
            }

            return WorldGP + new GridPos3D((ActorWidth - 1) / 2 + xmin, ymin, (ActorWidth - 1) / 2 + zmin);
        }
    }

    [DisableInEditorMode]
    [ShowInInspector]
    [FoldoutGroup("状态")]
    [LabelText("当前举着的箱子")]
    internal Box CurrentLiftBox = null;

    [DisableInEditorMode]
    [ShowInInspector]
    [FoldoutGroup("状态")]
    [LabelText("是否着地")]
    internal bool IsGrounded = false;

    #endregion

    #region 旋转朝向

    internal override void SwitchEntityOrientation(GridPosR.Orientation newOrientation)
    {
        if (EntityOrientation == newOrientation) return;

        // Actor由于限制死平面必须是正方形，因此可以用左下角坐标相减得到核心坐标偏移量；在旋转时应用此偏移量，可以保证平面正方形仍在老位置
        GridPos offset = ActorRotateWorldGPOffset(ActorWidth, newOrientation) - ActorRotateWorldGPOffset(ActorWidth, EntityOrientation);
        base.SwitchEntityOrientation(newOrientation);

        int delta_x = offset.x;
        int delta_z = offset.z;

        GridPosR.ApplyGridPosToLocalTrans(new GridPosR(delta_x + curWorldGP.x, delta_z + curWorldGP.z, newOrientation), transform, 1);
        WorldGP = GridPos3D.GetGridPosByTrans(transform, 1);
    }

    public static GridPos ActorRotateWorldGPOffset(int actorWidth, GridPosR.Orientation orientation) // todo 这里的先决条件是所有的Actor都以左下角作为原点
    {
        switch (orientation)
        {
            case GridPosR.Orientation.Up:
                return GridPos.Zero;
            case GridPosR.Orientation.Right:
                return new GridPos(0, actorWidth - 1);
            case GridPosR.Orientation.Down:
                return new GridPos(actorWidth - 1, actorWidth - 1);
            case GridPosR.Orientation.Left:
                return new GridPos(actorWidth - 1, 0);
        }

        return GridPos.Zero;
    }

    #endregion

    #region Occupation

    public const int ACTOR_MAX_HEIGHT = 4;
    public int ActorWidth => EntityIndicatorHelper.EntityOccupationData.ActorWidth;
    public int ActorHeight => EntityIndicatorHelper.EntityOccupationData.ActorHeight;

    public bool CheckIsGrounded(float checkDistance, out GridPos3D nearestGroundGP)
    {
        bool isGrounded = false;
        nearestGroundGP = GridPos3D.Zero;
        float nearestDistance = float.MaxValue;
        foreach (GridPos3D offset in GetEntityOccupationGPs_Rotated())
        {
            Vector3 startPos = transform.position + offset;
            bool gridGrounded = WorldManager.Instance.CurrentWorld.CheckIsGroundByPos(startPos, checkDistance, out GridPos3D groundGP);
            isGrounded |= gridGrounded;
            if (gridGrounded)
            {
                float dist = (nearestGroundGP - startPos).magnitude;
                if (nearestDistance > dist)
                {
                    nearestDistance = dist;
                    nearestGroundGP = groundGP;
                }
            }
        }

        return isGrounded;
    }

    #endregion

    #region 特效

    [FoldoutGroup("特效")]
    [LabelText("@\"踢特效\t\"+KickFX")]
    public FXConfig KickFX = new FXConfig();

    [FoldoutGroup("特效")]
    [LabelText("踢特效锚点")]
    public Transform KickFXPivot;

    [FoldoutGroup("特效")]
    [LabelText("@\"受伤特效\t\"+InjureFX")]
    public FXConfig InjureFX = new FXConfig();

    [FoldoutGroup("特效")]
    [LabelText("@\"生命恢复特效\t\"+HealFX")]
    public FXConfig HealFX = new FXConfig();

    [FoldoutGroup("特效")]
    [LabelText("@\"死亡特效\t\"+DieFX")]
    public FXConfig DieFX = new FXConfig();

    #endregion

    #region 手感

    [FoldoutGroup("手感")]
    [LabelText("Dash力度")]
    public float DashForce = 150f;

    [FoldoutGroup("手感")]
    [LabelText("起步速度")]
    public float Accelerate = 200f;

    protected float ThrowRadiusMin = 0.75f;

    [FoldoutGroup("手感")]
    [LabelText("踢箱子力量")]
    public float KickForce = 30;

    [FoldoutGroup("手感")]
    [LabelText("扔半径")]
    public float ThrowRadius = 15f;

    [FoldoutGroup("手感")]
    [LabelText("跳跃力")]
    public float ActiveJumpForce = 10f;

    [FoldoutGroup("手感")]
    [LabelText("坠落力")]
    public float FallForce = 0.1f;

    #endregion

    #region 推踢扔举能力

    [FoldoutGroup("推踢扔举能力")]
    [LabelText("推箱子类型")]
    [ListDrawerSettings(ListElementLabelName = "TypeName")]
    public List<TypeSelectHelper> PushableBoxList = new List<TypeSelectHelper>();

    [FoldoutGroup("推踢扔举能力")]
    [LabelText("踢箱子类型")]
    [ListDrawerSettings(ListElementLabelName = "TypeName")]
    public List<TypeSelectHelper> KickableBoxList = new List<TypeSelectHelper>();

    [FoldoutGroup("推踢扔举能力")]
    [LabelText("举箱子类型")]
    [ListDrawerSettings(ListElementLabelName = "TypeName")]
    public List<TypeSelectHelper> LiftableBoxList = new List<TypeSelectHelper>();

    [FoldoutGroup("推踢扔举能力")]
    [LabelText("扔箱子类型")]
    [ListDrawerSettings(ListElementLabelName = "TypeName")]
    public List<TypeSelectHelper> ThrowableBoxList = new List<TypeSelectHelper>();

    #endregion

    #region 死亡

    public void Reborn()
    {
        ReloadESPS(RawEntityStatPropSet);
    }

    public void ReloadESPS(EntityStatPropSet srcESPS)
    {
        EntityStatPropSet.OnRecycled();
        srcESPS.ApplyDataTo(EntityStatPropSet);
        EntityStatPropSet.Initialize(this);
        ActorBattleHelper.InGameHealthBar.Initialize(ActorBattleHelper, 100, 30);
        UIManager.Instance.GetBaseUIForm<PlayerStatHUDPanel>().Initialize();
        ActiveSkillMarkAsDestroyed = false;
        PassiveSkillMarkAsDestroyed = false;
        ActorBattleHelper.IsDestroying = false;
    }

    #endregion

    #region 冻结

    [FoldoutGroup("冻结")]
    [LabelText("@\"冻结特效\t\"+FrozeFX")]
    public FXConfig FrozeFX = new FXConfig();

    [FoldoutGroup("冻结")]
    [LabelText("@\"解冻特效\t\"+ThawFX")]
    public FXConfig ThawFX = new FXConfig();

    [SerializeReference]
    [FoldoutGroup("冻结")]
    [LabelText("冻结的箱子被动技能")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<EntityPassiveSkill> RawFrozenBoxPassiveSkills = new List<EntityPassiveSkill>(); // 干数据，禁修改

    #endregion

    private List<SmoothMove> SmoothMoves = new List<SmoothMove>();

    [ShowInInspector]
    [HideInEditorMode]
    internal bool IsInMicroWorld = false;

    public enum ActorBehaviourStates
    {
        Idle,
        Frozen,
        Walk,
        Chase,
        Attack,
        Push,
        Dash,
        Vault,
        Kick,
        Escape,
        Jump,
        SmashDown,
        InAirMoving,
    }

    [ReadOnly]
    [FoldoutGroup("状态")]
    [LabelText("行为状态")]
    public ActorBehaviourStates ActorBehaviourState = ActorBehaviourStates.Idle;

    public enum ThrowStates
    {
        None,
        Raising,
        Lifting,
        ThrowCharging,
    }

    [ReadOnly]
    [FoldoutGroup("状态")]
    [LabelText("扔技能状态")]
    public ThrowStates ThrowState = ThrowStates.None;

    public override void OnRecycled()
    {
        IsInMicroWorld = false;
        ForbidAction = true;
        if (!HasRigidbody) AddRigidbody();
        RigidBody.drag = 100f;
        RigidBody.velocity = Vector3.zero;

        ActorBehaviourState = ActorBehaviourStates.Idle;
        GraphOwner?.StopBehaviour();
        ActorAIAgent.Stop();
        CurMoveAttempt = Vector3.zero;
        LastMoveAttempt = Vector3.zero;
        CurThrowMoveAttempt = Vector3.zero;
        CurThrowPointOffset = Vector3.zero;
        CurForward = Vector3.forward;
        WorldGP = GridPos3D.Zero;
        LastWorldGP = GridPos3D.Zero;
        ThrowState = ThrowStates.None;
        ClearJumpParams();
        ThrowWhenDie();

        EntityBuffHelper.OnHelperRecycled();
        EntityFrozenHelper.OnHelperRecycled();
        EntityTriggerZoneHelper?.OnHelperRecycled();
        EntityGrindTriggerZoneHelper?.OnHelperRecycled();
        foreach (EntityFlamethrowerHelper h in EntityFlamethrowerHelpers)
        {
            h.OnHelperRecycled();
        }

        foreach (EntityLightningGeneratorHelper h in EntityLightningGeneratorHelpers)
        {
            h.OnHelperRecycled();
        }

        ActorArtHelper.OnHelperRecycled();
        ActorPushHelper.OnHelperRecycled();
        ActorFaceHelper.OnHelperRecycled();
        ActorSkinHelper.OnHelperRecycled();
        ActorLaunchArcRendererHelper.OnHelperRecycled();
        ActorBattleHelper.OnHelperRecycled();
        ActorBoxInteractHelper.OnHelperRecycled();

        UnInitActiveSkills();
        UnInitPassiveSkills();
        EntityStatPropSet.OnRecycled();

        ActorMoveColliderRoot.SetActive(false);
        SetModelSmoothMoveLerpTime(0);
        SwitchEntityOrientation(GridPosR.Orientation.Up);
        StopAllCoroutines();
        gameObject.SetActive(false);
        BattleManager.Instance.RemoveActor(this);
        base.OnRecycled();
    }

    public override void OnUsed()
    {
        gameObject.SetActive(true);
        base.OnUsed();
        EntityBuffHelper.OnHelperUsed();
        EntityFrozenHelper.OnHelperUsed();
        EntityTriggerZoneHelper?.OnHelperUsed();
        EntityGrindTriggerZoneHelper?.OnHelperUsed();
        foreach (EntityFlamethrowerHelper h in EntityFlamethrowerHelpers)
        {
            h.OnHelperUsed();
        }

        foreach (EntityLightningGeneratorHelper h in EntityLightningGeneratorHelpers)
        {
            h.OnHelperUsed();
        }

        ActorArtHelper.OnHelperUsed();
        ActorPushHelper.OnHelperUsed();
        ActorFaceHelper.OnHelperUsed();
        ActorSkinHelper.OnHelperUsed();
        ActorLaunchArcRendererHelper.OnHelperUsed();
        ActorBattleHelper.OnHelperUsed();
        ActorBoxInteractHelper.OnHelperUsed();

        ActorMoveColliderRoot.SetActive(true);
    }

    void Awake()
    {
        ActorAIAgent = new ActorAIAgent(this);
        GraphOwner = GetComponent<GraphOwner>();
        SmoothMoves = GetComponentsInChildren<SmoothMove>().ToList();
        SetModelSmoothMoveLerpTime(0);
        EntityStatPropSet = new EntityStatPropSet();
    }

    internal float DefaultSmoothMoveLerpTime = 0.02f;

    public void SetModelSmoothMoveLerpTime(float lerpTime)
    {
        if (lerpTime.Equals(0))
        {
            foreach (SmoothMove sm in SmoothMoves)
            {
                sm.enabled = false;
            }
        }
        else
        {
            foreach (SmoothMove sm in SmoothMoves)
            {
                sm.enabled = true;
                sm.SmoothTime = lerpTime;
            }
        }
    }

    public void Setup(string actorType, ActorCategory actorCategory, GridPosR.Orientation actorOrientation, uint initWorldModuleGUID)
    {
        base.Setup(initWorldModuleGUID);
        if (actorCategory == ActorCategory.Creature)
        {
            EntityTypeIndex = ConfigManager.GetTypeIndex(TypeDefineType.Enemy, actorType);
        }
        else if (actorCategory == ActorCategory.Player)
        {
            EntityTypeIndex = (ushort) ConfigManager.TypeStartIndex.Player;
            ClientGameManager.Instance.BattleMessenger.AddListener<Actor>((uint) Enum_Events.OnPlayerLoaded, OnLoaded);
        }

        ActorType = actorType;
        ActorCategory = actorCategory;
        RawEntityStatPropSet.ApplyDataTo(EntityStatPropSet);
        EntityStatPropSet.Initialize(this);
        ActorBattleHelper.Initialize();
        ActorBoxInteractHelper.Initialize();
        InitPassiveSkills();
        InitActiveSkills();

        ActorArtHelper.SetPFMoveGridSpeed(0);
        ActorArtHelper.SetIsPushing(false);

        curWorldGP = GridPos3D.GetGridPosByTrans(transform, 1);
        LastWorldGP = WorldGP;
        SwitchEntityOrientation(actorOrientation);
        ActorAIAgent.Start();

        ActorBattleHelper.OnDamaged += (damage) =>
        {
            float distance = (BattleManager.Instance.Player1.transform.position - transform.position).magnitude;
            CameraManager.Instance.FieldCamera.CameraShake(damage, distance);
        };

        ForbidAction = false;
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
            SetModelSmoothMoveLerpTime(DefaultSmoothMoveLerpTime);
        }
    }

    public void SetShown(bool shown)
    {
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (IsRecycled) return;
        if (ENABLE_ACTOR_MOVE_LOG && WorldGP != LastWorldGP) Debug.Log($"[{Time.frameCount}] [Actor] {name} Move {LastWorldGP} -> {WorldGP}");
        LastWorldGP = WorldGP;
        WorldGP = GridPos3D.GetGridPosByTrans(transform, 1);
    }

    public void TransportPlayerGridPos(GridPos3D worldGP)
    {
        SetModelSmoothMoveLerpTime(0);
        transform.position = worldGP;
        LastWorldGP = WorldGP;
        WorldGP = worldGP;
        SetModelSmoothMoveLerpTime(DefaultSmoothMoveLerpTime);
    }

    protected virtual void MoveInternal()
    {
        if (!ActiveSkillCanMove) CurMoveAttempt = Vector3.zero;
        if (!CannotAct && HasRigidbody)
        {
            if (CurMoveAttempt.magnitude > 0)
            {
                if (CurMoveAttempt.x.Equals(0)) RigidBody.velocity = new Vector3(0, RigidBody.velocity.y, RigidBody.velocity.z);
                if (CurMoveAttempt.z.Equals(0)) RigidBody.velocity = new Vector3(RigidBody.velocity.x, RigidBody.velocity.y, 0);
                if (this is EnemyActor enemyActor)
                {
                    ActorArtHelper.SetPFMoveGridSpeed(enemyActor.ActorAIAgent.NextStraightNodeCount);
                }
                else
                {
                    ActorArtHelper.SetPFMoveGridSpeed(1);
                }

                if (this is PlayerActor)
                {
                    if (!IsExecutingAirSkills())
                    {
                        ActorBehaviourState = ActorBehaviourStates.Walk;
                        ActorArtHelper.SetIsWalking(true);
                        ActorArtHelper.SetIsChasing(false);
                    }
                }
                else
                {
                    if (ActorBehaviourState == ActorBehaviourStates.Walk)
                    {
                        ActorArtHelper.SetIsWalking(true);
                        ActorArtHelper.SetIsChasing(false);
                    }
                    else if (ActorBehaviourState == ActorBehaviourStates.Chase)
                    {
                        ActorArtHelper.SetIsWalking(false);
                        ActorArtHelper.SetIsChasing(true);
                    }
                }

                RigidBody.drag = 0;
                RigidBody.mass = 1f;

                if (EntityStatPropSet.MoveSpeed.GetModifiedValue != 0)
                {
                    Vector3 velDiff = CurMoveAttempt.normalized * Time.fixedDeltaTime * Accelerate;
                    Vector3 finalVel = new Vector3(RigidBody.velocity.x + velDiff.x, 0, RigidBody.velocity.z + velDiff.z);
                    float finalSpeed = EntityStatPropSet.MoveSpeed.GetModifiedValue / 10f;
                    if (finalVel.magnitude > finalSpeed)
                    {
                        finalVel = finalVel.normalized * finalSpeed;
                    }

                    finalVel.y = RigidBody.velocity.y;
                    RigidBody.AddForce(finalVel - RigidBody.velocity, ForceMode.VelocityChange);
                }

                CurForward = CurMoveAttempt.normalized;

                ActorPushHelper.TriggerOut = true;
                bool isBoxOnFront = Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 1f, LayerManager.Instance.LayerMask_BoxIndicator);
                if (isBoxOnFront)
                {
                    Box box = hit.collider.GetComponentInParent<Box>();
                    if (box && !box.Passable)
                    {
                        ActorArtHelper.SetIsPushing(true);
                    }
                    else
                    {
                        ActorArtHelper.SetIsPushing(false);
                    }
                }
                else
                {
                    ActorArtHelper.SetIsPushing(false);
                }
            }
            else
            {
                if (!IsExecutingAirSkills())
                {
                    ActorArtHelper.SetPFMoveGridSpeed(0);
                    ActorArtHelper.SetIsWalking(false);
                    ActorArtHelper.SetIsChasing(false);
                    ActorArtHelper.SetIsPushing(false);
                    RigidBody.drag = 100f;
                    RigidBody.mass = 1f;
                    ActorPushHelper.TriggerOut = false;
                }
            }

            if (!IsExecutingAirSkills())
            {
                if (CurMoveAttempt.x.Equals(0))
                {
                    SnapToGridX();
                }

                if (CurMoveAttempt.z.Equals(0))
                {
                    SnapToGridZ();
                }
            }
        }
        else
        {
            ActorBehaviourState = ActorBehaviourStates.Idle;
            ActorArtHelper.SetPFMoveGridSpeed(0);
            ActorArtHelper.SetIsWalking(false);
            ActorArtHelper.SetIsChasing(false);
            ActorArtHelper.SetIsPushing(false);
            if (HasRigidbody)
            {
                RigidBody.drag = 0f;
                RigidBody.mass = 1f;
            }

            CurMoveAttempt = Vector3.zero;
        }

        WorldGP = transform.position.ToGridPos3D();

        // 着地判定
        IsGrounded = CheckIsGrounded(0.55f, out GridPos3D _);
        JumpingUpTick();
        InAirMovingToTargetPosTick();
        SmashingDownTick();
        if (!IsExecutingAirSkills() || (ActorBehaviourState == ActorBehaviourStates.Jump && JumpReachClimax))
        {
            if (!CannotAct && HasRigidbody)
            {
                if (IsGrounded)
                {
                    RigidBody.constraints |= RigidbodyConstraints.FreezePositionY;
                    SnapToGridY();
                }
                else
                {
                    RigidBody.constraints &= ~RigidbodyConstraints.FreezePositionY;
                    RigidBody.drag = 0f;
                    RigidBody.AddForce(Vector3.down * FallForce, ForceMode.VelocityChange);
                }
            }
        }

        if (ActorBattleHelper.IsDestroying)
        {
            if (HasRigidbody) RigidBody.constraints |= RigidbodyConstraints.FreezePositionY;
            SnapToGridY();
        }

        LastMoveAttempt = CurMoveAttempt;
    }

    private void AddRigidbody()
    {
        if (!HasRigidbody && RigidBody == null)
        {
            HasRigidbody = true;
            RigidBody = gameObject.AddComponent<Rigidbody>();
            RigidBody.velocity = Vector3.zero;
            RigidBody.mass = 1f;
            RigidBody.drag = 0f;
            RigidBody.angularDrag = 20f;
            RigidBody.useGravity = true;
            RigidBody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            RigidBody.interpolation = RigidbodyInterpolation.Interpolate;
            RigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }

    private void RemoveRigidbody()
    {
        if (HasRigidbody && RigidBody != null)
        {
            Destroy(RigidBody);
            HasRigidbody = false;
        }
    }

    public void SnapToGrid()
    {
        SnapToGridX();
        SnapToGridZ();
    }

    public void SnapToGridX()
    {
        transform.position = new Vector3(WorldGP.x, transform.position.y, transform.position.z);
    }

    public void SnapToGridY()
    {
        transform.position = new Vector3(transform.position.x, WorldGP.y, transform.position.z);
    }

    public void SnapToGridZ()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, WorldGP.z);
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

    public void VaultOrDash(bool directionKeyDown)
    {
        if (ThrowState != ThrowStates.None) return;
        Ray ray = new Ray(transform.position - transform.forward * 0.49f, transform.forward);
        //Debug.DrawRay(ray.origin, ray.direction, Color.red, 0.3f);
        if (Physics.Raycast(ray, out RaycastHit hit, 1.49f, LayerManager.Instance.LayerMask_BoxIndicator, QueryTriggerInteraction.Collide))
        {
            Box box = hit.collider.gameObject.GetComponentInParent<Box>();
            if (box && !box.Passable)
            {
                Vault();
            }
            else
            {
                if (directionKeyDown) Dash();
            }
        }
        else
        {
            if (directionKeyDown) Dash();
        }
    }

    private void Dash()
    {
        if (CannotAct) return;
        if (EntityStatPropSet.ActionPoint.Value >= EntityStatPropSet.DashConsumeActionPoint.GetModifiedValue)
        {
            EntityStatPropSet.ActionPoint.SetValue(EntityStatPropSet.ActionPoint.Value - EntityStatPropSet.DashConsumeActionPoint.GetModifiedValue, "Dash");
            if (IsFrozen)
            {
                EntityStatPropSet.FrozenValue.SetValue(EntityStatPropSet.FrozenValue.Value - 200, "Dash");
            }
            else
            {
                ActorArtHelper.Dash();
                RigidBody.AddForce(CurForward * DashForce, ForceMode.VelocityChange);
            }
        }
        else
        {
            UIManager.Instance.GetBaseUIForm<PlayerStatHUDPanel>().PlayerStatHUDs_Player[0].OnActionLowWarning();
        }
    }

    private void Vault()
    {
        if (CannotAct) return;
        if (EntityStatPropSet.ActionPoint.Value >= EntityStatPropSet.VaultConsumeActionPoint.GetModifiedValue)
        {
            if (IsFrozen)
            {
                EntityStatPropSet.FrozenValue.SetValue(EntityStatPropSet.FrozenValue.Value - 200, "Vault");
            }
            else
            {
                ActorArtHelper.Vault();
            }
        }
        else
        {
            UIManager.Instance.GetBaseUIForm<PlayerStatHUDPanel>().PlayerStatHUDs_Player[0].OnActionLowWarning();
        }
    }

    public void Kick()
    {
        if (CannotAct) return;
        if (EntityStatPropSet.ActionPoint.Value >= EntityStatPropSet.KickConsumeActionPoint.GetModifiedValue)
        {
            Ray ray = new Ray(transform.position - transform.forward * 0.49f, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 1.49f, LayerManager.Instance.LayerMask_BoxIndicator, QueryTriggerInteraction.Collide))
            {
                Box box = hit.collider.gameObject.GetComponentInParent<Box>();
                if (box && box.Kickable && ActorBoxInteractHelper.CanInteract(InteractSkillType.Kick, box.EntityTypeIndex))
                {
                    ActorArtHelper.Kick();
                }
            }
        }
        else
        {
            UIManager.Instance.GetBaseUIForm<PlayerStatHUDPanel>().PlayerStatHUDs_Player[0].OnActionLowWarning();
        }
    }

    public void KickBox()
    {
        Ray ray = new Ray(transform.position - transform.forward * 0.49f, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 1.49f, LayerManager.Instance.LayerMask_BoxIndicator, QueryTriggerInteraction.Collide))
        {
            Box box = hit.collider.gameObject.GetComponentInParent<Box>();
            if (box && box.Kickable && ActorBoxInteractHelper.CanInteract(InteractSkillType.Kick, box.EntityTypeIndex))
            {
                EntityStatPropSet.ActionPoint.SetValue(EntityStatPropSet.ActionPoint.Value - EntityStatPropSet.KickConsumeActionPoint.GetModifiedValue, "Kick");
                box.Kick(CurForward, KickForce, this);
                FX kickFX = FXManager.Instance.PlayFX(KickFX, KickFXPivot.position);
            }
        }
    }

    public void SwapBox()
    {
        if (CannotAct) return;
        Ray ray = new Ray(transform.position - transform.forward * 0.49f, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 1.49f, LayerManager.Instance.LayerMask_BoxIndicator, QueryTriggerInteraction.Collide))
        {
            Box box = hit.collider.gameObject.GetComponentInParent<Box>();
            GridPos3D actorSwapBoxMoveAttempt = (hit.collider.transform.position - transform.position).ToGridPos3D().Normalized();
            if (box && box.Pushable && ActorBoxInteractHelper.CanInteract(InteractSkillType.Push, box.EntityTypeIndex))
            {
                box.ForceStopWhenSwapBox(this);

                Vector3 boxIndicatorPos = hit.collider.transform.position;
                GridPos3D boxIndicatorGP_offset = (boxIndicatorPos - box.transform.position).ToGridPos3D();
                GridPos3D boxIndicatorGP = boxIndicatorGP_offset + box.WorldGP;

                // 如果角色面朝方向Box的厚度大于一格，则无法swap
                GridPos3D boxIndicatorGP_behind = boxIndicatorGP + actorSwapBoxMoveAttempt;
                foreach (GridPos3D offset in box.GetEntityOccupationGPs_Rotated())
                {
                    if (offset == boxIndicatorGP_behind - box.WorldGP) return;
                }

                EntityStatPropSet.ActionPoint.SetValue(EntityStatPropSet.ActionPoint.Value - EntityStatPropSet.VaultConsumeActionPoint.GetModifiedValue, "SwapBox"); // 消耗行动力

                GridPos3D boxWorldGP_before = box.WorldGP;
                GridPos3D boxWorldGP_after = LastWorldGP - boxIndicatorGP + box.WorldGP;
                if (WorldManager.Instance.CurrentWorld.MoveBoxColumn(box.WorldGP, -actorSwapBoxMoveAttempt, Box.States.BeingPushed, false, true, GUID))
                {
                    if (Box.ENABLE_BOX_MOVE_LOG) Debug.Log($"[{Time.frameCount}] [Box] {box.name} SwapBox {boxWorldGP_before} -> {box.WorldGP}");
                    transform.position = boxIndicatorGP;
                    LastWorldGP = WorldGP;
                    WorldGP = GridPos3D.GetGridPosByTrans(transform, 1);
                    if (ENABLE_ACTOR_MOVE_LOG) Debug.Log($"[{Time.frameCount}] [Actor] {name} Swap {LastWorldGP} -> {WorldGP}");
                }
                else
                {
                    if (Box.ENABLE_BOX_MOVE_LOG) Debug.Log($"[{Time.frameCount}] [Box] {box.name} SwapBox MoveFailed {boxWorldGP_before} -> {boxWorldGP_after}");
                    GridPos3D actorTargetGP = boxIndicatorGP + actorSwapBoxMoveAttempt;
                    Box targetBox = WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(actorTargetGP, out WorldModule _, out GridPos3D _, false);
                    if (targetBox == null || targetBox.Passable)
                    {
                        transform.position = boxIndicatorGP + actorSwapBoxMoveAttempt;
                        LastWorldGP = WorldGP;
                        WorldGP = GridPos3D.GetGridPosByTrans(transform, 1);
                        if (ENABLE_ACTOR_MOVE_LOG) Debug.Log($"[{Time.frameCount}] [Actor] {name} SwapFailed MoveSuc {LastWorldGP} -> {WorldGP}");
                    }
                    else
                    {
                        if (ENABLE_ACTOR_MOVE_LOG) Debug.Log($"[{Time.frameCount}] [Actor] {name} SwapFailed MoveFailed blocked by {targetBox.name} {LastWorldGP} -> {WorldGP}");
                    }
                }

                // todo kicking box的swap如何兼容
            }
            else
            {
                if (Box.ENABLE_BOX_MOVE_LOG) Debug.Log($"[{Time.frameCount}] [Box] {box.name} SwapBoxFailed");
            }
        }
    }

    public void Lift()
    {
        if (CurrentLiftBox) return;
        if (CannotAct) return;
        Ray ray = new Ray(transform.position - transform.forward * 0.49f, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 1.49f, LayerManager.Instance.LayerMask_BoxIndicator, QueryTriggerInteraction.Collide))
        {
            Box box = hit.collider.gameObject.GetComponentInParent<Box>();
            if (box && box.Liftable && ActorBoxInteractHelper.CanInteract(InteractSkillType.Lift, box.EntityTypeIndex))
            {
                if (box.BeingLift(this))
                {
                    CurrentLiftBox = box;
                    ThrowState = ThrowStates.Raising;
                    ActorFaceHelper.FacingBoxList.Remove(box);
                    box.transform.parent = LiftBoxPivot.transform.parent;
                    box.transform.DOLocalMove(LiftBoxPivot.transform.localPosition, 0.2f).OnComplete(() =>
                    {
                        if (box.Consumable)
                        {
                            box.State = Box.States.Static;
                            ThrowState = ThrowStates.None;
                            box.LiftThenConsume();
                        }
                        else
                        {
                            box.State = Box.States.Lifted;
                            ThrowState = ThrowStates.Lifting;
                        }
                    });

                    if (box.Consumable)
                    {
                        ThrowState = ThrowStates.None;
                    }
                }
            }
        }
    }

    public void ThrowCharge()
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
            ActorLaunchArcRendererHelper.InitializeByOffset(CurThrowPointOffset, 45, 2, 3f);
        }
    }

    public void ThrowOrPut()
    {
        if (CannotAct) return;
        if (CurrentLiftBox && ThrowState == ThrowStates.ThrowCharging)
        {
            if (ActorBoxInteractHelper.CanInteract(InteractSkillType.Throw, CurrentLiftBox.EntityTypeIndex))
            {
                Throw();
            }
            else
            {
                Put();
            }
        }
    }

    public void Throw()
    {
        if (CannotAct) return;
        if (CurrentLiftBox && CurrentLiftBox.Throwable && ThrowState == ThrowStates.ThrowCharging)
        {
            float velocity = ActorLaunchArcRendererHelper.CalculateVelocityByOffset(CurThrowPointOffset, 45);
            ThrowState = ThrowStates.None;
            Vector3 throwVel = (CurThrowPointOffset.normalized + Vector3.up) * velocity;
            CurrentLiftBox.Throw(throwVel, velocity, this);
            CurrentLiftBox = null;
        }
    }

    public void Put()
    {
        if (CannotAct) return;
        if (CurrentLiftBox && ThrowState == ThrowStates.Lifting || ThrowState == ThrowStates.ThrowCharging)
        {
            float velocity = ActorLaunchArcRendererHelper.CalculateVelocityByOffset(CurThrowPointOffset, 45);
            ThrowState = ThrowStates.None;
            Vector3 throwVel = (CurThrowPointOffset.normalized + Vector3.up) * velocity;
            CurrentLiftBox.Put(throwVel, velocity, this);
            CurrentLiftBox = null;
        }
    }

    private void ThrowWhenDie()
    {
        if (CurrentLiftBox)
        {
            ThrowState = ThrowStates.None;
            Vector3 throwVel = Vector3.up * 3;
            if (CurrentLiftBox.Throwable)
            {
                CurrentLiftBox.Throw(throwVel, 3, this);
            }
            else
            {
                CurrentLiftBox.Put(throwVel, 3, this);
            }

            CurrentLiftBox = null;
        }
    }

    #region Jump

    private void ClearJumpParams()
    {
        JumpHeight = 0;
        JumpStartWorldGP = GridPos3D.Zero;
        CurrentJumpForce = 0;
        KeepAddingJumpForce = false;
        JumpReachClimax = false;
        SmashDownTargetPos = Vector3.zero;
        SmashForce = 0;
        SmashReachTarget = false;
        InAirMoveTargetPos = Vector3.zero;
        InAirMoveSpeed = 0;
    }

    [FoldoutGroup("跳跃")]
    [LabelText("跳跃目标高度")]
    public int JumpHeight = 0;

    [FoldoutGroup("跳跃")]
    [LabelText("起跳坐标")]
    [DisplayAsString]
    public GridPos3D JumpStartWorldGP = GridPos3D.Zero;

    [FoldoutGroup("跳跃")]
    [LabelText("起跳力度")]
    [DisplayAsString]
    public float CurrentJumpForce = 0f;

    [FoldoutGroup("跳跃")]
    [LabelText("持续施加起跳力")]
    public bool KeepAddingJumpForce = false;

    [FoldoutGroup("跳跃")]
    [LabelText("跳跃是否抵达最高点")]
    public bool JumpReachClimax = false;

    [FoldoutGroup("跳跃")]
    [LabelText("下砸目标坐标")]
    [DisplayAsString]
    public Vector3 SmashDownTargetPos = Vector3.zero;

    [FoldoutGroup("跳跃")]
    [LabelText("下砸力度")]
    [DisplayAsString]
    public float SmashForce = 0f;

    [FoldoutGroup("跳跃")]
    [LabelText("下砸是否抵达目标")]
    public bool SmashReachTarget = false;

    [FoldoutGroup("跳跃")]
    [LabelText("悬空追击目标坐标")]
    [DisplayAsString]
    public Vector3 InAirMoveTargetPos = Vector3.zero;

    [FoldoutGroup("跳跃")]
    [LabelText("悬空追击移速")]
    [DisplayAsString]
    public float InAirMoveSpeed = 0f;

    public bool IsExecutingAirSkills()
    {
        return ActorBehaviourState == ActorBehaviourStates.Jump || ActorBehaviourState == ActorBehaviourStates.InAirMoving || ActorBehaviourState == ActorBehaviourStates.SmashDown;
    }

    public void SetJumpUpTargetHeight(float jumpForce, int jumpHeight, bool keepAddingForce)
    {
        if (ActorBehaviourState == ActorBehaviourStates.Jump || ActorBehaviourState == ActorBehaviourStates.InAirMoving || ActorBehaviourState == ActorBehaviourStates.SmashDown) return;
        if (CannotAct) return;
        if (HasRigidbody)
        {
            JumpHeight = jumpHeight;
            JumpStartWorldGP = WorldGP;
            CurrentJumpForce = jumpForce;
            KeepAddingJumpForce = keepAddingForce;
            ActorBehaviourState = ActorBehaviourStates.Jump;
            JumpReachClimax = false;
            RigidBody.constraints &= ~RigidbodyConstraints.FreezePositionY;
            RigidBody.drag = 0;
            RigidBody.velocity = Vector3.zero;
            RigidBody.AddForce(Vector3.up * CurrentJumpForce, ForceMode.VelocityChange);
        }
    }

    public void JumpingUpTick()
    {
        if (ActorBehaviourState == ActorBehaviourStates.Jump)
        {
            if (CannotAct) return;
            if (HasRigidbody)
            {
                if (KeepAddingJumpForce && !JumpReachClimax)
                {
                    RigidBody.AddForce(Vector3.up * (CurrentJumpForce - RigidBody.velocity.y), ForceMode.VelocityChange);
                }

                if ((WorldGP - JumpStartWorldGP).y >= JumpHeight)
                {
                    JumpReachClimax = true;
                }
                else
                {
                    if (HasRigidbody)
                    {
                        if (RigidBody.velocity.y < -1f)
                        {
                            JumpReachClimax = true;
                        }
                    }
                    else
                    {
                        JumpReachClimax = true;
                    }
                }

                if (JumpReachClimax && IsGrounded) ActorBehaviourState = ActorBehaviourStates.Idle;
            }
        }
    }

    public void SetSmashDownTargetPos(Vector3 targetPos, float smashForce)
    {
        if (ActorBehaviourState == ActorBehaviourStates.Jump || ActorBehaviourState == ActorBehaviourStates.InAirMoving)
        {
            if (CannotAct) return;
            if (HasRigidbody)
            {
                SmashDownTargetPos = targetPos;
                SmashForce = smashForce;
                ActorBehaviourState = ActorBehaviourStates.SmashDown;
                JumpReachClimax = true;
                SmashReachTarget = false;
                RigidBody.constraints &= ~RigidbodyConstraints.FreezePositionY;
                RigidBody.drag = 0;
                RigidBody.velocity = Vector3.zero;
            }
        }
    }

    public void SmashingDownTick()
    {
        if (ActorBehaviourState != ActorBehaviourStates.SmashDown) return;
        if (CannotAct) return;
        if (HasRigidbody)
        {
            if (transform.position.y > SmashDownTargetPos.y + 1f)
            {
                RigidBody.AddForce(Vector3.up * (-SmashForce - RigidBody.velocity.y), ForceMode.VelocityChange);
            }

            if (IsGrounded) ActorBehaviourState = ActorBehaviourStates.Idle;
        }
    }

    public void InAirSetMoveTargetPos(Vector3 targetPos, float moveSpeed)
    {
        if (ActorBehaviourState == ActorBehaviourStates.Jump || ActorBehaviourState == ActorBehaviourStates.InAirMoving)
        {
            if (CannotAct) return;
            if (HasRigidbody)
            {
                InAirMoveTargetPos = targetPos;
                InAirMoveSpeed = moveSpeed;
                ActorBehaviourState = ActorBehaviourStates.InAirMoving;
                RigidBody.velocity = Vector3.zero;
            }
        }
    }

    public void InAirMovingToTargetPosTick()
    {
        if (ActorBehaviourState == ActorBehaviourStates.InAirMoving)
        {
            if (HasRigidbody)
            {
                Vector3 diff = InAirMoveTargetPos - transform.position;
                RigidBody.velocity = Vector3.zero;
                RigidBody.AddForce(diff.normalized * InAirMoveSpeed, ForceMode.VelocityChange);
                if (RigidBody.velocity.magnitude > InAirMoveSpeed)
                {
                    RigidBody.velocity = RigidBody.velocity.normalized * InAirMoveSpeed;
                }
            }
        }
    }

    #endregion

    #endregion
}
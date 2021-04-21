using System;
using System.Collections;
using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;

#if UNITY_EDITOR

#endif

public partial class Box : Entity
{
    public static bool ENABLE_BOX_MOVE_LOG = false;

    internal bool hasRigidbody;
    internal Rigidbody Rigidbody;

    internal uint LastInteractActorGUID;

    internal Actor LastInteractActor
    {
        get
        {
            if (BattleManager.Instance.ActorDict.TryGetValue(LastInteractActorGUID, out Actor actor))
            {
                return actor;
            }

            return null;
        }
    }

    internal override EntityArtHelper EntityArtHelper => BoxArtHelper;
    internal override EntityWwiseHelper EntityWwiseHelper => BoxWwiseHelper;
    internal override EntityModelHelper EntityModelHelper => BoxModelHelper;
    internal override EntityIndicatorHelper EntityIndicatorHelper => BoxIndicatorHelper;
    internal override EntityBuffHelper EntityBuffHelper => BoxBuffHelper;
    internal override EntityFrozenHelper EntityFrozenHelper => BoxFrozenHelper;
    internal override EntityTriggerZoneHelper EntityTriggerZoneHelper => BoxTriggerZoneHelper;
    internal override EntityCollectHelper EntityCollectHelper => BoxCollectHelper;
    internal override EntityGrindTriggerZoneHelper EntityGrindTriggerZoneHelper => BoxGrindTriggerZoneHelper;
    internal override List<EntityFlamethrowerHelper> EntityFlamethrowerHelpers => BoxFlamethrowerHelpers;
    internal override List<EntityLightningGeneratorHelper> EntityLightningGeneratorHelpers => BoxLightningGeneratorHelpers;

    [FoldoutGroup("组件")]
    public BoxArtHelper BoxArtHelper;

    [FoldoutGroup("组件")]
    public EntityWwiseHelper BoxWwiseHelper;

    [FoldoutGroup("组件")]
    public EntityModelHelper BoxModelHelper;

    [FoldoutGroup("组件")]
    public EntityIndicatorHelper BoxIndicatorHelper;

    [FoldoutGroup("组件")]
    [SerializeField]
    private EntityBuffHelper BoxBuffHelper;

    [FoldoutGroup("组件")]
    [SerializeField]
    private BoxFrozenHelper BoxFrozenHelper;

    [FoldoutGroup("组件")]
    [SerializeField]
    private EntityTriggerZoneHelper BoxTriggerZoneHelper;

    [FoldoutGroup("组件")]
    [SerializeField]
    private EntityCollectHelper BoxCollectHelper;

    [FoldoutGroup("组件")]
    [SerializeField]
    private EntityGrindTriggerZoneHelper BoxGrindTriggerZoneHelper;

    [FoldoutGroup("组件")]
    [SerializeField]
    private List<EntityFlamethrowerHelper> BoxFlamethrowerHelpers;

    [FoldoutGroup("组件")]
    [SerializeField]
    private List<EntityLightningGeneratorHelper> BoxLightningGeneratorHelpers;

    internal BoxEffectHelper BoxEffectHelper;

    [FoldoutGroup("组件")]
    public BoxColliderHelper BoxColliderHelper;

    [FoldoutGroup("组件")]
    public DoorBoxHelper DoorBoxHelper;

    [FoldoutGroup("组件")]
    public BoxSkinHelper BoxSkinHelper;

    [FoldoutGroup("组件")]
    public BoxIconSpriteHelper BoxIconSpriteHelper;

    [FoldoutGroup("组件")]
    public BoxFrozenBoxHelper BoxFrozenBoxHelper;

    [FoldoutGroup("组件")]
    public BoxMarchingTextureHelper BoxMarchingTextureHelper;

    internal Actor FrozenActor
    {
        get { return BoxFrozenBoxHelper != null ? BoxFrozenBoxHelper.FrozenActor : null; }
        set
        {
            if (BoxFrozenBoxHelper != null)
            {
                BoxFrozenBoxHelper.FrozenActor = value;
            }
        }
    }

    [FoldoutGroup("组件")]
    public SmoothMove BoxIconSpriteSmoothMove;

    [FoldoutGroup("组件")]
    public SmoothMove BoxModelSmoothMove;

    internal bool ArtOnly;

    void Awake()
    {
        EntityStatPropSet = new EntityStatPropSet();
    }

    public override void OnUsed()
    {
        gameObject.SetActive(true);
        LastInteractActorGUID = 0;
        ArtOnly = true;

        EntityArtHelper?.OnHelperUsed();
        EntityWwiseHelper.OnHelperUsed();
        EntityModelHelper.OnHelperUsed();
        EntityIndicatorHelper.OnHelperUsed();
        EntityBuffHelper.OnHelperUsed();
        EntityFrozenHelper.OnHelperUsed();
        EntityTriggerZoneHelper?.OnHelperUsed();
        EntityCollectHelper?.OnHelperUsed();
        EntityGrindTriggerZoneHelper?.OnHelperUsed();
        foreach (EntityFlamethrowerHelper h in EntityFlamethrowerHelpers)
        {
            h.OnHelperUsed();
        }

        foreach (EntityLightningGeneratorHelper h in EntityLightningGeneratorHelpers)
        {
            h.OnHelperUsed();
        }

        BoxColliderHelper.OnBoxUsed();
        DoorBoxHelper?.OnHelperUsed();
        BoxSkinHelper?.OnHelperUsed();
        BoxIconSpriteHelper.OnHelperUsed();
        BoxFrozenBoxHelper?.OnHelperUsed();
        BoxMarchingTextureHelper?.OnHelperUsed();
        base.OnUsed();
    }

    public override void OnRecycled()
    {
        ArtOnly = true;
        WorldModule = null;
        WorldGP = GridPos3D.Zero;
        LastWorldGP = GridPos3D.Zero;
        worldGP_WhenKicked = GridPos3D.Zero;
        LastState = States.Static;
        State = States.Static;
        IsDestroying = false;

        EntityArtHelper?.OnHelperRecycled();
        EntityWwiseHelper.OnHelperRecycled();
        EntityModelHelper.OnHelperRecycled();
        EntityIndicatorHelper.OnHelperRecycled();
        EntityBuffHelper.OnHelperRecycled();
        EntityFrozenHelper.OnHelperRecycled();
        EntityTriggerZoneHelper?.OnHelperRecycled();
        EntityCollectHelper?.OnHelperRecycled();
        EntityGrindTriggerZoneHelper?.OnHelperRecycled();
        foreach (EntityFlamethrowerHelper h in EntityFlamethrowerHelpers)
        {
            h.OnHelperRecycled();
        }

        foreach (EntityLightningGeneratorHelper h in EntityLightningGeneratorHelpers)
        {
            h.OnHelperRecycled();
        }

        BoxEffectHelper?.OnBoxPoolRecycled();
        BoxEffectHelper = null;
        BoxColliderHelper.OnBoxPoolRecycled();
        DoorBoxHelper?.OnHelperRecycled();
        BoxSkinHelper?.OnHelperRecycled();
        BoxIconSpriteHelper?.OnHelperRecycled();
        BoxFrozenBoxHelper?.OnHelperRecycled();
        BoxMarchingTextureHelper?.OnHelperRecycled();

        UnInitPassiveSkills();
        UnInitActiveSkills();

        transform.DOPause();
        EntityModelHelper.transform.DOPause();
        EntityModelHelper.transform.localPosition = Vector3.zero;
        EntityModelHelper.transform.localScale = Vector3.one;
        alreadyCollidedActorSet.Clear();
        if (hasRigidbody)
        {
            Destroy(Rigidbody);
            hasRigidbody = false;
        }

        if (LastInteractActor.IsNotNullAndAlive() && LastInteractActor.CurrentLiftBox == this)
        {
            LastInteractActor.ThrowState = Actor.ThrowStates.None;
            LastInteractActor.CurrentLiftBox = null;
            LastInteractActorGUID = 0;
        }

        EntityStatPropSet.OnRecycled();
        MarkedAsMergedSourceBox = false;

        CurrentKickGlobalAxis = KickAxis.None;
        CurrentKickLocalAxis = KickAxis.None;
        CurrentMoveGlobalPlanerDir = GridPos3D.Zero;

        SwitchEntityOrientation(GridPosR.Orientation.Up);
        gameObject.SetActive(false);

        base.OnRecycled();
    }

    [FoldoutGroup("属性")]
    [LabelText("箱子特性")]
    [AssetsOnly]
    public BoxFeature BoxFeature;

    #region 旋转朝向

    internal override void SwitchEntityOrientation(GridPosR.Orientation boxOrientation)
    {
        base.SwitchEntityOrientation(boxOrientation);
        GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, boxOrientation), BoxColliderHelper.transform, 1);
        GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, boxOrientation), EntityModelHelper.transform, 1);
        GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, boxOrientation), BoxFrozenHelper.FrozeModelRoot.transform, 1);
        GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, boxOrientation), BoxIndicatorHelper.transform, 1);
        if (EntityTriggerZoneHelper) GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, boxOrientation), EntityTriggerZoneHelper.transform, 1);
        if (EntityGrindTriggerZoneHelper) GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, boxOrientation), EntityGrindTriggerZoneHelper.transform, 1);
    }

    #endregion

    #region 占位

    public override List<GridPos3D> GetEntityOccupationGPs_Rotated()
    {
        if (BoxFrozenBoxHelper != null)
        {
            return BoxFrozenBoxHelper.FrozenBoxOccupation;
        }

        return base.GetEntityOccupationGPs_Rotated();
    }

    #endregion

    #region 手感  静态属性

    [ReadOnly]
    [AssetsOnly]
    [ShowInInspector]
    [ShowIf("Interactable")]
    [FoldoutGroup("属性")]
    [LabelText("重量")]
    private const float Weight = 0.7f;

    public float FinalWeight => Weight * ConfigManager.BoxWeightFactor_Cheat;

    /// <summary>
    /// 扔箱子落地Drag
    /// </summary>
    internal const float Throw_Drag = 10f;

    /// <summary>
    /// 踢箱子摩阻力
    /// </summary>
    internal const float Dynamic_Drag = 0.5f;

    #endregion

    #region 各向异性

    [SerializeField]
    [FoldoutGroup("属性")]
    [LabelText("X轴(Local)踢出速度倍率")]
    private float KickForce_X = 1f;

    [SerializeField]
    [FoldoutGroup("属性")]
    [LabelText("Z轴(Local)踢出速度倍率")]
    private float KickForce_Z = 1f;

    [SerializeField]
    [FoldoutGroup("属性")]
    [LabelText("X轴(Local)击退力")]
    private float KickRepelForce_X = 10f;

    [SerializeField]
    [FoldoutGroup("属性")]
    [LabelText("Z轴(Local)击退力")]
    private float KickRepelForce_Z = 10f;

    [SerializeField]
    [FoldoutGroup("属性")]
    [LabelText("X轴(Local)碾压")]
    [SerializeReference]
    private bool Grind_X;

    [SerializeField]
    [FoldoutGroup("属性")]
    [LabelText("Z轴(Local)碾压")]
    [SerializeReference]
    private bool Grind_Z;

    #endregion

    [SerializeField]
    [FoldoutGroup("属性")]
    [LabelText("死亡延迟")]
    private float DeleteDelay = 0;

    [SerializeField]
    [FoldoutGroup("属性")]
    [LabelText("合并延迟")]
    private float MergeDelay = 0;

    [SerializeField]
    [FoldoutGroup("属性")]
    [LabelText("是否永远朝向相机")]
    private bool FaceToCameraForever = false;

    #region 合成

    [FoldoutGroup("合成")]
    [HideLabel]
    public BoxMergeConfig BoxMergeConfig;

    #endregion

    [AssetsOnly]
    [SerializeField]
    [FoldoutGroup("特效")]
    [LabelText("@\"撞击特效\t\"+CollideFX")]
    public FXConfig CollideFX = new FXConfig();

    [AssetsOnly]
    [SerializeField]
    [FoldoutGroup("特效")]
    [LabelText("@\"死亡特效\t\"+DestroyFX")]
    public FXConfig DestroyFX = new FXConfig();

    [FoldoutGroup("特效")]
    [LabelText("@\"解冻特效\t\"+ThawFX")]
    public FXConfig ThawFX = new FXConfig();

    [FoldoutGroup("特效")]
    [LabelText("@\"冻结特效\t\"+FrozeFX")]
    public FXConfig FrozeFX = new FXConfig();

    [FoldoutGroup("特效")]
    [LabelText("@\"合成特效\t\"+MergeFX")]
    public FXConfig MergeFX = new FXConfig();

    [FoldoutGroup("特效")]
    [LabelText("@\"合成后特效\t\"+MergedFX")]
    public FXConfig MergedFX = new FXConfig();

    public string BoxTypeName => ConfigManager.GetTypeName(TypeDefineType.Box, EntityTypeIndex);

    // 获得支撑此Box的底部Box
    public HashSet<Box> GetBeneathBoxes()
    {
        return WorldManager.Instance.CurrentWorld.GetBeneathBoxes(this);
    }

    public override GridPos3D WorldGP { get; set; }

    private GridPos3D worldGP_WhenKicked = GridPos3D.Zero;

    internal bool IsInGridSystem;
    internal bool MarkedAsMergedSourceBox;

    [HideInEditorMode]
    public WorldModule WorldModule;

    private HashSet<Actor> alreadyCollidedActorSet = new HashSet<Actor>();

    public enum States
    {
        Static,
        BeingPushed,
        BeingKicked,
        BeingKickedToGrind,
        BeingLift,
        Lifted,
        Flying,
        Putting,
        DroppingFromEntity,
        Dropping,
        DroppingFromAir,
    }

    public enum KickAxis
    {
        None,
        X,
        Z
    }

    public enum LerpType
    {
        Push,
        Kick,
        KickToGrind,
        Throw,
        Put,
        Drop,
        DropFromEntity,
        DropFromAir,
        Create,
    }

    [HideInPrefabAssets]
    [ReadOnly]
    [ShowInInspector]
    [FoldoutGroup("状态")]
    [LabelText("上一个移动状态")]
    private States LastState = States.Static;

    private States state = States.Static;

    private KickAxis CurrentKickLocalAxis = KickAxis.X;
    private KickAxis CurrentKickGlobalAxis = KickAxis.X;
    private GridPos3D CurrentMoveGlobalPlanerDir = GridPos3D.Zero;

    [HideInPrefabAssets]
    [ReadOnly]
    [ShowInInspector]
    [FoldoutGroup("状态")]
    [LabelText("移动状态")]
    public States State
    {
        get { return state; }
        set
        {
            if (state != value)
            {
                LastState = state;
                state = value;
            }
        }
    }

    protected virtual void Start()
    {
    }

    private void OnPlayerInteractSkillChanged(InteractSkillType interactSkillType, ushort boxTypeIndex)
    {
        if (boxTypeIndex == EntityTypeIndex)
        {
            if (BoxSkinHelper)
            {
                if (interactSkillType.HasFlag(InteractSkillType.Kick))
                {
                    BoxSkinHelper.SwitchBoxModelType(BoxModelType.Rounded);
                }
                else
                {
                    BoxSkinHelper.SwitchBoxModelType(BoxModelType.Normal);
                }
            }
        }
    }

    public void Setup(EntityData entityData, uint initWorldModuleGUID)
    {
        base.Setup(initWorldModuleGUID);
        if (IsHidden) BoxModelHelper.gameObject.SetActive(false);
        EntityTypeIndex = entityData.EntityTypeIndex;

        InitPassiveSkills();

        RawEntityStatPropSet.ApplyDataTo(EntityStatPropSet);
        EntityStatPropSet.Initialize(this);

        SwitchEntityOrientation(entityData.EntityOrientation);

        if (BattleManager.Instance.Player1) OnPlayerInteractSkillChanged(BattleManager.Instance.Player1.ActorBoxInteractHelper.GetInteractSkillType(EntityTypeIndex), EntityTypeIndex);
        ApplyEntityExtraSerializeData(entityData.RawEntityExtraSerializeData);
    }

    private void SetModelSmoothMoveLerpTime(float lerpTime)
    {
        if (lerpTime.Equals(0))
        {
            BoxIconSpriteSmoothMove.enabled = false;
            BoxModelSmoothMove.enabled = false;
            EntityFrozenHelper.IceBlockSmoothMove.enabled = false;
            if (BoxFrozenBoxHelper != null && BoxFrozenBoxHelper.FrozenActor != null)
            {
                BoxFrozenBoxHelper.FrozenActor.ActorFrozenHelper.IceBlockSmoothMove.enabled = false;
                BoxFrozenBoxHelper.FrozenActor.SetModelSmoothMoveLerpTime(0f);
            }
        }
        else
        {
            BoxIconSpriteSmoothMove.enabled = true;
            BoxIconSpriteSmoothMove.SmoothTime = lerpTime;
            BoxModelSmoothMove.enabled = true;
            BoxModelSmoothMove.SmoothTime = lerpTime;
            EntityFrozenHelper.IceBlockSmoothMove.enabled = true;
            EntityFrozenHelper.IceBlockSmoothMove.SmoothTime = lerpTime;
            if (BoxFrozenBoxHelper != null && BoxFrozenBoxHelper.FrozenActor != null)
            {
                BoxFrozenBoxHelper.FrozenActor.EntityFrozenHelper.IceBlockSmoothMove.enabled = true;
                BoxFrozenBoxHelper.FrozenActor.EntityFrozenHelper.IceBlockSmoothMove.SmoothTime = lerpTime;
                BoxFrozenBoxHelper.FrozenActor.SetModelSmoothMoveLerpTime(lerpTime);
            }
        }
    }

    public void Initialize(GridPos3D worldGridPos3D, WorldModule module, float lerpTime, bool artOnly, LerpType lerpType, bool needLerpModel = false, bool needCheckDrop = true)
    {
        //name = $"{BoxTypeName}_{worldGridPos3D}";

        if (ENABLE_BOX_MOVE_LOG) Debug.Log($"[{Time.frameCount}] [Box] {name} Init LerpType:{lerpType} TargetGP:{worldGridPos3D}");
        SetModelSmoothMoveLerpTime(0);
        ArtOnly = artOnly;
        LastInteractActorGUID = 0;
        LastWorldGP = WorldGP;
        WorldModule = module;
        WorldGP = worldGridPos3D;
        LocalGP = module.WorldGPToLocalGP(worldGridPos3D);
        transform.parent = module.WorldModuleBoxRoot;
        BoxColliderHelper.Initialize(Passable, artOnly, BoxFeature.HasFlag(BoxFeature.IsGround), lerpType == LerpType.Drop, lerpTime > 0);
        if (lerpTime > 0)
        {
            if (lerpType == LerpType.Push)
            {
                BoxColliderHelper.OnPush();
            }

            Profiler.BeginSample("Box.Initialize DOTween GC Check");
            transform.DOPause();
            transform.DOLocalMove(LocalGP, lerpTime).SetEase(Ease.Linear).OnComplete(() =>
            {
                State = States.Static;
                IsInGridSystem = true;
                if (lerpType == LerpType.Drop)
                {
                    BoxColliderHelper.OnDropComplete();
                }

                if (lerpType == LerpType.Push)
                {
                    BoxColliderHelper.OnPushEnd();
                }

                if (ENABLE_BOX_MOVE_LOG) Debug.Log($"[{Time.frameCount}] [Box] {name} Init LerpType:{lerpType} DOLerpGP:{LocalGP} finalPos:{transform.position}");
            });
            transform.DOLocalRotate(Vector3.zero, lerpTime);
            Profiler.EndSample();
            switch (lerpType)
            {
                case LerpType.Push:
                {
                    State = States.BeingPushed;
                    break;
                }
                case LerpType.Kick:
                {
                    State = States.BeingKicked;
                    break;
                }
                case LerpType.KickToGrind:
                {
                    State = States.BeingKickedToGrind;
                    break;
                }
                case LerpType.Throw:
                {
                    State = States.Flying;
                    break;
                }
                case LerpType.Put:
                {
                    State = States.Putting;
                    break;
                }
                case LerpType.Drop:
                {
                    State = States.Dropping;
                    break;
                }
                case LerpType.Create:
                {
                    State = States.Static;
                    break;
                }
            }
        }
        else
        {
            IsInGridSystem = true;
            switch (lerpType)
            {
                case LerpType.DropFromEntity:
                {
                    State = States.DroppingFromEntity;
                    break;
                }
                case LerpType.DropFromAir:
                {
                    State = States.DroppingFromAir;
                    break;
                }
                default:
                {
                    State = States.Static;
                    break;
                }
            }

            if (needLerpModel) SetModelSmoothMoveLerpTime(0.2f);
            transform.localPosition = LocalGP;
            if (ENABLE_BOX_MOVE_LOG) Debug.Log($"[{Time.frameCount}] [Box] {name} Init LerpType:{lerpType} NoLerpTime {transform.position}");
            transform.localRotation = Quaternion.identity;
        }

        BoxMarchingTextureHelper?.Initialize();
        if (needCheckDrop) WorldManager.Instance.CurrentWorld.CheckDropSelf(this);
    }

    #region Actions: Push, Stop, Kick, Swap, Lift, Throw, Put, DropFromAir, DropFromDeadActor

    public void Push(Vector3 direction, Actor actor)
    {
        if (state == States.Static)
        {
            SetModelSmoothMoveLerpTime(0);
            Vector3 targetPos = WorldGP + direction.normalized;
            GridPos3D gp = GridPos3D.GetGridPosByPoint(targetPos, 1);
            if (gp != WorldGP)
            {
                if (ENABLE_BOX_MOVE_LOG) Debug.Log($"[{Time.frameCount}] [Box] {name} Push {WorldGP} -> {gp}");
                CurrentMoveGlobalPlanerDir = (gp - WorldGP).Normalized();
                WorldManager.Instance.CurrentWorld.MoveBoxColumn(WorldGP, CurrentMoveGlobalPlanerDir, States.BeingPushed, true, false, actor.GUID);
            }
        }
    }

    public void ForceStopWhenSwapBox(Actor actor)
    {
        GridPos3D targetGP = transform.position.ToGridPos3D();
        GridPos3D moveDirection = (targetGP - WorldGP).Normalized();
        WorldManager.Instance.CurrentWorld.BoxColumnTransformDOPause(WorldGP, moveDirection, actor.GUID);
        if (moveDirection != GridPos3D.Zero)
        {
            if (ENABLE_BOX_MOVE_LOG) Debug.Log($"[{Time.frameCount}] Box] {name} ForceCancelPush {WorldGP} -> {targetGP}");
            CurrentMoveGlobalPlanerDir = moveDirection;
            WorldManager.Instance.CurrentWorld.MoveBoxColumn(WorldGP, CurrentMoveGlobalPlanerDir, States.Static, false, true, actor.GUID);
        }
    }

    public void Kick(Vector3 direction, float velocity, Actor actor)
    {
        if (state == States.BeingPushed || state == States.Flying || state == States.BeingKicked || state == States.BeingKickedToGrind || state == States.Static)
        {
            transform.DOPause();
            SetModelSmoothMoveLerpTime(0);
            if (BoxEffectHelper == null)
            {
                BoxEffectHelper = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.BoxEffectHelper].AllocateGameObject<BoxEffectHelper>(transform);
            }

            foreach (EntityPassiveSkill ps in EntityPassiveSkills)
            {
                ps.OnBeingKicked(actor);
            }

            alreadyCollidedActorSet.Clear();
            LastInteractActorGUID = actor.GUID;
            worldGP_WhenKicked = WorldGP;
            transform.DOPause();
            Rigidbody = gameObject.GetComponent<Rigidbody>();
            if (!hasRigidbody)
            {
                Rigidbody = gameObject.AddComponent<Rigidbody>();
                hasRigidbody = true;
            }

            foreach (EntityFlamethrowerHelper h in EntityFlamethrowerHelpers)
            {
                h.OnMoving();
            }

            Rigidbody.mass = FinalWeight;
            Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            Rigidbody.drag = Dynamic_Drag * ConfigManager.BoxKickDragFactor_Cheat;
            Rigidbody.angularDrag = 0;
            Rigidbody.useGravity = false;
            Rigidbody.angularVelocity = Vector3.zero;
            Rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

            CurrentMoveGlobalPlanerDir = direction.ToGridPos3D();
            if (CurrentMoveGlobalPlanerDir.x == 0)
            {
                Rigidbody.constraints |= RigidbodyConstraints.FreezePositionX;
            }
            else
            {
                CurrentKickGlobalAxis = KickAxis.X;
                if (EntityOrientation == GridPosR.Orientation.Left || EntityOrientation == GridPosR.Orientation.Right) CurrentKickLocalAxis = KickAxis.Z;
                else CurrentKickLocalAxis = KickAxis.X;
            }

            if (CurrentMoveGlobalPlanerDir.z == 0)
            {
                Rigidbody.constraints |= RigidbodyConstraints.FreezePositionZ;
            }
            else
            {
                CurrentKickGlobalAxis = KickAxis.Z;
                if (EntityOrientation == GridPosR.Orientation.Left || EntityOrientation == GridPosR.Orientation.Right) CurrentKickLocalAxis = KickAxis.X;
                else CurrentKickLocalAxis = KickAxis.Z;
            }

            if (CurrentKickLocalAxis == KickAxis.X && Grind_X || CurrentKickLocalAxis == KickAxis.Z && Grind_Z)
            {
                BoxGrindTriggerZoneHelper?.SetActive(true);
                BoxColliderHelper.OnKick_ToGrind();
                State = States.BeingKickedToGrind;
            }
            else
            {
                BoxGrindTriggerZoneHelper?.SetActive(false);
                BoxColliderHelper.OnKick();
                State = States.BeingKicked;
            }

            float kickForceMultiplier = CurrentKickLocalAxis == KickAxis.X ? KickForce_X : (CurrentKickLocalAxis == KickAxis.Z ? KickForce_Z : 1f);
            Rigidbody.velocity = direction.normalized * velocity * kickForceMultiplier;
            transform.position = transform.position.ToGridPos3D();
            EntityWwiseHelper.OnBeingKicked.Post(gameObject);
        }
    }

    private Quaternion DefaultRotBeforeLift;

    public bool BeingLift(Actor actor)
    {
        if (state == States.BeingPushed || state == States.Flying || state == States.BeingKicked || state == States.BeingKickedToGrind || state == States.Static)
        {
            SetModelSmoothMoveLerpTime(0);
            DefaultRotBeforeLift = transform.rotation;
            alreadyCollidedActorSet.Clear();
            LastInteractActorGUID = actor.GUID;
            foreach (EntityPassiveSkill ps in EntityPassiveSkills)
            {
                ps.OnBeingLift(actor);
            }

            WorldManager.Instance.CurrentWorld.RemoveBoxFromGrid(this);
            State = States.BeingLift;
            transform.DOPause();
            BoxColliderHelper.OnBeingLift();
            if (hasRigidbody)
            {
                DestroyImmediate(Rigidbody);
                hasRigidbody = false;
            }

            BoxEffectHelper?.PoolRecycle();
            BoxEffectHelper = null;
            EntityWwiseHelper.OnBeingLift.Post(gameObject);
            return true;
        }

        return false;
    }

    public void LiftThenConsume()
    {
        PlayFXOnEachGrid(CollideFX);
        PoolRecycle();
    }

    public void Throw(Vector3 direction, float velocity, Actor actor)
    {
        if (state == States.Lifted)
        {
            SetModelSmoothMoveLerpTime(0);
            alreadyCollidedActorSet.Clear();
            if (BoxEffectHelper == null)
            {
                BoxEffectHelper = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.BoxEffectHelper].AllocateGameObject<BoxEffectHelper>(transform);
            }

            LastInteractActorGUID = actor.GUID;
            State = States.Flying;
            transform.DOPause();
            transform.parent = WorldManager.Instance.CurrentWorld.transform;
            BoxColliderHelper.OnThrow();
            Rigidbody = gameObject.GetComponent<Rigidbody>();
            if (!hasRigidbody)
            {
                Rigidbody = gameObject.AddComponent<Rigidbody>();
                hasRigidbody = true;
            }

            foreach (EntityFlamethrowerHelper h in EntityFlamethrowerHelpers)
            {
                h.OnMoving();
            }

            Rigidbody.mass = FinalWeight;
            Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            Rigidbody.drag = 0;
            Rigidbody.angularDrag = 0;
            Rigidbody.useGravity = true;
            Rigidbody.velocity = direction.normalized * velocity;
            Rigidbody.angularVelocity = Vector3.zero;
            Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    public void Put(Vector3 direction, float velocity, Actor actor)
    {
        if (State == States.Lifted)
        {
            SetModelSmoothMoveLerpTime(0);
            alreadyCollidedActorSet.Clear();
            if (BoxEffectHelper == null)
            {
                BoxEffectHelper = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.BoxEffectHelper].AllocateGameObject<BoxEffectHelper>(transform);
            }

            LastInteractActorGUID = actor.GUID;
            State = States.Putting;
            transform.DOPause();
            transform.parent = WorldManager.Instance.CurrentWorld.transform;
            BoxColliderHelper.OnPut();
            Rigidbody = gameObject.GetComponent<Rigidbody>();
            if (!hasRigidbody)
            {
                Rigidbody = gameObject.AddComponent<Rigidbody>();
                hasRigidbody = true;
            }

            foreach (EntityFlamethrowerHelper h in EntityFlamethrowerHelpers)
            {
                h.OnMoving();
            }

            Rigidbody.mass = FinalWeight;
            Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            Rigidbody.drag = 0;
            Rigidbody.angularDrag = 0;
            Rigidbody.useGravity = true;
            Rigidbody.velocity = direction.normalized * velocity;
            CurrentMoveGlobalPlanerDir = direction.normalized.ToGridPos3D();
            CurrentMoveGlobalPlanerDir.y = 0;
            Rigidbody.angularVelocity = Vector3.zero;
            Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            EntityWwiseHelper.OnBeingDropped.Post(gameObject);
        }
    }

    public void DropFromEntity(Vector3 startVelocity)
    {
        SetModelSmoothMoveLerpTime(0);
        if (BoxEffectHelper == null)
        {
            BoxEffectHelper = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.BoxEffectHelper].AllocateGameObject<BoxEffectHelper>(transform);
        }

        alreadyCollidedActorSet.Clear();
        LastInteractActorGUID = 0;
        State = States.DroppingFromEntity;
        transform.DOPause();
        transform.parent = WorldManager.Instance.CurrentWorld.transform;
        BoxColliderHelper.OnDropFromEntity();
        Rigidbody = gameObject.GetComponent<Rigidbody>();
        if (!hasRigidbody)
        {
            Rigidbody = gameObject.AddComponent<Rigidbody>();
            hasRigidbody = true;
        }

        foreach (EntityFlamethrowerHelper h in EntityFlamethrowerHelpers)
        {
            h.OnMoving();
        }

        Rigidbody.mass = FinalWeight;
        Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        Rigidbody.drag = 0;
        Rigidbody.angularDrag = 0;
        Rigidbody.useGravity = true;
        Rigidbody.velocity = startVelocity;
        Rigidbody.angularVelocity = Vector3.zero;
        Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        EntityWwiseHelper.OnThrownUp.Post(gameObject);
    }

    public void DropFromAir()
    {
        SetModelSmoothMoveLerpTime(0);
        if (BoxEffectHelper == null)
        {
            BoxEffectHelper = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.BoxEffectHelper].AllocateGameObject<BoxEffectHelper>(transform);
        }

        alreadyCollidedActorSet.Clear();
        LastInteractActorGUID = 0;
        State = States.DroppingFromAir;
        transform.DOPause();
        transform.parent = WorldManager.Instance.CurrentWorld.transform;
        BoxColliderHelper.OnDropFromAir();
        Rigidbody = gameObject.GetComponent<Rigidbody>();
        if (!hasRigidbody)
        {
            Rigidbody = gameObject.AddComponent<Rigidbody>();
            hasRigidbody = true;
        }

        foreach (EntityFlamethrowerHelper h in EntityFlamethrowerHelpers)
        {
            h.OnMoving();
        }

        Rigidbody.mass = FinalWeight;
        Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        Rigidbody.drag = 0;
        Rigidbody.angularDrag = 0;
        Rigidbody.useGravity = true;
        Rigidbody.velocity = Vector3.down * 2f;
        Rigidbody.angularVelocity = Vector3.zero;
        Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    #endregion

    protected override void Tick(float interval)
    {
        base.Tick(interval);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (IsRecycled) return;
        if (FaceToCameraForever)
        {
            SwitchEntityOrientation(CameraManager.Instance.FieldCamera.RotateDirection);
        }

        if ((state == States.BeingKicked || state == States.BeingKickedToGrind) && IsInGridSystem)
        {
            if (transform.position.ToGridPos3D() != worldGP_WhenKicked)
            {
                WorldManager.Instance.CurrentWorld.RemoveBoxFromGrid(this);
                worldGP_WhenKicked = GridPos3D.Zero;
            }
        }

        if ((state == States.BeingKicked || state == States.BeingKickedToGrind || state == States.Flying || state == States.DroppingFromEntity || state == States.DroppingFromAir || state == States.Putting) && hasRigidbody)
        {
            if (state == States.BeingKicked || state == States.BeingKickedToGrind)
            {
                bool isGrounded = Physics.Raycast(new Ray(transform.position, Vector3.down), 0.55f, LayerManager.Instance.LayerMask_BoxIndicator | LayerManager.Instance.LayerMask_Ground);
                Rigidbody.useGravity = !isGrounded;
                if (isGrounded)
                {
                    Rigidbody.constraints |= RigidbodyConstraints.FreezePositionY;
                }
                else // 踢出地面时允许抛物线向下落
                {
                    Rigidbody.constraints &= ~RigidbodyConstraints.FreezePositionY;
                }
            }

            if (hasRigidbody && Rigidbody.velocity.magnitude < 1f)
            {
                bool isGrounded = false;
                foreach (GridPos3D offset in GetEntityOccupationGPs_Rotated())
                {
                    bool hitGround = Physics.Raycast(transform.position + offset, Vector3.down, 1, LayerManager.Instance.LayerMask_Box | LayerManager.Instance.LayerMask_Ground);
                    if (hitGround)
                    {
                        isGrounded = true;
                        break;
                    }
                }

                if (isGrounded)
                {
                    bool checkMerge = state == States.BeingKicked || state == States.BeingKickedToGrind && LastInteractActor != null && LastInteractActor == BattleManager.Instance.Player1;
                    LastInteractActorGUID = 0;
                    DestroyImmediate(Rigidbody);
                    hasRigidbody = false;
                    if (state == States.BeingKickedToGrind)
                    {
                        BoxColliderHelper.OnKick_ToGrind_End();
                    }

                    BoxColliderHelper.OnRigidbodyStop();
                    BoxEffectHelper?.PoolRecycle();
                    BoxEffectHelper = null;
                    BoxGrindTriggerZoneHelper?.SetActive(false);

                    foreach (EntityFlamethrowerHelper h in EntityFlamethrowerHelpers)
                    {
                        h.OnRigidbodyStop();
                    }

                    if (!IsDestroying) WorldManager.Instance.CurrentWorld.BoxReturnToWorldFromPhysics(this, checkMerge, CurrentMoveGlobalPlanerDir); // 这里面已经做了“Box本来就在Grid系统里”的判定
                    CurrentMoveGlobalPlanerDir = GridPos3D.Zero;
                    EntityWwiseHelper.OnSlideStop.Post(gameObject);
                }
            }
        }

        if (state == States.Lifted || state == States.BeingLift)
        {
            transform.rotation = DefaultRotBeforeLift;
        }

        if (hasRigidbody && Rigidbody.velocity.magnitude > 1f)
        {
            BoxEffectHelper?.Play();
        }
        else
        {
            BoxEffectHelper?.Stop();
        }

        if (hasRigidbody && Rigidbody.velocity.magnitude > 20f)
        {
            EntityModelHelper.SetVelocity(Rigidbody.velocity.x, Rigidbody.velocity.z);
        }
        else
        {
            EntityModelHelper.SetVelocity(0, 0);
        }

        if (PassiveSkillMarkAsDestroyed)
        {
            DestroySelf();
        }
    }

    #region Collide & Trigger

    protected override void OnCollisionEnter(Collision collision)
    {
        if (IsRecycled) return;
        if (collision.gameObject.layer == LayerManager.Instance.Layer_CollectableItem) return;
        if (LastInteractActor.IsNotNullAndAlive() && collision.gameObject == LastInteractActor.gameObject) return; // todo 这里判定上一个碰的Actor有啥用?
        switch (State)
        {
            case States.Putting:
            {
                Entity collidedEntity = collision.collider.GetComponentInParent<Entity>();
                if (!collidedEntity && collidedEntity is Box collidedBox) // 此处不判定死亡
                {
                    if (!collidedBox.BoxFeature.HasFlag(BoxFeature.IsBorder))
                    {
                        Rigidbody.drag = Throw_Drag * ConfigManager.BoxThrowDragFactor_Cheat;
                    }
                }

                break;
            }
            case States.Flying:
            {
                DealCollideDamageToActor(collision, KickAxis.None);
                PlayFXOnEachGrid(CollideFX);
                foreach (EntityPassiveSkill ps in EntityPassiveSkills)
                {
                    ps.OnFlyingCollisionEnter(collision);
                }

                if (PassiveSkillMarkAsDestroyed && !IsRecycled)
                {
                    DestroySelf();
                    return;
                }

                if (!IsRecycled)
                {
                    OnFlyingCollisionEnter(collision);
                    if (PassiveSkillMarkAsDestroyed)
                    {
                        DestroySelf();
                        return;
                    }
                }

                break;
            }
            case States.BeingKicked:
            case States.BeingKickedToGrind:
            {
                if (collision.gameObject.layer != LayerManager.Instance.Layer_Ground) // 避免在地上摩擦判定为碰撞
                {
                    if (State == States.BeingKicked) DealCollideDamageToActor(collision, CurrentKickLocalAxis);
                    PlayFXOnEachGrid(CollideFX);
                    if (hasRigidbody) Rigidbody.velocity = Vector3.zero;
                    foreach (EntityPassiveSkill ps in EntityPassiveSkills)
                    {
                        ps.OnBeingKickedCollisionEnter(collision, CurrentKickLocalAxis);
                    }

                    if (PassiveSkillMarkAsDestroyed && !IsRecycled)
                    {
                        DestroySelf();
                        return;
                    }

                    if (!IsRecycled)
                    {
                        OnBeingKickedCollisionEnter(collision);
                        if (PassiveSkillMarkAsDestroyed)
                        {
                            DestroySelf();
                            return;
                        }
                    }
                }

                break;
            }
            case States.DroppingFromAir:
            {
                Entity collidedEntity = collision.collider.GetComponentInParent<Entity>();
                if (!collidedEntity && collidedEntity is Box collidedBox)
                {
                    if (collidedBox.State == States.DroppingFromAir) return; // 避免空中和其他坠落的箱子相撞
                }

                DealCollideDamageToActor(collision, KickAxis.None);
                PlayFXOnEachGrid(CollideFX);
                foreach (EntityPassiveSkill ps in EntityPassiveSkills)
                {
                    ps.OnDroppingFromAirCollisionEnter(collision);
                }

                if (PassiveSkillMarkAsDestroyed && !IsRecycled)
                {
                    DestroySelf();
                    return;
                }

                if (!IsRecycled)
                {
                    OnDroppingFromAirCollisionEnter(collision);
                    if (PassiveSkillMarkAsDestroyed)
                    {
                        DestroySelf();
                        return;
                    }
                }

                break;
            }
        }
    }

    private bool DealCollideDamageToActor(Collision collision, KickAxis kickLocalAxis)
    {
        Actor collidedActor = collision.collider.GetComponentInParent<Actor>();
        if (collidedActor == null
            || alreadyCollidedActorSet.Contains(collidedActor)
            || !collidedActor.IsNotNullAndAlive()
            || collidedActor == LastInteractActor
            || !collidedActor.IsOpponentOrNeutralCampOf(LastInteractActor)
        ) return false;
        alreadyCollidedActorSet.Add(collidedActor);

        collidedActor.ActorBattleHelper.LastAttackBox = this;
        if (collidedActor.RigidBody != null)
        {
            Vector3 nearestBoxIndicator = GridPos3D.Zero;
            float nearestDist = float.MaxValue;
            foreach (GridPos3D offset in GetEntityOccupationGPs_Rotated())
            {
                Vector3 boxIndicatorPos = transform.position + offset;
                float distToContactPos = (boxIndicatorPos - collision.contacts[0].point).magnitude;
                if (nearestDist > distToContactPos)
                {
                    nearestDist = distToContactPos;
                    nearestBoxIndicator = boxIndicatorPos;
                }
            }

            Vector3 force = (collidedActor.transform.position - nearestBoxIndicator).normalized;
            force = CurrentKickGlobalAxis == KickAxis.X ? new Vector3(force.x, 0, 0) : new Vector3(0, 0, force.z);

            collidedActor.RigidBody.velocity = Vector3.zero;
            Vector3 repelForce = force * (CurrentKickLocalAxis == KickAxis.X ? KickRepelForce_X : KickRepelForce_Z); // 不同Local方向撞击击退力度不同
            collidedActor.RigidBody.AddForce(repelForce, ForceMode.VelocityChange);
            collidedActor.EntityBuffHelper.AddBuff(new EntityBuff_AttributeLabel {CasterActorGUID = LastInteractActorGUID, Duration = repelForce.magnitude / 30f, EntityBuffAttribute = EntityBuffAttribute.Repulse, IsPermanent = false});
            collidedActor.EntityBuffHelper.Damage(EntityStatPropSet.GetCollideDamageByAxis(kickLocalAxis).GetModifiedValue, EntityBuffAttribute.CollideDamage, LastInteractActorGUID);
            return true;
        }

        return false;
    }

    #endregion

    public void PlayFXOnEachGrid(FXConfig fxConfig)
    {
        foreach (GridPos3D offset in GetEntityOccupationGPs_Rotated())
        {
            FX hit = FXManager.Instance.PlayFX(fxConfig, transform.position + offset);
        }
    }

    #region Destroy

    public override void DestroySelfByModuleRecycle()
    {
        if (IsDestroying || IsRecycled) return;
        base.DestroySelfByModuleRecycle();
        IsDestroying = true;
        WorldManager.Instance.CurrentWorld.DeleteBox(this);
    }

    public override void DestroySelf(UnityAction callBack = null)
    {
        if (IsDestroying || IsRecycled) return;
        base.DestroySelf(callBack);
        IsDestroying = true;
        foreach (EntityPassiveSkill ps in EntityPassiveSkills)
        {
            ps.OnBeforeDestroyEntity();
        }

        StartCoroutine(Co_DelayDestroyBox(callBack));
    }

    IEnumerator Co_DelayDestroyBox(UnityAction callBack)
    {
        yield return new WaitForSeconds(DeleteDelay);
        foreach (EntityPassiveSkill ps in EntityPassiveSkills)
        {
            ps.OnDestroyEntity();
        }

        PlayFXOnEachGrid(DestroyFX);
        WorldManager.Instance.CurrentWorld.DeleteBox(this);
        callBack?.Invoke();
    }

    #endregion

    #region Merge

    public void MergeBox(GridPos3D mergeToWorldGP, UnityAction callBack = null)
    {
        MarkedAsMergedSourceBox = true;
        BoxColliderHelper.OnMerge();
        BoxIndicatorHelper.IsOn = false;
        foreach (EntityPassiveSkill ps in EntityPassiveSkills)
        {
            ps.OnBeforeMergeBox();
        }

        StartCoroutine(Co_DelayMergeBox(mergeToWorldGP, callBack));
    }

    IEnumerator Co_DelayMergeBox(GridPos3D mergeToWorldGP, UnityAction callBack)
    {
        PlayFXOnEachGrid(MergeFX);

        EntityModelHelper.transform.DOShakeScale(0.2f);
        EntityModelHelper.transform.DOMove(mergeToWorldGP, MergeDelay * 1.2f);
        yield return new WaitForSeconds(MergeDelay);
        foreach (EntityPassiveSkill ps in EntityPassiveSkills)
        {
            ps.OnMergeBox();
        }

        WorldManager.Instance.CurrentWorld.DeleteBox(this, callBack == null); // 有callback的是oldBoxCore，要避免上面box落下导致新箱子合成不出来
        callBack?.Invoke();
    }

    #endregion

    public void FuelBox()
    {
        foreach (EntityPassiveSkill ps in EntityPassiveSkills)
        {
            ps.OnBeingFueled();
        }

        DestroySelf();
    }

    #region Drop

    private Coroutine KeepTryingDropSelfCoroutine;

    public void StartTryingDropSelf()
    {
        if (KeepTryingDropSelfCoroutine != null) StopCoroutine(KeepTryingDropSelfCoroutine);
        KeepTryingDropSelfCoroutine = StartCoroutine(Co_KeepTryingDropSelf());
    }

    static WaitForSeconds tryingDropInterval = new WaitForSeconds(0.1f);

    IEnumerator Co_KeepTryingDropSelf()
    {
        yield return tryingDropInterval;
        WorldManager.Instance.CurrentWorld.CheckDropSelf(this);
        yield return tryingDropInterval;
        WorldManager.Instance.CurrentWorld.CheckDropSelf(this);
        yield return tryingDropInterval;
        WorldManager.Instance.CurrentWorld.CheckDropSelf(this);
        yield return tryingDropInterval;
        WorldManager.Instance.CurrentWorld.CheckDropSelf(this);
        yield return tryingDropInterval;
        WorldManager.Instance.CurrentWorld.CheckDropSelf(this);
        KeepTryingDropSelfCoroutine = null;
    }

    #endregion

    public bool Pushable => BoxFeature.HasFlag(BoxFeature.Pushable);

    public bool Kickable => BoxFeature.HasFlag(BoxFeature.Kickable);

    public bool Liftable => BoxFeature.HasFlag(BoxFeature.Liftable);

    public bool KickOrThrowable => Kickable || Throwable;

    public bool Throwable => BoxFeature.HasFlag(BoxFeature.Throwable);

    public bool Interactable => Pushable || Kickable || Liftable || Throwable;

    public bool Passable => EntityIndicatorHelper.EntityOccupationData.Passable;

    public bool Droppable => BoxFeature.HasFlag(BoxFeature.Droppable);

    public bool Consumable => BoxFeature.HasFlag(BoxFeature.LiftThenDisappear);

    public bool IsHidden => BoxFeature.HasFlag(BoxFeature.Hidden);
}

[Flags]
public enum BoxFeature
{
    None = 0,

    [LabelText("可推动")]
    Pushable = 1 << 0,

    [LabelText("可踢")]
    Kickable = 1 << 1,

    [LabelText("可举")]
    Liftable = 1 << 2,

    [LabelText("可扔")]
    Throwable = 1 << 3,

    [LabelText("会塌落")]
    Droppable = 1 << 4,

    [LabelText("--占位0--")]
    Other0 = 1 << 5,

    [LabelText("--占位1--")]
    Other1 = 1 << 6,

    [LabelText("--占位2--")]
    Other2 = 1 << 7,

    [LabelText("--占位3--")]
    Other3 = 1 << 8,

    [LabelText("墙壁")]
    IsBorder = 1 << 9,

    [LabelText("地面")]
    IsGround = 1 << 10,

    [LabelText("举起就消失")]
    LiftThenDisappear = 1 << 11,

    [LabelText("--占位4--")]
    Other4 = 1 << 12,

    [LabelText("不受BUff影响")]
    BuffImmune = 1 << 13,

    [LabelText("隐藏的")]
    Hidden = 1 << 14,
}
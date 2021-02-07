using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BiangLibrary;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.GamePlay;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;

#endif

public partial class Box : Entity
{
    public static bool ENABLE_BOX_MOVE_LOG = false;

    internal bool hasRigidbody;
    internal Rigidbody Rigidbody;

    internal Actor LastTouchActor;

    [FoldoutGroup("组件")]
    public GameObject ModelRoot;

    internal override EntityBuffHelper EntityBuffHelper => BoxBuffHelper;
    internal override EntityFrozenHelper EntityFrozenHelper => BoxFrozenHelper;
    internal override EntityTriggerZoneHelper EntityTriggerZoneHelper => BoxTriggerZoneHelper;
    internal override EntityGrindTriggerZoneHelper EntityGrindTriggerZoneHelper => BoxGrindTriggerZoneHelper;
    internal override List<EntityFlamethrowerHelper> EntityFlamethrowerHelpers => BoxFlamethrowerHelpers;

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
    private EntityGrindTriggerZoneHelper BoxGrindTriggerZoneHelper;

    [FoldoutGroup("组件")]
    [SerializeField]
    private List<EntityFlamethrowerHelper> BoxFlamethrowerHelpers;

    internal BoxEffectHelper BoxEffectHelper;

    [FoldoutGroup("组件")]
    public BoxColliderHelper BoxColliderHelper;

    [FoldoutGroup("组件")]
    public BoxIndicatorHelper BoxIndicatorHelper;

    [FoldoutGroup("组件")]
    public DoorBoxHelper DoorBoxHelper;

    [FoldoutGroup("组件")]
    public BoxSkinHelper BoxSkinHelper;

    [FoldoutGroup("组件")]
    public BoxIconSpriteHelper BoxIconSpriteHelper;

    [FoldoutGroup("组件")]
    public SmoothMove BoxIconSpriteSmoothMove;

    [FoldoutGroup("组件")]
    public SmoothMove BoxModelSmoothMove;

    internal Actor FrozenActor; // EnemyFrozenBox将敌人冻住包裹

    internal bool ArtOnly;

    void Awake()
    {
        EntityStatPropSet = new EntityStatPropSet();
    }

    public override void OnUsed()
    {
        LastTouchActor = null;
        ArtOnly = true;

        EntityBuffHelper.OnHelperUsed();
        EntityFrozenHelper.OnHelperUsed();
        EntityTriggerZoneHelper?.OnHelperUsed();
        EntityGrindTriggerZoneHelper?.OnHelperUsed();
        foreach (EntityFlamethrowerHelper h in EntityFlamethrowerHelpers)
        {
            h.OnHelperUsed();
        }

        BoxColliderHelper.OnBoxUsed();
        BoxIndicatorHelper.OnHelperUsed();
        DoorBoxHelper?.OnHelperUsed();
        BoxSkinHelper?.OnHelperUsed();
        BoxIconSpriteHelper.OnHelperUsed();
        base.OnUsed();
    }

    public override void OnRecycled()
    {
        if (KeepTryingDropSelfCoroutine != null) StopCoroutine(KeepTryingDropSelfCoroutine);
        ArtOnly = true;
        WorldModule = null;
        WorldGP = GridPos3D.Zero;
        LastWorldGP = GridPos3D.Zero;
        worldGP_WhenKicked = GridPos3D.Zero;
        LastState = States.Static;
        State = States.Static;

        EntityBuffHelper.OnHelperRecycled();
        EntityFrozenHelper.OnHelperRecycled();
        EntityTriggerZoneHelper?.OnHelperRecycled();
        EntityGrindTriggerZoneHelper?.OnHelperRecycled();
        foreach (EntityFlamethrowerHelper h in EntityFlamethrowerHelpers)
        {
            h.OnHelperRecycled();
        }

        BoxEffectHelper?.OnBoxPoolRecycled();
        BoxEffectHelper = null;
        BoxColliderHelper.OnBoxPoolRecycled();
        BoxIndicatorHelper.OnHelperRecycled();
        DoorBoxHelper?.OnHelperRecycled();
        BoxSkinHelper?.OnHelperRecycled();
        BoxIconSpriteHelper?.OnHelperRecycled();

        transform.DOPause();
        ModelRoot.transform.DOPause();
        ModelRoot.transform.localPosition = Vector3.zero;
        ModelRoot.transform.localScale = Vector3.one;
        alreadyCollidedActorSet.Clear();
        if (hasRigidbody)
        {
            Destroy(Rigidbody);
            hasRigidbody = false;
        }

        if (LastTouchActor.IsNotNullAndAlive() && LastTouchActor.CurrentLiftBox == this)
        {
            LastTouchActor.ThrowState = Actor.ThrowStates.None;
            LastTouchActor.CurrentLiftBox = null;
            LastTouchActor = null;
        }

        EntityStatPropSet.OnRecycled();
        MarkedAsMergedSourceBox = false;

        UnInitPassiveSkills();
        base.OnRecycled();
    }

    [HideInInspector]
    public ushort BoxTypeIndex;

    [FoldoutGroup("箱子属性")]
    [LabelText("箱子特性")]
    [AssetsOnly]
    public BoxFeature BoxFeature;

    #region 旋转朝向

    internal GridPosR.Orientation BoxOrientation { get; private set; }

    private void SwitchBoxOrientation(GridPosR.Orientation boxOrientation)
    {
        BoxOrientation = boxOrientation;
        GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, boxOrientation), BoxColliderHelper.transform, 1);
        GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, boxOrientation), ModelRoot.transform, 1);
        GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, boxOrientation), BoxFrozenHelper.FrozeModelRoot.transform, 1);
        GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, boxOrientation), BoxIndicatorHelper.transform, 1);
        if (EntityTriggerZoneHelper) GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, boxOrientation), EntityTriggerZoneHelper.transform, 1);
        if (EntityGrindTriggerZoneHelper) GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, boxOrientation), EntityGrindTriggerZoneHelper.transform, 1);
    }

    #endregion

    #region 手感  静态属性

    [ReadOnly]
    [AssetsOnly]
    [ShowInInspector]
    [ShowIf("Interactable")]
    [FoldoutGroup("箱子属性")]
    [LabelText("重量")]
    private const float Weight = 0.7f;

    public float FinalWeight => Weight * ConfigManager.BoxWeightFactor_Cheat;

    /// <summary>
    /// 扔箱子落地Drag
    /// </summary>
    internal const float Throw_Drag = 10f;

    /// <summary>
    /// 扔箱子落地摩擦力
    /// </summary>
    internal const float Throw_Friction = 1;

    /// <summary>
    /// 踢箱子摩阻力
    /// </summary>
    internal const float Dynamic_Drag = 0.5f;

    #endregion

    #region 各向异性

    [SerializeField]
    [FoldoutGroup("箱子属性")]
    [LabelText("X轴(Local)踢出速度倍率")]
    private float KickForce_X = 1f;

    [SerializeField]
    [FoldoutGroup("箱子属性")]
    [LabelText("Z轴(Local)踢出速度倍率")]
    private float KickForce_Z = 1f;

    [SerializeField]
    [FoldoutGroup("箱子属性")]
    [LabelText("X轴(Local)击退力")]
    private float KickRepelForce_X = 10f;

    [SerializeField]
    [FoldoutGroup("箱子属性")]
    [LabelText("Z轴(Local)击退力")]
    private float KickRepelForce_Z = 10f;

    [SerializeField]
    [FoldoutGroup("箱子属性")]
    [LabelText("X轴(Local)碾压")]
    [SerializeReference]
    private bool Grind_X;

    [SerializeField]
    [FoldoutGroup("箱子属性")]
    [LabelText("Z轴(Local)碾压")]
    [SerializeReference]
    private bool Grind_Z;

    #endregion

    [SerializeField]
    [FoldoutGroup("箱子属性")]
    [LabelText("死亡延迟")]
    private float DeleteDelay = 0;

    [SerializeField]
    [FoldoutGroup("箱子属性")]
    [LabelText("合并延迟")]
    private float MergeDelay = 0;

    #region 合成

    [FoldoutGroup("合成")]
    [LabelText("合成产物是否多格")]
    public bool MergeBoxFullOccupation = false;

    [BoxName]
    [FoldoutGroup("合成")]
    [LabelText("三合一")]
    [ValueDropdown("GetAllBoxTypeNames")]
    public string MergeBox_MatchThree;

    [BoxName]
    [FoldoutGroup("合成")]
    [LabelText("四合一")]
    [ValueDropdown("GetAllBoxTypeNames")]
    public string MergeBox_MatchFour;

    [BoxName]
    [FoldoutGroup("合成")]
    [LabelText("五合一")]
    [ValueDropdown("GetAllBoxTypeNames")]
    public string MergeBox_MatchFive;

    public ushort GetMergeBoxTypeIndex(int mergeCount)
    {
        switch (mergeCount)
        {
            case 3: return ConfigManager.GetBoxTypeIndex(MergeBox_MatchThree);
            case 4: return ConfigManager.GetBoxTypeIndex(MergeBox_MatchFour);
            case 5: return ConfigManager.GetBoxTypeIndex(MergeBox_MatchFive);
        }

        return 0;
    }

    #endregion

    [AssetsOnly]
    [SerializeField]
    [FoldoutGroup("特效")]
    [LabelText("撞击特效")]
    [ValueDropdown("GetAllFXTypeNames")]
    public string CollideFX;

    [AssetsOnly]
    [SerializeField]
    [FoldoutGroup("特效")]
    [LabelText("撞击特效尺寸")]
    public float CollideFXScale = 1f;

    [AssetsOnly]
    [SerializeField]
    [FoldoutGroup("特效")]
    [LabelText("死亡特效")]
    [ValueDropdown("GetAllFXTypeNames")]
    public string DestroyFX;

    [AssetsOnly]
    [SerializeField]
    [FoldoutGroup("特效")]
    [LabelText("死亡特效尺寸")]
    public float DestroyFXScale = 1f;

    [FoldoutGroup("特效")]
    [LabelText("解冻特效")]
    [ValueDropdown("GetAllFXTypeNames")]
    public string ThawFX;

    [AssetsOnly]
    [SerializeField]
    [FoldoutGroup("特效")]
    [LabelText("解冻特效尺寸")]
    public float ThawFXScale = 1f;

    [FoldoutGroup("特效")]
    [LabelText("冻结特效")]
    [ValueDropdown("GetAllFXTypeNames")]
    public string FrozeFX;

    [AssetsOnly]
    [SerializeField]
    [FoldoutGroup("特效")]
    [LabelText("冻结特效尺寸")]
    public float FrozeFXScale = 1f;

    [FoldoutGroup("特效")]
    [LabelText("合成特效")]
    [ValueDropdown("GetAllFXTypeNames")]
    public string MergeFX;

    [FoldoutGroup("特效")]
    [LabelText("合成特效尺寸")]
    public float MergeFXScale = 1f;

    [FoldoutGroup("特效")]
    [LabelText("合成后特效")]
    [ValueDropdown("GetAllFXTypeNames")]
    public string MergedFX;

    [FoldoutGroup("特效")]
    [LabelText("合成后特效尺寸")]
    public float MergedFXScale = 1f;

#if UNITY_EDITOR
    /// <summary>
    /// 仅仅用于Box的Prefab编辑，以供导出成Occupation配置表，（未经旋转过的 )
    /// </summary>
    /// <returns></returns>
    public BoxOccupationData GetBoxOccupationGPs_Editor()
    {
        BoxIndicatorHelper.RefreshBoxIndicatorOccupationData();
        return BoxIndicatorHelper.BoxOccupationData;
    }

#endif

    // 旋转过的局部坐标
    public List<GridPos3D> GetBoxOccupationGPs_Rotated()
    {
        List<GridPos3D> boxOccupation_rotated = ConfigManager.GetBoxOccupationData(BoxTypeIndex).BoxIndicatorGPs_RotatedDict[BoxOrientation];
        return boxOccupation_rotated;
    }

    public bool IsBoxShapeCuboid()
    {
        return ConfigManager.GetBoxOccupationData(BoxTypeIndex).IsBoxShapeCuboid;
    }

    public BoundsInt BoxBoundsInt => BoxIndicatorHelper.BoxOccupationData.BoundsInt; // ConfigManager不能序列化这个字段，很奇怪

    public string BoxTypeName => ConfigManager.GetBoxTypeName(BoxTypeIndex);

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
        DroppingFromDeadActor,
        Dropping,
        DroppingFromAir,
    }

    public enum KickLocalAxis
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
        DropFromDeadActor,
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

    private KickLocalAxis CurrentKickLocalAxis = KickLocalAxis.X;
    private KickLocalAxis CurrentKickGlobalAxis = KickLocalAxis.X;

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
        GUID = GetGUID();
        GUID_Mod_FixedFrameRate = BoxFeature.HasFlag(BoxFeature.SlowTick) ? ((int) GUID) % ClientGameManager.Instance.FixedFrameRate_2X : ((int) GUID) % ClientGameManager.Instance.FixedFrameRate;
    }

    private void OnPlayerInteractSkillChanged(InteractSkillType interactSkillType, ushort boxTypeIndex)
    {
        if (boxTypeIndex == BoxTypeIndex)
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

    public void Setup(ushort boxTypeIndex, GridPosR.Orientation orientation)
    {
        BoxTypeIndex = boxTypeIndex;
        InitPassiveSkills();

        RawEntityStatPropSet.ApplyDataTo(EntityStatPropSet);
        EntityStatPropSet.Initialize(this);

        SwitchBoxOrientation(orientation);

        if (BattleManager.Instance.Player1) OnPlayerInteractSkillChanged(BattleManager.Instance.Player1.ActorBoxInteractHelper.GetInteractSkillType(BoxTypeIndex), BoxTypeIndex);
    }

    private void SetModelSmoothMoveLerpTime(float lerpTime)
    {
        if (lerpTime.Equals(0))
        {
            BoxIconSpriteSmoothMove.enabled = false;
            BoxModelSmoothMove.enabled = false;
            EntityFrozenHelper.IceBlockSmoothMove.enabled = false;
            if (FrozenActor)
            {
                FrozenActor.ActorFrozenHelper.IceBlockSmoothMove.enabled = false;
                FrozenActor.SetModelSmoothMoveLerpTime(0f);
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
            if (FrozenActor)
            {
                FrozenActor.EntityFrozenHelper.IceBlockSmoothMove.enabled = true;
                FrozenActor.EntityFrozenHelper.IceBlockSmoothMove.SmoothTime = lerpTime;
                FrozenActor.SetModelSmoothMoveLerpTime(lerpTime);
            }
        }
    }

    public void Initialize(GridPos3D worldGridPos3D, WorldModule module, float lerpTime, bool artOnly, LerpType lerpType, bool needLerpModel = false, bool needCheckDrop = true)
    {
        //name = $"{BoxTypeName}_{worldGridPos3D}";

        if (ENABLE_BOX_MOVE_LOG) Debug.Log($"[{Time.frameCount}] [Box] {name} Init LerpType:{lerpType} TargetGP:{worldGridPos3D}");
        SetModelSmoothMoveLerpTime(0);
        ArtOnly = artOnly;
        LastTouchActor = null;
        LastWorldGP = WorldGP;
        WorldModule = module;
        WorldGP = worldGridPos3D;
        LocalGP = module.WorldGPToLocalGP(worldGridPos3D);
        transform.parent = module.transform;
        BoxColliderHelper.Initialize(Passable, artOnly, BoxFeature.HasFlag(BoxFeature.IsGround), lerpType == LerpType.Drop, lerpTime > 0);
        if (lerpTime > 0)
        {
            if (lerpType == LerpType.Push)
            {
                BoxColliderHelper.OnPush();
            }

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
                case LerpType.DropFromDeadActor:
                {
                    State = States.DroppingFromDeadActor;
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
                WorldManager.Instance.CurrentWorld.MoveBoxColumn(WorldGP, (gp - WorldGP).Normalized(), States.BeingPushed, true, false, actor.GUID);
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
            WorldManager.Instance.CurrentWorld.MoveBoxColumn(WorldGP, moveDirection, States.Static, false, true, actor.GUID);
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
            LastTouchActor = actor;
            worldGP_WhenKicked = WorldGP;
            //WorldManager.Instance.CurrentWorld.RemoveBoxFromGrid(this); // 放在FixedUpdate里面判定如果坐标有变则移除，避免踢向墙壁时上方塌落导致bug
            transform.DOPause();
            Rigidbody = gameObject.GetComponent<Rigidbody>();
            if (!hasRigidbody)
            {
                Rigidbody = gameObject.AddComponent<Rigidbody>();
                hasRigidbody = true;
            }

            Rigidbody.mass = FinalWeight;
            Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            Rigidbody.drag = Dynamic_Drag * ConfigManager.BoxKickDragFactor_Cheat;
            Rigidbody.angularDrag = 0;
            Rigidbody.useGravity = false;
            Rigidbody.angularVelocity = Vector3.zero;
            Rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

            GridPos3D dir_GP = direction.ToGridPos3D();
            if (dir_GP.x == 0)
            {
                Rigidbody.constraints |= RigidbodyConstraints.FreezePositionX;
            }
            else
            {
                CurrentKickGlobalAxis = KickLocalAxis.X;
                if (BoxOrientation == GridPosR.Orientation.Left || BoxOrientation == GridPosR.Orientation.Right) CurrentKickLocalAxis = KickLocalAxis.Z;
                else CurrentKickLocalAxis = KickLocalAxis.X;
            }

            if (dir_GP.z == 0)
            {
                Rigidbody.constraints |= RigidbodyConstraints.FreezePositionZ;
            }
            else
            {
                CurrentKickGlobalAxis = KickLocalAxis.Z;
                if (BoxOrientation == GridPosR.Orientation.Left || BoxOrientation == GridPosR.Orientation.Right) CurrentKickLocalAxis = KickLocalAxis.X;
                else CurrentKickLocalAxis = KickLocalAxis.Z;
            }

            if (CurrentKickLocalAxis == KickLocalAxis.X && Grind_X || CurrentKickLocalAxis == KickLocalAxis.Z && Grind_Z)
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

            float kickForceMultiplier = CurrentKickLocalAxis == KickLocalAxis.X ? KickForce_X : (CurrentKickLocalAxis == KickLocalAxis.Z ? KickForce_Z : 1f);
            Rigidbody.velocity = direction.normalized * velocity * kickForceMultiplier;
            transform.position = transform.position.ToGridPos3D();
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
            LastTouchActor = actor;
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
            return true;
        }

        return false;
    }

    public void LiftThenConsume()
    {
        PlayFXOnEachGrid(CollideFX, CollideFXScale);
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

            LastTouchActor = actor;
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

            LastTouchActor = actor;
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

    public void DropFromDeadActor()
    {
        SetModelSmoothMoveLerpTime(0);
        if (BoxEffectHelper == null)
        {
            BoxEffectHelper = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.BoxEffectHelper].AllocateGameObject<BoxEffectHelper>(transform);
        }

        alreadyCollidedActorSet.Clear();
        LastTouchActor = null;
        State = States.DroppingFromDeadActor;
        transform.DOPause();
        transform.parent = WorldManager.Instance.CurrentWorld.transform;
        BoxColliderHelper.OnDropFromDeadActor();
        Rigidbody = gameObject.GetComponent<Rigidbody>();
        if (!hasRigidbody)
        {
            Rigidbody = gameObject.AddComponent<Rigidbody>();
            hasRigidbody = true;
        }

        Rigidbody.mass = FinalWeight;
        Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        Rigidbody.drag = 0;
        Rigidbody.angularDrag = 0;
        Rigidbody.useGravity = true;
        Rigidbody.velocity = Vector3.up * 10f;
        Rigidbody.angularVelocity = Vector3.zero;
        Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    public void DropFromAir()
    {
        SetModelSmoothMoveLerpTime(0);
        if (BoxEffectHelper == null)
        {
            BoxEffectHelper = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.BoxEffectHelper].AllocateGameObject<BoxEffectHelper>(transform);
        }

        alreadyCollidedActorSet.Clear();
        LastTouchActor = null;
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

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (IsRecycled) return;
        if (BoxFeature.HasFlag(BoxFeature.SlowTick)) // 减慢Tick
        {
            if (GUID_Mod_FixedFrameRate == ClientGameManager.Instance.CurrentFixedFrameCount_Mod_FixedFrameRate_2X)
            {
                EntityStatPropSet.FixedUpdate(2f);
                EntityBuffHelper.BuffFixedUpdate(2f);
                foreach (EntityPassiveSkill ps in EntityPassiveSkills)
                {
                    ps.OnTick(2f);
                }
            }
        }
        else
        {
            if (GUID_Mod_FixedFrameRate == ClientGameManager.Instance.CurrentFixedFrameCount_Mod_FixedFrameRate)
            {
                EntityStatPropSet.FixedUpdate(1f);
                EntityBuffHelper.BuffFixedUpdate(1f);
                foreach (EntityPassiveSkill ps in EntityPassiveSkills)
                {
                    ps.OnTick(1f);
                }
            }
        }

        if ((state == States.BeingKicked || state == States.BeingKickedToGrind) && IsInGridSystem)
        {
            if (transform.position.ToGridPos3D() != worldGP_WhenKicked)
            {
                WorldManager.Instance.CurrentWorld.RemoveBoxFromGrid(this);
                worldGP_WhenKicked = GridPos3D.Zero;
            }
        }

        if ((state == States.BeingKicked || state == States.BeingKickedToGrind || state == States.Flying || state == States.DroppingFromDeadActor || state == States.DroppingFromAir || state == States.Putting) && hasRigidbody)
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
                    if ((Rigidbody.constraints & RigidbodyConstraints.FreezePositionY) != 0)
                    {
                        Rigidbody.constraints -= RigidbodyConstraints.FreezePositionY;
                    }
                }
            }

            if (Rigidbody.velocity.magnitude < 1f)
            {
                LastTouchActor = null;
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
                WorldManager.Instance.CurrentWorld.BoxReturnToWorldFromPhysics(this); // 这里面已经做了“Box本来就在Grid系统里”的判定
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

        if (PassiveSkillMarkAsDestroyed)
        {
            DestroyBox();
        }
    }

    #region Collide & Trigger

    protected override void OnCollisionEnter(Collision collision)
    {
        if (IsRecycled) return;
        if (LastTouchActor.IsNotNullAndAlive() && collision.gameObject == LastTouchActor.gameObject) return; // todo 这里判定上一个碰的Actor有啥用?
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
                DealCollideDamageToActor(collision, KickLocalAxis.None);
                PlayFXOnEachGrid(CollideFX, CollideFXScale);
                foreach (EntityPassiveSkill ps in EntityPassiveSkills)
                {
                    ps.OnFlyingCollisionEnter(collision);
                }

                if (PassiveSkillMarkAsDestroyed && !IsRecycled)
                {
                    DestroyBox();
                    return;
                }

                if (!IsRecycled)
                {
                    OnFlyingCollisionEnter(collision);
                    if (PassiveSkillMarkAsDestroyed)
                    {
                        DestroyBox();
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
                    PlayFXOnEachGrid(CollideFX, CollideFXScale);
                    if (hasRigidbody) Rigidbody.velocity = Vector3.zero;
                    foreach (EntityPassiveSkill ps in EntityPassiveSkills)
                    {
                        ps.OnBeingKickedCollisionEnter(collision, CurrentKickLocalAxis);
                    }

                    if (PassiveSkillMarkAsDestroyed && !IsRecycled)
                    {
                        DestroyBox();
                        return;
                    }

                    if (!IsRecycled)
                    {
                        OnBeingKickedCollisionEnter(collision);
                        if (PassiveSkillMarkAsDestroyed)
                        {
                            DestroyBox();
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

                DealCollideDamageToActor(collision, KickLocalAxis.None);
                PlayFXOnEachGrid(CollideFX, CollideFXScale);
                foreach (EntityPassiveSkill ps in EntityPassiveSkills)
                {
                    ps.OnDroppingFromAirCollisionEnter(collision);
                }

                if (PassiveSkillMarkAsDestroyed && !IsRecycled)
                {
                    DestroyBox();
                    return;
                }

                if (!IsRecycled)
                {
                    OnDroppingFromAirCollisionEnter(collision);
                    if (PassiveSkillMarkAsDestroyed)
                    {
                        DestroyBox();
                        return;
                    }
                }

                break;
            }
        }
    }

    private bool DealCollideDamageToActor(Collision collision, KickLocalAxis kickLocalAxis)
    {
        Actor collidedActor = collision.collider.GetComponentInParent<Actor>();
        if (collidedActor == null
            || alreadyCollidedActorSet.Contains(collidedActor)
            || !collidedActor.IsNotNullAndAlive()
            || collidedActor == LastTouchActor
            || !collidedActor.IsOpponentOrNeutralCampOf(LastTouchActor)
        ) return false;
        alreadyCollidedActorSet.Add(collidedActor);

        collidedActor.ActorBattleHelper.LastAttackBox = this;
        if (collidedActor.RigidBody != null)
        {
            Vector3 nearestBoxIndicator = GridPos3D.Zero;
            float nearestDist = float.MaxValue;
            foreach (GridPos3D offset in GetBoxOccupationGPs_Rotated())
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
            force = CurrentKickGlobalAxis == KickLocalAxis.X ? new Vector3(force.x, 0, 0) : new Vector3(0, 0, force.z);

            collidedActor.RigidBody.velocity = Vector3.zero;
            Vector3 repelForce = force * (CurrentKickLocalAxis == KickLocalAxis.X ? KickRepelForce_X : KickRepelForce_Z); // 不同Local方向撞击击退力度不同
            collidedActor.RigidBody.AddForce(repelForce, ForceMode.VelocityChange);
            collidedActor.EntityBuffHelper.AddBuff(new EntityBuff_AttributeLabel {Duration = repelForce.magnitude / 30f, EntityBuffAttribute = EntityBuffAttribute.Repulse, IsPermanent = false});
            collidedActor.EntityBuffHelper.Damage(EntityStatPropSet.GetCollideDamageByAxis(kickLocalAxis).GetModifiedValue, EntityBuffAttribute.CollideDamage);
            return true;
        }

        return false;
    }

    #endregion

    public void PlayFXOnEachGrid(string fxName, float scale)
    {
        foreach (GridPos3D offset in GetBoxOccupationGPs_Rotated())
        {
            FX hit = FXManager.Instance.PlayFX(fxName, transform.position + offset);
            if (hit) hit.transform.localScale = Vector3.one * scale;
        }
    }

    #region Destroy

    public void DestroyBox(UnityAction callBack = null)
    {
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

        // 防止BoxPassiveSkills里面的效果导致箱子损坏，从而造成CollectionModified的异常。仅在OnUsed使用时InitBoxPassiveSkills清空即可
        // BoxPassiveSkills.Clear(); 
        PlayFXOnEachGrid(DestroyFX, DestroyFXScale);
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
        PlayFXOnEachGrid(MergeFX, MergeFXScale);

        ModelRoot.transform.DOShakeScale(0.2f);
        ModelRoot.transform.DOMove(mergeToWorldGP, MergeDelay * 1.2f);
        yield return new WaitForSeconds(MergeDelay);
        foreach (EntityPassiveSkill ps in EntityPassiveSkills)
        {
            ps.OnMergeBox();
        }

        // 防止BoxPassiveSkills里面的效果导致箱子损坏，从而造成CollectionModified的异常。仅在OnUsed使用时InitBoxPassiveSkills清空即可
        // BoxPassiveSkills.Clear(); 
        WorldManager.Instance.CurrentWorld.DeleteBox(this);
        callBack?.Invoke();
    }

    #endregion

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

    #region BoxExtraData

    public void ApplyBoxExtraSerializeData(Box_LevelEditor.BoxExtraSerializeData boxExtraSerializeDataFromModule = null)
    {
        if (boxExtraSerializeDataFromModule != null)
        {
            foreach (EntityPassiveSkill extraBF in boxExtraSerializeDataFromModule.BoxPassiveSkills)
            {
                EntityPassiveSkill newPS = extraBF.Clone();
                AddNewPassiveSkill(newPS, true);
            }
        }
    }

    #endregion

#if UNITY_EDITOR

    public bool RenameBoxTypeName(string srcBoxName, string targetBoxName, StringBuilder info, bool moduleSpecial = false)
    {
        bool isDirty = false;
        if (MergeBox_MatchThree == srcBoxName)
        {
            info.Append($"替换{name}.MergeBox_MatchThree -> '{targetBoxName}'\n");
            MergeBox_MatchThree = targetBoxName;
            isDirty = true;
        }

        if (MergeBox_MatchFour == srcBoxName)
        {
            info.Append($"替换{name}.MergeBox_MatchFour -> '{targetBoxName}'\n");
            MergeBox_MatchFour = targetBoxName;
            isDirty = true;
        }

        if (MergeBox_MatchFive == srcBoxName)
        {
            info.Append($"替换{name}.MergeBox_MatchFive -> '{targetBoxName}'\n");
            MergeBox_MatchFive = targetBoxName;
            isDirty = true;
        }

        foreach (EntityPassiveSkill ps in RawEntityPassiveSkills)
        {
            bool dirty = ps.RenameBoxTypeName(name, srcBoxName, targetBoxName, info, moduleSpecial);
            isDirty |= dirty;
        }

        return isDirty;
    }

    public bool DeleteBoxTypeName(string srcBoxName, StringBuilder info, bool moduleSpecial = false)
    {
        bool isDirty = false;
        if (MergeBox_MatchThree == srcBoxName)
        {
            info.Append($"替换{name}.MergeBox_MatchThree -> 'None'\n");
            MergeBox_MatchThree = "None";
            isDirty = true;
        }

        if (MergeBox_MatchFour == srcBoxName)
        {
            info.Append($"替换{name}.MergeBox_MatchFour -> 'None'\n");
            MergeBox_MatchFour = "None";
            isDirty = true;
        }

        if (MergeBox_MatchFive == srcBoxName)
        {
            info.Append($"替换{name}.MergeBox_MatchFive -> 'None'\n");
            MergeBox_MatchFive = "None";
            isDirty = true;
        }

        foreach (EntityPassiveSkill ps in RawEntityPassiveSkills)
        {
            bool dirty = ps.DeleteBoxTypeName(name, srcBoxName, info, moduleSpecial);
            isDirty |= dirty;
        }

        return isDirty;
    }

    [AssetsOnly]
    [Button("刷新关卡编辑器中该箱子的形态", ButtonSizes.Large)]
    [GUIColor(0, 1, 0)]
    public void CreateBoxLevelEditor()
    {
        GameObject box_Instance = Instantiate(gameObject); // 这是实例化一个无链接的prefab实例（unpacked completely）
        Box box = box_Instance.GetComponent<Box>();
        GameObject modelRoot = box.ModelRoot;

        GameObject boxLevelEditorPrefab = ConfigManager.FindBoxLevelEditorPrefabByName(name);
        if (boxLevelEditorPrefab)
        {
            string box_LevelEditor_Prefab_Path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(boxLevelEditorPrefab);
            GameObject box_LevelEditor_Instance = PrefabUtility.LoadPrefabContents(box_LevelEditor_Prefab_Path); // 这是实例化一个在预览场景里的prefab实例，为了能够顺利删除子GameObject

            modelRoot.transform.parent = box_LevelEditor_Instance.transform;
            Box_LevelEditor box_LevelEditor = box_LevelEditor_Instance.GetComponent<Box_LevelEditor>();
            if (box_LevelEditor.ModelRoot) DestroyImmediate(box_LevelEditor.ModelRoot);
            box_LevelEditor.ModelRoot = modelRoot;

            PrefabUtility.SaveAsPrefabAsset(box_LevelEditor_Instance, box_LevelEditor_Prefab_Path, out bool suc); // 保存回改Prefab的Asset
            DestroyImmediate(box_LevelEditor_Instance);
        }
        else
        {
            PrefabManager.Instance.LoadPrefabs(); // todo delete this line
            GameObject BoxBase_LevelEditor_Prefab = PrefabManager.Instance.GetPrefab("BoxBase_LevelEditor");
            GameObject box_LevelEditor_Instance = (GameObject) PrefabUtility.InstantiatePrefab(BoxBase_LevelEditor_Prefab); // 这是实例化一个在当前场景里的prefab实例（有链接），为了能够顺利保存成Variant

            modelRoot.transform.parent = box_LevelEditor_Instance.transform;
            Box_LevelEditor box_LevelEditor = box_LevelEditor_Instance.GetComponent<Box_LevelEditor>();
            if (box_LevelEditor.ModelRoot) DestroyImmediate(box_LevelEditor.ModelRoot);
            box_LevelEditor.ModelRoot = modelRoot;

            string box_LevelEditor_PrefabPath = ConfigManager.FindBoxLevelEditorPrefabPathByName(name); // 保存成Variant
            PrefabUtility.SaveAsPrefabAsset(box_LevelEditor_Instance, box_LevelEditor_PrefabPath, out bool suc);
            DestroyImmediate(box_LevelEditor_Instance);
        }

        DestroyImmediate(box_Instance);
    }

#endif

    public bool Pushable => BoxFeature.HasFlag(BoxFeature.Pushable);

    public bool Kickable => BoxFeature.HasFlag(BoxFeature.Kickable);

    public bool Liftable => BoxFeature.HasFlag(BoxFeature.Liftable);

    public bool KickOrThrowable => Kickable || Throwable;

    public bool Throwable => BoxFeature.HasFlag(BoxFeature.Throwable);

    public bool Interactable => Pushable || Kickable || Liftable || Throwable;

    public bool Passable => BoxFeature.HasFlag(BoxFeature.Passable);

    public bool Droppable => BoxFeature.HasFlag(BoxFeature.Droppable);

    public bool Consumable => BoxFeature.HasFlag(BoxFeature.LiftThenDisappear);
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

    [LabelText("可穿过")]
    Passable = 1 << 5,

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

    [LabelText("缓慢Tick")]
    SlowTick = 1 << 12,

    [LabelText("不受BUff影响")]
    BuffImmune = 1 << 13,
}
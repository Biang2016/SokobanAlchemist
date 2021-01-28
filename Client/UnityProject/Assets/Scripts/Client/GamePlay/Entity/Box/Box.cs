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

    internal Rigidbody Rigidbody;

    internal Actor LastTouchActor;

    [FoldoutGroup("组件")]
    public GameObject ModelRoot;

    internal override EntityBuffHelper EntityBuffHelper => boxBuffHelper;
    internal override EntityFrozenHelper EntityFrozenHelper => BoxFrozenHelper;
    internal override EntityTriggerZoneHelper EntityTriggerZoneHelper => BoxTriggerZoneHelper;

    [FoldoutGroup("组件")]
    [SerializeField]
    private EntityBuffHelper boxBuffHelper;

    [FoldoutGroup("组件")]
    public BoxFrozenHelper BoxFrozenHelper;

    internal BoxEffectHelper BoxEffectHelper;

    [FoldoutGroup("组件")]
    public BoxColliderHelper BoxColliderHelper;

    [FoldoutGroup("组件")]
    public BoxIndicatorHelper BoxIndicatorHelper;

    [FoldoutGroup("组件")]
    public EntityTriggerZoneHelper BoxTriggerZoneHelper;

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

    public override void OnUsed()
    {
        LastTouchActor = null;
        ArtOnly = true;
        EntityBuffHelper.OnHelperUsed();
        BoxFrozenHelper.OnHelperUsed();
        BoxColliderHelper.OnBoxUsed();
        BoxIndicatorHelper.OnHelperUsed();
        BoxTriggerZoneHelper?.OnHelperUsed();
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
        BoxFrozenHelper.OnHelperRecycled();
        BoxEffectHelper?.OnBoxPoolRecycled();
        BoxEffectHelper = null;
        BoxColliderHelper.OnBoxPoolRecycled();
        BoxIndicatorHelper.OnHelperRecycled();
        BoxTriggerZoneHelper?.OnHelperRecycled();
        DoorBoxHelper?.OnHelperRecycled();
        BoxSkinHelper?.OnHelperRecycled();
        BoxIconSpriteHelper?.OnHelperRecycled();
        transform.DOPause();
        ModelRoot.transform.DOPause();
        ModelRoot.transform.localPosition = Vector3.zero;
        ModelRoot.transform.localScale = Vector3.one;
        alreadyCollide = false;
        if (Rigidbody != null) Destroy(Rigidbody);
        if (LastTouchActor.IsNotNullAndAlive() && LastTouchActor.CurrentLiftBox == this)
        {
            LastTouchActor.ThrowState = Actor.ThrowStates.None;
            LastTouchActor.CurrentLiftBox = null;
            LastTouchActor = null;
        }

        EntityStatPropSet.OnRecycled();

        UnInitPassiveSkills();
        base.OnRecycled();
    }

    [HideInInspector]
    public ushort BoxTypeIndex;

    [FoldoutGroup("箱子属性")]
    [LabelText("箱子特性")]
    [AssetsOnly]
    public BoxFeature BoxFeature;

    internal GridPosR.Orientation BoxOrientation { get; private set; }

    private void SwitchBoxOrientation(GridPosR.Orientation boxOrientation)
    {
        BoxOrientation = boxOrientation;
        GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, boxOrientation), BoxColliderHelper.transform, 1);
        GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, boxOrientation), ModelRoot.transform, 1);
        GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, boxOrientation), BoxFrozenHelper.FrozeModelRoot.transform, 1);
        GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, boxOrientation), BoxIndicatorHelper.transform, 1);
        if (BoxTriggerZoneHelper) GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, boxOrientation), BoxTriggerZoneHelper.transform, 1);
    }

    [ReadOnly]
    [AssetsOnly]
    [ShowInInspector]
    [ShowIf("Interactable")]
    [FoldoutGroup("箱子属性")]
    [LabelText("重量")]
    private float Weight = 0.7f;

    [SerializeField]
    [FoldoutGroup("箱子属性")]
    [LabelText("死亡延迟")]
    private float DeleteDelay = 0;

    [SerializeField]
    [FoldoutGroup("箱子属性")]
    [LabelText("合并延迟")]
    private float MergeDelay = 0;

    public float FinalWeight => Weight * ConfigManager.BoxWeightFactor_Cheat;

    /// <summary>
    /// 抗推力
    /// </summary>
    internal static float Static_Inertia = 0.3f;

    /// <summary>
    /// 扔箱子落地Drag
    /// </summary>
    internal float Throw_Drag = 10f;

    /// <summary>
    /// 扔箱子落地摩擦力
    /// </summary>
    internal float Throw_Friction = 1;

    /// <summary>
    /// 踢箱子摩阻力
    /// </summary>
    internal float Dynamic_Drag = 0.5f;

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
        List<GridPos3D> boxOccupation_rotated = GridPos3D.TransformOccupiedPositions_XZ(BoxOrientation, ConfigManager.GetBoxOccupationData(BoxTypeIndex).BoxIndicatorGPs);
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

    [HideInEditorMode]
    public WorldModule WorldModule;

    private bool alreadyCollide;

    public enum States
    {
        Static,
        BeingPushed,
        BeingKicked,
        BeingLift,
        Lifted,
        Flying,
        Putting,
        DroppingFromDeadActor,
        Dropping,
        DroppingFromAir,
    }

    public enum LerpType
    {
        Push,
        Kick,
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

    protected virtual void Awake()
    {
        GUID = GetGUID();
        GUID_Mod_FixedFrameRate = ((int) GUID) % ClientGameManager.Instance.FixedFrameRate;
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
            BoxFrozenHelper.IceBlockSmoothMove.enabled = false;
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
            BoxFrozenHelper.IceBlockSmoothMove.enabled = true;
            BoxFrozenHelper.IceBlockSmoothMove.SmoothTime = lerpTime;
            if (FrozenActor)
            {
                FrozenActor.ActorFrozenHelper.IceBlockSmoothMove.enabled = true;
                FrozenActor.ActorFrozenHelper.IceBlockSmoothMove.SmoothTime = lerpTime;
                FrozenActor.SetModelSmoothMoveLerpTime(lerpTime);
            }
        }
    }

    public void Initialize(GridPos3D worldGridPos3D, WorldModule module, float lerpTime, bool artOnly, LerpType lerpType, bool needLerpModel = false, bool needCheckDrop = true)
    {
        name = $"{BoxTypeName}_{worldGridPos3D}";

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
        if (state == States.BeingPushed || state == States.Flying || state == States.BeingKicked || state == States.Static)
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

            alreadyCollide = false;
            LastTouchActor = actor;
            worldGP_WhenKicked = WorldGP;
            //WorldManager.Instance.CurrentWorld.RemoveBoxFromGrid(this); // 放在FixedUpdate里面判定如果坐标有变则移除，避免踢向墙壁时上方塌落导致bug
            State = States.BeingKicked;
            transform.DOPause();
            BoxColliderHelper.OnKick();
            Rigidbody = gameObject.GetComponent<Rigidbody>();
            if (Rigidbody == null) Rigidbody = gameObject.AddComponent<Rigidbody>();
            Rigidbody.mass = FinalWeight;
            Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            Rigidbody.drag = Dynamic_Drag * ConfigManager.BoxKickDragFactor_Cheat;
            Rigidbody.angularDrag = 0;
            Rigidbody.useGravity = false;
            Rigidbody.angularVelocity = Vector3.zero;
            Rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            if (direction.x.Equals(0)) Rigidbody.constraints |= RigidbodyConstraints.FreezePositionX;
            if (direction.z.Equals(0)) Rigidbody.constraints |= RigidbodyConstraints.FreezePositionZ;
            Rigidbody.velocity = direction.normalized * velocity;
            transform.position = transform.position.ToGridPos3D();
        }
    }

    private Quaternion DefaultRotBeforeLift;

    public bool BeingLift(Actor actor)
    {
        if (state == States.BeingPushed || state == States.Flying || state == States.BeingKicked || state == States.Static)
        {
            SetModelSmoothMoveLerpTime(0);
            DefaultRotBeforeLift = transform.rotation;
            alreadyCollide = false;
            LastTouchActor = actor;
            foreach (EntityPassiveSkill ps in EntityPassiveSkills)
            {
                ps.OnBeingLift(actor);
            }

            WorldManager.Instance.CurrentWorld.RemoveBoxFromGrid(this);
            State = States.BeingLift;
            transform.DOPause();
            BoxColliderHelper.OnBeingLift();
            if (Rigidbody != null)
            {
                DestroyImmediate(Rigidbody);
            }

            BoxEffectHelper?.PoolRecycle();
            BoxEffectHelper = null;
            return true;
        }

        return false;
    }

    public void LiftThenConsume()
    {
        PlayFX(CollideFX, CollideFXScale);
        PoolRecycle();
    }

    public void Throw(Vector3 direction, float velocity, Actor actor)
    {
        if (state == States.Lifted)
        {
            SetModelSmoothMoveLerpTime(0);
            alreadyCollide = false;
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
            if (Rigidbody == null) Rigidbody = gameObject.AddComponent<Rigidbody>();
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
            alreadyCollide = false;
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
            if (Rigidbody == null) Rigidbody = gameObject.AddComponent<Rigidbody>();
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

        alreadyCollide = false;
        LastTouchActor = null;
        State = States.DroppingFromDeadActor;
        transform.DOPause();
        transform.parent = WorldManager.Instance.CurrentWorld.transform;
        BoxColliderHelper.OnDropFromDeadActor();
        Rigidbody = gameObject.GetComponent<Rigidbody>();
        if (Rigidbody == null) Rigidbody = gameObject.AddComponent<Rigidbody>();
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

        alreadyCollide = false;
        LastTouchActor = null;
        State = States.DroppingFromAir;
        transform.DOPause();
        transform.parent = WorldManager.Instance.CurrentWorld.transform;
        BoxColliderHelper.OnDropFromAir();
        Rigidbody = gameObject.GetComponent<Rigidbody>();
        if (Rigidbody == null) Rigidbody = gameObject.AddComponent<Rigidbody>();
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

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (IsRecycled) return;
        if (GUID_Mod_FixedFrameRate == ClientGameManager.Instance.CurrentFixedFrameCount_Mod_FixedFrameRate)
        {
            EntityStatPropSet.FixedUpdate(1f);
            EntityBuffHelper.BuffFixedUpdate(1f);
            foreach (EntityPassiveSkill ps in EntityPassiveSkills)
            {
                ps.OnTick(1f);
            }
        }

        if (state == States.BeingKicked && IsInGridSystem)
        {
            if (transform.position.ToGridPos3D() != worldGP_WhenKicked)
            {
                WorldManager.Instance.CurrentWorld.RemoveBoxFromGrid(this);
                worldGP_WhenKicked = GridPos3D.Zero;
            }
        }

        if ((state == States.BeingKicked || state == States.Flying || state == States.DroppingFromDeadActor || state == States.DroppingFromAir || state == States.Putting) && Rigidbody)
        {
            if (state == States.BeingKicked)
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
                BoxColliderHelper.OnRigidbodyStop();
                WorldManager.Instance.CurrentWorld.BoxReturnToWorldFromPhysics(this); // 这里面已经做了“Box本来就在Grid系统里”的判定
                BoxEffectHelper?.PoolRecycle();
                BoxEffectHelper = null;
            }
        }

        if (state == States.Lifted || state == States.BeingLift)
        {
            transform.rotation = DefaultRotBeforeLift;
        }

        if (Rigidbody != null && Rigidbody.velocity.magnitude > 1f)
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

    protected override void OnCollisionEnter(Collision collision)
    {
        if (IsRecycled) return;
        if (LastTouchActor.IsNotNullAndAlive() && collision.gameObject == LastTouchActor.gameObject) return;

        if (State == States.Putting)
        {
            Box box = collision.gameObject.GetComponentInParent<Box>();
            if (box && !box.BoxFeature.HasFlag(BoxFeature.IsBorder))
            {
                Rigidbody.drag = Throw_Drag * ConfigManager.BoxThrowDragFactor_Cheat;
            }
        }

        switch (State)
        {
            case States.Flying:
            {
                if (!alreadyCollide)
                {
                    DealCollideDamageToActors(collision.contacts[0].point);
                    PlayFX(CollideFX, CollideFXScale);

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
                }

                break;
            }
            case States.BeingKicked:
            {
                if (collision.gameObject.layer != LayerManager.Instance.Layer_Ground)
                {
                    if (!alreadyCollide)
                    {
                        DealCollideDamageToActors(collision.contacts[0].point);
                        PlayFX(CollideFX, CollideFXScale);

                        foreach (EntityPassiveSkill ps in EntityPassiveSkills)
                        {
                            ps.OnBeingKickedCollisionEnter(collision);
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
                }

                break;
            }
            case States.DroppingFromAir:
            {
                Box box = collision.gameObject.GetComponentInParent<Box>();
                if (box != null && box.State == States.DroppingFromAir) return;
                if (!alreadyCollide)
                {
                    DealCollideDamageToActors(collision.contacts[0].point);
                    PlayFX(CollideFX, CollideFXScale);

                    foreach (EntityPassiveSkill ps in EntityPassiveSkills)
                    {
                        ps.OnDroppingFromAirCollisionEnter(collision);
                    }
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

    private void DealCollideDamageToActors(Vector3 contactPos)
    {
        alreadyCollide = true;

        GridPos3D nearestBoxIndicator = GridPos3D.Zero;
        float nearestDist = float.MaxValue;
        foreach (GridPos3D offset in GetBoxOccupationGPs_Rotated())
        {
            Vector3 boxIndicatorPos = transform.position + offset;
            float distToContactPos = (boxIndicatorPos - contactPos).magnitude;
            if (nearestDist > distToContactPos)
            {
                nearestDist = distToContactPos;
                nearestBoxIndicator = offset;
            }
        }

        Vector3 eachContactPosToBoxIndicatorOffset = contactPos - (transform.position + nearestBoxIndicator);

        HashSet<Actor> damagedActors = new HashSet<Actor>();
        foreach (GridPos3D offset in GetBoxOccupationGPs_Rotated())
        {
            Vector3 boxIndicatorPos = transform.position + offset;
            Vector3 boxIndicatorCollidePos = boxIndicatorPos + eachContactPosToBoxIndicatorOffset;
            Collider[] colliders = Physics.OverlapSphere(boxIndicatorCollidePos, 0.3f, LayerManager.Instance.LayerMask_HitBox_Player | LayerManager.Instance.LayerMask_HitBox_Enemy);
            foreach (Collider collider in colliders)
            {
                Actor actor = collider.GetComponentInParent<Actor>();
                if (actor && actor != LastTouchActor && !damagedActors.Contains(actor))
                {
                    if (actor.IsOpponentOrNeutralCampOf(LastTouchActor))
                    {
                        actor.ActorBattleHelper.LastAttackBox = this;
                        actor.EntityBuffHelper.Damage(EntityStatPropSet.CollideDamage.GetModifiedValue, EntityBuffAttribute.CollideDamage);
                        if (actor.RigidBody != null)
                        {
                            Vector3 force = (actor.transform.position - boxIndicatorPos).normalized;
                            force = force.GetSingleDirectionVectorXZ();
                            actor.RigidBody.velocity = Vector3.zero;
                            actor.RigidBody.AddForce(force * 10f, ForceMode.VelocityChange);
                        }

                        damagedActors.Add(actor);
                    }
                }
            }
        }
    }

    public void PlayFX(string fxName, float scale)
    {
        foreach (GridPos3D offset in GetBoxOccupationGPs_Rotated())
        {
            FX hit = FXManager.Instance.PlayFX(fxName, transform.position + offset);
            if (hit) hit.transform.localScale = Vector3.one * scale;
        }
    }

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
        PlayFX(DestroyFX, DestroyFXScale);
        WorldManager.Instance.CurrentWorld.DeleteBox(this);
        callBack?.Invoke();
    }

    public void MergeBox(GridPos3D mergeToWorldGP, UnityAction callBack = null)
    {
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
        PlayFX(MergeFX, MergeFXScale);

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

    #region BoxExtraData

    public void ApplyBoxExtraSerializeData(Box_LevelEditor.BoxExtraSerializeData boxExtraSerializeDataFromModule = null, Box_LevelEditor.BoxExtraSerializeData boxExtraSerializeDataFromWorld = null)
    {
        if (boxExtraSerializeDataFromModule != null)
        {
            foreach (EntityPassiveSkill extraBF in boxExtraSerializeDataFromModule.BoxPassiveSkills)
            {
                EntityPassiveSkill newPS = extraBF.Clone();
                AddNewPassiveSkill(newPS);
            }
        }

        if (boxExtraSerializeDataFromWorld != null)
        {
            foreach (EntityPassiveSkill extraBF in boxExtraSerializeDataFromWorld.BoxPassiveSkills)
            {
                EntityPassiveSkill newPS = extraBF.Clone();
                AddNewPassiveSkill(newPS);
            }
        }
    }

    #endregion

#if UNITY_EDITOR

    public bool RenameBoxTypeName(string srcBoxName, string targetBoxName, StringBuilder info, bool moduleSpecial = false, bool worldSpecial = false)
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
            bool dirty = ps.RenameBoxTypeName(name, srcBoxName, targetBoxName, info, moduleSpecial, worldSpecial);
            isDirty |= dirty;
        }

        return isDirty;
    }

    public bool DeleteBoxTypeName(string srcBoxName, StringBuilder info, bool moduleSpecial = false, bool worldSpecial = false)
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
            bool dirty = ps.DeleteBoxTypeName(name, srcBoxName, info, moduleSpecial, worldSpecial);
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
}
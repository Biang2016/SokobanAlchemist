using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BiangLibrary;
using BiangLibrary.GameDataFormat.Grid;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using BiangLibrary.GamePlay;
#if UNITY_EDITOR
using UnityEditor;

#endif

public partial class Box : Entity
{
    internal Rigidbody Rigidbody;

    internal Actor LastTouchActor;

    [FoldoutGroup("组件")]
    public GameObject ModelRoot;

    [FoldoutGroup("组件")]
    public BoxBuffHelper BoxBuffHelper;

    [FoldoutGroup("组件")]
    public BoxFrozenHelper BoxFrozenHelper;

    internal BoxEffectHelper BoxEffectHelper;

    [FoldoutGroup("组件")]
    public BoxColliderHelper BoxColliderHelper;

    [FoldoutGroup("组件")]
    public BoxIndicatorHelper BoxIndicatorHelper;

    [FoldoutGroup("组件")]
    public BoxThornTrapTriggerHelper BoxThornTrapTriggerHelper;

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
        BoxBuffHelper.OnHelperUsed();
        BoxFrozenHelper.OnHelperUsed();
        BoxColliderHelper.OnBoxUsed();
        BoxIndicatorHelper.OnHelperUsed();
        BoxThornTrapTriggerHelper?.OnHelperUsed();
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
        BoxBuffHelper.OnHelperRecycled();
        BoxFrozenHelper.OnHelperRecycled();
        BoxEffectHelper?.OnBoxPoolRecycled();
        BoxEffectHelper = null;
        BoxColliderHelper.OnBoxPoolRecycled();
        BoxIndicatorHelper.OnHelperRecycled();
        BoxThornTrapTriggerHelper?.OnHelperRecycled();
        DoorBoxHelper?.OnHelperRecycled();
        BoxSkinHelper?.OnHelperRecycled();
        BoxIconSpriteHelper?.OnHelperRecycled();
        transform.DOPause();
        alreadyCollide = false;
        if (Rigidbody != null) Destroy(Rigidbody);
        if (LastTouchActor != null && LastTouchActor.CurrentLiftBox == this)
        {
            LastTouchActor.ThrowState = Actor.ThrowStates.None;
            LastTouchActor.CurrentLiftBox = null;
            LastTouchActor = null;
        }

        BoxStatPropSet.OnRecycled();

        UnInitPassiveSkills();
        base.OnRecycled();
    }

    [HideInInspector]
    public ushort BoxTypeIndex;

    [FoldoutGroup("箱子属性")]
    [LabelText("箱子特性")]
    [AssetsOnly]
    public BoxFeature BoxFeature;

    [FoldoutGroup("初始战斗数值")]
    [HideLabel]
    [DisableInPlayMode]
    public BoxStatPropSet RawBoxStatPropSet; // 干数据，禁修改

    [HideInEditorMode]
    [FoldoutGroup("当前战斗数值")]
    [HideLabel]
    public BoxStatPropSet BoxStatPropSet; // 湿数据，随生命周期消亡

    internal GridPosR.Orientation BoxOrientation { get; private set; }

    private void SwitchBoxOrientation(GridPosR.Orientation boxOrientation)
    {
        BoxOrientation = boxOrientation;
        GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, boxOrientation), BoxColliderHelper.transform, 1);
        GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, boxOrientation), ModelRoot.transform, 1);
        GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, boxOrientation), BoxFrozenHelper.FrozeModelRoot.transform, 1);
        GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, boxOrientation), BoxIndicatorHelper.transform, 1);
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

    public float FinalWeight => Weight * ConfigManager.BoxWeightFactor_Cheat;

    [AssetsOnly]
    [SerializeField]
    [FoldoutGroup("碰撞")]
    [LabelText("撞击特效")]
    [ValueDropdown("GetAllFXTypeNames")]
    private string CollideFX;

    [AssetsOnly]
    [SerializeField]
    [FoldoutGroup("碰撞")]
    [LabelText("撞击特效尺寸")]
    private float CollideFXScale = 1f;

    [AssetsOnly]
    [SerializeField]
    [ShowIf("KickOrThrowable")]
    [FoldoutGroup("碰撞")]
    [LabelText("碰撞伤害半径")]
    private float CollideDamageRadius = 0.5f;

    [AssetsOnly]
    [SerializeField]
    [ShowIf("KickOrThrowable")]
    [FoldoutGroup("碰撞")]
    [LabelText("碰撞伤害")]
    [GUIColor(1.0f, 0, 1.0f)]
    private float CollideDamage = 3f;

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

    [FoldoutGroup("冻结")]
    [LabelText("解冻特效")]
    [ValueDropdown("GetAllFXTypeNames")]
    public string ThawFX;

    [FoldoutGroup("冻结")]
    [LabelText("冻结特效")]
    [ValueDropdown("GetAllFXTypeNames")]
    public string FrozeFX;

#if UNITY_EDITOR
    /// <summary>
    /// 仅仅用于Box的Prefab编辑，以供导出成Occupation配置表，（未经旋转过的 )
    /// </summary>
    /// <returns></returns>
    public List<GridPos3D> GetBoxOccupationGPs_Editor()
    {
        BoxIndicatorHelper.RefreshBoxIndicatorOccupationData();
        return BoxIndicatorHelper.BoxIndicatorGPs;
    }

#endif

    // 旋转过的局部坐标
    public List<GridPos3D> GetBoxOccupationGPs_Rotated()
    {
        List<GridPos3D> boxOccupation_transformed = GridPos3D.TransformOccupiedPositions_XZ(BoxOrientation, ConfigManager.GetBoxOccupationData(BoxTypeIndex));
        return boxOccupation_transformed;
    }

    public BoundsInt BoxBoundsInt => GetBoxOccupationGPs_Rotated().GetBoundingRectFromListGridPos(WorldGP);

    #region 箱子被动技能

    [SerializeReference]
    [FoldoutGroup("箱子被动技能")]
    [LabelText("箱子被动技能")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<BoxPassiveSkill> RawBoxPassiveSkills = new List<BoxPassiveSkill>(); // 干数据，禁修改

    [HideInInspector]
    public List<BoxPassiveSkill> BoxPassiveSkills = new List<BoxPassiveSkill>(); // 湿数据，每个Box生命周期开始前从干数据拷出，结束后清除

    internal bool BoxPassiveSkillMarkAsDeleted = false;

    private void InitBoxPassiveSkills()
    {
        BoxPassiveSkills.Clear();
        foreach (BoxPassiveSkill rawBF in RawBoxPassiveSkills)
        {
            if (rawBF is BoxPassiveSkill_LevelEventTriggerAppear) continue;
            BoxPassiveSkills.Add(rawBF.Clone());
        }

        BoxPassiveSkillMarkAsDeleted = false;
        foreach (BoxPassiveSkill bf in BoxPassiveSkills)
        {
            AddNewPassiveSkill(bf);
        }
    }

    public void AddNewPassiveSkill(BoxPassiveSkill bf)
    {
        bf.Box = this;
        bf.OnInit();
        bf.OnRegisterLevelEventID();
    }

    private void UnInitPassiveSkills()
    {
        foreach (BoxPassiveSkill bf in BoxPassiveSkills)
        {
            bf.OnUnRegisterLevelEventID();
        }

        // 防止BoxPassiveSkills里面的效果导致箱子损坏，从而造成CollectionModified的异常。仅在OnUsed使用时InitBoxPassiveSkills清空即可
        //BoxPassiveSkills.Clear();
        BoxPassiveSkillMarkAsDeleted = false;
    }

    #endregion

    internal GridPos3D LastWorldGP;

    [HideInEditorMode]
    public GridPos3D WorldGP;

    private GridPos3D worldGP_WhenKicked = GridPos3D.Zero;

    internal bool IsInGridSystem;

    [HideInEditorMode]
    public GridPos3D LocalGP;

    [HideInEditorMode]
    public WorldModule WorldModule;

    private bool alreadyCollide;

    public enum States
    {
        Static,
        BeingPushed,
        PushingCanceling,
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
        InitBoxPassiveSkills();

        RawBoxStatPropSet.ApplyDataTo(BoxStatPropSet);
        BoxStatPropSet.Initialize(this);

        SwitchBoxOrientation(orientation);

        if (BattleManager.Instance.Player1) OnPlayerInteractSkillChanged(BattleManager.Instance.Player1.ActorSkillHelper.GetInteractSkillType(BoxTypeIndex), BoxTypeIndex);
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

    public void Initialize(GridPos3D localGridPos3D, WorldModule module, float lerpTime, bool artOnly, LerpType lerpType, bool needLerpModel = false, bool needCheckDrop = true)
    {
        SetModelSmoothMoveLerpTime(0);
        ArtOnly = artOnly;
        LastTouchActor = null;
        LastWorldGP = WorldGP;
        WorldModule = module;
        WorldGP = module.LocalGPToWorldGP(localGridPos3D);
        LocalGP = localGridPos3D;
        transform.parent = module.transform;
        BoxColliderHelper.Initialize(Passable, artOnly, BoxFeature.HasFlag(BoxFeature.IsGround), lerpType == LerpType.Drop, lerpTime > 0);
        if (lerpTime > 0)
        {
            if (lerpType == LerpType.Push)
            {
                BoxColliderHelper.OnPush();
            }

            transform.DOPause();
            transform.DOLocalMove(localGridPos3D, lerpTime).SetEase(Ease.Linear).OnComplete(() =>
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
            transform.localPosition = localGridPos3D;
            transform.localRotation = Quaternion.identity;
        }

        if (needCheckDrop) WorldManager.Instance.CurrentWorld.CheckDropSelf(this);
    }

    public void Push(Vector3 direction)
    {
        if (state == States.Static || state == States.PushingCanceling)
        {
            SetModelSmoothMoveLerpTime(0);
            Vector3 targetPos = WorldGP + direction.normalized;
            GridPos3D gp = GridPos3D.GetGridPosByPoint(targetPos, 1);
            if (gp != WorldGP)
            {
                if (Actor.ENABLE_ACTOR_MOVE_LOG) Debug.Log($"[Box] {name} Push {WorldGP} -> {gp}");
                WorldManager.Instance.CurrentWorld.MoveBoxColumn(WorldGP, (gp - WorldGP).Normalized(), States.BeingPushed);
            }
        }
    }

    public void PushCanceled()
    {
        if (state == States.BeingPushed)
        {
            if ((transform.localPosition - LocalGP).magnitude > (1 - Static_Inertia))
            {
                SetModelSmoothMoveLerpTime(0);
                if (Actor.ENABLE_ACTOR_MOVE_LOG) Debug.Log($"[Box] {name} PushCanceled {WorldGP} -> {LastWorldGP}");
                WorldManager.Instance.CurrentWorld.MoveBoxColumn(WorldGP, (LastWorldGP - WorldGP).Normalized(), States.PushingCanceling);
            }
        }
    }

    public void ForceStopWhenSwapBox()
    {
        GridPos3D targetGP = transform.position.ToGridPos3D();
        GridPos3D moveDirection = (targetGP - WorldGP).Normalized();
        WorldManager.Instance.CurrentWorld.BoxColumnTransformDOPause(WorldGP, moveDirection);
        if (Actor.ENABLE_ACTOR_MOVE_LOG) Debug.Log($"[Box] {name} ForceCancelPush {WorldGP} -> {targetGP}");
        WorldManager.Instance.CurrentWorld.MoveBoxColumn(WorldGP, moveDirection, States.Static, false, true);
    }

    public void Kick(Vector3 direction, float velocity, Actor actor)
    {
        if (state == States.BeingPushed || state == States.Flying || state == States.BeingKicked || state == States.Static || state == States.PushingCanceling)
        {
            SetModelSmoothMoveLerpTime(0);
            if (BoxEffectHelper == null)
            {
                BoxEffectHelper = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.BoxEffectHelper].AllocateGameObject<BoxEffectHelper>(transform);
            }

            foreach (BoxPassiveSkill bf in BoxPassiveSkills)
            {
                bf.OnBeingKicked(actor);
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
        if (state == States.BeingPushed || state == States.Flying || state == States.BeingKicked || state == States.Static || state == States.PushingCanceling)
        {
            SetModelSmoothMoveLerpTime(0);
            DefaultRotBeforeLift = transform.rotation;
            alreadyCollide = false;
            LastTouchActor = actor;
            foreach (BoxPassiveSkill bf in BoxPassiveSkills)
            {
                bf.OnBeingLift(actor);
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
        PlayCollideFX();
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
            BoxStatPropSet.FixedUpdate(1f);
            BoxBuffHelper.BuffFixedUpdate();
            foreach (BoxPassiveSkill boxPassiveSkill in BoxPassiveSkills)
            {
                boxPassiveSkill.OnTick(1f);
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

        if (BoxPassiveSkillMarkAsDeleted)
        {
            DestroyBox();
        }
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        if (IsRecycled) return;
        if (LastTouchActor != null && collision.gameObject == LastTouchActor.gameObject) return;

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
                    CollideAOEDamage(CollideDamageRadius, CollideDamage);
                    PlayCollideFX();

                    foreach (BoxPassiveSkill bf in BoxPassiveSkills)
                    {
                        bf.OnFlyingCollisionEnter(collision);
                    }

                    if (BoxPassiveSkillMarkAsDeleted && !IsRecycled)
                    {
                        DestroyBox();
                        return;
                    }

                    if (!IsRecycled)
                    {
                        OnFlyingCollisionEnter(collision);
                        if (BoxPassiveSkillMarkAsDeleted)
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
                        CollideAOEDamage(CollideDamageRadius, CollideDamage);
                        PlayCollideFX();

                        foreach (BoxPassiveSkill bf in BoxPassiveSkills)
                        {
                            bf.OnBeingKickedCollisionEnter(collision);
                        }

                        if (BoxPassiveSkillMarkAsDeleted && !IsRecycled)
                        {
                            DestroyBox();
                            return;
                        }

                        if (!IsRecycled)
                        {
                            OnBeingKickedCollisionEnter(collision);
                            if (BoxPassiveSkillMarkAsDeleted)
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
                    CollideAOEDamage(CollideDamageRadius, CollideDamage);
                    PlayCollideFX();

                    foreach (BoxPassiveSkill bf in BoxPassiveSkills)
                    {
                        bf.OnDroppingFromAirCollisionEnter(collision);
                    }
                }

                if (BoxPassiveSkillMarkAsDeleted && !IsRecycled)
                {
                    DestroyBox();
                    return;
                }

                if (!IsRecycled)
                {
                    OnDroppingFromAirCollisionEnter(collision);
                    if (BoxPassiveSkillMarkAsDeleted)
                    {
                        DestroyBox();
                        return;
                    }
                }

                break;
            }
        }
    }

    private void CollideAOEDamage(float radius, float damage)
    {
        alreadyCollide = true;
        WorldFeature wf = WorldManager.Instance.CurrentWorld.WorldData.WorldFeature;
        bool playerImmune = wf.HasFlag(WorldFeature.PlayerImmune);
        bool pvp = wf.HasFlag(WorldFeature.PVP);
        if (!playerImmune)
        {
            HashSet<Actor> damagedActors = new HashSet<Actor>();
            foreach (GridPos3D offset in GetBoxOccupationGPs_Rotated())
            {
                Vector3 boxIndicatorPos = transform.position + offset;
                Collider[] colliders = Physics.OverlapSphere(boxIndicatorPos, radius, LayerManager.Instance.LayerMask_HitBox_Player | LayerManager.Instance.LayerMask_HitBox_Enemy);
                foreach (Collider collider in colliders)
                {
                    Actor actor = collider.GetComponentInParent<Actor>();
                    if (actor && actor != LastTouchActor && actor.ActorBattleHelper && !damagedActors.Contains(actor))
                    {
                        if (actor.IsOpponentCampOf(LastTouchActor) || (pvp && actor.IsPlayer && LastTouchActor.IsPlayer))
                        {
                            actor.ActorBattleHelper.LastAttackBox = this;
                            actor.ActorBattleHelper.Damage(LastTouchActor, damage);
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
    }

    public void PlayCollideFX()
    {
        foreach (GridPos3D offset in GetBoxOccupationGPs_Rotated())
        {
            FX hit = FXManager.Instance.PlayFX(CollideFX, transform.position + offset);
            if (hit) hit.transform.localScale = Vector3.one * CollideFXScale;
        }
    }

    public void DestroyBox()
    {
        foreach (BoxPassiveSkill bf in BoxPassiveSkills)
        {
            bf.OnBeforeDestroyBox();
        }

        StartCoroutine(Co_DelayDestroyBox());
    }

    IEnumerator Co_DelayDestroyBox()
    {
        yield return new WaitForSeconds(DeleteDelay);
        foreach (BoxPassiveSkill bf in BoxPassiveSkills)
        {
            bf.OnDestroyBox();
        }

        // 防止BoxPassiveSkills里面的效果导致箱子损坏，从而造成CollectionModified的异常。仅在OnUsed使用时InitBoxPassiveSkills清空即可
        // BoxPassiveSkills.Clear(); 
        WorldManager.Instance.CurrentWorld.DeleteBox(this);
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
            foreach (BoxPassiveSkill extraBF in boxExtraSerializeDataFromModule.BoxPassiveSkills)
            {
                BoxPassiveSkill newPS = extraBF.Clone();
                newPS.Box = this;
                RawBoxPassiveSkills.Add(newPS);
            }
        }

        if (boxExtraSerializeDataFromWorld != null)
        {
            foreach (BoxPassiveSkill extraBF in boxExtraSerializeDataFromWorld.BoxPassiveSkills)
            {
                BoxPassiveSkill newPS = extraBF.Clone();
                newPS.Box = this;
                RawBoxPassiveSkills.Add(newPS);
            }
        }
    }

    #endregion

#if UNITY_EDITOR

    public bool RenameBoxTypeName(string srcBoxName, string targetBoxName, StringBuilder info, bool moduleSpecial = false, bool worldSpecial = false)
    {
        bool isDirty = false;
        foreach (BoxPassiveSkill bf in RawBoxPassiveSkills)
        {
            bool dirty = bf.RenameBoxTypeName(name, srcBoxName, targetBoxName, info, moduleSpecial, worldSpecial);
            isDirty |= dirty;
        }

        return isDirty;
    }

    public bool DeleteBoxTypeName(string srcBoxName, StringBuilder info, bool moduleSpecial = false, bool worldSpecial = false)
    {
        bool isDirty = false;

        foreach (BoxPassiveSkill bf in RawBoxPassiveSkills)
        {
            bool dirty = bf.DeleteBoxTypeName(name, srcBoxName, info, moduleSpecial, worldSpecial);
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
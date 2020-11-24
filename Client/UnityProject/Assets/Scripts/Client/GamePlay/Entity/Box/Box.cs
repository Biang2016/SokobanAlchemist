using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BiangStudio;
using BiangStudio.CloneVariant;
using BiangStudio.GameDataFormat.Grid;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;

#endif

public partial class Box : Entity
{
    private BoxUnderWorldModuleDesignerClamper BoxUnderWorldModuleDesignerClamper;
    private GridSnapper GridSnapper;

    internal Rigidbody Rigidbody;

    internal Actor LastTouchActor;

    [FoldoutGroup("组件")]
    public BoxBuffHelper BoxBuffHelper;

    [FoldoutGroup("组件")]
    public BoxFrozenHelper BoxFrozenHelper;

    internal BoxEffectHelper BoxEffectHelper;

    [FoldoutGroup("组件")]
    public BoxColliderHelper BoxColliderHelper;

    [FoldoutGroup("组件")]
    public BoxThornTrapTriggerHelper BoxThornTrapTriggerHelper;

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
        BoxColliderHelper = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.BoxColliderHelper].AllocateGameObject<BoxColliderHelper>(transform);
        BoxColliderHelper.Box = this;

        BoxBuffHelper.OnHelperUsed();
        BoxFrozenHelper.OnHelperUsed();
        BoxColliderHelper.OnBoxUsed();
        BoxThornTrapTriggerHelper?.OnHelperUsed();
        BoxSkinHelper?.OnHelperUsed();
        BoxIconSpriteHelper.OnHelperUsed();

        base.OnUsed();
    }

    public override void OnRecycled()
    {
        ArtOnly = true;
        WorldModule = null;
        WorldGP = GridPos3D.Zero;
        LastWorldGP = GridPos3D.Zero;
        LastState = States.Static;
        State = States.Static;
        BoxBuffHelper.OnHelperRecycled();
        BoxFrozenHelper.OnHelperRecycled();
        BoxEffectHelper?.OnBoxPoolRecycled();
        BoxEffectHelper = null;
        BoxColliderHelper.OnBoxPoolRecycled();
        BoxColliderHelper = null;
        BoxThornTrapTriggerHelper?.OnHelperRecycled();
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
        BoxStatPropSet = null;

        UnRegisterEvents();
        UnInitPassiveSkills();
        base.OnRecycled();
    }

    [HideInInspector]
    public ushort BoxTypeIndex;

    [FoldoutGroup("箱子属性")]
    [LabelText("箱子特性")]
    [AssetsOnly]
    public BoxFeature BoxFeature;

    [FoldoutGroup("箱子属性")]
    [LabelText("箱子形状")]
    [AssetsOnly]
    [OnValueChanged("SwitchBoxShapeType")]
    public BoxShapeType BoxShapeType;

    [FoldoutGroup("初始战斗数值")]
    [HideLabel]
    [DisableInPlayMode]
    public BoxStatPropSet RawBoxStatPropSet; // 干数据，禁修改

    [HideInEditorMode]
    [FoldoutGroup("当前战斗数值")]
    [HideLabel]
    public BoxStatPropSet BoxStatPropSet; // 湿数据，随生命周期消亡

    private void SwitchBoxShapeType()
    {
        if (BoxShapeType == BoxShapeType.Box)
        {
            BoxSkinHelper?.ResetBoxOrientation();
            BoxColliderHelper?.ResetBoxOrientation();
            BoxOrientation = GridPosR.Orientation.Up;
        }

        BoxSkinHelper?.RefreshBoxShapeType();
        BoxColliderHelper?.SwitchBoxShapeType();
    }

    [LabelText("箱子朝向")]
    [HideIf("BoxShapeType", BoxShapeType.Box)]
    [OnValueChanged("SwitchBoxOrientation")]
    [EnumToggleButtons]
    public GridPosR.Orientation BoxOrientation;

    private void SwitchBoxOrientation()
    {
        BoxSkinHelper?.SwitchBoxOrientation();
        BoxColliderHelper?.SwitchBoxOrientation();
    }

    [ReadOnly]
    [AssetsOnly]
    [ShowInInspector]
    [ShowIf("Interactable")]
    [FoldoutGroup("箱子属性")]
    [LabelText("重量")]
    private float Weight = 0.7f;

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

    [ReadOnly]
    [AssetsOnly]
    [ShowInInspector]
    [ShowIf("Pushable")]
    [FoldoutGroup("推箱子属性")]
    [LabelText("抗推力")]
    internal static float Static_Inertia = 0.3f;

    [ReadOnly]
    [AssetsOnly]
    [ShowInInspector]
    [ShowIf("Throwable")]
    [FoldoutGroup("扔箱子属性")]
    [LabelText("落地Drag")]
    internal float Throw_Drag = 10f;

    [ReadOnly]
    [AssetsOnly]
    [ShowInInspector]
    [ShowIf("Throwable")]
    [FoldoutGroup("扔箱子属性")]
    [LabelText("落地摩擦力")]
    internal float Throw_Friction = 1;

    [ReadOnly]
    [AssetsOnly]
    [ShowInInspector]
    [ShowIf("Kickable")]
    [FoldoutGroup("踢箱子属性")]
    [LabelText("摩阻力")]
    internal float Dynamic_Drag = 0.5f;

    [FoldoutGroup("冻结")]
    [LabelText("解冻特效")]
    [ValueDropdown("GetAllFXTypeNames")]
    public string ThawFX;

    [FoldoutGroup("冻结")]
    [LabelText("冻结特效")]
    [ValueDropdown("GetAllFXTypeNames")]
    public string FrozeFX;

    #region 箱子被动技能

    [NonSerialized]
    [ShowInInspector]
    [FoldoutGroup("箱子被动技能")]
    [LabelText("箱子被动技能")]
    [FormerlySerializedAs("RawBoxFunctions")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<BoxPassiveSkill> RawBoxPassiveSkills = new List<BoxPassiveSkill>(); // 干数据，禁修改

    [FormerlySerializedAs("BoxFunctions")]
    public List<BoxPassiveSkill> BoxPassiveSkills = new List<BoxPassiveSkill>(); // 湿数据，每个Box生命周期开始前从干数据拷出，结束后清除

    [FormerlySerializedAs("BoxFunctionDict")]
    public Dictionary<string, BoxPassiveSkill> BoxPassiveSkillDict = new Dictionary<string, BoxPassiveSkill>(); // 便于寻找

    internal bool BoxPassiveSkillMarkAsDeleted = false;

    [HideInInspector]
    [FormerlySerializedAs("BoxFunctionBaseData")]
    public byte[] BoxPassiveSkillBaseData;

    public override void OnBeforeSerialize()
    {
        base.OnBeforeSerialize();
        if (RawBoxPassiveSkills == null) RawBoxPassiveSkills = new List<BoxPassiveSkill>();
        BoxPassiveSkillBaseData = SerializationUtility.SerializeValue(RawBoxPassiveSkills, DataFormat.JSON);
    }

    public override void OnAfterDeserialize()
    {
        base.OnAfterDeserialize();
        RawBoxPassiveSkills = SerializationUtility.DeserializeValue<List<BoxPassiveSkill>>(BoxPassiveSkillBaseData, DataFormat.JSON);
    }

    private void InitBoxPassiveSkills()
    {
        BoxPassiveSkills.Clear();
        BoxPassiveSkillDict.Clear();
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
        string bfName = bf.GetType().Name;
        if (!BoxPassiveSkillDict.ContainsKey(bfName))
        {
            BoxPassiveSkillDict.Add(bfName, bf);
        }
    }

    private void UnInitPassiveSkills()
    {
        foreach (BoxPassiveSkill bf in BoxPassiveSkills)
        {
            bf.OnUnRegisterLevelEventID();
        }

        // 防止BoxPassiveSkills里面的效果导致箱子损坏，从而造成CollectionModified的异常。仅在使用时清空即可
        //BoxPassiveSkills.Clear();
        //BoxPassiveSkillDict.Clear();
        BoxPassiveSkillMarkAsDeleted = false;
    }

    #endregion

    internal GridPos3D LastWorldGP;

    [HideInEditorMode]
    public GridPos3D WorldGP;

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
    }

    public enum LerpType
    {
        Push,
        Kick,
        Throw,
        Put,
        Drop,
        DropFromDeadActor,
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
        GridSnapper = GetComponent<GridSnapper>();
        BoxUnderWorldModuleDesignerClamper = GetComponent<BoxUnderWorldModuleDesignerClamper>();
        BoxUnderWorldModuleDesignerClamper.enabled = !Application.isPlaying;
        GridSnapper.enabled = false;
        GUID = GetGUID();
        GUID_Mod_FixedFrameRate = ((int) GUID) % ClientGameManager.Instance.FixedFrameRate;
    }

    private void RegisterEvents()
    {
        ClientGameManager.Instance.BattleMessenger.AddListener<InteractSkillType, ushort>((uint) Enum_Events.OnPlayerInteractSkillChanged, OnPlayerInteractSkillChanged);
    }

    private void UnRegisterEvents()
    {
        ClientGameManager.Instance.BattleMessenger.RemoveListener<InteractSkillType, ushort>((uint) Enum_Events.OnPlayerInteractSkillChanged, OnPlayerInteractSkillChanged);
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

    public void Setup(ushort boxTypeIndex)
    {
        BoxTypeIndex = boxTypeIndex;
        InitBoxPassiveSkills();
        RegisterEvents();

        BoxStatPropSet = RawBoxStatPropSet.Clone();
        BoxStatPropSet.Initialize(this);

        foreach (BoxPassiveSkill bf in BoxPassiveSkills)
        {
            if (bf is BoxPassiveSkill_ShapeAndOrientation bf_so)
            {
                BoxShapeType = bf_so.BoxShapeType;
                BoxOrientation = bf_so.Orientation;
            }
        }

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

    public void Initialize(GridPos3D localGridPos3D, WorldModule module, float lerpTime, bool artOnly, LerpType lerpType, bool needLerpModel = false)
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
        SwitchBoxOrientation();
        SwitchBoxShapeType();
        if (lerpTime > 0)
        {
            if (lerpType == LerpType.Push)
            {
                BoxColliderHelper.OnPush();
            }

            transform.DOPause();
            transform.DOLocalMove(localGridPos3D.ToVector3(), lerpTime).SetEase(Ease.Linear).OnComplete(() =>
            {
                State = States.Static;
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
                case LerpType.DropFromDeadActor:
                {
                    State = States.DroppingFromDeadActor;
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
            if (needLerpModel) SetModelSmoothMoveLerpTime(0.2f);
            transform.localPosition = localGridPos3D.ToVector3();
            transform.localRotation = Quaternion.identity;
            State = States.Static;
        }

        WorldManager.Instance.CurrentWorld.CheckDropSelf(this);
    }

    public void Push(Vector3 direction)
    {
        if (state == States.Static || state == States.PushingCanceling)
        {
            SetModelSmoothMoveLerpTime(0);
            Vector3 targetPos = WorldGP.ToVector3() + direction.normalized;
            GridPos3D gp = GridPos3D.GetGridPosByPoint(targetPos, 1);
            if (gp != WorldGP)
            {
                if (Actor.ENABLE_ACTOR_MOVE_LOG) Debug.Log($"[Box] {name} Push {WorldGP} -> {gp}");
                WorldManager.Instance.CurrentWorld.MoveBox(WorldGP, gp, States.BeingPushed);
            }
        }
    }

    public void PushCanceled()
    {
        if (state == States.BeingPushed)
        {
            if ((transform.localPosition - LocalGP.ToVector3()).magnitude > (1 - Static_Inertia))
            {
                SetModelSmoothMoveLerpTime(0);
                if (Actor.ENABLE_ACTOR_MOVE_LOG) Debug.Log($"[Box] {name} PushCanceled {WorldGP} -> {LastWorldGP}");
                WorldManager.Instance.CurrentWorld.MoveBox(WorldGP, LastWorldGP, States.PushingCanceling);
            }
        }
    }

    public void ForceStop()
    {
        transform.DOPause();
        GridPos3D targetGP = transform.position.ToGridPos3D();
        if (Actor.ENABLE_ACTOR_MOVE_LOG) Debug.Log($"[Box] {name} ForceCancelPush {WorldGP} -> {targetGP}");
        WorldManager.Instance.CurrentWorld.MoveBox(WorldGP, targetGP, States.Static, false, true);
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

            alreadyCollide = false;
            LastTouchActor = actor;
            WorldManager.Instance.CurrentWorld.RemoveBoxFromGrid(this);
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
            transform.position = transform.position.ToGridPos3D().ToVector3();
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

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (IsRecycled) return;
        if (GUID_Mod_FixedFrameRate == ClientGameManager.Instance.CurrentFixedFrameCount_Mod_FixedFrameRate)
        {
            BoxStatPropSet.FixedUpdate(1f);
            BoxBuffHelper.BuffFixedUpdate();
        }

        if ((state == States.BeingKicked || state == States.Flying || state == States.DroppingFromDeadActor || state == States.Putting) && Rigidbody)
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
                WorldManager.Instance.CurrentWorld.BoxReturnToWorldFromPhysics(this);
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
            DeleteSelf();
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

        if (State == States.Flying)
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
                    DeleteSelf();
                    return;
                }

                if (!IsRecycled)
                {
                    OnFlyingCollisionEnter(collision);
                    if (BoxPassiveSkillMarkAsDeleted)
                    {
                        DeleteSelf();
                        return;
                    }
                }
            }
        }

        if (State == States.BeingKicked)
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
                        DeleteSelf();
                        return;
                    }

                    if (!IsRecycled)
                    {
                        OnBeingKickedCollisionEnter(collision);
                        if (BoxPassiveSkillMarkAsDeleted)
                        {
                            DeleteSelf();
                            return;
                        }
                    }
                }
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
            Collider[] colliders = Physics.OverlapSphere(transform.position, radius, LayerManager.Instance.LayerMask_HitBox_Player | LayerManager.Instance.LayerMask_HitBox_Enemy);
            HashSet<Actor> damagedActors = new HashSet<Actor>();
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
                            Vector3 force = (actor.transform.position - transform.position).normalized;
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

    public void PlayCollideFX()
    {
        FX hit = FXManager.Instance.PlayFX(CollideFX, transform.position);
        if (hit) hit.transform.localScale = Vector3.one * CollideFXScale;
    }

    public void DeleteSelf()
    {
        foreach (BoxPassiveSkill bf in BoxPassiveSkills)
        {
            bf.OnDeleteBox();
        }

        //BoxPassiveSkills.Clear();
        //BoxPassiveSkillDict.Clear();
        WorldManager.Instance.CurrentWorld.DeleteBox(this);
    }

#if UNITY_EDITOR

    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            if (RequireSerializeFunctionIntoWorldModule)
            {
                transform.DrawSpecialTip(Vector3.left * 0.5f, "#0AFFF1".HTMLColorToColor(), Color.cyan, "模特");
            }

            if (RequireHideInWorldForModuleBox)
            {
                transform.DrawSpecialTip(Vector3.left * 0.5f, "#FF8000".HTMLColorToColor(), Color.yellow, "世隐");
            }
            else if (RequireSerializeFunctionIntoWorld || IsUnderWorldSpecialBoxesRoot)
            {
                transform.DrawSpecialTip(Vector3.left * 0.5f, "#FF8000".HTMLColorToColor(), Color.yellow, "世特");
            }

            if (LevelEventTriggerAppearInWorldModule)
            {
                transform.DrawSpecialTip(Vector3.left * 0.5f + Vector3.forward * 0.5f, Color.clear, "#B30AFF".HTMLColorToColor(), "模预隐");
            }
            else if (LevelEventTriggerAppearInWorld)
            {
                transform.DrawSpecialTip(Vector3.left * 0.5f + Vector3.forward * 0.5f, Color.clear, "#FF0A69".HTMLColorToColor(), "世预隐");
            }
        }
    }

    private bool IsUnderWorldSpecialBoxesRoot = false;

    void OnTransformParentChanged()
    {
        RefreshIsUnderWorldSpecialBoxesRoot();
    }

    internal void RefreshIsUnderWorldSpecialBoxesRoot()
    {
        if (!Application.isPlaying)
        {
            IsUnderWorldSpecialBoxesRoot = transform.HasAncestorName($"@_{WorldHierarchyRootType.WorldSpecialBoxesRoot}");
        }
    }
#endif

#if UNITY_EDITOR
    [HideInPlayMode]
    [HideInPrefabAssets]
    [FoldoutGroup("模组编辑器")]
    [Button("替换Box", ButtonSizes.Large)]
    [GUIColor(0f, 1f, 1f)]
    private void ReplaceBox_Editor()
    {
        WorldModuleDesignHelper module = GetComponentInParent<WorldModuleDesignHelper>();
        if (!module)
        {
            Debug.LogError("此功能只能在模组编辑器中使用");
            return;
        }

        WorldDesignHelper world = GetComponentInParent<WorldDesignHelper>();
        if (world)
        {
            Debug.LogError("此功能只能在模组编辑器中使用");
            return;
        }

        GameObject prefab = (GameObject) Resources.Load("Prefabs/Designs/Box/" + ReplaceBoxTypeName);
        GameObject go = (GameObject) PrefabUtility.InstantiatePrefab(prefab, transform.parent);
        go.transform.position = transform.position;
        go.transform.rotation = Quaternion.identity;
        DestroyImmediate(gameObject);
    }

    [HideInPlayMode]
    [HideInPrefabAssets]
    [ShowInInspector]
    [NonSerialized]
    [FoldoutGroup("模组编辑器")]
    [LabelText("替换Box类型")]
    [ValueDropdown("GetAllBoxTypeNames")]
    private string ReplaceBoxTypeName = "None";

    public bool RenameBoxTypeName(string srcBoxName, string targetBoxName, StringBuilder info, bool moduleSpecial = false, bool worldSpecial = false)
    {
        bool isDirty = false;
        foreach (BoxPassiveSkill bf in RawBoxPassiveSkills)
        {
            if (moduleSpecial && bf.SpecialCaseType != BoxPassiveSkill.BoxPassiveSkillBaseSpecialCaseType.Module) continue;
            if (worldSpecial && bf.SpecialCaseType != BoxPassiveSkill.BoxPassiveSkillBaseSpecialCaseType.World) continue;

            foreach (FieldInfo fi in bf.GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
            {
                foreach (Attribute a in fi.GetCustomAttributes(false))
                {
                    if (a is BoxNameAttribute)
                    {
                        if (fi.FieldType == typeof(string))
                        {
                            string fieldValue = (string) fi.GetValue(bf);
                            if (fieldValue == srcBoxName)
                            {
                                info.Append($"替换{name}.BoxPassiveSkills.{bf.GetType().Name}.{fi.Name} -> '{targetBoxName}'\n");
                                fi.SetValue(bf, targetBoxName);
                                isDirty = true;
                            }
                        }
                    }
                    else if (a is BoxNameListAttribute)
                    {
                        if (fi.FieldType == typeof(List<string>))
                        {
                            List<string> fieldValueList = (List<string>) fi.GetValue(bf);
                            for (int i = 0; i < fieldValueList.Count; i++)
                            {
                                string fieldValue = fieldValueList[i];
                                if (fieldValue == srcBoxName)
                                {
                                    info.Append($"替换于{name}.PassiveSkills.{bf.GetType().Name}.{fi.Name}\n");
                                    fieldValueList[i] = targetBoxName;
                                    isDirty = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        return isDirty;
    }

    public bool DeleteBoxTypeName(string srcBoxName, StringBuilder info, bool moduleSpecial = false, bool worldSpecial = false)
    {
        bool isDirty = false;
        foreach (BoxPassiveSkill bf in RawBoxPassiveSkills)
        {
            if (moduleSpecial && bf.SpecialCaseType != BoxPassiveSkill.BoxPassiveSkillBaseSpecialCaseType.Module) continue;
            if (worldSpecial && bf.SpecialCaseType != BoxPassiveSkill.BoxPassiveSkillBaseSpecialCaseType.World) continue;

            foreach (FieldInfo fi in bf.GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
            {
                foreach (Attribute a in fi.GetCustomAttributes(false))
                {
                    if (a is BoxNameAttribute)
                    {
                        if (fi.FieldType == typeof(string))
                        {
                            string fieldValue = (string) fi.GetValue(bf);
                            if (fieldValue == srcBoxName)
                            {
                                info.Append($"替换{name}.BoxPassiveSkills.{bf.GetType().Name}.{fi.Name} -> 'None'\n");
                                fi.SetValue(bf, "None");
                                isDirty = true;
                            }
                        }
                    }
                    else if (a is BoxNameListAttribute)
                    {
                        if (fi.FieldType == typeof(List<string>))
                        {
                            List<string> fieldValueList = (List<string>) fi.GetValue(bf);
                            for (int i = 0; i < fieldValueList.Count; i++)
                            {
                                string fieldValue = fieldValueList[i];
                                if (fieldValue == srcBoxName)
                                {
                                    info.Append($"移除自{name}.PassiveSkills.{bf.GetType().Name}.{fi.Name}\n");
                                    fieldValueList.RemoveAt(i);
                                    i--;
                                    isDirty = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        return isDirty;
    }

#endif

    #region ExtraSerialize

    #region BoxSerializeInWorldData

    public class WorldSpecialBoxData : IClone<WorldSpecialBoxData>
    {
        public GridPos3D WorldGP;
        public ushort BoxTypeIndex;
        public BoxExtraSerializeData BoxExtraSerializeDataFromWorld; // 序列化到世界中的Box自己处理自己的ExtraData

        public WorldSpecialBoxData Clone()
        {
            WorldSpecialBoxData newData = new WorldSpecialBoxData();
            newData.WorldGP = WorldGP;
            newData.BoxTypeIndex = BoxTypeIndex;
            newData.BoxExtraSerializeDataFromWorld = BoxExtraSerializeDataFromWorld.Clone();
            return newData;
        }
    }

    public WorldSpecialBoxData GetBoxSerializeInWorldData()
    {
        WorldSpecialBoxData data = new WorldSpecialBoxData();
        data.BoxExtraSerializeDataFromWorld = GetBoxExtraSerializeDataForWorld();
        return data;
    }

    #endregion

    #region BoxExtraData

#if UNITY_EDITOR

    public bool RequireHideInWorldForModuleBox
    {
        get
        {
            foreach (BoxPassiveSkill bf in RawBoxPassiveSkills)
            {
                if (bf is BoxPassiveSkill_Hide hide)
                {
                    if (hide.SpecialCaseType == BoxPassiveSkill.BoxPassiveSkillBaseSpecialCaseType.World)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public bool RequireSerializeFunctionIntoWorld
    {
        get
        {
            foreach (BoxPassiveSkill bf in RawBoxPassiveSkills)
            {
                if (bf.SpecialCaseType == BoxPassiveSkill.BoxPassiveSkillBaseSpecialCaseType.World)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public bool RequireSerializeFunctionIntoWorldModule
    {
        get
        {
            foreach (BoxPassiveSkill bf in RawBoxPassiveSkills)
            {
                if (bf.SpecialCaseType == BoxPassiveSkill.BoxPassiveSkillBaseSpecialCaseType.Module)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public bool LevelEventTriggerAppearInWorldModule
    {
        get
        {
            foreach (BoxPassiveSkill bf in RawBoxPassiveSkills)
            {
                if (bf is BoxPassiveSkill_LevelEventTriggerAppear appear)
                {
                    if (appear.SpecialCaseType == BoxPassiveSkill.BoxPassiveSkillBaseSpecialCaseType.Module)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public bool LevelEventTriggerAppearInWorld
    {
        get
        {
            foreach (BoxPassiveSkill bf in RawBoxPassiveSkills)
            {
                if (bf is BoxPassiveSkill_LevelEventTriggerAppear appear)
                {
                    if (appear.SpecialCaseType == BoxPassiveSkill.BoxPassiveSkillBaseSpecialCaseType.World)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

#endif

    public class BoxExtraSerializeData : IClone<BoxExtraSerializeData>
    {
        public GridPos3D LocalGP; // Box在Module内的GP

        [FormerlySerializedAs("BoxFunctions")]
        public List<BoxPassiveSkill> BoxPassiveSkills = new List<BoxPassiveSkill>();

        public BoxExtraSerializeData Clone()
        {
            return new BoxExtraSerializeData
            {
                LocalGP = LocalGP,
                BoxPassiveSkills = BoxPassiveSkills.Clone()
            };
        }
    }

    public BoxExtraSerializeData GetBoxExtraSerializeDataForWorld()
    {
        BoxExtraSerializeData data = new BoxExtraSerializeData();
        data.BoxPassiveSkills = new List<BoxPassiveSkill>();
        foreach (BoxPassiveSkill bf in RawBoxPassiveSkills)
        {
            if (bf is BoxPassiveSkill_LevelEventTriggerAppear) continue;
            if (bf.SpecialCaseType == BoxPassiveSkill.BoxPassiveSkillBaseSpecialCaseType.World)
            {
                data.BoxPassiveSkills.Add(bf.Clone());
            }
        }

        return data;
    }

    public BoxExtraSerializeData GetBoxExtraSerializeDataForWorldOverrideWorldModule()
    {
        BoxExtraSerializeData data = new BoxExtraSerializeData();
        data.BoxPassiveSkills = new List<BoxPassiveSkill>();
        foreach (BoxPassiveSkill bf in RawBoxPassiveSkills)
        {
            if (bf.SpecialCaseType == BoxPassiveSkill.BoxPassiveSkillBaseSpecialCaseType.World)
            {
                data.BoxPassiveSkills.Add(bf.Clone());
            }
        }

        return data;
    }

    public BoxExtraSerializeData GetBoxExtraSerializeDataForWorldModule()
    {
        BoxExtraSerializeData data = new BoxExtraSerializeData();
        data.BoxPassiveSkills = new List<BoxPassiveSkill>();
        foreach (BoxPassiveSkill bf in RawBoxPassiveSkills)
        {
            if (bf is BoxPassiveSkill_LevelEventTriggerAppear) continue;
            if (bf.SpecialCaseType == BoxPassiveSkill.BoxPassiveSkillBaseSpecialCaseType.Module)
            {
                data.BoxPassiveSkills.Add(bf.Clone());
            }
        }

        return data;
    }

    public void ApplyBoxExtraSerializeData(BoxExtraSerializeData boxExtraSerializeDataFromModule = null, BoxExtraSerializeData boxExtraSerializeDataFromWorld = null)
    {
        if (boxExtraSerializeDataFromModule != null)
        {
            List<BoxPassiveSkill> newFunctionList = new List<BoxPassiveSkill>();
            foreach (BoxPassiveSkill extraBF in boxExtraSerializeDataFromModule.BoxPassiveSkills)
            {
                bool foundMatch = false;
                foreach (BoxPassiveSkill bf in RawBoxPassiveSkills)
                {
                    if (bf.GetType() == extraBF.GetType())
                    {
                        foundMatch = true;
                        bf.CopyDataFrom(extraBF);
                    }
                }

                if (!foundMatch)
                {
                    newFunctionList.Add(extraBF.Clone());
                }
            }

            foreach (BoxPassiveSkill newFunction in newFunctionList)
            {
                newFunction.Box = this;
                RawBoxPassiveSkills.Add(newFunction);
            }
        }

        // world box extra data has higher priority
        if (boxExtraSerializeDataFromWorld != null)
        {
            List<BoxPassiveSkill> newFunctionList = new List<BoxPassiveSkill>();
            foreach (BoxPassiveSkill extraBF in boxExtraSerializeDataFromWorld.BoxPassiveSkills)
            {
                bool foundMatch = false;
                foreach (BoxPassiveSkill bf in RawBoxPassiveSkills)
                {
                    if (bf.GetType() == extraBF.GetType())
                    {
                        foundMatch = true;
                        bf.CopyDataFrom(extraBF);
                    }
                }

                if (!foundMatch)
                {
                    newFunctionList.Add(extraBF.Clone());
                }
            }

            foreach (BoxPassiveSkill newFunction in newFunctionList)
            {
                newFunction.Box = this;
                RawBoxPassiveSkills.Add(newFunction);
            }
        }
    }

    #endregion

    #endregion

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

public enum BoxShapeType
{
    Box,
    Wedge,
}
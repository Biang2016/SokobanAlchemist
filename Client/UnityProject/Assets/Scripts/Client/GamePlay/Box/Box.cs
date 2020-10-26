using System;
using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ObjectPool;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BiangStudio;
using BiangStudio.CloneVariant;
using Sirenix.Serialization;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;

#endif

public class Box : PoolObject, ISerializationCallbackReceiver
{
    #region GUID

    [ReadOnly]
    [HideInEditorMode]
    public uint GUID;

    private static uint guidGenerator = (uint) ConfigManager.GUID_Separator.Box;

    private uint GetGUID()
    {
        return guidGenerator++;
    }

    #endregion

    private BoxUnderWorldModuleDesignerClamper BoxUnderWorldModuleDesignerClamper;
    private GridSnapper GridSnapper;

    internal Rigidbody Rigidbody;
    public Collider BoxIndicatorTrigger;

    internal Actor LastTouchActor;
    internal BoxEffectHelper BoxEffectHelper;
    public BoxColliderHelper BoxColliderHelper;
    public BoxThornTrapTriggerHelper BoxThornTrapTriggerHelper;
    public BoxSkinHelper BoxSkinHelper;
    public BoxIconSpriteHelper BoxIconSpriteHelper;

    internal bool ArtOnly;

    public override void OnUsed()
    {
        LastTouchActor = null;
        ArtOnly = true;
        BoxColliderHelper.OnUsed();
        BoxIndicatorTrigger.gameObject.SetActive(true);
        base.OnUsed();
    }

    public override void OnRecycled()
    {
        ArtOnly = true;
        WorldModule = null;
        WorldGP = GridPos3D.Zero;
        lastWorldGP = GridPos3D.Zero;
        LastState = States.Static;
        State = States.Static;
        BoxEffectHelper?.PoolRecycle();
        BoxColliderHelper.PoolRecycle();
        BoxThornTrapTriggerHelper?.PoolRecycle();
        BoxSkinHelper?.PoolRecycle();
        BoxIconSpriteHelper?.PoolRecycle();
        BoxEffectHelper = null;
        transform.DOPause();
        alreadyCollide = false;
        if (Rigidbody) Destroy(Rigidbody);
        if (LastTouchActor != null && LastTouchActor.CurrentLiftBox == this)
        {
            LastTouchActor.ThrowState = Actor.ThrowStates.None;
            LastTouchActor.CurrentLiftBox = null;
            LastTouchActor = null;
        }

        UnRegisterEvents();
        UnInitBoxFunctions();
        BoxIndicatorTrigger.gameObject.SetActive(false);
        base.OnRecycled();
    }

    [HideInInspector]
    public ushort BoxTypeIndex;

    [LabelText("箱子特性")]
    [AssetsOnly]
    public BoxFeature BoxFeature;

    [LabelText("箱子形状")]
    [AssetsOnly]
    [OnValueChanged("SwitchBoxShapeType")]
    public BoxShapeType BoxShapeType;

    private void SwitchBoxShapeType()
    {
        if (BoxShapeType == BoxShapeType.Box)
        {
            BoxSkinHelper?.ResetBoxOrientation();
            BoxColliderHelper.ResetBoxOrientation();
            BoxOrientation = GridPosR.Orientation.Up;
        }

        BoxSkinHelper?.RefreshBoxShapeType();
        BoxColliderHelper.SwitchBoxShapeType();
    }

    [LabelText("箱子朝向")]
    [HideIf("BoxShapeType", BoxShapeType.Box)]
    [OnValueChanged("SwitchBoxOrientation")]
    [EnumToggleButtons]
    public GridPosR.Orientation BoxOrientation;

    private void SwitchBoxOrientation()
    {
        BoxSkinHelper?.SwitchBoxOrientation();
        BoxColliderHelper.SwitchBoxOrientation();
    }

    [AssetsOnly]
    [ShowIf("Interactable")]
    [BoxGroup("箱子属性")]
    [LabelText("重量")]
    [SerializeField]
    private float Weight;

    public float FinalWeight => Weight * ConfigManager.BoxWeightFactor_Cheat;

    [AssetsOnly]
    [BoxGroup("碰撞")]
    [LabelText("撞击特效")]
    [SerializeField]
    [ValueDropdown("GetAllFXTypeNames")]
    private string CollideFX;

    [AssetsOnly]
    [BoxGroup("碰撞")]
    [LabelText("撞击特效尺寸")]
    [SerializeField]
    private float CollideFXScale = 1f;

    [AssetsOnly]
    [ShowIf("KickOrThrowable")]
    [BoxGroup("碰撞")]
    [LabelText("碰撞伤害半径")]
    [SerializeField]
    [FormerlySerializedAs("DestroyDamageRadius_Kick")]
    private float CollideDamageRadius = 0.5f;

    [AssetsOnly]
    [ShowIf("KickOrThrowable")]
    [BoxGroup("碰撞")]
    [LabelText("碰撞伤害")]
    [GUIColor(1.0f, 0, 1.0f)]
    [SerializeField]
    [FormerlySerializedAs("DestroyDamage_Kick")]
    private float CollideDamage = 3f;

    [AssetsOnly]
    [ShowIf("Pushable")]
    [BoxGroup("推箱子属性")]
    [LabelText("抗推力")]
    [PropertyRange(0, 1)]
    public float Static_Inertia = 0.5f;

    [AssetsOnly]
    [ShowIf("Throwable")]
    [BoxGroup("扔箱子属性")]
    [LabelText("落地Drag")]
    public float Throw_Drag = 0.5f;

    [AssetsOnly]
    [ShowIf("Throwable")]
    [BoxGroup("扔箱子属性")]
    [LabelText("落地摩擦力")]
    [Range(0, 1)]
    public float Throw_Friction = 1;

    [AssetsOnly]
    [ShowIf("Kickable")]
    [BoxGroup("踢箱子属性")]
    [LabelText("摩阻力")]
    public float Dynamic_Drag = 0.5f;

    #region 箱子特殊功能

    [BoxGroup("箱子特殊功能")]
    [ShowInInspector]
    [LabelText("箱子特殊功能")]
    [NonSerialized]
    [FormerlySerializedAs("BoxFunctions")]
    public List<BoxFunctionBase> RawBoxFunctions = new List<BoxFunctionBase>(); // 干数据，禁修改

    public List<BoxFunctionBase> BoxFunctions = new List<BoxFunctionBase>(); // 湿数据，每个Box生命周期开始前从干数据拷出，结束后清除
    public Dictionary<string, BoxFunctionBase> BoxFunctionDict = new Dictionary<string, BoxFunctionBase>(); // 便于寻找

    [HideInInspector]
    public byte[] BoxFunctionBaseData;

    public void OnBeforeSerialize()
    {
        if (RawBoxFunctions == null) RawBoxFunctions = new List<BoxFunctionBase>();
        BoxFunctionBaseData = SerializationUtility.SerializeValue(RawBoxFunctions, DataFormat.JSON);
    }

    public void OnAfterDeserialize()
    {
        RawBoxFunctions = SerializationUtility.DeserializeValue<List<BoxFunctionBase>>(BoxFunctionBaseData, DataFormat.JSON);
    }

    private void InitBoxFunctions()
    {
        BoxFunctions.Clear();
        BoxFunctionDict.Clear();
        BoxFunctions = RawBoxFunctions.Clone();
        foreach (BoxFunctionBase bf in RawBoxFunctions)
        {
            bf.Box = this;
            bf.OnRegisterLevelEventID();
            string bfName = bf.GetType().Name;
            if (!BoxFunctionDict.ContainsKey(bfName))
            {
                BoxFunctionDict.Add(bfName, bf.Clone());
            }
        }
    }

    private void UnInitBoxFunctions()
    {
        foreach (BoxFunctionBase bf in RawBoxFunctions)
        {
            bf.OnUnRegisterLevelEventID();
        }

        BoxFunctions.Clear();
        BoxFunctionDict.Clear();
    }

    #endregion

    private GridPos3D lastWorldGP;

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
    [BoxGroup("状态")]
    [LabelText("上一个移动状态")]
    private States LastState = States.Static;

    private States state = States.Static;

    [HideInPrefabAssets]
    [ReadOnly]
    [ShowInInspector]
    [BoxGroup("状态")]
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
        InitBoxFunctions();
        RegisterEvents();

        foreach (BoxFunctionBase bf in BoxFunctions)
        {
            if (bf is BoxFunction_ShapeAndOrientation bf_so)
            {
                BoxShapeType = bf_so.BoxShapeType;
                BoxOrientation = bf_so.Orientation;
            }
        }

        if (BattleManager.Instance.Player1) OnPlayerInteractSkillChanged(BattleManager.Instance.Player1.ActorSkillHelper.GetInteractSkillType(BoxTypeIndex), BoxTypeIndex);
    }

    public void Initialize(GridPos3D localGridPos3D, WorldModule module, float lerpTime, bool artOnly, LerpType lerpType)
    {
        ArtOnly = artOnly;
        LastTouchActor = null;
        lastWorldGP = WorldGP;
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
            transform.localPosition = localGridPos3D.ToVector3();
            transform.localRotation = Quaternion.identity;
            State = States.Static;
        }

        WorldManager.Instance.CurrentWorld.CheckDropSelf(this);
    }

    public void Push(Vector3 direction)
    {
        if (State == States.Static || State == States.PushingCanceling)
        {
            Vector3 targetPos = WorldGP.ToVector3() + direction.normalized;
            GridPos3D gp = GridPos3D.GetGridPosByPoint(targetPos, 1);
            if (gp != WorldGP)
            {
                WorldManager.Instance.CurrentWorld.MoveBox(WorldGP, gp, States.BeingPushed);
            }
        }
    }

    public void PushCanceled()
    {
        if (State == States.BeingPushed)
        {
            if ((transform.localPosition - LocalGP.ToVector3()).magnitude > (1 - Static_Inertia))
            {
                WorldManager.Instance.CurrentWorld.MoveBox(WorldGP, lastWorldGP, States.PushingCanceling);
            }
        }
    }

    public void Kick(Vector3 direction, float velocity, Actor actor)
    {
        if (State == States.BeingPushed || State == States.Flying || State == States.BeingKicked || State == States.Static || State == States.PushingCanceling)
        {
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
            if (!Rigidbody) Rigidbody = gameObject.AddComponent<Rigidbody>();
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
        if (State == States.BeingPushed || State == States.Flying || State == States.BeingKicked || State == States.Static || State == States.PushingCanceling)
        {
            DefaultRotBeforeLift = transform.rotation;
            alreadyCollide = false;
            LastTouchActor = actor;
            foreach (BoxFunctionBase bf in BoxFunctions)
            {
                bf.OnBeingLift(actor);
            }

            WorldManager.Instance.CurrentWorld.RemoveBoxFromGrid(this);
            State = States.BeingLift;
            transform.DOPause();
            BoxColliderHelper.OnBeingLift();
            if (Rigidbody)
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
        if (State == States.Lifted)
        {
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
            if (!Rigidbody) Rigidbody = gameObject.AddComponent<Rigidbody>();
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
            if (!Rigidbody) Rigidbody = gameObject.AddComponent<Rigidbody>();
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
        if (!Rigidbody) Rigidbody = gameObject.AddComponent<Rigidbody>();
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

    void FixedUpdate()
    {
        if (IsRecycled) return;
        if ((State == States.BeingKicked || State == States.Flying || State == States.DroppingFromDeadActor || State == States.Putting) && Rigidbody)
        {
            if (State == States.BeingKicked)
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

        if (Rigidbody && Rigidbody.velocity.magnitude > 1f)
        {
            BoxEffectHelper?.Play();
        }
        else
        {
            BoxEffectHelper?.Stop();
        }
    }

    void OnCollisionEnter(Collision collision)
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
                if (!BoxFunctionDict.ContainsKey(typeof(BoxFunction_CollideBreakable).Name))
                {
                    WorldManager.Instance.CurrentWorld.DeleteBox(this); // 如果没配默认撞击破坏
                }

                foreach (BoxFunctionBase bf in BoxFunctions)
                {
                    bf.OnFlyingCollisionEnter(collision);
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
                    if (!BoxFunctionDict.ContainsKey(typeof(BoxFunction_CollideBreakable).Name))
                    {
                        WorldManager.Instance.CurrentWorld.DeleteBox(this); // 如果没配默认撞击破坏
                    }

                    foreach (BoxFunctionBase bf in BoxFunctions)
                    {
                        bf.OnBeingKickedCollisionEnter(collision);
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

#if UNITY_EDITOR

    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            //Gizmos.color = new Color(0.2f, 0.2f, 0.2f, 0.1f);
            //Gizmos.DrawCube(transform.position, Vector3.one);

            if (RequireSerializeFunctionIntoWorldModule)
            {
                transform.DrawSpecialTip(Vector3.up, CommonUtils.HTMLColorToColor("#0AFFF1"), Color.cyan, "模特");
            }

            if (RequireHideInWorldForModuleBox)
            {
                transform.DrawSpecialTip(Vector3.up, CommonUtils.HTMLColorToColor("#FF8000"), Color.yellow, "世隐");
            }
            else if (RequireSerializeFunctionIntoWorld || IsUnderWorldSpecialBoxesRoot)
            {
                transform.DrawSpecialTip(Vector3.up, CommonUtils.HTMLColorToColor("#FF8000"), Color.yellow, "世特");
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
    [BoxGroup("模组编辑器")]
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
    [BoxGroup("模组编辑器")]
    [LabelText("替换Box类型")]
    [ValueDropdown("GetAllBoxTypeNames")]
    private string ReplaceBoxTypeName;

    public bool DeleteBoxTypeName(string srcBoxName, StringBuilder info, bool moduleSpecial = false, bool worldSpecial = false)
    {
        bool isDirty = false;
        foreach (BoxFunctionBase bf in BoxFunctions)
        {
            if (moduleSpecial && bf.SpecialCaseType != BoxFunctionBase.BoxFunctionBaseSpecialCaseType.Module) continue;
            if (worldSpecial && bf.SpecialCaseType != BoxFunctionBase.BoxFunctionBaseSpecialCaseType.World) continue;

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
                                info.Append($"替换{name}.BoxFunctions.{bf.GetType().Name}.{fi.Name} -> 'None'\n");
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
                                    info.Append($"移除自{name}.BoxFunctions.{bf.GetType().Name}.{fi.Name}\n");
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

    public bool RequireHideInWorldForModuleBox
    {
        get
        {
            foreach (BoxFunctionBase bf in BoxFunctions)
            {
                if (bf is BoxFunction_Hide hide)
                {
                    if (hide.SpecialCaseType == BoxFunctionBase.BoxFunctionBaseSpecialCaseType.World)
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
            foreach (BoxFunctionBase bf in BoxFunctions)
            {
                if (bf.SpecialCaseType == BoxFunctionBase.BoxFunctionBaseSpecialCaseType.World)
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
            foreach (BoxFunctionBase bf in BoxFunctions)
            {
                if (bf.SpecialCaseType == BoxFunctionBase.BoxFunctionBaseSpecialCaseType.Module)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class BoxExtraSerializeData : IClone<BoxExtraSerializeData>
    {
        public GridPos3D LocalGP; // Box在Module内的GP
        public List<BoxFunctionBase> BoxFunctions = new List<BoxFunctionBase>();

        public BoxExtraSerializeData Clone()
        {
            return new BoxExtraSerializeData
            {
                LocalGP = LocalGP,
                BoxFunctions = BoxFunctions.Clone()
            };
        }
    }

    public BoxExtraSerializeData GetBoxExtraSerializeDataForWorld()
    {
        BoxExtraSerializeData data = new BoxExtraSerializeData();
        data.BoxFunctions = new List<BoxFunctionBase>();
        foreach (BoxFunctionBase bf in BoxFunctions)
        {
            if (bf.SpecialCaseType == BoxFunctionBase.BoxFunctionBaseSpecialCaseType.World)
            {
                data.BoxFunctions.Add(bf.Clone());
            }
        }

        return data;
    }

    public BoxExtraSerializeData GetBoxExtraSerializeDataForWorldModule()
    {
        BoxExtraSerializeData data = new BoxExtraSerializeData();
        data.BoxFunctions = new List<BoxFunctionBase>();
        foreach (BoxFunctionBase bf in BoxFunctions)
        {
            if (bf.SpecialCaseType == BoxFunctionBase.BoxFunctionBaseSpecialCaseType.Module)
            {
                data.BoxFunctions.Add(bf.Clone());
            }
        }

        return data;
    }

    public void ApplyBoxExtraSerializeData(BoxExtraSerializeData boxExtraSerializeDataFromModule = null, BoxExtraSerializeData boxExtraSerializeDataFromWorld = null)
    {
        if (boxExtraSerializeDataFromModule != null)
        {
            List<BoxFunctionBase> newFunctionList = new List<BoxFunctionBase>();
            foreach (BoxFunctionBase extraBF in boxExtraSerializeDataFromModule.BoxFunctions)
            {
                bool foundMatch = false;
                foreach (BoxFunctionBase bf in BoxFunctions)
                {
                    if (bf.GetType() == extraBF.GetType())
                    {
                        foundMatch = true;
                        bf.ApplyData(extraBF);
                    }
                }

                if (!foundMatch)
                {
                    newFunctionList.Add(extraBF.Clone());
                }
            }

            foreach (BoxFunctionBase newFunction in newFunctionList)
            {
                newFunction.Box = this;
                BoxFunctions.Add(newFunction);
            }
        }

        // world box extra data has higher priority
        if (boxExtraSerializeDataFromWorld != null)
        {
            List<BoxFunctionBase> newFunctionList = new List<BoxFunctionBase>();
            foreach (BoxFunctionBase extraBF in boxExtraSerializeDataFromWorld.BoxFunctions)
            {
                bool foundMatch = false;
                foreach (BoxFunctionBase bf in BoxFunctions)
                {
                    if (bf.GetType() == extraBF.GetType())
                    {
                        foundMatch = true;
                        bf.ApplyData(extraBF);
                    }
                }

                if (!foundMatch)
                {
                    newFunctionList.Add(extraBF.Clone());
                }
            }

            foreach (BoxFunctionBase newFunction in newFunctionList)
            {
                newFunction.Box = this;
                BoxFunctions.Add(newFunction);
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

    #region Utils

    private IEnumerable<string> GetAllBoxTypeNames => ConfigManager.GetAllBoxTypeNames();

    private IEnumerable<string> GetAllFXTypeNames => ConfigManager.GetAllFXTypeNames();

    #endregion
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
using System;
using System.Linq;
using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ObjectPool;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
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
    public GameObject NormalColliders;
    public Collider StaticCollider;
    public Collider DynamicCollider;
    public Collider BoxOnlyDynamicCollider;
    internal Rigidbody Rigidbody;

    internal Actor LastTouchActor;
    internal BoxEffectHelper BoxEffectHelper;
    public BoxThornTrapTriggerHelper BoxThornTrapTriggerHelper;
    public BoxSkinHelper BoxSkinHelper;

    internal bool ArtOnly;

    public override void OnUsed()
    {
        LastTouchActor = null;
        ArtOnly = true;
        base.OnUsed();
    }

    public override void OnRecycled()
    {
        ArtOnly = true;
        WorldModule = null;
        GridPos3D = GridPos3D.Zero;
        lastGP = GridPos3D.Zero;
        LastState = States.Static;
        State = States.Static;
        BoxEffectHelper?.PoolRecycle();
        BoxThornTrapTriggerHelper?.PoolRecycle();
        BoxEffectHelper = null;
        transform.DOPause();
        StaticCollider.enabled = false;
        DynamicCollider.enabled = false;
        BoxOnlyDynamicCollider.enabled = false;
        damageTimes = 0;
        if (Rigidbody) Destroy(Rigidbody);
        if (LastTouchActor != null && LastTouchActor.CurrentLiftBox == this)
        {
            LastTouchActor.ThrowState = Actor.ThrowStates.None;
            LastTouchActor.CurrentLiftBox = null;
            LastTouchActor = null;
        }

        UnRegisterEvents();
        base.OnRecycled();
    }

    [HideInInspector]
    public byte BoxTypeIndex;

    [LabelText("箱子特性")]
    [AssetsOnly]
    public BoxFeature BoxFeature;

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

    [ShowInInspector]
    [LabelText("箱子特殊功能")]
    internal List<BoxFunctionBase> BoxFunctions = new List<BoxFunctionBase>();

    [HideInInspector]
    public byte[] BoxFunctionBaseData;

    public void OnBeforeSerialize()
    {
        if (BoxFunctions == null) BoxFunctions = new List<BoxFunctionBase>();
        BoxFunctionBaseData = SerializationUtility.SerializeValue(BoxFunctions, DataFormat.JSON);
    }

    public void OnAfterDeserialize()
    {
        BoxFunctions = SerializationUtility.DeserializeValue<List<BoxFunctionBase>>(BoxFunctionBaseData, DataFormat.JSON);
    }

    private GridPos3D lastGP;

    [HideInEditorMode]
    public GridPos3D GridPos3D;

    [HideInEditorMode]
    public GridPos3D LocalGridPos3D;

    [HideInEditorMode]
    public WorldModule WorldModule;

    private int damageTimes;

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

        foreach (BoxFunctionBase bf in BoxFunctions)
        {
            bf.Box = this;
        }
    }

    private void RegisterEvents()
    {
        ClientGameManager.Instance.BattleMessenger.AddListener<InteractSkillType, byte>((uint) Enum_Events.OnPlayerInteractSkillChanged, OnPlayerInteractSkillChanged);
        foreach (BoxFunctionBase bf in BoxFunctions)
        {
            bf.OnRegisterLevelEventID();
        }
    }

    private void UnRegisterEvents()
    {
        ClientGameManager.Instance.BattleMessenger.RemoveListener<InteractSkillType, byte>((uint) Enum_Events.OnPlayerInteractSkillChanged, OnPlayerInteractSkillChanged);
        foreach (BoxFunctionBase bf in BoxFunctions)
        {
            bf.OnUnRegisterLevelEventID();
        }
    }

    private void OnPlayerInteractSkillChanged(InteractSkillType interactSkillType, byte boxTypeIndex)
    {
        if (boxTypeIndex == BoxTypeIndex)
        {
            BoxSkinHelper?.SwitchModel(interactSkillType.ConvertToBoxModelType());
        }
    }

    public void Setup(byte boxTypeIndex)
    {
        BoxTypeIndex = boxTypeIndex;
        RegisterEvents();
        if (BattleManager.Instance.Player1) OnPlayerInteractSkillChanged(BattleManager.Instance.Player1.ActorSkillHelper.GetInteractSkillType(BoxTypeIndex), BoxTypeIndex);
    }

    public void Initialize(GridPos3D localGridPos3D, WorldModule module, float lerpTime, bool artOnly, LerpType lerpType)
    {
        StaticCollider.enabled = true;
        DynamicCollider.enabled = false;
        BoxOnlyDynamicCollider.enabled = false;
        if (lerpType == LerpType.Drop)
        {
            StaticCollider.enabled = false;
            DynamicCollider.enabled = false;
            BoxOnlyDynamicCollider.enabled = false;
        }

        ArtOnly = artOnly;
        NormalColliders.SetActive(!Passable);
        StaticCollider.gameObject.SetActive(!artOnly);
        DynamicCollider.gameObject.SetActive(!artOnly);
        BoxOnlyDynamicCollider.gameObject.SetActive(!artOnly && Passable);

        if (BoxFeature.HasFlag(BoxFeature.IsGround))
        {
            StaticCollider.material.staticFriction = 0;
            StaticCollider.material.dynamicFriction = 0;
            DynamicCollider.material.staticFriction = 0;
            DynamicCollider.material.dynamicFriction = 0;
        }

        LastTouchActor = null;
        lastGP = GridPos3D;
        WorldModule = module;
        GridPos3D = localGridPos3D + module.ModuleGP * WorldModule.MODULE_SIZE;
        LocalGridPos3D = localGridPos3D;
        transform.parent = module.transform;
        if (lerpTime > 0)
        {
            transform.DOPause();
            transform.DOLocalMove(localGridPos3D.ToVector3(), lerpTime).SetEase(Ease.Linear).OnComplete(() =>
            {
                State = States.Static;
                if (lerpType == LerpType.Drop)
                {
                    StaticCollider.enabled = true;
                    DynamicCollider.enabled = true;
                    BoxOnlyDynamicCollider.enabled = true;
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
            if (lerpType == LerpType.Drop)
            {
                StaticCollider.enabled = true;
                DynamicCollider.enabled = true;
                BoxOnlyDynamicCollider.enabled = true;
            }
        }

        WorldManager.Instance.CurrentWorld.CheckDropSelf(this);
    }

    public void Push(Vector3 direction)
    {
        if (State == States.Static || State == States.PushingCanceling)
        {
            Vector3 targetPos = GridPos3D.ToVector3() + direction.normalized;
            GridPos3D gp = GridPos3D.GetGridPosByPoint(targetPos, 1);
            if (gp != GridPos3D)
            {
                WorldManager.Instance.CurrentWorld.MoveBox(GridPos3D, gp, States.BeingPushed);
            }
        }
    }

    public void PushCanceled()
    {
        if (State == States.BeingPushed)
        {
            if ((transform.localPosition - LocalGridPos3D.ToVector3()).magnitude > (1 - Static_Inertia))
            {
                WorldManager.Instance.CurrentWorld.MoveBox(GridPos3D, lastGP, States.PushingCanceling);
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

            damageTimes = 0;
            LastTouchActor = actor;
            WorldManager.Instance.CurrentWorld.RemoveBox(this);
            State = States.BeingKicked;
            transform.DOPause();
            StaticCollider.enabled = false;
            DynamicCollider.enabled = true;
            BoxOnlyDynamicCollider.enabled = true;
            DynamicCollider.material.dynamicFriction = 0f;
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

    public bool BeingLift(Actor actor)
    {
        if (State == States.BeingPushed || State == States.Flying || State == States.BeingKicked || State == States.Static || State == States.PushingCanceling)
        {
            damageTimes = 0;
            LastTouchActor = actor;
            foreach (BoxFunctionBase bf in BoxFunctions)
            {
                bf.OnBeingLift(actor);
            }

            WorldManager.Instance.CurrentWorld.RemoveBox(this);
            State = States.BeingLift;
            transform.DOPause();
            StaticCollider.enabled = true;
            DynamicCollider.enabled = false;
            BoxOnlyDynamicCollider.enabled = true;
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
            damageTimes = 0;
            if (BoxEffectHelper == null)
            {
                BoxEffectHelper = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.BoxEffectHelper].AllocateGameObject<BoxEffectHelper>(transform);
            }

            LastTouchActor = actor;
            State = States.Flying;
            transform.DOPause();
            transform.parent = WorldManager.Instance.CurrentWorld.transform;
            StaticCollider.enabled = false;
            DynamicCollider.enabled = true;
            BoxOnlyDynamicCollider.enabled = true;
            DynamicCollider.material.dynamicFriction = Throw_Friction;
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
            damageTimes = 0;
            if (BoxEffectHelper == null)
            {
                BoxEffectHelper = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.BoxEffectHelper].AllocateGameObject<BoxEffectHelper>(transform);
            }

            LastTouchActor = actor;
            State = States.Putting;
            transform.DOPause();
            transform.parent = WorldManager.Instance.CurrentWorld.transform;
            StaticCollider.enabled = false;
            DynamicCollider.enabled = true;
            BoxOnlyDynamicCollider.enabled = true;
            DynamicCollider.material.dynamicFriction = Throw_Friction;
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

        damageTimes = 0;
        LastTouchActor = null;
        State = States.DroppingFromDeadActor;
        transform.DOPause();
        transform.parent = WorldManager.Instance.CurrentWorld.transform;
        StaticCollider.enabled = false;
        DynamicCollider.enabled = true;
        BoxOnlyDynamicCollider.enabled = true;
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
                StaticCollider.enabled = true;
                DynamicCollider.enabled = false;
                BoxOnlyDynamicCollider.enabled = false;
                WorldManager.Instance.CurrentWorld.BoxReturnToWorldFromPhysics(this);
                BoxEffectHelper?.PoolRecycle();
                BoxEffectHelper = null;
            }
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
        if (State == States.Flying)
        {
            if (damageTimes < 1)
            {
                CollideAOEDamage(CollideDamageRadius, CollideDamage);
                PlayCollideFX();
                foreach (BoxFunctionBase bf in BoxFunctions)
                {
                    bf.OnFlyingCollisionEnterDestroy(collision);
                }
            }

            if (BoxFeature.HasFlag(BoxFeature.ThrowHitBreakable))
            {
                PoolRecycle();
            }
            else
            {
                Box box = collision.gameObject.GetComponentInParent<Box>();
                if (box && !box.BoxFeature.HasFlag(BoxFeature.IsBorder))
                {
                    Rigidbody.drag = Throw_Drag * ConfigManager.BoxThrowDragFactor_Cheat;
                }
            }
        }

        if (State == States.Putting)
        {
            Box box = collision.gameObject.GetComponentInParent<Box>();
            if (box && !box.BoxFeature.HasFlag(BoxFeature.IsBorder))
            {
                Rigidbody.drag = Throw_Drag * ConfigManager.BoxThrowDragFactor_Cheat;
            }
        }

        if (State == States.BeingKicked)
        {
            if (collision.gameObject.layer != LayerManager.Instance.Layer_Ground)
            {
                if (damageTimes < 1)
                {
                    CollideAOEDamage(CollideDamageRadius, CollideDamage);
                    PlayCollideFX();
                    foreach (BoxFunctionBase bf in BoxFunctions)
                    {
                        bf.OnBeingKickedCollisionEnterDestroy(collision);
                    }
                }

                if (BoxFeature.HasFlag(BoxFeature.KickHitBreakable))
                {
                    PoolRecycle();
                }
            }
        }
    }

    private void CollideAOEDamage(float radius, float damage)
    {
        damageTimes++;
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

    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Gizmos.color = new Color(0.2f, 0.2f, 0.2f, 0.1f);
            Gizmos.DrawCube(transform.position, Vector3.one);
        }
    }

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

#endif

    public bool Pushable => BoxFeature.HasFlag(BoxFeature.Pushable);

    public bool Kickable => BoxFeature.HasFlag(BoxFeature.Kickable);

    public bool Liftable => BoxFeature.HasFlag(BoxFeature.Liftable);

    public bool KickOrThrowable => Kickable || Throwable;

    public bool Throwable => BoxFeature.HasFlag(BoxFeature.Throwable);

    public bool Interactable => Pushable || Kickable || Liftable || Throwable;

    public bool Passable => BoxFeature.HasFlag(BoxFeature.Passable);

    public bool Droppable => BoxFeature.HasFlag(BoxFeature.Droppable);

    public bool Breakable => BoxFeature.HasFlag(BoxFeature.ThrowHitBreakable) || BoxFeature.HasFlag(BoxFeature.KickHitBreakable);

    public bool Healable => BoxFeature.HasFlag(BoxFeature.HealingBox);

    #region Utils

    private IEnumerable<string> GetAllBoxTypeNames()
    {
        ConfigManager.LoadAllConfigs();
        List<string> res = ConfigManager.BoxTypeDefineDict.TypeIndexDict.Keys.ToList();
        res.Insert(0, "None");
        return res;
    }

    private IEnumerable<string> GetAllFXTypeNames()
    {
        ConfigManager.LoadAllConfigs();
        List<string> res = ConfigManager.FXTypeDefineDict.TypeIndexDict.Keys.ToList();
        res.Insert(0, "None");
        return res;
    }

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

    [LabelText("--占位--")]
    BelongToPlayer2 = 1 << 6,

    [LabelText("踢撞会碎")]
    KickHitBreakable = 1 << 7,

    [LabelText("扔撞会碎")]
    ThrowHitBreakable = 1 << 8,

    [LabelText("墙壁")]
    IsBorder = 1 << 9,

    [LabelText("地面")]
    IsGround = 1 << 10,

    [LabelText("补血")]
    HealingBox = 1 << 11,
}
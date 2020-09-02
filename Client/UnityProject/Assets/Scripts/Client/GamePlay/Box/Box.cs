using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ObjectPool;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class Box : PoolObject
{
    private BoxUnderWorldModuleDesignerClamper BoxUnderWorldModuleDesignerClamper;
    private GridSnapper GridSnapper;
    public Collider StaticCollider;
    public Collider DynamicCollider;
    internal Rigidbody Rigidbody;

    public override void OnUsed()
    {
        ClientGameManager.Instance.BattleMessenger.AddListener((uint) Enum_Events.OnBoxStaticBounceCheatChanged, OnStaticBounceChanged);
        ClientGameManager.Instance.BattleMessenger.AddListener((uint) Enum_Events.OnBoxStaticBounceCheatChanged, OnDynamicBounceChanged);
        base.OnUsed();
    }

    public override void OnRecycled()
    {
        transform.DOPause();
        ClientGameManager.Instance.BattleMessenger.RemoveListener((uint) Enum_Events.OnBoxStaticBounceCheatChanged, OnStaticBounceChanged);
        ClientGameManager.Instance.BattleMessenger.RemoveListener((uint) Enum_Events.OnBoxStaticBounceCheatChanged, OnDynamicBounceChanged);
        base.OnRecycled();
    }

    [LabelText("箱子类型")]
    public BoxType BoxType;

    [BoxGroup("箱子属性")]
    [LabelText("重量")]
    [FormerlySerializedAs("Weight")]
    [SerializeField]
    private float Weight;

    public float FinalWeight => Weight * ConfigManager.BoxWeightFactor_Cheat;

    [BoxGroup("静止属性")]
    [LabelText("弹性")]
    public float Static_Bounce;

    [BoxGroup("动态属性")]
    [LabelText("弹性")]
    public float Dynamic_Bounce = 1f;

    [BoxGroup("推箱子属性")]
    [LabelText("抗推力")]
    [PropertyRange(0, 1)]
    [FormerlySerializedAs("Inertia")]
    public float Static_Inertia = 0.5f;

    [BoxGroup("扔箱子属性")]
    [LabelText("落地摩阻力")]
    public float Throw_Drag = 0.5f;

    [BoxGroup("踢箱子属性")]
    [LabelText("摩阻力")]
    public float Dynamic_Drag = 0.5f;

    private GridPos3D lastGP;

    [HideInEditorMode]
    public GridPos3D GridPos3D;

    [HideInEditorMode]
    public GridPos3D LocalGridPos3D;

    [HideInEditorMode]
    public WorldModule WorldModule;

    public enum States
    {
        Static,
        BeingPushed,
        PushingCanceling,
        BeingKicked,
        BeingLift,
        Lifted,
        Flying,
    }

    [ReadOnly]
    [LabelText("移动状态")]
    public States State = States.Static;

    protected virtual void Awake()
    {
        GridSnapper = GetComponent<GridSnapper>();
        BoxUnderWorldModuleDesignerClamper = GetComponent<BoxUnderWorldModuleDesignerClamper>();
        BoxUnderWorldModuleDesignerClamper.enabled = !Application.isPlaying;
        GridSnapper.enabled = false;
    }

    protected virtual void Start()
    {
        OnStaticBounceChanged();
        OnDynamicBounceChanged();
    }

    private void OnStaticBounceChanged()
    {
        StaticCollider.material.bounciness = Static_Bounce * ConfigManager.BoxStaticBounceFactor_Cheat;
    }

    private void OnDynamicBounceChanged()
    {
        DynamicCollider.material.bounciness = Dynamic_Bounce * ConfigManager.BoxStaticBounceFactor_Cheat;
    }

    public void Initialize(GridPos3D localGridPos3D, WorldModule module, float lerpTime)
    {
        lastGP = GridPos3D;
        WorldModule = module;
        GridPos3D = localGridPos3D + module.ModuleGP * WorldModule.MODULE_SIZE;
        LocalGridPos3D = localGridPos3D;
        transform.parent = module.transform;
        if (lerpTime > 0)
        {
            transform.DOPause();
            transform.DOLocalMove(localGridPos3D.ToVector3(), lerpTime).SetEase(Ease.Linear).OnComplete(() => { State = States.Static; });
            transform.DOLocalRotate(Vector3.zero, lerpTime);
            State = States.BeingPushed;
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

    public void Kick(Vector3 direction, float force)
    {
        if (State == States.BeingPushed || State == States.Flying || State == States.BeingKicked || State == States.Static || State == States.PushingCanceling)
        {
            WorldManager.Instance.CurrentWorld.RemoveBox(this);
            State = States.BeingKicked;
            transform.DOPause();
            StaticCollider.enabled = false;
            DynamicCollider.enabled = true;
            Rigidbody = gameObject.GetComponent<Rigidbody>();
            if (!Rigidbody) Rigidbody = gameObject.AddComponent<Rigidbody>();
            Rigidbody.mass = FinalWeight;
            Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            Rigidbody.drag = Dynamic_Drag * ConfigManager.BoxKickDragFactor_Cheat;
            Rigidbody.angularDrag = 0;
            Rigidbody.velocity = direction * 1.1f;
            Rigidbody.angularVelocity = Vector3.zero;
            Rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            if (direction.x.Equals(0)) Rigidbody.constraints |= RigidbodyConstraints.FreezePositionX;
            if (direction.z.Equals(0)) Rigidbody.constraints |= RigidbodyConstraints.FreezePositionZ;
            Rigidbody.AddForce(direction.normalized * force);
            transform.position = transform.position.ToGridPos3D().ToVector3();
        }
    }

    public bool BeingLift()
    {
        if (State == States.BeingPushed || State == States.Flying || State == States.BeingKicked || State == States.Static || State == States.PushingCanceling)
        {
            WorldManager.Instance.CurrentWorld.RemoveBox(this);
            State = States.BeingLift;
            transform.DOPause();
            StaticCollider.enabled = false;
            DynamicCollider.enabled = false;
            if (Rigidbody)
            {
                DestroyImmediate(Rigidbody);
            }

            return true;
        }

        return false;
    }

    public void Throw(Vector3 direction, float velocity)
    {
        if (State == States.Lifted)
        {
            State = States.Flying;
            transform.DOPause();
            transform.parent = WorldManager.Instance.CurrentWorld.transform;
            StaticCollider.enabled = false;
            DynamicCollider.enabled = true;
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

    void FixedUpdate()
    {
        if ((State == States.BeingKicked || State == States.Flying) && Rigidbody && Rigidbody.velocity.magnitude < 1f)
        {
            DestroyImmediate(Rigidbody);
            StaticCollider.enabled = true;
            DynamicCollider.enabled = false;
            WorldManager.Instance.CurrentWorld.BoxReturnToWorldFromPhysics(this);
        }
    }

    public bool Pushable()
    {
        return BoxType != BoxType.None && BoxType != BoxType.GroundBox && BoxType != BoxType.BorderBox;
    }

    public bool Liftable()
    {
        return BoxType != BoxType.None && BoxType != BoxType.GroundBox && BoxType != BoxType.BorderBox;
    }

    public bool Droppable()
    {
        return BoxType != BoxType.None && BoxType != BoxType.GroundBox && BoxType != BoxType.BorderBox;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (State == States.Flying)
        {
            Rigidbody.drag = Throw_Drag * ConfigManager.BoxThrowDragFactor_Cheat;
        }

        //if ((State == States.Flying || State == States.BeingKicked) && Rigidbody)
        //{
        //    bool stop = false;
        //    if (collision.gameObject.layer == LayerManager.Instance.Layer_Box)
        //    {
        //        Box box = collision.gameObject.GetComponentInParent<Box>();
        //        if (box && box.BoxType != BoxType.GroundBox && box.BoxType != BoxType.None)
        //        {
        //            stop = true;
        //        }
        //    }

        //    if (collision.gameObject.layer == LayerManager.Instance.Layer_HitBox_Player || collision.gameObject.layer == LayerManager.Instance.Layer_HitBox_Enemy)
        //    {
        //        //stop = true;
        //    }

        //    if (stop)
        //    {
        //        Rigidbody.velocity = Rigidbody.velocity * 0.1f;
        //    }
        //}
    }
}

public enum BoxType
{
    None = 0,

    [LabelText("地面箱子")]
    GroundBox = 1,

    [LabelText("墙壁箱子")]
    BorderBox = 2,

    [LabelText("木箱子")]
    WoodenBox = 11,

    [LabelText("金箱子")]
    GoldenBox = 12,

    [LabelText("银箱子")]
    SilverBox = 13,
}
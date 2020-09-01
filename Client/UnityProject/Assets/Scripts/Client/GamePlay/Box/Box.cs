using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ObjectPool;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class Box : PoolObject
{
    private GridSnapper GridSnapper;
    public Collider StaticCollider;
    public Collider DynamicCollider;
    private Rigidbody Rigidbody;

    public override void PoolRecycle()
    {
        base.PoolRecycle();
    }

    [LabelText("箱子类型")]
    public BoxType BoxType;

    [BoxGroup("推箱子属性")]
    [LabelText("重量")]
    [FormerlySerializedAs("Weight")]
    public float Static_Weight;

    [BoxGroup("推箱子属性")]
    [LabelText("抗推力")]
    [PropertyRange(0, 1)]
    [FormerlySerializedAs("Inertia")]
    public float Static_Inertia = 0.5f;

    [BoxGroup("踢箱子属性")]
    [LabelText("阻力")]
    public float Dynamic_Drag = 0.5f;

    [BoxGroup("踢箱子属性")]
    [LabelText("弹性")]
    public float Dynamic_Bounce = 1f;

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
        Moving,
        MovingCanceling,
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
    }

    protected virtual void Start()
    {
        GridSnapper.enabled = false;
    }

    public void Initialize(GridPos3D localGridPos3D, WorldModule module, bool moveLerp)
    {
        lastGP = GridPos3D;
        WorldModule = module;
        GridPos3D = localGridPos3D + module.ModuleGP * WorldModule.MODULE_SIZE;
        LocalGridPos3D = localGridPos3D;
        transform.parent = module.transform;
        if (moveLerp)
        {
            transform.DOPause();
            transform.DOLocalMove(localGridPos3D.ToVector3(), Static_Weight).SetEase(Ease.Linear).OnComplete(() => { State = States.Static; });
            transform.DOLocalRotate(Vector3.zero, Static_Weight);
        }
        else
        {
            transform.localPosition = localGridPos3D.ToVector3();
            transform.localRotation = Quaternion.identity;
            State = States.Static;
        }
    }

    public void Push(Vector3 direction)
    {
        if (State == States.Static || State == States.MovingCanceling)
        {
            Vector3 targetPos = GridPos3D.ToVector3() + direction.normalized;
            GridPos3D gp = GridPos3D.GetGridPosByPoint(targetPos, 1);
            if (gp != GridPos3D)
            {
                WorldManager.Instance.CurrentWorld.MoveBox(GridPos3D, gp, States.Moving);
            }
        }
    }

    public void Kick(Vector3 direction, float force)
    {
        if (State == States.Moving || State == States.Static || State == States.MovingCanceling)
        {
            State = States.BeingKicked;
            WorldManager.Instance.CurrentWorld.RemoveBoxForPhysics(this);
            transform.DOPause();
            transform.localPosition = LocalGridPos3D.ToVector3();
            StaticCollider.enabled = false;
            DynamicCollider.enabled = true;
            Rigidbody = gameObject.GetComponent<Rigidbody>();
            if (!Rigidbody) Rigidbody = gameObject.AddComponent<Rigidbody>();
            Rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            Rigidbody.mass = Static_Weight;
            Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Rigidbody.AddForce(direction.normalized * force);
            Rigidbody.drag = Dynamic_Drag;
            DynamicCollider.material.bounciness = Dynamic_Bounce;
            Rigidbody.velocity = direction * 1.1f;
        }
    }

    public void Throw(Vector3 direction, float velocity)
    {
        State = States.Flying;
        transform.DOPause();
        StaticCollider.enabled = false;
        DynamicCollider.enabled = true;
        Rigidbody = gameObject.GetComponent<Rigidbody>();
        if (!Rigidbody) Rigidbody = gameObject.AddComponent<Rigidbody>();
        Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        Rigidbody.mass = Static_Weight;
        Rigidbody.useGravity = true;
        Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        Rigidbody.drag = 0;
        Rigidbody.angularDrag = 0;
        Rigidbody.velocity = direction.normalized * velocity;
        DynamicCollider.material.bounciness = 0;
    }

    void FixedUpdate()
    {
        bool destroyRigidBody = false;
        destroyRigidBody |= State == States.BeingKicked && Rigidbody && Rigidbody.velocity.magnitude < 1f;
        destroyRigidBody |= State == States.Flying && Rigidbody && Rigidbody.velocity.magnitude < 0.1f;

        if (destroyRigidBody)
        {
            Rigidbody.velocity = Vector3.zero;
            DestroyImmediate(Rigidbody);
            StaticCollider.enabled = true;
            DynamicCollider.enabled = false;
            WorldManager.Instance.CurrentWorld.BoxReturnToWorldFromPhysics(this);
        }
    }

    public void PushCanceled()
    {
        if (State == States.Moving)
        {
            if ((transform.localPosition - LocalGridPos3D.ToVector3()).magnitude > Static_Inertia)
            {
                WorldManager.Instance.CurrentWorld.MoveBox(GridPos3D, lastGP, States.MovingCanceling);
            }
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
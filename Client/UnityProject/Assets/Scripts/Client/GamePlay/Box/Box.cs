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
    [LabelText("惯性")]
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
        }
        else
        {
            transform.localPosition = localGridPos3D.ToVector3();
            State = States.Static;
        }
    }

    public void Push(Vector3 direction)
    {
        if (State == States.Static || State == States.Canceling)
        {
            Vector3 targetPos = GridPos3D.ToVector3() + direction.normalized;
            GridPos3D gp = GridPos3D.GetGridPosByPoint(targetPos, 1);
            if (gp != GridPos3D)
            {
                WorldManager.Instance.CurrentWorld.MoveBox(GridPos3D, gp, States.Moving);
            }
        }
    }

    public void Kick(Vector3 direction, float distance)
    {
        if (State == States.Moving || State == States.Static || State == States.Canceling)
        {
            State = States.BeingKicked;
            WorldManager.Instance.CurrentWorld.RemoveBoxForPhysics(this);
            StaticCollider.enabled = false;
            DynamicCollider.enabled = true;
            transform.DOPause();
            transform.localPosition = LocalGridPos3D.ToVector3();
            Rigidbody = gameObject.GetComponent<Rigidbody>();
            if (!Rigidbody) Rigidbody = gameObject.AddComponent<Rigidbody>();
            Rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            Rigidbody.mass = Static_Weight;
            Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Rigidbody.AddForce(direction * distance);
            Rigidbody.drag = Dynamic_Drag;
            StaticCollider.material.bounciness = Dynamic_Bounce;
            Rigidbody.velocity = direction * 1.1f;
        }
    }

    void FixedUpdate()
    {
        if (State == States.BeingKicked)
        {
            if (Rigidbody && Rigidbody.velocity.magnitude < 1f)
            {
                Rigidbody.velocity = Vector3.zero;
                DestroyImmediate(Rigidbody);
                StaticCollider.enabled = true;
                DynamicCollider.enabled = false;
                WorldManager.Instance.CurrentWorld.BoxReturnToWorldFromPhysics(this);
            }
        }
    }

    public void PushCanceled()
    {
        if (State == States.Moving)
        {
            if ((transform.localPosition - LocalGridPos3D.ToVector3()).magnitude > Static_Inertia)
            {
                WorldManager.Instance.CurrentWorld.MoveBox(GridPos3D, lastGP, States.Canceling);
            }
        }
    }

    public bool Pushable()
    {
        return BoxType != BoxType.None;
    }

    public enum States
    {
        Static,
        Moving,
        BeingKicked,
        Canceling,
    }

    [ReadOnly]
    [LabelText("移动状态")]
    public States State;
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
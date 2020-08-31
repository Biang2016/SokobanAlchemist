using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ObjectPool;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class BoxBase : PoolObject
{
    private GridSnapper GridSnapper;
    private Collider Collider;

    public override void PoolRecycle()
    {
        base.PoolRecycle();
    }

    public BoxType BoxType;

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
        Collider = GetComponent<Collider>();
    }

    protected virtual void Start()
    {
        GridSnapper.enabled = false;
    }

    public void Initialize(GridPos3D gp, WorldModule module, bool moveLerp)
    {
        lastGP = GridPos3D;
        WorldModule = module;
        GridPos3D = gp + module.ModuleGP * WorldModule.MODULE_SIZE;
        LocalGridPos3D = gp;
        transform.parent = module.transform;
        if (moveLerp)
        {
            transform.DOPause();
            transform.DOLocalMove(gp.ToVector3(), Weight).SetEase(Ease.Linear).OnComplete(() => { State = States.Static; });
        }
        else
        {
            transform.localPosition = gp.ToVector3();
            State = States.Static;
        }
    }

    [LabelText("重量")]
    public float Weight;

    [LabelText("惯性")]
    [PropertyRange(0, 1)]
    public float Inertia = 0.5f;

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

    public void PushCanceled()
    {
        if (State == States.Moving)
        {
            if ((transform.localPosition - LocalGridPos3D.ToVector3()).magnitude > Inertia)
            {
                WorldManager.Instance.CurrentWorld.MoveBox(GridPos3D, lastGP, States.Canceling);
            }
        }
    }

    public bool Pushable()
    {
        return BoxType != BoxType.None && BoxType != BoxType.GroundBox && BoxType != BoxType.BorderBox;
    }

    public enum States
    {
        Static,
        Moving,
        Canceling,
    }

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
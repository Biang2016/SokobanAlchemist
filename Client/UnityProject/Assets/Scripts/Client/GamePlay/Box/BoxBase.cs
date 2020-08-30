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

    public GridPos3D GridPos3D;
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
        WorldModule = module;
        GridPos3D = gp + module.ModuleGP * WorldModule.MODULE_SIZE;
        transform.parent = module.transform;
        Vector3 newPosition = GridPos3D.GetLocalPositionByGridPos(gp, transform, 1);
        if (moveLerp)
        {
            IsMoving = true;
            transform.DOLocalMove(newPosition, Weight).OnComplete(() => { IsMoving = false; });
        }
        else
        {
            transform.localPosition = newPosition;
        }
    }

    public float Weight;
    public float PushPowerAccumulated;
    internal bool IsMoving = false;

    public void ResetPush()
    {
        if (!IsMoving)
        {
            PushPowerAccumulated = 0;
        }
    }

    public void Push(float pushPower, Vector3 direction)
    {
        if (!IsMoving)
        {
            PushPowerAccumulated += pushPower;
            if (PushPowerAccumulated > Weight)
            {
                this.PushBox(direction);
                PushPowerAccumulated = 0;
            }
        }
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
}
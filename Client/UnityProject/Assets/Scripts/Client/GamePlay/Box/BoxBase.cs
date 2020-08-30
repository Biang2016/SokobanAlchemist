using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ObjectPool;
using Sirenix.OdinInspector;
using UnityEngine;

public class BoxBase : PoolObject
{
    public override void PoolRecycle()
    {
        base.PoolRecycle();
    }

    public BoxType BoxType;

    public GridPos3D GridPos3D;
    public WorldModule WorldModule;

    void Start()
    {
    }

    public void Initialize(GridPos3D gp, WorldModule module)
    {
        WorldModule = module;
        GridPos3D = gp + module.ModuleGP * WorldModule.MODULE_SIZE;
        transform.parent = module.transform;
        GridPos3D.ApplyGridPosToLocalTrans(gp, transform, 1);
    }

    public float PushThreshold;
    public float PushPowerAccumulated;

    public void ResetPush()
    {
        PushPowerAccumulated = 0;
    }

    public void Push(float pushPower, Vector3 direction)
    {
        PushPowerAccumulated += pushPower;
        if (PushPowerAccumulated > PushThreshold)
        {
            this.PushBox(direction);
            PushPowerAccumulated = 0;
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
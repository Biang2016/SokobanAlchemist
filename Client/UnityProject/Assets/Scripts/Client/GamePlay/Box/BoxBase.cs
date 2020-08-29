using BiangStudio.ObjectPool;
using Sirenix.OdinInspector;

public class BoxBase : PoolObject
{
    public override void PoolRecycle()
    {
        base.PoolRecycle();
    }

    public BoxType BoxType;

    void Start()
    {
    }

    void Update()
    {
    }
}

public enum BoxType
{
    None = 0,

    [LabelText("地面箱子")]
    GroundBox = 1,

    [LabelText("木箱子")]
    WoodenBox = 11,
}
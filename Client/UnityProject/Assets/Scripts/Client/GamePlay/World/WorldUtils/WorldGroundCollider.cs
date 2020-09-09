using BiangStudio.GameDataFormat.Grid;
using UnityEngine;
using BiangStudio.ObjectPool;

public class WorldGroundCollider : PoolObject
{
    private BoxCollider BoxCollider;

    void Awake()
    {
        BoxCollider = GetComponent<BoxCollider>();
    }

    public void Initialize(GridPos3D gp)
    {
        transform.position = gp.ToVector3() * WorldModule.MODULE_SIZE;
        BoxCollider.size = Vector3.one * WorldModule.MODULE_SIZE;
        BoxCollider.center = 0.5f * Vector3.one * (WorldModule.MODULE_SIZE - 1);
    }
}
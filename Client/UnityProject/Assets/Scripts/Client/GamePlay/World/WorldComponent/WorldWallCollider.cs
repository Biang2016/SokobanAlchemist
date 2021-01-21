using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.ObjectPool;
using UnityEngine;

public class WorldWallCollider : PoolObject
{
    private BoxCollider BoxCollider;

    void Awake()
    {
        BoxCollider = GetComponent<BoxCollider>();
    }

    public void Initialize(GridPos3D gp)
    {
        transform.position = gp * WorldModule.MODULE_SIZE;
        BoxCollider.size = Vector3.one * WorldModule.MODULE_SIZE;
        BoxCollider.center = 0.5f * Vector3.one * (WorldModule.MODULE_SIZE - 1);
    }
}
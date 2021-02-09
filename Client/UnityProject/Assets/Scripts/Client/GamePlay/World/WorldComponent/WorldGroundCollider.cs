using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.ObjectPool;
using UnityEngine;

public class WorldGroundCollider : PoolObject
{
    private BoxCollider BoxCollider;

    public override void OnUsed()
    {
        base.OnUsed();
        BoxCollider.enabled = true;
        gameObject.SetActive(true);
    }

    public override void OnRecycled()
    {
        base.OnRecycled();
        BoxCollider.enabled = false;
        gameObject.SetActive(false);
    }

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
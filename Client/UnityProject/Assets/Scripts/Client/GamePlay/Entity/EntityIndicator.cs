using System.Collections;
using BiangLibrary.ObjectPool;
using UnityEngine;

public class EntityIndicator : PoolObject
{
    public BoxCollider BoxCollider;

    public override void OnRecycled()
    {
        base.OnRecycled();
        BoxCollider.enabled = false;
    }

    public override void OnUsed()
    {
        base.OnUsed();
        BoxCollider.enabled = true;
    }
}
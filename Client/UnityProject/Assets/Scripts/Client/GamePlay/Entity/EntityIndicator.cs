using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.ObjectPool;
using UnityEngine;
#if UNITY_EDITOR
#endif

public class EntityIndicator : PoolObject
{
    public BoxCollider BoxCollider;
    internal GridPos3D Offset;

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

#if UNITY_EDITOR
    //public void OnDrawGizmos()
    //{
    //    if (!Application.isPlaying)
    //    {
    //        Gizmos.color = new Color(0,1f,0,0.2f);
    //        Gizmos.DrawCube(transform.position, 0.5f * Vector3.one);
    //    }
    //}

#endif
}
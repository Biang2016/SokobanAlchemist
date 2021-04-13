using System.Collections;
using BiangLibrary;
using BiangLibrary.ObjectPool;
#if UNITY_EDITOR
using UnityEditor;
#endif
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
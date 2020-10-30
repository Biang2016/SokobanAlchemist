﻿using UnityEngine;
using System.Collections;
using BiangStudio.ObjectPool;

public class WorldCameraPOI : PoolObject
{
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}
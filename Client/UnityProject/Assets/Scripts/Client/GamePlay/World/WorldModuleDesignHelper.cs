﻿using System.Collections.Generic;
using System.Linq;
using BiangStudio.GameDataFormat.Grid;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

[ExecuteInEditMode]
public class WorldModuleDesignHelper : MonoBehaviour
{
    public WorldModuleType WorldModuleType;

    public WorldModuleData ExportWorldModuleData()
    {
        List<Box> boxes = GetComponentsInChildren<Box>().ToList();

        WorldModuleData worldModuleData = new WorldModuleData();

        foreach (Box box in boxes)
        {
            GridPos3D gp = GridPos3D.GetGridPosByLocalTrans(box.transform, 1);
            worldModuleData.BoxMatrix[gp.x, gp.y, gp.z] = (byte) box.BoxType;
        }

        worldModuleData.WorldModuleType = WorldModuleType;
        return worldModuleData;
    }

#if UNITY_EDITOR
    public Color RangeGizmoColor;
    public Color RangeGizmoBorderColor;

    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        if (Selection.Contains(gameObject))
        {
            Gizmos.color = RangeGizmoColor;
            Gizmos.DrawCube(Vector3.zero + Vector3.one * (WorldModule.MODULE_SIZE / 2f - 0.5f), Vector3.one * WorldModule.MODULE_SIZE);
        }

        Gizmos.color = RangeGizmoBorderColor;
        Gizmos.DrawWireCube(Vector3.zero + Vector3.one * (WorldModule.MODULE_SIZE / 2f - 0.5f), Vector3.one * WorldModule.MODULE_SIZE);
    }
#endif
}
using System.Collections.Generic;
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
        List<BoxBase> boxes = GetComponentsInChildren<BoxBase>().ToList();

        WorldModuleData worldModuleData = new WorldModuleData();

        foreach (BoxBase box in boxes)
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
        if (Selection.Contains(gameObject))
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = RangeGizmoColor;
            Gizmos.DrawCube(Vector3.zero + Vector3.one * 7.5f, Vector3.one * 16);
            Gizmos.color = RangeGizmoBorderColor;
            Gizmos.DrawWireCube(Vector3.zero + Vector3.one * 7.5f, Vector3.one * 16);
        }
    }
#endif
}
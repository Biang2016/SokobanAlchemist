using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

public class BoxIndicatorHelper : BoxMonoHelper
{
    public List<GridPos3D> BoxIndicatorGPs = new List<GridPos3D>();

    [Button("刷新箱子占用位置坐标")]
    private void RefreshBoxIndicatorOccupationData()
    {
        BoxIndicatorGPs.Clear();
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders)
        {
            GridPos3D gp = new GridPos3D(Mathf.RoundToInt(c.transform.position.x), Mathf.RoundToInt(c.transform.position.y), Mathf.RoundToInt(c.transform.position.z));
            BoxIndicatorGPs.Add(gp);
        }
    }
}
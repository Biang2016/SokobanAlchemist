using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

public class BoxIndicatorHelper : BoxMonoHelper
{
    public List<GridPos3D> BoxIndicatorGPs = new List<GridPos3D>();

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
        gameObject.SetActive(true);
    }

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
        gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    [Button("刷新箱子占用位置坐标")]
    public void RefreshBoxIndicatorOccupationData()
    {
        BoxIndicatorGPs.Clear();
        Collider[] colliders = GetComponentsInChildren<Collider>();
        bool validOccupation = false;
        foreach (Collider c in colliders)
        {
            GridPos3D gp = new GridPos3D(Mathf.RoundToInt(c.transform.position.x), Mathf.RoundToInt(c.transform.position.y), Mathf.RoundToInt(c.transform.position.z));
            BoxIndicatorGPs.Add(gp);
            if (gp == GridPos3D.Zero) validOccupation = true;
        }

        if (!validOccupation)
        {
            GameObject boxPrefab = PrefabUtility.GetNearestPrefabInstanceRoot(gameObject);
            Debug.LogError($"{boxPrefab.name}的箱子占位配置错误，必须要有一个BoxIndicator位于(0,0,0)");
        }
    }
#endif
}
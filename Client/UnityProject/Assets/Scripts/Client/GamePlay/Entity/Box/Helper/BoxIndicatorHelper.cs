using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

public class BoxIndicatorHelper : BoxMonoHelper
{
    [LabelText("箱子占位信息")]
    public BoxOccupationData BoxOccupationData = new BoxOccupationData();

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
        IsOn = true;
    }

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
        IsOn = false;
    }

    private bool isOn = true;

    public bool IsOn
    {
        get { return isOn; }
        set
        {
            isOn = value;
            gameObject.SetActive(value);
        }
    }

#if UNITY_EDITOR
    public void RefreshBoxIndicatorOccupationData()
    {
        BoxOccupationData.Clear();
        Collider[] colliders = GetComponentsInChildren<Collider>();
        bool validOccupation = false;
        foreach (Collider c in colliders)
        {
            GridPos3D gp = new GridPos3D(Mathf.RoundToInt(c.transform.position.x), Mathf.RoundToInt(c.transform.position.y), Mathf.RoundToInt(c.transform.position.z));
            BoxOccupationData.BoxIndicatorGPs.Add(gp);
            if (gp == GridPos3D.Zero) validOccupation = true;
        }

        if (!validOccupation)
        {
            GameObject boxPrefab = PrefabUtility.GetNearestPrefabInstanceRoot(gameObject);
            Debug.LogError($"{boxPrefab.name}的箱子占位配置错误，必须要有一个BoxIndicator位于(0,0,0)");
            BoxOccupationData.IsBoxShapeCuboid = false;
            return;
        }

        BoxOccupationData.IsBoxShapeCuboid = false;
        BoxOccupationData.BoundsInt = BoxOccupationData.BoxIndicatorGPs.GetBoundingRectFromListGridPos(GridPos3D.Zero);
        bool[,,] occupationMatrix = new bool[BoxOccupationData.BoundsInt.size.x, BoxOccupationData.BoundsInt.size.y, BoxOccupationData.BoundsInt.size.z];
        foreach (GridPos3D offset in BoxOccupationData.BoxIndicatorGPs)
        {
            occupationMatrix[offset.x - BoxOccupationData.BoundsInt.xMin, offset.y - BoxOccupationData.BoundsInt.yMin, offset.z - BoxOccupationData.BoundsInt.zMin] = true;
        }

        BoxOccupationData.IsBoxShapeCuboid = true;
        foreach (bool b in occupationMatrix)
        {
            if (!b)
            {
                BoxOccupationData.IsBoxShapeCuboid = false;
                break;
            }
        }
    }
#endif
}
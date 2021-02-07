using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class BoxOccupationData : IClone<BoxOccupationData>
{
    [LabelText("长方体外形")]
    public bool IsBoxShapeCuboid = false;

    [LabelText("包围尺寸")]
    [SerializeField]
    public BoundsInt BoundsInt;

    [LabelText("格子占位信息")]
    [ListDrawerSettings(ListElementLabelName = "ToString")]
    public List<GridPos3D> BoxIndicatorGPs = new List<GridPos3D>();

    public Dictionary<GridPosR.Orientation, List<GridPos3D>> BoxIndicatorGPs_RotatedDict;

    public void Clear()
    {
        IsBoxShapeCuboid = false;
        BoundsInt = default;
        BoxIndicatorGPs.Clear();
    }

    public void CalculateEveryOrientationOccupationGPs()
    {
        if (BoxIndicatorGPs_RotatedDict == null) BoxIndicatorGPs_RotatedDict = new Dictionary<GridPosR.Orientation, List<GridPos3D>>();
        BoxIndicatorGPs_RotatedDict.Clear();
        BoxIndicatorGPs_RotatedDict.Add(GridPosR.Orientation.Up, GridPos3D.TransformOccupiedPositions_XZ(GridPosR.Orientation.Up, BoxIndicatorGPs));
        BoxIndicatorGPs_RotatedDict.Add(GridPosR.Orientation.Right, GridPos3D.TransformOccupiedPositions_XZ(GridPosR.Orientation.Right, BoxIndicatorGPs));
        BoxIndicatorGPs_RotatedDict.Add(GridPosR.Orientation.Down, GridPos3D.TransformOccupiedPositions_XZ(GridPosR.Orientation.Down, BoxIndicatorGPs));
        BoxIndicatorGPs_RotatedDict.Add(GridPosR.Orientation.Left, GridPos3D.TransformOccupiedPositions_XZ(GridPosR.Orientation.Left, BoxIndicatorGPs));
    }

    public BoxOccupationData Clone()
    {
        BoxOccupationData newData = new BoxOccupationData();
        newData.IsBoxShapeCuboid = IsBoxShapeCuboid;
        newData.BoundsInt = BoundsInt;
        newData.BoxIndicatorGPs = BoxIndicatorGPs.Clone();
        return newData;
    }
}
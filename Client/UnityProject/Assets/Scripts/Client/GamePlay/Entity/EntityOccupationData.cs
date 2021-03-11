using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntityOccupationData : IClone<EntityOccupationData>
{
    [LabelText("长方体外形")]
    public bool IsShapeCuboid = false;

    [LabelText("正方形底面")]
    public bool IsShapePlanSquare = false;

    [LabelText("包围尺寸")]
    [SerializeField]
    public BoundsInt BoundsInt;

    [LabelText("格子占位信息")]
    [ListDrawerSettings(ListElementLabelName = "ToString")]
    public List<GridPos3D> EntityIndicatorGPs = new List<GridPos3D>();

    public Dictionary<GridPosR.Orientation, List<GridPos3D>> EntityIndicatorGPs_RotatedDict;

    public void Clear()
    {
        IsShapeCuboid = false;
        IsShapePlanSquare = false;
        BoundsInt = default;
        EntityIndicatorGPs.Clear();
    }

    public void CalculateEveryOrientationOccupationGPs()
    {
        if (EntityIndicatorGPs_RotatedDict == null) EntityIndicatorGPs_RotatedDict = new Dictionary<GridPosR.Orientation, List<GridPos3D>>();
        EntityIndicatorGPs_RotatedDict.Clear();
        EntityIndicatorGPs_RotatedDict.Add(GridPosR.Orientation.Up, GridPos3D.TransformOccupiedPositions_XZ(GridPosR.Orientation.Up, EntityIndicatorGPs));
        EntityIndicatorGPs_RotatedDict.Add(GridPosR.Orientation.Right, GridPos3D.TransformOccupiedPositions_XZ(GridPosR.Orientation.Right, EntityIndicatorGPs));
        EntityIndicatorGPs_RotatedDict.Add(GridPosR.Orientation.Down, GridPos3D.TransformOccupiedPositions_XZ(GridPosR.Orientation.Down, EntityIndicatorGPs));
        EntityIndicatorGPs_RotatedDict.Add(GridPosR.Orientation.Left, GridPos3D.TransformOccupiedPositions_XZ(GridPosR.Orientation.Left, EntityIndicatorGPs));
    }

    public EntityOccupationData Clone()
    {
        EntityOccupationData newData = new EntityOccupationData();
        newData.IsShapeCuboid = IsShapeCuboid;
        newData.IsShapePlanSquare = IsShapePlanSquare;
        newData.BoundsInt = BoundsInt;
        newData.EntityIndicatorGPs = EntityIndicatorGPs.Clone();
        return newData;
    }
}
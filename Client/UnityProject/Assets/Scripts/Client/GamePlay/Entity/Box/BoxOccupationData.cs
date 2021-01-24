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

    public void Clear()
    {
        IsBoxShapeCuboid = false;
        BoundsInt = default;
        BoxIndicatorGPs.Clear();
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
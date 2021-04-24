using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntityOccupationData : IClone<EntityOccupationData>
{
    [LabelText("是否是Trigger实体")]
    public bool IsTriggerEntity;

    [LabelText("长方体外形")]
    public bool IsShapeCuboid = false;

    [LabelText("正方形底面")]
    public bool IsShapePlanSquare = false;

    [BoxGroup("角色")]
    [LabelText("角色体宽")]
    public int ActorWidth = 1;

    [BoxGroup("角色")]
    [LabelText("角色身高")]
    public int ActorHeight = 1;

    [BoxGroup("箱子")]
    [LabelText("可穿过")]
    public bool Passable = false;

    [LabelText("包围尺寸")]
    [SerializeField]
    public BoundsInt BoundsInt;

    [LabelText("格子占位信息")]
    [ListDrawerSettings(ListElementLabelName = "ToString")]
    public List<GridPos3D> EntityIndicatorGPs = new List<GridPos3D>();

    [LabelText("形心局部坐标")]
    public Vector3 LocalGeometryCenter = Vector3.zero;

    public Dictionary<GridPosR.Orientation, List<GridPos3D>> EntityIndicatorGPs_RotatedDict;
    public Dictionary<GridPosR.Orientation, Vector3> LocalGeometryCenter_RotatedDict;

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

        if (LocalGeometryCenter_RotatedDict == null) LocalGeometryCenter_RotatedDict = new Dictionary<GridPosR.Orientation, Vector3>();
        LocalGeometryCenter_RotatedDict.Clear();
        LocalGeometryCenter_RotatedDict.Add(GridPosR.Orientation.Up, LocalGeometryCenter);
        LocalGeometryCenter_RotatedDict.Add(GridPosR.Orientation.Right, new Vector3(LocalGeometryCenter.x, -LocalGeometryCenter.z));
        LocalGeometryCenter_RotatedDict.Add(GridPosR.Orientation.Down, new Vector3(-LocalGeometryCenter.x, -LocalGeometryCenter.z));
        LocalGeometryCenter_RotatedDict.Add(GridPosR.Orientation.Left, new Vector3(LocalGeometryCenter.x, LocalGeometryCenter.z));
    }

    public EntityOccupationData Clone()
    {
        EntityOccupationData newData = new EntityOccupationData();
        newData.IsTriggerEntity = IsTriggerEntity;
        newData.IsShapeCuboid = IsShapeCuboid;
        newData.IsShapePlanSquare = IsShapePlanSquare;
        newData.ActorWidth = ActorWidth;
        newData.ActorHeight = ActorHeight;
        newData.Passable = Passable;
        newData.BoundsInt = BoundsInt;
        newData.LocalGeometryCenter = LocalGeometryCenter;
        newData.EntityIndicatorGPs = EntityIndicatorGPs.Clone<GridPos3D, GridPos3D>();
        return newData;
    }
}
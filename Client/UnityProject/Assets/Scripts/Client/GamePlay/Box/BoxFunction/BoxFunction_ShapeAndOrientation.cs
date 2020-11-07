using System;
using BiangStudio.GameDataFormat.Grid;
using Sirenix.OdinInspector;

[Serializable]
public class BoxFunction_ShapeAndOrientation : BoxFunctionBase
{
    protected override string BoxFunctionDisplayName => "导出形状和朝向";

    [LabelText("形状")]
    public BoxShapeType BoxShapeType = BoxShapeType.Box;

    [LabelText("朝向")]
    public GridPosR.Orientation Orientation = GridPosR.Orientation.Up;

    protected override void ChildClone(BoxFunctionBase newBF)
    {
        base.ChildClone(newBF);
        BoxFunction_ShapeAndOrientation bf = ((BoxFunction_ShapeAndOrientation) newBF);
        bf.BoxShapeType = BoxShapeType;
        bf.Orientation = Orientation;
    }

    public override void CopyDataFrom(BoxFunctionBase srcData)
    {
        base.CopyDataFrom(srcData);
        BoxFunction_ShapeAndOrientation bf = ((BoxFunction_ShapeAndOrientation) srcData);
        BoxShapeType = bf.BoxShapeType;
        Orientation = bf.Orientation;
    }
}
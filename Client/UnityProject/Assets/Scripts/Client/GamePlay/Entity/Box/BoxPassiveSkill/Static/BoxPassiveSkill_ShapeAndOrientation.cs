using System;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;

[Serializable]
public class BoxPassiveSkill_ShapeAndOrientation : BoxPassiveSkill
{
    protected override string Description => "导出形状和朝向";

    [LabelText("形状")]
    public BoxShapeType BoxShapeType = BoxShapeType.Box;

    [LabelText("朝向")]
    public GridPosR.Orientation Orientation = GridPosR.Orientation.Up;

    protected override void ChildClone(BoxPassiveSkill newBF)
    {
        base.ChildClone(newBF);
        BoxPassiveSkill_ShapeAndOrientation bf = ((BoxPassiveSkill_ShapeAndOrientation) newBF);
        bf.BoxShapeType = BoxShapeType;
        bf.Orientation = Orientation;
    }

    public override void CopyDataFrom(BoxPassiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        BoxPassiveSkill_ShapeAndOrientation bf = ((BoxPassiveSkill_ShapeAndOrientation) srcData);
        BoxShapeType = bf.BoxShapeType;
        Orientation = bf.Orientation;
    }
}
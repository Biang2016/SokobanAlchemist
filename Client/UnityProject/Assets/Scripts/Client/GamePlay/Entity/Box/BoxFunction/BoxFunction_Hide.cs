using System;
using Sirenix.OdinInspector;

[Serializable]
[LabelText("隐藏")]
public class BoxFunction_Hide : BoxFunctionBase
{
    protected override string BoxFunctionDisplayName => "世界特例专用，在世界中不生成此Box";

    protected override void ChildClone(BoxFunctionBase newBF)
    {
        base.ChildClone(newBF);
        BoxFunction_Hide bf = ((BoxFunction_Hide) newBF);
    }

    public override void CopyDataFrom(BoxFunctionBase srcData)
    {
        base.CopyDataFrom(srcData);
        BoxFunction_Hide bf = ((BoxFunction_Hide) srcData);
    }
}
using System;
using Sirenix.OdinInspector;

[assembly: Sirenix.Serialization.BindTypeNameToType("BoxFunction_Hide", typeof(BoxPassiveSkill_Hide))]

[Serializable]
[LabelText("隐藏")]
public class BoxPassiveSkill_Hide : BoxPassiveSkill
{
    protected override string Description => "世界特例专用，在世界中不生成此Box";

    protected override void ChildClone(BoxPassiveSkill newBF)
    {
        base.ChildClone(newBF);
        BoxPassiveSkill_Hide bf = ((BoxPassiveSkill_Hide) newBF);
    }

    public override void CopyDataFrom(BoxPassiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        BoxPassiveSkill_Hide bf = ((BoxPassiveSkill_Hide) srcData);
    }
}
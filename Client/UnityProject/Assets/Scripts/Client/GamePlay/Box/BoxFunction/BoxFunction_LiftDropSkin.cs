using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class BoxFunction_LiftDropSkin : BoxFunctionBase
{
    protected override string BoxFunctionDisplayName => "举起箱子更换皮肤";

    [GUIColor(0, 1.0f, 0)]
    [LabelText("皮肤")]
    public Material DieDropMaterial;

    public override void OnBeingLift(Actor actor)
    {
        base.OnBeingLift(actor);
        actor.ActorSkinHelper.SwitchSkin(DieDropMaterial);
    }

    protected override void ChildClone(BoxFunctionBase newBF)
    {
        base.ChildClone(newBF);
        BoxFunction_LiftDropSkin bf = ((BoxFunction_LiftDropSkin) newBF);
        bf.DieDropMaterial = DieDropMaterial;
    }

    public override void CopyDataFrom(BoxFunctionBase srcData)
    {
        base.CopyDataFrom(srcData);
        BoxFunction_LiftDropSkin bf = ((BoxFunction_LiftDropSkin) srcData);
        DieDropMaterial = bf.DieDropMaterial;
    }
}
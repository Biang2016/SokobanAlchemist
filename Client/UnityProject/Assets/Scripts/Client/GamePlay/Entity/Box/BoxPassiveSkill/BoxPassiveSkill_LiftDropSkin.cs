using System;
using Sirenix.OdinInspector;
using UnityEngine;

[assembly: Sirenix.Serialization.BindTypeNameToType("BoxFunction_LiftDropSkin", typeof(BoxPassiveSkill_LiftDropSkin))]

[Serializable]
public class BoxPassiveSkill_LiftDropSkin : BoxPassiveSkill
{
    protected override string BoxPassiveSkillDisplayName => "举起箱子更换皮肤";

    [GUIColor(0, 1.0f, 0)]
    [LabelText("皮肤")]
    public Material DieDropMaterial;

    public override void OnBeingLift(Actor actor)
    {
        base.OnBeingLift(actor);
        actor.ActorSkinHelper.SwitchSkin(DieDropMaterial);
    }

    protected override void ChildClone(BoxPassiveSkill newBF)
    {
        base.ChildClone(newBF);
        BoxPassiveSkill_LiftDropSkin bf = ((BoxPassiveSkill_LiftDropSkin) newBF);
        bf.DieDropMaterial = DieDropMaterial;
    }

    public override void CopyDataFrom(BoxPassiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        BoxPassiveSkill_LiftDropSkin bf = ((BoxPassiveSkill_LiftDropSkin) srcData);
        DieDropMaterial = bf.DieDropMaterial;
    }
}
using System;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntitySkillAction_PlayFXOnWorldGP : EntitySkillAction, EntitySkillAction.IWorldGPAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "播放特效";

    [LabelText("@\"特效\t\"+FX")]
    public FXConfig FX = new FXConfig();

    public void ExecuteOnWorldGP(GridPos3D worldGP)
    {
        FXManager.Instance.PlayFX(FX, worldGP);
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_PlayFXOnWorldGP action = ((EntitySkillAction_PlayFXOnWorldGP) newAction);
        action.FX = FX.Clone();
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_PlayFXOnWorldGP action = ((EntitySkillAction_PlayFXOnWorldGP) srcData);
        FX.CopyDataFrom(action.FX);
    }
}
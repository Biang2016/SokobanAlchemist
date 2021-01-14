using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class BoxPassiveSkillAction_PlayFX : BoxPassiveSkillAction, BoxPassiveSkillAction.IPureAction
{
    protected override string Description => "播放特效";

    [ValueDropdown("GetAllFXTypeNames")]
    public string FXTypeName;

    public int FXScale = 1;

    public void Execute()
    {
        FXManager.Instance.PlayFX(FXTypeName, Box.transform.position, FXScale);
    }


    protected override void ChildClone(BoxPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        BoxPassiveSkillAction_PlayFX action = ((BoxPassiveSkillAction_PlayFX) newAction);
        action.FXTypeName = FXTypeName;
        action.FXScale = FXScale;
    }

    public override void CopyDataFrom(BoxPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        BoxPassiveSkillAction_PlayFX action = ((BoxPassiveSkillAction_PlayFX) srcData);
        FXTypeName = action.FXTypeName;
        action.FXScale = FXScale;
    }
}
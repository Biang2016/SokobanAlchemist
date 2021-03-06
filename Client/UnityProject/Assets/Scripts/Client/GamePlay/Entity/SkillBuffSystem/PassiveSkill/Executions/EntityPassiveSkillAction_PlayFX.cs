using System;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntityPassiveSkillAction_PlayFX : EntityPassiveSkillAction, EntityPassiveSkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "播放特效";

    [ValueDropdown("GetAllFXTypeNames")]
    public string FXTypeName;

    public float FXScale = 1;

    public void Execute()
    {
        if (Entity is Box box)
        {
            foreach (GridPos3D offset in box.GetEntityOccupationGPs_Rotated())
            {
                Vector3 boxIndicatorPos = box.transform.position + offset;
                FXManager.Instance.PlayFX(FXTypeName, boxIndicatorPos, FXScale);
            }
        }
        else if (Entity is Actor actor)
        {
            FXManager.Instance.PlayFX(FXTypeName, actor.transform.position, FXScale);
        }
    }

    protected override void ChildClone(EntityPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        EntityPassiveSkillAction_PlayFX action = ((EntityPassiveSkillAction_PlayFX) newAction);
        action.FXTypeName = FXTypeName;
        action.FXScale = FXScale;
    }

    public override void CopyDataFrom(EntityPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntityPassiveSkillAction_PlayFX action = ((EntityPassiveSkillAction_PlayFX) srcData);
        FXTypeName = action.FXTypeName;
        FXScale = action.FXScale;
    }
}
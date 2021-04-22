using System;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntitySkillAction_PlayFX : EntitySkillAction, EntitySkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "播放特效";

    [LabelText("@\"特效\t\"+FX")]
    public FXConfig FX = new FXConfig();

    public void Execute()
    {
        if (Entity is Box box)
        {
            foreach (GridPos3D offset in box.GetEntityOccupationGPs_Rotated())
            {
                Vector3 boxIndicatorPos = box.transform.position + offset;
                FXManager.Instance.PlayFX(FX, boxIndicatorPos);
            }
        }
        else if (Entity is Actor actor)
        {
            FXManager.Instance.PlayFX(FX, actor.transform.position);
        }
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_PlayFX action = ((EntitySkillAction_PlayFX) newAction);
        action.FX = FX.Clone();
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_PlayFX action = ((EntitySkillAction_PlayFX) srcData);
        FX.CopyDataFrom(action.FX);
    }
}
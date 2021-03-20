using System;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class EntityPassiveSkillAction_PlayFX : EntityPassiveSkillAction, EntityPassiveSkillAction.IPureAction
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

    protected override void ChildClone(EntityPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        EntityPassiveSkillAction_PlayFX action = ((EntityPassiveSkillAction_PlayFX) newAction);
        action.FX = FX.Clone();
    }

    public override void CopyDataFrom(EntityPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntityPassiveSkillAction_PlayFX action = ((EntityPassiveSkillAction_PlayFX) srcData);
        FX.CopyDataFrom(action.FX);
    }
}
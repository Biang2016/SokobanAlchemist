using System;
using System.Collections.Generic;
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

    [LabelText("每格都播放特效")]
    public bool PlayFXForEveryGrid = true;

    public void Execute()
    {
        if (PlayFXForEveryGrid)
        {
            if (Entity == null) return;
            GridPos3D worldGP = Entity.WorldGP;
            if (Entity is Box box)
            {
                if (box.State != Box.States.Static)
                {
                    worldGP = box.transform.position.ToGridPos3D();
                }
            }

            List<GridPos3D> occupations = Entity.GetEntityOccupationGPs_Rotated();
            foreach (GridPos3D gridPos in occupations)
            {
                GridPos3D gridWorldGP = worldGP + gridPos;
                FXManager.Instance.PlayFX(FX, gridWorldGP);
            }
        }
        else
        {
            FXManager.Instance.PlayFX(FX, Entity.EntityGeometryCenter);
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
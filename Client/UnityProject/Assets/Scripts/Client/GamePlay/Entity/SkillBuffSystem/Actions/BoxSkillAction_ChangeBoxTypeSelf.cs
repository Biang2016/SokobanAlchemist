using System;
using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;

[Serializable]
public class BoxSkillAction_ChangeBoxTypeSelf : BoxSkillAction, EntitySkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "更改箱子类型(仅箱子使用)";

    [HideLabel]
    public EntityData EntityData = new EntityData();

    [LabelText("每一格都生成一个")]
    public bool ChangeForEveryGrid = false;

    public void Execute()
    {
        GridPos3D worldGP = GridPos3D.Zero;
        if (Box.State == Box.States.Static)
        {
            worldGP = Box.WorldGP;
        }
        else
        {
            worldGP = Box.transform.position.ToGridPos3D();
        }

        if (ChangeForEveryGrid)
        {
            List<GridPos3D> occupations = Box.GetEntityOccupationGPs_Rotated();
            Box.DestroySelfByModuleRecycle();
            foreach (GridPos3D gridPos in occupations)
            {
                GridPos3D gridWorldGP = worldGP + gridPos;
                WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(gridWorldGP);
                if (module != null) module.GenerateEntity(EntityData.Clone(), gridWorldGP);
            }
        }
        else
        {
            Box.DestroySelfByModuleRecycle();
            WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(worldGP);
            if (module != null) module.GenerateEntity(EntityData.Clone(), worldGP);
        }
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        BoxSkillAction_ChangeBoxTypeSelf action = ((BoxSkillAction_ChangeBoxTypeSelf) newAction);
        action.EntityData = EntityData.Clone();
        action.ChangeForEveryGrid = ChangeForEveryGrid;
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        BoxSkillAction_ChangeBoxTypeSelf action = ((BoxSkillAction_ChangeBoxTypeSelf) srcData);
        EntityData = action.EntityData.Clone();
        ChangeForEveryGrid = action.ChangeForEveryGrid;
    }
}
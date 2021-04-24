using System;
using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;

[Serializable]
public class EntitySkillAction_ChangeBoxType : EntitySkillAction, EntitySkillAction.IEntityAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "更改箱子类型";

    [HideLabel]
    public EntityData EntityData = new EntityData();

    [LabelText("每一格都生成一个")]
    public bool ChangeForEveryGrid = false;

    public void ExecuteOnEntity(Entity entity)
    {
        GridPos3D worldGP = entity.WorldGP;
        if (entity is Box box)
        {
            if (box.State != Box.States.Static)
            {
                worldGP = box.transform.position.ToGridPos3D();
            }
        }

        if (ChangeForEveryGrid)
        {
            List<GridPos3D> occupations = entity.GetEntityOccupationGPs_Rotated();
            entity.DestroySelfByModuleRecycle();
            foreach (GridPos3D gridPos in occupations)
            {
                GridPos3D gridWorldGP = worldGP + gridPos;
                WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(gridWorldGP);
                if (module != null) module.GenerateEntity(EntityData.Clone(), gridWorldGP);
            }
        }
        else
        {
            entity.DestroySelfByModuleRecycle();
            WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(worldGP);
            if (module != null) module.GenerateEntity(EntityData.Clone(), worldGP);
        }
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_ChangeBoxType action = ((EntitySkillAction_ChangeBoxType) newAction);
        action.EntityData = EntityData.Clone();
        action.ChangeForEveryGrid = ChangeForEveryGrid;
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_ChangeBoxType action = ((EntitySkillAction_ChangeBoxType) srcData);
        EntityData = action.EntityData.Clone();
        ChangeForEveryGrid = action.ChangeForEveryGrid;
    }
}
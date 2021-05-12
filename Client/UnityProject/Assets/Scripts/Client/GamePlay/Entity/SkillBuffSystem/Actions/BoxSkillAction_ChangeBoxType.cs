using System;
using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

[Serializable]
public class BoxSkillAction_ChangeBoxType : BoxSkillAction, EntitySkillAction.IPureAction, EntitySkillAction.IEntityAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "更改箱子类型(仅箱子使用)";

    [HideLabel]
    public EntityData EntityData = new EntityData();

    [LabelText("每一格都生成一个")]
    public bool ChangeForEveryGrid = false;

    [LabelText("True:对目标Entity生效; False:对本Entity生效")]
    public bool ExertOnTarget;

    public void ExecuteOnEntity(Entity entity)
    {
        if (!ExertOnTarget) return;
        ExecuteCore(entity);
    }

    public void Execute()
    {
        if (ExertOnTarget) return;
        ExecuteCore(Box);
    }

    private void ExecuteCore(Entity target)
    {
        GridPos3D worldGP = GridPos3D.Zero;
        Box targetBox = (Box) target;
        if (targetBox.State == Box.States.Static)
        {
            worldGP = target.WorldGP;
        }
        else
        {
            worldGP = targetBox.transform.position.ToGridPos3D();
        }

        if (ChangeForEveryGrid)
        {
            List<GridPos3D> occupations = targetBox.GetEntityOccupationGPs_Rotated();
            targetBox.DestroySelfWithoutSideEffect();
            foreach (GridPos3D gridPos in occupations)
            {
                GridPos3D gridWorldGP = worldGP + gridPos;
                WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(gridWorldGP);
                if (module != null) module.GenerateEntity(EntityData.Clone(), gridWorldGP);
            }
        }
        else
        {
            targetBox.DestroySelfWithoutSideEffect();
            WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(worldGP);
            if (module != null) module.GenerateEntity(EntityData.Clone(), worldGP);
        }
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        BoxSkillAction_ChangeBoxType action = ((BoxSkillAction_ChangeBoxType) newAction);
        action.EntityData = EntityData.Clone();
        action.ChangeForEveryGrid = ChangeForEveryGrid;
        action.ExertOnTarget = ExertOnTarget;
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        BoxSkillAction_ChangeBoxType action = ((BoxSkillAction_ChangeBoxType) srcData);
        EntityData = action.EntityData.Clone();
        ChangeForEveryGrid = action.ChangeForEveryGrid;
        ExertOnTarget = action.ExertOnTarget;
    }
}
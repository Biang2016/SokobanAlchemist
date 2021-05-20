using System;
using Sirenix.OdinInspector;

[Serializable]
public class EntitySkillAction_ClearActorResources : EntitySkillAction, EntitySkillAction.IEntityAction, EntitySkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "清空角色资产";

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
        ExecuteCore(Entity);
    }

    private void ExecuteCore(Entity target)
    {
        target.EntityStatPropSet.Gold.SetValue(0);
        target.EntityStatPropSet.FireElementFragment.SetValue(0);
        target.EntityStatPropSet.IceElementFragment.SetValue(0);
        target.EntityStatPropSet.LightningElementFragment.SetValue(0);
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_ClearActorResources action = ((EntitySkillAction_ClearActorResources) newAction);
        action.ExertOnTarget = ExertOnTarget;
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_ClearActorResources action = ((EntitySkillAction_ClearActorResources) srcData);
        ExertOnTarget = action.ExertOnTarget;
    }
}
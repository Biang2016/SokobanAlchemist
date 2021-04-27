using System;
using Sirenix.OdinInspector;

[Serializable]
public class EntitySkillAction_ClearEnemyThreat : EntitySkillAction, EntitySkillAction.IPureAction, EntitySkillAction.IEntityAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "清除敌人仇恨";

    [LabelText("True会被攻击False不会")]
    public bool CanBeThreatened;

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
        target.CanBeThreatened = CanBeThreatened;
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_ClearEnemyThreat action = ((EntitySkillAction_ClearEnemyThreat) newAction);
        action.CanBeThreatened = CanBeThreatened;
        action.ExertOnTarget = ExertOnTarget;
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_ClearEnemyThreat action = ((EntitySkillAction_ClearEnemyThreat) srcData);
        CanBeThreatened = action.CanBeThreatened;
        ExertOnTarget = action.ExertOnTarget;
    }
}
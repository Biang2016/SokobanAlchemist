using System;
using Sirenix.OdinInspector;

[Serializable]
public class EntitySkillAction_SetCanBeThreatened : EntitySkillAction, EntitySkillAction.IPureAction, EntitySkillAction.IEntityAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "设置能否被威胁";

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
        if (!CanBeThreatened)
        {
            BattleManager.Instance.SetActorInCamp(true);
        }
        else
        {
            BattleManager.Instance.SetActorInCamp(false);
        }
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_SetCanBeThreatened action = ((EntitySkillAction_SetCanBeThreatened) newAction);
        action.CanBeThreatened = CanBeThreatened;
        action.ExertOnTarget = ExertOnTarget;
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_SetCanBeThreatened action = ((EntitySkillAction_SetCanBeThreatened) srcData);
        CanBeThreatened = action.CanBeThreatened;
        ExertOnTarget = action.ExertOnTarget;
    }
}
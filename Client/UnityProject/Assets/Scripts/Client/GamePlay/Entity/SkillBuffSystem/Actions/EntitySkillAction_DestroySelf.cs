using System;

[Serializable]
public class EntitySkillAction_DestroySelf : EntitySkillAction, EntitySkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "毁灭自己";

    public void Execute()
    {
        Entity.PassiveSkillMarkAsDestroyed = true;
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_DestroySelf action = ((EntitySkillAction_DestroySelf) newAction);
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_DestroySelf action = ((EntitySkillAction_DestroySelf) srcData);
    }
}
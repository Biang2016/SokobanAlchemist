using System;
[Serializable]
public class EntityPassiveSkillAction_DestroySelf : EntityPassiveSkillAction, EntityPassiveSkillAction.IPureAction
{
    protected override string Description => "毁灭自己";

    public void Execute()
    {
        Entity.PassiveSkillMarkAsDestroyed = true;
    }

    protected override void ChildClone(EntityPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        EntityPassiveSkillAction_DestroySelf action = ((EntityPassiveSkillAction_DestroySelf) newAction);
    }

    public override void CopyDataFrom(EntityPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntityPassiveSkillAction_DestroySelf action = ((EntityPassiveSkillAction_DestroySelf) srcData);
    }
}
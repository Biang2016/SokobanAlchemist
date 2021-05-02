using System;
using BiangLibrary.GamePlay.UI;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntitySkillAction_LearnUpgrade : EntitySkillAction, EntitySkillAction.IEntityAction, EntitySkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "改变角色属性值";

    public EntityUpgrade EntityUpgrade;

    [LabelText("True:对目标Entity生效; False:对本Entity生效")]
    public bool ExertOnTarget;

    public void ExecuteOnEntity(Entity entity)
    {
        if (!ExertOnTarget) return;
        entity.GetUpgraded(EntityUpgrade);
    }

    public void Execute()
    {
        if (ExertOnTarget) return;
        Entity.GetUpgraded(EntityUpgrade);
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_LearnUpgrade action = ((EntitySkillAction_LearnUpgrade) newAction);
        action.EntityUpgrade = EntityUpgrade.Clone();
        action.ExertOnTarget = ExertOnTarget;
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_LearnUpgrade action = ((EntitySkillAction_LearnUpgrade) srcData);
        EntityUpgrade = action.EntityUpgrade.Clone();
        ExertOnTarget = action.ExertOnTarget;
    }
}
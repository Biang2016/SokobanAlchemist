using System;
using BiangLibrary.GamePlay.UI;
using UnityEngine;

[Serializable]
public class EntitySkillAction_ShowLearnUpgradePanel : EntitySkillAction, EntitySkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "改变角色属性值";

    public EntityUpgrade EntityUpgrade;

    public void Execute()
    {
        LearnUpgradePanel learnUpgradePanel = UIManager.Instance.ShowUIForms<LearnUpgradePanel>();
        learnUpgradePanel.Initialize(EntityUpgrade, OnLearned);
    }

    private void OnLearned()
    {
        Entity.DestroySelf();
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_ShowLearnUpgradePanel action = ((EntitySkillAction_ShowLearnUpgradePanel) newAction);
        action.EntityUpgrade = EntityUpgrade.Clone();
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_ShowLearnUpgradePanel action = ((EntitySkillAction_ShowLearnUpgradePanel) srcData);
        EntityUpgrade = action.EntityUpgrade.Clone();
    }
}
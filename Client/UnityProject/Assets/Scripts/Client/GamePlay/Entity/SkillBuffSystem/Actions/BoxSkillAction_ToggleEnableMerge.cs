using System;
using Sirenix.OdinInspector;

[Serializable]
public class BoxSkillAction_ToggleEnableMerge : BoxSkillAction, EntitySkillAction.IPureAction, EntitySkillAction.IEntityAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "开关合成";

    [LabelText("启用合成")]
    public bool EnableMerge = true;

    [LabelText("True:对目标Entity生效; False:对本Entity生效")]
    public bool ExertOnTarget;

    public void Execute()
    {
        if (ExertOnTarget) return;
        Box.BoxMergeConfig.MergeEnable = EnableMerge;
    }

    public void ExecuteOnEntity(Entity entity)
    {
        if (!ExertOnTarget) return;
        if (entity is Box box)
        {
            box.BoxMergeConfig.MergeEnable = EnableMerge;
        }
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        BoxSkillAction_ToggleEnableMerge action = ((BoxSkillAction_ToggleEnableMerge) newAction);
        action.EnableMerge = EnableMerge;
        action.ExertOnTarget = ExertOnTarget;
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        BoxSkillAction_ToggleEnableMerge action = ((BoxSkillAction_ToggleEnableMerge) srcData);
        EnableMerge = action.EnableMerge;
        ExertOnTarget = action.ExertOnTarget;
    }
}
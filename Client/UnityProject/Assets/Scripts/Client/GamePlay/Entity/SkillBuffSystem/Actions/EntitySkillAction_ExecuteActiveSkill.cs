using System;
using Sirenix.OdinInspector;

[Serializable]
public class EntitySkillAction_ExecuteActiveSkill : EntitySkillAction, EntitySkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "执行主动技能";

    [LabelText("目标类型")]
    public TargetEntityType TargetEntityType;

    [LabelText("技能")]
    [OnValueChanged("RefreshSkillGUID")]
    public EntitySkillSO EntitySkillSO;

    [ReadOnly]
    [LabelText("技能GUID")]
    public string SkillGUID = "";

    public void RefreshSkillGUID()
    {
        if (EntitySkillSO != null)
        {
            SkillGUID = EntitySkillSO.EntitySkill.SkillGUID;
        }
        else
        {
            SkillGUID = "";
        }
    }

    public void Execute()
    {
        EntityActiveSkill activeSkill = (EntityActiveSkill) ConfigManager.GetEntitySkill(SkillGUID);
        activeSkill.Entity = Entity;
        activeSkill.ParentActiveSkill = null;
        activeSkill.OnInit();
        if (activeSkill.CheckCanTriggerSkill(TargetEntityType, 100))
        {
            activeSkill.TriggerActiveSkill(TargetEntityType);
        }
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_ExecuteActiveSkill action = ((EntitySkillAction_ExecuteActiveSkill) newAction);
        action.TargetEntityType = TargetEntityType;
        action.SkillGUID = SkillGUID;
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_ExecuteActiveSkill action = ((EntitySkillAction_ExecuteActiveSkill) srcData);
        TargetEntityType = action.TargetEntityType;
        SkillGUID = action.SkillGUID;
    }
}
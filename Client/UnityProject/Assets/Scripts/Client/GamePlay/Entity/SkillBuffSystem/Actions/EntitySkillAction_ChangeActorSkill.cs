using System;
using Sirenix.OdinInspector;

[Serializable]
public class EntitySkillAction_ChangeActorSkill : EntitySkillAction, EntitySkillAction.IEntityAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "询问是否学习技能界面";

    [LabelText("True添加False移除")]
    public bool AddOrRemove;

    [LabelText("技能")]
    [OnValueChanged("RefreshSkillGUID")]
    public EntitySkillSO EntitySkillSO;

    [ReadOnly]
    [LabelText("技能GUID")]
    public string SkillGUID = "";

    [ShowIf("AddOrRemove")]
    [LabelText("主动技能按键绑定")]
    public int KeyBind;

    [ShowIf("AddOrRemove")]
    [LabelText("清除技能绑定所有技能")]
    public bool ClearAllExistedSkillInKeyBind;

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

    public void ExecuteOnEntity(Entity entity)
    {
        if (AddOrRemove)
        {
            EntitySkill entitySkill = ConfigManager.GetEntitySkill(SkillGUID);
            if (entitySkill is EntityActiveSkill eas)
            {
                entity.AddNewActiveSkill(eas, KeyBind, ClearAllExistedSkillInKeyBind);
            }
            else if (entitySkill is EntityPassiveSkill eps)
            {
                entity.AddNewPassiveSkill(eps);
            }
        }
        else
        {
            EntitySkill entitySkill = ConfigManager.GetRawEntitySkill(SkillGUID);
            if (entitySkill is EntityActiveSkill eas)
            {
                entity.ForgetActiveSkill(SkillGUID);
            }
            else if (entitySkill is EntityPassiveSkill eps)
            {
                entity.ForgetPassiveSkill(SkillGUID);
            }
        }
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_ChangeActorSkill action = ((EntitySkillAction_ChangeActorSkill)newAction);
        action.AddOrRemove = AddOrRemove;
        action.SkillGUID = SkillGUID;
        action.KeyBind = KeyBind;
        action.ClearAllExistedSkillInKeyBind = ClearAllExistedSkillInKeyBind;
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_ChangeActorSkill action = ((EntitySkillAction_ChangeActorSkill)srcData);
        AddOrRemove = action.AddOrRemove;
        SkillGUID = action.SkillGUID;
        KeyBind = action.KeyBind;
        ClearAllExistedSkillInKeyBind = action.ClearAllExistedSkillInKeyBind;
    }
}
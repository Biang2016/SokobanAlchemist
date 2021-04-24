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
    [ShowIf("IsActiveSkill")]
    [LabelText("主动技能按键绑定")]
    public PlayerControllerHelper.KeyBind KeyBind;

    private bool IsActiveSkill
    {
        get
        {
            if (EntitySkillSO != null && EntitySkillSO.EntitySkill != null)
            {
                return EntitySkillSO.EntitySkill is EntityActiveSkill;
            }

            return false;
        }
    }

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
                entity.AddNewActiveSkill(eas);
                entity.BindActiveSkillToKey(eas, KeyBind, false);
            }
            else if (entitySkill is EntityPassiveSkill eps)
            {
                entity.AddNewPassiveSkill(eps);
            }
        }
        else
        {
            EntitySkill entitySkill = ConfigManager.GetRawEntitySkill(SkillGUID);
            if (entitySkill is EntityActiveSkill)
            {
                entity.ForgetActiveSkill(SkillGUID);
            }
            else if (entitySkill is EntityPassiveSkill)
            {
                entity.ForgetPassiveSkill(SkillGUID);
            }
        }
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_ChangeActorSkill action = ((EntitySkillAction_ChangeActorSkill) newAction);
        action.AddOrRemove = AddOrRemove;
        action.SkillGUID = SkillGUID;
        action.KeyBind = KeyBind;
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_ChangeActorSkill action = ((EntitySkillAction_ChangeActorSkill) srcData);
        AddOrRemove = action.AddOrRemove;
        SkillGUID = action.SkillGUID;
        KeyBind = action.KeyBind;
    }
}
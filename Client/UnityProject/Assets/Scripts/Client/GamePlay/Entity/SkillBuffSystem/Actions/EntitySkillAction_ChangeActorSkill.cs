using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

[Serializable]
public class EntitySkillAction_ChangeActorSkill : EntitySkillAction, EntitySkillAction.IEntityAction, EntitySkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "技能变化";

    [LabelText("忘记所有主动技能")]
    [HideIf("RecoverAllActiveSkill")]
    public bool ForbidAllActiveSkill;

    [LabelText("恢复所有主动技能")]
    [HideIf("ForbidAllActiveSkill")]
    public bool RecoverAllActiveSkill;

    [LabelText("忘记所有掉落技能")]
    public bool ForbidAllDropSkill;

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

    [LabelText("True:对目标Entity生效; False:对本Entity生效")]
    public bool ExertOnTarget;

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
        if (ForbidAllActiveSkill)
        {
            foreach (EntitySkillSO so in target.RawEntityActiveSkillSOs)
            {
                target.Async_ForgetActiveSkill(so.EntitySkill.SkillGUID);
            }
        }

        if (RecoverAllActiveSkill)
        {
            target.InitActiveSkills();
        }

        if (ForbidAllDropSkill)
        {
            List<string> forgetPassiveSkillGUIDs = new List<string>();
            foreach (EntityPassiveSkill eps in target.EntityPassiveSkills)
            {
                if (eps.IsLevelExtraEntitySkill) continue;
                if (eps is EntityPassiveSkill_Conditional eps_conditional)
                {
                    foreach (EntitySkillAction action in eps_conditional.EntitySkillActions)
                    {
                        if (action is EntitySkillAction_DropBox || action is EntitySkillAction_DropSkillScroll)
                        {
                            forgetPassiveSkillGUIDs.Add(eps.SkillGUID);
                            break;
                        }
                    }
                }
            }

            foreach (string guid in forgetPassiveSkillGUIDs)
            {
                target.Async_ForgetPassiveSkill(guid);
            }
        }

        if (AddOrRemove)
        {
            EntitySkill entitySkill = ConfigManager.GetEntitySkill(SkillGUID);
            if (entitySkill is EntityActiveSkill eas)
            {
                if (target.AddNewActiveSkill(eas))
                {
                    target.BindActiveSkillToKey(eas, KeyBind, false);
                }
            }
            else if (entitySkill is EntityPassiveSkill eps)
            {
                target.Async_AddNewPassiveSkill(eps);
            }
        }
        else
        {
            EntitySkill entitySkill = ConfigManager.GetRawEntitySkill(SkillGUID);
            if (entitySkill is EntityActiveSkill)
            {
                target.Async_ForgetActiveSkill(SkillGUID);
            }
            else if (entitySkill is EntityPassiveSkill)
            {
                target.Async_ForgetPassiveSkill(SkillGUID);
            }
        }
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_ChangeActorSkill action = ((EntitySkillAction_ChangeActorSkill) newAction);
        action.ForbidAllActiveSkill = ForbidAllActiveSkill;
        action.RecoverAllActiveSkill = RecoverAllActiveSkill;
        action.ForbidAllDropSkill = ForbidAllDropSkill;
        action.AddOrRemove = AddOrRemove;
        action.SkillGUID = SkillGUID;
        action.KeyBind = KeyBind;
        action.ExertOnTarget = ExertOnTarget;
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_ChangeActorSkill action = ((EntitySkillAction_ChangeActorSkill) srcData);
        ForbidAllActiveSkill = action.ForbidAllActiveSkill;
        RecoverAllActiveSkill = action.RecoverAllActiveSkill;
        ForbidAllDropSkill = action.ForbidAllDropSkill;
        AddOrRemove = action.AddOrRemove;
        SkillGUID = action.SkillGUID;
        KeyBind = action.KeyBind;
        ExertOnTarget = action.ExertOnTarget;
    }
}
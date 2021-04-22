using System;
using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.GamePlay.UI;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntitySkillAction_ShowLearnSkillPanel : EntitySkillAction, EntitySkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "询问是否学习技能界面";

    [LabelText("技能")]
    [OnValueChanged("RefreshSkillGUID")]
    public EntitySkillSO EntitySkillSO;

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

    [ReadOnly]
    [LabelText("技能GUID")]
    public string SkillGUID = "";

    public void Execute()
    {
        LearnSkillPanel learnSkillPanel = UIManager.Instance.ShowUIForms<LearnSkillPanel>();
        learnSkillPanel.Initialize(SkillGUID);
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_ShowLearnSkillPanel action = ((EntitySkillAction_ShowLearnSkillPanel) newAction);
        action.SkillGUID = SkillGUID;
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_ShowLearnSkillPanel action = ((EntitySkillAction_ShowLearnSkillPanel) srcData);
        SkillGUID = action.SkillGUID;
    }
}
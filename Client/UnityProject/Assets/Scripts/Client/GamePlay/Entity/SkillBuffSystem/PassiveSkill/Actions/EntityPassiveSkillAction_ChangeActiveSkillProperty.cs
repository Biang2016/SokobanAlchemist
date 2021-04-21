using System;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntityPassiveSkillAction_ChangeActiveSkillProperty : EntityPassiveSkillAction, EntityPassiveSkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "改变主动技能参数";

    [LabelText("(改变对象)主动技能GUID")]
    public string ActiveSkillGUID;

    [LabelText("属性类型")]
    public EntitySkillPropertyType EntitySkillPropertyType;

    [LabelText("改变值")]
    public EntityProperty EntitySkillProperty = new EntityProperty();

    private EntityProperty originalSkillProperty = new EntityProperty();

    public void Execute()
    {
        if (string.IsNullOrWhiteSpace(ActiveSkillGUID)) return;
        if (Entity.EntityActiveSkillGUIDDict.TryGetValue(ActiveSkillGUID, out EntityActiveSkill eas))
        {
            EntityProperty skillProperty = eas.SkillsPropertyCollection.PropertyDict[EntitySkillPropertyType];
            skillProperty.ApplyDataTo(originalSkillProperty);
            EntitySkillProperty.ApplyDataTo(skillProperty);
        }
    }

    protected override void ChildClone(EntityPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        EntityPassiveSkillAction_ChangeActiveSkillProperty action = ((EntityPassiveSkillAction_ChangeActiveSkillProperty) newAction);
        action.ActiveSkillGUID = ActiveSkillGUID;
    }

    public override void CopyDataFrom(EntityPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntityPassiveSkillAction_ChangeActiveSkillProperty action = ((EntityPassiveSkillAction_ChangeActiveSkillProperty) srcData);
        ActiveSkillGUID = action.ActiveSkillGUID;
    }
}
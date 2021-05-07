using System;
using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntitySkillAction_VerticallyMove : EntitySkillAction, EntitySkillAction.IPureAction, EntitySkillAction.IEntityAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "垂直移动Entity";

    [LabelText("True上False下")]
    public bool UpOrDown;

    public void Execute()
    {
    }

    public void ExecuteOnEntity(Entity entity)
    {
        if (entity is Actor actor)
        {
            if (UpOrDown)
            {
                actor.MoveUp();
            }
            else
            {
                actor.MoveDown();
            }
        }
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_VerticallyMove action = ((EntitySkillAction_VerticallyMove) newAction);
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_VerticallyMove action = ((EntitySkillAction_VerticallyMove) srcData);
    }
}
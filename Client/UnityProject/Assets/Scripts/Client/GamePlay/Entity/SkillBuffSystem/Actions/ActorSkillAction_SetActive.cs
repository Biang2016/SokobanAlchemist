using System;
using Sirenix.OdinInspector;

[Serializable]
public class ActorSkillAction_SetActive : EntitySkillAction, EntitySkillAction.IPureAction, EntitySkillAction.IEntityAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "开关激活角色";

    [LabelText("True激活False休眠")]
    public bool Active;

    [LabelText("True:对目标Entity生效; False:对本Entity生效")]
    public bool ExertOnTarget;

    public void Execute()
    {
        if (ExertOnTarget) return;
        if (Entity is Actor actor)
        {
            actor.ForbidAction = !Active;
            if (Active)
            {
                actor.ActorAIAgent?.Start();
            }
            else
            {
                actor.ActorAIAgent?.Stop();
            }
        }
    }

    public void ExecuteOnEntity(Entity entity)
    {
        if (!ExertOnTarget) return;
        if (entity is Actor actor)
        {
            actor.ForbidAction = Active;
            if (Active)
            {
                actor.ActorAIAgent?.Start();
            }
            else
            {
                actor.ActorAIAgent?.Stop();
            }
        }
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        ActorSkillAction_SetActive action = ((ActorSkillAction_SetActive) newAction);
        action.Active = Active;
        action.ExertOnTarget = ExertOnTarget;
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        ActorSkillAction_SetActive action = ((ActorSkillAction_SetActive) srcData);
        Active = action.Active;
        ExertOnTarget = action.ExertOnTarget;
    }
}
using System;
using Sirenix.OdinInspector;

[Serializable]
public class BoxPassiveSkillAction_PlayerGainHealth : BoxPassiveSkillAction, BoxPassiveSkillAction.IActorOperationAction
{
    protected override string Description => "回复生命";

    [LabelText("回复生命")]
    public int GainHealth;

    public void OnOperation(Actor actor)
    {
        actor.ActorBattleHelper.Heal(actor, GainHealth);
    }

    protected override void ChildClone(BoxPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        BoxPassiveSkillAction_PlayerGainHealth action = ((BoxPassiveSkillAction_PlayerGainHealth) newAction);
        action.GainHealth = GainHealth;
    }

    public override void CopyDataFrom(BoxPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        BoxPassiveSkillAction_PlayerGainHealth action = ((BoxPassiveSkillAction_PlayerGainHealth) srcData);
        GainHealth = action.GainHealth;
    }
}
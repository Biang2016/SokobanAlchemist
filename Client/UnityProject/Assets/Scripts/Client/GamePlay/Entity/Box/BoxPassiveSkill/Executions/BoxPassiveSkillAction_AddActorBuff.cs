using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class BoxPassiveSkillAction_AddActorBuff : BoxPassiveSkillAction, BoxPassiveSkillAction.ICollideAction, BoxPassiveSkillAction.IActorOperationAction
{
    protected override string Description => "给单个Actor施加Buff";

    [LabelText("生效于相对阵营")]
    public RelativeCamp EffectiveOnRelativeCamp;

    [HideLabel]
    [SerializeReference]
    public ActorBuff ActorBuff;

    public void OnCollide(Collision collision)
    {
        Actor actor = collision.gameObject.GetComponentInParent<Actor>();
        CoreAddBuff(actor);
    }

    public void OnOperation(Actor actor)
    {
        CoreAddBuff(actor);
    }

    private void CoreAddBuff(Actor actor)
    {
        if (actor != null && !actor.IsRecycled)
        {
            Actor m_Actor = Box.LastTouchActor;
            if (m_Actor != null)
            {
                if (EffectiveOnRelativeCamp == RelativeCamp.FriendCamp && !actor.IsSameCampOf(m_Actor))
                {
                    return;
                }
                else if (EffectiveOnRelativeCamp == RelativeCamp.OpponentCamp && !actor.IsOpponentCampOf(m_Actor))
                {
                    return;
                }
                else if (EffectiveOnRelativeCamp == RelativeCamp.NeutralCamp && !actor.IsNeutralCampOf(m_Actor))
                {
                    return;
                }
                else if (EffectiveOnRelativeCamp == RelativeCamp.AllCamp)
                {
                }
                else if (EffectiveOnRelativeCamp == RelativeCamp.None)
                {
                    return;
                }
            }

            if (!actor.ActorBuffHelper.AddBuff(ActorBuff.Clone()))
            {
                Debug.Log($"Failed to AddBuff: {ActorBuff.GetType().Name} to {actor.name}");
            }
        }
    }

    protected override void ChildClone(BoxPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        BoxPassiveSkillAction_AddActorBuff action = ((BoxPassiveSkillAction_AddActorBuff) newAction);
        action.ActorBuff = (ActorBuff) ActorBuff.Clone();
        action.EffectiveOnRelativeCamp = EffectiveOnRelativeCamp;
    }

    public override void CopyDataFrom(BoxPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        BoxPassiveSkillAction_AddActorBuff action = ((BoxPassiveSkillAction_AddActorBuff) srcData);
        ActorBuff = (ActorBuff) action.ActorBuff.Clone();
        EffectiveOnRelativeCamp = action.EffectiveOnRelativeCamp;
    }
}
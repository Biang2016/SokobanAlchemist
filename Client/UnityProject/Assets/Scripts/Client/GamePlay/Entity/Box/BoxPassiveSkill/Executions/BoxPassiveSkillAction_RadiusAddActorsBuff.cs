using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class BoxPassiveSkillAction_RadiusAddActorsBuff : BoxPassiveSkillAction, BoxPassiveSkillAction.IPureAction
{
    protected override string Description => "箱子撞击爆炸AOE给Actors施加Buff";

    [LabelText("生效于相对阵营")]
    public RelativeCamp EffectiveOnRelativeCamp;

    [LabelText("判定半径")]
    public int AddBuffRadius = 2;

    [SerializeReference]
    [HideLabel]
    public ActorBuff ActorBuff;

    public void Execute()
    {
        Collider[] colliders = Physics.OverlapSphere(Box.transform.position, AddBuffRadius, LayerManager.Instance.LayerMask_HitBox_Enemy | LayerManager.Instance.LayerMask_HitBox_Player);
        List<Actor> actorList = new List<Actor>();
        foreach (Collider collider in colliders)
        {
            Actor actor = collider.gameObject.GetComponentInParent<Actor>();
            if (actor != null)
            {
                Actor m_Actor = Box.LastTouchActor;
                if (m_Actor != null)
                {
                    if (EffectiveOnRelativeCamp == RelativeCamp.FriendCamp && !actor.IsSameCampOf(m_Actor))
                    {
                        continue;
                    }
                    else if (EffectiveOnRelativeCamp == RelativeCamp.OpponentCamp && !actor.IsOpponentCampOf(m_Actor))
                    {
                        continue;
                    }
                    else if (EffectiveOnRelativeCamp == RelativeCamp.NeutralCamp && !actor.IsNeutralCampOf(m_Actor))
                    {
                        continue;
                    }
                    else if (EffectiveOnRelativeCamp == RelativeCamp.AllCamp)
                    {
                    }
                    else if (EffectiveOnRelativeCamp == RelativeCamp.None)
                    {
                        continue;
                    }
                }

                if (!actorList.Contains(actor))
                {
                    actorList.Add(actor);
                    if (!actor.ActorBuffHelper.AddBuff(ActorBuff.Clone()))
                    {
                        Debug.Log($"Failed to AddBuff: {ActorBuff.GetType().Name} to {actor.name}");
                    }
                }
            }
        }
    }

    protected override void ChildClone(BoxPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        BoxPassiveSkillAction_RadiusAddActorsBuff action = ((BoxPassiveSkillAction_RadiusAddActorsBuff) newAction);
        action.ActorBuff = (ActorBuff) ActorBuff.Clone();
        action.EffectiveOnRelativeCamp = EffectiveOnRelativeCamp;
        action.AddBuffRadius = AddBuffRadius;
    }

    public override void CopyDataFrom(BoxPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        BoxPassiveSkillAction_RadiusAddActorsBuff action = ((BoxPassiveSkillAction_RadiusAddActorsBuff) srcData);
        ActorBuff = (ActorBuff) action.ActorBuff.Clone();
        EffectiveOnRelativeCamp = action.EffectiveOnRelativeCamp;
        AddBuffRadius = action.AddBuffRadius;
    }
}
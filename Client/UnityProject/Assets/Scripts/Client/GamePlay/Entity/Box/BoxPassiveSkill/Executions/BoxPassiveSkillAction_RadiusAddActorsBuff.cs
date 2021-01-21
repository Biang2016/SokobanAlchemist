using System;
using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
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
        HashSet<uint> actorList = new HashSet<uint>();
        foreach (GridPos3D offset in Box.GetBoxOccupationGPs())
        {
            Vector3 boxIndicatorPos = Box.transform.position + offset;
            Collider[] colliders = Physics.OverlapSphere(boxIndicatorPos, AddBuffRadius, LayerManager.Instance.LayerMask_HitBox_Enemy | LayerManager.Instance.LayerMask_HitBox_Player);
            foreach (Collider collider in colliders)
            {
                Actor actor = collider.gameObject.GetComponentInParent<Actor>();
                if (actor != null && !actorList.Contains(actor.GUID))
                {
                    actorList.Add(actor.GUID);
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
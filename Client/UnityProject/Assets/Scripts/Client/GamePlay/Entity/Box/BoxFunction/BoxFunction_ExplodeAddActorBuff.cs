using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class BoxFunction_ExplodeAddActorBuff : BoxFunctionBase
{
    protected override string BoxFunctionDisplayName => "箱子撞击爆炸AOE施加Buff";

    [LabelText("生效于相对阵营")]
    public RelativeCamp EffectiveOnRelativeCamp;

    [LabelText("判定半径")]
    public int AddBuffRadius = 2;

    [BoxGroup("爆炸施加ActorBuff")]
    [HideLabel]
    public ActorBuff ActorBuff;

    public override void OnFlyingCollisionEnter(Collision collision)
    {
        base.OnFlyingCollisionEnter(collision);
        ExplodeAddBuff();
    }

    public override void OnBeingKickedCollisionEnter(Collision collision)
    {
        base.OnBeingKickedCollisionEnter(collision);
        ExplodeAddBuff();
    }

    private void ExplodeAddBuff()
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

    protected override void ChildClone(BoxFunctionBase newBF)
    {
        base.ChildClone(newBF);
        BoxFunction_ExplodeAddActorBuff bf = ((BoxFunction_ExplodeAddActorBuff) newBF);
        bf.ActorBuff = (ActorBuff) ActorBuff.Clone();
        bf.EffectiveOnRelativeCamp = EffectiveOnRelativeCamp;
        bf.AddBuffRadius = AddBuffRadius;
    }

    public override void CopyDataFrom(BoxFunctionBase srcData)
    {
        base.CopyDataFrom(srcData);
        BoxFunction_ExplodeAddActorBuff bf = ((BoxFunction_ExplodeAddActorBuff) srcData);
        ActorBuff = (ActorBuff) bf.ActorBuff.Clone();
        EffectiveOnRelativeCamp = bf.EffectiveOnRelativeCamp;
        AddBuffRadius = bf.AddBuffRadius;
    }
}
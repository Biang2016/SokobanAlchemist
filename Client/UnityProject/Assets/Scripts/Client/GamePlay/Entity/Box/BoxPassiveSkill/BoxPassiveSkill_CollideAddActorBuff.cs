using System;
using Sirenix.OdinInspector;
using UnityEngine;

[assembly: Sirenix.Serialization.BindTypeNameToType("BoxFunction_CollideAddActorBuff", typeof(BoxPassiveSkill_CollideAddActorBuff))]

[Serializable]
public class BoxPassiveSkill_CollideAddActorBuff : BoxPassiveSkill
{
    protected override string BoxPassiveSkillDisplayName => "箱子撞击给Actor施加Buff";

    [LabelText("生效于相对阵营")]
    public RelativeCamp EffectiveOnRelativeCamp;

    [BoxGroup("爆炸施加ActorBuff")]
    [HideLabel]
    public ActorBuff ActorBuff;

    public override void OnFlyingCollisionEnter(Collision collision)
    {
        base.OnFlyingCollisionEnter(collision);
        Collide(collision);
    }

    public override void OnBeingKickedCollisionEnter(Collision collision)
    {
        base.OnBeingKickedCollisionEnter(collision);
        Collide(collision);
    }

    private void Collide(Collision collision)
    {
        Actor actor = collision.gameObject.GetComponentInParent<Actor>();
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

    protected override void ChildClone(BoxPassiveSkill newBF)
    {
        base.ChildClone(newBF);
        BoxPassiveSkill_CollideAddActorBuff bf = ((BoxPassiveSkill_CollideAddActorBuff) newBF);
        bf.ActorBuff = (ActorBuff) ActorBuff.Clone();
        bf.EffectiveOnRelativeCamp = EffectiveOnRelativeCamp;
    }

    public override void CopyDataFrom(BoxPassiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        BoxPassiveSkill_CollideAddActorBuff bf = ((BoxPassiveSkill_CollideAddActorBuff) srcData);
        ActorBuff = (ActorBuff) bf.ActorBuff.Clone();
        EffectiveOnRelativeCamp = bf.EffectiveOnRelativeCamp;
    }
}
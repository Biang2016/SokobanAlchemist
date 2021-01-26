using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[assembly: BindTypeNameToType("BoxPassiveSkillAction_AddActorBuff", typeof(BoxPassiveSkillAction_AddEntityBuff))]
[Serializable]
public class BoxPassiveSkillAction_AddEntityBuff : BoxPassiveSkillAction, BoxPassiveSkillAction.ICollideAction, BoxPassiveSkillAction.IActorOperationAction
{
    protected override string Description => "给单个Actor施加Buff";

    [LabelText("生效于相对阵营(与角色交互时)")]
    public RelativeCamp EffectiveOnRelativeCamp;

    [LabelText("Buff列表")]
    [SerializeReference]
    public List<EntityBuff> EntityBuffs = new List<EntityBuff>();

    public void OnCollide(Collision collision)
    {
        Entity entity = collision.gameObject.GetComponentInParent<Entity>();
        if (entity != null && !entity.IsRecycled) CoreAddBuff(entity);
    }

    public void OnOperation(Actor actor)
    {
        CoreAddBuff(actor);
    }

    private void CoreAddBuff(Entity entity)
    {
        if (entity is Actor actor)
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

          
        }

        foreach (EntityBuff entityBuff in EntityBuffs)
        {
            if (!entity.EntityBuffHelper.AddBuff(entityBuff.Clone()))
            {
                Debug.Log($"Failed to AddBuff: {entityBuff.GetType().Name} to {entity.name}");
            }
        }
    }

    protected override void ChildClone(BoxPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        BoxPassiveSkillAction_AddEntityBuff action = ((BoxPassiveSkillAction_AddEntityBuff) newAction);
        action.EntityBuffs = EntityBuffs.Clone();
        action.EffectiveOnRelativeCamp = EffectiveOnRelativeCamp;
    }

    public override void CopyDataFrom(BoxPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        BoxPassiveSkillAction_AddEntityBuff action = ((BoxPassiveSkillAction_AddEntityBuff) srcData);
        EntityBuffs = action.EntityBuffs.Clone();
        EffectiveOnRelativeCamp = action.EffectiveOnRelativeCamp;
    }
}
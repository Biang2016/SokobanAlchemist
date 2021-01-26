using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntityPassiveSkillAction_AddEntityBuff : EntityPassiveSkillAction, EntityPassiveSkillAction.ICollideAction, EntityPassiveSkillAction.IActorOperationAction
{
    protected override string Description => "给单个Entity施加Buff";

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
        if (Entity is Box box)
        {
            Actor m_Actor = box.LastTouchActor;
            if (m_Actor != null)
            {
                if (EffectiveOnRelativeCamp == RelativeCamp.FriendCamp && !entity.IsSameCampOf(m_Actor))
                {
                    return;
                }
                else if (EffectiveOnRelativeCamp == RelativeCamp.OpponentCamp && !entity.IsOpponentCampOf(m_Actor))
                {
                    return;
                }
                else if (EffectiveOnRelativeCamp == RelativeCamp.NeutralCamp && !entity.IsNeutralCampOf(m_Actor))
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

    protected override void ChildClone(EntityPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        EntityPassiveSkillAction_AddEntityBuff action = ((EntityPassiveSkillAction_AddEntityBuff) newAction);
        action.EntityBuffs = EntityBuffs.Clone();
        action.EffectiveOnRelativeCamp = EffectiveOnRelativeCamp;
    }

    public override void CopyDataFrom(EntityPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntityPassiveSkillAction_AddEntityBuff action = ((EntityPassiveSkillAction_AddEntityBuff) srcData);
        EntityBuffs = action.EntityBuffs.Clone();
        EffectiveOnRelativeCamp = action.EffectiveOnRelativeCamp;
    }
}
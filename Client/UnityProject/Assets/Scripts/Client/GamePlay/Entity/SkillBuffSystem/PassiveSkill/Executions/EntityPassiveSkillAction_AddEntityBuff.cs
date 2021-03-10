using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntityPassiveSkillAction_AddEntityBuff : EntityPassiveSkillAction, EntityPassiveSkillAction.ICollideAction, EntityPassiveSkillAction.IActorOperationAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "碰撞时给撞击者Entity施加Buff，或被角色交互时给该角色Entity施加buff";

    [LabelText("Buff列表")]
    [SerializeReference]
    public List<EntityBuff> RawEntityBuffs = new List<EntityBuff>();

    public void OnCollide(Collision collision)
    {
        Entity entity = collision.gameObject.GetComponentInParent<Entity>();
        if (entity.IsNotNullAndAlive()) CoreAddBuff(entity);
    }

    public void OnOperation(Actor actor)
    {
        CoreAddBuff(actor);
    }

    private void CoreAddBuff(Entity entity)
    {
        foreach (EntityBuff entityBuff in RawEntityBuffs)
        {
            if (entity.IsNotNullAndAlive())
            {
                if (!entity.EntityBuffHelper.AddBuff(entityBuff.Clone()))
                {
                    Debug.Log($"Failed to AddBuff: {entityBuff.GetType().Name} to {entity.name}");
                }
            }
        }
    }

    protected override void ChildClone(EntityPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        EntityPassiveSkillAction_AddEntityBuff action = ((EntityPassiveSkillAction_AddEntityBuff) newAction);
        action.RawEntityBuffs = RawEntityBuffs.Clone();
    }

    public override void CopyDataFrom(EntityPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntityPassiveSkillAction_AddEntityBuff action = ((EntityPassiveSkillAction_AddEntityBuff) srcData);
        if (RawEntityBuffs.Count != action.RawEntityBuffs.Count)
        {
            Debug.LogError("EntityPassiveSkillAction_AddEntityBuff CopyDataFrom() RawEntityBuffs数量不一致");
        }
        else
        {
            for (int i = 0; i < RawEntityBuffs.Count; i++)
            {
                RawEntityBuffs[i].CopyDataFrom(action.RawEntityBuffs[i]);
            }
        }
    }
}
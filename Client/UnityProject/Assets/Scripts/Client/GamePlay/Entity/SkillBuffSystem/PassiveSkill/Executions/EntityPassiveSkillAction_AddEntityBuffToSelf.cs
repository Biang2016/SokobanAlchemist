using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntityPassiveSkillAction_AddEntityBuffToSelf : EntityPassiveSkillAction, EntityPassiveSkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "给自身Entity施加buff";

    [LabelText("Buff列表")]
    [SerializeReference]
    public List<EntityBuff> RawEntityBuffs = new List<EntityBuff>();

    public void OnCollide(Collision collision)
    {
        if (Entity.IsNotNullAndAlive()) CoreAddBuff(Entity);
    }

    public void Execute()
    {
        if (Entity.IsNotNullAndAlive()) CoreAddBuff(Entity);
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
        EntityPassiveSkillAction_AddEntityBuffToSelf action = ((EntityPassiveSkillAction_AddEntityBuffToSelf) newAction);
        action.RawEntityBuffs = RawEntityBuffs.Clone<EntityBuff, EntityBuff>();
    }

    public override void CopyDataFrom(EntityPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntityPassiveSkillAction_AddEntityBuffToSelf action = ((EntityPassiveSkillAction_AddEntityBuffToSelf) srcData);
        if (RawEntityBuffs.Count != action.RawEntityBuffs.Count)
        {
            Debug.LogError("EntityPassiveSkillAction_AddEntityBuffToSelf CopyDataFrom RawEntityBuffs数量不一致");
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
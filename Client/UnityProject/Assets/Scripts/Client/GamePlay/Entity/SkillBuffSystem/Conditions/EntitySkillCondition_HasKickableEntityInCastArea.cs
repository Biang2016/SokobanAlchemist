using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;

[Serializable]
public class EntitySkillCondition_HasInteractableEntityInCastArea : EntitySkillCondition, EntitySkillCondition.IEntityCondition
{
    [LabelText("由玩家交互技能决定")]
    public bool DependOnActorInteractSkill = false;

    [LabelText("交互方式")]
    [ShowIf("DependOnActorInteractSkill")]
    public List<InteractSkillType> InteractSkillTypes = new List<InteractSkillType>();

    public bool OnCheckConditionOnEntity(Entity entity)
    {
        if (entity != null)
        {
            if (Entity is Actor actor && DependOnActorInteractSkill)
            {
                foreach (InteractSkillType ist in InteractSkillTypes)
                {
                    if (actor.ActorBoxInteractHelper.CanInteract(ist, entity.EntityTypeIndex))
                    {
                        return true;
                    }
                }
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    protected override void ChildClone(EntitySkillCondition cloneData)
    {
        EntitySkillCondition_HasInteractableEntityInCastArea newCondition = (EntitySkillCondition_HasInteractableEntityInCastArea) cloneData;
        newCondition.DependOnActorInteractSkill = DependOnActorInteractSkill;
        newCondition.InteractSkillTypes = InteractSkillTypes.Clone<InteractSkillType, InteractSkillType>();
    }

    public override void CopyDataFrom(EntitySkillCondition srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillCondition_HasInteractableEntityInCastArea srcCondition = (EntitySkillCondition_HasInteractableEntityInCastArea) srcData;
        DependOnActorInteractSkill = srcCondition.DependOnActorInteractSkill;
        InteractSkillTypes = srcCondition.InteractSkillTypes.Clone<InteractSkillType, InteractSkillType>();
    }
}
using System;
using Sirenix.OdinInspector;

[Serializable]
public class EntitySkillCondition_HasEntityInCastArea : EntitySkillCondition, EntitySkillCondition.IEntityCondition
{
    [LabelText("Entity种类")]
    public TypeSelectHelper EntityType = new TypeSelectHelper {TypeDefineType = TypeDefineType.Box};

    public bool OnCheckConditionOnEntity(Entity entity)
    {
        if (entity != null && entity.EntityTypeIndex == ConfigManager.GetTypeIndex(EntityType.TypeDefineType, EntityType.TypeName))
        {
            return true;
        }

        return false;
    }

    protected override void ChildClone(EntitySkillCondition cloneData)
    {
        EntitySkillCondition_HasEntityInCastArea newCondition = (EntitySkillCondition_HasEntityInCastArea) cloneData;
    }

    public override void CopyDataFrom(EntitySkillCondition srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillCondition_HasEntityInCastArea srcCondition = (EntitySkillCondition_HasEntityInCastArea) srcData;
    }
}
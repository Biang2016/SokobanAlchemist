using System;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;

[Serializable]
public abstract class EntitySkill : IClone<EntitySkill>
{
    [ReadOnly]
    public string SkillGUID; // e.g: (ade24d16-db0f-40af-8794-1e08e2040df3);

    public EntitySkill Clone()
    {
        Type type = GetType();
        EntitySkill newES = (EntitySkill) Activator.CreateInstance(type);
        ChildClone(newES);
        return newES;
    }

    protected virtual void ChildClone(EntitySkill cloneData)
    {
        cloneData.SkillGUID = SkillGUID;
    }

    public virtual void CopyDataFrom(EntitySkill srcData)
    {
        SkillGUID = srcData.SkillGUID;
    }
}
using System;
using BiangLibrary.CloneVariant;

[Serializable]
public class EntitySkill : IClone<EntitySkill>
{
    public EntitySkill Clone()
    {
        Type type = GetType();
        EntitySkill newES = (EntitySkill) Activator.CreateInstance(type);
        ChildClone(newES);
        return newES;
    }

    protected virtual void ChildClone(EntitySkill cloneData)
    {
    }

    public virtual void CopyDataFrom(EntitySkill srcData)
    {
    }
}
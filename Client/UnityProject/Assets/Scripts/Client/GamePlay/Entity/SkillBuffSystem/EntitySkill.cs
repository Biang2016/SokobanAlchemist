using System;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;

[Serializable]
public abstract class EntitySkill : IClone<EntitySkill>
{
    [ReadOnly]
    public string SkillGUID; // e.g: (ade24d16-db0f-40af-8794-1e08e2040df3);

    [LabelText("技能花名")]
    [Required]
    public string SkillAlias;

    [LabelText("技能描述")]
    [ShowInInspector]
    [PropertyOrder(-1)]
    protected virtual string Description => SkillAlias;

    [LabelText("技能图标")]
    public TypeSelectHelper SkillIcon = new TypeSelectHelper {TypeDefineType = TypeDefineType.EntitySkillIcon};

    [LabelText("技能具体描述")]
    public string SkillDescription = "";

    public EntitySkill Clone()
    {
        Type type = GetType();
        EntitySkill newES = (EntitySkill) Activator.CreateInstance(type);
        newES.SkillGUID = SkillGUID;
        newES.SkillAlias = SkillAlias;
        newES.SkillIcon = SkillIcon?.Clone();
        newES.SkillDescription = SkillDescription;
        ChildClone(newES);
        return newES;
    }

    protected virtual void ChildClone(EntitySkill cloneData)
    {
    }

    public virtual void CopyDataFrom(EntitySkill srcData)
    {
        SkillGUID = srcData.SkillGUID;
        SkillAlias = srcData.SkillAlias;
        SkillIcon?.CopyDataFrom(srcData.SkillIcon);
        SkillDescription = srcData.SkillDescription;
    }
}
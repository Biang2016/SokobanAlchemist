using System;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class EntitySkill : IClone<EntitySkill>
{
    internal Entity Entity;

    [ReadOnly]
    [HideInEditorMode]
    public uint InitWorldModuleGUID; // 创建时所属的世界模组GUID

    [ReadOnly]
    [PropertyOrder(-10)]
    public string SkillGUID; // e.g: (ade24d16-db0f-40af-8794-1e08e2040df3);

    [LabelText("技能花名")]
    [Required]
    [PropertyOrder(-9)]
    public string SkillAlias;

    [LabelText("技能类型描述")]
    [ShowInInspector]
    [PropertyOrder(-10)]
    protected virtual string Description => SkillAlias;

    [LabelText("技能图标")]
    [PropertyOrder(-9)]
    public TypeSelectHelper SkillIcon = new TypeSelectHelper {TypeDefineType = TypeDefineType.EntitySkillIcon};

    [PreviewField]
    [ShowInInspector]
    [PropertyOrder(-9)]
    [HideLabel]
    private Sprite SkillIconPreview => ConfigManager.GetEntitySkillIconByName(SkillIcon.TypeName);

    [LabelText("技能具体描述")]
    public string SkillDescription = "";

    public virtual void OnInit()
    {
        InitWorldModuleGUID = Entity.InitWorldModuleGUID;
    }

    public virtual void OnUnInit()
    {
    }

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
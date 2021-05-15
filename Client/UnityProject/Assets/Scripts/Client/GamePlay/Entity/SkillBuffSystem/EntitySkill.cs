using System;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class EntitySkill : IClone<EntitySkill>
{
    [ShowInInspector]
    [HideInEditorMode]
    internal Entity Entity;

    [ReadOnly]
    [HideInEditorMode]
    public uint InitWorldModuleGUID; // 创建时所属的世界模组GUID

    [ReadOnly]
    [HideInEditorMode]
    public string InitStaticLayoutGUID= ""; // 创建时所属的静态布局GUID

    internal bool IsLevelExtraEntitySkill;
    internal bool MarkAsForget;

    [ReadOnly]
    [PropertyOrder(-10)]
    public string SkillGUID; // e.g: (ade24d16-db0f-40af-8794-1e08e2040df3);

    [LabelText("技能花名")]
    [Required]
    [PropertyOrder(-9)]
    public string SkillAlias;

    [LabelText("技能名EN")]
    [Required]
    [PropertyOrder(-9)]
    public string SkillName_EN;

    [LabelText("技能名ZH")]
    [Required]
    [PropertyOrder(-9)]
    public string SkillName_ZH;

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

    [LabelText("技能具体描述EN")]
    public string SkillDescription_EN = "";

    [LabelText("技能具体描述ZH")]
    public string SkillDescription_ZH = "";

    public virtual string GetSkillDescription_EN => SkillDescription_EN;
    public virtual string GetSkillDescription_ZH => SkillDescription_ZH;

    [LabelText("玩家可学习")]
    [PropertyOrder(-8)]
    public bool PlayerCanLearn;

    [LabelText("购买价格")]
    [PropertyOrder(-8)]
    public int GoldCost;

    [LabelText("占据技能格子")]
    [PropertyOrder(-8)]
    public bool OccupySkillGrid;

    [LabelText("显示到技能清单")]
    [PropertyOrder(-8)]
    public bool ShowInSkillPreviewPanel;

    [LabelText("技能分类")]
    [PropertyOrder(-8)]
    public SkillCategoryType SkillCategoryType;

    [LabelText("技能阶级")]
    [PropertyOrder(-8)]
    public SkillRankType SkillRankType;

    [LabelText("技能卷轴箱子类型")]
    [PropertyOrder(-8)]
    public TypeSelectHelper SkillScrollType = new TypeSelectHelper {TypeDefineType = TypeDefineType.Box};

    public virtual void OnInit()
    {
        InitWorldModuleGUID = Entity.InitWorldModuleGUID;
        InitStaticLayoutGUID = Entity.CurrentEntityData.InitStaticLayoutGUID;
    }

    public virtual void OnUnInit()
    {
        MarkAsForget = false;
    }

    public virtual void OnUpdate(float deltaTime)
    {
    }

    public virtual void OnTick(float tickInterval)
    {
    }

    public EntitySkill Clone()
    {
        Type type = GetType();
        EntitySkill newES = (EntitySkill) Activator.CreateInstance(type);
        newES.InitWorldModuleGUID = InitWorldModuleGUID;
        newES.InitStaticLayoutGUID = InitStaticLayoutGUID;
        newES.SkillGUID = SkillGUID;
        newES.SkillAlias = SkillAlias;
        newES.SkillName_EN = SkillName_EN;
        newES.SkillName_ZH = SkillName_ZH;
        newES.SkillIcon = SkillIcon?.Clone();
        newES.SkillDescription_EN = SkillDescription_EN;
        newES.SkillDescription_ZH = SkillDescription_ZH;
        newES.PlayerCanLearn = PlayerCanLearn;
        newES.GoldCost = GoldCost;
        newES.OccupySkillGrid = OccupySkillGrid;
        newES.ShowInSkillPreviewPanel = ShowInSkillPreviewPanel;
        newES.SkillCategoryType = SkillCategoryType;
        newES.SkillRankType = SkillRankType;
        newES.SkillScrollType = SkillScrollType.Clone();
        ChildClone(newES);
        return newES;
    }

    protected virtual void ChildClone(EntitySkill cloneData)
    {
    }

    public virtual void CopyDataFrom(EntitySkill srcData)
    {
        InitWorldModuleGUID = srcData.InitWorldModuleGUID;
        InitStaticLayoutGUID = srcData.InitStaticLayoutGUID;
        SkillGUID = srcData.SkillGUID;
        SkillAlias = srcData.SkillAlias;
        SkillName_EN = srcData.SkillName_EN;
        SkillName_ZH = srcData.SkillName_ZH;
        SkillIcon?.CopyDataFrom(srcData.SkillIcon);
        SkillDescription_EN = srcData.SkillDescription_EN;
        SkillDescription_ZH = srcData.SkillDescription_ZH;
        PlayerCanLearn = srcData.PlayerCanLearn;
        GoldCost = srcData.GoldCost;
        OccupySkillGrid = srcData.OccupySkillGrid;
        ShowInSkillPreviewPanel = srcData.ShowInSkillPreviewPanel;
        SkillCategoryType = srcData.SkillCategoryType;
        SkillRankType = srcData.SkillRankType;
        SkillScrollType = srcData.SkillScrollType.Clone();
    }

    public override string ToString()
    {
        return SkillAlias + ": " + Description;
    }
}
using System;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntityUpgrade : IClone<EntityUpgrade>
{
    [LabelText("升级图标")]
    [PropertyOrder(-9)]
    public TypeSelectHelper UpgradeIcon = new TypeSelectHelper {TypeDefineType = TypeDefineType.EntitySkillIcon};

    [PreviewField]
    [ShowInInspector]
    [PropertyOrder(-9)]
    [HideLabel]
    private Sprite UpgradeIconPreview => ConfigManager.GetEntitySkillIconByName(UpgradeIcon.TypeName);

    [LabelText("升级名EN")]
    [Required]
    [PropertyOrder(-9)]
    public string UpgradeName_EN;

    [LabelText("升级名ZH")]
    [Required]
    [PropertyOrder(-9)]
    public string UpgradeName_ZH;

    [LabelText("升级具体描述EN")]
    public string UpgradeDescription_EN = "";

    [LabelText("升级具体描述ZH")]
    public string UpgradeDescription_ZH = "";

    public virtual string GetSkillDescription_EN => UpgradeDescription_EN;
    public virtual string GetSkillDescription_ZH => UpgradeDescription_ZH;

    public EntityUpgradeType EntityUpgradeType;

    [ShowIf("EntityUpgradeType", EntityUpgradeType.Property)]
    public EntityPropertyType EntityPropertyType;

    [ShowIf("EntityUpgradeType", EntityUpgradeType.Stat)]
    public EntityStatType EntityStatType;

    [ShowIf("EntityUpgradeType", EntityUpgradeType.Property)]
    public int Delta_BaseValue;

    [ShowIf("EntityUpgradeType", EntityUpgradeType.Property)]
    public int Percent_BaseValue;

    [ShowIf("EntityUpgradeType", EntityUpgradeType.Stat)]
    public int Delta_MinValue;

    [ShowIf("EntityUpgradeType", EntityUpgradeType.Stat)]
    public int Percent_MinValue;

    [ShowIf("EntityUpgradeType", EntityUpgradeType.Stat)]
    public int Delta_MaxValue;

    [ShowIf("EntityUpgradeType", EntityUpgradeType.Stat)]
    public int Percent_MaxValue;

    [ShowIf("EntityUpgradeType", EntityUpgradeType.Stat)]
    public int Delta_AbnormalStatResistance;

    [ShowIf("EntityUpgradeType", EntityUpgradeType.Stat)]
    public int Percent_AbnormalStatResistance;

    [ShowIf("EntityUpgradeType", EntityUpgradeType.Stat)]
    public int Delta_AutoChange;

    [ShowIf("EntityUpgradeType", EntityUpgradeType.Stat)]
    public int Percent_AutoChange;

    [ShowIf("EntityUpgradeType", EntityUpgradeType.Stat)]
    public int Delta_AutoChangePercent;

    [ShowIf("EntityUpgradeType", EntityUpgradeType.Stat)]
    public int Percent_AutoChangePercent;

    public EntityUpgrade Clone()
    {
        EntityUpgrade cloneData = new EntityUpgrade();
        cloneData.UpgradeIcon = UpgradeIcon;
        cloneData.UpgradeName_EN = UpgradeName_EN;
        cloneData.UpgradeName_ZH = UpgradeName_ZH;
        cloneData.UpgradeDescription_EN = UpgradeDescription_EN;
        cloneData.UpgradeDescription_ZH = UpgradeDescription_ZH;

        cloneData.EntityUpgradeType = EntityUpgradeType;
        cloneData.EntityPropertyType = EntityPropertyType;
        cloneData.EntityStatType = EntityStatType;
        cloneData.Delta_BaseValue = Delta_BaseValue;
        cloneData.Percent_BaseValue = Percent_BaseValue;
        cloneData.Delta_MinValue = Delta_MinValue;
        cloneData.Percent_MinValue = Percent_MinValue;
        cloneData.Delta_MaxValue = Delta_MaxValue;
        cloneData.Percent_MaxValue = Percent_MaxValue;
        cloneData.Delta_AbnormalStatResistance = Delta_AbnormalStatResistance;
        cloneData.Percent_AbnormalStatResistance = Percent_AbnormalStatResistance;
        cloneData.Delta_AutoChange = Delta_AutoChange;
        cloneData.Percent_AutoChange = Percent_AutoChange;
        cloneData.Delta_AutoChangePercent = Delta_AutoChangePercent;
        cloneData.Percent_AutoChangePercent = Percent_AutoChangePercent;
        return cloneData;
    }

    public virtual void CopyDataFrom(EntityUpgrade srcData)
    {
        UpgradeIcon = srcData.UpgradeIcon;
        UpgradeName_EN = srcData.UpgradeName_EN;
        UpgradeName_ZH = srcData.UpgradeName_ZH;
        UpgradeDescription_EN = srcData.UpgradeDescription_EN;
        UpgradeDescription_ZH = srcData.UpgradeDescription_ZH;

        EntityUpgradeType = srcData.EntityUpgradeType;
        EntityPropertyType = srcData.EntityPropertyType;
        EntityStatType = srcData.EntityStatType;
        Delta_BaseValue = srcData.Delta_BaseValue;
        Percent_BaseValue = srcData.Percent_BaseValue;
        Delta_MinValue = srcData.Delta_MinValue;
        Percent_MinValue = srcData.Percent_MinValue;
        Delta_MaxValue = srcData.Delta_MaxValue;
        Percent_MaxValue = srcData.Percent_MaxValue;
        Delta_AbnormalStatResistance = srcData.Delta_AbnormalStatResistance;
        Percent_AbnormalStatResistance = srcData.Percent_AbnormalStatResistance;
        Delta_AutoChange = srcData.Delta_AutoChange;
        Percent_AutoChange = srcData.Percent_AutoChange;
        Delta_AutoChangePercent = srcData.Delta_AutoChangePercent;
        Percent_AutoChangePercent = srcData.Percent_AutoChangePercent;
    }

    public override string ToString()
    {
        return UpgradeName_EN + ": " + UpgradeDescription_EN;
    }
}

public enum EntityUpgradeType
{
    Stat = 0,
    Property = 1,
}
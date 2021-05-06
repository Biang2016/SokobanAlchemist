using System;
using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
#endif

[Serializable]
public class EntityData : IClone<EntityData>
{
    public GridPos3D WorldGP;
    public GridPos3D LocalGP;

    [ReadOnly]
    [HideInEditorMode]
    public string InitStaticLayoutGUID = ""; // 创建时所属的静态布局GUID

    public override string ToString()
    {
        return EntityType.ToString();
    }

    [LabelText("实体类型")]
    [HideIf("hideOrientationInEntityLevelEditorInspector")]
    public TypeSelectHelper EntityType = new TypeSelectHelper(); // 在关卡编辑器中不赋值，在Export到Config时数据中自行判断是哪种Entity并赋值

    internal ushort EntityTypeIndex
    {
        get
        {
            switch (EntityType.TypeDefineType)
            {
                case TypeDefineType.Box:
                {
                    return ConfigManager.GetTypeIndex(TypeDefineType.Box, EntityType.TypeName);
                }
                case TypeDefineType.Actor:
                {
                    return ConfigManager.GetTypeIndex(TypeDefineType.Actor, EntityType.TypeName);
                }
            }

            return 0;
        }
        set
        {
            ConfigManager.TypeStartIndex tsi = value.ConvertToTypeStartIndex();
            switch (tsi)
            {
                case ConfigManager.TypeStartIndex.Box:
                {
                    EntityType.TypeDefineType = TypeDefineType.Box;
                    EntityType.TypeSelection = ConfigManager.GetTypeName(TypeDefineType.Box, value);
                    EntityType.RefreshGUID();
                    break;
                }
                case ConfigManager.TypeStartIndex.Actor:
                {
                    EntityType.TypeDefineType = TypeDefineType.Actor;
                    EntityType.TypeSelection = ConfigManager.GetTypeName(TypeDefineType.Actor, value);
                    EntityType.RefreshGUID();
                    break;
                }
            }
        }
    }

    [LabelText("朝向")]
    [EnumToggleButtons]
    [HideIf("hideOrientationInEntityLevelEditorInspector")]
    public GridPosR.Orientation EntityOrientation = GridPosR.Orientation.Up;

    internal bool hideOrientationInEntityLevelEditorInspector = false;

    [HideLabel]
    public EntityExtraSerializeData RawEntityExtraSerializeData = new EntityExtraSerializeData(); // 干数据，禁修改

    public EntityData()
    {
    }

    public EntityData(ushort entityTypeIndex, GridPosR.Orientation entityOrientation, EntityExtraSerializeData rawEntityExtraSerializeData = null)
    {
        EntityTypeIndex = entityTypeIndex;
        EntityOrientation = entityOrientation;
        RawEntityExtraSerializeData = rawEntityExtraSerializeData ?? RawEntityExtraSerializeData;
        EntityType.RefreshGUID();
    }

    public void RemoveAllLevelEventTriggerAppearPassiveSkill()
    {
        List<EntityPassiveSkill> removeEPSList = new List<EntityPassiveSkill>();
        foreach (EntityPassiveSkill eps in RawEntityExtraSerializeData.EntityPassiveSkills)
        {
            if (eps is EntityPassiveSkill_LevelEventTriggerAppear)
            {
                removeEPSList.Add(eps);
            }
        }

        foreach (EntityPassiveSkill eps in removeEPSList)
        {
            RawEntityExtraSerializeData.EntityPassiveSkills.Remove(eps);
        }
    }

    public bool ProbablyShow()
    {
        foreach (EntityPassiveSkill eps in RawEntityExtraSerializeData.EntityPassiveSkills)
        {
            if (eps is EntityPassiveSkill_ProbablyShow probablyShow)
            {
                if (probablyShow.ShowProbabilityPercent.ProbabilityBool())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void RemoveAllProbablyShowPassiveSkill()
    {
        List<EntityPassiveSkill> removeEPSList = new List<EntityPassiveSkill>();
        foreach (EntityPassiveSkill eps in RawEntityExtraSerializeData.EntityPassiveSkills)
        {
            if (eps is EntityPassiveSkill_ProbablyShow)
            {
                removeEPSList.Add(eps);
            }
        }

        foreach (EntityPassiveSkill eps in removeEPSList)
        {
            RawEntityExtraSerializeData.EntityPassiveSkills.Remove(eps);
        }
    }

    public EntityData Clone()
    {
        EntityData newEntityData = new EntityData(EntityTypeIndex, EntityOrientation, RawEntityExtraSerializeData?.Clone());
        newEntityData.WorldGP = WorldGP;
        newEntityData.LocalGP = LocalGP;
        newEntityData.InitStaticLayoutGUID = InitStaticLayoutGUID;
        return newEntityData;
    }
}
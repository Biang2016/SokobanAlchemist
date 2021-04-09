using System;
using System.Collections;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[Serializable]
public class EntityData : IClone<EntityData>
{
    public override string ToString()
    {
        return EntityType.ToString();
    }

    [LabelText("实体类型")]
    [HideIf("hideOrientationInEntityLevelEditorInspector")]
    public TypeSelectHelper EntityType = new TypeSelectHelper(); // 在关卡编辑器中不赋值，在Export到Config时数据中自行判断是那种Entity并赋值

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

    public EntityData Clone()
    {
        EntityData newEntityData = new EntityData(EntityTypeIndex, EntityOrientation, RawEntityExtraSerializeData?.Clone());
        return newEntityData;
    }
}
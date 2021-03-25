using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[CreateAssetMenu(menuName = "类型映射表")]
public class TypeGUIDMappingAsset : SerializedScriptableObject
{
    public class Mapping
    {
        [OdinSerialize]
        public Dictionary<string, string> Type_GUIDDict = new Dictionary<string, string>(); // Key: TypeName Value: GUID

        [OdinSerialize]
        [HideInInspector]
        public Dictionary<string, string> GUID_TypeDict = new Dictionary<string, string>(); // Key: GUID Value: TypeName

        /// <summary>
        /// Call by ConfigManager when serialize config
        /// </summary>
        /// <param name="typeName"></param>
        public bool TryAddNewType(string typeName)
        {
            if (!Type_GUIDDict.ContainsKey(typeName))
            {
                string guid = Guid.NewGuid().ToString("P");
                Type_GUIDDict.Add(typeName, guid);
                GUID_TypeDict.Add(guid, typeName);
                return true;
            }

            return false;
        }

        public void LoadData(string typeName, string guid)
        {
            Type_GUIDDict.Add(typeName, guid);
            GUID_TypeDict.Add(guid, typeName);
        }

        public void RefreshGUID_TypeDict()
        {
            GUID_TypeDict.Clear();
            foreach (KeyValuePair<string, string> kv in Type_GUIDDict)
            {
                GUID_TypeDict.Add(kv.Value, kv.Key);
            }
        }
    }

    [OdinSerialize]
    public Dictionary<TypeDefineType, Mapping> TypeGUIDMappings = new Dictionary<TypeDefineType, Mapping>
    {
        {TypeDefineType.Box, new Mapping()},
        {TypeDefineType.BoxIcon, new Mapping()},
        {TypeDefineType.Enemy, new Mapping()},
        {TypeDefineType.LevelTrigger, new Mapping()},
        {TypeDefineType.WorldModule, new Mapping()},
        {TypeDefineType.StaticLayout, new Mapping()},
        {TypeDefineType.World, new Mapping()},
        {TypeDefineType.FX, new Mapping()},
        {TypeDefineType.BattleIndicator, new Mapping()},
    };

    public int version = 0;
}

[Serializable]
public class TypeSelectHelper : IClone<TypeSelectHelper>
{
    [ShowInInspector]
    [LabelText("类型名称")]
    public string TypeName => ToString();

    public TypeSelectHelper Clone()
    {
        TypeSelectHelper newHelper = new TypeSelectHelper();
        newHelper.TypeDefineType = TypeDefineType;
        newHelper.TypeGUID = TypeGUID;
        newHelper.TypeSelection = TypeSelection;
        return newHelper;
    }

    public void CopyDataFrom(TypeSelectHelper srcData)
    {
        TypeDefineType = srcData.TypeDefineType;
        TypeGUID = srcData.TypeGUID;
        TypeSelection = srcData.TypeSelection;
    }

    public override string ToString()
    {
        if (!string.IsNullOrWhiteSpace(TypeGUID))
        {
            ConfigManager.LoadAllConfigs();
            if (ConfigManager.TypeGUIDMappings[TypeDefineType].GUID_TypeDict.TryGetValue(TypeGUID, out string typeName))
            {
                TypeSelection = typeName;
                return typeName;
            }
            else
            {
                return TypeSelection;
            }
        }
        else
        {
            return TypeSelection;
        }
    }

    [LabelText("枚举类型")]
    public TypeDefineType TypeDefineType;

    [LabelText("类型选择")]
    [ValueDropdown("GetAllTypeNames")]
    [OnValueChanged("RefreshGUID")]
    [NonSerialized]
    [ShowInInspector]
    internal string TypeSelection = "None";

    [SerializeField]
    [ReadOnly]
    [LabelText("类型GUID")]
    //[HideInInspector]
    private string TypeGUID;

    private IEnumerable<string> GetAllTypeNames => ConfigManager.GetAllTypeNames(TypeDefineType);

    public void RefreshGUID()
    {
        if (ConfigManager.TypeGUIDMappings.TryGetValue(TypeDefineType, out TypeGUIDMappingAsset.Mapping mapping))
        {
            if (mapping.Type_GUIDDict.TryGetValue(TypeSelection, out string guid))
            {
                TypeGUID = guid;
            }
            else
            {
                TypeGUID = "";
            }
        }
    }
}

public enum TypeDefineType
{
    Box,
    BoxIcon,
    Enemy,
    LevelTrigger,
    WorldModule,
    StaticLayout,
    World,
    FX,
    BattleIndicator,
}
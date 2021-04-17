using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BiangLibrary;
using BiangLibrary.Singleton;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;

#endif

// 笔记：重命名类名且不丢失旧的序列化
// [assembly: BindTypeNameToType("ConfigManager_RenameClassSample", typeof(ConfigManager))]

public class ConfigManager : TSingletonBaseManager<ConfigManager>
{
    public enum GUID_Separator
    {
        WorldModule = 500,
        Entity = 1000,
        Buff = 10000,
        BornPointData = 90000,
        PropertyModifier = 100000,
    }

    public static bool ShowEnemyPathFinding = false;

    public static float BoxThrowDragFactor_Cheat = 10f;
    public static float BoxKickDragFactor_Cheat = 1f;
    public static float BoxWeightFactor_Cheat = 1f;

    [ReadOnly]
    [ShowInInspector]
    [BoxGroup("角色Buff相克表")]
    public static EntityBuffAttributeRelationship[,] EntityBuffAttributeMatrix; // [老Buff, 新Buff], Value: Buff克制关系

    public static Dictionary<TypeDefineType, TypeGUIDMappingAsset.Mapping> TypeGUIDMappings = new Dictionary<TypeDefineType, TypeGUIDMappingAsset.Mapping>();

    public const int TERRAIN_TYPE_COUNT = 12;

    public enum TypeStartIndex
    {
        None = 0,
        Box = 1,
        BoxIcon = 10000,
        Actor = 11000,
        CollectableItem = 12000,
        LevelTrigger = 13000,
        WorldModule = 14000,
        StaticLayout = 20000,
        World = 30000,
        FX = 31000,
        SkyBox = 31900,
        PostProcessingProfile = 31950,
        BattleIndicator = 32000,
    }

    public abstract class TypeDefineConfig
    {
        public Dictionary<string, ushort> TypeIndexDict = new Dictionary<string, ushort>();
        public SortedDictionary<ushort, string> TypeNameDict = new SortedDictionary<ushort, string>();

        public Dictionary<string, string> TypeAssetDataBasePathDict = new Dictionary<string, string>();

        public abstract string AssetDataBaseFolderPath { get; }

        public abstract string GetTypeAssetDataBasePath(string assetName);
#if UNITY_EDITOR

        public abstract void ExportTypeNames();

#endif

        public abstract void LoadTypeNames();
        public abstract void Clear();
    }

    public class TypeDefineConfig<T> : TypeDefineConfig where T : Object
    {
        public TypeDefineConfig(string typeNamePrefix, string prefabFolder, bool includeSubFolder, TypeStartIndex typeStartIndex)
        {
            TypeNamePrefix = typeNamePrefix;
            PrefabFolder_Relative = prefabFolder;
            IncludeSubFolder = includeSubFolder;
            m_TypeStartIndex = typeStartIndex;
        }

        private string TypeNamePrefix;
        private string TypeNamesConfig_File => $"{TypeNamesConfigFolder_Build}/{TypeNamePrefix}Names.config";
        private string PrefabFolder_Relative;
        private bool IncludeSubFolder;
        private TypeStartIndex m_TypeStartIndex;

        public override string AssetDataBaseFolderPath => "Assets" + PrefabFolder_Relative;

        public override string GetTypeAssetDataBasePath(string assetName)
        {
            if (TypeAssetDataBasePathDict.TryGetValue(assetName, out string assetDataBasePath))
            {
                return assetDataBasePath;
            }

            {
                return null;
            }
        }

#if UNITY_EDITOR
        public override void ExportTypeNames()
        {
            Clear();
            TypeAssetDataBasePathDict.Clear();
            string folder = TypeNamesConfigFolder_Build;
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            ushort index = (ushort) m_TypeStartIndex;
            DirectoryInfo di = new DirectoryInfo(Application.dataPath + PrefabFolder_Relative);

            if (CommonUtils.IsBaseType(typeof(T), typeof(MonoBehaviour)))
            {
                foreach (FileInfo fi in di.GetFiles("*.prefab", IncludeSubFolder ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    if (index == ushort.MaxValue)
                    {
                        Debug.LogError($"{typeof(T).Name}类型数量超过{ushort.MaxValue}");
                        break;
                    }

                    string relativePath = CommonUtils.ConvertAbsolutePathToProjectPath(fi.FullName);
                    GameObject obj = (GameObject) AssetDatabase.LoadAssetAtPath<Object>(relativePath);
                    T t = obj.GetComponent<T>();
                    if (t != null)
                    {
                        TypeNameDict.Add(index, t.name);
                        TypeIndexDict.Add(t.name, index);
                        TypeAssetDataBasePathDict.Add(t.name, relativePath);
                        index++;
                    }
                    else
                    {
                        Debug.LogError($"Prefab {fi.Name} 不含{typeof(T).Name}脚本，已跳过");
                    }
                }
            }
            else
            {
                foreach (FileInfo fi in di.GetFiles("*.*", IncludeSubFolder ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    if (fi.Name.EndsWith(".meta")) continue;
                    if (index == ushort.MaxValue)
                    {
                        Debug.LogError($"{typeof(T).Name}类型数量超过{ushort.MaxValue}");
                        break;
                    }

                    string relativePath = CommonUtils.ConvertAbsolutePathToProjectPath(fi.FullName);
                    Object obj = AssetDatabase.LoadAssetAtPath<Object>(relativePath);
                    T t = (T) obj;
                    if (t != null)
                    {
                        TypeNameDict.Add(index, t.name);
                        TypeIndexDict.Add(t.name, index);
                        TypeAssetDataBasePathDict.Add(t.name, relativePath);
                        index++;
                    }
                    else
                    {
                        Debug.LogError($"文件 {fi.Name} 不是{typeof(T).Name}类型，已跳过");
                    }
                }
            }

            string json = JsonConvert.SerializeObject(TypeIndexDict, Formatting.Indented);
            StreamWriter sw = new StreamWriter(TypeNamesConfig_File);
            sw.Write(json);
            sw.Close();
        }
#endif

        public override void LoadTypeNames()
        {
            FileInfo fi = new FileInfo(TypeNamesConfig_File);
            if (fi.Exists)
            {
                StreamReader sr = new StreamReader(TypeNamesConfig_File);
                string content = sr.ReadToEnd();
                sr.Close();
                TypeIndexDict.Clear();
                TypeNameDict.Clear();
                TypeIndexDict = JsonConvert.DeserializeObject<Dictionary<string, ushort>>(content);
                foreach (KeyValuePair<string, ushort> kv in TypeIndexDict)
                {
                    TypeNameDict.Add(kv.Value, kv.Key);
                }
            }
        }

        public override void Clear()
        {
            TypeIndexDict.Clear();
            TypeNameDict.Clear();
        }
    }

    public static Dictionary<TypeDefineType, TypeDefineConfig> TypeDefineConfigs = new Dictionary<TypeDefineType, TypeDefineConfig>
    {
        {TypeDefineType.Box, new TypeDefineConfig<Box>("Box", "/Resources/Prefabs/Designs/Box", true, TypeStartIndex.Box)},
        {TypeDefineType.BoxIcon, new TypeDefineConfig<Texture2D>("BoxIcon", "/Resources/BoxIcons", true, TypeStartIndex.BoxIcon)},
        {TypeDefineType.Actor, new TypeDefineConfig<Actor>("Actor", "/Resources/Prefabs/Designs/Actor", true, TypeStartIndex.Actor)},
        {TypeDefineType.CollectableItem, new TypeDefineConfig<CollectableItem>("CollectableItem", "/Resources/Prefabs/Designs/CollectableItem", true, TypeStartIndex.CollectableItem)},
        {TypeDefineType.LevelTrigger, new TypeDefineConfig<LevelTriggerBase>("LevelTrigger", "/Resources/Prefabs/Designs/LevelTrigger", true, TypeStartIndex.LevelTrigger)},
        {TypeDefineType.WorldModule, new TypeDefineConfig<WorldModuleDesignHelper>("WorldModule", "/Designs/WorldModule", true, TypeStartIndex.WorldModule)},
        {TypeDefineType.StaticLayout, new TypeDefineConfig<WorldModuleDesignHelper>("StaticLayout", "/Designs/StaticLayout", true, TypeStartIndex.StaticLayout)},
        {TypeDefineType.World, new TypeDefineConfig<WorldDesignHelper>("World", "/Designs/Worlds", true, TypeStartIndex.World)},
        {TypeDefineType.FX, new TypeDefineConfig<FX>("FX", "/Resources/Prefabs/FX", true, TypeStartIndex.FX)},
        {TypeDefineType.BattleIndicator, new TypeDefineConfig<BattleIndicator>("BattleIndicator", "/Resources/Prefabs/BattleIndicator", true, TypeStartIndex.BattleIndicator)},
        {TypeDefineType.SkyBox, new TypeDefineConfig<Material>("SkyBox", "/Resources/SkyBox", true, TypeStartIndex.SkyBox)},
        {TypeDefineType.PostProcessingProfile, new TypeDefineConfig<PostProcessProfile>("PostProcessingProfile", "/Resources/PostProcessingProfile", true, TypeStartIndex.PostProcessingProfile)},
    };

    [ShowInInspector]
    [LabelText("Entity占位配置表")]
    public static readonly Dictionary<ushort, EntityOccupationData> EntityOccupationConfigDict = new Dictionary<ushort, EntityOccupationData>();

    [ShowInInspector]
    [LabelText("世界模组配置表")]
    public static readonly SortedDictionary<ushort, WorldModuleData> WorldModuleDataConfigDict = new SortedDictionary<ushort, WorldModuleData>();

    [ShowInInspector]
    [LabelText("静态布局配置表")]
    public static readonly SortedDictionary<ushort, WorldModuleData> StaticLayoutDataConfigDict = new SortedDictionary<ushort, WorldModuleData>();

    [ShowInInspector]
    [LabelText("世界配置表")]
    public static readonly Dictionary<ushort, WorldData> WorldDataConfigDict = new Dictionary<ushort, WorldData>();

    [ShowInInspector]
    [LabelText("技能库")]
    public static readonly Dictionary<string, EntitySkill> EntitySkillLibrary = new Dictionary<string, EntitySkill>();

    public static string DesignRoot = "/Designs/";
    public static string ResourcesPrefabDesignRoot = "/Resources/Prefabs/Designs/";

    public static string EntityBuffAttributeMatrixAssetPath_Relative = "Buff/EntityBuffAttributeMatrixAsset.asset";
    public static string EntityBuffAttributeMatrixConfigFolder_Relative = "EntityBuffAttributeMatrix";
    public static string TypeGUIDMappingAssetPath_Relative = "TypeGUIDMapping/TypeGUIDMappingAsset.asset";
    public static string TypeGUIDMappingConfigFolder_Relative = "TypeGUIDMapping";
    public static string EntityOccupationConfigDictFolder_Relative = "Entity";
    public static string WorldModuleDataConfigFolder_Relative = "WorldModule";
    public static string StaticLayoutDataConfigFolder_Relative = "StaticLayout";
    public static string WorldDataConfigFolder_Relative = "Worlds";
    public static string EntitySkillLibraryFolder_Relative = "EntitySkill";

    public static string ConfigFolder_Build = Application.streamingAssetsPath + "/Configs/";
    public static string TypeNamesConfigFolder_Build = ConfigFolder_Build + "/TypeNames";
    public static string EntityBuffAttributeMatrixConfigFolder_Build = ConfigFolder_Build + EntityBuffAttributeMatrixConfigFolder_Relative + "/";
    public static string TypeGUIDMappingConfigFolder_Build = ConfigFolder_Build + TypeGUIDMappingConfigFolder_Relative + "/";
    public static string EntityOccupationConfigDictFolder_Build = ConfigFolder_Build + EntityOccupationConfigDictFolder_Relative + "/";
    public static string WorldModuleDataConfigFolder_Build = ConfigFolder_Build + WorldModuleDataConfigFolder_Relative + "s/";
    public static string StaticLayoutDataConfigFolder_Build = ConfigFolder_Build + StaticLayoutDataConfigFolder_Relative + "s/";
    public static string WorldDataConfigFolder_Build = ConfigFolder_Build + WorldDataConfigFolder_Relative + "/";
    public static string EntitySkillLibraryFolder_Build = ConfigFolder_Build + EntitySkillLibraryFolder_Relative + "/";

    public override void Awake()
    {
        LoadAllConfigs();
    }

    private static void Clear()
    {
        foreach (KeyValuePair<TypeDefineType, TypeDefineConfig> kv in TypeDefineConfigs)
        {
            kv.Value.Clear();
        }

        EntityOccupationConfigDict.Clear();
        WorldModuleDataConfigDict.Clear();
        StaticLayoutDataConfigDict.Clear();
        WorldDataConfigDict.Clear();
    }

    #region Export

#if UNITY_EDITOR
    [MenuItem("开发工具/配置/序列化配置")]
    public static void ExportConfigs()
    {
        ExportConfigs(true);
    }

    public static bool IsExporting = false;

    public static void ExportConfigs(bool dialogShow = true)
    {
        IsExporting = true;
        // http://www.sirenix.net/odininspector/faq?Search=&t-11=on#faq
        DataFormat dataFormat = DataFormat.Binary;

        if (Directory.Exists(ConfigFolder_Build)) Directory.Delete(ConfigFolder_Build, true);
        Directory.CreateDirectory(ConfigFolder_Build);

        // 时序，先导出类型表
        ExportTypeGUIDMapping();
        ExportPassiveSkillLibrary(dataFormat);

        SortAllWorldModules();
        SortAllStaticLayouts();
        SortAllWorlds();
        ExportEntityBuffAttributeMatrix(dataFormat);
        ExportAllEntityOccupationDataConfigs(dataFormat);
        ExportAllWorldModuleDataConfigs(dataFormat);
        ExportAllStaticLayoutDataConfigs(dataFormat);
        ExportAllWorldDataConfigs(dataFormat);

        AssetDatabase.Refresh();
        IsLoaded = false;
        IsExporting = false;
        LoadAllConfigs();
        if (dialogShow) EditorUtility.DisplayDialog("Notice", "Serialize Config Success", "Confirm");
    }

    [MenuItem("开发工具/配置/快速序列化类型")]
    public static void QuickExportConfigs_TypeDefines()
    {
        if (Directory.Exists(TypeNamesConfigFolder_Build)) Directory.Delete(TypeNamesConfigFolder_Build, true);
        Directory.CreateDirectory(TypeNamesConfigFolder_Build);

        // 时序，先导出类型表
        ExportTypeGUIDMapping();
        ExportPassiveSkillLibrary(DataFormat.Binary);
        ExportAllEntityOccupationDataConfigs(DataFormat.Binary);

        AssetDatabase.Refresh();
        IsLoaded = false;
        LoadAllConfigs();
        EditorUtility.DisplayDialog("Notice", "Quick Serialize Types Success", "Confirm");
    }

    [MenuItem("Assets/SerializeConfigForThisModuleOrWorld", priority = -50)]
    public static void ExportConfigsForSelectedAssets()
    {
        LoadAllConfigs();
        DataFormat dataFormat = DataFormat.Binary;
        ExportTypeGUIDMapping();
        GameObject[] selectedGOs = Selection.gameObjects;
        foreach (GameObject go in selectedGOs)
        {
            if (go.GetComponent<WorldModuleDesignHelper>())
            {
                if (SortWorldModule(go.name))
                {
                    SortWorldModule(go.name);
                    ExportWorldModuleDataConfig(dataFormat, go.name);
                }
                else
                {
                    if (SortStaticLayout(go.name))
                    {
                        ExportStaticLayoutDataConfig(dataFormat, go.name);
                    }
                }
            }
            else if (go.GetComponent<WorldDesignHelper>())
            {
                SortWorld(go.name);
                ExportWorldDataConfig(dataFormat, go.name);
            }
        }

        EditorUtility.DisplayDialog("Notice", "SerializeConfigForThisModuleOrWorld Success", "Confirm");
    }

    private static void ExportTypeGUIDMapping()
    {
        TypeGUIDMappingAsset typeMapping = GetTypeGUIDMappingAsset();
        bool dirty = false;
        foreach (KeyValuePair<TypeDefineType, TypeDefineConfig> kv in TypeDefineConfigs)
        {
            kv.Value.ExportTypeNames();
            foreach (KeyValuePair<string, ushort> _kv in kv.Value.TypeIndexDict)
            {
                dirty |= typeMapping.TypeGUIDMappings[kv.Key].TryAddNewType(_kv.Key);
            }

            List<string> removeList = new List<string>();
            foreach (KeyValuePair<string, string> _kv in typeMapping.TypeGUIDMappings[kv.Key].Type_GUIDDict)
            {
                if (!kv.Value.TypeIndexDict.ContainsKey(_kv.Key))
                {
                    removeList.Add(_kv.Key);
                }
            }

            foreach (string key in removeList)
            {
                dirty = true;
                typeMapping.TypeGUIDMappings[kv.Key].Type_GUIDDict.Remove(key);
            }
        }

        if (dirty)
        {
            typeMapping.version += 1;
            EditorUtility.SetDirty(typeMapping);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        ExportTypeGUIDMappingAsset(DataFormat.Binary);
        LoadTypeGUIDMappingFromConfig(DataFormat.Binary);
    }

    public static TypeGUIDMappingAsset GetTypeGUIDMappingAsset()
    {
        FileInfo assetFile = new FileInfo(Application.dataPath + DesignRoot + TypeGUIDMappingAssetPath_Relative);
        string relativePath = CommonUtils.ConvertAbsolutePathToProjectPath(assetFile.FullName);
        Object configObj = AssetDatabase.LoadAssetAtPath<Object>(relativePath);
        TypeGUIDMappingAsset configSSO = (TypeGUIDMappingAsset) configObj;
        return configSSO;
    }

    public static void ExportTypeGUIDMappingAsset(DataFormat dataFormat)
    {
        string folder = TypeGUIDMappingConfigFolder_Build;
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        string file = $"{folder}/TypeGUIDMapping.config";
        if (File.Exists(file)) File.Delete(file);

        TypeGUIDMappingAsset configSSO = GetTypeGUIDMappingAsset();

        Dictionary<string, Dictionary<string, string>> exportDict = new Dictionary<string, Dictionary<string, string>>();
        foreach (KeyValuePair<TypeDefineType, TypeGUIDMappingAsset.Mapping> kv in configSSO.TypeGUIDMappings)
        {
            kv.Value.RefreshGUID_TypeDict();
            exportDict.Add(kv.Key.ToString(), new Dictionary<string, string>());
            foreach (KeyValuePair<string, string> _kv in kv.Value.Type_GUIDDict)
            {
                exportDict[kv.Key.ToString()].Add(_kv.Key, _kv.Value);
            }
        }

        byte[] bytes = SerializationUtility.SerializeValue(exportDict, DataFormat.JSON);
        File.WriteAllBytes(file, bytes);
        AssetDatabase.Refresh();
    }

    private static void ExportPassiveSkillLibrary(DataFormat dataFormat)
    {
        string folder = EntitySkillLibraryFolder_Build;
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        Dictionary<string, EntitySkill> entitySkillDict = new Dictionary<string, EntitySkill>();

        DirectoryInfo di = new DirectoryInfo(Application.dataPath + DesignRoot + EntitySkillLibraryFolder_Relative);
        foreach (FileInfo fi in di.GetFiles("*.asset", SearchOption.AllDirectories))
        {
            string relativePath = CommonUtils.ConvertAbsolutePathToProjectPath(fi.FullName);
            EntitySkillSSO obj = AssetDatabase.LoadAssetAtPath<EntitySkillSSO>(relativePath);
            entitySkillDict.Add(obj.EntitySkill.SkillGUID, obj.EntitySkill);
        }

        string path = folder + "EntitySkillLibrary.config";
        byte[] bytes = SerializationUtility.SerializeValue(entitySkillDict, dataFormat);
        File.WriteAllBytes(path, bytes);
    }

    private static void SortAllWorldModules()
    {
        List<string> worldModuleNames = TypeDefineConfigs[TypeDefineType.WorldModule].TypeIndexDict.Keys.ToList();
        foreach (string worldModuleName in worldModuleNames)
        {
            SortWorldModule(worldModuleName);
        }
    }

    private static bool SortWorldModule(string worldModuleName)
    {
        string prefabPath = TypeDefineConfigs[TypeDefineType.WorldModule].GetTypeAssetDataBasePath(worldModuleName);
        if (string.IsNullOrEmpty(prefabPath)) return false;
        GameObject worldModulePrefab = PrefabUtility.LoadPrefabContents(prefabPath);
        if (worldModulePrefab)
        {
            WorldModuleDesignHelper module = worldModulePrefab.GetComponent<WorldModuleDesignHelper>();
            if (module)
            {
                bool isDirty = module.SortModule();
                if (isDirty) PrefabUtility.SaveAsPrefabAsset(worldModulePrefab, prefabPath);
            }
        }

        PrefabUtility.UnloadPrefabContents(worldModulePrefab);
        return true;
    }

    private static void SortAllStaticLayouts()
    {
        List<string> staticLayoutNames = TypeDefineConfigs[TypeDefineType.StaticLayout].TypeIndexDict.Keys.ToList();
        foreach (string staticLayoutName in staticLayoutNames)
        {
            SortStaticLayout(staticLayoutName);
        }
    }

    private static bool SortStaticLayout(string staticLayoutName)
    {
        string prefabPath = TypeDefineConfigs[TypeDefineType.StaticLayout].GetTypeAssetDataBasePath(staticLayoutName);
        if (string.IsNullOrEmpty(prefabPath)) return false;
        GameObject staticLayoutPrefab = PrefabUtility.LoadPrefabContents(prefabPath);
        if (staticLayoutPrefab)
        {
            WorldModuleDesignHelper module = staticLayoutPrefab.GetComponent<WorldModuleDesignHelper>();
            if (module)
            {
                bool isDirty = module.SortModule();
                if (isDirty) PrefabUtility.SaveAsPrefabAsset(staticLayoutPrefab, prefabPath);
            }
        }

        PrefabUtility.UnloadPrefabContents(staticLayoutPrefab);
        return true;
    }

    private static void SortAllWorlds()
    {
        List<string> worldNames = TypeDefineConfigs[TypeDefineType.World].TypeIndexDict.Keys.ToList();
        foreach (string worldName in worldNames)
        {
            SortWorld(worldName);
        }
    }

    private static void SortWorld(string worldName)
    {
        string prefabPath = TypeDefineConfigs[TypeDefineType.World].GetTypeAssetDataBasePath(worldName);
        GameObject worldPrefab = PrefabUtility.LoadPrefabContents(prefabPath);
        if (worldPrefab)
        {
            WorldDesignHelper world = worldPrefab.GetComponent<WorldDesignHelper>();
            if (world)
            {
                bool isDirty = world.SortWorld();
                if (isDirty) PrefabUtility.SaveAsPrefabAsset(worldPrefab, prefabPath);
            }
        }

        PrefabUtility.UnloadPrefabContents(worldPrefab);
    }

    public static EntityBuffAttributeMatrixAsset GetBuffAttributeMatrixAsset()
    {
        FileInfo assetFile = new FileInfo(Application.dataPath + DesignRoot + EntityBuffAttributeMatrixAssetPath_Relative);
        string relativePath = CommonUtils.ConvertAbsolutePathToProjectPath(assetFile.FullName);
        Object configObj = AssetDatabase.LoadAssetAtPath<Object>(relativePath);
        EntityBuffAttributeMatrixAsset configSSO = (EntityBuffAttributeMatrixAsset) configObj;
        return configSSO;
    }

    public static void CreateNewBuffAttributeMatrixAsset()
    {
        EntityBuffAttributeMatrixAsset configSSO = GetBuffAttributeMatrixAsset();
        int buffTypeEnumCount = Enum.GetValues(typeof(EntityBuffAttribute)).Length;
        configSSO.EntityBuffAttributeMatrix = new EntityBuffAttributeRelationship[buffTypeEnumCount, buffTypeEnumCount];
        ExportEntityBuffAttributeMatrix(DataFormat.JSON);
    }

    public static void ExportEntityBuffAttributeMatrix(DataFormat dataFormat)
    {
        string folder = EntityBuffAttributeMatrixConfigFolder_Build;
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        string file = $"{folder}/EntityBuffAttributeMatrix.config";
        if (File.Exists(file)) File.Delete(file);

        EntityBuffAttributeMatrixAsset configSSO = GetBuffAttributeMatrixAsset();

        SortedDictionary<string, SortedDictionary<string, string>> exportDict = new SortedDictionary<string, SortedDictionary<string, string>>();
        // 左侧标签为既有buff，顶部标签为新增buff
        for (int y = 0; y < configSSO.EntityBuffAttributeMatrix.GetLength(0); y++)
        {
            for (int x = 0; x < configSSO.EntityBuffAttributeMatrix.GetLength(1); x++)
            {
                EntityBuffAttribute oldBuff = (EntityBuffAttribute) y; // 左侧竖直标签
                EntityBuffAttribute newBuff = (EntityBuffAttribute) x; // 顶部水平标签
                if (!exportDict.ContainsKey(oldBuff.ToString()))
                {
                    exportDict.Add(oldBuff.ToString(), new SortedDictionary<string, string>());
                }

                if (!exportDict[oldBuff.ToString()].ContainsKey(newBuff.ToString()))
                {
                    exportDict[oldBuff.ToString()].Add(newBuff.ToString(), configSSO.EntityBuffAttributeMatrix[y, x].ToString());
                }
            }
        }

        byte[] bytes = SerializationUtility.SerializeValue(exportDict, DataFormat.JSON);
        File.WriteAllBytes(file, bytes);
        AssetDatabase.Refresh();
    }

    private static void ExportAllEntityOccupationDataConfigs(DataFormat dataFormat)
    {
        EntityOccupationConfigDict.Clear();
        string folder = EntityOccupationConfigDictFolder_Build;
        string file = folder + "EntityOccupation.config";
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        if (File.Exists(file)) File.Delete(file);

        // Box's occupation
        List<string> boxNames = TypeDefineConfigs[TypeDefineType.Box].TypeIndexDict.Keys.ToList();
        foreach (string boxName in boxNames)
        {
            string prefabPath = TypeDefineConfigs[TypeDefineType.Box].GetTypeAssetDataBasePath(boxName);
            GameObject boxPrefab = PrefabUtility.LoadPrefabContents(prefabPath);
            if (boxPrefab)
            {
                Box box = boxPrefab.GetComponent<Box>();
                if (box)
                {
                    box.BoxIndicatorHelper.RefreshEntityIndicatorOccupationData();
                    EditorUtility.SetDirty(boxPrefab);
                    EntityOccupationData occupationData = box.BoxIndicatorHelper.EntityOccupationData.Clone();
                    ushort entityTypeIndex = TypeDefineConfigs[TypeDefineType.Box].TypeIndexDict[box.name];
                    if (entityTypeIndex != 0)
                    {
                        EntityOccupationConfigDict.Add(entityTypeIndex, occupationData);
                    }

                    PrefabUtility.SaveAsPrefabAsset(boxPrefab, prefabPath, out bool suc);
                }
            }

            PrefabUtility.UnloadPrefabContents(boxPrefab);
        }

        // Actor's occupation
        List<string> actorNames = TypeDefineConfigs[TypeDefineType.Actor].TypeIndexDict.Keys.ToList();
        foreach (string actorName in actorNames)
        {
            string prefabPath = TypeDefineConfigs[TypeDefineType.Actor].GetTypeAssetDataBasePath(actorName);
            GameObject actorPrefab = PrefabUtility.LoadPrefabContents(prefabPath);
            if (actorPrefab)
            {
                Actor actor = actorPrefab.GetComponent<Actor>();
                if (actor)
                {
                    actor.EntityIndicatorHelper.RefreshEntityIndicatorOccupationData();
                    EditorUtility.SetDirty(actorPrefab);
                    EntityOccupationData occupationData = actor.ActorIndicatorHelper.EntityOccupationData.Clone();
                    ushort entityTypeIndex = TypeDefineConfigs[TypeDefineType.Actor].TypeIndexDict[actor.name];
                    if (entityTypeIndex != 0)
                    {
                        EntityOccupationConfigDict.Add(entityTypeIndex, occupationData);
                    }

                    PrefabUtility.SaveAsPrefabAsset(actorPrefab, prefabPath, out bool suc);
                }
            }

            PrefabUtility.UnloadPrefabContents(actorPrefab);
        }

        byte[] bytes = SerializationUtility.SerializeValue(EntityOccupationConfigDict, dataFormat);
        File.WriteAllBytes(file, bytes);
    }

    private static void ExportAllWorldModuleDataConfigs(DataFormat dataFormat)
    {
        string folder = WorldModuleDataConfigFolder_Build;
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        DirectoryInfo di = new DirectoryInfo(Application.dataPath + DesignRoot + WorldModuleDataConfigFolder_Relative);
        foreach (FileInfo fi in di.GetFiles("*.prefab", SearchOption.AllDirectories))
        {
            ExportWorldModuleDataConfigCore(dataFormat, fi, folder);
        }
    }

    private static void ExportWorldModuleDataConfig(DataFormat dataFormat, string worldModuleName)
    {
        string folder = WorldModuleDataConfigFolder_Build;
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        DirectoryInfo di = new DirectoryInfo(Application.dataPath + DesignRoot + WorldModuleDataConfigFolder_Relative);
        foreach (FileInfo fi in di.GetFiles("*.prefab", SearchOption.AllDirectories))
        {
            if (fi.Name.Equals(worldModuleName + ".prefab"))
            {
                ExportWorldModuleDataConfigCore(dataFormat, fi, folder);
                Debug.Log($"[世界模组] 序列化成功: {worldModuleName}");
            }
        }
    }

    private static void ExportWorldModuleDataConfigCore(DataFormat dataFormat, FileInfo fi, string folder)
    {
        string relativePath = CommonUtils.ConvertAbsolutePathToProjectPath(fi.FullName);
        GameObject obj = (GameObject) AssetDatabase.LoadAssetAtPath<Object>(relativePath);
        WorldModuleDesignHelper module = obj.GetComponent<WorldModuleDesignHelper>();
        WorldModuleData data = module.ExportWorldModuleData();
        TypeDefineConfigs[TypeDefineType.WorldModule].TypeIndexDict.TryGetValue(module.name, out ushort worldModuleTypeIndex);
        if (worldModuleTypeIndex != 0)
        {
            data.WorldModuleTypeIndex = worldModuleTypeIndex;
            data.WorldModuleTypeName = module.name;
            string path = folder + module.name + ".config";
            byte[] bytes = SerializationUtility.SerializeValue(data, dataFormat);
            File.WriteAllBytes(path, bytes);
        }
        else
        {
            Debug.LogError($"WorldModuleTypeDefineDict 中无{module.name}");
        }
    }

    private static void ExportAllStaticLayoutDataConfigs(DataFormat dataFormat)
    {
        string folder = StaticLayoutDataConfigFolder_Build;
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        DirectoryInfo di = new DirectoryInfo(Application.dataPath + DesignRoot + StaticLayoutDataConfigFolder_Relative);
        foreach (FileInfo fi in di.GetFiles("*.prefab", SearchOption.AllDirectories))
        {
            ExportStaticLayoutDataConfigCore(dataFormat, fi, folder);
        }
    }

    private static void ExportStaticLayoutDataConfig(DataFormat dataFormat, string staticLayoutName)
    {
        string folder = StaticLayoutDataConfigFolder_Build;
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        DirectoryInfo di = new DirectoryInfo(Application.dataPath + DesignRoot + StaticLayoutDataConfigFolder_Relative);
        foreach (FileInfo fi in di.GetFiles("*.prefab", SearchOption.AllDirectories))
        {
            if (fi.Name.Equals(staticLayoutName + ".prefab"))
            {
                ExportStaticLayoutDataConfigCore(dataFormat, fi, folder);
                Debug.Log($"[静态布局] 序列化成功: {staticLayoutName}");
            }
        }
    }

    private static void ExportStaticLayoutDataConfigCore(DataFormat dataFormat, FileInfo fi, string folder)
    {
        string relativePath = CommonUtils.ConvertAbsolutePathToProjectPath(fi.FullName);
        GameObject obj = (GameObject) AssetDatabase.LoadAssetAtPath<Object>(relativePath);
        WorldModuleDesignHelper module = obj.GetComponent<WorldModuleDesignHelper>();
        WorldModuleData data = module.ExportWorldModuleData();
        TypeDefineConfigs[TypeDefineType.StaticLayout].TypeIndexDict.TryGetValue(module.name, out ushort worldModuleTypeIndex);
        if (worldModuleTypeIndex != 0)
        {
            data.WorldModuleTypeIndex = worldModuleTypeIndex;
            data.WorldModuleTypeName = module.name;
            string path = folder + module.name + ".config";
            byte[] bytes = SerializationUtility.SerializeValue(data, dataFormat);
            File.WriteAllBytes(path, bytes);
        }
        else
        {
            Debug.LogError($"StaticLayoutTypeDefineDict 中无{module.name}");
        }
    }

    private static void ExportAllWorldDataConfigs(DataFormat dataFormat)
    {
        string folder = WorldDataConfigFolder_Build;
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        DirectoryInfo di = new DirectoryInfo(Application.dataPath + DesignRoot + WorldDataConfigFolder_Relative);
        foreach (FileInfo fi in di.GetFiles("*.prefab", SearchOption.AllDirectories))
        {
            ExportWorldDataConfigCore(dataFormat, fi, folder);
        }
    }

    private static void ExportWorldDataConfig(DataFormat dataFormat, string worldName)
    {
        string folder = WorldDataConfigFolder_Build;
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        DirectoryInfo di = new DirectoryInfo(Application.dataPath + DesignRoot + WorldDataConfigFolder_Relative);
        foreach (FileInfo fi in di.GetFiles("*.prefab", SearchOption.AllDirectories))
        {
            if (fi.Name.Equals(worldName + ".prefab"))
            {
                ExportWorldDataConfigCore(dataFormat, fi, folder);
                Debug.Log($"[世界] 序列化成功: {worldName}");
            }
        }
    }

    private static void ExportWorldDataConfigCore(DataFormat dataFormat, FileInfo fi, string folder)
    {
        string relativePath = CommonUtils.ConvertAbsolutePathToProjectPath(fi.FullName);
        GameObject obj = (GameObject) AssetDatabase.LoadAssetAtPath<Object>(relativePath);
        WorldDesignHelper world = obj.GetComponent<WorldDesignHelper>();
        WorldData data = world.ExportWorldData();
        TypeDefineConfigs[TypeDefineType.World].TypeIndexDict.TryGetValue(world.name, out ushort worldTypeIndex);
        if (worldTypeIndex != 0)
        {
            data.WorldTypeIndex = worldTypeIndex;
            data.WorldTypeName = world.name;
            string path = folder + world.name + ".config";
            byte[] bytes = SerializationUtility.SerializeValue(data, dataFormat);
            File.WriteAllBytes(path, bytes);
        }
        else
        {
            Debug.LogError($"WorldTypeDefineDict 中无{world.name}");
        }
    }

#endif

    #endregion

    #region Load

    public static bool IsLoaded = false;

#if UNITY_EDITOR
    [MenuItem("开发工具/配置/加载配置")]
#endif
    public static void LoadAllConfigs()
    {
        if (IsLoaded) return;
#if UNITY_EDITOR
        if (IsExporting) return;
#endif
        Clear();
        DataFormat dataFormat = DataFormat.Binary;

        AllBuffAttributeTypes = Enum.GetValues(typeof(EntityBuffAttribute));

        foreach (KeyValuePair<TypeDefineType, TypeDefineConfig> kv in TypeDefineConfigs)
        {
            kv.Value.LoadTypeNames();
        }

        LoadTypeGUIDMappingFromConfig(dataFormat);
        LoadPassiveSkillLibrary(dataFormat);
        LoadEntityStatPropertyEnumList();
        LoadEntityBuffStatPropertyEnumReflection();
        LoadEntityBuffAttributeMatrixFromConfig(dataFormat);
        LoadAllEntityOccupationDataConfigs(dataFormat);
        LoadWorldModuleDataConfig(dataFormat);
        LoadStaticLayoutDataConfig(dataFormat);
        LoadWorldDataConfig(dataFormat);

        IsLoaded = true;
    }

    public static void LoadTypeGUIDMappingFromConfig(DataFormat dataFormat)
    {
        string file = $"{TypeGUIDMappingConfigFolder_Build}/TypeGUIDMapping.config";
        FileInfo fi = new FileInfo(file);
        if (fi.Exists)
        {
            byte[] bytes = File.ReadAllBytes(fi.FullName);
            TypeGUIDMappings.Clear();
            Dictionary<string, Dictionary<string, string>> loadDict = SerializationUtility.DeserializeValue<Dictionary<string, Dictionary<string, string>>>(bytes, DataFormat.JSON);
            foreach (KeyValuePair<string, Dictionary<string, string>> kv in loadDict)
            {
                if (Enum.TryParse(kv.Key, out TypeDefineType typeDefineType))
                {
                    TypeGUIDMappings.Add(typeDefineType, new TypeGUIDMappingAsset.Mapping());
                    foreach (KeyValuePair<string, string> _kv in kv.Value)
                    {
                        TypeGUIDMappings[typeDefineType].LoadData(_kv.Key, _kv.Value);
                    }
                }
            }
        }
    }

    private static void LoadPassiveSkillLibrary(DataFormat dataFormat)
    {
        EntitySkillLibrary.Clear();

        string folder = EntitySkillLibraryFolder_Build;
        DirectoryInfo di = new DirectoryInfo(WorldDataConfigFolder_Build);
        if (di.Exists)
        {
            string path = folder + "EntitySkillLibrary.config";
            FileInfo fi = new FileInfo(path);
            byte[] bytes = File.ReadAllBytes(fi.FullName);
            Dictionary<string, EntitySkill> data = SerializationUtility.DeserializeValue<Dictionary<string, EntitySkill>>(bytes, dataFormat);
            foreach (KeyValuePair<string, EntitySkill> kv in data)
            {
                EntitySkillLibrary.Add(kv.Key, kv.Value);
            }
        }
        else
        {
            Debug.LogError("技能库不存在");
        }
    }

    public static void LoadEntityStatPropertyEnumList()
    {
        foreach (EntityStatType est in Enum.GetValues(typeof(EntityStatType)))
        {
            EntityStatPropSet.EntityStatTypeEnumList.Add(est);
        }

        foreach (EntityPropertyType ept in Enum.GetValues(typeof(EntityPropertyType)))
        {
            EntityStatPropSet.EntityPropertyTypeEnumList.Add(ept);
        }
    }

    public static void LoadEntityBuffStatPropertyEnumReflection()
    {
        Type PT = typeof(EntityPropertyType);
        foreach (EntityPropertyType ept in Enum.GetValues(PT))
        {
            object[] attributes_entity = PT.GetMember(ept.ToString())[0].GetCustomAttributes(typeof(EntityPropertyAttribute), false);
            if (attributes_entity.Length > 0)
            {
                EntityBuffHelper.BoxBuffEnums_Property.Add(ept);
                EntityBuffHelper.ActorBuffEnums_Property.Add(ept);
            }
            else
            {
                object[] attributes_box = PT.GetMember(ept.ToString())[0].GetCustomAttributes(typeof(BoxPropertyAttribute), false);
                if (attributes_box.Length > 0) EntityBuffHelper.BoxBuffEnums_Property.Add(ept);
                object[] attribute_actor = PT.GetMember(ept.ToString())[0].GetCustomAttributes(typeof(ActorPropertyAttribute), false);
                if (attribute_actor.Length > 0) EntityBuffHelper.ActorBuffEnums_Property.Add(ept);
            }
        }

        Type ST = typeof(EntityStatType);
        foreach (EntityStatType est in Enum.GetValues(ST))
        {
            object[] attributes_entity = ST.GetMember(est.ToString())[0].GetCustomAttributes(typeof(EntityStatAttribute), false);
            if (attributes_entity.Length > 0)
            {
                EntityBuffHelper.BoxBuffEnums_Stat.Add(est);
                EntityBuffHelper.ActorBuffEnums_Stat.Add(est);
            }
            else
            {
                object[] attribute_box = ST.GetMember(est.ToString())[0].GetCustomAttributes(typeof(BoxStatAttribute), false);
                if (attribute_box.Length > 0) EntityBuffHelper.BoxBuffEnums_Stat.Add(est);
                object[] attribute_actor = ST.GetMember(est.ToString())[0].GetCustomAttributes(typeof(ActorStatAttribute), false);
                if (attribute_actor.Length > 0) EntityBuffHelper.ActorBuffEnums_Stat.Add(est);
            }
        }
    }

    public static void LoadEntityBuffAttributeMatrixFromConfig(DataFormat dataFormat)
    {
        string file = $"{EntityBuffAttributeMatrixConfigFolder_Build}/EntityBuffAttributeMatrix.config";
        FileInfo fi = new FileInfo(file);
        if (fi.Exists)
        {
            byte[] bytes = File.ReadAllBytes(fi.FullName);
            SortedDictionary<string, SortedDictionary<string, string>> loadDict = SerializationUtility.DeserializeValue<SortedDictionary<string, SortedDictionary<string, string>>>(bytes, DataFormat.JSON);
            int buffTypeEnumCount = Enum.GetValues(typeof(EntityBuffAttribute)).Length;
            EntityBuffAttributeMatrix = new EntityBuffAttributeRelationship[buffTypeEnumCount, buffTypeEnumCount];
            foreach (KeyValuePair<string, SortedDictionary<string, string>> kv in loadDict)
            {
                foreach (KeyValuePair<string, string> _kv in kv.Value)
                {
                    if (Enum.TryParse(kv.Key, out EntityBuffAttribute oldBuff))
                    {
                        if (Enum.TryParse(_kv.Key, out EntityBuffAttribute newBuff))
                        {
                            if (Enum.TryParse(_kv.Value, out EntityBuffAttributeRelationship value))
                            {
                                EntityBuffAttributeMatrix[(int) oldBuff, (int) newBuff] = value;
                            }
                        }
                    }
                }
            }
        }
    }

#if UNITY_EDITOR
    public static void LoadEntityBuffAttributeMatrixFromAsset()
    {
        EntityBuffAttributeMatrixAsset configSSO = GetBuffAttributeMatrixAsset();
        EntityBuffAttributeMatrix = configSSO.EntityBuffAttributeMatrix;
    }

    public static void UpgradeEntityBuffAttributeMatrixAssetVersion()
    {
        EntityBuffAttributeMatrixAsset configSSO = GetBuffAttributeMatrixAsset();
        configSSO.version += 1;
        EditorUtility.SetDirty(configSSO);
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }
#endif

    private static void LoadAllEntityOccupationDataConfigs(DataFormat dataFormat)
    {
        EntityOccupationConfigDict.Clear();

        DirectoryInfo di = new DirectoryInfo(EntityOccupationConfigDictFolder_Build);
        if (di.Exists)
        {
            foreach (FileInfo fi in di.GetFiles("*.config", SearchOption.AllDirectories))
            {
                byte[] bytes = File.ReadAllBytes(fi.FullName);
                Dictionary<ushort, EntityOccupationData> data = SerializationUtility.DeserializeValue<Dictionary<ushort, EntityOccupationData>>(bytes, dataFormat);
                foreach (KeyValuePair<ushort, EntityOccupationData> kv in data)
                {
                    kv.Value.CalculateEveryOrientationOccupationGPs();
                    EntityOccupationConfigDict.Add(kv.Key, kv.Value);
                }
            }
        }
        else
        {
            Debug.LogError("Entity占位配置表不存在");
        }
    }

    private static void LoadWorldModuleDataConfig(DataFormat dataFormat)
    {
        WorldModuleDataConfigDict.Clear();

        DirectoryInfo di = new DirectoryInfo(WorldModuleDataConfigFolder_Build);
        if (di.Exists)
        {
            foreach (FileInfo fi in di.GetFiles("*.config", SearchOption.AllDirectories))
            {
                byte[] bytes = File.ReadAllBytes(fi.FullName);
                WorldModuleData data = SerializationUtility.DeserializeValue<WorldModuleData>(bytes, dataFormat);
                if (WorldModuleDataConfigDict.ContainsKey(data.WorldModuleTypeIndex))
                {
                    Debug.LogError($"世界模组重名:{data.WorldModuleTypeIndex}");
                }
                else
                {
                    WorldModuleDataConfigDict.Add(data.WorldModuleTypeIndex, data);
                }
            }
        }
        else
        {
            Debug.LogError("世界模组配置表不存在");
        }
    }

    private static void LoadStaticLayoutDataConfig(DataFormat dataFormat)
    {
        StaticLayoutDataConfigDict.Clear();

        DirectoryInfo di = new DirectoryInfo(StaticLayoutDataConfigFolder_Build);
        if (di.Exists)
        {
            foreach (FileInfo fi in di.GetFiles("*.config", SearchOption.AllDirectories))
            {
                byte[] bytes = File.ReadAllBytes(fi.FullName);
                WorldModuleData data = SerializationUtility.DeserializeValue<WorldModuleData>(bytes, dataFormat);
                if (StaticLayoutDataConfigDict.ContainsKey(data.WorldModuleTypeIndex))
                {
                    Debug.LogError($"静态布局重名:{data.WorldModuleTypeIndex}");
                }
                else
                {
                    StaticLayoutDataConfigDict.Add(data.WorldModuleTypeIndex, data);
                }
            }
        }
        else
        {
            Debug.LogError("静态布局配置表不存在");
        }
    }

    private static void LoadWorldDataConfig(DataFormat dataFormat)
    {
        WorldDataConfigDict.Clear();

        DirectoryInfo di = new DirectoryInfo(WorldDataConfigFolder_Build);
        if (di.Exists)
        {
            foreach (FileInfo fi in di.GetFiles("*.config", SearchOption.AllDirectories))
            {
                byte[] bytes = File.ReadAllBytes(fi.FullName);
                WorldData data = SerializationUtility.DeserializeValue<WorldData>(bytes, dataFormat);
                if (WorldDataConfigDict.ContainsKey(data.WorldTypeIndex))
                {
                    Debug.LogError($"世界重名:{data.WorldTypeIndex}");
                }
                else
                {
                    WorldDataConfigDict.Add(data.WorldTypeIndex, data);
                }
            }
        }
        else
        {
            Debug.LogError("世界配置表不存在");
        }
    }

    #endregion

    #region Special types const

    public static IEnumerable AllBuffAttributeTypes;
    public static ushort WorldModule_EmptyModuleIndex => GetTypeIndex(TypeDefineType.WorldModule, "Common_Empty");
    public static ushort World_OpenWorldIndex => GetTypeIndex(TypeDefineType.World, "OpenWorld");
    public static ushort Box_EnemyFrozenBoxIndex => GetTypeIndex(TypeDefineType.Box, "EnemyFrozenBox");
    public static ushort Box_GroundBoxIndex => GetTypeIndex(TypeDefineType.Box, "GroundBox");
    public static ushort Box_BrickBoxIndex => GetTypeIndex(TypeDefineType.Box, "BrickBox");
    public static ushort Box_CombinedGroundBoxIndex => GetTypeIndex(TypeDefineType.Box, "CombinedGroundBox");
    public static ushort Actor_PlayerIndex => GetTypeIndex(TypeDefineType.Actor, "Player");

    #endregion

    #region Getter

    public static IEnumerable GetAllBuffAttributeTypes()
    {
        return AllBuffAttributeTypes;
    }

    public static List<string> GetAllTypeNames(TypeDefineType typeDefineType, bool withNone = true)
    {
        LoadAllConfigs();
        List<string> res = TypeDefineConfigs[typeDefineType].TypeIndexDict.Keys.ToList();
        if (withNone) res.Insert(0, "None");
        return res;
    }

    public static string GetTypeName(TypeDefineType typeDefineType, ushort typeIndex)
    {
        if (!IsLoaded) LoadAllConfigs();
        TypeDefineConfigs[typeDefineType].TypeNameDict.TryGetValue(typeIndex, out string typeName);
        return typeName;
    }

    public static ushort GetTypeIndex(TypeDefineType typeDefineType, string typeName)
    {
        if (!IsLoaded) LoadAllConfigs();
        if (typeName == "None" || string.IsNullOrEmpty(typeName)) return 0;
        if (TypeDefineConfigs[typeDefineType].TypeIndexDict.TryGetValue(typeName, out ushort typeIndex))
        {
            return typeIndex;
        }
        else
        {
            Debug.Log(CommonUtils.HighlightStringFormat("无法找到名为{0}的{1}的资源", "#00A9D1", typeName, typeDefineType));
            return 0;
        }
    }

    public static EntitySkill GetEntitySkill(string skillGUID)
    {
        if (EntitySkillLibrary.TryGetValue(skillGUID, out EntitySkill entitySkill))
        {
            return entitySkill.Clone();
        }
        else
        {
            return null;
        }
    }

    public static EntityOccupationData GetEntityOccupationData(ushort entityTypeIndex)
    {
        if (!IsLoaded) LoadAllConfigs();
        EntityOccupationConfigDict.TryGetValue(entityTypeIndex, out EntityOccupationData occupationData);
        return occupationData;
    }

    public static WorldModuleData GetWorldModuleDataConfig(ushort worldModuleTypeIndex)
    {
        if (!IsLoaded) LoadAllConfigs();
        WorldModuleDataConfigDict.TryGetValue(worldModuleTypeIndex, out WorldModuleData worldModuleData);
        return worldModuleData?.Clone();
    }

    public static WorldModuleData GetStaticLayoutDataConfig(ushort staticLayoutTypeIndex, bool clone)
    {
        if (!IsLoaded) LoadAllConfigs();
        StaticLayoutDataConfigDict.TryGetValue(staticLayoutTypeIndex, out WorldModuleData worldModuleData);
        if (clone)
        {
            return worldModuleData?.Clone();
        }
        else
        {
            return worldModuleData;
        }
    }

    public static WorldData GetWorldDataConfig(ushort worldTypeIndex)
    {
        if (!IsLoaded) LoadAllConfigs();
        WorldDataConfigDict.TryGetValue(worldTypeIndex, out WorldData worldData);
        return worldData?.Clone();
    }

    public static Material GetSkyBoxByName(string skyBoxTypeName)
    {
        return Resources.Load<Material>($"SkyBox/{skyBoxTypeName}");
    }

    public static PostProcessProfile GetPostProcessingProfileByName(string postProcessingProfileTypeName)
    {
        return Resources.Load<PostProcessProfile>($"PostProcessingProfile/{postProcessingProfileTypeName}");
    }

    #endregion

    #region Prefabs

#if UNITY_EDITOR
    public static GameObject FindActorPrefabByName(string actorName)
    {
        TypeDefineConfigs[TypeDefineType.Actor].ExportTypeNames(); // todo 判断是否要删掉此行
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TypeDefineConfigs[TypeDefineType.Actor].GetTypeAssetDataBasePath(actorName));
        return prefab;
    }

    public static GameObject FindBoxPrefabByName(string boxName)
    {
        TypeDefineConfigs[TypeDefineType.Box].ExportTypeNames(); // todo 判断是否要删掉此行
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TypeDefineConfigs[TypeDefineType.Box].GetTypeAssetDataBasePath(boxName));
        return prefab;
    }

    public static GameObject FindEntityLevelEditorPrefabByName(TypeDefineType entityType, string entityName)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FindEntityLevelEditorPrefabPathByName(entityType, entityName));
        return prefab;
    }

    public static string FindEntityLevelEditorPrefabPathByName(TypeDefineType entityType, string entityName)
    {
        TypeDefineConfigs[entityType].ExportTypeNames(); // todo 判断是否要删掉此行
        string boxPrefabPath = TypeDefineConfigs[entityType].GetTypeAssetDataBasePath(entityName);
        if (entityType == TypeDefineType.Box)
        {
            return boxPrefabPath.Replace($"/{entityType}/", $"/{entityType}_LevelEditor/").Replace(entityName, entityName + "_LevelEditor");
        }
        else if (entityType == TypeDefineType.Actor)
        {
            return boxPrefabPath.Replace($"/{entityType}/", $"/{entityType}_LevelEditor/").Replace(entityName, entityName + "_LevelEditor");
        }

        return null;
    }

    public static GameObject FindLevelTriggerPrefabByName(string levelTriggerName)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TypeDefineConfigs[TypeDefineType.LevelTrigger].GetTypeAssetDataBasePath(levelTriggerName));
        return prefab;
    }

    public static GameObject FindWorldModulePrefabByName(TypeDefineType worldModuleType, string worldModuleName)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FindWorldModulePrefabPathByName(worldModuleType, worldModuleName));
        return prefab;
    }

    public static string FindWorldModulePrefabPathByName(TypeDefineType worldModuleType, string worldModuleName)
    {
        TypeDefineConfigs[worldModuleType].ExportTypeNames(); // todo 判断是否要删掉此行
        return TypeDefineConfigs[worldModuleType].GetTypeAssetDataBasePath(worldModuleName);
    }

    public static GameObject FindWorldPrefabByName(string worldName)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FindWorldPrefabPathByName(worldName));
        return prefab;
    }

    public static string FindWorldPrefabPathByName(string worldName)
    {
        TypeDefineConfigs[TypeDefineType.World].ExportTypeNames(); // todo 判断是否要删掉此行
        return TypeDefineConfigs[TypeDefineType.World].GetTypeAssetDataBasePath(worldName);
    }

#endif

    #endregion
}
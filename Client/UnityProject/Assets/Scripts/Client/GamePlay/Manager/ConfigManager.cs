using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BiangLibrary;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.GamePlay;
using BiangLibrary.Singleton;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
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

    public static Dictionary<MarchingTextureCase, Texture>[,] TerrainMarchingTextureDict = new Dictionary<MarchingTextureCase, Texture>[10, 10]; // 上三角矩阵 index_0 <= index_1

    public enum TypeStartIndex
    {
        None = 0,
        Box = 1,
        BoxIcon = 10000,
        Enemy = 11000,
        LevelTrigger = 12000,
        WorldModule = 13000,
        StaticLayout = 20000,
        World = 30000,
        FX = 31000,
        BattleIndicator = 32000,
        Player = 65535,
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
        {TypeDefineType.Box, new TypeDefineConfig<Box>("Box", "/Resources/Prefabs/Designs/Box", true, ConfigManager.TypeStartIndex.Box)},
        {TypeDefineType.BoxIcon, new TypeDefineConfig<Texture2D>("BoxIcon", "/Resources/BoxIcons", true, ConfigManager.TypeStartIndex.BoxIcon)},
        {TypeDefineType.Enemy, new TypeDefineConfig<EnemyActor>("Enemy", "/Resources/Prefabs/Designs/Enemy", true, ConfigManager.TypeStartIndex.Enemy)},
        {TypeDefineType.LevelTrigger, new TypeDefineConfig<LevelTriggerBase>("LevelTrigger", "/Resources/Prefabs/Designs/LevelTrigger", true, ConfigManager.TypeStartIndex.LevelTrigger)},
        {TypeDefineType.WorldModule, new TypeDefineConfig<WorldModuleDesignHelper>("WorldModule", "/Designs/WorldModule", true, ConfigManager.TypeStartIndex.WorldModule)},
        {TypeDefineType.StaticLayout, new TypeDefineConfig<WorldModuleDesignHelper>("StaticLayout", "/Designs/StaticLayout", true, ConfigManager.TypeStartIndex.StaticLayout)},
        {TypeDefineType.World, new TypeDefineConfig<WorldDesignHelper>("World", "/Designs/Worlds", true, ConfigManager.TypeStartIndex.World)},
        {TypeDefineType.FX, new TypeDefineConfig<FX>("FX", "/Resources/Prefabs/FX", true, ConfigManager.TypeStartIndex.FX)},
        {TypeDefineType.BattleIndicator, new TypeDefineConfig<BattleIndicator>("BattleIndicator", "/Resources/Prefabs/BattleIndicator", true, ConfigManager.TypeStartIndex.BattleIndicator)},
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

    public static string ConfigFolder_Build = Application.streamingAssetsPath + "/Configs/";
    public static string TypeNamesConfigFolder_Build = ConfigFolder_Build + "/TypeNames";
    public static string EntityBuffAttributeMatrixConfigFolder_Build = ConfigFolder_Build + EntityBuffAttributeMatrixConfigFolder_Relative + "/";
    public static string TypeGUIDMappingConfigFolder_Build = ConfigFolder_Build + TypeGUIDMappingConfigFolder_Relative + "/";
    public static string EntityOccupationConfigDictFolder_Build = ConfigFolder_Build + EntityOccupationConfigDictFolder_Relative + "/";
    public static string WorldModuleDataConfigFolder_Build = ConfigFolder_Build + WorldModuleDataConfigFolder_Relative + "s/";
    public static string StaticLayoutDataConfigFolder_Build = ConfigFolder_Build + StaticLayoutDataConfigFolder_Relative + "s/";
    public static string WorldDataConfigFolder_Build = ConfigFolder_Build + WorldDataConfigFolder_Relative + "/";

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

        // 时序，后面的导出都需要映射表，因此映射表先导出并加载
        ExportTypeGUIDMappingAsset(dataFormat);
        LoadTypeGUIDMappingFromConfig(dataFormat);

        SortWorldModule();
        SortStaticLayouts();
        SortWorlds();
        ExportEntityBuffAttributeMatrix(dataFormat);
        ExportEntityOccupationDataConfig(dataFormat);
        ExportWorldModuleDataConfig(dataFormat);
        ExportStaticLayoutDataConfig(dataFormat);
        ExportWorldDataConfig(dataFormat);

        AssetDatabase.Refresh();
        IsLoaded = false;
        IsExporting = false;
        LoadAllConfigs();
        if (dialogShow) EditorUtility.DisplayDialog("提示", "序列化成功", "确定");
    }

    [MenuItem("开发工具/配置/快速序列化类型")]
    public static void QuickExportConfigs_TypeDefines()
    {
        if (Directory.Exists(TypeNamesConfigFolder_Build)) Directory.Delete(TypeNamesConfigFolder_Build, true);
        Directory.CreateDirectory(TypeNamesConfigFolder_Build);

        // 时序，先导出类型表
        ExportTypeGUIDMapping();

        // 时序，后面的导出都需要映射表，因此映射表先导出并加载
        ExportTypeGUIDMappingAsset(DataFormat.Binary);
        LoadTypeGUIDMappingFromConfig(DataFormat.Binary);

        AssetDatabase.Refresh();
        IsLoaded = false;
        LoadAllConfigs();
        EditorUtility.DisplayDialog("提示", "快速序列化类型成功", "确定");
    }

    private static void ExportTypeGUIDMapping()
    {
        TypeGUIDMappingAsset typeMapping = GetTypeGUIDMappingAsset();
        foreach (KeyValuePair<TypeDefineType, TypeDefineConfig> kv in TypeDefineConfigs)
        {
            kv.Value.ExportTypeNames();
            foreach (KeyValuePair<string, ushort> _kv in kv.Value.TypeIndexDict)
            {
                typeMapping.TypeGUIDMappings[kv.Key].TryAddNewType(_kv.Key);
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
                typeMapping.TypeGUIDMappings[kv.Key].Type_GUIDDict.Remove(key);
            }
        }
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

    private static void SortWorldModule()
    {
        List<string> worldModuleNames = TypeDefineConfigs[TypeDefineType.WorldModule].TypeIndexDict.Keys.ToList();
        foreach (string worldModuleName in worldModuleNames)
        {
            SortWorldModule(worldModuleName);
        }
    }

    private static void SortWorldModule(string worldModuleName)
    {
        string prefabPath = TypeDefineConfigs[TypeDefineType.WorldModule].GetTypeAssetDataBasePath(worldModuleName);
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
    }

    private static void SortStaticLayouts()
    {
        List<string> staticLayoutNames = TypeDefineConfigs[TypeDefineType.StaticLayout].TypeIndexDict.Keys.ToList();
        foreach (string staticLayoutName in staticLayoutNames)
        {
            SortStaticLayout(staticLayoutName);
        }
    }

    private static void SortStaticLayout(string staticLayoutName)
    {
        string prefabPath = TypeDefineConfigs[TypeDefineType.StaticLayout].GetTypeAssetDataBasePath(staticLayoutName);
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
    }

    private static void SortWorlds()
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

    private static void ExportEntityOccupationDataConfig(DataFormat dataFormat)
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
                    EntityOccupationData occupationData = box.GetEntityOccupationGPs_Editor().Clone();
                    ushort entityTypeIndex = TypeDefineConfigs[TypeDefineType.Box].TypeIndexDict[box.name];
                    if (entityTypeIndex != 0)
                    {
                        EntityOccupationConfigDict.Add(entityTypeIndex, occupationData);
                    }

                    PrefabUtility.SaveAsPrefabAsset(boxPrefab, prefabPath);
                }
            }

            PrefabUtility.UnloadPrefabContents(boxPrefab);
        }

        // Enemy's occupation
        List<string> enemyNames = TypeDefineConfigs[TypeDefineType.Enemy].TypeIndexDict.Keys.ToList();
        foreach (string enemyName in enemyNames)
        {
            string prefabPath = TypeDefineConfigs[TypeDefineType.Enemy].GetTypeAssetDataBasePath(enemyName);
            GameObject enemyPrefab = PrefabUtility.LoadPrefabContents(prefabPath);
            if (enemyPrefab)
            {
                EnemyActor enemy = enemyPrefab.GetComponent<EnemyActor>();
                if (enemy)
                {
                    enemy.EntityIndicatorHelper.RefreshEntityIndicatorOccupationData();
                    EntityOccupationData occupationData = enemy.GetEntityOccupationGPs_Editor().Clone();
                    ushort entityTypeIndex = TypeDefineConfigs[TypeDefineType.Enemy].TypeIndexDict[enemy.name];
                    if (entityTypeIndex != 0)
                    {
                        EntityOccupationConfigDict.Add(entityTypeIndex, occupationData);
                    }

                    PrefabUtility.SaveAsPrefabAsset(enemyPrefab, prefabPath);
                }
            }

            PrefabUtility.UnloadPrefabContents(enemyPrefab);
        }

        // Player's occupation
        EntityOccupationData playerOccupationData = new EntityOccupationData();
        playerOccupationData.EntityIndicatorGPs.Add(GridPos3D.Zero);
        playerOccupationData.IsShapeCuboid = true;
        playerOccupationData.IsShapePlanSquare = true;
        playerOccupationData.BoundsInt = playerOccupationData.EntityIndicatorGPs.GetBoundingRectFromListGridPos(GridPos3D.Zero);
        EntityOccupationConfigDict.Add((ushort) TypeStartIndex.Player, playerOccupationData);

        byte[] bytes = SerializationUtility.SerializeValue(EntityOccupationConfigDict, dataFormat);
        File.WriteAllBytes(file, bytes);
    }

    private static void ExportWorldModuleDataConfig(DataFormat dataFormat)
    {
        string folder = WorldModuleDataConfigFolder_Build;
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        DirectoryInfo di = new DirectoryInfo(Application.dataPath + DesignRoot + WorldModuleDataConfigFolder_Relative);
        foreach (FileInfo fi in di.GetFiles("*.prefab", SearchOption.AllDirectories))
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
    }

    private static void ExportStaticLayoutDataConfig(DataFormat dataFormat)
    {
        string folder = StaticLayoutDataConfigFolder_Build;
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        DirectoryInfo di = new DirectoryInfo(Application.dataPath + DesignRoot + StaticLayoutDataConfigFolder_Relative);
        foreach (FileInfo fi in di.GetFiles("*.prefab", SearchOption.AllDirectories))
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
    }

    private static void ExportWorldDataConfig(DataFormat dataFormat)
    {
        string folder = WorldDataConfigFolder_Build;
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        DirectoryInfo di = new DirectoryInfo(Application.dataPath + DesignRoot + WorldDataConfigFolder_Relative);
        foreach (FileInfo fi in di.GetFiles("*.prefab", SearchOption.AllDirectories))
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

        LoadBoxMarchingTextureConfigMatrix();
        LoadTypeGUIDMappingFromConfig(dataFormat);
        LoadEntityBuffStatPropertyEnumReflection();
        LoadEntityBuffAttributeMatrixFromConfig(dataFormat);
        LoadEntityOccupationDataConfig(dataFormat);
        LoadWorldModuleDataConfig(dataFormat);
        LoadStaticLayoutDataConfig(dataFormat);
        LoadWorldDataConfig(dataFormat);

        IsLoaded = true;
    }

    public static void LoadBoxMarchingTextureConfigMatrix()
    {
        TerrainMarchingTextureDict = new Dictionary<MarchingTextureCase, Texture>[10, 10];
        foreach (TerrainType basicTerrain in Enum.GetValues(typeof(TerrainType)))
        {
            foreach (TerrainType transitTerrain in Enum.GetValues(typeof(TerrainType)))
            {
                if ((int) basicTerrain > (int) transitTerrain) continue;
                TerrainMarchingTextureDict[(int) basicTerrain, (int) transitTerrain] = new Dictionary<MarchingTextureCase, Texture>();
                if (basicTerrain == transitTerrain)
                {
                    foreach (MarchingTextureCase marchingTextureCase in Enum.GetValues(typeof(MarchingTextureCase)))
                    {
                        TerrainMarchingTextureDict[(int) basicTerrain, (int) transitTerrain].Add(marchingTextureCase, ClientGameManager.Instance.BoxMarchingTextureConfigMatrix.PureTerrain[(int) basicTerrain]);
                    }
                }
                else
                {
                    BoxMarchingTextureConfigSSO config = ClientGameManager.Instance.BoxMarchingTextureConfigMatrix.Matrix[(int) basicTerrain, (int) transitTerrain];
                    if (config != null && config.TextureDict != null)
                    {
                        foreach (KeyValuePair<MarchingTextureCase, Texture> kv in config.TextureDict)
                        {
                            TerrainMarchingTextureDict[(int) basicTerrain, (int) transitTerrain].Add(kv.Key, kv.Value);
                        }
                    }
                }
            }
        }
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

    private static void LoadEntityOccupationDataConfig(DataFormat dataFormat)
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

    public static ushort WorldModule_DeadZoneIndex => GetTypeIndex(TypeDefineType.WorldModule, "Common_DeadZone");
    public static ushort WorldModule_HiddenWallIndex => GetTypeIndex(TypeDefineType.WorldModule, "Common_Wall_Hidden");
    public static ushort WorldModule_GroundIndex => GetTypeIndex(TypeDefineType.WorldModule, "Common_Ground");
    public static ushort World_OpenWorldIndex => GetTypeIndex(TypeDefineType.World, "OpenWorld");
    public static ushort Box_EnemyFrozenBoxIndex => GetTypeIndex(TypeDefineType.Box, "EnemyFrozenBox");
    public static ushort Box_GroundBoxIndex => GetTypeIndex(TypeDefineType.Box, "GroundBox");
    public static ushort Box_BrickBoxIndex => GetTypeIndex(TypeDefineType.Box, "BrickBox");
    public static ushort Box_CombinedGroundBoxIndex => GetTypeIndex(TypeDefineType.Box, "CombinedGroundBox");

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

    #endregion

    #region Prefabs

#if UNITY_EDITOR
    public static GameObject FindBoxPrefabByName(string boxName)
    {
        TypeDefineConfigs[TypeDefineType.Box].ExportTypeNames(); // todo 判断是否要删掉此行
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TypeDefineConfigs[TypeDefineType.Box].GetTypeAssetDataBasePath(boxName));
        return prefab;
    }

    public static GameObject FindBoxLevelEditorPrefabByName(string boxName)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FindBoxLevelEditorPrefabPathByName(boxName));
        return prefab;
    }

    public static string FindBoxLevelEditorPrefabPathByName(string boxName)
    {
        TypeDefineConfigs[TypeDefineType.Box].ExportTypeNames(); // todo 判断是否要删掉此行
        string boxPrefabPath = TypeDefineConfigs[TypeDefineType.Box].GetTypeAssetDataBasePath(boxName);
        return boxPrefabPath.Replace("/Box/", "/Box_LevelEditor/").Replace(boxName, boxName + "_LevelEditor");
    }

    public static bool DeleteBoxPrefabByName(string boxName)
    {
        TypeDefineConfigs[TypeDefineType.Box].ExportTypeNames(); // todo 判断是否要删掉此行
        string boxPrefabPath = TypeDefineConfigs[TypeDefineType.Box].GetTypeAssetDataBasePath(boxName);
        string boxLevelEditorPrefabPath = boxPrefabPath.Replace("/Box/", "/Box_LevelEditor/").Replace(boxName, boxName + "_LevelEditor");
        bool res_1 = AssetDatabase.DeleteAsset(boxPrefabPath);
        bool res_2 = AssetDatabase.DeleteAsset(boxLevelEditorPrefabPath);
        return res_1; // 源Box的Prefab删除就算成功
    }

    public static string RenameBoxPrefabByName(string boxName, string targetBoxName)
    {
        TypeDefineConfigs[TypeDefineType.Box].ExportTypeNames(); // todo 判断是否要删掉此行
        string boxPrefabPath = TypeDefineConfigs[TypeDefineType.Box].GetTypeAssetDataBasePath(boxName);
        string boxLevelEditorPrefabPath = boxPrefabPath.Replace("/Box/", "/Box_LevelEditor/").Replace(boxName, boxName + "_LevelEditor");
        string res_1 = AssetDatabase.RenameAsset(boxPrefabPath, targetBoxName);
        string res_2 = AssetDatabase.RenameAsset(boxLevelEditorPrefabPath, targetBoxName + "_LevelEditor");
        return res_1 + "\t" + res_2;
    }

    public static GameObject FindActorPrefabByName(string actorName)
    {
        if (actorName.StartsWith("Player"))
        {
            PrefabManager.Instance.LoadPrefabs();
            return PrefabManager.Instance.GetPrefab("Player");
        }
        else
        {
            GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TypeDefineConfigs[TypeDefineType.Enemy].GetTypeAssetDataBasePath(actorName));
            return enemyPrefab;
        }
    }

    public static GameObject FindLevelTriggerPrefabByName(string levelTriggerName)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TypeDefineConfigs[TypeDefineType.LevelTrigger].GetTypeAssetDataBasePath(levelTriggerName));
        return prefab;
    }

    public static GameObject FindWorldModulePrefabByName(string worldModuleName)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FindWorldModulePrefabPathByName(worldModuleName));
        return prefab;
    }

    public static string FindWorldModulePrefabPathByName(string worldModuleName)
    {
        TypeDefineConfigs[TypeDefineType.WorldModule].ExportTypeNames(); // todo 判断是否要删掉此行
        return TypeDefineConfigs[TypeDefineType.WorldModule].GetTypeAssetDataBasePath(worldModuleName);
    }

    public static GameObject FindStaticLayoutPrefabByName(string staticLayoutName)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FindStaticLayoutPrefabPathByName(staticLayoutName));
        return prefab;
    }

    public static string FindStaticLayoutPrefabPathByName(string staticLayoutName)
    {
        TypeDefineConfigs[TypeDefineType.StaticLayout].ExportTypeNames(); // todo 判断是否要删掉此行
        return TypeDefineConfigs[TypeDefineType.StaticLayout].GetTypeAssetDataBasePath(staticLayoutName);
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
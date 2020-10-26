﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using BiangStudio;
using BiangStudio.GamePlay;
using BiangStudio.Singleton;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

public class ConfigManager : TSingletonBaseManager<ConfigManager>
{
    public enum GUID_Separator
    {
        Actor = 2000,
        Box = 3000,
        Buff = 10000,
    }

    public static bool ShowEnemyPathFinding = false;

    public static float BoxThrowDragFactor_Cheat = 10f;
    public static float BoxKickDragFactor_Cheat = 1f;
    public static float BoxWeightFactor_Cheat = 1f;

    public class TypeDefineConfig<T> where T : Object
    {
        public TypeDefineConfig(string typeNamePrefix, string prefabFolder, bool includeSubFolder)
        {
            TypeNamePrefix = typeNamePrefix;
            PrefabFolder_Relative = prefabFolder;
            IncludeSubFolder = includeSubFolder;
        }

        private string TypeNamePrefix;
        private string TypeNamesConfig_File => $"{TypeNamesConfigFolder_Build}/{TypeNamePrefix}Names.config";
        private string TypeNamesConfigFolder_Build => Application.streamingAssetsPath + $"/Configs/{TypeNamePrefix}";
        private string PrefabFolder_Relative;
        private bool IncludeSubFolder;

        public Dictionary<string, ushort> TypeIndexDict = new Dictionary<string, ushort>();
        public SortedDictionary<ushort, string> TypeNameDict = new SortedDictionary<ushort, string>();

        public Dictionary<string, string> TypeAssetDataBasePathDict = new Dictionary<string, string>();

        public string AssetDataBaseFolderPath => "Assets" + PrefabFolder_Relative;

        public string GetTypeAssetDataBasePath(string assetName)
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
        public void ExportTypeNames()
        {
            Clear();
            TypeAssetDataBasePathDict.Clear();
            string folder = TypeNamesConfigFolder_Build;
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
            Directory.CreateDirectory(folder);

            ushort index = 1;
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

        public void LoadTypeNames()
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

        public void Clear()
        {
            TypeIndexDict.Clear();
            TypeNameDict.Clear();
        }
    }

    [ShowInInspector]
    [LabelText("箱子类型表")]
    public static readonly TypeDefineConfig<Box> BoxTypeDefineDict = new TypeDefineConfig<Box>("Box", "/Resources/Prefabs/Designs/Box", true);

    [ShowInInspector]
    [LabelText("箱子Icon类型表")]
    public static readonly TypeDefineConfig<Texture2D> BoxIconTypeDefineDict = new TypeDefineConfig<Texture2D>("BoxIcon", "/Resources/BoxIcons", true);

    [ShowInInspector]
    [LabelText("敌人类型表")]
    public static readonly TypeDefineConfig<EnemyActor> EnemyTypeDefineDict = new TypeDefineConfig<EnemyActor>("Enemy", "/Resources/Prefabs/Designs/Enemy", true);

    [ShowInInspector]
    [LabelText("关卡Trigger类型表")]
    public static readonly TypeDefineConfig<LevelTriggerBase> LevelTriggerTypeDefineDict = new TypeDefineConfig<LevelTriggerBase>("LevelTrigger", "/Resources/Prefabs/Designs/LevelTrigger", true);

    [ShowInInspector]
    [LabelText("世界模组类型表")]
    public static readonly TypeDefineConfig<WorldModuleDesignHelper> WorldModuleTypeDefineDict = new TypeDefineConfig<WorldModuleDesignHelper>("WorldModule", "/Designs/WorldModule", true);

    [ShowInInspector]
    [LabelText("世界类型表")]
    public static readonly TypeDefineConfig<WorldDesignHelper> WorldTypeDefineDict = new TypeDefineConfig<WorldDesignHelper>("World", "/Designs/Worlds", true);

    [ShowInInspector]
    [LabelText("FX类型表")]
    public static readonly TypeDefineConfig<FX> FXTypeDefineDict = new TypeDefineConfig<FX>("FX", "/Resources/Prefabs/FX", true);

    [ShowInInspector]
    [LabelText("世界配置表")]
    public static readonly Dictionary<ushort, WorldData> WorldDataConfigDict = new Dictionary<ushort, WorldData>();

    [ShowInInspector]
    [LabelText("世界模组配置表")]
    public static readonly SortedDictionary<ushort, WorldModuleData> WorldModuleDataConfigDict = new SortedDictionary<ushort, WorldModuleData>();

    public static string DesignRoot = "/Designs/";

    public static string WorldDataConfigFolder_Relative = "Worlds";
    public static string WorldModuleDataConfigFolder_Relative = "WorldModule";

    public static string WorldDataConfigFolder_Build = Application.streamingAssetsPath + "/Configs/" + WorldDataConfigFolder_Relative + "/";
    public static string WorldModuleDataConfigFolder_Build = Application.streamingAssetsPath + "/Configs/" + WorldModuleDataConfigFolder_Relative + "s/";

    public override void Awake()
    {
        LoadAllConfigs();
    }

    private static void Clear()
    {
        BoxTypeDefineDict.Clear();
        BoxIconTypeDefineDict.Clear();
        EnemyTypeDefineDict.Clear();
        LevelTriggerTypeDefineDict.Clear();
        WorldModuleTypeDefineDict.Clear();
        WorldTypeDefineDict.Clear();
        FXTypeDefineDict.Clear();
        WorldDataConfigDict.Clear();
        WorldModuleDataConfigDict.Clear();
    }

    #region Export

#if UNITY_EDITOR
    [MenuItem("开发工具/配置/序列化配置")]
    public static void ExportConfigs()
    {
        // http://www.sirenix.net/odininspector/faq?Search=&t-11=on#faq
        DataFormat dataFormat = DataFormat.Binary;
        BoxTypeDefineDict.ExportTypeNames();
        BoxIconTypeDefineDict.ExportTypeNames();
        EnemyTypeDefineDict.ExportTypeNames();
        LevelTriggerTypeDefineDict.ExportTypeNames();
        WorldModuleTypeDefineDict.ExportTypeNames();
        WorldTypeDefineDict.ExportTypeNames();
        FXTypeDefineDict.ExportTypeNames();
        SortWorldAndWorldModule();
        ExportWorldDataConfig(dataFormat);
        ExportWorldModuleDataConfig(dataFormat);
        AssetDatabase.Refresh();
        IsLoaded = false;
        LoadAllConfigs();
        EditorUtility.DisplayDialog("提示", "序列化成功", "确定");
    }

    private static void SortWorldAndWorldModule()
    {
        List<string> worldModuleNames = WorldModuleTypeDefineDict.TypeIndexDict.Keys.ToList();
        foreach (string worldModuleName in worldModuleNames)
        {
            string prefabPath = WorldModuleTypeDefineDict.GetTypeAssetDataBasePath(worldModuleName);
            GameObject worldModulePrefab = PrefabUtility.LoadPrefabContents(prefabPath);
            bool isDirty = false;
            if (worldModulePrefab)
            {
                WorldModuleDesignHelper module = worldModulePrefab.GetComponent<WorldModuleDesignHelper>();
                if (module)
                {
                    isDirty = module.SortModule();
                }
            }

            if (isDirty)
            {
                PrefabUtility.SaveAsPrefabAsset(worldModulePrefab, prefabPath);
            }

            PrefabUtility.UnloadPrefabContents(worldModulePrefab);
        }

        List<string> worldNames = WorldTypeDefineDict.TypeIndexDict.Keys.ToList();
        foreach (string worldName in worldNames)
        {
            string prefabPath = WorldTypeDefineDict.GetTypeAssetDataBasePath(worldName);
            GameObject worldPrefab = PrefabUtility.LoadPrefabContents(prefabPath);
            bool isDirty = false;
            if (worldPrefab)
            {
                WorldDesignHelper world = worldPrefab.GetComponent<WorldDesignHelper>();
                if (world)
                {
                    isDirty = world.SortWorld();
                }
            }

            if (isDirty)
            {
                PrefabUtility.SaveAsPrefabAsset(worldPrefab, prefabPath);
            }

            PrefabUtility.UnloadPrefabContents(worldPrefab);
        }
    }

    private static void ExportWorldDataConfig(DataFormat dataFormat)
    {
        string folder = WorldDataConfigFolder_Build;
        if (Directory.Exists(folder)) Directory.Delete(folder, true);
        Directory.CreateDirectory(folder);

        DirectoryInfo di = new DirectoryInfo(Application.dataPath + DesignRoot + WorldDataConfigFolder_Relative);
        foreach (FileInfo fi in di.GetFiles("*.prefab"))
        {
            string relativePath = CommonUtils.ConvertAbsolutePathToProjectPath(fi.FullName);
            GameObject obj = (GameObject) AssetDatabase.LoadAssetAtPath<Object>(relativePath);
            WorldDesignHelper world = obj.GetComponent<WorldDesignHelper>();
            WorldData data = world.ExportWorldData();
            WorldTypeDefineDict.TypeIndexDict.TryGetValue(world.name, out ushort worldTypeIndex);
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

    private static void ExportWorldModuleDataConfig(DataFormat dataFormat)
    {
        string folder = WorldModuleDataConfigFolder_Build;
        if (Directory.Exists(folder)) Directory.Delete(folder, true);
        Directory.CreateDirectory(folder);

        DirectoryInfo di = new DirectoryInfo(Application.dataPath + DesignRoot + WorldModuleDataConfigFolder_Relative);
        foreach (FileInfo fi in di.GetFiles("*.prefab"))
        {
            string relativePath = CommonUtils.ConvertAbsolutePathToProjectPath(fi.FullName);
            GameObject obj = (GameObject) AssetDatabase.LoadAssetAtPath<Object>(relativePath);
            WorldModuleDesignHelper module = obj.GetComponent<WorldModuleDesignHelper>();
            WorldModuleData data = module.ExportWorldModuleData();
            WorldModuleTypeDefineDict.TypeIndexDict.TryGetValue(module.name, out ushort worldModuleTypeIndex);
            if (worldModuleTypeIndex != 0)
            {
                data.WorldModuleTypeIndex = GetWorldModuleTypeIndex(module.name);
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
        Clear();
        DataFormat dataFormat = DataFormat.Binary;
        BoxTypeDefineDict.LoadTypeNames();
        BoxIconTypeDefineDict.LoadTypeNames();
        EnemyTypeDefineDict.LoadTypeNames();
        LevelTriggerTypeDefineDict.LoadTypeNames();
        WorldModuleTypeDefineDict.LoadTypeNames();
        WorldTypeDefineDict.LoadTypeNames();
        FXTypeDefineDict.LoadTypeNames();
        LoadWorldDataConfig(dataFormat);
        LoadWorldModuleDataConfig(dataFormat);
        IsLoaded = true;
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

    #endregion

    #region Getter

    // -------- Get All Type Names --------

    public static List<string> GetAllWorldNames()
    {
        LoadAllConfigs();
        List<string> res = WorldTypeDefineDict.TypeIndexDict.Keys.ToList();
        res.Insert(0, "None");
        return res;
    }

    public static List<string> GetAllWorldModuleNames()
    {
        LoadAllConfigs();
        List<string> res = WorldModuleTypeDefineDict.TypeIndexDict.Keys.ToList();
        res.Insert(0, "None");
        return res;
    }

    public static List<string> GetAllBoxTypeNames()
    {
        LoadAllConfigs();
        List<string> res = BoxTypeDefineDict.TypeIndexDict.Keys.ToList();
        res.Insert(0, "None");
        return res;
    }

    public static List<string> GetAllFXTypeNames()
    {
        LoadAllConfigs();
        List<string> res = FXTypeDefineDict.TypeIndexDict.Keys.ToList();
        res.Insert(0, "None");
        return res;
    }

    public static List<string> GetAllEnemyNames()
    {
        List<string> res = new List<string>();
        res = EnemyTypeDefineDict.TypeIndexDict.Keys.ToList();
        res.Insert(0, "None");
        return res;
    }

    public static List<string> GetAllActorNames()
    {
        LoadAllConfigs();
        List<string> res = new List<string>();
        res = EnemyTypeDefineDict.TypeIndexDict.Keys.ToList();
        res.Insert(0, "Player1");
        res.Insert(0, "None");
        return res;
    }

    public static List<string> GetAllLevelTriggerNames()
    {
        List<string> res = new List<string>();
        res = LevelTriggerTypeDefineDict.TypeIndexDict.Keys.ToList();
        res.Insert(0, "None");
        return res;
    }

    // -------- Get All Type Names --------

    public static string GetBoxTypeName(ushort boxTypeIndex)
    {
        if (!IsLoaded) LoadAllConfigs();
        BoxTypeDefineDict.TypeNameDict.TryGetValue(boxTypeIndex, out string boxTypeName);
        return boxTypeName;
    }

    public static ushort GetBoxTypeIndex(string boxTypeName)
    {
        if (!IsLoaded) LoadAllConfigs();
        if (string.IsNullOrEmpty(boxTypeName))
        {
            Debug.Log(CommonUtils.HighlightStringFormat("无法找到名为{0}的箱子Prefab", "#00A9D1", boxTypeName));
            return 0;
        }

        BoxTypeDefineDict.TypeIndexDict.TryGetValue(boxTypeName, out ushort boxTypeIndex);
        return boxTypeIndex;
    }

    public static string GetBoxIconTypeName(ushort boxIconTypeIndex)
    {
        if (!IsLoaded) LoadAllConfigs();
        BoxIconTypeDefineDict.TypeNameDict.TryGetValue(boxIconTypeIndex, out string boxIconTypeName);
        return boxIconTypeName;
    }

    public static ushort GetBoxIconTypeIndex(string boxIconTypeName)
    {
        if (!IsLoaded) LoadAllConfigs();
        if (string.IsNullOrEmpty(boxIconTypeName))
        {
            Debug.Log(CommonUtils.HighlightStringFormat("无法找到名为{0}的箱子Icon", "#0DD100", boxIconTypeName));
            return 0;
        }

        BoxIconTypeDefineDict.TypeIndexDict.TryGetValue(boxIconTypeName, out ushort boxIconTypeIndex);
        return boxIconTypeIndex;
    }

    public static string GetEnemyTypeName(ushort enemyTypeIndex)
    {
        if (!IsLoaded) LoadAllConfigs();
        EnemyTypeDefineDict.TypeNameDict.TryGetValue(enemyTypeIndex, out string enemyTypeName);
        return enemyTypeName;
    }

    public static ushort GetEnemyTypeIndex(string enemyTypeName)
    {
        if (!IsLoaded) LoadAllConfigs();
        if (string.IsNullOrEmpty(enemyTypeName))
        {
            Debug.Log(CommonUtils.HighlightStringFormat("无法找到名为{0}的敌兵Prefab", "#8600D1", enemyTypeName));
            return 0;
        }

        EnemyTypeDefineDict.TypeIndexDict.TryGetValue(enemyTypeName, out ushort enemyTypeIndex);
        return enemyTypeIndex;
    }

    public static string GetLevelTriggerTypeName(ushort levelTriggerTypeIndex)
    {
        if (!IsLoaded) LoadAllConfigs();
        LevelTriggerTypeDefineDict.TypeNameDict.TryGetValue(levelTriggerTypeIndex, out string levelTriggerTypeName);
        return levelTriggerTypeName;
    }

    public static ushort GetLevelTriggerTypeIndex(string levelTriggerTypeName)
    {
        if (!IsLoaded) LoadAllConfigs();
        if (string.IsNullOrEmpty(levelTriggerTypeName))
        {
            Debug.Log(CommonUtils.HighlightStringFormat("无法找到名为{0}的LevelTrigger Prefab", "#6DF707", levelTriggerTypeName));
            return 0;
        }

        LevelTriggerTypeDefineDict.TypeIndexDict.TryGetValue(levelTriggerTypeName, out ushort levelTriggerTypeIndex);
        return levelTriggerTypeIndex;
    }

    public static string GetWorldModuleName(ushort worldModuleTypeIndex)
    {
        if (!IsLoaded) LoadAllConfigs();
        WorldModuleTypeDefineDict.TypeNameDict.TryGetValue(worldModuleTypeIndex, out string worldModuleTypeName);
        return worldModuleTypeName;
    }

    public static ushort GetWorldModuleTypeIndex(string worldModuleTypeName)
    {
        if (!IsLoaded) LoadAllConfigs();
        if (string.IsNullOrEmpty(worldModuleTypeName))
        {
            Debug.Log(CommonUtils.HighlightStringFormat("无法找到名为{0}的世界模组Prefab", "#D100BC", worldModuleTypeName));
            return 0;
        }

        WorldModuleTypeDefineDict.TypeIndexDict.TryGetValue(worldModuleTypeName, out ushort worldModuleTypeIndex);
        return worldModuleTypeIndex;
    }

    public static string GetWorldName(ushort worldTypeIndex)
    {
        if (!IsLoaded) LoadAllConfigs();
        WorldTypeDefineDict.TypeNameDict.TryGetValue(worldTypeIndex, out string worldTypeName);
        return worldTypeName;
    }

    public static ushort GetWorldTypeIndex(string worldTypeName)
    {
        if (!IsLoaded) LoadAllConfigs();
        if (string.IsNullOrEmpty(worldTypeName))
        {
            Debug.Log(CommonUtils.HighlightStringFormat("无法找到名为{0}的世界Prefab", "#009CD1", worldTypeName));
            return 0;
        }

        WorldTypeDefineDict.TypeIndexDict.TryGetValue(worldTypeName, out ushort worldTypeIndex);
        return worldTypeIndex;
    }

    public static string GetFXName(ushort fxTypeIndex)
    {
        if (!IsLoaded) LoadAllConfigs();
        FXTypeDefineDict.TypeNameDict.TryGetValue(fxTypeIndex, out string fxTypeName);
        return fxTypeName;
    }

    public static ushort GetFXTypeIndex(string fxTypeName)
    {
        if (!IsLoaded) LoadAllConfigs();
        if (string.IsNullOrEmpty(fxTypeName))
        {
            Debug.Log(CommonUtils.HighlightStringFormat("无法找到名为{0}的FX Prefab", "#D1004D", fxTypeName));
            return 0;
        }

        FXTypeDefineDict.TypeIndexDict.TryGetValue(fxTypeName, out ushort fxTypeIndex);
        return fxTypeIndex;
    }

    public static WorldData GetWorldDataConfig(ushort worldTypeIndex)
    {
        if (!IsLoaded) LoadAllConfigs();
        WorldDataConfigDict.TryGetValue(worldTypeIndex, out WorldData worldData);
        return worldData?.Clone();
    }

    public static WorldModuleData GetWorldModuleDataConfig(ushort worldModuleTypeIndex)
    {
        if (!IsLoaded) LoadAllConfigs();
        WorldModuleDataConfigDict.TryGetValue(worldModuleTypeIndex, out WorldModuleData worldModuleData);
        return worldModuleData?.Clone();
    }

    #endregion

    #region Prefabs

    public static GameObject FindBoxPrefabByName(string boxName)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ConfigManager.BoxTypeDefineDict.GetTypeAssetDataBasePath(boxName));
        return prefab;
    }

    public static bool DeleteBoxPrefabByName(string boxName)
    {
        return AssetDatabase.DeleteAsset(ConfigManager.BoxTypeDefineDict.GetTypeAssetDataBasePath(boxName));
    }

    public static string RenameBoxPrefabByName(string boxName, string targetBoxName)
    {
        return AssetDatabase.RenameAsset(ConfigManager.BoxTypeDefineDict.GetTypeAssetDataBasePath(boxName), targetBoxName);
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
            GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyTypeDefineDict.GetTypeAssetDataBasePath(actorName));
            return enemyPrefab;
        }
    }

    public static GameObject FindLevelTriggerPrefabByName(string levelTriggerName)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(LevelTriggerTypeDefineDict.GetTypeAssetDataBasePath(levelTriggerName));
        return prefab;
    }

    public static GameObject FindWorldModulePrefabByName(string worldModuleName)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(WorldModuleTypeDefineDict.GetTypeAssetDataBasePath(worldModuleName));
        return prefab;
    }

    public static GameObject FindWorldPrefabByName(string worldName)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(WorldTypeDefineDict.GetTypeAssetDataBasePath(worldName));
        return prefab;
    }

    #endregion
}
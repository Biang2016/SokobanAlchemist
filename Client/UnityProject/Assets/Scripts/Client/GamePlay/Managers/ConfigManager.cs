using System.Collections.Generic;
using System.IO;
using BiangStudio;
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

    public class TypeDefineConfig<T> where T : MonoBehaviour
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

#if UNITY_EDITOR
        public void ExportTypeNames()
        {
            TypeNameDict.Clear();
            TypeIndexDict.Clear();
            string folder = TypeNamesConfigFolder_Build;
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
            Directory.CreateDirectory(folder);

            ushort index = 1;
            DirectoryInfo di = new DirectoryInfo(Application.dataPath + PrefabFolder_Relative);
            foreach (FileInfo fi in di.GetFiles("*.prefab", IncludeSubFolder ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            {
                if (index == ushort.MaxValue)
                {
                    Debug.LogError($"{typeof(T).Name}类型数量超过255");
                    break;
                }

                string relativePath = CommonUtils.ConvertAbsolutePathToProjectPath(fi.FullName);
                GameObject obj = (GameObject) AssetDatabase.LoadAssetAtPath<Object>(relativePath);
                T t = obj.GetComponent<T>();
                if (t != null)
                {
                    TypeNameDict.Add(index, t.name);
                    TypeIndexDict.Add(t.name, index);
                    index++;
                }
                else
                {
                    Debug.LogError($"Prefab {fi.Name} 不含{typeof(T).Name}脚本，已跳过");
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
    [LabelText("敌人类型表")]
    public static readonly TypeDefineConfig<EnemyActor> EnemyTypeDefineDict = new TypeDefineConfig<EnemyActor>("Enemy", "/Resources/Prefabs/Designs/Enemy", true);

    [ShowInInspector]
    [LabelText("世界模组类型表")]
    public static readonly TypeDefineConfig<WorldModuleDesignHelper> WorldModuleTypeDefineDict = new TypeDefineConfig<WorldModuleDesignHelper>("WorldModule", "/Designs/WorldModule", true);

    [ShowInInspector]
    [LabelText("FX类型表")]
    public static readonly TypeDefineConfig<FX> FXTypeDefineDict = new TypeDefineConfig<FX>("FX", "/Resources/Prefabs/FX", true);

    [ShowInInspector]
    [LabelText("世界配置表")]
    public static readonly Dictionary<string, WorldData> WorldDataConfigDict = new Dictionary<string, WorldData>();

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

    #region Export

#if UNITY_EDITOR
    [MenuItem("开发工具/配置/序列化配置")]
    public static void ExportConfigs()
    {
        // http://www.sirenix.net/odininspector/faq?Search=&t-11=on#faq

        DataFormat dataFormat = DataFormat.Binary;
        BoxTypeDefineDict.ExportTypeNames();
        EnemyTypeDefineDict.ExportTypeNames();
        WorldModuleTypeDefineDict.ExportTypeNames();
        FXTypeDefineDict.ExportTypeNames();
        ExportWorldDataConfig(dataFormat);
        ExportWorldModuleDataConfig(dataFormat);
        AssetDatabase.Refresh();
        LoadAllConfigs();
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
            WorldDesignHelper module = obj.GetComponent<WorldDesignHelper>();
            WorldData data = module.ExportWorldData();
            string path = folder + module.name + ".config";
            byte[] bytes = SerializationUtility.SerializeValue(data, dataFormat);
            File.WriteAllBytes(path, bytes);
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
            data.WorldModuleTypeIndex = GetWorldModuleTypeIndex(module.name);
            data.WorldModuleTypeName = module.name;
            string path = folder + module.name + ".config";
            byte[] bytes = SerializationUtility.SerializeValue(data, dataFormat);
            File.WriteAllBytes(path, bytes);
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
        DataFormat dataFormat = DataFormat.Binary;
        BoxTypeDefineDict.LoadTypeNames();
        EnemyTypeDefineDict.LoadTypeNames();
        WorldModuleTypeDefineDict.LoadTypeNames();
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
                if (WorldDataConfigDict.ContainsKey(data.WorldName))
                {
                    Debug.LogError($"世界重名:{data.WorldName}");
                }
                else
                {
                    WorldDataConfigDict.Add(data.WorldName, data);
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

    public static string GetBoxTypeName(ushort boxTypeIndex)
    {
        if (!IsLoaded) LoadAllConfigs();
        BoxTypeDefineDict.TypeNameDict.TryGetValue(boxTypeIndex, out string boxTypeName);
        return boxTypeName;
    }

    public static ushort GetBoxTypeIndex(string boxTypeName)
    {
        if (!IsLoaded) LoadAllConfigs();
        BoxTypeDefineDict.TypeIndexDict.TryGetValue(boxTypeName, out ushort boxTypeIndex);
        return boxTypeIndex;
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
        EnemyTypeDefineDict.TypeIndexDict.TryGetValue(enemyTypeName, out ushort enemyTypeIndex);
        return enemyTypeIndex;
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
        WorldModuleTypeDefineDict.TypeIndexDict.TryGetValue(worldModuleTypeName, out ushort worldModuleTypeIndex);
        return worldModuleTypeIndex;
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
        FXTypeDefineDict.TypeIndexDict.TryGetValue(fxTypeName, out ushort fxTypeIndex);
        return fxTypeIndex;
    }

    public static WorldData GetWorldDataConfig(string worldType)
    {
        if (!IsLoaded) LoadAllConfigs();
        WorldDataConfigDict.TryGetValue(worldType, out WorldData worldData);
        return worldData?.Clone();
    }

    public static WorldModuleData GetWorldModuleDataConfig(ushort worldModuleTypeIndex)
    {
        if (!IsLoaded) LoadAllConfigs();
        WorldModuleDataConfigDict.TryGetValue(worldModuleTypeIndex, out WorldModuleData worldModuleData);
        return worldModuleData?.Clone();
    }

    #endregion
}
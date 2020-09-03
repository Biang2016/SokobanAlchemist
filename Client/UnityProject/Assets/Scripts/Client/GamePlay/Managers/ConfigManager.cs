using System.Collections.Generic;
using System.IO;
using BiangStudio;
using BiangStudio.Singleton;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

public class ConfigManager : TSingletonBaseManager<ConfigManager>
{
    public static float BoxStaticBounceFactor_Cheat = 1f;
    public static float BoxDynamicBounceFactor_Cheat = 1f;
    public static float BoxThrowDragFactor_Cheat = 10f;
    public static float BoxKickDragFactor_Cheat = 1f;
    public static float BoxWeightFactor_Cheat = 1f;

    public static string BoxTypeNamesConfig_File => $"{BoxTypeNamesConfigFolder_Build}/BoxNames.config";
    public static string BoxTypeNamesConfigFolder_Build = Application.streamingAssetsPath + "/Configs/Boxes";
    public static string BoxPrefabFolder_Relative = "/Resources/Prefabs/Designs/Boxes";
    public static Dictionary<string, byte> BoxTypeIndexDict = new Dictionary<string, byte>();

    [ShowInInspector]
    [LabelText("箱子类型表")]
    public static readonly SortedDictionary<byte, string> BoxTypeNameDict = new SortedDictionary<byte, string>();

    public static string WorldModuleTypeNamesConfig_File => $"{WorldModuleTypeNamesConfigFolder_Build}/WorldModules.config";
    public static string WorldModuleTypeNamesConfigFolder_Build = Application.streamingAssetsPath + "/Configs/WorldModules";
    public static Dictionary<string, byte> WorldModuleTypeIndexDict = new Dictionary<string, byte>();

    [ShowInInspector]
    [LabelText("世界模组类型表")]
    public static readonly SortedDictionary<byte, string> WorldModuleTypeNameDict = new SortedDictionary<byte, string>();

    [ShowInInspector]
    [LabelText("世界配置表")]
    public static readonly Dictionary<string, WorldData> WorldDataConfigDict = new Dictionary<string, WorldData>();

    [ShowInInspector]
    [LabelText("世界模组配置表")]
    public static readonly SortedDictionary<byte, WorldModuleData> WorldModuleDataConfigDict = new SortedDictionary<byte, WorldModuleData>();

    public static string DesignRoot = "/Designs/";

    public static string WorldDataConfigFolder_Relative = "Worlds";
    public static string WorldModuleDataConfigFolder_Relative = "WorldModules";

    public static string WorldDataConfigFolder_Build = Application.streamingAssetsPath + "/Configs/" + WorldDataConfigFolder_Relative + "/";
    public static string WorldModuleDataConfigFolder_Build = Application.streamingAssetsPath + "/Configs/" + WorldModuleDataConfigFolder_Relative + "/";

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
        ExportBoxTypeNames();
        ExportWorldModuleTypeNames();
        ExportWorldDataConfig(dataFormat);
        ExportWorldModuleDataConfig(dataFormat);
        AssetDatabase.Refresh();
    }

    private static void ExportBoxTypeNames()
    {
        BoxTypeNameDict.Clear();
        BoxTypeIndexDict.Clear();
        string folder = BoxTypeNamesConfigFolder_Build;
        if (Directory.Exists(folder)) Directory.Delete(folder, true);
        Directory.CreateDirectory(folder);

        byte index = 1;
        DirectoryInfo di = new DirectoryInfo(Application.dataPath + BoxPrefabFolder_Relative);
        foreach (FileInfo fi in di.GetFiles("*.prefab"))
        {
            if (index == byte.MaxValue)
            {
                Debug.LogError($"箱子类型数量超过255");
                break;
            }

            string relativePath = CommonUtils.ConvertAbsolutePathToProjectPath(fi.FullName);
            GameObject obj = (GameObject) AssetDatabase.LoadAssetAtPath<Object>(relativePath);
            Box box = obj.GetComponent<Box>();
            if (box != null)
            {
                BoxTypeNameDict.Add(index, box.name);
                BoxTypeIndexDict.Add(box.name, index);
                index++;
            }
            else
            {
                Debug.LogError($"Prefab {fi.Name} 不含{typeof(Box).Name}脚本，已跳过");
            }
        }

        string json = JsonConvert.SerializeObject(BoxTypeIndexDict, Formatting.Indented);
        StreamWriter sw = new StreamWriter(BoxTypeNamesConfig_File);
        sw.Write(json);
        sw.Close();
    }

    private static void ExportWorldModuleTypeNames()
    {
        WorldModuleTypeNameDict.Clear();
        WorldModuleTypeIndexDict.Clear();
        string folder = WorldModuleTypeNamesConfigFolder_Build;
        if (Directory.Exists(folder)) Directory.Delete(folder, true);
        Directory.CreateDirectory(folder);

        byte index = 1;
        DirectoryInfo di = new DirectoryInfo(Application.dataPath + DesignRoot + WorldModuleDataConfigFolder_Relative);
        foreach (FileInfo fi in di.GetFiles("*.prefab"))
        {
            if (index == byte.MaxValue)
            {
                Debug.LogError($"世界模组类型数量超过255");
                break;
            }

            string relativePath = CommonUtils.ConvertAbsolutePathToProjectPath(fi.FullName);
            GameObject obj = (GameObject) AssetDatabase.LoadAssetAtPath<Object>(relativePath);
            WorldModuleDesignHelper module = obj.GetComponent<WorldModuleDesignHelper>();
            if (module != null)
            {
                WorldModuleTypeNameDict.Add(index, module.name);
                WorldModuleTypeIndexDict.Add(module.name, index);
                index++;
            }
            else
            {
                Debug.LogError($"Prefab {fi.Name} 不含{typeof(WorldModuleDesignHelper).Name}脚本，已跳过");
            }
        }

        string json = JsonConvert.SerializeObject(WorldModuleTypeIndexDict, Formatting.Indented);
        StreamWriter sw = new StreamWriter(WorldModuleTypeNamesConfig_File);
        sw.Write(json);
        sw.Close();
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
        LoadBoxTypeNames();
        LoadWorldModuleTypeNames();
        LoadWorldDataConfig(dataFormat);
        LoadWorldModuleDataConfig(dataFormat);
        IsLoaded = true;
    }

    private static void LoadBoxTypeNames()
    {
        FileInfo fi = new FileInfo(BoxTypeNamesConfig_File);
        if (fi.Exists)
        {
            StreamReader sr = new StreamReader(BoxTypeNamesConfig_File);
            string content = sr.ReadToEnd();
            sr.Close();
            BoxTypeIndexDict.Clear();
            BoxTypeNameDict.Clear();
            BoxTypeIndexDict = JsonConvert.DeserializeObject<Dictionary<string, byte>>(content);
            foreach (KeyValuePair<string, byte> kv in BoxTypeIndexDict)
            {
                BoxTypeNameDict.Add(kv.Value, kv.Key);
            }
        }
    }

    private static void LoadWorldModuleTypeNames()
    {
        FileInfo fi = new FileInfo(WorldModuleTypeNamesConfig_File);
        if (fi.Exists)
        {
            StreamReader sr = new StreamReader(WorldModuleTypeNamesConfig_File);
            string content = sr.ReadToEnd();
            sr.Close();
            WorldModuleTypeIndexDict.Clear();
            WorldModuleTypeNameDict.Clear();
            WorldModuleTypeIndexDict = JsonConvert.DeserializeObject<Dictionary<string, byte>>(content);
            foreach (KeyValuePair<string, byte> kv in WorldModuleTypeIndexDict)
            {
                WorldModuleTypeNameDict.Add(kv.Value, kv.Key);
            }
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

    public static string GetBoxTypeName(byte boxTypeIndex)
    {
        if (!IsLoaded) LoadAllConfigs();
        BoxTypeNameDict.TryGetValue(boxTypeIndex, out string boxTypeName);
        return boxTypeName;
    }

    public static byte GetBoxTypeIndex(string boxTypeName)
    {
        if (!IsLoaded) LoadAllConfigs();
        BoxTypeIndexDict.TryGetValue(boxTypeName, out byte boxTypeIndex);
        return boxTypeIndex;
    }

    public static string GetWorldModuleName(byte worldModuleTypeIndex)
    {
        if (!IsLoaded) LoadAllConfigs();
        WorldModuleTypeNameDict.TryGetValue(worldModuleTypeIndex, out string worldModuleTypeName);
        return worldModuleTypeName;
    }

    public static byte GetWorldModuleTypeIndex(string worldModuleTypeName)
    {
        if (!IsLoaded) LoadAllConfigs();
        WorldModuleTypeIndexDict.TryGetValue(worldModuleTypeName, out byte worldModuleTypeIndex);
        return worldModuleTypeIndex;
    }

    public static WorldData GetWorldDataConfig(string worldType)
    {
        if (!IsLoaded) LoadAllConfigs();
        WorldDataConfigDict.TryGetValue(worldType, out WorldData worldData);
        return worldData.Clone();
    }

    public static WorldModuleData GetWorldModuleDataConfig(byte worldModuleTypeIndex)
    {
        if (!IsLoaded) LoadAllConfigs();
        WorldModuleDataConfigDict.TryGetValue(worldModuleTypeIndex, out WorldModuleData worldModuleData);
        return worldModuleData.Clone();
    }

    #endregion
}
using System.Collections.Generic;
using System.IO;
using BiangStudio;
using BiangStudio.Singleton;
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

    [ShowInInspector]
    [LabelText("世界配置表")]
    public static readonly Dictionary<WorldType, WorldData> WorldDataConfigDict = new Dictionary<WorldType, WorldData>();

    [ShowInInspector]
    [LabelText("世界模组配置表")]
    public static readonly Dictionary<WorldModuleType, WorldModuleData> WorldModuleDataConfigDict = new Dictionary<WorldModuleType, WorldModuleData>();

    public static string DesignRoot = "/Designs/";

    public static string WorldDataConfigFolder_Relative = "Worlds";
    public static string WorldModuleDataConfigFolder_Relative = "WorldModules";

    public static string WorldDataConfigFolder_Build = Application.streamingAssetsPath + "/" + WorldDataConfigFolder_Relative + "/";
    public static string WorldModuleDataConfigFolder_Build = Application.streamingAssetsPath + "/" + WorldModuleDataConfigFolder_Relative + "/";

    public override void Awake()
    {
    }

    #region Export

#if UNITY_EDITOR
    [MenuItem("开发工具/配置/序列化配置")]
    public static void ExportConfigs()
    {
        // http://www.sirenix.net/odininspector/faq?Search=&t-11=on#faq

        DataFormat dataFormat = DataFormat.Binary;
        ExportWorldDataConfig(dataFormat);
        ExportWorldModuleDataConfig(dataFormat);
        AssetDatabase.Refresh();
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
            string path = folder + module.WorldType + ".config";
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
            string path = folder + module.WorldModuleType + ".config";
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
        DataFormat dataFormat = DataFormat.Binary;
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
                if (WorldDataConfigDict.ContainsKey(data.WorldType))
                {
                    Debug.LogError($"世界重名:{data.WorldType}");
                }
                else
                {
                    WorldDataConfigDict.Add(data.WorldType, data);
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
                if (WorldModuleDataConfigDict.ContainsKey(data.WorldModuleType))
                {
                    Debug.LogError($"世界模组重名:{data.WorldModuleType}");
                }
                else
                {
                    WorldModuleDataConfigDict.Add(data.WorldModuleType, data);
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

    public WorldData GetWorldDataConfig(WorldType worldType)
    {
        if (!IsLoaded) LoadAllConfigs();
        WorldDataConfigDict.TryGetValue(worldType, out WorldData worldData);
        return worldData.Clone();
    }

    public WorldModuleData GetWorldModuleDataConfig(WorldModuleType worldModuleType)
    {
        if (!IsLoaded) LoadAllConfigs();
        WorldModuleDataConfigDict.TryGetValue(worldModuleType, out WorldModuleData worldModuleData);
        return worldModuleData.Clone();
    }

    #endregion
}
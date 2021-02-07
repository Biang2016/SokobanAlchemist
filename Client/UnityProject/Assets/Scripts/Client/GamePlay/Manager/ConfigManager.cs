using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BiangLibrary;
using BiangLibrary.GamePlay;
using BiangLibrary.Singleton;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
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
        Entity = 1000,
        Buff = 10000,
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
        private string TypeNamesConfigFolder_Build => ConfigFolder_Build + "/TypeNames";
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
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

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
    [LabelText("BattleIndicator类型表")]
    public static readonly TypeDefineConfig<BattleIndicator> BattleIndicatorTypeDefineDict = new TypeDefineConfig<BattleIndicator>("BattleIndicator", "/Resources/Prefabs/BattleIndicator", true);

    [ShowInInspector]
    [LabelText("箱子占位配置表")]
    public static readonly Dictionary<ushort, BoxOccupationData> BoxOccupationConfigDict = new Dictionary<ushort, BoxOccupationData>();

    [ShowInInspector]
    [LabelText("世界配置表")]
    public static readonly Dictionary<ushort, WorldData> WorldDataConfigDict = new Dictionary<ushort, WorldData>();

    [ShowInInspector]
    [LabelText("世界模组配置表")]
    public static readonly SortedDictionary<ushort, WorldModuleData> WorldModuleDataConfigDict = new SortedDictionary<ushort, WorldModuleData>();

    public static string DesignRoot = "/Designs/";
    public static string ResourcesPrefabDesignRoot = "/Resources/Prefabs/Designs/";

    public static string EntityBuffAttributeMatrixAssetPath_Relative = "Buff/EntityBuffAttributeMatrixAsset.asset";
    public static string EntityBuffAttributeMatrixConfigFolder_Relative = "EntityBuffAttributeMatrix";
    public static string BoxOccupationConfigDictFolder_Relative = "Box";
    public static string WorldDataConfigFolder_Relative = "Worlds";
    public static string WorldModuleDataConfigFolder_Relative = "WorldModule";

    public static string ConfigFolder_Build = Application.streamingAssetsPath + "/Configs/";
    public static string EntityBuffAttributeMatrixConfigFolder_Build = ConfigFolder_Build + EntityBuffAttributeMatrixConfigFolder_Relative + "/";
    public static string BoxOccupationConfigDictFolder_Build = ConfigFolder_Build + BoxOccupationConfigDictFolder_Relative + "/";
    public static string WorldDataConfigFolder_Build = ConfigFolder_Build + WorldDataConfigFolder_Relative + "/";
    public static string WorldModuleDataConfigFolder_Build = ConfigFolder_Build + WorldModuleDataConfigFolder_Relative + "s/";

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
        BattleIndicatorTypeDefineDict.Clear();
        BoxOccupationConfigDict.Clear();
        WorldDataConfigDict.Clear();
        WorldModuleDataConfigDict.Clear();
    }

    #region Export

#if UNITY_EDITOR
    [MenuItem("开发工具/配置/序列化配置")]
    public static void ExportConfigs()
    {
        ExportConfigs(true);
    }

    public static void ExportConfigs(bool dialogShow = true)
    {
        // http://www.sirenix.net/odininspector/faq?Search=&t-11=on#faq
        DataFormat dataFormat = DataFormat.Binary;

        if (Directory.Exists(ConfigFolder_Build)) Directory.Delete(ConfigFolder_Build, true);
        Directory.CreateDirectory(ConfigFolder_Build);

        // 时序，先导出类型表
        BoxTypeDefineDict.ExportTypeNames();
        BoxIconTypeDefineDict.ExportTypeNames();
        EnemyTypeDefineDict.ExportTypeNames();
        LevelTriggerTypeDefineDict.ExportTypeNames();
        WorldModuleTypeDefineDict.ExportTypeNames();
        WorldTypeDefineDict.ExportTypeNames();
        FXTypeDefineDict.ExportTypeNames();
        BattleIndicatorTypeDefineDict.ExportTypeNames();

        SortWorldAndWorldModule();
        ExportEntityBuffAttributeMatrix(dataFormat);
        ExportBoxOccupationDataConfig(dataFormat);
        ExportWorldDataConfig(dataFormat);
        ExportWorldModuleDataConfig(dataFormat);

        AssetDatabase.Refresh();
        IsLoaded = false;
        LoadAllConfigs();
        if (dialogShow) EditorUtility.DisplayDialog("提示", "序列化成功", "确定");
    }

    private static void SortWorldAndWorldModule()
    {
        List<string> worldModuleNames = WorldModuleTypeDefineDict.TypeIndexDict.Keys.ToList();
        foreach (string worldModuleName in worldModuleNames)
        {
            string prefabPath = WorldModuleTypeDefineDict.GetTypeAssetDataBasePath(worldModuleName);
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

        List<string> worldNames = WorldTypeDefineDict.TypeIndexDict.Keys.ToList();
        foreach (string worldName in worldNames)
        {
            string prefabPath = WorldTypeDefineDict.GetTypeAssetDataBasePath(worldName);
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

    private static void ExportBoxOccupationDataConfig(DataFormat dataFormat)
    {
        BoxOccupationConfigDict.Clear();
        string folder = BoxOccupationConfigDictFolder_Build;
        string file = folder + "BoxOccupation.config";
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        if (File.Exists(file)) File.Delete(file);

        List<string> boxNames = BoxTypeDefineDict.TypeIndexDict.Keys.ToList();
        foreach (string boxName in boxNames)
        {
            string prefabPath = BoxTypeDefineDict.GetTypeAssetDataBasePath(boxName);
            GameObject boxPrefab = PrefabUtility.LoadPrefabContents(prefabPath);
            if (boxPrefab)
            {
                Box box = boxPrefab.GetComponent<Box>();
                if (box)
                {
                    box.BoxIndicatorHelper.RefreshBoxIndicatorOccupationData();
                    BoxOccupationData occupationData = box.GetBoxOccupationGPs_Editor().Clone();
                    ushort boxTypeIndex = BoxTypeDefineDict.TypeIndexDict[box.name];
                    if (boxTypeIndex != 0)
                    {
                        BoxOccupationConfigDict.Add(boxTypeIndex, occupationData);
                    }

                    PrefabUtility.SaveAsPrefabAsset(boxPrefab, prefabPath);
                }
            }

            PrefabUtility.UnloadPrefabContents(boxPrefab);
        }

        byte[] bytes = SerializationUtility.SerializeValue(BoxOccupationConfigDict, dataFormat);
        File.WriteAllBytes(file, bytes);
    }

    private static void ExportWorldDataConfig(DataFormat dataFormat)
    {
        string folder = WorldDataConfigFolder_Build;
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

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
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

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

        AllBuffAttributeTypes = Enum.GetValues(typeof(EntityBuffAttribute));

        BoxTypeDefineDict.LoadTypeNames();
        BoxIconTypeDefineDict.LoadTypeNames();
        EnemyTypeDefineDict.LoadTypeNames();
        LevelTriggerTypeDefineDict.LoadTypeNames();
        WorldModuleTypeDefineDict.LoadTypeNames();
        WorldTypeDefineDict.LoadTypeNames();
        FXTypeDefineDict.LoadTypeNames();
        BattleIndicatorTypeDefineDict.LoadTypeNames();

        LoadEntityBuffStatPropertyEnumReflection();
        LoadEntityBuffAttributeMatrix(dataFormat);
        LoadBoxOccupationDataConfig(dataFormat);
        LoadWorldDataConfig(dataFormat);
        LoadWorldModuleDataConfig(dataFormat);

        IsLoaded = true;
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

    public static void LoadEntityBuffAttributeMatrix(DataFormat dataFormat)
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

#if UNITY_EDITOR
            EntityBuffAttributeMatrixAsset configSSO = GetBuffAttributeMatrixAsset();
            configSSO.EntityBuffAttributeMatrix = EntityBuffAttributeMatrix;
            AssetDatabase.Refresh();
        }
        else
        {
            ExportEntityBuffAttributeMatrix(dataFormat);
            LoadEntityBuffAttributeMatrix(dataFormat);
#endif
        }
    }

    private static void LoadBoxOccupationDataConfig(DataFormat dataFormat)
    {
        BoxOccupationConfigDict.Clear();

        DirectoryInfo di = new DirectoryInfo(BoxOccupationConfigDictFolder_Build);
        if (di.Exists)
        {
            foreach (FileInfo fi in di.GetFiles("*.config", SearchOption.AllDirectories))
            {
                byte[] bytes = File.ReadAllBytes(fi.FullName);
                Dictionary<ushort, BoxOccupationData> data = SerializationUtility.DeserializeValue<Dictionary<ushort, BoxOccupationData>>(bytes, dataFormat);
                foreach (KeyValuePair<ushort, BoxOccupationData> kv in data)
                {
                    kv.Value.CalculateEveryOrientationOccupationGPs();
                    BoxOccupationConfigDict.Add(kv.Key, kv.Value);
                }
            }
        }
        else
        {
            Debug.LogError("箱子占位配置表不存在");
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

    #region Special types const

    public static IEnumerable AllBuffAttributeTypes;

    public static ushort WorldModule_DeadZoneIndex => GetWorldModuleTypeIndex("Common_DeadZone");
    public static ushort WorldModule_HiddenWallIndex => GetWorldModuleTypeIndex("Common_Wall_Hidden");
    public static ushort WorldModule_OpenWorldModule => GetWorldModuleTypeIndex("OpenWorldModule");
    public static ushort WorldModule_GroundIndex => GetWorldModuleTypeIndex("Common_Ground");
    public static ushort Box_EnemyFrozenBoxIndex => GetBoxTypeIndex("EnemyFrozenBox");
    public static ushort Box_GroundBoxIndex => GetBoxTypeIndex("GroundBox");

    #endregion

    #region Getter

    public static IEnumerable GetAllBuffAttributeTypes()
    {
        return AllBuffAttributeTypes;
    }

    // -------- Get All Type Names Starts--------

    public static List<string> GetAllWorldNames(bool withNone = true)
    {
        LoadAllConfigs();
        List<string> res = WorldTypeDefineDict.TypeIndexDict.Keys.ToList();
        if (withNone) res.Insert(0, "None");
        return res;
    }

    public static List<string> GetAllWorldModuleNames(bool withNone = true)
    {
        LoadAllConfigs();
        List<string> res = WorldModuleTypeDefineDict.TypeIndexDict.Keys.ToList();
        if (withNone) res.Insert(0, "None");
        return res;
    }

    public static List<string> GetAllBoxTypeNames(bool withNone = true)
    {
        LoadAllConfigs();
        List<string> res = BoxTypeDefineDict.TypeIndexDict.Keys.ToList();
        if (withNone) res.Insert(0, "None");
        return res;
    }

    public static List<string> GetAllBoxIconTypeNames(bool withNone = true)
    {
        LoadAllConfigs();
        List<string> res = BoxIconTypeDefineDict.TypeIndexDict.Keys.ToList();
        if (withNone) res.Insert(0, "None");
        return res;
    }

    public static List<string> GetAllFXTypeNames(bool withNone = true)
    {
        LoadAllConfigs();
        List<string> res = FXTypeDefineDict.TypeIndexDict.Keys.ToList();
        if (withNone) res.Insert(0, "None");
        return res;
    }

    public static List<string> GetAllBattleIndicatorTypeNames(bool withNone = true)
    {
        LoadAllConfigs();
        List<string> res = BattleIndicatorTypeDefineDict.TypeIndexDict.Keys.ToList();
        if (withNone) res.Insert(0, "None");
        return res;
    }

    public static List<string> GetAllEnemyNames(bool withNone = true)
    {
        List<string> res = new List<string>();
        res = EnemyTypeDefineDict.TypeIndexDict.Keys.ToList();
        if (withNone) res.Insert(0, "None");
        return res;
    }

    public static List<string> GetAllActorNames(bool withNone = true)
    {
        LoadAllConfigs();
        List<string> res = new List<string>();
        res = EnemyTypeDefineDict.TypeIndexDict.Keys.ToList();
        res.Insert(0, "Player1");
        if (withNone) res.Insert(0, "None");
        return res;
    }

    public static List<string> GetAllLevelTriggerNames(bool withNone = true)
    {
        List<string> res = new List<string>();
        res = LevelTriggerTypeDefineDict.TypeIndexDict.Keys.ToList();
        if (withNone) res.Insert(0, "None");
        return res;
    }

    // -------- Get All Type Names Ends --------

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

    public static string GetBattleIndicatorName(ushort battleIndicatorTypeIndex)
    {
        if (!IsLoaded) LoadAllConfigs();
        BattleIndicatorTypeDefineDict.TypeNameDict.TryGetValue(battleIndicatorTypeIndex, out string battleIndicatorTypeName);
        return battleIndicatorTypeName;
    }

    public static ushort GetBattleIndicatorTypeIndex(string battleIndicatorTypeName)
    {
        if (!IsLoaded) LoadAllConfigs();
        if (string.IsNullOrEmpty(battleIndicatorTypeName))
        {
            Debug.Log(CommonUtils.HighlightStringFormat("无法找到名为{0}的BattleIndicator Prefab", "#D1004D", battleIndicatorTypeName));
            return 0;
        }

        BattleIndicatorTypeDefineDict.TypeIndexDict.TryGetValue(battleIndicatorTypeName, out ushort battleIndicatorTypeIndex);
        return battleIndicatorTypeIndex;
    }

    public static BoxOccupationData GetBoxOccupationData(ushort boxTypeIndex)
    {
        if (!IsLoaded) LoadAllConfigs();
        BoxOccupationConfigDict.TryGetValue(boxTypeIndex, out BoxOccupationData occupationData);
        return occupationData;
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

#if UNITY_EDITOR
    public static GameObject FindBoxPrefabByName(string boxName)
    {
        BoxTypeDefineDict.ExportTypeNames(); // todo 判断是否要删掉此行
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BoxTypeDefineDict.GetTypeAssetDataBasePath(boxName));
        return prefab;
    }

    public static GameObject FindBoxLevelEditorPrefabByName(string boxName)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FindBoxLevelEditorPrefabPathByName(boxName));
        return prefab;
    }

    public static string FindBoxLevelEditorPrefabPathByName(string boxName)
    {
        BoxTypeDefineDict.ExportTypeNames(); // todo 判断是否要删掉此行
        string boxPrefabPath = BoxTypeDefineDict.GetTypeAssetDataBasePath(boxName);
        return boxPrefabPath.Replace("/Box/", "/Box_LevelEditor/").Replace(boxName, boxName + "_LevelEditor");
    }

    public static bool DeleteBoxPrefabByName(string boxName)
    {
        BoxTypeDefineDict.ExportTypeNames(); // todo 判断是否要删掉此行
        string boxPrefabPath = BoxTypeDefineDict.GetTypeAssetDataBasePath(boxName);
        string boxLevelEditorPrefabPath = boxPrefabPath.Replace("/Box/", "/Box_LevelEditor/").Replace(boxName, boxName + "_LevelEditor");
        bool res_1 = AssetDatabase.DeleteAsset(boxPrefabPath);
        bool res_2 = AssetDatabase.DeleteAsset(boxLevelEditorPrefabPath);
        return res_1; // 源Box的Prefab删除就算成功
    }

    public static string RenameBoxPrefabByName(string boxName, string targetBoxName)
    {
        BoxTypeDefineDict.ExportTypeNames(); // todo 判断是否要删掉此行
        string boxPrefabPath = BoxTypeDefineDict.GetTypeAssetDataBasePath(boxName);
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
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FindWorldModulePrefabPathByName(worldModuleName));
        return prefab;
    }

    public static string FindWorldModulePrefabPathByName(string worldModuleName)
    {
        WorldModuleTypeDefineDict.ExportTypeNames(); // todo 判断是否要删掉此行
        return WorldModuleTypeDefineDict.GetTypeAssetDataBasePath(worldModuleName);
    }

    public static GameObject FindWorldPrefabByName(string worldName)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FindWorldPrefabPathByName(worldName));
        return prefab;
    }

    public static string FindWorldPrefabPathByName(string worldName)
    {
        WorldTypeDefineDict.ExportTypeNames(); // todo 判断是否要删掉此行
        return WorldTypeDefineDict.GetTypeAssetDataBasePath(worldName);
    }
#endif

    #endregion
}
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class BoxEditorWindow : EditorWindow
{
    [MenuItem("开发工具/配置/箱子编辑窗口")]
    public static void ShowBoxEditorWindow()
    {
        BoxEditorWindow window = new BoxEditorWindow();
        window.ShowUtility();
    }

    [MenuItem("开发工具/配置/测试高斯随机数")]
    public static void TestGaussianRandom()
    {
        GaussianRandom gRandom = new GaussianRandom();
        float sum = 0;
        for (int i = 0; i < 500; i++)
        {
            float value = gRandom.Range(5, 3);
            sum += value;
            Debug.Log(value);
        }

        Debug.Log(sum/ 500);
    }

    //[MenuItem("开发工具/配置/关卡编辑器箱子替换成纯美术体")]
    //public static void ReplaceBoxToPureArt()
    //{
    //    List<string> worldModuleNames = ConfigManager.GetAllWorldModuleNames();
    //    foreach (string worldModuleName in worldModuleNames)
    //    {
    //        string worldModulePrefabPath = ConfigManager.FindWorldModulePrefabPathByName(worldModuleName);
    //        if (!string.IsNullOrEmpty(worldModulePrefabPath))
    //        {
    //            GameObject worldModuleGO = PrefabUtility.LoadPrefabContents(worldModulePrefabPath);
    //            WorldModuleDesignHelper module = worldModuleGO.GetComponent<WorldModuleDesignHelper>();
    //            if (module)
    //            {
    //                Box[] boxes = module.GetComponentsInChildren<Box>();
    //                foreach (Box box in boxes)
    //                {
    //                    List<BoxPassiveSkill> passiveSkills = new List<BoxPassiveSkill>();
    //                    foreach (BoxPassiveSkill bps in box.RawBoxPassiveSkills)
    //                    {
    //                        if (bps.SpecialCaseType != BoxPassiveSkill.BoxPassiveSkillBaseSpecialCaseType.None)
    //                        {
    //                            passiveSkills.Add(bps.Clone());
    //                        }
    //                    }

    //                    GameObject boxPrefab = PrefabUtility.GetCorrespondingObjectFromSource(box.gameObject);
    //                    GameObject boxLevelEditorPrefab = ConfigManager.FindBoxLevelEditorPrefabByName(boxPrefab.name);
    //                    GameObject boxLevelEditorGO = (GameObject) PrefabUtility.InstantiatePrefab(boxLevelEditorPrefab);
    //                    boxLevelEditorGO.transform.parent = box.transform.parent;
    //                    boxLevelEditorGO.transform.localPosition = box.transform.localPosition;
    //                    boxLevelEditorGO.transform.localRotation = box.transform.localRotation;
    //                    boxLevelEditorGO.transform.SetSiblingIndex(box.transform.GetSiblingIndex());
    //                    DestroyImmediate(box.gameObject);
    //                    Box_LevelEditor boxLevelEditor = boxLevelEditorGO.GetComponent<Box_LevelEditor>();
    //                    boxLevelEditor.RawBoxPassiveSkills = passiveSkills;
    //                }
    //            }

    //            PrefabUtility.SaveAsPrefabAsset(worldModuleGO, worldModulePrefabPath, out bool suc); // 保存回改Prefab的Asset
    //        }
    //    }
    //}

    //[MenuItem("开发工具/配置/世界编辑器特殊箱子替换成纯美术体")]
    //public static void ReplaceBoxToPureArt()
    //{
    //    List<string> worldNames = ConfigManager.GetAllWorldNames();
    //    foreach (string worldName in worldNames)
    //    {
    //        string worldPrefabPath = ConfigManager.FindWorldPrefabPathByName(worldName);
    //        if (!string.IsNullOrEmpty(worldPrefabPath))
    //        {
    //            GameObject worldGO = PrefabUtility.LoadPrefabContents(worldPrefabPath);
    //            WorldDesignHelper world = worldGO.GetComponent<WorldDesignHelper>();
    //            if (world)
    //            {
    //                Box[] boxes = world.GetComponentsInChildren<Box>();
    //                foreach (Box box in boxes)
    //                {
    //                    List<BoxPassiveSkill> passiveSkills = new List<BoxPassiveSkill>();
    //                    foreach (BoxPassiveSkill bps in box.RawBoxPassiveSkills)
    //                    {
    //                        if (bps.SpecialCaseType != BoxPassiveSkill.BoxPassiveSkillBaseSpecialCaseType.None)
    //                        {
    //                            passiveSkills.Add(bps.Clone());
    //                        }
    //                    }

    //                    GameObject boxPrefab = PrefabUtility.GetCorrespondingObjectFromSource(box.gameObject);
    //                    GameObject boxLevelEditorPrefab = ConfigManager.FindBoxLevelEditorPrefabByName(boxPrefab.name);
    //                    GameObject boxLevelEditorGO = (GameObject) PrefabUtility.InstantiatePrefab(boxLevelEditorPrefab);
    //                    boxLevelEditorGO.transform.parent = box.transform.parent;
    //                    boxLevelEditorGO.transform.localPosition = box.transform.localPosition;
    //                    boxLevelEditorGO.transform.localRotation = box.transform.localRotation;
    //                    boxLevelEditorGO.transform.SetSiblingIndex(box.transform.GetSiblingIndex());
    //                    DestroyImmediate(box.gameObject);
    //                    Box_LevelEditor boxLevelEditor = boxLevelEditorGO.GetComponent<Box_LevelEditor>();
    //                    boxLevelEditor.RawBoxPassiveSkills = passiveSkills;
    //                }
    //            }

    //            PrefabUtility.SaveAsPrefabAsset(worldGO, worldPrefabPath, out bool suc); // 保存回改Prefab的Asset
    //        }
    //    }
    //}

    private string srcBoxName;
    private string tarBoxName;

    void OnEnable()
    {
        name = "箱子编辑器";
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("每次点击按钮前请序列化配置！！！");
        srcBoxName = EditorGUILayout.TextField("源箱子名称", srcBoxName);
        tarBoxName = EditorGUILayout.TextField("目标箱子名称", tarBoxName);

        if (GUILayout.Button("将源箱子更名为目标箱子名称"))
        {
            RenameBox(srcBoxName, tarBoxName);
        }

        if (GUILayout.Button("删除源箱子"))
        {
            DeleteBox(srcBoxName);
        }

        if (GUILayout.Button("(工具)刷新"))
        {
            RefreshBoxLevelEditor();
        }
    }

    private void RenameBox(string srcBoxName, string tarBoxName)
    {
        string renameBoxRef()
        {
            StringBuilder info = new StringBuilder();

            // Ref in Boxes
            Dictionary<string, ushort> boxDict = ConfigManager.BoxTypeDefineDict.TypeIndexDict;
            foreach (string boxName in boxDict.Keys.ToList())
            {
                GameObject boxPrefab = ConfigManager.FindBoxPrefabByName(boxName);
                bool isDirty = false;
                if (boxPrefab)
                {
                    Box box = boxPrefab.GetComponent<Box>();
                    if (box)
                    {
                        isDirty = box.RenameBoxTypeName(srcBoxName, tarBoxName, info);
                    }
                }

                if (isDirty)
                {
                    PrefabUtility.SavePrefabAsset(boxPrefab);
                }
            }

            // Ref in Actors
            List<string> actorList = ConfigManager.GetAllActorNames();
            foreach (string actorName in actorList)
            {
                GameObject actorPrefab = ConfigManager.FindActorPrefabByName(actorName);
                bool isDirty = false;
                if (actorPrefab)
                {
                    Actor actor = actorPrefab.GetComponent<Actor>();
                    if (actor)
                    {
                        isDirty = actor.RenameBoxTypeName(srcBoxName, tarBoxName, info);
                    }
                }

                if (isDirty)
                {
                    PrefabUtility.SavePrefabAsset(actorPrefab);
                }
            }

            // Ref in LevelTriggers
            List<string> levelTriggerNames = ConfigManager.GetAllLevelTriggerNames();
            foreach (string levelTriggerName in levelTriggerNames)
            {
                GameObject levelTriggerPrefab = ConfigManager.FindLevelTriggerPrefabByName(levelTriggerName);
                bool isDirty = false;
                if (levelTriggerPrefab)
                {
                    LevelTriggerBase levelTrigger = levelTriggerPrefab.GetComponent<LevelTriggerBase>();
                    if (levelTrigger)
                    {
                        isDirty = levelTrigger.RenameBoxTypeName(srcBoxName, tarBoxName, info);
                    }
                }

                if (isDirty)
                {
                    PrefabUtility.SavePrefabAsset(levelTriggerPrefab);
                }
            }

            // Ref in WorldModules
            List<string> worldModuleNames = ConfigManager.GetAllWorldModuleNames();
            foreach (string worldModuleName in worldModuleNames)
            {
                GameObject worldModulePrefab = ConfigManager.FindWorldModulePrefabByName(worldModuleName);
                bool isDirty = false;
                if (worldModulePrefab)
                {
                    WorldModuleDesignHelper module = worldModulePrefab.GetComponent<WorldModuleDesignHelper>();
                    if (module)
                    {
                        isDirty = module.RenameBoxTypeName(srcBoxName, tarBoxName, info);
                    }
                }

                if (isDirty)
                {
                    PrefabUtility.SavePrefabAsset(worldModulePrefab);
                }
            }

            List<string> staticLayoutNames = ConfigManager.GetAllStaticLayoutNames();
            foreach (string staticLayoutName in staticLayoutNames)
            {
                GameObject staticLayoutPrefab = ConfigManager.FindStaticLayoutPrefabByName(staticLayoutName);
                bool isDirty = false;
                if (staticLayoutPrefab)
                {
                    WorldModuleDesignHelper module = staticLayoutPrefab.GetComponent<WorldModuleDesignHelper>();
                    if (module)
                    {
                        isDirty = module.RenameBoxTypeName(srcBoxName, tarBoxName, info);
                    }
                }

                if (isDirty)
                {
                    PrefabUtility.SavePrefabAsset(staticLayoutPrefab);
                }
            }

            // Ref in Worlds
            List<string> worldNames = ConfigManager.GetAllWorldNames();
            foreach (string worldName in worldNames)
            {
                GameObject worldPrefab = ConfigManager.FindWorldPrefabByName(worldName);
                bool isDirty = false;
                if (worldPrefab)
                {
                    WorldDesignHelper world = worldPrefab.GetComponent<WorldDesignHelper>();
                    if (world)
                    {
                        isDirty = world.RenameBoxTypeName(srcBoxName, tarBoxName, info);
                    }
                }

                if (isDirty)
                {
                    PrefabUtility.SavePrefabAsset(worldPrefab);
                }
            }

            return info.ToString();
        }

        if (string.IsNullOrEmpty(srcBoxName) || string.IsNullOrEmpty(tarBoxName))
        {
            EditorUtility.DisplayDialog("箱子更名确认", $"源箱子名称或目标箱子名称不能为空", "确定");
            return;
        }

        if (EditorUtility.DisplayDialog("箱子更名确认", $"您确定将箱子{srcBoxName}更名为{tarBoxName}? 这将包括对应名称的Prefab及其相关引用", "确定"))
        {
            GameObject srcPrefabFound = ConfigManager.FindBoxPrefabByName(srcBoxName);
            GameObject tarPrefabFound = ConfigManager.FindBoxPrefabByName(tarBoxName);
            if (srcPrefabFound)
            {
                if (tarPrefabFound)
                {
                    if (EditorUtility.DisplayDialog("箱子更名确认", $"目标箱子名{tarBoxName}已存在对应Prefab，无法将源箱子重命名为{srcBoxName}", "取消更名"))
                    {
                        return;
                    }
                }
                else
                {
                    if (EditorUtility.DisplayDialog("箱子更名确认", $"现在为您替换该箱子{srcBoxName}相关的引用", "确定", "取消更名"))
                    {
                        string info = renameBoxRef();
                        EditorUtility.DisplayDialog("箱子更名确认", $"已更名引用:\n{info}", "确定");

                        if (EditorUtility.DisplayDialog("箱子更名确认", $"现在将名为{srcBoxName}的Prefab重名名为{tarBoxName}", "确定", "取消更名"))
                        {
                            ConfigManager.RenameBoxPrefabByName(srcBoxName, tarBoxName);
                            EditorUtility.DisplayDialog("箱子更名确认", $"Prefab重命名成功", "确定");
                        }

                        ConfigManager.ExportConfigs();
                    }
                }
            }
            else
            {
                if (EditorUtility.DisplayDialog("箱子更名确认", $"无法检测到Prefab中的{srcBoxName}（可能已被删除或更名），点击继续为您替换该箱子相关的引用", "继续", "取消更名"))
                {
                    string info = renameBoxRef();
                    EditorUtility.DisplayDialog("箱子更名确认", $"已更名引用:\n{info}", "确定");
                    ConfigManager.ExportConfigs();
                    return;
                }
            }
        }
    }

    private void DeleteBox(string srcBoxName)
    {
        string deleteBoxRef()
        {
            StringBuilder info = new StringBuilder();

            // Ref in Boxes
            Dictionary<string, ushort> boxDict = ConfigManager.BoxTypeDefineDict.TypeIndexDict;
            foreach (string boxName in boxDict.Keys.ToList())
            {
                GameObject boxPrefab = ConfigManager.FindBoxPrefabByName(boxName);
                bool isDirty = false;
                if (boxPrefab)
                {
                    Box box = boxPrefab.GetComponent<Box>();
                    if (box)
                    {
                        isDirty = box.DeleteBoxTypeName(srcBoxName, info);
                    }
                }

                if (isDirty)
                {
                    PrefabUtility.SavePrefabAsset(boxPrefab);
                }
            }

            // Ref in Actors
            List<string> actorList = ConfigManager.GetAllActorNames();
            foreach (string actorName in actorList)
            {
                GameObject actorPrefab = ConfigManager.FindActorPrefabByName(actorName);
                bool isDirty = false;
                if (actorPrefab)
                {
                    Actor actor = actorPrefab.GetComponent<Actor>();
                    if (actor)
                    {
                        isDirty = actor.DeleteBoxTypeName(srcBoxName, info);
                    }
                }

                if (isDirty)
                {
                    PrefabUtility.SavePrefabAsset(actorPrefab);
                }
            }

            // Ref in LevelTriggers
            List<string> levelTriggerNames = ConfigManager.GetAllLevelTriggerNames();
            foreach (string levelTriggerName in levelTriggerNames)
            {
                GameObject levelTriggerPrefab = ConfigManager.FindLevelTriggerPrefabByName(levelTriggerName);
                bool isDirty = false;
                if (levelTriggerPrefab)
                {
                    LevelTriggerBase levelTrigger = levelTriggerPrefab.GetComponent<LevelTriggerBase>();
                    if (levelTrigger)
                    {
                        isDirty = levelTrigger.DeleteBoxTypeName(srcBoxName, info);
                    }
                }

                if (isDirty)
                {
                    PrefabUtility.SavePrefabAsset(levelTriggerPrefab);
                }
            }

            // Ref in WorldModules
            List<string> worldModuleNames = ConfigManager.GetAllWorldModuleNames();
            foreach (string worldModuleName in worldModuleNames)
            {
                GameObject worldModulePrefab = ConfigManager.FindWorldModulePrefabByName(worldModuleName);
                bool isDirty = false;
                if (worldModulePrefab)
                {
                    WorldModuleDesignHelper module = worldModulePrefab.GetComponent<WorldModuleDesignHelper>();
                    if (module)
                    {
                        isDirty = module.DeleteBoxTypeName(srcBoxName, info);
                    }
                }

                if (isDirty)
                {
                    PrefabUtility.SavePrefabAsset(worldModulePrefab);
                }
            }

            // Ref in StaticLayouts
            List<string> staticLayoutNames = ConfigManager.GetAllStaticLayoutNames();
            foreach (string staticLayoutName in staticLayoutNames)
            {
                GameObject staticLayoutPrefab = ConfigManager.FindStaticLayoutPrefabByName(staticLayoutName);
                bool isDirty = false;
                if (staticLayoutPrefab)
                {
                    WorldModuleDesignHelper module = staticLayoutPrefab.GetComponent<WorldModuleDesignHelper>();
                    if (module)
                    {
                        isDirty = module.DeleteBoxTypeName(srcBoxName, info);
                    }
                }

                if (isDirty)
                {
                    PrefabUtility.SavePrefabAsset(staticLayoutPrefab);
                }
            }

            // Ref in Worlds
            List<string> worldNames = ConfigManager.GetAllWorldNames();
            foreach (string worldName in worldNames)
            {
                GameObject worldPrefab = ConfigManager.FindWorldPrefabByName(worldName);
                bool isDirty = false;
                if (worldPrefab)
                {
                    WorldDesignHelper world = worldPrefab.GetComponent<WorldDesignHelper>();
                    if (world)
                    {
                        isDirty = world.DeleteBoxTypeName(srcBoxName, info);
                    }
                }

                if (isDirty)
                {
                    PrefabUtility.SavePrefabAsset(worldPrefab);
                }
            }

            return info.ToString();
        }

        if (string.IsNullOrEmpty(srcBoxName))
        {
            EditorUtility.DisplayDialog("箱子删除确认", $"源箱子名称名称不能为空", "确定");
            return;
        }

        if (EditorUtility.DisplayDialog("箱子删除确认", $"您确定将箱子{srcBoxName}删除? 这将包括对应名称的Prefab及其相关引用", "确定"))
        {
            GameObject srcPrefabFound = ConfigManager.FindBoxPrefabByName(srcBoxName);
            if (srcPrefabFound)
            {
                if (EditorUtility.DisplayDialog("箱子删除确认", $"现在将名为{srcBoxName}的Prefab删除", "确定", "取消删除"))
                {
                    bool sucDelete = ConfigManager.DeleteBoxPrefabByName(srcBoxName);
                    if (sucDelete)
                    {
                        EditorUtility.DisplayDialog("箱子删除确认", $"Prefab删除成功", "确定");
                        if (EditorUtility.DisplayDialog("箱子删除确认", $"现在为您删除该箱子{srcBoxName}相关的引用", "确定", "取消删除"))
                        {
                            string info = deleteBoxRef();
                            EditorUtility.DisplayDialog("箱子删除确认", $"已删除引用:\n{info}", "确定");
                        }

                        ConfigManager.ExportConfigs();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("箱子删除确认", $"Prefab删除失败，请重试", "确定");
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (EditorUtility.DisplayDialog("箱子删除确认", $"无法检测到Prefab中的{srcBoxName}（可能已被删除或更名），点击继续为您删除该箱子相关的引用", "继续", "取消删除"))
                {
                    string info = deleteBoxRef();
                    EditorUtility.DisplayDialog("箱子删除确认", $"已删除引用:\n{info}", "确定");
                    ConfigManager.ExportConfigs();
                    return;
                }
            }
        }
        else
        {
            return;
        }
    }

    private void RefreshBoxLevelEditor()
    {
        // Ref in WorldModules
        List<string> worldModuleNames = ConfigManager.GetAllWorldModuleNames();
        foreach (string worldModuleName in worldModuleNames)
        {
            GameObject worldModulePrefab = ConfigManager.FindWorldModulePrefabByName(worldModuleName);
            bool isDirty = false;
            if (worldModulePrefab)
            {
                WorldModuleDesignHelper module = worldModulePrefab.GetComponent<WorldModuleDesignHelper>();
                if (module)
                {
                    isDirty = module.RefreshBoxLevelEditor();
                }
            }

            if (isDirty)
            {
                PrefabUtility.SavePrefabAsset(worldModulePrefab);
            }
        }

        // Ref in StaticLayouts
        List<string> staticLayoutNames = ConfigManager.GetAllStaticLayoutNames();
        foreach (string staticLayoutName in staticLayoutNames)
        {
            GameObject staticLayoutPrefab = ConfigManager.FindStaticLayoutPrefabByName(staticLayoutName);
            bool isDirty = false;
            if (staticLayoutPrefab)
            {
                WorldModuleDesignHelper module = staticLayoutPrefab.GetComponent<WorldModuleDesignHelper>();
                if (module)
                {
                    isDirty = module.RefreshBoxLevelEditor();
                }
            }

            if (isDirty)
            {
                PrefabUtility.SavePrefabAsset(staticLayoutPrefab);
            }
        }
    }
}
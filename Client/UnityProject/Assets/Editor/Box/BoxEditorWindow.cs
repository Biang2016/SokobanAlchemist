using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class EntityEditorWindow : EditorWindow
{
    [MenuItem("开发工具/配置/Entity编辑窗口")]
    public static void ShowBoxEditorWindow()
    {
        EntityEditorWindow window = new EntityEditorWindow();
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

        Debug.Log(sum / 500);
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

    void OnEnable()
    {
        name = "箱子编辑器";
    }

    void OnGUI()
    {
        if (GUILayout.Button("(工具)刷新"))
        {
            RefreshBoxLevelEditor();
        }
    }

    private void RefreshBoxLevelEditor()
    {
        // Ref in WorldModules
        List<string> worldModuleNames = ConfigManager.GetAllTypeNames(TypeDefineType.WorldModule);
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
        List<string> staticLayoutNames = ConfigManager.GetAllTypeNames(TypeDefineType.StaticLayout);
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

    private void RefreshBornPointDesignHelper()
    {
        // Ref in WorldModules
        List<string> worldModuleNames = ConfigManager.GetAllTypeNames(TypeDefineType.WorldModule);
        foreach (string worldModuleName in worldModuleNames)
        {
            GameObject worldModulePrefab = ConfigManager.FindWorldModulePrefabByName(worldModuleName);
            bool isDirty = false;
            if (worldModulePrefab)
            {
                WorldModuleDesignHelper module = worldModulePrefab.GetComponent<WorldModuleDesignHelper>();
                if (module)
                {
                    isDirty = module.RefreshBornPointDesignHelper();
                }
            }

            if (isDirty)
            {
                PrefabUtility.SavePrefabAsset(worldModulePrefab);
            }
        }

        // Ref in StaticLayouts
        List<string> staticLayoutNames = ConfigManager.GetAllTypeNames(TypeDefineType.StaticLayout);
        foreach (string staticLayoutName in staticLayoutNames)
        {
            GameObject staticLayoutPrefab = ConfigManager.FindStaticLayoutPrefabByName(staticLayoutName);
            bool isDirty = false;
            if (staticLayoutPrefab)
            {
                WorldModuleDesignHelper module = staticLayoutPrefab.GetComponent<WorldModuleDesignHelper>();
                if (module)
                {
                    isDirty = module.RefreshBornPointDesignHelper();
                }
            }

            if (isDirty)
            {
                PrefabUtility.SavePrefabAsset(staticLayoutPrefab);
            }
        }
    }
}
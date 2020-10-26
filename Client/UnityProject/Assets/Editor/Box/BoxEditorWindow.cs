using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BiangStudio.GamePlay;
using UnityEditor;
using UnityEngine;

public class BoxEditorWindow : EditorWindow
{
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
    }

    private void RenameBox(string srcBoxName, string tarBoxName)
    {
        if (string.IsNullOrEmpty(srcBoxName) || string.IsNullOrEmpty(tarBoxName))
        {
            EditorUtility.DisplayDialog("箱子更名确认", $"源箱子名称或目标箱子名称不能为空", "确定");
            return;
        }

        if (EditorUtility.DisplayDialog("箱子更名确认", $"您确定将箱子{srcBoxName}更名为{tarBoxName}? 这将包括对应名称的Prefab及其相关引用", "确定"))
        {
            GameObject srcPrefabFound = FindBoxPrefabByName(srcBoxName);
            GameObject tarPrefabFound = FindBoxPrefabByName(tarBoxName);
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
                    if (EditorUtility.DisplayDialog("箱子更名确认", $"现在将名为{srcBoxName}的Prefab重名名为{tarBoxName}", "确定", "取消更名"))
                    {
                        // todo rename
                        EditorUtility.DisplayDialog("箱子更名确认", $"Prefab重命名成功", "确定");
                        if (EditorUtility.DisplayDialog("箱子更名确认", $"现在为您替换该箱子{srcBoxName}相关的引用", "确定", "取消更名"))
                        {
                            // todo rename ref
                            return;
                        }

                        ConfigManager.ExportConfigs();
                    }
                }
            }
            else
            {
                if (EditorUtility.DisplayDialog("箱子更名确认", $"无法检测到Prefab中的{srcBoxName}（可能已被删除或更名），点击继续为您替换该箱子相关的引用", "继续", "取消更名"))
                {
                    // todo rename ref
                    ConfigManager.ExportConfigs();
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
                GameObject boxPrefab = FindBoxPrefabByName(boxName);
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
                GameObject actorPrefab = FindActorPrefabByName(actorName);
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
                GameObject levelTriggerPrefab = FindLevelTriggerPrefabByName(levelTriggerName);
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
                GameObject worldModulePrefab = FindWorldModulePrefabByName(worldModuleName);
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

            // Ref in Worlds
            List<string> worldNames = ConfigManager.GetAllWorldNames();
            foreach (string worldName in worldNames)
            {
                GameObject worldPrefab = FindWorldPrefabByName(worldName);
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
            GameObject srcPrefabFound = FindBoxPrefabByName(srcBoxName);
            if (srcPrefabFound)
            {
                if (EditorUtility.DisplayDialog("箱子删除确认", $"现在将名为{srcBoxName}的Prefab删除", "确定", "取消删除"))
                {
                    bool sucDelete = DeleteBoxPrefabByName(srcBoxName);
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

    private GameObject FindBoxPrefabByName(string boxName)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ConfigManager.BoxTypeDefineDict.GetTypeAssetDataBasePath(boxName));
        return prefab;
    }

    private bool DeleteBoxPrefabByName(string boxName)
    {
        return AssetDatabase.DeleteAsset(ConfigManager.BoxTypeDefineDict.GetTypeAssetDataBasePath(boxName));
    }

    private string RenameBoxPrefabByName(string boxName, string targetBoxName)
    {
        return AssetDatabase.RenameAsset(ConfigManager.BoxTypeDefineDict.GetTypeAssetDataBasePath(boxName), targetBoxName);
    }

    private GameObject FindActorPrefabByName(string actorName)
    {
        if (actorName.StartsWith("Player"))
        {
            PrefabManager.Instance.LoadPrefabs();
            return PrefabManager.Instance.GetPrefab("Player");
        }
        else
        {
            GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ConfigManager.EnemyTypeDefineDict.GetTypeAssetDataBasePath(actorName));
            return enemyPrefab;
        }
    }

    private GameObject FindLevelTriggerPrefabByName(string levelTriggerName)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ConfigManager.LevelTriggerTypeDefineDict.GetTypeAssetDataBasePath(levelTriggerName));
        return prefab;
    }

    private GameObject FindWorldModulePrefabByName(string worldModuleName)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ConfigManager.WorldModuleTypeDefineDict.GetTypeAssetDataBasePath(worldModuleName));
        return prefab;
    }

    private GameObject FindWorldPrefabByName(string worldName)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ConfigManager.WorldTypeDefineDict.GetTypeAssetDataBasePath(worldName));
        return prefab;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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
            Dictionary<string, ushort> boxDict = ConfigManager.BoxTypeDefineDict.TypeIndexDict;
            foreach (KeyValuePair<string, ushort> kv in boxDict)
            {
                GameObject boxPrefab = FindBoxPrefabByName(kv.Key);
                if (boxPrefab)
                {
                    Box box = boxPrefab.GetComponent<Box>();
                    if (box)
                    {
                        foreach (BoxFunctionBase bf in box.BoxFunctions)
                        {
                            foreach (FieldInfo fi in bf.GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                            {
                                foreach (Attribute a in fi.GetCustomAttributes(false))
                                {
                                    if (a is BoxNameAttribute)
                                    {
                                        if (fi.FieldType == typeof(string))
                                        {
                                            string fieldValue = (string) fi.GetValue(bf);
                                            if (fieldValue == srcBoxName)
                                            {
                                                info.Append($"{box.name}.BoxFunctions.{bf.GetType().Name}.{fi.Name} = '{srcBoxName}' -> 'None'");
                                                fi.SetValue(bf, "None");
                                            }
                                        }
                                    }
                                    else if (a is BoxNameListAttribute)
                                    {
                                        if (fi.FieldType == typeof(List<string>))
                                        {
                                            List<string> fieldValueList = (List<string>) fi.GetValue(bf);
                                            for (int i = 0; i < fieldValueList.Count; i++)
                                            {
                                                string fieldValue = fieldValueList[i];
                                                if (fieldValue == srcBoxName)
                                                {
                                                    info.Append($"{box.name}.BoxFunctions.{bf.GetType().Name}.{fi.Name} Remove '{srcBoxName}'");
                                                    fieldValueList.RemoveAt(i);
                                                    i--;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
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
                    DeleteBox(srcBoxName);
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
    }

    private GameObject FindBoxPrefabByName(string boxName)
    {
        GameObject boxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ConfigManager.BoxTypeDefineDict.GetTypeAssetDataBasePath(boxName));
        return boxPrefab;
    }

    private bool DeleteBoxPrefabByName(string boxName)
    {
        return AssetDatabase.DeleteAsset(ConfigManager.BoxTypeDefineDict.GetTypeAssetDataBasePath(boxName));
    }

    private string RenameBoxPrefabByName(string boxName, string targetBoxName)
    {
        return AssetDatabase.RenameAsset(ConfigManager.BoxTypeDefineDict.GetTypeAssetDataBasePath(boxName), targetBoxName);
    }
}
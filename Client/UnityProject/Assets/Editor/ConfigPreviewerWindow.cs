using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

public class ConfigPreviewerWindow : OdinEditorWindow
{
    [MenuItem("开发工具/面板/配置浏览器")]
    public static void OpenConfigPreviewWindow()
    {
        ConfigManager.LoadAllConfigs();
        GetWindow<ConfigPreviewerWindow>().Show();
    }

    [ShowInInspector]
    [LabelText("世界模组配置表")]
    [TableList]
    public List<WorldModuleData> WorldModuleDataConfigDict => ConfigManager.WorldModuleDataConfigDict.Values.ToList();

    [ShowInInspector]
    [LabelText("静态布局配置表")]
    [TableList]
    public List<WorldModuleData> StaticLayoutDataConfigDict => ConfigManager.StaticLayoutDataConfigDict.Values.ToList();

    [ShowInInspector]
    [LabelText("世界配置表")]
    [TableList]
    public List<WorldData> WorldDataConfigDict => ConfigManager.WorldDataConfigDict.Values.ToList();

    [ReadOnly]
    [ShowInInspector]
    [LabelText("角色Buff相克表")]
    public EntityBuffAttributeRelationship[,] ActorBuffAttributeMatrix => ConfigManager.EntityBuffAttributeMatrix;
}
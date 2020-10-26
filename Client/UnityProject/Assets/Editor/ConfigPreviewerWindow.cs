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
    [LabelText("箱子类型表")]
    public SortedDictionary<ushort, string> BoxTypeNameDict => ConfigManager.BoxTypeDefineDict.TypeNameDict;

    [ShowInInspector]
    [LabelText("箱子icon类型表")]
    public SortedDictionary<ushort, string> BoxIconTypeNameDict => ConfigManager.BoxIconTypeDefineDict.TypeNameDict;

    [ShowInInspector]
    [LabelText("敌人类型表")]
    public SortedDictionary<ushort, string> EnemyTypeNameDict => ConfigManager.EnemyTypeDefineDict.TypeNameDict;

    [ShowInInspector]
    [LabelText("关卡Trigger类型表")]
    public SortedDictionary<ushort, string> LevelTriggerTypeNameDict => ConfigManager.LevelTriggerTypeDefineDict.TypeNameDict;

    [ShowInInspector]
    [LabelText("世界模组类型表")]
    public SortedDictionary<ushort, string> WorldModuleTypeNameDict => ConfigManager.WorldModuleTypeDefineDict.TypeNameDict;

    [ShowInInspector]
    [LabelText("世界配置表")]
    [TableList]
    public List<WorldData> WorldDataConfigDict => ConfigManager.WorldDataConfigDict.Values.ToList();

    [ShowInInspector]
    [LabelText("世界模组配置表")]
    [TableList]
    public List<WorldModuleData> WorldModuleDataConfigDict => ConfigManager.WorldModuleDataConfigDict.Values.ToList();
}
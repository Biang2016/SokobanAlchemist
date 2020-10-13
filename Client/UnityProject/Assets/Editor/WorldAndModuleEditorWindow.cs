using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class WorldAndModuleEditorWindow : OdinEditorWindow
{
    [MenuItem("开发工具/面板/模组编辑器")]
    public static void OpenWorldAndModuleEditorWindow()
    {
        //ControlManager.Instance.Awake();
        ConfigManager.LoadAllConfigs();
        GetWindow<WorldAndModuleEditorWindow>().Show();
    }

    [LabelText("编辑模式选择")]
    [EnumToggleButtons]
    [ShowInInspector]
    public static EditMode EditMode;


}

public enum EditMode
{
    Normal,
    Selection,
    DeleteBox,
    HideBoxInWorldMode,
}
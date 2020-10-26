using UnityEngine;
using UnityEditor;

public class BoxEditorTools
{
    [MenuItem("开发工具/配置/箱子编辑窗口")]
    public static void ShowBoxEditorWindow()
    {
        BoxEditorWindow window = new BoxEditorWindow();
        window.ShowUtility();
    }
}
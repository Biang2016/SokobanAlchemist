using UnityEngine;
using UnityEditor;

namespace UnityToolbarExtender
{
    [InitializeOnLoad]
    public class SceneSwitchLeftButton
    {
        static class ToolbarStyles
        {
            public static readonly GUIStyle toolbarbuttonLeft;
            public static readonly GUIStyle toolbarbuttonRight;
            public static readonly GUIStyle toolbarbutton;

            static ToolbarStyles()
            {
                toolbarbuttonLeft = new GUIStyle("toolbarbuttonLeft")
                {
                    fontSize = 11,
                    alignment = TextAnchor.MiddleCenter,
                    imagePosition = ImagePosition.ImageAbove,
                    fontStyle = FontStyle.Normal,
                };
                toolbarbuttonRight = new GUIStyle("toolbarbuttonRight")
                {
                    fontSize = 11,
                    alignment = TextAnchor.MiddleCenter,
                    imagePosition = ImagePosition.ImageAbove,
                    fontStyle = FontStyle.Normal,
                };
                toolbarbutton = new GUIStyle("toolbarbutton")
                {
                    fontSize = 11,
                    alignment = TextAnchor.MiddleCenter,
                    imagePosition = ImagePosition.ImageAbove,
                    fontStyle = FontStyle.Normal,
                };
            }
        }

        static SceneSwitchLeftButton()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnLeftToolbarGUI);
            ToolbarExtender.RightToolbarGUI.Add(OnRightToolbarGUI);
        }

        private static void OnLeftToolbarGUI()
        {
            if (GUILayout.Button(new GUIContent("箱子编辑器"), ToolbarStyles.toolbarbutton))
            {
                EntityEditorWindow.ShowBoxEditorWindow();
            }

            if (GUILayout.Button(new GUIContent("Buff克制编辑"), ToolbarStyles.toolbarbutton))
            {
                BuffEditorWindow.ShowBuffEditorWindow();
            }

            GUILayout.FlexibleSpace();
        }

        private static void OnRightToolbarGUI()
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("快速序列化类型"), ToolbarStyles.toolbarbuttonLeft))
            {
                ConfigManager.QuickExportConfigs_TypeDefines();
            }

            if (GUILayout.Button(new GUIContent("序列化配置"), ToolbarStyles.toolbarbuttonRight))
            {
                ConfigManager.ExportConfigs();
            }
        }
    }
}
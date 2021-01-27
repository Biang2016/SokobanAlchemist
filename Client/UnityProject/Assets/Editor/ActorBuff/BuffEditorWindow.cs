using System;
using System.Collections.Generic;
using System.Reflection;
using BiangLibrary;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class BuffEditorWindow : EditorWindow
{
    [MenuItem("开发工具/配置/Buff克制编辑")]
    public static void ShowBuffEditorWindow()
    {
        BuffEditorWindow window = new BuffEditorWindow();
        window.ShowUtility();
    }

    void OnEnable()
    {
        name = "Buff克制编辑";
        Init();
    }

    private GUITable table;

    private void Init()
    {
        RelationshipColorDict = new Dictionary<EntityBuffAttributeRelationship, Color>
        {
            {EntityBuffAttributeRelationship.Compatible, "#16C904".HTMLColorToColor()},
            {EntityBuffAttributeRelationship.Disperse, "#23E4E8".HTMLColorToColor()},
            {EntityBuffAttributeRelationship.Repel, "#BC8EF9".HTMLColorToColor()},
            {EntityBuffAttributeRelationship.SetOff, "#A3A3A3".HTMLColorToColor()},
            {EntityBuffAttributeRelationship.MaxDominant, "#F3CD5D".HTMLColorToColor()},
        };

        int buffTypeEnumCount = Enum.GetValues(typeof(EntityBuffAttribute)).Length;
        string[] descriptionsOfBuffType = new string[buffTypeEnumCount];

        {
            Type enumType = typeof(EntityBuffAttribute);
            MemberInfo[] memberInfos = enumType.GetMembers();
            int descIndex = 0;
            foreach (MemberInfo mi in memberInfos)
            {
                if (mi.DeclaringType == enumType)
                {
                    object[] valueAttributes = mi.GetCustomAttributes(typeof(LabelTextAttribute), false);
                    foreach (object va in valueAttributes)
                    {
                        if (va is LabelTextAttribute a)
                        {
                            descriptionsOfBuffType[descIndex] = a.Text;
                            descIndex++;
                        }
                    }
                }
            }
        }

        int relationshipEnumCount = Enum.GetValues(typeof(EntityBuffAttributeRelationship)).Length;
        string[] descriptionsOfRelationship = new string[relationshipEnumCount];
        {
            Type enumType = typeof(EntityBuffAttributeRelationship);
            MemberInfo[] memberInfos = enumType.GetMembers();
            int descIndex = 0;
            foreach (MemberInfo mi in memberInfos)
            {
                if (mi.DeclaringType == enumType)
                {
                    object[] valueAttributes = mi.GetCustomAttributes(typeof(LabelTextAttribute), false);
                    foreach (object va in valueAttributes)
                    {
                        if (va is LabelTextAttribute a)
                        {
                            descriptionsOfRelationship[descIndex] = a.Text;
                            descIndex++;
                        }
                    }
                }
            }
        }

        // 左侧标签为既有buff，顶部标签为新增buff
        // x为顶部标签，水平向坐标，y为左侧标签，竖直向坐标
        EntityBuffAttributeRelationship[,] arr = ConfigManager.GetBuffAttributeMatrixAsset().EntityBuffAttributeMatrix;
        if (arr != null)
        {
            this.table = GUITable.Create(
                twoDimArray: arr,
                drawElement: (rect, x, y) =>
                {
                    Rect left = new Rect(rect.x, rect.y, rect.width / 2f, rect.height);
                    Rect right = new Rect(rect.x + rect.width / 2f, rect.y, rect.width / 2f, rect.height);
                    EntityBuffAttributeRelationship newValue = (EntityBuffAttributeRelationship) EditorGUI.EnumPopup(left, arr[y, x]);
                    if (x != y && (newValue == EntityBuffAttributeRelationship.MaxDominant))
                    {
                        Debug.LogError($"【Buff相克矩阵】{(EntityBuffAttribute) x}和{(EntityBuffAttribute) y}之间的关系有误，异种BuffAttribute之间的关系不允许选用{newValue}");
                    }
                    else
                    {
                        arr[y, x] = newValue;
                        GUI.color = RelationshipColorDict[newValue];
                        EditorGUI.LabelField(right, descriptionsOfRelationship[(int) newValue]);
                        GUI.color = Color.white;
                    }
                },
                horizontalLabel: null, // horizontalLabel is optional and can be null.
                columnLabels: (rect, x) => GUI.Label(rect, descriptionsOfBuffType[x]), // columnLabels is optional and can be null.
                verticalLabel: null, // verticalLabel is optional and can be null.
                rowLabels: (rect, y) => GUI.Label(rect, descriptionsOfBuffType[y]) // rowLabels is optional and can be null.
            );
        }
        else
        {
            table = null;
        }
    }

    private Dictionary<EntityBuffAttributeRelationship, Color> RelationshipColorDict;

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Buff克制矩阵, 左侧标签为既有buff，顶部标签为新增buff");
        try
        {
            table?.DrawTable();
        }
        catch
        {
        }

        if (GUILayout.Button("加载 / 修改矩阵维度后先加载再保存，否则丢失数据"))
        {
            ConfigManager.LoadEntityBuffAttributeMatrix(DataFormat.Binary);
            Init();
        }

        if (GUILayout.Button("保存"))
        {
            ConfigManager.ExportEntityBuffAttributeMatrix(DataFormat.Binary);
            ConfigManager.LoadEntityBuffAttributeMatrix(DataFormat.Binary);
            Init();
        }

        if (GUILayout.Button("新建"))
        {
            if (EditorUtility.DisplayDialog("提示", $"新建矩阵将覆盖现有矩阵，是否继续?", "继续", "取消"))
            {
                ConfigManager.CreateNewBuffAttributeMatrixAsset();
            }
        }
    }
}
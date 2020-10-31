using System;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class ActorBuffEditorWindow : EditorWindow
{
    [MenuItem("开发工具/配置/角色Buff克制编辑")]
    public static void ShowActorBuffEditorWindow()
    {
        ActorBuffEditorWindow window = new ActorBuffEditorWindow();
        window.ShowUtility();
    }

    void OnEnable()
    {
        name = "角色Buff克制编辑";
        Init();
    }

    private GUITable table;

    private void Init()
    {
        int buffTypeEnumCount = Enum.GetValues(typeof(ActorBuffAttribute)).Length;
        string[] descriptionsOfBuffType = new string[buffTypeEnumCount];

        {
            Type enumType = typeof(ActorBuffAttribute);
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

        int relationshipEnumCount = Enum.GetValues(typeof(ActorBuffAttributeRelationship)).Length;
        string[] descriptionsOfRelationship = new string[relationshipEnumCount];
        {
            Type enumType = typeof(ActorBuffAttributeRelationship);
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

        ActorBuffAttributeRelationship[,] arr = ConfigManager.GetActorBuffAttributeMatrixAsset().ActorBuffAttributeMatrix;
        if (arr != null)
        {
            this.table = GUITable.Create(
                twoDimArray: arr,
                drawElement: (rect, x, y) =>
                {
                    if (x > y) return;
                    Rect left = new Rect(rect.x, rect.y, rect.width / 2f, rect.height);
                    Rect right = new Rect(rect.x + rect.width / 2f, rect.y, rect.width / 2f, rect.height);
                    ActorBuffAttributeRelationship newValue = (ActorBuffAttributeRelationship) EditorGUI.EnumPopup(left, arr[x, y]);
                    if (x != y && (newValue == ActorBuffAttributeRelationship.MaxDominant))
                    {
                        Debug.LogError($"【角色Buff相克矩阵】{(ActorBuffAttribute) x}和{(ActorBuffAttribute) y}之间的关系有误，异种BuffAttribute之间的关系不允许选用{newValue}");
                    }
                    else
                    {
                        arr[x, y] = newValue;
                        arr[y, x] = newValue;
                        EditorGUI.LabelField(right, descriptionsOfRelationship[(int) newValue]);
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

    private void OnGUI()
    {
        EditorGUILayout.LabelField("角色Buff克制矩阵");
        table?.DrawTable();
        if (GUILayout.Button("加载 / 修改矩阵维度后先加载再保存，否则丢失数据"))
        {
            ConfigManager.LoadActorBuffAttributeMatrix(DataFormat.Binary);
            Init();
        }

        if (GUILayout.Button("保存"))
        {
            ConfigManager.ExportActorBuffAttributeMatrix(DataFormat.Binary);
            ConfigManager.LoadActorBuffAttributeMatrix(DataFormat.Binary);
            Init();
        }

        if (GUILayout.Button("新建"))
        {
            if (EditorUtility.DisplayDialog("提示", $"新建矩阵将覆盖现有矩阵，是否继续?", "继续", "取消"))
            {
                ConfigManager.CreateNewActorBuffAttributeMatrixAsset();
            }
        }
    }
}
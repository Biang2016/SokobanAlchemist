using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BiangLibrary;
using BiangLibrary.GameDataFormat.Grid;
using FlowCanvas;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;

#endif

[ExecuteInEditMode]
public class WorldModuleDesignHelper : MonoBehaviour
{
    public WorldModuleFeature WorldModuleFeature;

    [LabelText("模组AI")]
    [PropertyOrder(-1)]
    public FlowScript WorldModuleAI;

#if UNITY_EDITOR

    [PropertyOrder(-1)]
    [Button("创建模组AI")]
    [HideIf("WorldModuleAI", null)]
    private void CreateModuleAIFlowScript()
    {
        if (WorldModuleAI != null) return;
        Graph newGraph = (Graph) EditorUtils.CreateAsset(typeof(FlowScript), "Assets/Resources/AI/ModuleAI/" + name + ".asset");
        if (newGraph != null)
        {
            UndoUtility.RecordObject(gameObject, "Create Module AI asset");
            WorldModuleAI = (FlowScript) newGraph;
            UndoUtility.SetDirty(gameObject);
            UndoUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }

    public WorldModuleData ExportWorldModuleData()
    {
        List<Box_LevelEditor> boxes = GetComponentsInChildren<Box_LevelEditor>().ToList();

        WorldModuleData worldModuleData = new WorldModuleData();
        worldModuleData.InitNormalModuleData();
        worldModuleData.WorldModuleFeature = WorldModuleFeature;

        Grid3DBounds boxBounds = new Grid3DBounds();
        int xMin = int.MaxValue;
        int xMax = int.MinValue;
        int yMin = int.MaxValue;
        int yMax = int.MinValue;
        int zMin = int.MaxValue;
        int zMax = int.MinValue;
        foreach (Box_LevelEditor box in boxes)
        {
            GridPos3D gp = GridPos3D.GetGridPosByLocalTrans(box.transform, 1);
            GameObject boxEditorPrefab = PrefabUtility.GetCorrespondingObjectFromSource(box.gameObject);
            string boxName = boxEditorPrefab.name.Replace("_LevelEditor", "");
            ushort boxTypeIndex = ConfigManager.BoxTypeDefineDict.TypeIndexDict[boxName];

            bool isLevelEventTriggerAppearBox = false;
            foreach (EntityPassiveSkill eps in box.RawBoxPassiveSkills)
            {
                if (eps is BoxPassiveSkill_LevelEventTriggerAppear bf_leta)
                {
                    BoxPassiveSkill_LevelEventTriggerAppear.Data data = new BoxPassiveSkill_LevelEventTriggerAppear.Data();
                    data.LocalGP = gp;
                    data.BoxTypeIndex = boxTypeIndex;
                    data.BoxOrientation = box.BoxOrientation;
                    data.BoxPassiveSkill_LevelEventTriggerAppear = (BoxPassiveSkill_LevelEventTriggerAppear) bf_leta.Clone();
                    worldModuleData.EventTriggerAppearBoxDataList.Add(data);
                    isLevelEventTriggerAppearBox = true;
                    break;
                }
            }

            if (!isLevelEventTriggerAppearBox)
            {
                bool spaceAvailable = true;
                List<GridPos3D> boxOccupation_rotated = GridPos3D.TransformOccupiedPositions_XZ(box.BoxOrientation, ConfigManager.BoxOccupationConfigDict[boxTypeIndex].BoxIndicatorGPs);
                foreach (GridPos3D gridPos3D in boxOccupation_rotated)
                {
                    GridPos3D gridPos = gridPos3D + gp;
                    if (worldModuleData.BoxMatrix_Temp_CheckOverlap[gridPos.x, gridPos.y, gridPos.z] != 0)
                    {
                        spaceAvailable = false;
                        string box1Name = ConfigManager.BoxTypeDefineDict.TypeNameDict[worldModuleData.BoxMatrix_Temp_CheckOverlap[gridPos.x, gridPos.y, gridPos.z]];
                        string box2Name = ConfigManager.BoxTypeDefineDict.TypeNameDict[boxTypeIndex];
                        Debug.Log($"世界模组[{name}]的{gridPos}位置处存在重叠箱子{box1Name}和{box2Name},已忽略后者");
                    }
                }

                if (spaceAvailable)
                {
                    foreach (GridPos3D gridPos3D in boxOccupation_rotated)
                    {
                        GridPos3D gridPos = gridPos3D + gp;
                        xMin = Mathf.Min(xMin, gridPos.x);
                        xMax = Mathf.Max(xMax, gridPos.x);
                        yMin = Mathf.Min(yMin, gridPos.y);
                        yMax = Mathf.Max(yMax, gridPos.y);
                        zMin = Mathf.Min(zMin, gridPos.z);
                        zMax = Mathf.Max(zMax, gridPos.z);
                    }

                    worldModuleData.RawBoxMatrix[gp.x, gp.y, gp.z] = boxTypeIndex;
                    worldModuleData.RawBoxOrientationMatrix[gp.x, gp.y, gp.z] = box.BoxOrientation;
                    foreach (GridPos3D gridPos3D in boxOccupation_rotated)
                    {
                        GridPos3D gridPos = gridPos3D + gp;
                        worldModuleData.BoxMatrix_Temp_CheckOverlap[gridPos.x, gridPos.y, gridPos.z] = boxTypeIndex;
                    }
                }
            }

            // 就算是LevelEventTriggerAppear的Box，模组特例数据也按原样序列化，箱子生成时到Matrix里面读取ExtraSerializeData
            if (box.RequireSerializePassiveSkillsIntoWorldModule)
            {
                Box_LevelEditor.BoxExtraSerializeData data = box.GetBoxExtraSerializeDataForWorldModule();
                data.LocalGP = gp;
                worldModuleData.BoxExtraSerializeDataMatrix[gp.x, gp.y, gp.z] = data;
            }
        }

        boxBounds.position = new GridPos3D(xMin, yMin, zMin);
        boxBounds.size = new GridPos3D(xMax - xMin + 1, yMax - yMin + 1, zMax - zMin + 1);
        worldModuleData.BoxBounds = boxBounds;

        List<LevelTriggerBase> levelTriggers = GetComponentsInChildren<LevelTriggerBase>().ToList();
        foreach (LevelTriggerBase trigger in levelTriggers)
        {
            LevelTriggerBase.Data data = (LevelTriggerBase.Data) trigger.TriggerData.Clone();
            GameObject levelTriggerPrefab = PrefabUtility.GetCorrespondingObjectFromSource(trigger.gameObject);
            ushort levelTriggerTypeIndex = ConfigManager.LevelTriggerTypeDefineDict.TypeIndexDict[levelTriggerPrefab.name];
            data.LevelTriggerTypeIndex = levelTriggerTypeIndex;
            GridPos3D gp = GridPos3D.GetGridPosByLocalTrans(trigger.transform, 1);
            data.LocalGP = gp;
            worldModuleData.WorldModuleLevelTriggerGroupData.TriggerDataList.Add(data);
        }

        List<BornPointDesignHelper> bornPoints = GetComponentsInChildren<BornPointDesignHelper>().ToList();
        foreach (BornPointDesignHelper bp in bornPoints)
        {
            BornPointData data = (BornPointData) bp.BornPointData.Clone();
            GridPos3D gp = GridPos3D.GetGridPosByLocalTrans(bp.transform, 1);
            data.LocalGP = gp;
            if (data.ActorCategory == ActorCategory.Player)
            {
                worldModuleData.WorldModuleBornPointGroupData.PlayerBornPoints.Add(name + (string.IsNullOrEmpty(data.BornPointAlias) ? "" : "_" + data.BornPointAlias), data);
            }
            else
            {
                worldModuleData.WorldModuleBornPointGroupData.EnemyBornPoints.Add(data);
            }
        }

        worldModuleData.WorldModuleFlowAssetPath = "";
        if (WorldModuleAI)
        {
            string aiAssetPath = AssetDatabase.GetAssetPath(WorldModuleAI);
            if (aiAssetPath.StartsWith("Assets/Resources/") && aiAssetPath.EndsWith(".asset"))
            {
                worldModuleData.WorldModuleFlowAssetPath = aiAssetPath.Replace("Assets/Resources/", "").Replace(".asset", "");
            }
        }

        return worldModuleData;
    }

    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = "#9E93383D".HTMLColorToColor();
        Gizmos.DrawWireCube(Vector3.zero + Vector3.one * (WorldModule.MODULE_SIZE / 2f - 0.5f), Vector3.one * WorldModule.MODULE_SIZE);

        // 仅在模组编辑器中绘制坐标标尺
        WorldDesignHelper world = GetComponentInParent<WorldDesignHelper>();
        if (!world)
        {
            for (int x = 0; x < WorldModule.MODULE_SIZE; x++)
            {
                for (int z = 0; z < WorldModule.MODULE_SIZE; z++)
                {
                    if (x == 0 && z == 0)
                    {
                        transform.DrawSpecialTip(new Vector3(x - 0.25f, -0.5f, z - 0.1f), Color.clear, "#6BC04543".HTMLColorToColor(), $"{x}", 7);
                    }
                    else if (z == 0)
                    {
                        transform.DrawSpecialTip(new Vector3(x - 0.25f, -0.5f, z - 0.1f), Color.clear, "#6BC04543".HTMLColorToColor(), $"{x}", 7);
                        Gizmos.color = "#1AFFFFFF".HTMLColorToColor();
                        Gizmos.DrawLine(new Vector3(x, 0, z) - Vector3.one * 0.5f, new Vector3(x, 0, WorldModule.MODULE_SIZE) - Vector3.one * 0.5f);
                    }
                    else if (x == 0)
                    {
                        transform.DrawSpecialTip(new Vector3(x - 0.25f, -0.5f, z - 0.1f), Color.clear, "#66609BF9".HTMLColorToColor(), $"{z}", 7);
                        Gizmos.color = "#1AFFFFFF".HTMLColorToColor();
                        Gizmos.DrawLine(new Vector3(x, 0, z) - Vector3.one * 0.5f, new Vector3(WorldModule.MODULE_SIZE, 0, z) - Vector3.one * 0.5f);
                    }
                }
            }
        }
    }

    [HideInPlayMode]
    [HideInPrefabAssets]
    [BoxGroup("世界编辑器")]
    [Button("替换世界模组", ButtonSizes.Large)]
    [GUIColor(0f, 1f, 1f)]
    private void ReplaceWorldModule_Editor()
    {
        WorldDesignHelper world = GetComponentInParent<WorldDesignHelper>();
        if (!world)
        {
            Debug.LogError("此功能只能在世界编辑器中使用");
            return;
        }

        GameObject prefab = (GameObject) AssetDatabase.LoadAssetAtPath<Object>("Assets/Designs/WorldModule/" + ReplaceWorldModuleTypeName + ".prefab");
        GameObject go = (GameObject) PrefabUtility.InstantiatePrefab(prefab, transform.parent);
        go.transform.position = transform.position;
        go.transform.rotation = Quaternion.identity;
        DestroyImmediate(gameObject);
    }

    [HideInPlayMode]
    [HideInPrefabAssets]
    [ShowInInspector]
    [NonSerialized]
    [BoxGroup("世界编辑器")]
    [LabelText("替换世界模组类型")]
    [ValueDropdown("GetAllWorldModuleTypeNames")]
    private string ReplaceWorldModuleTypeName;

    private IEnumerable<string> GetAllWorldModuleTypeNames()
    {
        ConfigManager.LoadAllConfigs();
        List<string> res = ConfigManager.WorldModuleTypeDefineDict.TypeIndexDict.Keys.ToList();
        return res;
    }

    public bool SortModule()
    {
        WorldDesignHelper world = GetComponentInParent<WorldDesignHelper>();
        if (world)
        {
            Debug.LogError("此功能只能在模组编辑器中使用");
            return false;
        }

        bool dirty = false;
        dirty |= ArrangeAllRoots();
        dirty |= FormatAllBoxName_Editor();
        dirty |= FormatAllBornPointName_Editor();
        dirty |= FormatAllLevelTriggerName_Editor();
        return dirty;
    }

    private bool FormatAllBoxName_Editor()
    {
        bool dirty = false;
        List<Box_LevelEditor> boxes = GetComponentsInChildren<Box_LevelEditor>().ToList();
        foreach (Box_LevelEditor box in boxes)
        {
            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(box.gameObject);
            dirty |= box.RefreshOrientation();
            string prefabName = prefab.name.Replace("_LevelEditor", "");
            if (box.name != prefabName)
            {
                box.name = prefabName;
                dirty = true;
            }
        }

        return dirty;
    }

    private bool FormatAllLevelTriggerName_Editor()
    {
        bool dirty = false;
        List<LevelTriggerBase> levelTriggers = GetComponentsInChildren<LevelTriggerBase>().ToList();
        foreach (LevelTriggerBase trigger in levelTriggers)
        {
            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(trigger.gameObject);
            if (trigger.name != prefab.name)
            {
                trigger.name = prefab.name;
                dirty = true;
            }
        }

        return dirty;
    }

    private bool FormatAllBornPointName_Editor()
    {
        bool dirty = false;
        List<BornPointDesignHelper> bornPoints = GetComponentsInChildren<BornPointDesignHelper>().ToList();
        foreach (BornPointDesignHelper bp in bornPoints)
        {
            dirty |= bp.FormatAllName_Editor();
        }

        return dirty;
    }

    private Transform GetRoot(WorldModuleHierarchyRootType rootType)
    {
        Transform root = transform.Find($"@_{rootType}");
        if (root) return root;
        return transform;
    }

    private bool ArrangeAllRoots()
    {
        bool dirty = false;
        Transform root;
        foreach (WorldModuleHierarchyRootType rootType in Enum.GetValues(typeof(WorldModuleHierarchyRootType)))
        {
            root = transform.Find("@_" + rootType);
            if (root == null)
            {
                root = new GameObject("@_" + rootType).transform;
                root.parent = transform;
                dirty = true;
            }
        }

        List<LevelTriggerBase> triggers = GetComponentsInChildren<LevelTriggerBase>().ToList();
        root = GetRoot(WorldModuleHierarchyRootType.WorldModuleLevelTriggersRoot);
        foreach (LevelTriggerBase trigger in triggers)
        {
            if (!trigger.transform.IsChildOf(root))
            {
                trigger.transform.parent = root;
                dirty = true;
            }

            trigger.RefreshIsUnderWorldOrModuleBoxesRoot();
        }

        List<BornPointDesignHelper> bornPoints = GetComponentsInChildren<BornPointDesignHelper>().ToList();
        root = GetRoot(WorldModuleHierarchyRootType.WorldModuleBornPointsRoot);
        foreach (BornPointDesignHelper bornPoint in bornPoints)
        {
            if (!bornPoint.transform.IsChildOf(root))
            {
                bornPoint.transform.parent = root;
                dirty = true;
            }
        }

        List<Box_LevelEditor> boxes = GetComponentsInChildren<Box_LevelEditor>().ToList();
        root = GetRoot(WorldModuleHierarchyRootType.BoxesRoot);
        foreach (Box_LevelEditor box in boxes)
        {
            if (!box.transform.IsChildOf(root))
            {
                box.transform.parent = root;
                dirty = true;
            }
        }

        return dirty;
    }

    public bool RenameBoxTypeName(string srcBoxName, string targetBoxName, StringBuilder info)
    {
        bool isDirty = false;
        StringBuilder localInfo = new StringBuilder();
        localInfo.Append($"------------ ModuleStart: {name}\n");
        List<LevelTriggerBase> triggers = GetComponentsInChildren<LevelTriggerBase>().ToList();
        foreach (LevelTriggerBase trigger in triggers)
        {
            isDirty |= trigger.RenameBoxTypeName(srcBoxName, targetBoxName, localInfo);
        }

        List<Box_LevelEditor> boxes = GetComponentsInChildren<Box_LevelEditor>().ToList();
        foreach (Box_LevelEditor box in boxes)
        {
            if (box.RequireSerializePassiveSkillsIntoWorldModule)
            {
                isDirty |= box.RenameBoxTypeName(srcBoxName, targetBoxName, localInfo, true);
            }
        }

        localInfo.Append($"ModuleEnd: {name} ------------\n");
        if (isDirty) info.Append(localInfo);
        return isDirty;
    }

    public bool DeleteBoxTypeName(string srcBoxName, StringBuilder info)
    {
        bool isDirty = false;
        StringBuilder localInfo = new StringBuilder();
        localInfo.Append($"------------ ModuleStart: {name}\n");
        List<LevelTriggerBase> triggers = GetComponentsInChildren<LevelTriggerBase>().ToList();
        foreach (LevelTriggerBase trigger in triggers)
        {
            isDirty |= trigger.DeleteBoxTypeName(srcBoxName, localInfo);
        }

        List<Box_LevelEditor> boxes = GetComponentsInChildren<Box_LevelEditor>().ToList();
        foreach (Box_LevelEditor box in boxes)
        {
            if (box.RequireSerializePassiveSkillsIntoWorldModule)
            {
                isDirty |= box.DeleteBoxTypeName(srcBoxName, localInfo, true);
            }
        }

        localInfo.Append($"ModuleEnd: {name} ------------\n");
        if (isDirty) info.Append(localInfo);
        return isDirty;
    }

#endif
}

public enum WorldModuleHierarchyRootType
{
    BoxesRoot = 0,
    WorldModuleLevelTriggersRoot = 1,
    WorldModuleBornPointsRoot = 2,
}
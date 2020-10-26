using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BiangStudio.GameDataFormat.Grid;
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

    [LabelText("模组AI路径 Resources/??")]
    public string WorldModuleFlowAssetPath;

#if UNITY_EDITOR
    public WorldModuleData ExportWorldModuleData()
    {
        List<Box> boxes = GetComponentsInChildren<Box>().ToList();

        WorldModuleData worldModuleData = new WorldModuleData();
        worldModuleData.WorldModuleFeature = WorldModuleFeature;
        foreach (Box box in boxes)
        {
            GridPos3D gp = GridPos3D.GetGridPosByLocalTrans(box.transform, 1);
            GameObject boxPrefab = PrefabUtility.GetCorrespondingObjectFromSource(box.gameObject);
            ushort boxTypeIndex = ConfigManager.BoxTypeDefineDict.TypeIndexDict[boxPrefab.name];
            worldModuleData.BoxMatrix[gp.x, gp.y, gp.z] = boxTypeIndex;

            if (box.RequireSerializeFunctionIntoWorldModule)
            {
                Box.BoxExtraSerializeData data = box.GetBoxExtraSerializeDataForWorldModule();
                data.LocalGP = gp;
                worldModuleData.BoxExtraSerializeDataMatrix[gp.x, gp.y, gp.z] = data;
            }
        }

        List<LevelTriggerBase> levelTriggers = GetComponentsInChildren<LevelTriggerBase>().ToList();
        foreach (LevelTriggerBase trigger in levelTriggers)
        {
            LevelTriggerBase.Data data = (LevelTriggerBase.Data) trigger.TriggerData.Clone();
            GameObject levelTriggerPrefab = PrefabUtility.GetCorrespondingObjectFromSource(trigger.gameObject);
            ushort levelTriggerTypeIndex = ConfigManager.LevelTriggerTypeDefineDict.TypeIndexDict[levelTriggerPrefab.name];
            data.LevelTriggerTypeIndex = levelTriggerTypeIndex;
            GridPos3D gp = GridPos3D.GetGridPosByLocalTrans(trigger.transform, 1);
            data.LocalGP = gp;
            data.LevelComponentBelongsTo = LevelComponentBelongsTo.WorldModule;
            worldModuleData.WorldModuleLevelTriggerGroupData.TriggerDataList.Add(data);
        }

        List<BornPointDesignHelper> bornPoints = GetComponentsInChildren<BornPointDesignHelper>().ToList();
        foreach (BornPointDesignHelper bp in bornPoints)
        {
            BornPointData data = (BornPointData) bp.BornPointData.Clone();
            GridPos3D gp = GridPos3D.GetGridPosByLocalTrans(bp.transform, 1);
            data.LocalGP = gp;
            data.LevelComponentBelongsTo = LevelComponentBelongsTo.WorldModule;
            worldModuleData.WorldModuleBornPointGroupData.BornPoints.Add(data);
        }

        worldModuleData.WorldModuleFlowAssetPath = WorldModuleFlowAssetPath;
        return worldModuleData;
    }

    public Color RangeGizmoColor;
    public Color RangeGizmoBorderColor;

    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        //if (Selection.Contains(gameObject))
        //{
        //    Gizmos.color = RangeGizmoColor;
        //    Gizmos.DrawCube(Vector3.zero + Vector3.one * (WorldModule.MODULE_SIZE / 2f - 0.5f), Vector3.one * WorldModule.MODULE_SIZE);
        //}

        Gizmos.color = RangeGizmoBorderColor;
        Gizmos.DrawWireCube(Vector3.zero + Vector3.one * (WorldModule.MODULE_SIZE / 2f - 0.5f), Vector3.one * WorldModule.MODULE_SIZE);
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

    [HideInPlayMode]
    [HideInPrefabAssets]
    [Button("一键整理", ButtonSizes.Large)]
    [GUIColor(0f, 1f, 0f)]
    public void SortModuleButton()
    {
        SortModule();
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
        dirty |= FormatAllBoxName_Editor();
        dirty |= FormatAllBornPointName_Editor();
        dirty |= FormatAllLevelTriggerName_Editor();
        dirty |= ArrangeAllRoots();
        return dirty;
    }

    private bool FormatAllBoxName_Editor()
    {
        bool dirty = false;
        List<Box> boxes = GetComponentsInChildren<Box>().ToList();
        foreach (Box box in boxes)
        {
            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(box.gameObject);
            if (box.name != prefab.name)
            {
                box.name = prefab.name;
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
            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(bp.gameObject);
            if (bp.name != prefab.name)
            {
                bp.name = prefab.name;
                dirty = true;
            }
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

        List<Box> boxes = GetComponentsInChildren<Box>().ToList();
        root = GetRoot(WorldModuleHierarchyRootType.BoxesRoot);
        foreach (Box box in boxes)
        {
            if (!box.transform.IsChildOf(root))
            {
                box.transform.parent = root;
                dirty = true;
            }

            box.RefreshIsUnderWorldSpecialBoxesRoot();
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

        List<Box> boxes = GetComponentsInChildren<Box>().ToList();
        foreach (Box box in boxes)
        {
            if (box.RequireSerializeFunctionIntoWorldModule)
            {
                isDirty |= box.RenameBoxTypeName(srcBoxName, targetBoxName, localInfo, true, false);
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

        List<Box> boxes = GetComponentsInChildren<Box>().ToList();
        foreach (Box box in boxes)
        {
            if (box.RequireSerializeFunctionIntoWorldModule)
            {
                isDirty |= box.DeleteBoxTypeName(srcBoxName, localInfo, true, false);
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
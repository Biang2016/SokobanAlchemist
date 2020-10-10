using System;
using System.Collections.Generic;
using System.Linq;
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

        return worldModuleData;
    }

    public Color RangeGizmoColor;
    public Color RangeGizmoBorderColor;

    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        if (Selection.Contains(gameObject))
        {
            Gizmos.color = RangeGizmoColor;
            Gizmos.DrawCube(Vector3.zero + Vector3.one * (WorldModule.MODULE_SIZE / 2f - 0.5f), Vector3.one * WorldModule.MODULE_SIZE);
        }

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
    private void SortModule()
    {
        WorldDesignHelper world = GetComponentInParent<WorldDesignHelper>();
        if (world)
        {
            Debug.LogError("此功能只能在模组编辑器中使用");
            return;
        }

        FormatAllBoxName_Editor();
        ArrangeAllRoots();
    }

    private void FormatAllBoxName_Editor()
    {
        List<Box> boxes = GetComponentsInChildren<Box>().ToList();
        foreach (Box box in boxes)
        {
            GameObject boxPrefab = PrefabUtility.GetCorrespondingObjectFromSource(box.gameObject);
            box.name = boxPrefab.name;
        }
    }

    private Transform GetRoot(WorldModuleHierarchyRootType rootType)
    {
        Transform root = transform.Find($"@_{rootType}");
        if (root) return root;
        return transform;
    }

    private void ArrangeAllRoots()
    {
        foreach (WorldModuleHierarchyRootType rootType in Enum.GetValues(typeof(WorldModuleHierarchyRootType)))
        {
            Transform root = transform.Find("@_" + rootType);
            if (root == null)
            {
                root = new GameObject("@_" + rootType).transform;
                root.parent = transform;
            }
        }

        List<LevelTriggerBase> triggers = GetComponentsInChildren<LevelTriggerBase>().ToList();
        foreach (LevelTriggerBase trigger in triggers)
        {
            Transform root = GetRoot(WorldModuleHierarchyRootType.WorldModuleLevelTriggersRoot);
            if (!trigger.transform.IsChildOf(root))
            {
                trigger.transform.parent = root;
            }
        }

        List<BornPointDesignHelper> bornPoints = GetComponentsInChildren<BornPointDesignHelper>().ToList();
        foreach (BornPointDesignHelper bornPoint in bornPoints)
        {
            Transform root = GetRoot(WorldModuleHierarchyRootType.WorldModuleBornPointsRoot);
            if (!bornPoint.transform.IsChildOf(root))
            {
                bornPoint.transform.parent = root;
            }
        }

        List<Box> boxes = GetComponentsInChildren<Box>().ToList();
        foreach (Box box in boxes)
        {
            Transform boxesRoot = GetRoot(WorldModuleHierarchyRootType.BoxesRoot);
            if (!box.transform.IsChildOf(boxesRoot))
            {
                box.transform.parent = boxesRoot;
            }
        }
    }

#endif
}

public enum WorldModuleHierarchyRootType
{
    BoxesRoot = 0,
    WorldModuleLevelTriggersRoot = 1,
    WorldModuleBornPointsRoot = 2,
}
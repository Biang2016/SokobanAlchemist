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
    [Button("所有箱子命名重置为Prefab名称", ButtonSizes.Large)]
    [GUIColor(0f, 1f, 0f)]
    private void FormatAllBoxName_Editor()
    {
        WorldDesignHelper world = GetComponentInParent<WorldDesignHelper>();
        if (world)
        {
            Debug.LogError("此功能只能在模组编辑器中使用");
            return;
        }

        List<Box> boxes = GetComponentsInChildren<Box>().ToList();
        foreach (Box box in boxes)
        {
            GameObject boxPrefab = PrefabUtility.GetCorrespondingObjectFromSource(box.gameObject);
            box.name = boxPrefab.name;
        }
    }
#endif
}
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
        worldModuleData.WorldModuleFeature = WorldModuleFeature;
        foreach (Box_LevelEditor box in boxes)
        {
            GridPos3D gp = GridPos3D.GetGridPosByLocalTrans(box.transform, 1);
            GameObject boxPrefab = PrefabUtility.GetCorrespondingObjectFromSource(box.gameObject);
            ushort boxTypeIndex = ConfigManager.BoxTypeDefineDict.TypeIndexDict[boxPrefab.name];

            bool isLevelEventTriggerAppearBox = false;
            foreach (BoxPassiveSkill bf in box.RawBoxPassiveSkills)
            {
                if (bf is BoxPassiveSkill_LevelEventTriggerAppear bf_leta)
                {
                    BoxPassiveSkill_LevelEventTriggerAppear.Data data = new BoxPassiveSkill_LevelEventTriggerAppear.Data();
                    data.LocalGP = gp;
                    data.LevelComponentBelongsTo = LevelComponentBelongsTo.WorldModule;
                    data.BoxTypeIndex = boxTypeIndex;
                    data.BoxPassiveSkill_LevelEventTriggerAppear = (BoxPassiveSkill_LevelEventTriggerAppear) bf_leta.Clone();
                    worldModuleData.EventTriggerAppearBoxDataList.Add(data);
                    isLevelEventTriggerAppearBox = true;
                    break;
                }
            }

            if (!isLevelEventTriggerAppearBox) worldModuleData.BoxMatrix[gp.x, gp.y, gp.z] = boxTypeIndex;

            // 就算是LevelEventTriggerAppear的Box，模组特例数据也按原样序列化，箱子生成时到Matrix里面读取ExtraSerializeData
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
        Gizmos.color = "#93383D".HTMLColorToColor();
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
        dirty |= RemoveTriggersFromBoxes_Editor();
        dirty |= FormatAllBoxName_Editor();
        dirty |= FormatAllBornPointName_Editor();
        dirty |= FormatAllLevelTriggerName_Editor();
        return dirty;
    }

    private bool RemoveTriggersFromBoxes_Editor()
    {
        bool dirty = false;
        List<Box> boxes = GetComponentsInChildren<Box>().ToList();
        foreach (Box box in boxes)
        {
            if (box.BoxColliderHelper != null)
            {
                DestroyImmediate(box.BoxColliderHelper.gameObject);
                box.BoxColliderHelper = null;
                dirty = true;
            }
        }

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

        List<Box_LevelEditor> boxes = GetComponentsInChildren<Box_LevelEditor>().ToList();
        foreach (Box_LevelEditor box in boxes)
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

        List<Box_LevelEditor> boxes = GetComponentsInChildren<Box_LevelEditor>().ToList();
        foreach (Box_LevelEditor box in boxes)
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
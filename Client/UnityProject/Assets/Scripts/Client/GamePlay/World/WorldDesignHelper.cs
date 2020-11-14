using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BiangStudio;
using BiangStudio.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

[ExecuteInEditMode]
public class WorldDesignHelper : MonoBehaviour
{
    public WorldFeature WorldFeature;

    [LabelText("默认出生点花名")]
    public string DefaultWorldActorBornPointAlias;

#if UNITY_EDITOR
    public WorldData ExportWorldData()
    {
        List<WorldModuleDesignHelper> modules = GetComponentsInChildren<WorldModuleDesignHelper>().ToList();
        GridPos3D zeroPoint = GridPos3D.Zero;
        foreach (WorldModuleDesignHelper module in modules)
        {
            GridPos3D gp = GridPos3D.GetGridPosByLocalTrans(module.transform, WorldModule.MODULE_SIZE);
            zeroPoint.x = Mathf.Min(gp.x, zeroPoint.x);
            zeroPoint.y = Mathf.Min(gp.y, zeroPoint.y);
            zeroPoint.z = Mathf.Min(gp.z, zeroPoint.z);
        }

        WorldData worldData = new WorldData();
        worldData.WorldFeature = WorldFeature;
        worldData.DefaultWorldActorBornPointAlias = DefaultWorldActorBornPointAlias;
        foreach (WorldModuleDesignHelper module in modules)
        {
            GridPos3D gp = GridPos3D.GetGridPosByLocalTrans(module.transform, WorldModule.MODULE_SIZE);
            gp -= zeroPoint;
            if (gp.x >= World.WORLD_SIZE || gp.y >= World.WORLD_HEIGHT || gp.z >= World.WORLD_SIZE)
            {
                Debug.LogError($"世界大小超过限制，存在模组坐标为{gp}，已忽略该模组");
                continue;
            }

            GameObject worldModulePrefab = PrefabUtility.GetCorrespondingObjectFromSource(module.gameObject);
            ushort worldModuleTypeIndex = ConfigManager.WorldModuleTypeDefineDict.TypeIndexDict[worldModulePrefab.name];
            worldData.ModuleMatrix[gp.x, gp.y, gp.z] = worldModuleTypeIndex;
            worldData.WorldModuleGPOrder.Add(gp);

            // Box ExtraSerializeData
            List<Box> boxes = module.transform.GetComponentsInChildren<Box>().ToList();
            foreach (Box box in boxes)
            {
                GridPos3D boxModuleGP = GridPos3D.GetGridPosByLocalTrans(box.transform, 1);
                if (box.RequireSerializeFunctionIntoWorld)
                {
                    Box.BoxExtraSerializeData data = box.GetBoxExtraSerializeDataForWorldOverrideWorldModule();
                    data.LocalGP = boxModuleGP;
                    worldData.ModuleBoxExtraSerializeDataMatrix[gp.x, gp.y, gp.z].Add(data);
                }
            }
        }

        List<BornPointDesignHelper> bornPoints = GetComponentsInChildren<BornPointDesignHelper>().ToList();
        foreach (BornPointDesignHelper bp in bornPoints)
        {
            if (bp.transform.HasAncestorName($"@_{WorldModuleHierarchyRootType.WorldModuleBornPointsRoot}")) continue;
            BornPointData data = (BornPointData) bp.BornPointData.Clone();
            GridPos3D gp = GridPos3D.GetGridPosByLocalTrans(bp.transform, 1);
            gp -= zeroPoint * WorldModule.MODULE_SIZE;
            data.WorldGP = gp;
            data.LevelComponentBelongsTo = LevelComponentBelongsTo.World;
            if (data.ActorCategory == ActorCategory.Player)
            {
                worldData.WorldBornPointGroupData.WorldSpecialBornPointGroupData.PlayerBornPoints.Add(data.BornPointAlias, data);
            }
            else
            {
                worldData.WorldBornPointGroupData.WorldSpecialBornPointGroupData.EnemyBornPoints.Add(data);
            }
        }

        List<WorldCameraPOI> cameraPOIs = GetComponentsInChildren<WorldCameraPOI>().ToList();
        foreach (WorldCameraPOI poi in cameraPOIs)
        {
            GridPos3D gp = GridPos3D.GetGridPosByLocalTrans(poi.transform, 1);
            gp -= zeroPoint * WorldModule.MODULE_SIZE;
            worldData.WorldCameraPOIData.POIs.Add(gp);
        }

        List<LevelTriggerBase> levelTriggers = GetComponentsInChildren<LevelTriggerBase>().ToList();
        foreach (LevelTriggerBase trigger in levelTriggers)
        {
            if (trigger.transform.HasAncestorName($"@_{WorldModuleHierarchyRootType.WorldModuleLevelTriggersRoot}")) continue;
            WorldModuleDesignHelper module = trigger.transform.GetComponentInParent<WorldModuleDesignHelper>();
            if (module) continue;
            LevelTriggerBase.Data data = (LevelTriggerBase.Data) trigger.TriggerData.Clone();
            GridPos3D gp = GridPos3D.GetGridPosByLocalTrans(trigger.transform, 1);
            gp -= zeroPoint * WorldModule.MODULE_SIZE;
            data.WorldGP = gp;
            data.LevelComponentBelongsTo = LevelComponentBelongsTo.World;
            GameObject levelTriggerPrefab = PrefabUtility.GetCorrespondingObjectFromSource(trigger.gameObject);
            ushort levelTriggerTypeIndex = ConfigManager.LevelTriggerTypeDefineDict.TypeIndexDict[levelTriggerPrefab.name];
            data.LevelTriggerTypeIndex = levelTriggerTypeIndex;
            worldData.WorldLevelTriggerGroupData.TriggerDataList.Add(data);
        }

        // 世界SpecialBox
        List<Box> worldBoxes = GetRoot(WorldHierarchyRootType.WorldSpecialBoxesRoot).GetComponentsInChildren<Box>().ToList();
        foreach (Box worldBox in worldBoxes)
        {
            GameObject boxPrefab = PrefabUtility.GetCorrespondingObjectFromSource(worldBox.gameObject);
            ushort boxTypeIndex = ConfigManager.BoxTypeDefineDict.TypeIndexDict[boxPrefab.name];
            GridPos3D gp = GridPos3D.GetGridPosByLocalTrans(worldBox.transform, 1);
            gp -= zeroPoint * WorldModule.MODULE_SIZE;
            Box.WorldSpecialBoxData worldSpecialBoxData = worldBox.GetBoxSerializeInWorldData();
            worldSpecialBoxData.WorldGP = gp;
            worldSpecialBoxData.BoxTypeIndex = boxTypeIndex;

            // 关卡事件触发出现的Box序列化到单独的地方
            bool isLevelEventTriggerAppearBox = false;
            foreach (BoxPassiveSkill bf in worldBox.RawBoxPassiveSkills)
            {
                if (bf is BoxPassiveSkill_LevelEventTriggerAppear bf_leta)
                {
                    BoxPassiveSkill_LevelEventTriggerAppear.Data data = new BoxPassiveSkill_LevelEventTriggerAppear.Data();
                    data.WorldGP = gp;
                    data.LevelComponentBelongsTo = LevelComponentBelongsTo.World;
                    data.BoxTypeIndex = boxTypeIndex;
                    data.BoxPassiveSkill_LevelEventTriggerAppear = (BoxPassiveSkill_LevelEventTriggerAppear) bf_leta.Clone();
                    data.WorldSpecialBoxData = worldSpecialBoxData; // 世界维度LevelEventTriggerAppear的箱子自己处理自己的ExtraSerializeData
                    worldData.WorldSpecialBoxEventTriggerAppearBoxDataList.Add(data); // 序列到这里
                    isLevelEventTriggerAppearBox = true;
                    break;
                }
            }

            // LevelEventTriggerAppear的箱子不记录到此列表中
            if (!isLevelEventTriggerAppearBox) worldData.WorldSpecialBoxDataList.Add(worldSpecialBoxData);
        }

        return worldData;
    }

    public bool SortWorld()
    {
        bool dirty = false;
        dirty |= ArrangeAllRoots();
        dirty |= FormatAllWorldModuleName_Editor();
        dirty |= FormatAllBornPointName_Editor();
        dirty |= FormatAllWorldCameraPOIName_Editor();
        dirty |= FormatAllLevelTriggerName_Editor();
        dirty |= RemoveTriggersFromBoxes_Editor();
        dirty |= FormatAllBoxName_Editor();
        return dirty;
    }

    private bool FormatAllWorldModuleName_Editor()
    {
        bool dirty = false;
        List<WorldModuleDesignHelper> modules = GetComponentsInChildren<WorldModuleDesignHelper>().ToList();
        foreach (WorldModuleDesignHelper module in modules)
        {
            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(module.gameObject);
            if (module.name != prefab.name)
            {
                module.name = prefab.name;
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

    private bool FormatAllWorldCameraPOIName_Editor()
    {
        bool dirty = false;
        List<WorldCameraPOI> cameraPOIs = GetComponentsInChildren<WorldCameraPOI>().ToList();
        foreach (WorldCameraPOI poi in cameraPOIs)
        {
            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(poi.gameObject);
            if (poi.name != prefab.name)
            {
                poi.name = prefab.name;
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

    private bool RemoveTriggersFromBoxes_Editor()
    {
        bool dirty = false;
        List<Box> worldBoxes = GetRoot(WorldHierarchyRootType.WorldSpecialBoxesRoot).GetComponentsInChildren<Box>().ToList();
        foreach (Box worldBox in worldBoxes)
        {
            if (worldBox.BoxColliderHelper != null)
            {
                DestroyImmediate(worldBox.BoxColliderHelper.gameObject);
                worldBox.BoxColliderHelper = null;
                dirty = true;
            }
        }

        return dirty;
    }

    private bool FormatAllBoxName_Editor()
    {
        bool dirty = false;
        List<Box> worldBoxes = GetRoot(WorldHierarchyRootType.WorldSpecialBoxesRoot).GetComponentsInChildren<Box>().ToList();
        foreach (Box worldBox in worldBoxes)
        {
            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(worldBox.gameObject);
            if (worldBox.name != prefab.name)
            {
                worldBox.name = prefab.name;
                dirty = true;
            }
        }

        return dirty;
    }

    private Transform GetRoot(WorldHierarchyRootType rootType)
    {
        Transform root = transform.Find($"@_{rootType}");
        if (root) return root;
        return transform;
    }

    private bool ArrangeAllRoots()
    {
        bool dirty = false;
        foreach (WorldHierarchyRootType rootType in Enum.GetValues(typeof(WorldHierarchyRootType)))
        {
            Transform root = transform.Find("@_" + rootType);
            if (root == null)
            {
                root = new GameObject("@_" + rootType).transform;
                root.parent = transform;
                dirty = true;
            }
        }

        List<WorldModuleDesignHelper> modules = GetComponentsInChildren<WorldModuleDesignHelper>().ToList();
        foreach (WorldModuleDesignHelper module in modules)
        {
            if (module.WorldModuleFeature.HasFlag(WorldModuleFeature.Wall))
            {
                Transform root = GetRoot(WorldHierarchyRootType.WallModulesRoot);
                if (!module.transform.IsChildOf(root))
                {
                    module.transform.parent = root;
                    dirty = true;
                }
            }
            else if (module.WorldModuleFeature.HasFlag(WorldModuleFeature.DeadZone))
            {
                Transform root = GetRoot(WorldHierarchyRootType.DeadZoneModulesRoot);
                if (!module.transform.IsChildOf(root))
                {
                    module.transform.parent = root;
                    dirty = true;
                }
            }
            else if (module.WorldModuleFeature.HasFlag(WorldModuleFeature.Ground))
            {
                Transform root = GetRoot(WorldHierarchyRootType.GroundModulesRoot);
                if (!module.transform.IsChildOf(root))
                {
                    module.transform.parent = root;
                    dirty = true;
                }
            }
            else
            {
                Transform root = GetRoot(WorldHierarchyRootType.ModulesRoot);
                if (!module.transform.IsChildOf(root))
                {
                    module.transform.parent = root;
                    dirty = true;
                }
            }
        }

        List<BornPointDesignHelper> bornPoints = GetComponentsInChildren<BornPointDesignHelper>().ToList();
        foreach (BornPointDesignHelper bornPoint in bornPoints)
        {
            WorldModuleDesignHelper module = bornPoint.transform.GetComponentInParent<WorldModuleDesignHelper>();
            if (module) continue;
            Transform root = GetRoot(WorldHierarchyRootType.WorldBornPointsRoot);
            if (!bornPoint.transform.IsChildOf(root))
            {
                bornPoint.transform.parent = root;
                dirty = true;
            }
        }

        List<WorldCameraPOI> pois = GetComponentsInChildren<WorldCameraPOI>().ToList();
        foreach (WorldCameraPOI poi in pois)
        {
            Transform root = GetRoot(WorldHierarchyRootType.WorldCameraPOIsRoot);
            if (!poi.transform.IsChildOf(root))
            {
                poi.transform.parent = root;
                dirty = true;
            }
        }

        List<LevelTriggerBase> triggers = GetComponentsInChildren<LevelTriggerBase>().ToList();
        foreach (LevelTriggerBase trigger in triggers)
        {
            WorldModuleDesignHelper module = trigger.transform.GetComponentInParent<WorldModuleDesignHelper>();
            if (module) continue;
            Transform root = GetRoot(WorldHierarchyRootType.WorldLevelTriggersRoot);
            if (!trigger.transform.IsChildOf(root))
            {
                trigger.transform.parent = root;
                dirty = true;
            }

            trigger.RefreshIsUnderWorldOrModuleBoxesRoot();
        }

        List<Box> boxes = GetComponentsInChildren<Box>().ToList();
        foreach (Box box in boxes)
        {
            WorldModuleDesignHelper module = box.transform.GetComponentInParent<WorldModuleDesignHelper>();
            if (module) continue;
            Transform worldSpecialBoxesRoot = GetRoot(WorldHierarchyRootType.WorldSpecialBoxesRoot);
            if (!box.transform.IsChildOf(worldSpecialBoxesRoot))
            {
                box.transform.parent = worldSpecialBoxesRoot;
                dirty = true;
            }
        }

        return dirty;
    }

    public bool RenameBoxTypeName(string srcBoxName, string targetBoxName, StringBuilder info)
    {
        bool isDirty = false;
        StringBuilder localInfo = new StringBuilder();
        localInfo.Append($"------------ WorldStart: {name}\n");
        List<LevelTriggerBase> triggers = GetComponentsInChildren<LevelTriggerBase>().ToList();
        foreach (LevelTriggerBase trigger in triggers)
        {
            if (trigger.transform.HasAncestorName($"@_{WorldModuleHierarchyRootType.WorldModuleLevelTriggersRoot}")) continue;
            WorldModuleDesignHelper module = trigger.transform.GetComponentInParent<WorldModuleDesignHelper>();
            if (module) continue;
            isDirty |= trigger.RenameBoxTypeName(srcBoxName, targetBoxName, localInfo);
        }

        List<Box> boxes = GetComponentsInChildren<Box>().ToList();
        foreach (Box box in boxes)
        {
            if (box.RequireSerializeFunctionIntoWorld)
            {
                isDirty |= box.RenameBoxTypeName(srcBoxName, targetBoxName, localInfo, false, true);
            }
            else
            {
                WorldModuleDesignHelper module = box.transform.GetComponentInParent<WorldModuleDesignHelper>();
                if (module) continue;
                isDirty |= box.RenameBoxTypeName(srcBoxName, targetBoxName, localInfo, false, true);
            }
        }

        localInfo.Append($"WorldEnd: {name} ------------\n");
        if (isDirty) info.Append(localInfo);
        return isDirty;
    }

    public bool DeleteBoxTypeName(string srcBoxName, StringBuilder info)
    {
        bool isDirty = false;
        StringBuilder localInfo = new StringBuilder();
        localInfo.Append($"------------ WorldStart: {name}\n");
        List<LevelTriggerBase> triggers = GetComponentsInChildren<LevelTriggerBase>().ToList();
        foreach (LevelTriggerBase trigger in triggers)
        {
            if (trigger.transform.HasAncestorName($"@_{WorldModuleHierarchyRootType.WorldModuleLevelTriggersRoot}")) continue;
            WorldModuleDesignHelper module = trigger.transform.GetComponentInParent<WorldModuleDesignHelper>();
            if (module) continue;
            isDirty |= trigger.DeleteBoxTypeName(srcBoxName, localInfo);
        }

        List<Box> boxes = GetComponentsInChildren<Box>().ToList();
        foreach (Box box in boxes)
        {
            if (box.RequireSerializeFunctionIntoWorld)
            {
                isDirty |= box.DeleteBoxTypeName(srcBoxName, localInfo, false, true);
            }
            else
            {
                WorldModuleDesignHelper module = box.transform.GetComponentInParent<WorldModuleDesignHelper>();
                if (module) continue;
                isDirty |= box.DeleteBoxTypeName(srcBoxName, localInfo, false, true);
            }
        }

        localInfo.Append($"WorldEnd: {name} ------------\n");
        if (isDirty) info.Append(localInfo);
        return isDirty;
    }
#endif
}

public enum WorldHierarchyRootType
{
    ModulesRoot = 0,
    WallModulesRoot = 1,
    DeadZoneModulesRoot = 2,
    GroundModulesRoot = 3,
    WorldBornPointsRoot = 4,
    WorldCameraPOIsRoot = 5,
    WorldSpecialBoxesRoot = 6,
    WorldLevelTriggersRoot = 7,
}
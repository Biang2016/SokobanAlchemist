using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BiangStudio;
using BiangStudio.GameDataFormat.Grid;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;

#endif

[ExecuteInEditMode]
public class WorldDesignHelper : MonoBehaviour
{
    public WorldFeature WorldFeature;

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

            // Box ExtraSerializeData
            List<Box> boxes = module.transform.GetComponentsInChildren<Box>().ToList();
            foreach (Box box in boxes)
            {
                GridPos3D boxModuleGP = GridPos3D.GetGridPosByLocalTrans(box.transform, 1);
                if (box.RequireSerializeFunctionIntoWorld)
                {
                    Box.BoxExtraSerializeData data = box.GetBoxExtraSerializeDataForWorld();
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
            worldData.WorldBornPointGroupData.BornPoints.Add(data);
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
            if (!ConfigManager.LevelTriggerTypeDefineDict.TypeIndexDict.ContainsKey(levelTriggerPrefab.name))
            {
                int a = 0;
            }
            else
            {
                ushort levelTriggerTypeIndex = ConfigManager.LevelTriggerTypeDefineDict.TypeIndexDict[levelTriggerPrefab.name];
                data.LevelTriggerTypeIndex = levelTriggerTypeIndex;
                worldData.WorldLevelTriggerGroupData.TriggerDataList.Add(data);
            }
        }

        List<Box> worldBoxes = GetRoot(WorldHierarchyRootType.WorldSpecialBoxesRoot).GetComponentsInChildren<Box>().ToList();
        foreach (Box worldBox in worldBoxes)
        {
            Box.WorldSpecialBoxData worldSpecialBoxData = worldBox.GetBoxSerializeInWorldData();
            GridPos3D gp = GridPos3D.GetGridPosByLocalTrans(worldBox.transform, 1);
            gp -= zeroPoint * WorldModule.MODULE_SIZE;
            GameObject boxPrefab = PrefabUtility.GetCorrespondingObjectFromSource(worldBox.gameObject);
            ushort boxTypeIndex = ConfigManager.BoxTypeDefineDict.TypeIndexDict[boxPrefab.name];
            worldSpecialBoxData.WorldGP = gp;
            worldSpecialBoxData.BoxTypeIndex = boxTypeIndex;
            worldData.WorldSpecialBoxDataList.Add(worldSpecialBoxData);
        }

        return worldData;
    }

    [HideInPlayMode]
    [HideInPrefabAssets]
    [Button("一键整理", ButtonSizes.Large)]
    [GUIColor(0f, 1f, 0)]
    public void SortWorldButton()
    {
        SortWorld();
    }

    public bool SortWorld()
    {
        bool dirty = false;
        dirty |= FormatAllWorldModuleName_Editor();
        dirty |= FormatAllBornPointName_Editor();
        dirty |= FormatAllWorldCameraPOIName_Editor();
        dirty |= FormatAllLevelTriggerName_Editor();
        dirty |= FormatAllBoxName_Editor();
        dirty |= ArrangeAllRoots();
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
            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(bp.gameObject);
            if (bp.name != prefab.name)
            {
                bp.name = prefab.name;
                dirty = true;
            }
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

    public bool DeleteBoxTypeName(string srcBoxName, StringBuilder info)
    {
        bool isDirty = false;
        StringBuilder localInfo = new StringBuilder();
        localInfo.Append($"WorldStart: {name} ---\n");
        List<LevelTriggerBase> triggers = GetComponentsInChildren<LevelTriggerBase>().ToList();
        foreach (LevelTriggerBase trigger in triggers)
        {
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

        localInfo.Append($"WorldEnd: {name} ---\n");
        info.Append(localInfo.ToString());
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
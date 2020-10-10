using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
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

        worldData.WorldName = name;

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
            worldData.WorldLevelTriggerGroupData.TriggerDataList.Add(data);
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
    public void SortWorld()
    {
        FormatAllWorldModuleName_Editor();
        ArrangeAllRoots();
    }

    private void FormatAllWorldModuleName_Editor()
    {
        List<WorldModuleDesignHelper> modules = GetComponentsInChildren<WorldModuleDesignHelper>().ToList();
        foreach (WorldModuleDesignHelper module in modules)
        {
            GameObject modulePrefab = PrefabUtility.GetCorrespondingObjectFromSource(module.gameObject);
            module.name = modulePrefab.name;
        }
    }

    private Transform GetRoot(WorldHierarchyRootType rootType)
    {
        Transform root = transform.Find($"@_{rootType}");
        if (root) return root;
        return transform;
    }

    private void ArrangeAllRoots()
    {
        foreach (WorldHierarchyRootType rootType in Enum.GetValues(typeof(WorldHierarchyRootType)))
        {
            Transform root = transform.Find("@_" + rootType);
            if (root == null)
            {
                root = new GameObject("@_" + rootType).transform;
                root.parent = transform;
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
                }
            }
            else if (module.WorldModuleFeature.HasFlag(WorldModuleFeature.DeadZone))
            {
                Transform root = GetRoot(WorldHierarchyRootType.DeadZoneModulesRoot);
                if (!module.transform.IsChildOf(root))
                {
                    module.transform.parent = root;
                }
            }
            else if (module.WorldModuleFeature.HasFlag(WorldModuleFeature.Ground))
            {
                Transform root = GetRoot(WorldHierarchyRootType.GroundModulesRoot);
                if (!module.transform.IsChildOf(root))
                {
                    module.transform.parent = root;
                }
            }
            else
            {
                Transform root = GetRoot(WorldHierarchyRootType.ModulesRoot);
                if (!module.transform.IsChildOf(root))
                {
                    module.transform.parent = root;
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
            }
        }

        List<WorldCameraPOI> pois = GetComponentsInChildren<WorldCameraPOI>().ToList();
        foreach (WorldCameraPOI poi in pois)
        {
            Transform root = GetRoot(WorldHierarchyRootType.WorldCameraPOIsRoot);
            if (!poi.transform.IsChildOf(root))
            {
                poi.transform.parent = root;
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
            }
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
            }
        }
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
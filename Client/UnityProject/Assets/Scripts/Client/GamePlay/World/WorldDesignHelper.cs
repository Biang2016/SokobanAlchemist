using System;
using System.Collections.Generic;
using System.Linq;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

[ExecuteInEditMode]
public class WorldDesignHelper : MonoBehaviour
{
    [LabelText("世界特性")]
    public WorldFeature WorldFeature;

    [LabelText("默认出生点花名")]
    public string DefaultWorldActorBornPointAlias;

    [BoxGroup("开局数据")]
    [LabelText("是否使用特殊开局数据")]
    public bool UseSpecialPlayerEnterESPS = false;

    [BoxGroup("开局数据")]
    [LabelText("玩家特殊开局数据")]
    [ShowIf("UseSpecialPlayerEnterESPS")]
    public EntityStatPropSet Raw_PlayerEnterESPS = new EntityStatPropSet(); // 干数据

    [BoxGroup("相机配置")]
    [LabelText("是否使用特殊相机配置")]
    public bool UseSpecialCameraConfig = false;

    [BoxGroup("相机配置")]
    [LabelText("特殊相机配置")]
    [ShowIf("UseSpecialCameraConfig")]
    public FieldCamera.CameraConfigData CameraConfigData = new FieldCamera.CameraConfigData();

    [BoxGroup("相机配置")]
    [LabelText("天空盒材质类型")]
    public TypeSelectHelper SkyBoxType = new TypeSelectHelper {TypeDefineType = TypeDefineType.SkyBox};

    [BoxGroup("相机配置")]
    [LabelText("后处理配置类型")]
    public TypeSelectHelper PostProcessingProfileType = new TypeSelectHelper {TypeDefineType = TypeDefineType.PostProcessingProfile};

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
        worldData.UseSpecialPlayerEnterESPS = UseSpecialPlayerEnterESPS;
        Raw_PlayerEnterESPS.ApplyDataTo(worldData.Raw_PlayerEnterESPS);
        CameraConfigData.ApplyTo(worldData.CameraConfigData, true);
        worldData.SkyBoxType = SkyBoxType.Clone();
        worldData.PostProcessingProfileType = PostProcessingProfileType.Clone();
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
            ushort worldModuleTypeIndex = ConfigManager.TypeDefineConfigs[TypeDefineType.WorldModule].TypeIndexDict[worldModulePrefab.name];
            worldData.ModuleMatrix[gp.x, gp.y, gp.z] = worldModuleTypeIndex;
            worldData.WorldModuleGPOrder.Add(gp);
        }


        return worldData;
    }

    public bool SortWorld()
    {
        bool dirty = false;
        dirty |= ArrangeAllRoots();
        dirty |= FormatAllWorldModuleName_Editor();
        dirty |= FormatAllBornPointName_Editor();
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

        return dirty;
    }

#endif
}

public enum WorldHierarchyRootType
{
    ModulesRoot = 0,
    WallModulesRoot = 1,
    DeadZoneModulesRoot = 2,
    GroundModulesRoot = 3,
}
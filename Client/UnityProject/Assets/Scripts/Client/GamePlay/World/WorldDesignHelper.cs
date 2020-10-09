using UnityEngine;
using System.Collections.Generic;
using System.Linq;
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
        }

        worldData.WorldName = name;

        List<BornPointDesignHelper> bornPoints = GetComponentsInChildren<BornPointDesignHelper>().ToList();
        foreach (BornPointDesignHelper bp in bornPoints)
        {
            GridPos3D gp = GridPos3D.GetGridPosByLocalTrans(bp.transform, 1);
            gp -= zeroPoint * WorldModule.MODULE_SIZE;
            bp.BornPointData.GridPos3D = gp;
            worldData.WorldActorData.BornPoints.Add(bp.BornPointData);
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
            GridPos3D gp = GridPos3D.GetGridPosByLocalTrans(trigger.transform, 1);
            gp -= zeroPoint * WorldModule.MODULE_SIZE;
            trigger.TriggerData.GridPos = gp;
            worldData.WorldLevelTriggerData.TriggerDataList.Add(trigger.TriggerData);
        }

        return worldData;
    }

    [HideInPlayMode]
    [HideInPrefabAssets]
    [Button("所有世界模组命名重置为Prefab名称", ButtonSizes.Large)]
    [GUIColor(0f, 1f, 0f)]
    private void FormatAllWorldModuleName_Editor()
    {
        List<WorldModuleDesignHelper> modules = GetComponentsInChildren<WorldModuleDesignHelper>().ToList();
        foreach (WorldModuleDesignHelper module in modules)
        {
            GameObject modulePrefab = PrefabUtility.GetCorrespondingObjectFromSource(module.gameObject);
            module.name = modulePrefab.name;
        }
    }
#endif
}
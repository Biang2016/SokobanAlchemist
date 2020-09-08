﻿using System.Collections.Generic;
using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ObjectPool;
using UnityEngine;

public class World : PoolObject
{
    public const int WORLD_SIZE = 16;
    public const int WORLD_HEIGHT = 8;

    public WorldData WorldData;

    [HideInInspector]
    public WorldModule[,,] WorldModuleMatrix = new WorldModule[WORLD_SIZE, WORLD_HEIGHT, WORLD_SIZE];

    public bool[,,] WorldDeadZoneTriggerMatrix = new bool[WORLD_SIZE, WORLD_HEIGHT, WORLD_SIZE];

    private List<WorldCameraPOI> POIs = new List<WorldCameraPOI>();

    public void Initialize(WorldData worldData)
    {
        WorldData = worldData;
        for (int x = 0; x < worldData.ModuleMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < worldData.ModuleMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < worldData.ModuleMatrix.GetLength(2); z++)
                {
                    byte worldModuleTypeIndex = worldData.ModuleMatrix[x, y, z];
                    if (worldModuleTypeIndex != 0)
                    {
                        WorldModule wm = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.WorldModule].AllocateGameObject<WorldModule>(transform);
                        wm.name = $"WorldModule({x}, {y}, {z})";
                        WorldModuleData data = ConfigManager.GetWorldModuleDataConfig(worldModuleTypeIndex);
                        WorldModuleMatrix[x, y, z] = wm;
                        GridPos3D gp = new GridPos3D(x, y, z);
                        GridPos3D.ApplyGridPosToLocalTrans(gp, wm.transform, WorldModule.MODULE_SIZE);
                        wm.Initialize(data, gp, this);
                    }
                }
            }
        }

        foreach (GridPos3D gp in WorldManager.Instance.CurrentWorld.WorldData.WorldCameraPOIData.POIs)
        {
            WorldCameraPOI poi = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.WorldCameraPOI].AllocateGameObject<WorldCameraPOI>(transform);
            GridPos3D.ApplyGridPosToLocalTrans(gp, poi.transform, 1);
            POIs.Add(poi);
            ClientGameManager.Instance.BattleMessenger.Broadcast((uint) Enum_Events.OnWorldCameraPOILoaded, poi);
        }
    }

    #region MoveBox Calculators

    public Box GetBoxByGridPosition(GridPos3D gp, out WorldModule module, out GridPos3D localGP)
    {
        module = GetModuleByGridPosition(gp);
        if (module != null)
        {
            localGP = gp - module.ModuleGP * WorldModule.MODULE_SIZE;
            return module.BoxMatrix[localGP.x, localGP.y, localGP.z];
        }
        else
        {
            localGP = GridPos3D.Zero;
            return null;
        }
    }

    public WorldModule GetModuleByGridPosition(GridPos3D gp)
    {
        GridPos3D gp_module = new GridPos3D(gp.x / WorldModule.MODULE_SIZE, gp.y / WorldModule.MODULE_SIZE, gp.z / WorldModule.MODULE_SIZE);
        if (gp_module.x >= 0 && gp_module.x < WORLD_SIZE && gp_module.y >= 0 && gp_module.y < WORLD_HEIGHT && gp_module.z >= 0 && gp_module.z < WORLD_SIZE)
        {
            return WorldModuleMatrix[gp_module.x, gp_module.y, gp_module.z];
        }
        else
        {
            return null;
        }
    }

    public void MoveBox(GridPos3D srcGP, GridPos3D targetGP, Box.States sucState)
    {
        Box box_src = GetBoxByGridPosition(srcGP, out WorldModule module_src, out GridPos3D localGP_src);
        Box box_target = GetBoxByGridPosition(targetGP, out WorldModule module_target, out GridPos3D localGP_target);
        if (module_src == null || module_target == null || box_src == null || box_target != null) return;
        box_src.State = sucState;
        module_src.BoxMatrix[localGP_src.x, localGP_src.y, localGP_src.z] = null;
        module_target.BoxMatrix[localGP_target.x, localGP_target.y, localGP_target.z] = box_src;
        CheckDropAbove(box_src);
        box_src.Initialize(localGP_target, module_target, box_src.FinalWeight);
    }

    public void RemoveBox(Box box)
    {
        if (box.WorldModule)
        {
            WorldModule module = box.WorldModule;
            GridPos3D localGridPos3D = box.LocalGridPos3D;
            if (module.BoxMatrix[localGridPos3D.x, localGridPos3D.y, localGridPos3D.z] == box)
            {
                module.BoxMatrix[localGridPos3D.x, localGridPos3D.y, localGridPos3D.z] = null;
                CheckDropAbove(box);
                box.WorldModule = null;
            }
        }
    }

    public void BoxReturnToWorldFromPhysics(Box box)
    {
        GridPos3D gp = GridPos3D.GetGridPosByTrans(box.transform, 1);
        WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByGridPosition(gp);
        GridPos3D localGP = gp - module.ModuleGP * WorldModule.MODULE_SIZE;
        Box existBox = module.BoxMatrix[localGP.x, localGP.y, localGP.z];
        if (existBox != null && existBox != box)
        {
            string logStr = $"{box.name}想要前往的位置{localGP}非空, 存在{existBox.name}";
            Debug.LogError(logStr);
            return;
        }

        if (existBox == null)
        {
            module.BoxMatrix[localGP.x, localGP.y, localGP.z] = box;
            box.Initialize(localGP, module, 0.3f);
        }
    }

    public void CheckDropSelf(Box box)
    {
        if (box && box.Droppable())
        {
            WorldModule module = box.WorldModule;
            GridPos3D localGridPos3D = box.LocalGridPos3D;
            if (localGridPos3D.y > 0)
            {
                Box boxBeneath = module.BoxMatrix[localGridPos3D.x, localGridPos3D.y - 1, localGridPos3D.z];
                if (boxBeneath == null)
                {
                    GridPos3D localGP = new GridPos3D(localGridPos3D.x, localGridPos3D.y - 1, localGridPos3D.z);
                    box.WorldModule.BoxMatrix[box.LocalGridPos3D.x, box.LocalGridPos3D.y, box.LocalGridPos3D.z] = null;
                    module.BoxMatrix[localGridPos3D.x, localGridPos3D.y - 1, localGridPos3D.z] = box;
                    CheckDropAbove(box);
                    box.Initialize(localGP, module, 0.1f);
                }
            }
            else
            {
                if (module.ModuleGP.y > 0)
                {
                    WorldModule moduleBeneath = WorldModuleMatrix[module.ModuleGP.x, module.ModuleGP.y - 1, module.ModuleGP.z];
                    if (moduleBeneath)
                    {
                        Box boxBeneath = moduleBeneath.BoxMatrix[localGridPos3D.x, WorldModule.MODULE_SIZE - 1, localGridPos3D.z];
                        if (boxBeneath == null)
                        {
                            GridPos3D localGP = new GridPos3D(localGridPos3D.x, WorldModule.MODULE_SIZE - 1, localGridPos3D.z);
                            box.WorldModule.BoxMatrix[box.LocalGridPos3D.x, box.LocalGridPos3D.y, box.LocalGridPos3D.z] = null;
                            moduleBeneath.BoxMatrix[localGridPos3D.x, WorldModule.MODULE_SIZE - 1, localGridPos3D.z] = box;
                            CheckDropAbove(box);
                            box.Initialize(localGP, moduleBeneath, 0.3f);
                        }
                    }
                }
            }
        }
    }

    public void CheckDropAbove(Box box)
    {
        WorldModule module = box.WorldModule;
        GridPos3D localGridPos3D = box.LocalGridPos3D;
        if (localGridPos3D.y < WorldModule.MODULE_SIZE - 1)
        {
            Box boxAbove = module.BoxMatrix[localGridPos3D.x, localGridPos3D.y + 1, localGridPos3D.z];
            CheckDropSelf(boxAbove);
        }
        else
        {
            if (module.ModuleGP.y < WORLD_HEIGHT - 1)
            {
                WorldModule moduleAbove = WorldModuleMatrix[module.ModuleGP.x, module.ModuleGP.y + 1, module.ModuleGP.z];
                if (moduleAbove)
                {
                    Box boxAbove = moduleAbove.BoxMatrix[localGridPos3D.x, 0, localGridPos3D.z];
                    if (boxAbove)
                    {
                        CheckDropSelf(boxAbove);
                    }
                }
            }
        }
    }

    public enum SearchRangeShape
    {
        Circle,
        Square,
    }

    public List<Box> SearchBoxInRange(GridPos3D center, int radius, string boxTypeName, SearchRangeShape shape)
    {
        List<Box> res = new List<Box>();
        if (boxTypeName == "None") return res;

        GetBoxByGridPosition(center, out WorldModule module, out GridPos3D localGP);
        if (module)
        {
            byte boxTypeIndex = ConfigManager.GetBoxTypeIndex(boxTypeName);
            if (boxTypeIndex == 0 && boxTypeName != "All")
            {
                return res;
            }

            for (int x = -radius; x <= radius; x++)
            {
                for (int z = -radius; z <= radius; z++)
                {
                    if (shape == SearchRangeShape.Circle)
                    {
                        if (x * x + z * z > radius * radius) continue;
                    }
                    else if (shape == SearchRangeShape.Square)
                    {
                    }

                    GridPos3D gp = center + new GridPos3D(x, 0, z);
                    Box box = GetBoxByGridPosition(gp, out WorldModule tarModule, out GridPos3D _);
                    if (box != null)
                    {
                        if (boxTypeName == "All" || box.BoxTypeIndex == boxTypeIndex)
                        {
                            res.Add(box);
                        }
                    }
                }
            }
        }

        return res;
    }

    #endregion
}
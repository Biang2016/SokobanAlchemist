using System.Collections.Generic;
using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ObjectPool;
using UnityEngine;

public class World : PoolObject
{
    public const int WORLD_SIZE = 16;
    public const int WORLD_HEIGHT = 8;

    public WorldData WorldData;
    public WorldModule[,,] WorldModuleMatrix = new WorldModule[WORLD_SIZE, WORLD_HEIGHT, WORLD_SIZE];

    public void Initialize(WorldData worldData)
    {
        WorldData = worldData;
        for (int x = 0; x < worldData.ModuleMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < worldData.ModuleMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < worldData.ModuleMatrix.GetLength(2); z++)
                {
                    WorldModuleType wmType = (WorldModuleType) worldData.ModuleMatrix[x, y, z];
                    if (wmType != WorldModuleType.None)
                    {
                        WorldModule wm = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.WorldModule].AllocateGameObject<WorldModule>(transform);
                        wm.name = $"WorldModule({x}, {y}, {z})";
                        WorldModuleData data = ConfigManager.Instance.GetWorldModuleDataConfig(wmType);
                        WorldModuleMatrix[x, y, z] = wm;
                        GridPos3D gp = new GridPos3D(x, y, z);
                        GridPos3D.ApplyGridPosToLocalTrans(gp, wm.transform, WorldModule.MODULE_SIZE);
                        wm.Initialize(data, gp, this);
                    }
                }
            }
        }
    }

    #region MoveBox Calculators

    public BoxBase GetBoxByGridPosition(GridPos3D gp, out WorldModule module, out GridPos3D localGP)
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

    public void MoveBox(GridPos3D srcGP, GridPos3D targetGP, BoxBase.States sucState)
    {
        BoxBase box_src = GetBoxByGridPosition(srcGP, out WorldModule module_src, out GridPos3D localGP_src);
        BoxBase box_target = GetBoxByGridPosition(targetGP, out WorldModule module_target, out GridPos3D localGP_target);
        if (module_src == null || module_target == null || box_src == null || box_target != null) return;
        box_src.State = sucState;
        module_src.BoxMatrix[localGP_src.x, localGP_src.y, localGP_src.z] = null;
        module_target.BoxMatrix[localGP_target.x, localGP_target.y, localGP_target.z] = box_src;
        box_src.Initialize(localGP_target, module_target, true);
    }

    #endregion
}

public enum WorldType
{
    None = 0,
    SampleWorld = 1,

    Max = 255
}
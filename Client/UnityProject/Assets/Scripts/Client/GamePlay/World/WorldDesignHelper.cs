using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BiangStudio.GameDataFormat.Grid;

[ExecuteInEditMode]
public class WorldDesignHelper : MonoBehaviour
{
    public WorldType WorldType;

    public WorldData ExportWorldData()
    {
        List<WorldModuleDesignHelper> modules = GetComponentsInChildren<WorldModuleDesignHelper>().ToList();

        WorldData worldData = new WorldData();
        foreach (WorldModuleDesignHelper module in modules)
        {
            GridPos3D gp = GridPos3D.GetGridPosByLocalTrans(module.transform, 16);
            worldData.ModuleMatrix[gp.x, gp.y, gp.z] = (byte) module.WorldModuleType;
        }

        worldData.WorldType = WorldType;
        return worldData;
    }
}
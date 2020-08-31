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
        GridPos3D zeroPoint = GridPos3D.Zero;
        foreach (WorldModuleDesignHelper module in modules)
        {
            GridPos3D gp = GridPos3D.GetGridPosByLocalTrans(module.transform, WorldModule.MODULE_SIZE);
            zeroPoint.x = Mathf.Min(gp.x, zeroPoint.x);
            zeroPoint.y = Mathf.Min(gp.y, zeroPoint.y);
            zeroPoint.z = Mathf.Min(gp.z, zeroPoint.z);
        }

        WorldData worldData = new WorldData();
        foreach (WorldModuleDesignHelper module in modules)
        {
            GridPos3D gp = GridPos3D.GetGridPosByLocalTrans(module.transform, WorldModule.MODULE_SIZE);
            gp -= zeroPoint;
            if (gp.x >= World.WORLD_SIZE || gp.y >= World.WORLD_HEIGHT || gp.z >= World.WORLD_SIZE)
            {
                Debug.LogError($"世界大小超过限制，存在模组坐标为{gp}，已忽略该模组");
                continue;
            }

            worldData.ModuleMatrix[gp.x, gp.y, gp.z] = (byte) module.WorldModuleType;
        }

        worldData.WorldType = WorldType;

        List<BornPoint> bornPoints = GetComponentsInChildren<BornPoint>().ToList();
        foreach (BornPoint bp in bornPoints)
        {
            GridPos3D gp = GridPos3D.GetGridPosByLocalTrans(bp.transform, 1);
            gp -= zeroPoint * WorldModule.MODULE_SIZE;
            if (bp.BornPointType == BornPointType.Player)
            {
                switch (bp.PlayerNumber)
                {
                    case PlayerNumber.Player1:
                    {
                        worldData.WorldActorData.Player1BornPoint = gp;
                        break;
                    }
                    case PlayerNumber.Player2:
                    {
                        worldData.WorldActorData.Player2BornPoint = gp;
                        break;
                    }
                }
            }
        }

        return worldData;
    }
}
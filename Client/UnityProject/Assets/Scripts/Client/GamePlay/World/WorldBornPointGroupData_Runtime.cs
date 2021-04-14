using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using UnityEngine;

public class WorldBornPointGroupData_Runtime
{
    private Dictionary<GridPos3D, List<BornPointData>> ActorBP_ModuleDict = new Dictionary<GridPos3D, List<BornPointData>>(512); // Key: ModuleGP, All Data
    private Dictionary<uint, BornPointData> BornPointDataGUIDDict = new Dictionary<uint, BornPointData>(); // Key: BornPointDataGUID

    // Current loaded BPs:
    public Dictionary<string, BornPointData> PlayerBornPointDataAliasDict = new Dictionary<string, BornPointData>(); // 带花名的玩家出生点词典

    public void Init()
    {
        PlayerBornPointDataAliasDict.Clear();
    }

    public List<BornPointData> TryLoadModuleBPData(GridPos3D moduleGP)
    {
        if (ActorBP_ModuleDict.TryGetValue(moduleGP, out List<BornPointData> data))
        {
            return data;
        }
        else
        {
            return null;
        }
    }

    public void SetDefaultPlayerBP_OpenWorld(BornPointData bp)
    {
        GridPos3D moduleGP = WorldManager.Instance.CurrentWorld.GetModuleGPByWorldGP(bp.WorldGP);
        if (!ActorBP_ModuleDict.ContainsKey(moduleGP))
        {
            ActorBP_ModuleDict.Add(moduleGP, new List<BornPointData>());
        }

        ActorBP_ModuleDict[moduleGP].Add(bp);

        if (!BornPointDataGUIDDict.ContainsKey(bp.GUID))
        {
            BornPointDataGUIDDict.Add(bp.GUID, bp);
        }

        PlayerBornPointDataAliasDict.Add(bp.BornPointAlias, bp);
    }

    public void Init_LoadModuleData(GridPos3D moduleGP, WorldModuleData moduleData)
    {
        if (!ActorBP_ModuleDict.ContainsKey(moduleGP))
        {
            ActorBP_ModuleDict.Add(moduleGP, new List<BornPointData>());
        }

        foreach (KeyValuePair<string, BornPointData> kv in moduleData.WorldModuleBornPointGroupData.PlayerBornPoints)
        {
            BornPointData playerBP = (BornPointData) kv.Value.Clone();
            playerBP.InitGUID();
            playerBP.WorldGP = playerBP.LocalGP + moduleGP * WorldModule.MODULE_SIZE;
            ActorBP_ModuleDict[moduleGP].Add(playerBP);
            if (!BornPointDataGUIDDict.ContainsKey(playerBP.GUID))
            {
                BornPointDataGUIDDict.Add(playerBP.GUID, playerBP);
            }
        }
    }

    public void UnInit_UnloadModuleData(GridPos3D moduleGP)
    {
        List<BornPointData> moduleBPs = TryLoadModuleBPData(moduleGP);
        if (moduleBPs != null)
        {
            foreach (BornPointData bp in moduleBPs)
            {
                PlayerBornPointDataAliasDict.Remove(bp.BornPointAlias);
                BornPointDataGUIDDict.Remove(bp.GUID);
            }

            ActorBP_ModuleDict.Remove(moduleGP);
        }
    }

    public void Dynamic_UnloadModuleData(GridPos3D moduleGP)
    {
        List<BornPointData> moduleBPs = TryLoadModuleBPData(moduleGP);
        if (moduleBPs != null)
        {
            foreach (BornPointData bp in moduleBPs)
            {
                PlayerBornPointDataAliasDict.Remove(bp.BornPointAlias);
            }
        }
    }

    private void AddPlayerBP(BornPointData bp)
    {
        if (!PlayerBornPointDataAliasDict.ContainsKey(bp.BornPointAlias))
        {
            PlayerBornPointDataAliasDict.Add(bp.BornPointAlias, bp);
        }
        else
        {
            Debug.Log($"主角出生点花名重复: {bp.BornPointAlias}");
        }
    }

    public BornPointData GetBornPointDataByGUID(uint bornPointDataGUID)
    {
        if (BornPointDataGUIDDict.TryGetValue(bornPointDataGUID, out BornPointData bp))
        {
            return bp;
        }

        return null;
    }
}
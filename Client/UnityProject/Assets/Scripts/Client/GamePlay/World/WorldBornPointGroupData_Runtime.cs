using System.Collections;
using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using UnityEngine;

public class WorldBornPointGroupData_Runtime
{
    private Dictionary<GridPos3D, List<BornPointData>> ActorBP_ModuleDict = new Dictionary<GridPos3D, List<BornPointData>>(512); // All Data

    // Current loaded BPs:
    public Dictionary<string, BornPointData> PlayerBornPointDataAliasDict = new Dictionary<string, BornPointData>(); // 带花名的玩家出生点词典
    public Dictionary<string, BornPointData> EnemyBornPointDataAliasDict = new Dictionary<string, BornPointData>(); // 带花名的敌人出生点词典

    public void InitTempData()
    {
        PlayerBornPointDataAliasDict.Clear();
        EnemyBornPointDataAliasDict.Clear();
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
            playerBP.WorldGP = playerBP.LocalGP + moduleGP * WorldModule.MODULE_SIZE;
            ActorBP_ModuleDict[moduleGP].Add(playerBP);
        }

        foreach (BornPointData bp in moduleData.WorldModuleBornPointGroupData.EnemyBornPoints)
        {
            BornPointData enemyBP = (BornPointData) bp.Clone();
            enemyBP.WorldGP = enemyBP.LocalGP + moduleGP * WorldModule.MODULE_SIZE;
            ActorBP_ModuleDict[moduleGP].Add(enemyBP);
        }
    }

    public void UnInit_UnloadModuleData(GridPos3D moduleGP)
    {
        List<BornPointData> moduleBPs = TryLoadModuleBPData(moduleGP);
        if (moduleBPs != null)
        {
            foreach (BornPointData bp in moduleBPs)
            {
                if (bp.ActorCategory == ActorCategory.Player)
                {
                    PlayerBornPointDataAliasDict.Remove(bp.BornPointAlias);
                }
                else
                {
                    EnemyBornPointDataAliasDict.Remove(bp.BornPointAlias);
                }
            }

            ActorBP_ModuleDict.Remove(moduleGP);
        }
    }

    public IEnumerator Dynamic_LoadModuleData(GridPos3D moduleGP)
    {
        List<BornPointData> moduleBPs = TryLoadModuleBPData(moduleGP);
        if (moduleBPs != null)
        {
            foreach (BornPointData bp in moduleBPs)
            {
                if (bp.ActorCategory == ActorCategory.Creature)
                {
                    AddEnemyBP(bp);
                }
            }

            yield return BattleManager.Instance.CreateActorByBornPointDataList(moduleBPs);
        }
    }

    public void Dynamic_UnloadModuleData(GridPos3D moduleGP)
    {
        List<BornPointData> moduleBPs = TryLoadModuleBPData(moduleGP);
        if (moduleBPs != null)
        {
            foreach (BornPointData bp in moduleBPs)
            {
                if (bp.ActorCategory == ActorCategory.Player)
                {
                    PlayerBornPointDataAliasDict.Remove(bp.BornPointAlias);
                }
                else
                {
                    EnemyBornPointDataAliasDict.Remove(bp.BornPointAlias);
                }
            }
        }

        BattleManager.Instance.DestroyActorByModuleGP(moduleGP);
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

    private void AddEnemyBP(BornPointData bp)
    {
        if (!string.IsNullOrEmpty(bp.BornPointAlias))
        {
            if (!EnemyBornPointDataAliasDict.ContainsKey(bp.BornPointAlias))
            {
                EnemyBornPointDataAliasDict.Add(bp.BornPointAlias, bp);
            }
            else
            {
                Debug.Log($"敌人出生点花名重复: {bp.BornPointAlias}");
            }
        }
    }
}
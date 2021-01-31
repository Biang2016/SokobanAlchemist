using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;

public class WorldBornPointGroupData_Runtime
{
    public Dictionary<string, BornPointData> PlayerBornPointDataAliasDict = new Dictionary<string, BornPointData>(); // 和模组数据整合后的带花名的玩家出生点词典
    public Dictionary<string, BornPointData> EnemyBornPointDataAliasDict = new Dictionary<string, BornPointData>(); // 和模组数据整合后的带花名的敌人出生点词典
    public List<BornPointData> AllEnemyBornPointDataList = new List<BornPointData>(); // 和模组数据整合后的敌人出生点列表

    public void InitTempData()
    {
        PlayerBornPointDataAliasDict.Clear();
        EnemyBornPointDataAliasDict.Clear();
        AllEnemyBornPointDataList.Clear();
    }

    public void AddModuleData(WorldModule module, GridPos3D worldModuleGP)
    {
        foreach (KeyValuePair<string, BornPointData> kv in module.WorldModuleData.WorldModuleBornPointGroupData.PlayerBornPoints)
        {
            BornPointData newData = (BornPointData) kv.Value.Clone();
            newData.WorldGP = module.LocalGPToWorldGP(newData.LocalGP);
            PlayerBornPointDataAliasDict.Add(kv.Key, newData);
        }

        foreach (BornPointData bp in module.WorldModuleData.WorldModuleBornPointGroupData.EnemyBornPoints)
        {
            BornPointData newData = (BornPointData) bp.Clone();
            newData.WorldGP = module.LocalGPToWorldGP(newData.LocalGP);
            AllEnemyBornPointDataList.Add(newData);
            if (!string.IsNullOrEmpty(newData.BornPointAlias))
            {
                EnemyBornPointDataAliasDict.Add(newData.BornPointAlias, newData);
            }
        }
    }
}
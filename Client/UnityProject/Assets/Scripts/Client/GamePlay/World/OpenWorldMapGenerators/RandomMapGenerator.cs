using System;
using BiangLibrary.GameDataFormat;
using BiangLibrary.GameDataFormat.Grid;

public class RandomMapGenerator : MapGenerator
{
    public RandomMapGenerator(OpenWorld.GenerateLayerData layerData, int width, int height, uint seed, GridPos leaveSpaceForPlayerBP) : base(layerData, width, height, seed, leaveSpaceForPlayerBP)
    {
    }

    public override void ApplyGeneratorToWorldModuleData(WorldModuleData moduleData, int module_x, int module_z)
    {
        base.ApplyGeneratorToWorldModuleData(moduleData, module_x, module_z);
        SRandom = new SRandom(Seed); // 确保每次写入时随机数序列都与最初相同

        for (int x = 0; x < WorldModule.MODULE_SIZE; x++)
        for (int z = 0; z < WorldModule.MODULE_SIZE; z++)
        {
            if (x == LeaveSpaceForPlayerBP.x && z == LeaveSpaceForPlayerBP.z) continue;
            bool isOverlapWithOtherActor = false;
            foreach (BornPointData enemyBornPoint in moduleData.WorldModuleBornPointGroupData.EnemyBornPoints)
            {
                if (enemyBornPoint.LocalGP.x == x && enemyBornPoint.LocalGP.z == z)
                {
                    isOverlapWithOtherActor = true;
                    break;
                }
            }

            if (isOverlapWithOtherActor) continue;

            bool genSuc = SRandom.Range(0, 1000) < GenerateLayerData.CountPerThousandGrid;
            if (genSuc)
            {
                ushort existedBoxTypeIndex = moduleData.RawBoxMatrix[x, 0, z];
                TryOverrideBoxInfoOnMap(moduleData, existedBoxTypeIndex, x, z, module_x, module_z);
            }
        }
    }
}
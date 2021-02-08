using System;
using BiangLibrary.GameDataFormat;
using BiangLibrary.GameDataFormat.Grid;

public class RandomMapGenerator : MapGenerator
{
    public RandomMapGenerator(OpenWorld.GenerateBoxLayerData boxLayerData, int width, int height, uint seed, GridPos leaveSpaceForPlayerBP) : base(boxLayerData, width, height, seed, leaveSpaceForPlayerBP)
    {
    }

    public override void WriteMapInfoIntoWorldModuleData(WorldModuleData moduleData, int module_x, int module_z)
    {
        base.WriteMapInfoIntoWorldModuleData(moduleData, module_x, module_z);
        SRandom = new SRandom(Seed); // 确保每次写入时随机数序列相同

        for (int x = 0; x < WorldModule.MODULE_SIZE; x++)
        for (int z = 0; z < WorldModule.MODULE_SIZE; z++)
        {
            if (x == LeaveSpaceForPlayerBP.x && z == LeaveSpaceForPlayerBP.z) continue;
            bool genSuc = SRandom.Range(0, 1000) < GenerateBoxLayerData.BoxCountPerThousandGrid;
            if (genSuc)
            {
                ushort existedBoxTypeIndex = moduleData.RawBoxMatrix[x, 0, z];
                TryOverrideBoxInfoOnMap(moduleData, existedBoxTypeIndex, x, z);
            }
        }
    }
}
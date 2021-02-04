using System;
using BiangLibrary.GameDataFormat;

public class RandomMapGenerator : MapGenerator
{
    public RandomMapGenerator(OpenWorld.GenerateBoxLayerData boxLayerData, int width, int height, uint seed) : base(boxLayerData, width, height, seed)
    {
    }

    public override void WriteMapInfoIntoWorldModuleData(WorldModuleData moduleData, int module_x, int module_z)
    {
        base.WriteMapInfoIntoWorldModuleData(moduleData, module_x, module_z);
        SRandom = new SRandom(Seed); // 确保每次写入时随机数序列相同

        for (int x = 0; x < WorldModule.MODULE_SIZE; x++)
        for (int z = 0; z < WorldModule.MODULE_SIZE; z++)
        {
            bool genSuc = SRandom.Range(0, 1000) < GenerateBoxLayerData.BoxCountPerThousandGrid;
            if (genSuc)
            {
                ushort existedBoxTypeIndex = moduleData.BoxMatrix[x, 0, z];
                TryOverrideBoxInfoOnMap(moduleData, existedBoxTypeIndex, x, z);
            }
        }
    }
}
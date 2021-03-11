using BiangLibrary.GameDataFormat;

public sealed class RandomMapGenerator : MapGenerator
{
    public RandomMapGenerator(OpenWorld.GenerateLayerData layerData, int width, int depth, uint seed, OpenWorld openWorld) : base(layerData, width, depth, seed, openWorld)
    {
    }

    public override void ApplyToWorldMap()
    {
        if (GenerateLayerData.m_GenerateAlgorithm == OpenWorld.GenerateAlgorithm.Random && !GenerateLayerData.CertainNumber && GenerateLayerData.CountPer10KGrid == 0) return; // 避免大量运算
        if (GenerateLayerData.m_GenerateAlgorithm == OpenWorld.GenerateAlgorithm.Random && GenerateLayerData.CertainNumber && GenerateLayerData.Count == 0) return; // 避免大量运算

        if (GenerateLayerData.CertainNumber)
        {
            SRandom = new SRandom(Seed);
            int genCount = 0;
            while (genCount < GenerateLayerData.Count)
            {
                if (genCount >= GenerateLayerData.Count) break;
                int module_x = SRandom.Range(0, Width / WorldModule.MODULE_SIZE);
                int module_z = SRandom.Range(0, Depth / WorldModule.MODULE_SIZE);
                for (int local_x = 0; local_x < WorldModule.MODULE_SIZE; local_x++)
                {
                    if (genCount >= GenerateLayerData.Count) break;
                    for (int local_z = 0; local_z < WorldModule.MODULE_SIZE; local_z++)
                    {
                        if (genCount >= GenerateLayerData.Count) break;
                        int world_x = module_x * WorldModule.MODULE_SIZE + local_x;
                        int world_z = module_z * WorldModule.MODULE_SIZE + local_z;
                        if (TryOverrideToWorldMap(world_x, Height, world_z))
                        {
                            genCount++;
                        }
                    }
                }
            }
        }
        else
        {
            for (int module_x = 0; module_x < Width / WorldModule.MODULE_SIZE; module_x++)
            for (int module_z = 0; module_z < Depth / WorldModule.MODULE_SIZE; module_z++)
            {
                uint module_Seed = (uint) (Seed + module_x * 100 + module_z); // 每个模组的Seed独一无二
                SRandom = new SRandom(module_Seed);

                for (int local_x = 0; local_x < WorldModule.MODULE_SIZE; local_x++)
                for (int local_z = 0; local_z < WorldModule.MODULE_SIZE; local_z++)
                {
                    bool genSuc = SRandom.Range(0, 10000) < GenerateLayerData.CountPer10KGrid;
                    if (genSuc)
                    {
                        int world_x = module_x * WorldModule.MODULE_SIZE + local_x;
                        int world_z = module_z * WorldModule.MODULE_SIZE + local_z;
                        TryOverrideToWorldMap(world_x, Height, world_z);
                    }
                }
            }
        }
    }
}
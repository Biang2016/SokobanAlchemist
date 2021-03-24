using System;
using System.Collections.Generic;
using System.Linq;
using BiangLibrary.GameDataFormat;

public sealed class CellularAutomataTerrainGenerator
{
    public TerrainType[,] map_1;
    public TerrainType[,] map_2;

    public int Width;
    public int Depth;

    public OpenWorld OpenWorld;
    public SRandom SRandom;
    public TerrainType[,] WorldMap_TerrainType => OpenWorld.WorldMap_TerrainType; // 地图地形分布

    private Dictionary<TerrainType, int> cached_SurroundingTerrainTypeCountDict = new Dictionary<TerrainType, int>();
    private Dictionary<TerrainType, int> cached_SurroundingTerrainTypeCountDict_2x = new Dictionary<TerrainType, int>();

    public CellularAutomataTerrainGenerator(OpenWorld.GenerateTerrainData data, int width, int depth, uint seed, OpenWorld openWorld)
    {
        SRandom = new SRandom(seed);
        Width = width;
        Depth = depth;
        OpenWorld = openWorld;
        map_1 = new TerrainType[Width, Depth];
        map_2 = new TerrainType[Width, Depth];

        cached_SurroundingTerrainTypeCountDict.Clear();
        foreach (TerrainType terrainType in Enum.GetValues(typeof(TerrainType)))
        {
            cached_SurroundingTerrainTypeCountDict.Add(terrainType, 0);
        }

        cached_SurroundingTerrainTypeCountDict_2x.Clear();
        foreach (TerrainType terrainType in Enum.GetValues(typeof(TerrainType)))
        {
            cached_SurroundingTerrainTypeCountDict_2x.Add(terrainType, 0);
        }

        foreach (Pass pass in data.ProcessingPassList)
        {
            switch (pass)
            {
                case RandomFillPass randomFillPass:
                {
                    InitRandomFillMap(randomFillPass);
                    break;
                }
                case SmoothPass smoothPass:
                {
                    SmoothMap(smoothPass);
                    break;
                }
            }
        }
    }

    private void InitRandomFillMap(RandomFillPass randomFillPass)
    {
        for (int world_x = 0; world_x < Width; world_x++)
        {
            for (int world_z = 0; world_z < Depth; world_z++)
            {
                bool fill = false;
                if (world_x == 0 || world_x == Width - 1 || world_z == 0 || world_z == Depth - 1)
                {
                    fill = true;
                }
                else
                {
                    fill = SRandom.Range(0, 100) < randomFillPass.FillPercent;
                    if (randomFillPass.OnlyOverrideSomeTerrain)
                    {
                        if (map_1[world_x, world_z] != randomFillPass.OverrideTerrainType)
                        {
                            fill = false;
                        }
                    }
                }

                if (fill)
                {
                    map_1[world_x, world_z] = randomFillPass.TerrainType;
                }
                else
                {
                    map_1[world_x, world_z] = TerrainType.Earth;
                }
            }
        }
    }

    private void SmoothMap(SmoothPass smoothPass)
    {
        for (int i = 0; i < smoothPass.SmoothTimes; i++)
        {
            for (int world_x = 0; world_x < Width; world_x++)
            for (int world_z = 0; world_z < Depth; world_z++)
            {
                bool isStaticLayout = WorldMap_TerrainType[world_x, world_z] != 0; // 识别静态布局
                if (isStaticLayout) continue; // 静态布局内不受影响
                Dictionary<TerrainType, int> neighborCount = GetSurroundingWallCount(map_1, world_x, world_z, 1);

                // 核心逻辑
                foreach (SmoothPass.NeighborIteration iteration in smoothPass.NeighborIterations)
                {
                    if (neighborCount[iteration.NeighborTerrainType] >= iteration.Threshold) map_2[world_x, world_z] = iteration.ChangeTerrainTypeTo;
                }
            }

            TerrainType[,] swap = map_1;
            map_1 = map_2;
            map_2 = swap;
        }
    }

    private Dictionary<TerrainType, int> GetSurroundingWallCount(TerrainType[,] oldMap, int world_x, int world_z, int rounds)
    {
        Dictionary<TerrainType, int> dict = rounds == 1 ? cached_SurroundingTerrainTypeCountDict : cached_SurroundingTerrainTypeCountDict_2x;
        foreach (TerrainType key in dict.Keys.ToList())
        {
            dict[key] = 0;
        }

        for (int neighborX = world_x - rounds; neighborX <= world_x + rounds; neighborX++)
        {
            for (int neighborZ = world_z - rounds; neighborZ <= world_z + rounds; neighborZ++)
            {
                if (neighborX >= 0 && neighborX < Width && neighborZ >= 0 && neighborZ < Depth)
                {
                    if (neighborX != world_x || neighborZ != world_z)
                    {
                        TerrainType terrainType = oldMap[neighborX, neighborZ];
                        dict[terrainType]++;
                    }
                }
                else
                {
                    dict[TerrainType.Earth]++;
                }
            }
        }

        return dict;
    }

    public void ApplyToWorldTerrainMap()
    {
        for (int world_x = 0; world_x < Width; world_x++)
        for (int world_z = 0; world_z < Depth; world_z++)
        {
            WorldMap_TerrainType[world_x, world_z] = map_1[world_x, world_z];
        }
    }
}
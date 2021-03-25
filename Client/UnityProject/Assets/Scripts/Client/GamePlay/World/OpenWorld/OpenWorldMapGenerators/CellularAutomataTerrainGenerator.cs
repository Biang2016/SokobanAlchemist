using System;
using System.Collections.Generic;
using System.Linq;
using BiangLibrary.GameDataFormat;
using UnityEngine;

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

    public CellularAutomataTerrainGenerator(GenerateTerrainData data, int width, int depth, uint seed, OpenWorld openWorld)
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

        foreach (TerrainProcessPass pass in data.ProcessingPassList)
        {
            switch (pass)
            {
                case TerrainProcessPass_RandomFill randomFillPass:
                {
                    InitRandomFillMap(randomFillPass);
                    break;
                }
                case TerrainProcessPass_Smooth smoothPass:
                {
                    SmoothMap(smoothPass);
                    break;
                }
            }
        }
    }

    private void InitRandomFillMap(TerrainProcessPass_RandomFill randomFillPass)
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
                    int fillPercent = randomFillPass.FillPercent;
                    if (randomFillPass.ControlFillPercentWithPerlinNoise)
                    {
                        float per_x = world_x * 19f / 7f;
                        float per_z = world_z * 19f / 7f;
                        float perlinValue = Perlin.Noise(per_x, per_z);
                        fillPercent = Mathf.FloorToInt((perlinValue + 1f) / 2f * 100);
                    }

                    fill = SRandom.Range(0, 100) < fillPercent;
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
            }
        }
    }

    private void SmoothMap(TerrainProcessPass_Smooth smoothPass)
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
                foreach (TerrainProcessPass_Smooth.NeighborIteration iteration in smoothPass.NeighborIterations)
                {
                    if (iteration.LimitSelfType && iteration.SelfTerrainType != map_2[world_x, world_z]) continue;
                    switch (iteration.Operator)
                    {
                        case TerrainProcessPass_Smooth.Operator.LessEquals:
                        {
                            if (neighborCount[iteration.NeighborTerrainType] <= iteration.Threshold) map_2[world_x, world_z] = iteration.ChangeTerrainTypeTo;
                            break;
                        }
                        case TerrainProcessPass_Smooth.Operator.Equals:
                        {
                            if (neighborCount[iteration.NeighborTerrainType] == iteration.Threshold) map_2[world_x, world_z] = iteration.ChangeTerrainTypeTo;
                            break;
                        }
                        case TerrainProcessPass_Smooth.Operator.GreaterEquals:
                        {
                            if (neighborCount[iteration.NeighborTerrainType] >= iteration.Threshold) map_2[world_x, world_z] = iteration.ChangeTerrainTypeTo;
                            break;
                        }
                    }
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
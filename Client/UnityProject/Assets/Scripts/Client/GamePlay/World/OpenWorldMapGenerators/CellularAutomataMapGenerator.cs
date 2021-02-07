public class CellularAutomataMapGenerator : MapGenerator
{
    public bool[,] map_1;
    public bool[,] map_2;

    public bool this[int x, int y] => map_1[x, y];

    public CellularAutomataMapGenerator(OpenWorld.GenerateBoxLayerData boxLayerData, int width, int height, int randomFillPercent, int smoothTimes, int smoothTimes_generateWallInOpenSpace, uint seed)
        : base(boxLayerData, width, height, seed)
    {
        map_1 = new bool[Width, Height];
        map_2 = new bool[Width, Height];

        InitRandomFillMap(randomFillPercent);

        for (int i = 0; i < smoothTimes_generateWallInOpenSpace; i++)
        {
            SmoothMapGenerateWallInOpenSpace(map_1, map_2);
            bool[,] swap = map_1;
            map_1 = map_2;
            map_2 = swap;
        }

        for (int i = 0; i < smoothTimes; i++)
        {
            SmoothMap(map_1, map_2);
            bool[,] swap = map_1;
            map_1 = map_2;
            map_2 = swap;
        }
    }

    private void InitRandomFillMap(int randomFillPercent)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                bool fill = false;
                if (x == 0 || x == Width - 1 || y == 0 || y == Height - 1)
                {
                    fill = true;
                }
                else
                {
                    fill = SRandom.Range(0, 100) < randomFillPercent;
                }

                map_1[x, y] = fill;
            }
        }
    }

    private void SmoothMap(bool[,] oldMap, bool[,] newMap)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                int neighborWallCount = GetSurroundingWallCount(oldMap, x, y, 1);
                newMap[x, y] = neighborWallCount >= 5;
            }
        }
    }

    private void SmoothMapGenerateWallInOpenSpace(bool[,] oldMap, bool[,] newMap)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                int neighborWallCount = GetSurroundingWallCount(oldMap, x, y, 1);
                int neighborWallCount_2x = GetSurroundingWallCount(oldMap, x, y, 2);
                newMap[x, y] = neighborWallCount >= 5 || neighborWallCount_2x <= 2;
            }
        }
    }

    private int GetSurroundingWallCount(bool[,] oldMap, int gridX, int gridY, int rounds)
    {
        int wallCount = 0;
        for (int neighborX = gridX - rounds; neighborX <= gridX + rounds; neighborX++)
        {
            for (int neighborY = gridY - rounds; neighborY <= gridY + rounds; neighborY++)
            {
                if (neighborX >= 0 && neighborX < Width && neighborY >= 0 && neighborY < Height)
                {
                    if (neighborX != gridX || neighborY != gridY)
                    {
                        wallCount += oldMap[neighborX, neighborY] ? 1 : 0;
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }

    public override void WriteMapInfoIntoWorldModuleData(WorldModuleData moduleData, int module_x, int module_z)
    {
        for (int x = 0; x < WorldModule.MODULE_SIZE; x++)
        for (int z = 0; z < WorldModule.MODULE_SIZE; z++)
        {
            bool genSuc = map_1[x + WorldModule.MODULE_SIZE * module_x, z + WorldModule.MODULE_SIZE * module_z];
            if (genSuc)
            {
                ushort existedBoxTypeIndex = moduleData.BoxMatrix[x, 0, z];
                TryOverrideBoxInfoOnMap(moduleData, existedBoxTypeIndex, x, z);
            }
        }
    }
}
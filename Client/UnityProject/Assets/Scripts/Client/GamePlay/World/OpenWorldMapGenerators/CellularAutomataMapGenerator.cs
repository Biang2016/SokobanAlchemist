using BiangLibrary.GameDataFormat;

public class CellularAutomataMapGenerator : MapGenerator
{
    public uint[,] map_1;
    public uint[,] map_2;

    public bool this[int x, int y] => GetValueFromMap(map_1, x, y);

    private bool GetValueFromMap(uint[,] map, int x, int y)
    {
        int x_bitIndex = x % 32;
        int x_map = x / 32;
        uint uintValue = map[x_map, y];
        bool value = ((uintValue >> x_bitIndex) & 1) == 1;
        return value;
    }

    private void WriteValueToMap(uint[,] map, int x, int y, bool value)
    {
        int x_bitIndex = x % 32;
        int x_map = x / 32;
        uint uintValue = map[x_map, y];
        if (value)
        {
            uintValue |= ((uint) 1 << x_bitIndex);
        }
        else
        {
            uintValue &= ~ ((uint) 1 << x_bitIndex);
        }

        map[x_map, y] = uintValue;
    }

    public CellularAutomataMapGenerator(OpenWorld.GenerateBoxLayerData boxLayerData, int width, int height, int randomFillPercent, int smoothTimes, int smoothTimes_generateWallInOpenSpace, uint seed)
        : base(boxLayerData, width, height, seed)
    {
        map_1 = new uint[Width / 32, Height];
        map_2 = new uint[Width / 32, Height];

        InitRandomFillMap(randomFillPercent);

        for (int i = 0; i < smoothTimes_generateWallInOpenSpace; i++)
        {
            SmoothMapGenerateWallInOpenSpace(map_1, map_2);
            uint[,] swap = map_1;
            map_1 = map_2;
            map_2 = swap;
        }

        for (int i = 0; i < smoothTimes; i++)
        {
            SmoothMap(map_1, map_2);
            uint[,] swap = map_1;
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
                if (x == 0 || x == Width - 1 || y == 0 || y == Height - 1)
                {
                    WriteValueToMap(map_1, x, y, true);
                }
                else
                {
                    WriteValueToMap(map_1, x, y, SRandom.Range(0, 100) < randomFillPercent);
                }
            }
        }
    }

    private void SmoothMap(uint[,] oldMap, uint[,] newMap)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                int neighborWallCount = GetSurroundingWallCount(oldMap, x, y, 1);
                WriteValueToMap(newMap, x, y, neighborWallCount >= 5);
            }
        }
    }

    private void SmoothMapGenerateWallInOpenSpace(uint[,] oldMap, uint[,] newMap)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                int neighborWallCount = GetSurroundingWallCount(oldMap, x, y, 1);
                int neighborWallCount_2x = GetSurroundingWallCount(oldMap, x, y, 2);
                WriteValueToMap(newMap, x, y, neighborWallCount >= 5 || neighborWallCount_2x <= 2);
            }
        }
    }

    private int GetSurroundingWallCount(uint[,] oldMap, int gridX, int gridY, int rounds)
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
                        wallCount += GetValueFromMap(oldMap, neighborX, neighborY) ? 1 : 0;
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
            bool genSuc = GetValueFromMap(map_1, x + WorldModule.MODULE_SIZE * module_x, z + WorldModule.MODULE_SIZE * module_z);
            if (genSuc)
            {
                ushort existedBoxTypeIndex = moduleData.BoxMatrix[x, 0, z];
                TryOverrideBoxInfoOnMap(moduleData, existedBoxTypeIndex, x, z);
            }
        }
    }
}
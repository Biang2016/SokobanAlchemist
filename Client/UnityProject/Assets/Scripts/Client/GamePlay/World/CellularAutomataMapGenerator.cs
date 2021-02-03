using BiangLibrary.GameDataFormat;

public class CellularAutomataMapGenerator
{
    public int Width;
    public int Height;
    private SRandom SRandom;
    public ushort[,] map_1;
    public ushort[,] map_2;

    public ushort this[int x, int y] => map_1[x, y];

    public CellularAutomataMapGenerator(int width, int height, int randomFillPercent, int smoothTimes, int smoothTimes_generateWallInOpenSpace, SRandom _SRandom)
    {
        SRandom = _SRandom;

        Width = width;
        Height = height;
        map_1 = new ushort[Width, Height];
        map_2 = new ushort[Width, Height];

        InitRandomFillMap(randomFillPercent);

        for (int i = 0; i < smoothTimes_generateWallInOpenSpace; i++)
        {
            SmoothMapGenerateWallInOpenSpace(map_1, map_2);
            ushort[,] swap = map_1;
            map_1 = map_2;
            map_2 = swap;
        }

        for (int i = 0; i < smoothTimes; i++)
        {
            SmoothMap(map_1, map_2);
            ushort[,] swap = map_1;
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
                    map_1[x, y] = 1;
                }
                else
                {
                    map_1[x, y] = (ushort) ((SRandom.Range(0, 100) < randomFillPercent) ? 1 : 0);
                }
            }
        }
    }

    private void SmoothMap(ushort[,] oldMap, ushort[,] newMap)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                int neighborWallCount = GetSurroundingWallCount(oldMap, x, y, 1);
                newMap[x, y] = neighborWallCount >= 5 ? (ushort) 1 : (ushort) 0;
            }
        }
    }

    private void SmoothMapGenerateWallInOpenSpace(ushort[,] oldMap, ushort[,] newMap)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                int neighborWallCount = GetSurroundingWallCount(oldMap, x, y, 1);
                int neighborWallCount_2x = GetSurroundingWallCount(oldMap, x, y, 2);
                newMap[x, y] = neighborWallCount >= 5 || neighborWallCount_2x <= 2 ? (ushort) 1 : (ushort) 0;
            }
        }
    }

    private int GetSurroundingWallCount(ushort[,] oldMap, int gridX, int gridY, int rounds)
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
                        wallCount += oldMap[neighborX, neighborY];
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
}
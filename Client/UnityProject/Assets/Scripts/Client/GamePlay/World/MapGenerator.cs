using System.Collections.Generic;
using BiangLibrary.GameDataFormat;
using UnityEngine;

public class MapGenerator
{
    public int Width;
    public int Height;
    public uint Seed;
    public int RandomFillPercent;
    private SRandom SRandom;
    public ushort[,] map;

    public ushort this[int x, int y] => map[x, y];

    public MapGenerator(int width, int height, int randomFillPercent, int smoothTimes, bool useRandomSeed, uint seed)
    {
        Seed = useRandomSeed ? (ushort) Time.time.ToString().GetHashCode() : seed;
        SRandom = new SRandom(Seed);

        Width = width;
        Height = height;
        RandomFillPercent = randomFillPercent;
        map = new ushort[Width, Height];

        InitRandomFillMap();
        for (int i = 0; i < smoothTimes; i++)
        {
            SmoothMap();
        }
    }

    private void InitRandomFillMap()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (x == 0 || x == Width - 1 || y == 0 || y == Height - 1)
                {
                    map[x, y] = 1;
                }
                else
                {
                    map[x, y] = (ushort) ((SRandom.Range(0, 100) < RandomFillPercent) ? 1 : 0);
                }
            }
        }
    }

    private void SmoothMap()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                int neighborWallCount = GetSurroundingWallCount(x, y);

                if (neighborWallCount > 4)
                    map[x, y] = 1;
                else if (neighborWallCount < 4)
                    map[x, y] = 0;
            }
        }
    }

    private int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighborX = gridX - 1; neighborX <= gridX + 1; neighborX++)
        {
            for (int neighborY = gridY - 1; neighborY <= gridY + 1; neighborY++)
            {
                if (neighborX >= 0 && neighborX < Width && neighborY >= 0 && neighborY < Height)
                {
                    if (neighborX != gridX || neighborY != gridY)
                    {
                        wallCount += map[neighborX, neighborY];
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
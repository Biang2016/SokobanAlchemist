using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using UnityEngine;

public class CellularAutomataMapGenerator : MapGenerator
{
    public bool[,] map_1;
    public bool[,] map_2;
    public int[,] CaveIndexMap;

    public GridPos ValidPlayerPosInConnectedCaves = GridPos.Zero;

    public bool this[int x, int y] => map_1[x, y];

    public CellularAutomataMapGenerator(OpenWorld.GenerateBoxLayerData boxLayerData, int width, int height, uint seed, GridPos leaveSpaceForPlayerBP)
        : base(boxLayerData, width, height, seed, leaveSpaceForPlayerBP)
    {
        map_1 = new bool[Width, Height];
        map_2 = new bool[Width, Height];

        // 初始化：随机填充
        InitRandomFillMap(boxLayerData.FillPercent);

        // 细胞自动机平滑，且空地生墙
        for (int i = 0; i < boxLayerData.SmoothTimes_GenerateWallInOpenSpace; i++)
        {
            SmoothMapGenerateWallInOpenSpace(map_1, map_2);
            bool[,] swap = map_1;
            map_1 = map_2;
            map_2 = swap;
        }

        // 细胞自动机平滑
        for (int i = 0; i < boxLayerData.SmoothTimes; i++)
        {
            SmoothMap(map_1, map_2);
            bool[,] swap = map_1;
            map_1 = map_2;
            map_2 = swap;
        }

        // 联通洞穴
        ConnectCave(boxLayerData.CaveConnectPercent, boxLayerData.DeterminePlayerBP);

        if (!boxLayerData.DeterminePlayerBP)
        {
            if (leaveSpaceForPlayerBP.x >= 0 && leaveSpaceForPlayerBP.x < Width && leaveSpaceForPlayerBP.z >= 0 && leaveSpaceForPlayerBP.z < Height)
            {
                map_1[leaveSpaceForPlayerBP.x, leaveSpaceForPlayerBP.z] = false;
            }
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

    private void ConnectCave(int connectCavePercent, bool determinePlayerBP)
    {
        if (connectCavePercent <= 0) return;
        CaveIndexMap = new int[Width, Height];
        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++)
            CaveIndexMap[x, y] = -1;

        #region Indentify caves

        int caveIndex = 0;

        GridPos cavePointMax_X = new GridPos(-1, 0);
        GridPos cavePointMin_X = new GridPos(int.MaxValue, 0);
        GridPos cavePointMax_Y = new GridPos(0, -1);
        GridPos cavePointMin_Y = new GridPos(0, int.MaxValue);

        List<List<GridPos>> CaveConnectPoints = new List<List<GridPos>>();
        Queue<GridPos> floodQueue = new Queue<GridPos>(200);

        while (PickStartPoint(out int x, out int y))
        {
            floodQueue.Clear();
            floodQueue.Enqueue(new GridPos(x, y));
            Flood(caveIndex);
            caveIndex++;
            CaveConnectPoints.Add(new List<GridPos> {cavePointMax_X, cavePointMin_X, cavePointMax_Y, cavePointMin_Y});

            cavePointMax_X = new GridPos(-1, 0);
            cavePointMin_X = new GridPos(int.MaxValue, 0);
            cavePointMax_Y = new GridPos(0, -1);
            cavePointMin_Y = new GridPos(0, int.MaxValue);
            if (determinePlayerBP) ValidPlayerPosInConnectedCaves = cavePointMin_X; // 处在洞穴中的玩家位置,随便取一个作为初始，避免只有单洞穴情况后面没有赋值
        }

        bool PickStartPoint(out int x, out int y)
        {
            x = 0;
            y = 0;
            for (x = 0; x < Width; x++)
            {
                for (y = 0; y < Height; y++)
                {
                    if (!map_1[x, y] && CaveIndexMap[x, y] == -1)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        void Flood(int _caveIndex)
        {
            while (floodQueue.Count > 0)
            {
                GridPos center = floodQueue.Dequeue();
                int x = center.x;
                int y = center.z;
                if (x < 0 || x >= Width || y < 0 || y >= Height) continue;
                if (map_1[x, y]) continue;
                if (CaveIndexMap[x, y] == _caveIndex) continue;
                CaveIndexMap[x, y] = _caveIndex;
                if (cavePointMax_X.x < x) cavePointMax_X = new GridPos(x, y);
                if (cavePointMin_X.x > x) cavePointMin_X = new GridPos(x, y);
                if (cavePointMax_Y.z < y) cavePointMax_Y = new GridPos(x, y);
                if (cavePointMin_Y.z > y) cavePointMin_Y = new GridPos(x, y);
                floodQueue.Enqueue(new GridPos(x - 1, y));
                floodQueue.Enqueue(new GridPos(x, y - 1));
                floodQueue.Enqueue(new GridPos(x + 1, y));
                floodQueue.Enqueue(new GridPos(x, y + 1));
            }
        }

        #endregion

        #region 最小生成树Prim算法计算最短联通边

        int caveCount = caveIndex;
        if (caveCount == 0)
        {
            Debug.Log("OpenWorld Generator: No Caves!");
            return;
        }

        if (caveCount == 1) return;

        List<int> connectedCaveIndexArray = new List<int>();
        for (int i = 0; i < caveCount; i++)
        {
            if (SRandom.Range(0, 100) < connectCavePercent)
            {
                connectedCaveIndexArray.Add(i);
            }
        }

        int connectCaveCount = connectedCaveIndexArray.Count;
        if (connectCaveCount <= 1) return;

        int[,] Graph = new int[connectCaveCount, connectCaveCount];
        for (int a = 0; a < connectCaveCount; a++)
        for (int b = a + 1; b < connectCaveCount; b++)
        {
            int cost = CalculateCaveConnectCost(connectedCaveIndexArray[a], connectedCaveIndexArray[b]);
            Graph[a, b] = cost;
            Graph[b, a] = cost;
        }

        HashSet<int> caveInGraph_graphIndex = new HashSet<int>();
        int rootCave_graphIndex = SRandom.Range(0, connectCaveCount);

        // Choose Player Position
        if (determinePlayerBP)
        {
            int rootCaveIndex = connectedCaveIndexArray[rootCave_graphIndex];
            for (int x = 0; x < Width; x++)
            {
                bool found = false;
                for (int y = 0; y < Height; y++)
                {
                    if (CaveIndexMap[x, y] == rootCaveIndex)
                    {
                        ValidPlayerPosInConnectedCaves = new GridPos(x, y);
                        found = true;
                        break;
                    }
                }

                if (found) break;
            }
        }

        ExpandGraph(rootCave_graphIndex);

        int CalculateCaveConnectCost(int caveIndex_A, int caveIndex_B)
        {
            List<GridPos> ccp_A = CaveConnectPoints[caveIndex_A];
            List<GridPos> ccp_B = CaveConnectPoints[caveIndex_B];
            float minDist = float.MaxValue;
            for (int a = 0; a < ccp_A.Count; a++)
            for (int b = 0; b < ccp_B.Count; b++)
            {
                float dist = (ccp_A[a] - ccp_B[b]).magnitude;
                if (minDist > dist)
                {
                    minDist = dist;
                }
            }

            return Mathf.CeilToInt(minDist);
        }

        void ExpandGraph(int newCave_graphIndex)
        {
            caveInGraph_graphIndex.Add(newCave_graphIndex);
            if (caveInGraph_graphIndex.Count == connectCaveCount) return; // 全部洞穴连接完毕
            float minDist = float.MaxValue;
            int nearestCaveIndex_graphIndex = -1;
            int nearestCaveIndex = -1;
            int nearestCaveIndex_InGraph = -1;
            foreach (int ci in caveInGraph_graphIndex)
            {
                for (int i = 0; i < connectCaveCount; i++)
                {
                    if (i == ci) continue;
                    if (caveInGraph_graphIndex.Contains(i)) continue;
                    int dist = Graph[ci, i];
                    if (minDist > dist)
                    {
                        minDist = dist;
                        nearestCaveIndex_graphIndex = i;
                        nearestCaveIndex = connectedCaveIndexArray[i];
                        nearestCaveIndex_InGraph = connectedCaveIndexArray[ci];
                    }
                }
            }

            if (nearestCaveIndex == -1 && nearestCaveIndex_InGraph == -1) return;
            MakeTunnelBetweenCaves(nearestCaveIndex, nearestCaveIndex_InGraph); // 找最近的洞穴打穿隧道
            ExpandGraph(nearestCaveIndex_graphIndex);
        }

        void MakeTunnelBetweenCaves(int caveIndex_A, int caveIndex_B)
        {
            List<GridPos> ccp_A = CaveConnectPoints[caveIndex_A];
            List<GridPos> ccp_B = CaveConnectPoints[caveIndex_B];
            float minDist = float.MaxValue;
            GridPos cp_A = GridPos.Zero;
            GridPos cp_B = GridPos.Zero;
            for (int a = 0; a < ccp_A.Count; a++)
            for (int b = 0; b < ccp_B.Count; b++)
            {
                float dist = (ccp_A[a] - ccp_B[b]).magnitude;
                if (minDist > dist)
                {
                    minDist = dist;
                    cp_A = ccp_A[a];
                    cp_B = ccp_B[b];
                }
            }

            int xMin = Mathf.Min(cp_A.x, cp_B.x);
            int xMax = Mathf.Max(cp_A.x, cp_B.x);
            int yMin = Mathf.Min(cp_A.z, cp_B.z);
            int yMax = Mathf.Max(cp_A.z, cp_B.z);

            int cur_X = xMin;
            int cur_Y = yMin;

            while (cur_X != xMax || cur_Y != yMax)
            {
                bool goX = false;
                if (cur_X == xMax)
                {
                    goX = false;
                }
                else if (cur_Y == yMax)
                {
                    goX = true;
                }
                else
                {
                    goX = SRandom.Range(0, 2) == 0;
                }

                if (goX)
                {
                    int diffX = SRandom.Range(0, xMax - cur_X + 1);
                    for (int dx = 1; dx <= diffX; dx++)
                    {
                        map_1[cur_X + dx, cur_Y] = false;
                    }

                    cur_X += diffX;
                }
                else
                {
                    int diffY = SRandom.Range(0, yMax - cur_Y + 1);
                    for (int dy = 1; dy <= diffY; dy++)
                    {
                        map_1[cur_X, cur_Y + dy] = false;
                    }

                    cur_Y += diffY;
                }
            }
        }

        #endregion
    }

    public override void WriteMapInfoIntoWorldModuleData(WorldModuleData moduleData, int module_x, int module_z)
    {
        for (int x = 0; x < WorldModule.MODULE_SIZE; x++)
        for (int z = 0; z < WorldModule.MODULE_SIZE; z++)
        {
            bool genSuc = map_1[x + WorldModule.MODULE_SIZE * module_x, z + WorldModule.MODULE_SIZE * module_z];
            if (genSuc)
            {
                ushort existedBoxTypeIndex = moduleData.RawBoxMatrix[x, 0, z];
                TryOverrideBoxInfoOnMap(moduleData, existedBoxTypeIndex, x, z);
            }
        }
    }
}
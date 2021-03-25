using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using UnityEngine;
using UnityEngine.Assertions;

public sealed class CellularAutomataMapGenerator : MapGenerator
{
    public bool[,] map_1;
    public bool[,] map_2;
    public int[,] CaveIndexMap;

    public static bool CAVE_GEN_DEBUG_LOG
#if UNITY_EDITOR
        = false;
#else
        = false;
#endif

    public bool this[int x, int y] => map_1[x, y];

    public CellularAutomataMapGenerator(GenerateBoxLayerData boxLayerData, int width, int depth, uint seed, OpenWorld openWorld)
        : base(boxLayerData, width, depth, seed, openWorld)
    {
        map_1 = new bool[Width, Depth];
        map_2 = new bool[Width, Depth];

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
        ConnectCave(boxLayerData.CaveConnectPercent);
    }

    private void InitRandomFillMap(int randomFillPercent)
    {
        for (int world_x = 0; world_x < Width; world_x++)
        {
            for (int world_z = 0; world_z < Depth; world_z++)
            {
                bool isStaticLayout = WorldMap_StaticLayoutOccupied_IntactForBox[world_x, 0, world_z] != 0; // 识别静态布局
                bool fill = false;
                if (world_x == 0 || world_x == Width - 1 || world_z == 0 || world_z == Depth - 1)
                {
                    fill = true;
                }
                else if (isStaticLayout)
                {
                    fill = WorldMap[world_x, 0, world_z] == EntityTypeIndex;
                }
                else
                {
                    fill = SRandom.Range(0, 100) < randomFillPercent;
                }

                map_1[world_x, world_z] = fill;
            }
        }
    }

    private void SmoothMap(bool[,] oldMap, bool[,] newMap)
    {
        for (int world_x = 0; world_x < Width; world_x++)
        for (int world_z = 0; world_z < Depth; world_z++)
        {
            bool isStaticLayout = WorldMap_StaticLayoutOccupied_IntactForBox[world_x, 0, world_z] != 0; // 识别静态布局
            if (isStaticLayout) continue; // 静态布局内不受影响
            int neighborWallCount = GetSurroundingWallCount(oldMap, world_x, world_z, 1);
            newMap[world_x, world_z] = neighborWallCount >= 5;
        }
    }

    private void SmoothMapGenerateWallInOpenSpace(bool[,] oldMap, bool[,] newMap)
    {
        for (int world_x = 0; world_x < Width; world_x++)
        for (int world_z = 0; world_z < Depth; world_z++)
        {
            bool isStaticLayout = WorldMap_StaticLayoutOccupied_IntactForBox[world_x, 0, world_z] != 0; // 识别静态布局
            if (isStaticLayout) continue; // 静态布局内不受影响
            int neighborWallCount = GetSurroundingWallCount(oldMap, world_x, world_z, 1);
            int neighborWallCount_2x = GetSurroundingWallCount(oldMap, world_x, world_z, 2);
            newMap[world_x, world_z] = neighborWallCount >= 5 || neighborWallCount_2x <= 2;
        }
    }

    private int GetSurroundingWallCount(bool[,] oldMap, int world_x, int world_z, int rounds)
    {
        int wallCount = 0;
        for (int neighborX = world_x - rounds; neighborX <= world_x + rounds; neighborX++)
        {
            for (int neighborZ = world_z - rounds; neighborZ <= world_z + rounds; neighborZ++)
            {
                if (neighborX >= 0 && neighborX < Width && neighborZ >= 0 && neighborZ < Depth)
                {
                    if (neighborX != world_x || neighborZ != world_z)
                    {
                        bool isWall = oldMap[neighborX, neighborZ];

                        // 静态布局中的所有Hill、BorderBox、BrickBox都算作既有墙
                        if (GenerateLayerData.ConsiderStaticLayout)
                        {
                            ushort existedIndex = WorldMap_Occupied[neighborX, 0, neighborZ];
                            ConfigManager.TypeStartIndex typeStartIndex = existedIndex.ConvertToTypeStartIndex();
                            if (typeStartIndex == ConfigManager.TypeStartIndex.Box)
                            {
                                string boxTypeName = ConfigManager.GetTypeName(TypeDefineType.Box, existedIndex);
                                if (boxTypeName.StartsWith("Hill") || boxTypeName.Equals("BrickBox") || boxTypeName.Equals("BorderBox"))
                                {
                                    isWall = true;
                                }
                            }
                        }

                        wallCount += isWall ? 1 : 0;
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

    private void ConnectCave(int connectCavePercent)
    {
        if (connectCavePercent <= 0) return;
        CaveIndexMap = new int[Width, Depth];
        for (int world_x = 0; world_x < Width; world_x++)
        for (int world_z = 0; world_z < Depth; world_z++)
            CaveIndexMap[world_x, world_z] = -1;

        #region Indentify caves

        int caveIndex = 0;

        GridPos cavePointMax_X = new GridPos(-1, 0);
        GridPos cavePointMin_X = new GridPos(int.MaxValue, 0);
        GridPos cavePointMax_Z = new GridPos(0, -1);
        GridPos cavePointMin_Z = new GridPos(0, int.MaxValue);

        List<List<GridPos>> CaveConnectPoints = new List<List<GridPos>>();
        Queue<GridPos> floodQueue = new Queue<GridPos>(200);

        while (PickStartPoint(out int world_x, out int world_z))
        {
            floodQueue.Clear();
            floodQueue.Enqueue(new GridPos(world_x, world_z));
            Flood(caveIndex, out int area);
            caveIndex++;
            CaveConnectPoints.Add(new List<GridPos> {cavePointMax_X, cavePointMin_X, cavePointMax_Z, cavePointMin_Z});

            cavePointMax_X = new GridPos(-1, 0);
            cavePointMin_X = new GridPos(int.MaxValue, 0);
            cavePointMax_Z = new GridPos(0, -1);
            cavePointMin_Z = new GridPos(0, int.MaxValue);
        }

        bool PickStartPoint(out int world_x, out int world_z)
        {
            world_x = 0;
            world_z = 0;
            for (world_x = 0; world_x < Width; world_x++)
            {
                for (world_z = 0; world_z < Depth; world_z++)
                {
                    if (!map_1[world_x, world_z] && CaveIndexMap[world_x, world_z] == -1)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        void Flood(int _caveIndex, out int caveArea)
        {
            caveArea = 0;
            while (floodQueue.Count > 0)
            {
                GridPos center = floodQueue.Dequeue();
                int world_x = center.x;
                int world_z = center.z;
                if (world_x < 0 || world_x >= Width || world_z < 0 || world_z >= Depth) continue;
                if (map_1[world_x, world_z]) continue;
                if (CaveIndexMap[world_x, world_z] == _caveIndex) continue;
                CaveIndexMap[world_x, world_z] = _caveIndex;
                caveArea++;
                if (cavePointMax_X.x < world_x) cavePointMax_X = new GridPos(world_x, world_z);
                if (cavePointMin_X.x > world_x) cavePointMin_X = new GridPos(world_x, world_z);
                if (cavePointMax_Z.z < world_z) cavePointMax_Z = new GridPos(world_x, world_z);
                if (cavePointMin_Z.z > world_z) cavePointMin_Z = new GridPos(world_x, world_z);
                floodQueue.Enqueue(new GridPos(world_x - 1, world_z));
                floodQueue.Enqueue(new GridPos(world_x, world_z - 1));
                floodQueue.Enqueue(new GridPos(world_x + 1, world_z));
                floodQueue.Enqueue(new GridPos(world_x, world_z + 1));
            }
        }

        #endregion

        #region 最小生成树Prim算法计算最短联通边

        int initialCaveCount = caveIndex;
        Log($"InitialCaveCount: {initialCaveCount}");
        if (initialCaveCount == 0)
        {
            Log("No Initial Caves!");
            return;
        }

        if (initialCaveCount == 1) return;

        List<int> connectedCaveIndexArray = new List<int>();
        for (int i = 0; i < initialCaveCount; i++)
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

        Log($"ConnectCaveCount: {connectCaveCount}");

        HashSet<int> caveInGraph_graphIndex = new HashSet<int>();
        int rootCave_graphIndex = 0;
        int rootCaveIndex = CaveIndexMap[m_OpenWorld.InitialPlayerBP.x, m_OpenWorld.InitialPlayerBP.z];
        if (rootCaveIndex == -1)
        {
            Debug.LogError("OpenWorld玩家初始坐标处在非洞穴区域");
        }
        else
        {
            for (int i = 0; i < connectedCaveIndexArray.Count; i++)
            {
                if (connectedCaveIndexArray[i] == rootCaveIndex)
                {
                    rootCave_graphIndex = i;
                    break;
                }
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

            Assert.IsFalse(map_1[cp_A.x, cp_A.z]);
            Assert.IsFalse(map_1[cp_B.x, cp_B.z]);

            int xMin = Mathf.Min(cp_A.x, cp_B.x);
            int xMax = Mathf.Max(cp_A.x, cp_B.x);
            int zMin = Mathf.Min(cp_A.z, cp_B.z);
            int zMax = Mathf.Max(cp_A.z, cp_B.z);

            int cur_X = xMin;
            bool revDiagonal = (cp_A.x - cp_B.x) * (cp_A.z - cp_B.z) < 0; // 左上到右下
            int cur_Z = revDiagonal ? zMax : zMin;

            int tunnelCount = 0;
            if (!revDiagonal)
            {
                while (cur_X != xMax || cur_Z != zMax)
                {
                    Assert.IsFalse(cur_X > xMax || cur_Z > zMax);
                    bool goX = false;
                    if (cur_X == xMax)
                    {
                        goX = false;
                    }
                    else if (cur_Z == zMax)
                    {
                        goX = true;
                    }
                    else
                    {
                        goX = SRandom.Range(0, 2) == 0;
                    }

                    if (goX)
                    {
                        int diffX = Mathf.Max(1, SRandom.Range(1, (xMax - cur_X + 1) / 2 + 1));
                        for (int dx = 1; dx <= diffX; dx++)
                        {
                            bool isStaticLayout = WorldMap_StaticLayoutOccupied_IntactForBox[cur_X + dx, 0, cur_Z] != 0; // 识别静态布局
                            if (isStaticLayout) continue; // 静态布局内不受影响
                            map_1[cur_X + dx, cur_Z] = false;
                            tunnelCount++;
                        }

                        cur_X += diffX;
                    }
                    else
                    {
                        int diffZ = Mathf.Max(1, SRandom.Range(1, (zMax - cur_Z + 1) / 2 + 1));
                        for (int dy = 1; dy <= diffZ; dy++)
                        {
                            bool isStaticLayout = WorldMap_StaticLayoutOccupied_IntactForBox[cur_X, 0, cur_Z + dy] != 0; // 识别静态布局
                            if (isStaticLayout) continue; // 静态布局内不受影响
                            map_1[cur_X, cur_Z + dy] = false;
                            tunnelCount++;
                        }

                        cur_Z += diffZ;
                    }
                }
            }
            else
            {
                while (cur_X != xMax || cur_Z != zMin)
                {
                    Assert.IsFalse(cur_X > xMax || cur_Z < zMin);
                    bool goX = false;
                    if (cur_X == xMax)
                    {
                        goX = false;
                    }
                    else if (cur_Z == zMin)
                    {
                        goX = true;
                    }
                    else
                    {
                        goX = SRandom.Range(0, 2) == 0;
                    }

                    if (goX)
                    {
                        int diffX = Mathf.Max(1, SRandom.Range(1, (xMax - cur_X + 1) / 2 + 1));
                        for (int dx = 1; dx <= diffX; dx++)
                        {
                            bool isStaticLayout = WorldMap_StaticLayoutOccupied_IntactForBox[cur_X + dx, 0, cur_Z] != 0; // 识别静态布局
                            if (isStaticLayout) continue; // 静态布局内不受影响
                            map_1[cur_X + dx, cur_Z] = false;
                            tunnelCount++;
                        }

                        cur_X += diffX;
                    }
                    else
                    {
                        int diffZ = Mathf.Max(1, SRandom.Range(1, (cur_Z - zMin + 1) / 2 + 1));
                        for (int dy = 1; dy <= diffZ; dy++)
                        {
                            bool isStaticLayout = WorldMap_StaticLayoutOccupied_IntactForBox[cur_X, 0, cur_Z - dy] != 0; // 识别静态布局
                            if (isStaticLayout) continue; // 静态布局内不受影响
                            map_1[cur_X, cur_Z - dy] = false;
                            tunnelCount++;
                        }

                        cur_Z -= diffZ;
                    }
                }
            }

            //Log($"Make Tunnel from {cp_A} to {cp_B} by TunnelGridCount {tunnelCount}");
        }

        #endregion

        if (CAVE_GEN_DEBUG_LOG) // 统计、验证、日志
        {
            // 重新对洞穴进行标记
            for (int world_x = 0; world_x < Width; world_x++)
            {
                for (int world_z = 0; world_z < Depth; world_z++)
                {
                    CaveIndexMap[world_x, world_z] = -1;
                }
            }

            caveIndex = 0;
            List<int> caveAreaArray = new List<int>(initialCaveCount - connectCaveCount + 1);
            while (PickStartPoint(out int x, out int y))
            {
                floodQueue.Clear();
                floodQueue.Enqueue(new GridPos(x, y));
                Flood(caveIndex, out int caveArea);
                Assert.IsTrue(caveIndex < initialCaveCount - connectCaveCount + 1);
                caveAreaArray.Add(caveArea);
                caveIndex++;
            }

            int caveTotalArea = 0;
            foreach (int a in caveAreaArray)
            {
                caveTotalArea += a;
            }

            float caveRatio = (float) caveTotalArea / (Width * Depth) * 100;
            float caveConnectRatio = (float) (initialCaveCount - caveIndex + 1) / initialCaveCount * 100;

            Log($"FinalCaveCount: {caveIndex}. TotalCaveArea: {caveTotalArea}, CaveRatio: {caveRatio:##.#}%, CaveConnectRatio: {caveConnectRatio:##.#}%");

            float playerCaveAreaRatio = (float) caveAreaArray[rootCaveIndex] / caveTotalArea * 100;
            Log($"PlayerCaveArea: {caveAreaArray[rootCaveIndex]} PlayerCaveAreaRatio: {playerCaveAreaRatio:##.#}%");
        }
    }

    public override void ApplyToWorldMap()
    {
        for (int world_x = 0; world_x < Width; world_x++)
        for (int world_z = 0; world_z < Depth; world_z++)
        {
            bool genSuc = map_1[world_x, world_z];
            if (genSuc)
            {
                TryOverrideToWorldMap(world_x, Height, world_z);
            }
        }
    }

    public void Log(string log)
    {
        if (CAVE_GEN_DEBUG_LOG) Debug.Log("CaveGenerator: " + log);
    }
}
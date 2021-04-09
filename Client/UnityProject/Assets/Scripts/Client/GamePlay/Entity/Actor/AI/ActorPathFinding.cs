using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.ObjectPool;
using UnityEngine;
using UnityEngine.Profiling;

public static class ActorPathFinding
{
    #region 占位缓存，加速寻路搜索

    private static byte[,,] SpaceAvailableForActorHeight;

    public static void InitializeSpaceAvailableForActorHeight(int worldGridSize_x, int worldGridSize_y, int worldGridSize_z)
    {
        SpaceAvailableForActorHeight = new byte[worldGridSize_x, worldGridSize_y, worldGridSize_z];
    }

    public static bool GetSpaceAvailableForActorHeight(GridPos3D worldGP, int actorHeight)
    {
        if (worldGP.x < 0 || worldGP.x >= SpaceAvailableForActorHeight.GetLength(0) ||
            worldGP.y < 0 || worldGP.y >= SpaceAvailableForActorHeight.GetLength(1) ||
            worldGP.z < 0 || worldGP.z >= SpaceAvailableForActorHeight.GetLength(2)) return false;
        byte curValue = SpaceAvailableForActorHeight[worldGP.x, worldGP.y, worldGP.z];
        byte compare = (byte) (1 << (actorHeight - 1));
        return (curValue & compare) == compare;
    }

    public static void SetSpaceAvailableForActorHeight(GridPos3D worldGP, int actorHeight, bool available)
    {
        if (worldGP.x < 0 || worldGP.x >= SpaceAvailableForActorHeight.GetLength(0) ||
            worldGP.y < 0 || worldGP.y >= SpaceAvailableForActorHeight.GetLength(1) ||
            worldGP.z < 0 || worldGP.z >= SpaceAvailableForActorHeight.GetLength(2)) return;
        byte curValue = SpaceAvailableForActorHeight[worldGP.x, worldGP.y, worldGP.z];
        byte resValue = 0;
        if (available)
        {
            resValue = (byte) (curValue | (1 << (actorHeight - 1)));
        }
        else
        {
            resValue = (byte) (curValue & ~(1 << (actorHeight - 1)));
        }

        SpaceAvailableForActorHeight[worldGP.x, worldGP.y, worldGP.z] = resValue;
    }

    #endregion

    public class Node : IClassPoolObject<Node>
    {
        public int PoolIndex;
        public Node ParentNode;
        public GridPos3D GridPos3D_PF;
        public float F => G + H; // G+H
        public float G; // 从起点到该点的路径长度
        public float H; // 从该点到终点的估计路程

        public void OnUsed()
        {
        }

        public void Release()
        {
            NodeFactory.Release(this);
        }

        public void OnRelease()
        {
            ParentNode = null;
            GridPos3D_PF = GridPos3D.Zero;
            G = 0;
            H = 0;
        }

        public void SetPoolIndex(int index)
        {
            PoolIndex = index;
        }

        public int GetPoolIndex()
        {
            return PoolIndex;
        }
    }

    private static ClassObjectPool<Node> NodeFactory = new ClassObjectPool<Node>(100);

    internal static int InvokeTimes = 0;

    #region AStar PathFinding

    private static List<Node> OpenList = new List<Node>();
    private static List<Node> CloseList = new List<Node>();

    public static bool FindPath(GridPos3D ori_PF, GridPos3D dest_PF, Vector3 actorPos, List<Node> resPath, float keepDistanceMin, float keepDistanceMax, DestinationType destinationType, int actorWidth, int actorHeight, uint exceptActorGUID)
    {
        if (ori_PF.y != dest_PF.y) return false;
        WorldModule oriModule = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(ori_PF.ConvertPathFindingNodeGPToWorldPosition(actorWidth).ToGridPos3D());
        WorldModule destModule = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(dest_PF.ConvertPathFindingNodeGPToWorldPosition(actorWidth).ToGridPos3D());
        if (oriModule.IsNotNullAndAvailable() && destModule.IsNotNullAndAvailable())
        {
            Node oriNode = NodeFactory.Alloc();
            oriNode.GridPos3D_PF = ori_PF;
            Node destNode = NodeFactory.Alloc();
            destNode.GridPos3D_PF = dest_PF;
            return FindPath(oriNode, destNode, actorPos, resPath, keepDistanceMin, keepDistanceMax, (ori_PF - dest_PF).magnitude * 1.8f, destinationType, actorWidth, actorHeight, exceptActorGUID);
        }

        return false;
    }

    public static bool FindRandomAccessibleDestination(GridPos3D ori_PF, Vector3 actorPos, float rangeRadius, out GridPos3D destination_PF, int actorWidth, int actorHeight, uint exceptActorGUID)
    {
        destination_PF = GridPos3D.Zero;
        WorldModule oriModule = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(ori_PF.ConvertPathFindingNodeGPToWorldPosition(actorWidth).ToGridPos3D());
        if (oriModule.IsNotNullAndAvailable())
        {
            Profiler.BeginSample("UnionFindNodes");
            List<GridPos3D> validNodes = UnionFindNodes(ori_PF, actorPos, rangeRadius, actorWidth, actorHeight, exceptActorGUID);
            Profiler.EndSample();
            if (validNodes.Count == 0) return false;
            destination_PF = CommonUtils.GetRandomFromList(validNodes);
            return true;
        }

        return false;
    }

    private static bool FindPath(Node ori, Node dest, Vector3 actorPos, List<Node> resPath, float keepDistanceMin, float keepDistanceMax, float pathFindingRadius, DestinationType destinationType, int actorWidth, int actorHeight, uint exceptActorGUID)
    {
        if (ori.GridPos3D_PF.y != dest.GridPos3D_PF.y) return false;
        OpenList.Clear();
        CloseList.Clear();
        OpenList.Add(ori);
        ori.G = 0;
        ori.H = AStarHeuristicsDistance(ori, dest);
        while (OpenList.Count > 0)
        {
            float minF = float.MaxValue;
            Node minFNode = OpenList[0];
            foreach (Node node in OpenList)
            {
                if (node.F < minF)
                {
                    minF = node.F;
                    minFNode = node;
                }
            }

            OpenList.Remove(minFNode);
            CloseList.Add(minFNode);
            List<Node> adjacentNodes = GetAdjacentNodesForAStar(minFNode, actorPos, dest.GridPos3D_PF, destinationType, actorWidth, actorHeight, exceptActorGUID, CloseList);
            List<Node> uselessAdjacentNodes = cached_adjacentNodesList_clone; // 对象引用仍为同一个,此list只是为了避免modify collection inside foreach
            foreach (Node node in adjacentNodes)
            {
                bool inCloseList = false;
                foreach (Node nodeInCloseList in CloseList)
                {
                    if (node.GridPos3D_PF == nodeInCloseList.GridPos3D_PF)
                    {
                        inCloseList = true;
                        break;
                    }
                }

                if (inCloseList) continue;

                bool inOpenList = false;
                foreach (Node nodeInOpenList in OpenList)
                {
                    if (node.GridPos3D_PF == nodeInOpenList.GridPos3D_PF)
                    {
                        inOpenList = true;
                        break;
                    }
                }

                float newG = AStarHeuristicsDistance(node, minFNode) + minFNode.G;
                if (inOpenList)
                {
                    // 最短线路优化,Reparent
                    if (newG < node.G)
                    {
                        node.G = newG;
                        node.ParentNode = minFNode;
                    }
                }
                else
                {
                    uselessAdjacentNodes.Remove(node);
                    OpenList.Add(node);
                    node.ParentNode = minFNode;
                    node.G = newG;
                    node.H = AStarHeuristicsDistance(node, dest);

                    float diffToDest = (node.GridPos3D_PF - dest.GridPos3D_PF).magnitude;
                    if (diffToDest <= keepDistanceMax && diffToDest >= keepDistanceMin)
                    {
                        if (resPath != null)
                        {
                            foreach (Node n in resPath)
                            {
                                n.Release();
                            }

                            resPath.Clear();
                        }

                        Node nodePtr = node;
                        while (nodePtr != null)
                        {
                            if (resPath != null)
                            {
                                Node pathNode = NodeFactory.Alloc(); // return的List中的Node均为新分配的，与本类中的cache无关
                                pathNode.GridPos3D_PF = nodePtr.GridPos3D_PF;
                                resPath.Add(pathNode);
                            }

                            nodePtr = nodePtr.ParentNode;
                        }

                        resPath?.Reverse();

                        foreach (Node n in uselessAdjacentNodes)
                        {
                            n.Release();
                        }

                        uselessAdjacentNodes.Clear();

                        releaseNodes();
                        return true;
                    }
                }
            }

            foreach (Node n in uselessAdjacentNodes)
            {
                n.Release();
            }

            uselessAdjacentNodes.Clear();
        }

        releaseNodes();

        void releaseNodes()
        {
            foreach (Node n in OpenList)
            {
                n.Release();
            }

            OpenList.Clear();

            foreach (Node n in CloseList)
            {
                n.Release();
            }

            CloseList.Clear();
            dest.Release();
        }

        return false;
    }

    private static List<Node> cached_adjacentNodesList = new List<Node>(4);
    private static List<Node> cached_adjacentNodesList_clone = new List<Node>(4);

    private static List<Node> GetAdjacentNodesForAStar(Node node, Vector3 actorPos, GridPos3D destGP_PF, DestinationType destinationType, int actorWidth, int actorHeight, uint exceptActorGUID, List<Node> ref_CloseList)
    {
        cached_adjacentNodesList.Clear();
        cached_adjacentNodesList_clone.Clear();

        InvokeTimes++;
        Profiler.BeginSample("AISA_GetAdjacentNodesForAStar");
        bool available = CheckSpaceAvailableForActorOccupation(node.GridPos3D_PF, actorPos, actorWidth, actorHeight, exceptActorGUID);
        Profiler.EndSample();
        if (!available) return cached_adjacentNodesList;

        bool arriveDestGP_PF = false;

        void tryAddNode(GridPos3D gp_PF)
        {
            void OnSearchForward()
            {
                Node leftNode = NodeFactory.Alloc();
                leftNode.GridPos3D_PF = gp_PF;
                leftNode.ParentNode = node;
                cached_adjacentNodesList.Add(leftNode);
                cached_adjacentNodesList_clone.Add(leftNode);
            }

            Vector3 v_dest = destGP_PF - actorPos;
            Vector3 v_search = gp_PF - actorPos;
            float theta = Vector3.Angle(v_dest, v_search);
            float searchDistanceRatio = 1f;
            if (theta >= 0 && theta < 90f)
            {
                searchDistanceRatio = Mathf.Lerp(1.5f, 1, theta / 90f);
            }
            else if (theta >= 90f && theta <= 180f)
            {
                searchDistanceRatio = Mathf.Lerp(1f, 0.5f, (theta - 90f) / 90f);
            }

            if ((gp_PF - actorPos).magnitude > searchDistanceRatio * (destGP_PF - actorPos).magnitude) return; // 寻路到太远(超出目标距离若干倍数)则停止
            foreach (Node closeNode in ref_CloseList) // 性能优化，避免反复搜索Close节点
            {
                if (gp_PF == closeNode.GridPos3D_PF) return;
            }

            if (destinationType == DestinationType.Box || destinationType == DestinationType.Actor) // 如果目的地是个箱子或者Actor，那么走到前一格就可以停了 todo 没考虑自身和目标角色的身宽
            {
                if (gp_PF == destGP_PF)
                {
                    OnSearchForward();
                    arriveDestGP_PF = true;
                    return;
                }
            }

            InvokeTimes++;
            Profiler.BeginSample("AISA_tryAddNode");
            bool available = CheckSpaceAvailableForActorOccupation(node.GridPos3D_PF, actorPos, actorWidth, actorHeight, exceptActorGUID);
            Profiler.EndSample();
            if (available)
            {
                OnSearchForward();
                if (gp_PF == destGP_PF) arriveDestGP_PF = true;
            }
        }

        // 四邻边
        tryAddNode(node.GridPos3D_PF + new GridPos3D(-1, 0, 0));
        if (arriveDestGP_PF) return cached_adjacentNodesList;
        tryAddNode(node.GridPos3D_PF + new GridPos3D(1, 0, 0));
        if (arriveDestGP_PF) return cached_adjacentNodesList;
        tryAddNode(node.GridPos3D_PF + new GridPos3D(0, 0, -1));
        if (arriveDestGP_PF) return cached_adjacentNodesList;
        tryAddNode(node.GridPos3D_PF + new GridPos3D(0, 0, 1));
        return cached_adjacentNodesList;
    }

    public enum DestinationType
    {
        EmptyGrid,
        Box,
        Actor,
    }

    private static float AStarHeuristicsDistance(Node start, Node end)
    {
        return AStarHeuristicsDistance(start.GridPos3D_PF, end.GridPos3D_PF);
    }

    public static float AStarHeuristicsDistance(GridPos3D start, GridPos3D end)
    {
        GridPos3D diff = start - end;
        return diff.magnitude;
    }

    #endregion

    #region UnionFind

    private static List<GridPos3D> cached_UnionFindNodeList = new List<GridPos3D>(256);

    private static Queue<GridPos3D> cached_QueueUnionFind = new Queue<GridPos3D>(256);
    private static bool[,] cached_OccupationUnionFind = new bool[256, 256];

    public static List<GridPos3D> UnionFindNodes(GridPos3D center_PF, Vector3 actorPos, float rangeRadius, int actorWidth, int actorHeight, uint exceptActorGUID)
    {
        cached_UnionFindNodeList.Clear();
        int radius = Mathf.RoundToInt(rangeRadius);
        for (int i = 0; i < cached_OccupationUnionFind.GetLength(0); i++)
        {
            for (int j = 0; j < cached_OccupationUnionFind.GetLength(1); j++)
            {
                cached_OccupationUnionFind[i, j] = false;
            }
        }

        cached_QueueUnionFind.Clear();
        cached_QueueUnionFind.Enqueue(center_PF);
        cached_OccupationUnionFind[0 + radius, 0 + radius] = true; // 从中心开始UnionFind
        cached_UnionFindNodeList.Add(center_PF);
        while (cached_QueueUnionFind.Count > 0)
        {
            GridPos3D head = cached_QueueUnionFind.Dequeue();
            WorldModule curModule = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(head.ConvertPathFindingNodeGPToWorldPosition(actorWidth).ToGridPos3D());
            if (!curModule.IsNotNullAndAvailable()) return cached_UnionFindNodeList;

            // 四邻边
            tryAddNode(head + new GridPos3D(-1, 0, 0));
            tryAddNode(head + new GridPos3D(1, 0, 0));
            tryAddNode(head + new GridPos3D(0, 0, -1));
            tryAddNode(head + new GridPos3D(0, 0, 1));

            void tryAddNode(GridPos3D pathFindingNodeGP)
            {
                if ((pathFindingNodeGP - center_PF).magnitude > radius) return;
                GridPos3D offset = pathFindingNodeGP - center_PF;
                if (cached_OccupationUnionFind[offset.x + radius, offset.z + radius]) return;

                InvokeTimes++;
                Profiler.BeginSample("AISA_UnionFindNodes");
                bool available = CheckSpaceAvailableForActorOccupation(pathFindingNodeGP, actorPos, actorWidth, actorHeight, exceptActorGUID);
                Profiler.EndSample();
                if (available)
                {
                    cached_QueueUnionFind.Enqueue(pathFindingNodeGP);
                    cached_UnionFindNodeList.Add(pathFindingNodeGP);
                    cached_OccupationUnionFind[offset.x + radius, offset.z + radius] = true;
                }
            }
        }

        return cached_UnionFindNodeList;
    }

    #endregion

    /// <summary>
    /// 检查某寻路节点是否能放得下此角色
    /// </summary>
    /// <param name="center_PF">寻路节点坐标，非真实世界坐标，有可能位于半格处，取决于角色身宽</param>
    /// <param name="actorPos"></param>
    /// <param name="actorWidth"></param>
    /// <param name="actorHeight"></param>
    /// <param name="exceptActorGUID">寻路排除的占位Actor对象</param>
    /// <returns></returns>
    public static bool CheckSpaceAvailableForActorOccupation(GridPos3D center_PF, Vector3 actorPos, int actorWidth, int actorHeight, uint exceptActorGUID)
    {
        InvokeTimes++;
        Profiler.BeginSample("AISA");
        for (int occupied_x = 0; occupied_x < actorWidth; occupied_x++)
        for (int occupied_z = 0; occupied_z < actorWidth; occupied_z++)
        {
            GridPos3D gridPos = center_PF + new GridPos3D(occupied_x, 0, occupied_z);
            if (!GetSpaceAvailableForActorHeight(gridPos, actorHeight))
            {
                Profiler.EndSample();
                return false;
            }
        }

        for (int occupied_x = 0; occupied_x < actorWidth; occupied_x++)
        for (int occupied_y = 0; occupied_y < actorHeight; occupied_y++)
        for (int occupied_z = 0; occupied_z < actorWidth; occupied_z++)
        {
            GridPos3D gridPos = center_PF + new GridPos3D(occupied_x, occupied_y, occupied_z);

            // 该范围内无Actor占位
            foreach (Actor enemy in BattleManager.Instance.Enemies)
            {
                if (enemy.GUID == exceptActorGUID) continue;
                if ((enemy.transform.position - actorPos).magnitude > 2f * (enemy.ActorWidth + actorWidth)) continue; // 太远的敌人略过不处理，减少计算量
                foreach (GridPos3D offset in enemy.GetEntityOccupationGPs_Rotated())
                {
                    if (enemy.WorldGP + offset == gridPos)
                    {
                        Profiler.EndSample();
                        return false;
                    }
                }
            }

            if (BattleManager.Instance.Player1.GUID != exceptActorGUID && BattleManager.Instance.Player1.WorldGP == gridPos)
            {
                Profiler.EndSample();
                return false;
            }

            if (BattleManager.Instance.Player2 != null && BattleManager.Instance.Player2.GUID != exceptActorGUID && BattleManager.Instance.Player2.WorldGP == gridPos)
            {
                Profiler.EndSample();
                return false;
            }
        }

        Profiler.EndSample();
        return true;
    }

    public static Vector3 ConvertPathFindingNodeGPToWorldPosition(this GridPos3D worldGP_PF, int actorWidth)
    {
        return worldGP_PF + new Vector3(0.5f, 0, 0.5f) * (actorWidth - 1);
    }

    public static GridPos3D ConvertWorldPositionToPathFindingNodeGP(this Vector3 worldPos, int actorWidth)
    {
        return (worldPos - new Vector3(0.5f, 0, 0.5f) * (actorWidth - 1)).ToGridPos3D();
    }
}
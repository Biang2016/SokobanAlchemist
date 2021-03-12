using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.ObjectPool;
using UnityEngine;

public static class ActorPathFinding
{
    public class Node : IClassPoolObject<Node>
    {
        public int PoolIndex;
        public Node ParentNode;
        public GridPos3D GridPos3D_PF;
        public int F => G + H; // G+H
        public int G; // 从起点到该点的路径长度
        public int H; // 从该点到终点的估计路程

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

    #region AStar PathFinding

    private static List<Node> OpenList = new List<Node>();
    private static List<Node> CloseList = new List<Node>();

    public static bool FindPath(GridPos3D ori, GridPos3D dest,Vector3 actorPos, List<Node> resPath, float keepDistanceMin, float keepDistanceMax, DestinationType destinationType, int actorWidth, int actorHeight, uint exceptActorGUID)
    {
        WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(ori, out WorldModule oriModule, out GridPos3D _);
        WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(dest, out WorldModule destModule, out GridPos3D _);
        if (oriModule != null && destModule != null)
        {
            Node oriNode = NodeFactory.Alloc();
            oriNode.GridPos3D_PF = ori;
            Node destNode = NodeFactory.Alloc();
            destNode.GridPos3D_PF = dest;
            return FindPath(oriNode, destNode, actorPos, resPath, keepDistanceMin, keepDistanceMax, destinationType, actorWidth, actorHeight, exceptActorGUID);
        }

        return false;
    }

    public static bool FindRandomAccessibleDestination(GridPos3D ori_PF, Vector3 actorPos, float rangeRadius, out GridPos3D destination_PF, int actorWidth, int actorHeight, uint exceptActorGUID)
    {
        destination_PF = GridPos3D.Zero;
        WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(ori_PF.ConvertPathFindingNodeGPToWorldPosition(actorWidth).ToGridPos3D(), out WorldModule oriModule, out GridPos3D _);
        if (oriModule != null)
        {
            List<GridPos3D> validNodes = UnionFindNodes(ori_PF, actorPos,rangeRadius, actorWidth, actorHeight, exceptActorGUID);
            if (validNodes.Count == 0) return false;
            destination_PF = CommonUtils.GetRandomFromList(validNodes);
            return true;
        }

        return false;
    }

    private static bool FindPath(Node ori, Node dest,Vector3 actorPos, List<Node> resPath, float keepDistanceMin, float keepDistanceMax, DestinationType destinationType, int actorWidth, int actorHeight, uint exceptActorGUID)
    {
        OpenList.Clear();
        CloseList.Clear();
        OpenList.Add(ori);
        ori.G = 0;
        ori.H = AStarHeuristicsDistance(ori, dest);
        while (OpenList.Count > 0)
        {
            int minF = int.MaxValue;
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
            List<Node> adjacentNodes = GetAdjacentNodesForAStar(minFNode, actorPos, dest.GridPos3D_PF, destinationType, actorWidth, actorHeight, exceptActorGUID);
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

                int newG = AStarHeuristicsDistance(node, minFNode) + minFNode.G;
                if (inOpenList)
                {
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

    private static List<Node> GetAdjacentNodesForAStar(Node node,Vector3 actorPos, GridPos3D destGP_PF, DestinationType destinationType, int actorWidth, int actorHeight, uint exceptActorGUID)
    {
        cached_adjacentNodesList.Clear();
        cached_adjacentNodesList_clone.Clear();
        if (!CheckSpaceAvailableForActorOccupation(node.GridPos3D_PF, actorPos,actorWidth, actorHeight, exceptActorGUID)) return cached_adjacentNodesList;

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

            if (destinationType == DestinationType.Box || destinationType == DestinationType.Actor) // 如果目的地是个箱子或者Actor，那么走到前一格就可以停了 todo 没考虑自身和目标角色的身宽
            {
                if (gp_PF == destGP_PF)
                {
                    OnSearchForward();
                    arriveDestGP_PF = true;
                    return;
                }
            }

            if (CheckSpaceAvailableForActorOccupation(node.GridPos3D_PF, actorPos, actorWidth, actorHeight, exceptActorGUID))
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

    private static int AStarHeuristicsDistance(Node start, Node end)
    {
        return ClientUtils.AStarHeuristicsDistance(start.GridPos3D_PF, end.GridPos3D_PF);
    }

    #endregion

    #region UnionFind

    private static List<GridPos3D> cached_UnionFindNodeList = new List<GridPos3D>(256);

    private static Queue<GridPos3D> cached_QueueUnionFind = new Queue<GridPos3D>(256);
    private static bool[,] cached_OccupationUnionFind = new bool[30, 30];

    private static List<GridPos3D> UnionFindNodes(GridPos3D center_PF,Vector3 actorPos, float rangeRadius, int actorWidth, int actorHeight, uint exceptActorGUID)
    {
        cached_UnionFindNodeList.Clear();
        int radius = Mathf.RoundToInt(rangeRadius);
        int matrixSize = radius * 2 + 1;
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
            WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(head, out WorldModule curModule, out GridPos3D _);
            if (curModule == null) return cached_UnionFindNodeList;

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
                if (CheckSpaceAvailableForActorOccupation(pathFindingNodeGP, actorPos, actorWidth, actorHeight, exceptActorGUID))
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
        bool isBoxBeneath = false;
        for (int occupied_x = 0; occupied_x < actorWidth; occupied_x++)
        for (int occupied_y = 0; occupied_y < actorHeight; occupied_y++)
        for (int occupied_z = 0; occupied_z < actorWidth; occupied_z++)
        {
            GridPos3D gridPos = center_PF + new GridPos3D(occupied_x, occupied_y, occupied_z);

            // 该范围内无Actor占位
            foreach (EnemyActor enemy in BattleManager.Instance.Enemies)
            {
                if (enemy.GUID == exceptActorGUID) continue;
                if ((enemy.transform.position - actorPos).magnitude > 2f * (enemy.ActorWidth + actorWidth)) continue; // 太远的敌人略过不处理，减少计算量
                foreach (GridPos3D offset in enemy.GetEntityOccupationGPs_Rotated())
                {
                    if (enemy.WorldGP + offset == gridPos) return false;
                }
            }

            if (BattleManager.Instance.Player1.GUID != exceptActorGUID && BattleManager.Instance.Player1.WorldGP == gridPos) return false;
            if (BattleManager.Instance.Player2 != null && BattleManager.Instance.Player2.GUID != exceptActorGUID && BattleManager.Instance.Player2.WorldGP == gridPos) return false;
            Box box = WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(gridPos, out WorldModule _, out GridPos3D _, false);
            if (box != null && !box.Passable) return false;
            if (occupied_y == 0)
            {
                Box box_beneath = WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(gridPos + GridPos3D.Down, out WorldModule _, out GridPos3D _, false);
                if (box_beneath != null && !box_beneath.Passable) isBoxBeneath = true; // 下方要至少要有一格有支撑箱子
            }
        }

        if (!isBoxBeneath) return false;
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
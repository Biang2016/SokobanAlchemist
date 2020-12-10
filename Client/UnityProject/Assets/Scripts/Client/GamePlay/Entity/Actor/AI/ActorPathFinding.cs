using System.Collections.Generic;
using BiangStudio;
using BiangStudio.CloneVariant;
using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ObjectPool;
using UnityEngine;

public class ActorPathFinding
{
    public class Node : IClassPoolObject<Node>
    {
        public int PoolIndex;
        public Node ParentNode;
        public GridPos3D GridPos3D;
        public int F => G + H; // G+H
        public int G; // 从起点到该点的路径长度
        public int H; // 从该点到终点的估计路程

        public Node Create()
        {
            return new Node();
        }

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
            GridPos3D = GridPos3D.Zero;
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

    public static bool FindPath(GridPos3D ori, GridPos3D dest, List<Node> resPath, float keepDistanceMin, float keepDistanceMax, DestinationType destinationType)
    {
        WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(ori, out WorldModule oriModule, out GridPos3D _);
        WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(dest, out WorldModule destModule, out GridPos3D _);
        if (oriModule != null && destModule != null)
        {
            Node oriNode = NodeFactory.Alloc();
            oriNode.GridPos3D = ori;
            Node destNode = NodeFactory.Alloc();
            destNode.GridPos3D = dest;
            FindPath(oriNode, destNode, resPath, keepDistanceMin, keepDistanceMax, destinationType);
            return true;
        }

        return false;
    }

    public static bool FindRandomAccessibleDestination(GridPos3D ori, float rangeRadius, out GridPos3D gp)
    {
        gp = GridPos3D.Zero;
        WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(ori, out WorldModule oriModule, out GridPos3D _);
        if (oriModule != null)
        {
            List<GridPos3D> validNodes = UnionFindNodes(ori, rangeRadius);
            if (validNodes.Count == 0) return false;
            gp = CommonUtils.GetRandomFromList(validNodes);
            return true;
        }

        return false;
    }

    private static bool FindPath(Node ori, Node dest, List<Node> resPath, float keepDistanceMin, float keepDistanceMax, DestinationType destinationType)
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
            List<Node> adjacentNodes = GetAdjacentNodesForAStar(minFNode, dest.GridPos3D, destinationType);
            List<Node> uselessAdjacentNodes = cached_adjacentNodesList_clone; // 对象引用仍为同一个,此list只是为了避免modify collection inside foreach
            foreach (Node node in adjacentNodes)
            {
                bool inCloseList = false;
                foreach (Node nodeInCloseList in CloseList)
                {
                    if (node.GridPos3D == nodeInCloseList.GridPos3D)
                    {
                        inCloseList = true;
                        break;
                    }
                }

                if (inCloseList) continue;

                bool inOpenList = false;
                foreach (Node nodeInOpenList in OpenList)
                {
                    if (node.GridPos3D == nodeInOpenList.GridPos3D)
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

                    float diffToDest = (node.GridPos3D - dest.GridPos3D).ToVector3().magnitude;
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
                                pathNode.GridPos3D = nodePtr.GridPos3D;
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

    private static List<Node> GetAdjacentNodesForAStar(Node node, GridPos3D destGP, DestinationType destinationType)
    {
        cached_adjacentNodesList.Clear();
        cached_adjacentNodesList_clone.Clear();
        WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(node.GridPos3D, out WorldModule curModule, out GridPos3D _);
        if (curModule == null) return cached_adjacentNodesList;

        void tryAddNode(GridPos3D gp)
        {
            if (destinationType == DestinationType.Box || destinationType == DestinationType.Actor)
            {
                if (gp == destGP)
                {
                    Node leftNode = NodeFactory.Alloc();
                    leftNode.GridPos3D = gp;
                    leftNode.ParentNode = node;
                    cached_adjacentNodesList.Add(leftNode);
                    cached_adjacentNodesList_clone.Add(leftNode);
                    return;
                }
            }

            Box box = WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(gp, out WorldModule module, out GridPos3D _);
            Box box_beneath = WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(gp + new GridPos3D(0, -1, 0), out WorldModule module_beneath, out GridPos3D _, false);
            if (module != null && (box == null || box.Passable) && module_beneath != null && box_beneath != null)
            {
                if (gp == destGP)
                {
                    Node leftNode = NodeFactory.Alloc();
                    leftNode.GridPos3D = gp;
                    leftNode.ParentNode = node;
                    cached_adjacentNodesList.Add(leftNode);
                    cached_adjacentNodesList_clone.Add(leftNode);
                    return;
                }

                //bool isActorOnTheWay = false;
                //foreach (EnemyActor enemy in BattleManager.Instance.Enemies)
                //{
                //    if (enemy.CurGP == gp)
                //    {
                //        isActorOnTheWay = true;
                //        break;
                //    }
                //}

                //if (BattleManager.Instance.Player1.CurGP == gp) isActorOnTheWay = true;
                //if (BattleManager.Instance.Player2 != null && BattleManager.Instance.Player2.CurGP == gp) isActorOnTheWay = true;
                //if (!isActorOnTheWay)
                {
                    Node leftNode = NodeFactory.Alloc();
                    leftNode.GridPos3D = gp;
                    leftNode.ParentNode = node;
                    cached_adjacentNodesList.Add(leftNode);
                    cached_adjacentNodesList_clone.Add(leftNode);
                }
            }
        }

        // 四邻边
        tryAddNode(node.GridPos3D + new GridPos3D(-1, 0, 0));
        tryAddNode(node.GridPos3D + new GridPos3D(1, 0, 0));
        tryAddNode(node.GridPos3D + new GridPos3D(0, 0, -1));
        tryAddNode(node.GridPos3D + new GridPos3D(0, 0, 1));
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
        return ClientUtils.AStarHeuristicsDistance(start.GridPos3D, end.GridPos3D);
    }

    #endregion

    #region UnionFind

    private static List<GridPos3D> cached_UnionFindNodeList = new List<GridPos3D>(100);

    private static Queue<GridPos3D> cached_QueueUnionFind = new Queue<GridPos3D>(50);
    private static bool[,] cached_OccupationUnionFind = new bool[20, 20];

    private static List<GridPos3D> UnionFindNodes(GridPos3D center, float rangeRadius)
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
        cached_QueueUnionFind.Enqueue(center);
        cached_OccupationUnionFind[0 + radius, 0 + radius] = true;
        cached_UnionFindNodeList.Add(center);
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

            void tryAddNode(GridPos3D gp)
            {
                if ((gp.ToVector3() - center.ToVector3()).magnitude > radius) return;
                GridPos3D offset = gp - center;
                if (cached_OccupationUnionFind[offset.x + radius, offset.z + radius]) return;
                foreach (EnemyActor enemy in BattleManager.Instance.Enemies)
                {
                    if (enemy.CurWorldGP == gp)
                    {
                        return;
                    }
                }

                if (BattleManager.Instance.Player1.CurWorldGP == gp) return;
                if (BattleManager.Instance.Player2 != null && BattleManager.Instance.Player2.CurWorldGP == gp) return;

                Box box = WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(gp, out WorldModule module, out GridPos3D _);
                if (module != null && box == null)
                {
                    cached_QueueUnionFind.Enqueue(gp);
                    cached_UnionFindNodeList.Add(gp);
                    cached_OccupationUnionFind[offset.x + radius, offset.z + radius] = true;
                }
            }
        }

        return cached_UnionFindNodeList;
    }

    #endregion
}
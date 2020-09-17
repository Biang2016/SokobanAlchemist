using System.Collections.Generic;
using System.Linq;
using BiangStudio;
using BiangStudio.GameDataFormat.Grid;
using BiangStudio.GamePlay;
using UnityEngine;

public class ActorPathFinding
{
    public class Node
    {
        public Node ParentNode;
        public GridPos3D GridPos3D;
        public int F => G + H; // G+H
        public int G; // 从起点到该点的路径长度
        public int H; // 从该点到终点的估计路程
    }

    #region AStar PathFinding

    private static LinkedList<Node> OpenList = new LinkedList<Node>();
    private static LinkedList<Node> CloseList = new LinkedList<Node>();

    public static LinkedList<GridPos3D> FindPath(GridPos3D ori, GridPos3D dest, float keepDistanceMin, float keepDistanceMax)
    {
        WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(ori, out WorldModule oriModule, out GridPos3D _);
        WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(dest, out WorldModule destModule, out GridPos3D _);
        if (oriModule != null && destModule != null)
        {
            return FindPath(new Node {GridPos3D = ori}, new Node {GridPos3D = dest}, keepDistanceMin, keepDistanceMax);
        }

        return null;
    }

    public static bool FindRandomAccessibleDestination(GridPos3D ori, float rangeRadius, out GridPos3D gp)
    {
        gp = GridPos3D.Zero;
        WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(ori, out WorldModule oriModule, out GridPos3D _);
        if (oriModule != null)
        {
            List<GridPos3D> validNodes = UnionFindNodes(ori, rangeRadius);
            if (validNodes.Count == 0) return false;
            gp = CommonUtils.GetRandomFromList(validNodes, 1)[0];
            return true;
        }

        return false;
    }

    private static LinkedList<GridPos3D> FindPath(Node ori, Node dest, float keepDistanceMin, float keepDistanceMax)
    {
        OpenList.Clear();
        CloseList.Clear();
        OpenList.AddFirst(ori);
        ori.G = 0;
        ori.H = AStarHeuristicsDistance(ori, dest);
        while (OpenList.Count > 0)
        {
            int minF = int.MaxValue;
            Node minFNode = OpenList.First.Value;
            foreach (Node node in OpenList)
            {
                if (node.F < minF)
                {
                    minF = node.F;
                    minFNode = node;
                }
            }

            OpenList.Remove(minFNode);
            CloseList.AddFirst(minFNode);
            List<Node> adjacentNodes = GetAdjacentNodesForAStar(minFNode, dest.GridPos3D);
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
                    OpenList.AddFirst(node);
                    node.ParentNode = minFNode;
                    node.G = newG;
                    node.H = AStarHeuristicsDistance(node, dest);

                    float diffToDest = (node.GridPos3D - dest.GridPos3D).ToVector3().magnitude;
                    if (diffToDest <= keepDistanceMax && diffToDest >= keepDistanceMin)
                    {
                        LinkedList<GridPos3D> path = new LinkedList<GridPos3D>();
                        Node nodePtr = node;
                        while (nodePtr != null)
                        {
                            path.AddFirst(nodePtr.GridPos3D);
                            nodePtr = nodePtr.ParentNode;
                        }

                        return path;
                    }
                }
            }
        }

        return null;
    }

    private static List<Node> GetAdjacentNodesForAStar(Node node, GridPos3D destGP)
    {
        List<Node> res = new List<Node>();
        WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(node.GridPos3D, out WorldModule curModule, out GridPos3D _);
        if (curModule == null) return res;

        void tryAddNode(GridPos3D gp)
        {
            if (gp == destGP)
            {
                Node leftNode = new Node {GridPos3D = gp, ParentNode = node};
                res.Add(leftNode);
                return;
            }

            Box box = WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(gp, out WorldModule module, out GridPos3D _);
            if (module != null && box == null)
            {
                bool isActorOnTheWay = false;
                foreach (EnemyActor enemy in BattleManager.Instance.Enemies)
                {
                    if (enemy.CurGP == gp)
                    {
                        isActorOnTheWay = true;
                        break;
                    }
                }

                if (BattleManager.Instance.Player1.CurGP == gp) isActorOnTheWay = true;
                if (BattleManager.Instance.Player2 != null && BattleManager.Instance.Player2.CurGP == gp) isActorOnTheWay = true;
                if (!isActorOnTheWay)
                {
                    Node leftNode = new Node {GridPos3D = gp, ParentNode = node};
                    res.Add(leftNode);
                }
            }
        }

        // 四邻边
        tryAddNode(node.GridPos3D + new GridPos3D(-1, 0, 0));
        tryAddNode(node.GridPos3D + new GridPos3D(1, 0, 0));
        tryAddNode(node.GridPos3D + new GridPos3D(0, 0, -1));
        tryAddNode(node.GridPos3D + new GridPos3D(0, 0, 1));
        return res;
    }

    private static int AStarHeuristicsDistance(Node start, Node end)
    {
        return ClientUtils.AStarHeuristicsDistance(start.GridPos3D, end.GridPos3D);
    }

    #endregion

    #region UnionFind

    private static List<GridPos3D> UnionFindNodes(GridPos3D center, float rangeRadius)
    {
        int radius = Mathf.RoundToInt(rangeRadius);
        int matrixSize = radius * 2 + 1;
        bool[,] occupation = new bool[matrixSize, matrixSize];
        List<GridPos3D> res = new List<GridPos3D>();
        Queue<GridPos3D> queue = new Queue<GridPos3D>();
        queue.Enqueue(center);
        occupation[0 + radius, 0 + radius] = true;
        res.Add(center);
        while (queue.Count > 0)
        {
            GridPos3D head = queue.Dequeue();
            WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(head, out WorldModule curModule, out GridPos3D _);
            if (curModule == null) return res;

            // 四邻边
            tryAddNode(head + new GridPos3D(-1, 0, 0));
            tryAddNode(head + new GridPos3D(1, 0, 0));
            tryAddNode(head + new GridPos3D(0, 0, -1));
            tryAddNode(head + new GridPos3D(0, 0, 1));

            void tryAddNode(GridPos3D gp)
            {
                if ((gp.ToVector3() - center.ToVector3()).magnitude > radius) return;
                GridPos3D offset = gp - center;
                if (occupation[offset.x + radius, offset.z + radius]) return;
                foreach (EnemyActor enemy in BattleManager.Instance.Enemies)
                {
                    if (enemy.CurGP == gp)
                    {
                        return;
                    }
                }

                if (BattleManager.Instance.Player1.CurGP == gp) return;
                if (BattleManager.Instance.Player2 != null && BattleManager.Instance.Player2.CurGP == gp) return;

                Box box = WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(gp, out WorldModule module, out GridPos3D _);
                if (module != null && box == null)
                {
                    queue.Enqueue(gp);
                    res.Add(gp);
                    occupation[offset.x + radius, offset.z + radius] = true;
                }
            }
        }

        return res;
    }

    #endregion
}
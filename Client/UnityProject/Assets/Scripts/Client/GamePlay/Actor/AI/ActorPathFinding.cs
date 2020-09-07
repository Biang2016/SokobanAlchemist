using System.Collections.Generic;
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

    private static LinkedList<Node> OpenList = new LinkedList<Node>();
    private static LinkedList<Node> CloseList = new LinkedList<Node>();

    public static LinkedList<GridPos3D> FindPath(GridPos3D ori, GridPos3D dest)
    {
        WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(ori, out WorldModule oriModule, out GridPos3D _);
        Box destBox = WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(dest, out WorldModule destModule, out GridPos3D _);
        if (oriModule != null && destModule != null && destBox == null)
        {
            return FindPath(new Node {GridPos3D = ori}, new Node {GridPos3D = dest});
        }

        return null;
    }

    private static LinkedList<GridPos3D> FindPath(Node ori, Node dest)
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
            List<Node> adjacentNodes = GetAdjacentNodes(minFNode);
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
                }

                if (node.GridPos3D == dest.GridPos3D)
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

        return null;
    }

    private static List<Node> GetAdjacentNodes(Node node)
    {
        List<Node> res = new List<Node>();
        WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(node.GridPos3D, out WorldModule curModule, out GridPos3D _);
        if (curModule == null) return res;

        void tryAddNode(GridPos3D gp)
        {
            Box box = WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(gp, out WorldModule module, out GridPos3D _);
            if (module != null && box == null)
            {
                Node leftNode = new Node {GridPos3D = gp, ParentNode = node};
                res.Add(leftNode);
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
        GridPos3D diff = start.GridPos3D - end.GridPos3D;
        return Mathf.Abs(diff.x) + Mathf.Abs(diff.z);
    }
}
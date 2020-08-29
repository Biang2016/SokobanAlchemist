using System.Collections.Generic;
using UnityEngine;

namespace BiangStudio.GameDataFormat.Grid
{
    public static class GridUtils
    {
        public static Vector3 GetPosByMousePos(Transform parentTransform, Ray ray, Vector3 planeNormal, int gridSize)
        {
            Vector3 intersect = CommonUtils.GetIntersectWithLineAndPlane(ray.origin, ray.direction, planeNormal, parentTransform.position);
            Vector3 rot_intersect = parentTransform.InverseTransformPoint(intersect);
            return rot_intersect;
        }

        public static GridPos GetGridPosByMousePos(Transform parentTransform, Ray ray, Vector3 planeNormal, int gridSize)
        {
            Vector3 rot_intersect = GetPosByMousePos(parentTransform, ray, planeNormal, gridSize);
            GridPos local_GP = GridPos.GetGridPosByPointXZ(rot_intersect + Vector3.one * gridSize / 2f, 1);

            int x = Mathf.FloorToInt(local_GP.x / gridSize) * gridSize;
            int z = Mathf.FloorToInt(local_GP.z / gridSize) * gridSize;
            return new GridPos(x, z);
        }

        public static Vector3 GetIntersectWithLineAndPlane(Vector3 point, Vector3 direct, Vector3 planeNormal, Vector3 planePoint)
        {
            float d = Vector3.Dot(planePoint - point, planeNormal) / Vector3.Dot(direct.normalized, planeNormal);
            return d * direct.normalized + point;
        }

        public static string GridPositionListToString(this List<GridPos> gridPositions)
        {
            string res = "";
            foreach (GridPos gp in gridPositions)
            {
                res += gp + ", ";
            }

            return res;
        }

        public static GridPosR.OrientationFlag ToFlag(this GridPosR.Orientation ori)
        {
            switch (ori)
            {
                case GridPosR.Orientation.Up:
                {
                    return GridPosR.OrientationFlag.Up;
                }
                case GridPosR.Orientation.Down:
                {
                    return GridPosR.OrientationFlag.Down;
                }
                case GridPosR.Orientation.Left:
                {
                    return GridPosR.OrientationFlag.Left;
                }
                case GridPosR.Orientation.Right:
                {
                    return GridPosR.OrientationFlag.Right;
                }
            }

            return 0;
        }

        public static GridRect GetBoundingRectFromListGridPos(this List<GridPos> gridPositions)
        {
            int X_min = 999;
            int X_max = -999;
            int Z_min = 999;
            int Z_max = -999;
            foreach (GridPos gp in gridPositions)
            {
                if (gp.x < X_min)
                {
                    X_min = gp.x;
                }

                if (gp.x > X_max)
                {
                    X_max = gp.x;
                }

                if (gp.z < Z_min)
                {
                    Z_min = gp.z;
                }

                if (gp.z > Z_max)
                {
                    Z_max = gp.z;
                }
            }

            return new GridRect(X_min, Z_min, X_max - X_min + 1, Z_max - Z_min + 1);
        }

        public static void GetConnectionMatrix(this List<GridPos> gridPositions, out bool[,] connectionMatrix, out GridPos offset)
        {
            connectionMatrix = null;
            offset = GridPos.Zero;
            if (gridPositions.Count == 0) return;

            GridRect gridRect = gridPositions.GetBoundingRectFromListGridPos();
            offset = -gridRect.position;
            gridRect.position = GridPos.Zero;
            connectionMatrix = new bool[gridRect.size.x, gridRect.size.z];
            foreach (GridPos gp in gridPositions)
            {
                connectionMatrix[gp.x + offset.x, gp.z + offset.z] = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gridPos"></param>
        /// <param name="connectionMatrix"></param>
        /// <param name="offset"></param>
        /// <param name="adjacentConnection"></param>
        /// <param name="diagonalConnection">Up->TopRight, Right->BottomRight, Down->BottomLeft, Left->TopLeft</param>
        /// <returns></returns>
        public static void GetConnection(this GridPos gridPos, bool[,] connectionMatrix, GridPos offset, out GridPosR.OrientationFlag adjacentConnection, out GridPosR.OrientationFlag diagonalConnection)
        {
            adjacentConnection = GridPosR.OrientationFlag.None;
            diagonalConnection = GridPosR.OrientationFlag.None;
            GridPos localGP = gridPos + offset;
            if (localGP.x < 0 || localGP.x >= connectionMatrix.GetLength(0) || localGP.z < 0 || localGP.z >= connectionMatrix.GetLength(1))
            {
                return;
            }

            if (localGP.x + 1 < connectionMatrix.GetLength(0) && connectionMatrix[localGP.x + 1, localGP.z]) adjacentConnection |= GridPosR.OrientationFlag.Right;
            if (localGP.x - 1 >= 0 && connectionMatrix[localGP.x - 1, localGP.z]) adjacentConnection |= GridPosR.OrientationFlag.Left;
            if (localGP.z + 1 < connectionMatrix.GetLength(1) && connectionMatrix[localGP.x, localGP.z + 1]) adjacentConnection |= GridPosR.OrientationFlag.Up;
            if (localGP.z - 1 >= 0 && connectionMatrix[localGP.x, localGP.z - 1]) adjacentConnection |= GridPosR.OrientationFlag.Down;

            if (localGP.x + 1 < connectionMatrix.GetLength(0) && localGP.z + 1 < connectionMatrix.GetLength(1) && connectionMatrix[localGP.x + 1, localGP.z + 1]) diagonalConnection |= GridPosR.OrientationFlag.Up;
            if (localGP.x + 1 < connectionMatrix.GetLength(0) && localGP.z - 1 >= 0 && connectionMatrix[localGP.x + 1, localGP.z - 1]) diagonalConnection |= GridPosR.OrientationFlag.Right;
            if (localGP.x - 1 >= 0 && localGP.z - 1 >= 0 && connectionMatrix[localGP.x - 1, localGP.z - 1]) diagonalConnection |= GridPosR.OrientationFlag.Down;
            if (localGP.x - 1 >= 0 && localGP.z + 1 < connectionMatrix.GetLength(1) && connectionMatrix[localGP.x - 1, localGP.z + 1]) diagonalConnection |= GridPosR.OrientationFlag.Left;
        }
    }
}
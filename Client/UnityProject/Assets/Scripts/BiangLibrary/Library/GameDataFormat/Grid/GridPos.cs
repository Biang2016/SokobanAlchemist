using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

namespace BiangLibrary.GameDataFormat.Grid
{
    [Serializable]
    public struct GridPosR
    {
        public int x;
        public int z;
        public Orientation orientation;

        private static readonly GridPosR zeroGPR = new GridPosR(0, 0, Orientation.Up);
        public static GridPosR Zero => zeroGPR;
        private static readonly GridPos oneGPR = new GridPosR(1, 1, Orientation.Up);
        public static GridPosR One => oneGPR;

        public GridPosR(int x, int z)
        {
            this.x = x;
            this.z = z;
            orientation = Orientation.Up;
        }

        public GridPosR(int x, int z, Orientation orientation)
        {
            this.x = x;
            this.z = z;
            this.orientation = orientation;
        }

        public float magnitude => Mathf.Sqrt(x * x + z * z);
        public Vector3 normalized => ((Vector3) this).normalized;

        public static Orientation GetOrientationByLocalTrans(Transform transform)
        {
            int rotY = Mathf.RoundToInt(transform.localRotation.eulerAngles.y / 90f) % 4;
            return (Orientation) rotY;
        }

        public static Orientation GetOrientationByTrans(Transform transform)
        {
            int rotY = Mathf.RoundToInt(transform.rotation.eulerAngles.y / 90f) % 4;
            return (Orientation) rotY;
        }

        public static GridPosR GetGridPosByLocalTrans(Transform transform, int gridSize)
        {
            int x = Mathf.FloorToInt(transform.localPosition.x / gridSize) * gridSize;
            int z = Mathf.FloorToInt(transform.localPosition.z / gridSize) * gridSize;
            int rotY = Mathf.RoundToInt(transform.localRotation.eulerAngles.y / 90f) % 4;
            return new GridPosR(x, z, (Orientation) rotY);
        }

        public static GridPos GetGridPosByTrans(Transform transform, int gridSize)
        {
            int x = Mathf.FloorToInt(transform.position.x / gridSize) * gridSize;
            int z = Mathf.FloorToInt(transform.position.z / gridSize) * gridSize;
            int rotY = Mathf.RoundToInt(transform.rotation.eulerAngles.y / 90f) % 4;
            return new GridPosR(x, z, (Orientation) rotY);
        }

        public static GridPos GetGridPosByPoint(Vector3 position, int gridSize)
        {
            int x = Mathf.FloorToInt(position.x / gridSize) * gridSize;
            int z = Mathf.FloorToInt(position.z / gridSize) * gridSize;
            return new GridPosR(x, z, Orientation.Up);
        }

        public static void ApplyGridPosToLocalTrans(GridPosR gridPos, Transform transform, int gridSize)
        {
            float x = gridPos.x * gridSize;
            float z = gridPos.z * gridSize;
            float rotY = (int) gridPos.orientation * 90f;
            transform.localPosition = new Vector3(x, transform.localPosition.y, z);
            transform.localRotation = Quaternion.Euler(0, rotY, 0);
        }

        public static Orientation RotateOrientationClockwise90(Orientation orientation)
        {
            return (Orientation) (((int) orientation + 1) % 4);
        }

        public static Orientation RotateOrientationAntiClockwise90(Orientation orientation)
        {
            return (Orientation) (((int) orientation - 1 + 4) % 4);
        }

        public static List<GridPos> TransformOccupiedPositions(GridPosR localGridPos, List<GridPos> ori_OccupiedPositions)
        {
            List<GridPos> resGP = new List<GridPos>();

            foreach (GridPos oriGP in ori_OccupiedPositions)
            {
                GridPos temp_rot = GridPos.RotateGridPos(oriGP, localGridPos.orientation);
                GridPos final = temp_rot + (GridPos) localGridPos;
                resGP.Add(final);
            }

            return resGP;
        }

        public static GridPos TransformOccupiedPosition(GridPosR localGridPos, GridPos ori_OccupiedPosition)
        {
            GridPos temp_rot = GridPos.RotateGridPos(ori_OccupiedPosition, localGridPos.orientation);
            GridPos final = temp_rot + (GridPos) localGridPos;
            return final;
        }

        public bool Equals(GridPosR gp)
        {
            return gp.x == x && gp.z == z && gp.orientation == orientation;
        }

        public bool Equals(GridPos gp)
        {
            return gp.x == x && gp.z == z;
        }

        /// <summary>
        /// clamp X, Z inside [-1, 1]
        /// </summary>
        /// <returns></returns>
        public GridPos ClampOneUnit()
        {
            int newX = 0;
            int newY = 0;
            int newZ = 0;
            if (x > 0) newX = 1;
            if (x < 0) newX = -1;
            if (x == 0) newX = 0;
            if (z > 0) newZ = 1;
            if (z < 0) newZ = -1;
            if (z == 0) newZ = 0;
            return new GridPos(newX, newZ);
        }

        public static GridPos operator -(GridPosR a, GridPosR b)
        {
            return new GridPosR(a.x - b.x, a.z - b.z, a.orientation);
        }

        public static GridPosR operator +(GridPosR a, GridPosR b)
        {
            return new GridPosR(a.x + b.x, a.z + b.z, a.orientation);
        }

        public static GridPosR operator +(GridPosR a, GridPos b)
        {
            return new GridPosR(a.x + b.x, a.z + b.z, a.orientation);
        }

        public static GridPosR operator -(GridPosR a, GridPos b)
        {
            return new GridPosR(a.x - b.x, a.z - b.z, a.orientation);
        }

        public static GridPosR operator +(GridPos a, GridPosR b)
        {
            return new GridPosR(a.x + b.x, a.z + b.z, b.orientation);
        }

        public static GridPosR operator *(GridPosR a, int b)
        {
            return new GridPosR(a.x * b, a.z * b, a.orientation);
        }

        public static GridPosR operator *(int b, GridPosR a)
        {
            return new GridPosR(a.x * b, a.z * b, a.orientation);
        }

        public static implicit operator Vector3(GridPosR gpr)
        {
            return new Vector3(gpr.x, 0, gpr.z);
        }

        public override string ToString()
        {
            return $"({x}, {z}, {orientation})";
        }

        public string ToShortString()
        {
            return $"({x}, {z})";
        }

        public override int GetHashCode()
        {
            return (x.GetHashCode() + z.GetHashCode() + orientation.GetHashCode()).GetHashCode();
        }

        public enum Orientation
        {
            Up = 0,
            Right = 1,
            Down = 2,
            Left = 3,
        }

        [Flags]
        public enum OrientationFlag
        {
            None = 0,
            Up = 1 << 0,
            Right = 1 << 1,
            Down = 1 << 2,
            Left = 1 << 3,
            All = ~0,
        }
    }

    [Serializable]
    public struct GridPos
    {
        public int x;
        public int z;

        private static readonly GridPos zeroGP = new GridPos(0, 0);
        public static GridPos Zero => zeroGP;
        private static readonly GridPos rightGP = new GridPos(1, 0);
        public static GridPos Right => rightGP;
        private static readonly GridPos leftGP = new GridPos(-1, 0);
        public static GridPos Left => leftGP;
        private static readonly GridPos forwardGP = new GridPos(0, 1);
        public static GridPos Forward => forwardGP;
        private static readonly GridPos backGP = new GridPos(0, -1);
        public static GridPos Back => backGP;

        public GridPos(int x, int z)
        {
            this.x = x;
            this.z = z;
        }

        public float magnitude => Mathf.Sqrt(x * x + z * z);
        public Vector3 normalized => ((Vector3) this).normalized;

        public static GridPos GetGridPosByLocalTransXZ(Transform transform, int gridSize)
        {
            return GetGridPosByPointXZ(transform.localPosition, gridSize);
        }

        public static GridPos GetGridPosByTransXZ(Transform transform, int gridSize)
        {
            return GetGridPosByPointXZ(transform.position, gridSize);
        }

        public static GridPos GetGridPosByPointXZ(Vector3 position, int gridSize)
        {
            int x = Mathf.RoundToInt(position.x / gridSize);
            int z = Mathf.RoundToInt(position.z / gridSize);
            return new GridPos(x, z);
        }

        public static GridPos GetGridPosByPointXY(Vector3 position, int gridSize)
        {
            int x = Mathf.RoundToInt(position.x / gridSize);
            int y = Mathf.RoundToInt(position.y / gridSize);
            return new GridPos(x, y);
        }

        public static void ApplyGridPosToLocalTransXZ(GridPos gridPos, Transform transform, int gridSize)
        {
            float x = gridPos.x * gridSize;
            float z = gridPos.z * gridSize;
            transform.localPosition = new Vector3(x, transform.localPosition.y, z);
        }

        public static GridPos RotateGridPos(GridPos oriGP, GridPosR.Orientation orientation)
        {
            switch (orientation)
            {
                case GridPosR.Orientation.Up:
                {
                    return oriGP;
                }
                case GridPosR.Orientation.Right:
                {
                    return new GridPos(oriGP.z, -oriGP.x);
                }
                case GridPosR.Orientation.Down:
                {
                    return new GridPos(-oriGP.x, -oriGP.z);
                }
                case GridPosR.Orientation.Left:
                {
                    return new GridPos(-oriGP.z, oriGP.x);
                }
            }

            return new GridPos(0, 0);
        }

        public static GridPos GetLocalGridPosByCenter(GridPosR center, GridPos gp_global)
        {
            GridPos diff = gp_global - (GridPos) center;
            GridPos localGP = RotateGridPos(diff, (GridPosR.Orientation) (4 - (int) center.orientation));
            return localGP;
        }

        public static List<GridPos> TransformOccupiedPositions(GridPos localGridPos, List<GridPos> ori_OccupiedPositions)
        {
            for (int i = 0; i < ori_OccupiedPositions.Count; i++)
            {
                ori_OccupiedPositions[i] += localGridPos;
            }

            return ori_OccupiedPositions;
        }

        public bool Equals(GridPos gp)
        {
            return gp.x == x && gp.z == z;
        }

        public bool Equals(GridPosR r)
        {
            return r.x == x && r.z == z;
        }

        public static GridPos operator -(GridPos a)
        {
            return new GridPos(-a.x, -a.z);
        }

        public static GridPos operator -(GridPos a, GridPos b)
        {
            return new GridPos(a.x - b.x, a.z - b.z);
        }

        public static GridPos operator +(GridPos a, GridPos b)
        {
            return new GridPos(a.x + b.x, a.z + b.z);
        }

        public static GridPos operator *(GridPos a, int b)
        {
            return new GridPos(a.x * b, a.z * b);
        }

        public static GridPos operator *(int b, GridPos a)
        {
            return new GridPos(a.x * b, a.z * b);
        }

        public static implicit operator GridPos(GridPosR r)
        {
            return new GridPos(r.x, r.z);
        }

        public static implicit operator GridPosR(GridPos gp)
        {
            return new GridPosR(gp.x, gp.z, GridPosR.Orientation.Up);
        }

        public static implicit operator Vector3(GridPos gp)
        {
            return new Vector3(gp.x, 0, gp.z);
        }

        public override string ToString()
        {
            return $"({x},{z})";
        }

        public override int GetHashCode()
        {
            return (x.GetHashCode() + z.GetHashCode()).GetHashCode();
        }
    }

    [Serializable]
    public struct GridPos3D
    {
        public int x;
        public int y;
        public int z;

        private static readonly GridPos3D zeroGP = new GridPos3D(0, 0, 0);
        public static GridPos3D Zero => zeroGP;
        private static readonly GridPos3D rightGP = new GridPos3D(1, 0, 0);
        public static GridPos3D Right => rightGP;
        private static readonly GridPos3D leftGP = new GridPos3D(-1, 0, 0);
        public static GridPos3D Left => leftGP;
        private static readonly GridPos3D upGP = new GridPos3D(0, 1, 0);
        public static GridPos3D Up => upGP;
        private static readonly GridPos3D downGP = new GridPos3D(0, -1, 0);
        public static GridPos3D Down => downGP;
        private static readonly GridPos3D forwardGP = new GridPos3D(0, 0, 1);
        public static GridPos3D Forward => forwardGP;
        private static readonly GridPos3D backGP = new GridPos3D(0, 0, -1);
        public static GridPos3D Back => backGP;

        public GridPos3D(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static GridPos3D GetGridPosByLocalTrans(Transform transform, int gridSize)
        {
            return GetGridPosByPoint(transform.localPosition, gridSize);
        }

        public static GridPos3D GetGridPosByTrans(Transform transform, int gridSize)
        {
            return GetGridPosByPoint(transform.position, gridSize);
        }

        public static GridPos3D GetGridPosByPoint(Vector3 position, int gridSize)
        {
            int x = Mathf.RoundToInt(position.x / gridSize);
            int y = Mathf.RoundToInt(position.y / gridSize);
            int z = Mathf.RoundToInt(position.z / gridSize);
            return new GridPos3D(x, y, z);
        }

        public static void ApplyGridPosToLocalTrans(GridPos3D gridPos, Transform transform, int gridSize)
        {
            transform.localPosition = GetLocalPositionByGridPos(gridPos, transform, gridSize);
        }

        public static Vector3 GetLocalPositionByGridPos(GridPos3D gridPos, Transform transform, int gridSize)
        {
            float x = gridPos.x * gridSize;
            float y = gridPos.y * gridSize;
            float z = gridPos.z * gridSize;
            return new Vector3(x, y, z);
        }

        public static List<GridPos3D> TransformOccupiedPositions_XZ(GridPosR.Orientation orientation, List<GridPos3D> ori_OccupiedPositions)
        {
            List<GridPos3D> res = new List<GridPos3D>();

            foreach (GridPos3D oriGP in ori_OccupiedPositions)
            {
                GridPos temp_rot = GridPos.RotateGridPos(new GridPos(oriGP.x, oriGP.z), orientation);
                GridPos3D final = new GridPos3D(temp_rot.x, oriGP.y, temp_rot.z);
                res.Add(final);
            }

            return res;
        }

        public static GridPos3D GetNearestGPFromList(GridPos3D srcGP, List<GridPos3D> possibleGPs)
        {
            float minDistance = float.MaxValue;
            GridPos3D nearestGP = Zero;
            foreach (GridPos3D possibleGP in possibleGPs)
            {
                float dist = (srcGP - possibleGP).magnitude;
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearestGP = possibleGP;
                }
            }

            return nearestGP;
        }

        public static implicit operator Vector3(GridPos3D gp)
        {
            return new Vector3(gp.x, gp.y, gp.z);
        }

        /// <summary>
        /// clamp X, Y, Z inside [-1, 1]
        /// </summary>
        /// <returns></returns>
        public GridPos3D ClampOneUnit()
        {
            int newX = 0;
            int newY = 0;
            int newZ = 0;
            if (x > 0) newX = 1;
            if (x < 0) newX = -1;
            if (x == 0) newX = 0;
            if (y > 0) newY = 1;
            if (y < 0) newY = -1;
            if (y == 0) newY = 0;
            if (z > 0) newZ = 1;
            if (z < 0) newZ = -1;
            if (z == 0) newZ = 0;
            return new GridPos3D(newX, newY, newZ);
        }

        public GridPos3D Normalized()
        {
            int maxAxis = 0;
            if (Mathf.Abs(x) >= Mathf.Abs(y) && Mathf.Abs(x) >= Mathf.Abs(z)) maxAxis = 0;
            if (Mathf.Abs(y) >= Mathf.Abs(x) && Mathf.Abs(y) >= Mathf.Abs(z)) maxAxis = 1;
            if (Mathf.Abs(z) >= Mathf.Abs(x) && Mathf.Abs(z) >= Mathf.Abs(y)) maxAxis = 2;
            switch (maxAxis)
            {
                case 0: return x > 0 ? Right : (x < 0 ? Left : Zero);
                case 1: return y > 0 ? Up : (y < 0 ? Down : Zero);
                case 2: return z > 0 ? Forward : (z < 0 ? Back : Zero);
            }

            return Zero;
        }

        public float magnitude => Mathf.Sqrt(x * x + y * y + z * z);
        public Vector3 normalized => ((Vector3) this).normalized;

        public static GridPos3D operator -(GridPos3D a)
        {
            return new GridPos3D(-a.x, -a.y, -a.z);
        }

        public static GridPos3D operator -(GridPos3D a, GridPos3D b)
        {
            return new GridPos3D(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static GridPos3D operator +(GridPos3D a, GridPos3D b)
        {
            return new GridPos3D(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static GridPos3D operator *(GridPos3D a, int b)
        {
            return new GridPos3D(a.x * b, a.y * b, a.z * b);
        }

        public static GridPos3D operator *(int b, GridPos3D a)
        {
            return new GridPos3D(a.x * b, a.y * b, a.z * b);
        }

        public static bool operator ==(GridPos3D a, GridPos3D b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z;
        }

        public static bool operator !=(GridPos3D a, GridPos3D b)
        {
            return a.x != b.x || a.y != b.y || a.z != b.z;
        }

        public override string ToString()
        {
            return $"({x},{y},{z})";
        }

        public override bool Equals(object obj)
        {
            if (obj != null)
            {
                GridPos3D target = (GridPos3D) obj;
                return x == target.x && y == target.y && z == target.z;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return (x.GetHashCode() + y.GetHashCode() + z.GetHashCode()).GetHashCode();
        }
    }
}
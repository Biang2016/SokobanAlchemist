using System;

namespace BiangLibrary.GameDataFormat.Grid
{
    [Serializable]
    public struct Grid3DBounds
    {
        public GridPos3D position;
        public GridPos3D size;

        public GridPos3D center => new GridPos3D(position.x + size.x / 2, position.y + size.y / 2, position.z + size.z / 2);

        public int x_min => position.x;
        public int x_max => position.x + size.x - 1;
        public int y_min => position.y;
        public int y_max => position.y + size.y - 1;
        public int z_min => position.z;
        public int z_max => position.z + size.z - 1;

        public Grid3DBounds(int x, int y, int z, int width, int height, int depth)
        {
            position.x = x;
            position.y = y;
            position.z = z;
            size.x = width;
            size.y = height;
            size.z = depth;
        }

        public bool Contains(GridPos3D gp)
        {
            if (gp.x > x_max || gp.x < x_min || gp.y > y_max || gp.y < y_min || gp.z > z_max || gp.z < z_min) return false;
            return true;
        }
    }
}
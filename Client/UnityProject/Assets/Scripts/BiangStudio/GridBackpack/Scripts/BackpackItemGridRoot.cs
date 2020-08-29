using System.Collections.Generic;
using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ShapedInventory;
using UnityEngine;

namespace BiangStudio.GridBackpack
{
    public class BackpackItemGridRoot : MonoBehaviour
    {
        [HideInInspector]
        public Backpack Backpack;

        [SerializeField]
        private Transform BackpackItemGridContainer;

        private List<BackpackItemGrid> BackpackItemGrids = new List<BackpackItemGrid>();

        internal void Initialize(Backpack backpack, InventoryItem item)
        {
            Backpack = backpack;
            Clear();

            item.OccupiedGridPositions_Matrix.GetConnectionMatrix(out bool[,] connectionMatrix, out GridPos offset);
            foreach (GridPos gp in item.OccupiedGridPositions_Matrix)
            {
                gp.GetConnection(connectionMatrix, offset, out GridPosR.OrientationFlag adjacentConnection, out GridPosR.OrientationFlag diagonalConnection);
                GridPos localGP = gp - item.BoundingRect.position;
                BackpackItemGrid grid = Backpack.CreateBackpackItemGrid(BackpackItemGridContainer);
                grid.Initialize(localGP, new GridRect(localGP.x, -localGP.z, Backpack.GridSize, Backpack.GridSize), adjacentConnection, diagonalConnection);
                BackpackItemGrids.Add(grid);
            }
        }

        internal BackpackItemGrid FindGrid(Collider collider)
        {
            foreach (BackpackItemGrid grid in BackpackItemGrids)
            {
                if (grid.BoxCollider == collider)
                {
                    return grid;
                }
            }

            return null;
        }

        public void Clear()
        {
            foreach (BackpackItemGrid b in BackpackItemGrids)
            {
                b.PoolRecycle();
            }

            BackpackItemGrids.Clear();
        }

        public void SetGridColor(Color color)
        {
            foreach (BackpackItemGrid grid in BackpackItemGrids)
            {
                grid.SetGridColor(color);
            }
        }
    }
}
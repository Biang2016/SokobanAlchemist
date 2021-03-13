using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using UnityEngine;

namespace BiangLibrary.AdvancedInventory.UIInventory
{
    public class UIInventoryItemGridRoot : MonoBehaviour
    {
        [HideInInspector]
        public UIInventory UIInventory;

        [SerializeField]
        private Transform UIInventoryItemGridContainer;

        private List<UIInventoryItemGrid> UIInventoryItemGrids = new List<UIInventoryItemGrid>();

        internal void Initialize(UIInventory uiInventory, InventoryItem item)
        {
            UIInventory = uiInventory;
            Clear();

            item.OccupiedGridPositions_Matrix.GetConnectionMatrix(out bool[,] connectionMatrix, out GridPos offset);
            foreach (GridPos gp in item.OccupiedGridPositions_Matrix)
            {
                gp.GetConnection(connectionMatrix, offset, out GridPosR.OrientationFlag adjacentConnection, out GridPosR.OrientationFlag diagonalConnection);
                GridPos localGP = gp - item.BoundingRect.position;
                UIInventoryItemGrid grid = UIInventory.CreateUIInventoryItemGrid(UIInventoryItemGridContainer);
                grid.Initialize(localGP, new GridRect(localGP.x, -localGP.z, UIInventory.GridSize, UIInventory.GridSize), adjacentConnection, diagonalConnection);
                UIInventoryItemGrids.Add(grid);
            }
        }

        internal UIInventoryItemGrid FindGrid(Collider collider)
        {
            foreach (UIInventoryItemGrid grid in UIInventoryItemGrids)
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
            foreach (UIInventoryItemGrid b in UIInventoryItemGrids)
            {
                Destroy(b.gameObject);
            }

            UIInventoryItemGrids.Clear();
        }

        public void SetGridColor(Color color)
        {
            foreach (UIInventoryItemGrid grid in UIInventoryItemGrids)
            {
                grid.SetGridColor(color);
            }
        }
    }
}
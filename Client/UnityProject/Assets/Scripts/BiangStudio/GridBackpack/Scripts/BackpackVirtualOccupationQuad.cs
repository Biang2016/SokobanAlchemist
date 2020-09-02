using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ObjectPool;
using BiangStudio.ShapedInventory;
using UnityEngine;
using UnityEngine.UI;

namespace BiangStudio.GridBackpack
{
    public class BackpackVirtualOccupationQuad : PoolObject
    {
        public override void OnRecycled()
        {
            Inventory = null;
            base.OnRecycled();
        }

        void Awake()
        {
            RectTransform = (RectTransform) transform;
        }

        [SerializeField] private Image Image;
        [SerializeField] private Color AvailableColor;
        [SerializeField] private Color ForbiddenColor;

        private RectTransform RectTransform;
        private Inventory Inventory;

        public void Init(int gridSize, GridPos gp_matrix, Inventory inventory)
        {
            Inventory = inventory;
            GridPos gp_world = Inventory.CoordinateTransformationHandler_FromMatrixIndexToPos(gp_matrix);
            RectTransform.sizeDelta = gridSize * Vector2.one;
            RectTransform.anchoredPosition = new Vector2(gp_world.x * gridSize, gp_world.z * gridSize);
            Image.color = inventory.InventoryGridMatrix[gp_matrix.x, gp_matrix.z].Available ? AvailableColor : ForbiddenColor;
        }
    }
}
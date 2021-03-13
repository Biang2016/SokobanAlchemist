using BiangLibrary.DragHover;
using BiangLibrary.GameDataFormat.Grid;
using UnityEngine;

namespace BiangLibrary.AdvancedInventory.UIInventory
{
    public class UIInventoryDragAreaIndicator : DragAreaIndicator
    {
        public UIInventory UIInventory;
        private DragProcessor DragProcessor;

        public void Init(UIInventory uiInventory)
        {
            UIInventory = uiInventory;
            DragProcessor = DragManager.Instance.GetDragProcessor<UIInventoryItem>();
        }

        void Update()
        {
            Vector2 size = ((RectTransform) transform).rect.size;
            BoxCollider.size = new Vector3(size.x, size.y, 0.1f);
        }

        RaycastHit[] cachedRaycastHits = new RaycastHit[5];

        public bool GetMousePosOnThisArea(out Vector3 pos_world, out Vector3 pos_local, out Vector3 pos_matrix, out GridPos gp_matrix)
        {
            pos_world = Vector3.zero;
            pos_local = Vector3.zero;
            pos_matrix = Vector3.zero;
            gp_matrix = GridPos.Zero;
            Ray ray = DragProcessor.Camera.ScreenPointToRay(DragProcessor.CurrentMousePosition_Screen);
            int hitCount = Physics.RaycastNonAlloc(ray, cachedRaycastHits, 1000f, 1 << BoxCollider.gameObject.layer);
            if (hitCount > 0)
            {
                for (int i = 0; i < hitCount; i++)
                {
                    RaycastHit hit = cachedRaycastHits[i];
                    if (hit.collider != null && hit.collider == BoxCollider)
                    {
                        pos_world = hit.point;
                        Vector3 pos_local_absolute = UIInventory.UIInventoryPanel.ItemContainer.transform.InverseTransformPoint(pos_world);
                        Vector2 containerSize = ((RectTransform) UIInventory.UIInventoryPanel.ItemContainer).rect.size;
                        pos_local = new Vector3(pos_local_absolute.x + containerSize.x / 2f - UIInventory.GridSize / 2f, pos_local_absolute.y - containerSize.y / 2f + UIInventory.GridSize / 2f);
                        pos_matrix = new Vector3(pos_local_absolute.x + containerSize.x / 2f, -pos_local_absolute.y + containerSize.y / 2f);
                        Vector3 pos_matrix_round = new Vector3(pos_matrix.x - UIInventory.GridSize / 2f, pos_matrix.y - UIInventory.GridSize / 2f);
                        gp_matrix = GridPos.GetGridPosByPointXY(pos_matrix_round, UIInventory.GridSize);
                        return true;
                    }
                }
            }

            pos_world = DragProcessor.Camera.ScreenToWorldPoint(new Vector3(DragProcessor.CurrentMousePosition_Screen.x, DragProcessor.CurrentMousePosition_Screen.y, UIInventory.CanvasDistance));
            return false;
        }
    }
}
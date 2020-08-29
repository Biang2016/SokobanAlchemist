using BiangStudio.DragHover;
using BiangStudio.GameDataFormat.Grid;
using UnityEngine;

namespace BiangStudio.GridBackpack
{
    public class BackpackDragArea : DragAreaIndicator
    {
        private Backpack Backpack;
        private DragProcessor DragProcessor;

        [SerializeField]
        private BoxCollider BoxCollider;

        public void Init(Backpack backpack)
        {
            Backpack = backpack;
            DragArea = new DragArea(backpack.DragArea.DragAreaName);
            DragProcessor = DragManager.Instance.GetDragProcessor<BackpackItem>();
        }

        void Update()
        {
            Vector2 size = ((RectTransform) transform).rect.size;
            BoxCollider.size = new Vector3(size.x, size.y, 0.1f);
        }

        public void SetColliderSize(Vector2 size)
        {
            BoxCollider.size = new Vector3(size.x, size.y, 0.1f);
        }

        public bool GetMousePosOnThisArea(out Vector3 pos_world, out Vector3 pos_local, out Vector3 pos_matrix, out GridPos gp_matrix)
        {
            pos_world = Vector3.zero;
            pos_local = Vector3.zero;
            pos_matrix = Vector3.zero;
            gp_matrix = GridPos.Zero;
            Ray ray = DragProcessor.Camera.ScreenPointToRay(DragProcessor.CurrentMousePosition_Screen);
            Physics.Raycast(ray, out RaycastHit hit, 1000f, 1 << BoxCollider.gameObject.layer);
            if (hit.collider)
            {
                if (hit.collider == BoxCollider)
                {
                    pos_world = hit.point;
                    Vector3 pos_local_absolute = Backpack.BackpackPanel.ItemContainer.transform.InverseTransformPoint(pos_world);
                    Vector2 containerSize = ((RectTransform) Backpack.BackpackPanel.ItemContainer).rect.size;
                    pos_local = new Vector3(pos_local_absolute.x + containerSize.x / 2f - Backpack.GridSize / 2f, pos_local_absolute.y - containerSize.y / 2f + Backpack.GridSize / 2f);
                    pos_matrix = new Vector3(pos_local_absolute.x + containerSize.x / 2f, -pos_local_absolute.y + containerSize.y / 2f);
                    Vector3 pos_matrix_round = new Vector3(pos_matrix.x - Backpack.GridSize / 2f, pos_matrix.y - Backpack.GridSize / 2f);
                    gp_matrix = GridPos.GetGridPosByPointXY(pos_matrix_round, Backpack.GridSize);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }
    }
}
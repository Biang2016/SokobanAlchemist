using System.Collections.Generic;
using BiangStudio.CloneVariant;
using BiangStudio.DragHover;
using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ObjectPool;
using BiangStudio.ShapedInventory;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BiangStudio.GridBackpack
{
    public class BackpackItem : PoolObject, IDraggable, IMouseHoverComponent
    {
        public override void OnRecycled()
        {
            BackpackItemGridRoot.Clear();
            base.OnRecycled();
            Backpack = null;
            InventoryItem = null;
        }

        [HideInInspector]
        public Backpack Backpack;

        [HideInInspector]
        public InventoryItem InventoryItem;

        private Draggable Draggable;
        public RectTransform PanelRectTransform => (RectTransform) Backpack.BackpackPanel.ItemContainer;

        internal UnityAction OnHoverAction;
        internal UnityAction OnHoverEndAction;

        [SerializeField]
        private Image Image;

        [SerializeField]
        private BackpackItemGridRoot BackpackItemGridRoot;

        public RectTransform RectTransform => (RectTransform) transform;

        private void Awake()
        {
            Draggable = GetComponent<Draggable>();
        }

        public void Initialize(Backpack backpack, InventoryItem inventoryItem, UnityAction onHoverAction, UnityAction onHoverEndAction)
        {
            Backpack = backpack;
            SetInventoryItem(inventoryItem);
            OnHoverAction = onHoverAction;
            OnHoverEndAction = onHoverEndAction;

            RefreshView();
        }

        public void SetInventoryItem(InventoryItem inventoryItem)
        {
            InventoryItem = inventoryItem;
            InventoryItem.OnSetGridPosHandler = SetVirtualGridPos;
            InventoryItem.OnRefreshViewHandler = RefreshView;
            Image.sprite = BackpackManager.Instance.GetBackpackItemSprite(inventoryItem.ItemContentInfo.ItemSpriteKey);
        }

        private void Rotate()
        {
            InventoryItem.GridPos_Matrix.orientation = GridPosR.RotateOrientationClockwise90(InventoryItem.GridPos_Matrix.orientation);
            InventoryItem.SetGridPosition(InventoryItem.GridPos_Matrix);
            dragStartLocalPos += new Vector2(InventoryItem.BoundingRect.x_min * Backpack.GridSize, -InventoryItem.BoundingRect.z_min * Backpack.GridSize) - RectTransform.anchoredPosition;
            SetVirtualGridPos(InventoryItem.GridPos_World);
            RefreshView();
        }

        private void RefreshView()
        {
            Vector2 size = new Vector2(InventoryItem.BoundingRect.size.x * Backpack.GridSize, InventoryItem.BoundingRect.size.z * Backpack.GridSize);
            Vector2 sizeRev = new Vector2(size.y, size.x);

            int UI_Pos_X = InventoryItem.BoundingRect.x_min * Backpack.GridSize;
            int UI_Pos_Z = -InventoryItem.BoundingRect.z_min * Backpack.GridSize;

            bool isRotated = InventoryItem.GridPos_Matrix.orientation == GridPosR.Orientation.Right || InventoryItem.GridPos_Matrix.orientation == GridPosR.Orientation.Left;
            Image.rectTransform.sizeDelta = (isRotated ? sizeRev : size) - Vector2.one * 8;
            Image.rectTransform.rotation = Quaternion.Euler(0, 0, 90f * (int) InventoryItem.GridPos_Matrix.orientation);

            RectTransform.sizeDelta = size;

            RectTransform.anchoredPosition = new Vector2(UI_Pos_X, UI_Pos_Z);
            BackpackItemGridRoot.Initialize(Backpack, InventoryItem);
            BackpackItemGridRoot.SetGridColor(InventoryItem.ItemContentInfo.ItemColor);
        }

        #region IDraggable

        private Vector2 dragStartLocalPos;
        private GridPosR dragStartGridPos_Matrix;
        private List<GridPos> dragStartOccupiedPositions_Matrix = new List<GridPos>();

        public void Draggable_OnMouseDown(DragArea dragArea, Collider collider)
        {
            dragStartLocalPos = RectTransform.anchoredPosition;
            dragStartOccupiedPositions_Matrix = InventoryItem.OccupiedGridPositions_Matrix.Clone();
            dragStartGridPos_Matrix = InventoryItem.GridPos_Matrix;
            InventoryItem.Inventory.PickUpItem(InventoryItem);
        }

        private void SetVirtualGridPos(GridPosR gridPos_World)
        {
            Backpack.BackpackPanel.BackpackItemVirtualOccupationRoot.Clear();
            foreach (GridPos gp_matrix in InventoryItem.OccupiedGridPositions_Matrix)
            {
                BackpackVirtualOccupationQuad quad = Backpack.CreateBackpackItemVirtualOccupationQuad(Backpack.BackpackPanel.BackpackItemVirtualOccupationRoot.transform);
                quad.Init(InventoryItem.Inventory.GridSize, gp_matrix, InventoryItem.Inventory);
                Backpack.BackpackPanel.BackpackItemVirtualOccupationRoot.backpackVirtualOccupationQuads.Add(quad);
            }
        }

        public void Draggable_OnMousePressed(DragArea dragArea, Vector3 diffFromStart, Vector3 deltaFromLastFrame)
        {
            if (dragArea.Equals(Backpack.DragArea))
            {
                if (Backpack.RotateItemKeyDownHandler != null && Backpack.RotateItemKeyDownHandler.Invoke())
                {
                    Rotate();
                }

                if (diffFromStart.magnitude <= Draggable_DragMinDistance)
                {
                    // stay
                }
                else
                {
                    Vector2 diffLocal = RectTransform.parent.InverseTransformVector(diffFromStart);
                    Vector2 currentLocalPos = dragStartLocalPos + diffLocal;
                    GridPosR diff_world = GridPos.GetGridPosByPointXY(diffLocal, Backpack.GridSize);
                    diff_world.orientation = InventoryItem.GridPos_Matrix.orientation;
                    GridPosR diff_matrix = Backpack.CoordinateTransformationHandler_FromPosToMatrixIndex(diff_world);
                    GridPosR gp_matrix = dragStartGridPos_Matrix + diff_matrix;
                    gp_matrix.orientation = InventoryItem.GridPos_Matrix.orientation;
                    InventoryItem.SetGridPosition(gp_matrix);
                    RectTransform.anchoredPosition = currentLocalPos;
                }
            }
            else
            {
                if (Backpack.DragItemOutBackpackCallback != null)
                {
                    Backpack.BackpackPanel.BackpackItemVirtualOccupationRoot.Clear();
                    if (Backpack.DragItemOutBackpackCallback.Invoke(this))
                    {
                        InventoryItem.GridPos_Matrix = dragStartGridPos_Matrix;
                        Backpack.RemoveItem(InventoryItem, true);
                        PoolRecycle();
                    }
                }
            }
        }

        public void Draggable_OnMouseUp(DragArea dragArea, Vector3 diffFromStart, Vector3 deltaFromLastFrame)
        {
            Backpack.BackpackPanel.BackpackItemVirtualOccupationRoot.Clear();
            if (dragArea.Equals(Backpack.DragArea))
            {
                if (Backpack.CheckSpaceAvailable(dragStartOccupiedPositions_Matrix))
                {
                    Backpack.ResetGrids(dragStartOccupiedPositions_Matrix);
                }
                else
                {
                    // indicates that this is dragged in from outside, and conflict at the first position with another item.
                }

                if (!Backpack.CheckSpaceAvailable(InventoryItem.OccupiedGridPositions_Matrix))
                {
                    InventoryItem.GridPos_Matrix = dragStartGridPos_Matrix;
                }

                if (!Backpack.CheckSpaceAvailable(InventoryItem.OccupiedGridPositions_Matrix))
                {
                    if (Backpack.FindSpaceToPutItem(InventoryItem))
                    {
                    }
                    else
                    {
                        Debug.LogError("No space in bag but suc drag in");
                    }
                }

                Backpack.PutDownItem(InventoryItem);
                RefreshView();
            }
        }

        public void Draggable_SetStates(ref bool canDrag, ref DragArea dragFrom)
        {
            canDrag = true;
            dragFrom = Backpack.DragArea;
        }

        public float Draggable_DragMinDistance => 0f;

        public float Draggable_DragMaxDistance => 99f;

        #endregion

        #region IMouseHoverComponent

        public void MouseHoverComponent_OnHoverBegin(Vector3 mousePosition)
        {
            OnHoverAction?.Invoke();
        }

        public void MouseHoverComponent_OnHoverEnd()
        {
            OnHoverEndAction?.Invoke();
        }

        public void MouseHoverComponent_OnFocusBegin(Vector3 mousePosition)
        {
        }

        public void MouseHoverComponent_OnFocusEnd()
        {
        }

        public void MouseHoverComponent_OnMousePressEnterImmediately(Vector3 mousePosition)
        {
        }

        public void MouseHoverComponent_OnMousePressLeaveImmediately()
        {
        }

        #endregion
    }
}
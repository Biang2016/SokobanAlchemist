using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using BiangLibrary.DragHover;
using BiangLibrary.GameDataFormat.Grid;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BiangLibrary.AdvancedInventory.UIInventory
{
    public class UIInventoryItem : MonoBehaviour, IDraggable, IMouseHoverComponent
    {
        /// <summary>
        /// If you use object pool, please invoke this function before reuse.
        /// </summary>
        public void OnRecycled()
        {
            UIInventoryItemGridRoot.Clear();
            UIInventory = null;
            InventoryItem = null;
        }

        [HideInInspector]
        public UIInventory UIInventory;

        [HideInInspector]
        public InventoryItem InventoryItem;

        private Draggable Draggable;
        public RectTransform PanelRectTransform => (RectTransform) UIInventory.UIInventoryPanel.ItemContainer;

        internal UnityAction OnHoverAction;
        internal UnityAction OnHoverEndAction;

        [SerializeField]
        private Image Image;

        [SerializeField]
        private UIInventoryItemGridRoot UIInventoryItemGridRoot;

        public RectTransform RectTransform => (RectTransform) transform;

        private void Awake()
        {
            Draggable = GetComponent<Draggable>();
        }

        public void Initialize(UIInventory uiInventory, InventoryItem inventoryItem, UnityAction onHoverAction, UnityAction onHoverEndAction)
        {
            UIInventory = uiInventory;
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
            Image.sprite = inventoryItem.ItemContentInfo.ItemSprite;
        }

        private void Rotate()
        {
            if (!InventoryItem.ItemContentInfo.Rotatable) return;
            GridPosR.Orientation newOri = GridPosR.RotateOrientationClockwise90(InventoryItem.GridPos_Matrix.orientation);
            InventoryItem.GridPos_Matrix = new GridPosR(InventoryItem.GridPos_Matrix.x, InventoryItem.GridPos_Matrix.z, newOri);
            SetVirtualGridPos(InventoryItem.GridPos_World);
            dragStartLocalPos += new Vector2(InventoryItem.BoundingRect.x_min * UIInventory.GridSize, -InventoryItem.BoundingRect.z_min * UIInventory.GridSize) - RectTransform.anchoredPosition;
            RefreshView();
        }

        private void RefreshView()
        {
            Vector2 size = new Vector2(InventoryItem.BoundingRect.size.x * UIInventory.GridSize, InventoryItem.BoundingRect.size.z * UIInventory.GridSize);
            Vector2 sizeRev = new Vector2(size.y, size.x);

            int UI_Pos_X = InventoryItem.BoundingRect.x_min * UIInventory.GridSize;
            int UI_Pos_Z = -InventoryItem.BoundingRect.z_min * UIInventory.GridSize;

            bool isRotated = InventoryItem.GridPos_Matrix.orientation == GridPosR.Orientation.Right || InventoryItem.GridPos_Matrix.orientation == GridPosR.Orientation.Left;
            Image.rectTransform.sizeDelta = (isRotated ? sizeRev : size) - Vector2.one * 8;
            Image.rectTransform.rotation = Quaternion.Euler(0, 0, 90f * (int) InventoryItem.GridPos_Matrix.orientation);

            RectTransform.sizeDelta = size;

            RectTransform.anchoredPosition = new Vector2(UI_Pos_X, UI_Pos_Z);
            UIInventoryItemGridRoot.Initialize(UIInventory, InventoryItem);
            UIInventoryItemGridRoot.SetGridColor(InventoryItem.ItemContentInfo.ItemColor);
        }

        private void SetVirtualGridPos(GridPosR gridPos_World)
        {
            UIInventory.UIInventoryPanel.UIInventoryVirtualOccupationQuadRoot.Clear();
            foreach (GridPos gp_matrix in InventoryItem.OccupiedGridPositions_Matrix)
            {
                if (!UIInventory.ContainsGP(gp_matrix))
                {
                    continue;
                }

                UIInventoryVirtualOccupationQuad quad = UIInventory.CreateUIInventoryItemVirtualOccupationQuad(UIInventory.UIInventoryPanel.UIInventoryVirtualOccupationQuadRoot.transform);
                quad.Init(InventoryItem.Inventory.GridSize, gp_matrix, InventoryItem.Inventory);
                UIInventory.UIInventoryPanel.UIInventoryVirtualOccupationQuadRoot.uiInventoryVirtualOccupationQuads.Add(quad);
            }
        }

        #region IDraggable

        private Vector2 dragStartLocalPos;
        private GridPosR dragStartGridPos_Matrix;
        private List<GridPos> dragStartOccupiedPositions_Matrix = new List<GridPos>();

        public void Draggable_OnMouseDown(DragAreaIndicator dragAreaIndicator, Collider collider)
        {
            dragStartLocalPos = RectTransform.anchoredPosition;
            dragStartOccupiedPositions_Matrix = InventoryItem.OccupiedGridPositions_Matrix.Clone();
            dragStartGridPos_Matrix = InventoryItem.GridPos_Matrix;
            InventoryItem.Inventory.PickUpItem(InventoryItem);
        }

        public void Draggable_OnMousePressed(DragAreaIndicator dragAreaIndicator, Vector3 diffFromStart, Vector3 deltaFromLastFrame)
        {
            void ResumePausedDrag()
            {
                InventoryItem.GridPos_Matrix = dragStartGridPos_Matrix;
                UIInventory.RemoveItem(InventoryItem, true);
                Destroy(gameObject);
                DragManager.Instance.ResumePausedDrag();
            }

            if (dragAreaIndicator == UIInventory.DragAreaIndicator)
            {
                if (UIInventory.RotateItemKeyDownHandler != null && UIInventory.RotateItemKeyDownHandler.Invoke())
                {
                    Rotate();
                }

                if (diffFromStart.magnitude > Draggable_DragMinDistance)
                {
                    Vector2 diffLocal = RectTransform.parent.InverseTransformVector(diffFromStart);
                    Vector2 currentLocalPos = dragStartLocalPos + diffLocal;
                    GridPosR diff_world = GridPos.GetGridPosByPointXY(diffLocal, UIInventory.GridSize);
                    diff_world.orientation = InventoryItem.GridPos_Matrix.orientation;
                    GridPosR diff_matrix = UIInventory.CoordinateTransformationHandler_FromPosToMatrixIndex(diff_world);
                    GridPosR gp_matrix = dragStartGridPos_Matrix + diff_matrix;
                    gp_matrix.orientation = InventoryItem.GridPos_Matrix.orientation;
                    InventoryItem.GridPos_Matrix = gp_matrix;
                    SetVirtualGridPos(InventoryItem.GridPos_World);
                    RectTransform.anchoredPosition = currentLocalPos;
                }
            }
            else
            {
                Vector2 diffLocal = RectTransform.parent.InverseTransformVector(diffFromStart);
                Vector2 currentLocalPos = dragStartLocalPos + diffLocal;
                RectTransform.anchoredPosition = currentLocalPos;
                UIInventory.UIInventoryPanel.UIInventoryVirtualOccupationQuadRoot.Clear();
                if (dragAreaIndicator != null) // drag to other DragAreas
                {
                    if (DragManager.Instance.PausedDrag != null)
                    {
                        ResumePausedDrag();
                    }
                    else
                    {
                        // only when mouse move to available grid then generate previewItem
                        if (dragAreaIndicator is UIInventoryDragAreaIndicator uiDAI)
                        {
                            uiDAI.UIInventory.UIInventoryPanel.UIInventoryDragAreaIndicator.GetMousePosOnThisArea(out Vector3 pos_world, out Vector3 pos_local, out Vector3 pos_matrix, out GridPos gp_matrix);
                            GridPosR gpr = gp_matrix;
                            gpr.orientation = InventoryItem.GridPos_Matrix.orientation;
                            InventoryItem previewItem = new InventoryItem(InventoryItem.Clone().ItemContentInfo, uiDAI.UIInventory, gpr);
                            uiDAI.UIInventory.AddPreviewItem(previewItem);
                            UIInventoryItem uiInventoryItem = uiDAI.UIInventory.UIInventoryPanel.GetUIInventoryItem(previewItem.GUID);
                            DragManager.Instance.PauseDrag();
                            DragManager.Instance.CurrentDrag = uiInventoryItem.Draggable;
                            uiInventoryItem.Draggable.SetOnDrag(true, uiDAI.UIInventory.UIInventoryPanel.UIInventoryDragAreaIndicator.BoxCollider, UIInventory.DragProcessor);
                        }
                    }
                }
                else // drag to non-DragArea
                {
                    if (DragManager.Instance.PausedDrag != null)
                    {
                        ResumePausedDrag();
                    }
                }
            }
        }

        public void Draggable_OnMouseUp(DragAreaIndicator dragAreaIndicator, Vector3 diffFromStart, Vector3 deltaFromLastFrame)
        {
            void ReturnItemToOriginalPlace()
            {
                UIInventory.ClearTempUnavailableGridState();
                InventoryItem.GridPos_Matrix = dragStartGridPos_Matrix;
                UIInventory.PutDownItem(InventoryItem);
                RefreshView();
            }

            if (dragAreaIndicator == UIInventory.DragAreaIndicator)
            {
                bool validPutDown = true;
                UIInventory.UIInventoryPanel.UIInventoryVirtualOccupationQuadRoot.Clear();
                if (UIInventory.CheckSpaceAvailable(dragStartOccupiedPositions_Matrix))
                {
                    UIInventory.ResetGrids(dragStartOccupiedPositions_Matrix);
                }
                else
                {
                    // indicates that this is dragged in from outside, and conflict at the first position with another item.
                }

                // if the dropping grids are not available, then try the original drag position
                if (!UIInventory.CheckSpaceAvailable(InventoryItem.OccupiedGridPositions_Matrix))
                {
                    InventoryItem.GridPos_Matrix = dragStartGridPos_Matrix;
                }

                // if the original drag position is not available, then search a position for this item 
                if (!UIInventory.CheckSpaceAvailable(InventoryItem.OccupiedGridPositions_Matrix))
                {
                    if (!UIInventory.FindSpaceToPutItem(InventoryItem, false)) // if this function returns true, it will also change the item's position/rotation
                    {
                        // but if failed, interrupt this move
                        UIInventory.RemoveItem(InventoryItem, true);
                        Destroy(gameObject);
                        DragManager.Instance.ResumePausedDrag();
                        DragManager.Instance.CurrentDrag = null;
                        validPutDown = false;
                    }
                }

                if (validPutDown)
                {
                    UIInventory.PutDownItem(InventoryItem);
                    RefreshView();
                    DragManager.Instance.SucceedPausedDrag();
                }
            }
            else if (dragAreaIndicator != null) // drag to other DragAreas
            {
                // this will always occur when other dragArea doesn't have enough space for this item
                ReturnItemToOriginalPlace();
            }
            else // drag to non-DragArea
            {
                if (DragManager.Instance.PausedDrag == null)
                {
                    if (UIInventory.DragOutDrop)
                    {
                        InventoryItem.GridPos_Matrix = dragStartGridPos_Matrix;
                        UIInventory.DragItemOutUIInventoryCallback = () =>
                        {
                            UIInventory.RemoveItem(InventoryItem, true);
                            Destroy(gameObject);
                        };

                        UIInventory.DragItemOutUIInventoryCallback?.Invoke();
                        UIInventory.DragItemOutUIInventoryCallback = null;

                        // todo confirm panel
                    }
                    else
                    {
                        ReturnItemToOriginalPlace();
                    }
                }
            }
        }

        public void Draggable_OnPaused()
        {
            gameObject.SetActive(false);
        }

        public void Draggable_OnResume()
        {
            gameObject.SetActive(true);
        }

        public void Draggable_OnSucceedWhenPaused()
        {
            UIInventory.ClearTempUnavailableGridState();
            InventoryItem.GridPos_Matrix = dragStartGridPos_Matrix;
            UIInventory.RemoveItem(InventoryItem, true);
            Destroy(gameObject);
        }

        public void Draggable_SetStates(ref bool canDrag, ref DragAreaIndicator dragFromDragAreaIndicator)
        {
            canDrag = true;
            dragFromDragAreaIndicator = UIInventory.DragAreaIndicator;
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
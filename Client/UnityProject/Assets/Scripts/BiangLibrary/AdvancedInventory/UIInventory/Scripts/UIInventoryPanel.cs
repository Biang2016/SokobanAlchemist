using System;
using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BiangLibrary.AdvancedInventory.UIInventory
{
    public class UIInventoryPanel : MonoBehaviour
    {
        [HideInInspector]
        public UIInventory UIInventory;

        public UIInventoryItemInfoPanel UIInventoryItemInfoPanel;

        [SerializeField]
        private Text UIInventoryTitle;

        [SerializeField]
        private Transform UIInventoryTitleContainer;

        [SerializeField]
        private UIInventoryDragMoveHandle UIInventoryDragMoveHandle;

        [SerializeField]
        private GridLayoutGroup ItemContainerGridLayout;

        public RectTransform PanelTransform;
        public Transform Container;

        [SerializeField]
        private Transform UIInventoryGridContainer;

        public Transform ItemContainer;

        public UIInventoryVirtualOccupationQuadRoot UIInventoryVirtualOccupationQuadRoot;

        public UIInventoryDragAreaIndicator UIInventoryDragAreaIndicator;

        private UIInventoryGrid[,] uiInventoryGridMatrix; // column, row
        private SortedDictionary<uint, UIInventoryItem> uiInventoryItems = new SortedDictionary<uint, UIInventoryItem>();

        public UnityAction<UIInventoryItem> OnHoverUIInventoryItem;
        public UnityAction<UIInventoryItem> OnHoverEndUIInventoryItem;

        void Update()
        {
        }

        void ResetPanel()
        {
            foreach (KeyValuePair<uint, UIInventoryItem> kv in uiInventoryItems)
            {
                Destroy(kv.Value.gameObject);
            }

            uiInventoryItems.Clear();
        }

        public void Init(UIInventory uiInventory, UnityAction<UIInventoryItem> onHoverUIInventoryItem = null, UnityAction<UIInventoryItem> onHoverEndUIInventoryItem = null)
        {
            UIInventory = uiInventory;
            OnHoverUIInventoryItem = onHoverUIInventoryItem;
            OnHoverEndUIInventoryItem = onHoverEndUIInventoryItem;
            UIInventoryDragAreaIndicator.Init(uiInventory);
            UIInventory.UIInventoryPanel = this;

            UIInventoryTitle.text = uiInventory.InventoryName;
            UIInventoryDragMoveHandle.EnableScreenClamp = uiInventory.EnableScreenClamp;
            ((RectTransform) UIInventoryTitleContainer).sizeDelta = new Vector2(uiInventory.GridSize * UIInventory.Columns, ((RectTransform) UIInventoryTitleContainer).sizeDelta.y);
            ((RectTransform) Container).sizeDelta = new Vector2(uiInventory.GridSize * UIInventory.Columns, uiInventory.GridSize * UIInventory.Rows);

            ItemContainerGridLayout.constraintCount = uiInventory.Columns;
            ItemContainerGridLayout.cellSize = new Vector2(uiInventory.GridSize, uiInventory.GridSize);

            uiInventoryGridMatrix = new UIInventoryGrid[UIInventory.Columns, UIInventory.Rows];
            for (int row = 0; row < UIInventory.Rows; row++)
            {
                for (int col = 0; col < UIInventory.Columns; col++)
                {
                    UIInventoryGrid bg = UIInventory.CreateUIInventoryGrid(UIInventoryGridContainer);
                    bg.Init(UIInventory.InventoryGridMatrix[col, row], new GridPos(col, row));
                    uiInventoryGridMatrix[col, row] = bg;
                }
            }

            UIInventory.OnAddItemSucAction = OnAddItemSuc;
            UIInventory.OnRemoveItemSucAction = OnRemoveItemSuc;
            uiInventory.RefreshInventoryGrids();
        }

        public UIInventoryItem GetUIInventoryItem(uint guid)
        {
            uiInventoryItems.TryGetValue(guid, out UIInventoryItem uiInventoryItem);
            return uiInventoryItem;
        }

        private void OnAddItemSuc(InventoryItem ii)
        {
            UIInventoryItem uiInventoryItem = UIInventory.CreateUIInventoryItem(ItemContainer);
            uiInventoryItem.Initialize(UIInventory, ii, delegate { OnHoverUIInventoryItem?.Invoke(uiInventoryItem); }, delegate { OnHoverEndUIInventoryItem?.Invoke(uiInventoryItem); });
            uiInventoryItems.Add(ii.GUID, uiInventoryItem);
        }

        private void OnRemoveItemSuc(InventoryItem ii)
        {
            UIInventoryItem bi = uiInventoryItems[ii.GUID];
            Destroy(bi.gameObject);
            uiInventoryItems.Remove(ii.GUID);
        }
    }
}
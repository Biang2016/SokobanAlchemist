using System.Collections.Generic;
using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ShapedInventory;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BiangStudio.GridBackpack
{
    public class BackpackPanel : MonoBehaviour
    {
        [ReadOnly]
        public Backpack Backpack;

        [SerializeField]
        private Text BackpackTitle;

        [SerializeField]
        private GridLayoutGroup ItemContainerGridLayout;

        public RectTransform PanelTransform;
        public Transform Container;

        [SerializeField]
        private Transform GridContainer;

        public Transform ItemContainer;

        internal BackpackItemVirtualOccupationRoot BackpackItemVirtualOccupationRoot;

        public BackpackDragArea BackpackDragArea;

        private BackpackGrid[,] backpackGridMatrix; // column, row
        private SortedDictionary<uint, BackpackItem> backpackItems = new SortedDictionary<uint, BackpackItem>();

        public UnityAction<BackpackItem> OnHoverBackpackItem;
        public UnityAction<BackpackItem> OnHoverEndBackpackItem;

        void Awake()
        {
            BackpackItemVirtualOccupationRoot = GetComponentInChildren<BackpackItemVirtualOccupationRoot>();
        }

        void Update()
        {
        }

        void ResetPanel()
        {
            foreach (KeyValuePair<uint, BackpackItem> kv in backpackItems)
            {
                kv.Value.PoolRecycle();
            }

            backpackItems.Clear();
        }

        public void Init(Backpack backPack, UnityAction<BackpackItem> onHoverBackpackItem = null, UnityAction<BackpackItem> onHoverEndBackpackItem = null)
        {
            Backpack = backPack;
            OnHoverBackpackItem = onHoverBackpackItem;
            OnHoverEndBackpackItem = onHoverEndBackpackItem;
            BackpackDragArea.Init(backPack);
            Backpack.BackpackPanel = this;

            BackpackTitle.text = backPack.InventoryName;
            ((RectTransform) BackpackTitle.transform).sizeDelta = new Vector2(backPack.GridSize * Backpack.Columns, ((RectTransform) BackpackTitle.transform).sizeDelta.y);
            ((RectTransform) Container).sizeDelta = new Vector2(backPack.GridSize * Backpack.Columns, backPack.GridSize * Backpack.Rows);
            ItemContainerGridLayout.constraintCount = backPack.Columns;

            backpackGridMatrix = new BackpackGrid[Backpack.Columns, Backpack.Rows];
            for (int row = 0; row < Backpack.Rows; row++)
            {
                for (int col = 0; col < Backpack.Columns; col++)
                {
                    BackpackGrid bg = Backpack.CreateBackpackGrid(GridContainer);
                    bg.Init(Backpack.InventoryGridMatrix[col, row], new GridPos(col, row));
                    backpackGridMatrix[col, row] = bg;
                }
            }

            Backpack.OnAddItemSucAction = OnAddItemSuc;
            Backpack.OnRemoveItemSucAction = OnRemoveItemSuc;
            backPack.RefreshInventoryGrids();
        }

        public BackpackItem GetBackpackItem(uint guid)
        {
            backpackItems.TryGetValue(guid, out BackpackItem backpackItem);
            return backpackItem;
        }

        private void OnAddItemSuc(InventoryItem ii)
        {
            BackpackItem backpackItem = Backpack.CreateBackpackItem(ItemContainer);
            backpackItem.Initialize(Backpack, ii, delegate { OnHoverBackpackItem?.Invoke(backpackItem); }, delegate { OnHoverEndBackpackItem?.Invoke(backpackItem); });
            backpackItems.Add(ii.GUID, backpackItem);
        }

        private void OnRemoveItemSuc(InventoryItem ii)
        {
            BackpackItem bi = backpackItems[ii.GUID];
            bi.PoolRecycle();
            backpackItems.Remove(ii.GUID);
        }
    }
}
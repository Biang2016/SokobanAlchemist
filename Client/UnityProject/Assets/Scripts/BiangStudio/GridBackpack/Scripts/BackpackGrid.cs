using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ObjectPool;
using BiangStudio.ShapedInventory;
using UnityEngine;
using UnityEngine.UI;

namespace BiangStudio.GridBackpack
{
    /// <summary>
    /// The class control the display and the background of a grid in a backpack
    /// </summary>
    public class BackpackGrid : PoolObject
    {
        public override void OnRecycled()
        {
            Data = null;
            base.OnRecycled();
        }

        public InventoryGrid Data;

        [SerializeField]
        private Image Image;

        [SerializeField]
        private Text GridPosText;

        [SerializeField]
        private Color LockedColor;

        [SerializeField]
        private Color AvailableColor;

        [SerializeField]
        private Color UnavailableColor;

        [SerializeField]
        private Color TempUnavailableColor;

        [SerializeField]
        private Color PreviewColor;

        internal bool Available => Data.Available;

        internal bool Locked => Data.Locked;

        [SerializeField]
        private bool ShowGridPosDebugText;

        internal InventoryGrid.States State
        {
            get => Data.State;
            set => Data.State = value;
        }

        internal void Init(InventoryGrid data, GridPos gp)
        {
            Data = data;
            Data.SetStateHandler = OnSetState;
            if (ShowGridPosDebugText)
            {
                GridPosText.text = gp.ToString();
            }
            else
            {
                GridPosText.text = "";
            }
        }

        internal void OnSetState(InventoryGrid.States newValue)
        {
            switch (newValue)
            {
                case InventoryGrid.States.Locked:
                {
                    Image.color = LockedColor;
                    break;
                }
                case InventoryGrid.States.Unavailable:
                {
                    Image.color = UnavailableColor;
                    break;
                }
                case InventoryGrid.States.TempUnavailable:
                {
                    Image.color = TempUnavailableColor;
                    break;
                }
                case InventoryGrid.States.Available:
                {
                    Image.color = AvailableColor;
                    break;
                }
                case InventoryGrid.States.Preview:
                {
                    Image.color = PreviewColor;
                    break;
                }
            }
        }
    }
}
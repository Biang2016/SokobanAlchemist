using BiangLibrary.GameDataFormat.Grid;
using UnityEngine;
using UnityEngine.UI;

namespace BiangLibrary.AdvancedInventory.UIInventory
{
    /// <summary>
    /// The class control the display and the background of a grid in a uiInventory
    /// </summary>
    public class UIInventoryGrid : MonoBehaviour
    {
        /// <summary>
        /// If you use object pool, please invoke this function before reuse.
        /// </summary>
        public void OnRecycled()
        {
            Data = null;
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

        [SerializeField]
        private Image LockIcon;

        internal bool Available => Data.Available;

        internal bool Locked => Data.Locked;

        [SerializeField]
        private bool ShowGridPosDebugText;

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
            LockIcon.enabled = newValue == InventoryGrid.States.Locked;
            switch (newValue)
            {
                case InventoryGrid.States.Locked:
                {
                    Image.color = LockedColor;
                    GridPosText.color = LockedColor;
                    break;
                }
                case InventoryGrid.States.Unavailable:
                {
                    Image.color = UnavailableColor;
                    GridPosText.color = UnavailableColor;
                    break;
                }
                case InventoryGrid.States.TempUnavailable:
                {
                    Image.color = TempUnavailableColor;
                    GridPosText.color = TempUnavailableColor;
                    break;
                }
                case InventoryGrid.States.Available:
                {
                    Image.color = AvailableColor;
                    GridPosText.color = AvailableColor;
                    break;
                }
                case InventoryGrid.States.Preview:
                {
                    Image.color = PreviewColor;
                    GridPosText.color = PreviewColor;
                    break;
                }
            }
        }
    }
}
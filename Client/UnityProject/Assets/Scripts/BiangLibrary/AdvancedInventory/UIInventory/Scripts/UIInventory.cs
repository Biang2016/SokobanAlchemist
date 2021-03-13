using System;
using BiangLibrary.DragHover;
using BiangLibrary.GameDataFormat.Grid;
using UnityEngine;
using UnityEngine.Events;

namespace BiangLibrary.AdvancedInventory.UIInventory
{
    [Serializable]
    public class UIInventory : Inventory
    {
        public UIInventoryPanel UIInventoryPanel;
        public DragProcessor DragProcessor;

        private KeyDownDelegate ToggleUIInventoryKeyDownHandler;

        private InstantiatePrefabDelegate InstantiateUIInventoryGridHandler;
        private InstantiatePrefabDelegate InstantiateUIInventoryItemHandler;
        private InstantiatePrefabDelegate InstantiateUIInventoryItemGridHandler;
        private InstantiatePrefabDelegate InstantiateUIInventoryItemVirtualOccupationQuadHandler;

        /// <summary>
        /// This callback will be execute when the uiInventory is opened or closed
        /// </summary>
        public UnityAction<bool> ToggleUIInventoryCallback;

        /// <summary>
        /// This callback will be execute when the uiInventory debug mode is enable or disable
        /// </summary>
        public UnityAction<bool> ToggleDebugCallback;

        /// <summary>
        /// This callback will be execute when the uiInventory item is dragged and drop out of the uiInventory.
        /// </summary>
        public UnityAction DragItemOutUIInventoryCallback;

        public float CanvasDistance { get; private set; }

        private bool isOpen = false;

        private bool IsOpen
        {
            get { return isOpen; }

            set
            {
                if (isOpen != value)
                {
                    ToggleUIInventoryCallback?.Invoke(value);
                    isOpen = value;
                }
            }
        }

        private bool isDebug = false;

        private bool IsDebug
        {
            get { return isDebug; }

            set
            {
                if (isDebug != value)
                {
                    ToggleDebugCallback?.Invoke(value);
                    isDebug = value;
                }
            }
        }

        public bool EnableScreenClamp = false;

        /// <summary>
        /// Initialize the uiInventory manager.
        /// </summary>
        /// <param name="inventoryName">The inventory name</param>
        /// <param name="dragAreaIndicator"></param>
        /// <param name="dragProcessor"></param>
        /// <param name="canvasDistance">the distance of canvas</param>
        /// <param name="gridSize">the gridSize of inventory</param>
        /// <param name="rows">how many rows in total</param>
        /// <param name="columns">how many columns in total</param>
        /// <param name="x_Mirror">is X axis mirrored</param>
        /// <param name="z_Mirror">is Z axis mirrored</param>
        /// <param name="unlockedPartialGrids">is there any grid locked at the beginning</param>
        /// <param name="unlockedGridCount">how many grids are locked at the beginning</param>
        /// <param name="dragOutDrop">allows item to be dragged and dropped outside inventory</param>
        /// <param name="enableScreenClamp">enable inventory UI panel's screen-clamping</param>
        /// <param name="enableLog">enable inventory log</param>
        /// <param name="toggleUIInventoryKeyDownHandler">This handler should return a signal which toggles the uiInventory(e.g. return Input.GetKeyDown(KeyCode.B);)</param>
        /// <param name="rotateItemKeyDownHandler">This handler should return a signal which rotates the uiInventory item(e.g. return Input.GetKeyDown(KeyCode.R);)</param>
        /// <param name="instantiateUIInventoryGridHandler">This handler should instantiate a prefab with UIInventoryGrid component.</param>
        /// <param name="instantiateUIInventoryItemHandler">This handler should instantiate a prefab with UIInventoryItem component.</param>
        /// <param name="instantiateUIInventoryItemGridHandler">This handler should instantiate a prefab with UIInventoryItemGrid component.</param>
        /// <param name="instantiateUIInventoryItemVirtualOccupationQuadHandler">This handler should instantiate a image for indicating the occupation.</param>
        public UIInventory(
            string inventoryName,
            DragAreaIndicator dragAreaIndicator,
            DragProcessor dragProcessor,
            float canvasDistance,
            int gridSize, int rows, int columns, bool x_Mirror, bool z_Mirror,
            bool unlockedPartialGrids, int unlockedGridCount, bool dragOutDrop, bool enableScreenClamp, bool enableLog,
            KeyDownDelegate toggleUIInventoryKeyDownHandler,
            KeyDownDelegate rotateItemKeyDownHandler,
            InstantiatePrefabDelegate instantiateUIInventoryGridHandler,
            InstantiatePrefabDelegate instantiateUIInventoryItemHandler,
            InstantiatePrefabDelegate instantiateUIInventoryItemGridHandler,
            InstantiatePrefabDelegate instantiateUIInventoryItemVirtualOccupationQuadHandler
        ) : base(inventoryName, dragAreaIndicator, gridSize, rows, columns, x_Mirror, z_Mirror, unlockedPartialGrids, unlockedGridCount, dragOutDrop, enableLog, rotateItemKeyDownHandler,
            (gridPos) => new GridPosR(gridPos.x, -gridPos.z),
            (gridPos_matrix) => new GridPosR(gridPos_matrix.x, -gridPos_matrix.z),
            (gridPos) => new GridPosR(gridPos.x, -gridPos.z),
            (gridPos_matrix) => new GridPosR(gridPos_matrix.x, -gridPos_matrix.z))
        {
            DragProcessor = dragProcessor;
            CanvasDistance = canvasDistance;
            ToggleUIInventoryKeyDownHandler = toggleUIInventoryKeyDownHandler;
            InstantiateUIInventoryGridHandler = instantiateUIInventoryGridHandler;
            InstantiateUIInventoryItemHandler = instantiateUIInventoryItemHandler;
            InstantiateUIInventoryItemGridHandler = instantiateUIInventoryItemGridHandler;
            InstantiateUIInventoryItemVirtualOccupationQuadHandler = instantiateUIInventoryItemVirtualOccupationQuadHandler;
            EnableScreenClamp = enableScreenClamp;
        }

        public void Update()
        {
            if (ToggleUIInventoryKeyDownHandler != null && ToggleUIInventoryKeyDownHandler.Invoke())
            {
                IsOpen = !IsOpen;
            }

            if (ToggleDebugKeyDownHandler != null && ToggleDebugKeyDownHandler.Invoke())
            {
                IsDebug = !IsDebug;
            }
        }

        public UIInventoryGrid CreateUIInventoryGrid(Transform transform)
        {
            if (InstantiateUIInventoryGridHandler != null)
            {
                MonoBehaviour mono = InstantiateUIInventoryGridHandler?.Invoke(transform);
                if (mono != null)
                {
                    try
                    {
                        UIInventoryGrid res = (UIInventoryGrid) mono;
                        return res;
                    }
                    catch (Exception e)
                    {
                        LogError(e.ToString());
                    }
                }
            }

            return null;
        }

        public UIInventoryItem CreateUIInventoryItem(Transform transform)
        {
            if (InstantiateUIInventoryItemHandler != null)
            {
                MonoBehaviour mono = InstantiateUIInventoryItemHandler?.Invoke(transform);
                if (mono != null)
                {
                    try
                    {
                        UIInventoryItem res = (UIInventoryItem) mono;
                        return res;
                    }
                    catch (Exception e)
                    {
                        LogError(e.ToString());
                    }
                }
            }

            return null;
        }

        public UIInventoryItemGrid CreateUIInventoryItemGrid(Transform transform)
        {
            if (InstantiateUIInventoryItemGridHandler != null)
            {
                MonoBehaviour mono = InstantiateUIInventoryItemGridHandler?.Invoke(transform);
                if (mono != null)
                {
                    try
                    {
                        UIInventoryItemGrid res = (UIInventoryItemGrid) mono;
                        return res;
                    }
                    catch (Exception e)
                    {
                        LogError(e.ToString());
                    }
                }
            }

            return null;
        }

        public UIInventoryVirtualOccupationQuad CreateUIInventoryItemVirtualOccupationQuad(Transform transform)
        {
            if (InstantiateUIInventoryItemVirtualOccupationQuadHandler != null)
            {
                MonoBehaviour mono = InstantiateUIInventoryItemVirtualOccupationQuadHandler?.Invoke(transform);
                if (mono != null)
                {
                    try
                    {
                        UIInventoryVirtualOccupationQuad res = (UIInventoryVirtualOccupationQuad) mono;
                        return res;
                    }
                    catch (Exception e)
                    {
                        LogError(e.ToString());
                    }
                }
            }

            return null;
        }
    }
}
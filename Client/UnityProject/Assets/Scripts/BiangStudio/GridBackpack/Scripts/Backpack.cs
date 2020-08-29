using System;
using BiangStudio.DragHover;
using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ShapedInventory;
using UnityEngine;
using UnityEngine.Events;

namespace BiangStudio.GridBackpack
{
    [Serializable]
    public class Backpack : Inventory
    {
        public BackpackPanel BackpackPanel;

        private KeyDownDelegate ToggleBackpackKeyDownHandler;

        private InstantiatePrefabDelegate InstantiateBackpackGridHandler;
        private InstantiatePrefabDelegate InstantiateBackpackItemHandler;
        private InstantiatePrefabDelegate InstantiateBackpackItemGridHandler;
        private InstantiatePrefabDelegate InstantiateBackpackItemVirtualOccupationQuadHandler;

        /// <summary>
        /// This callback will be execute when the backpack is opened or closed
        /// </summary>
        public UnityAction<bool> ToggleBackpackCallback;

        /// <summary>
        /// This callback will be execute when the backpack debug mode is enable or disable
        /// </summary>
        public UnityAction<bool> ToggleDebugCallback;

        public delegate bool DragItemOutBackpackDelegate(BackpackItem item);

        /// <summary>
        /// This callback will be execute when the backpack item is dragged out of the backpack.
        /// </summary>
        public DragItemOutBackpackDelegate DragItemOutBackpackCallback;

        private bool isOpen = false;

        private bool IsOpen
        {
            get { return isOpen; }

            set
            {
                if (isOpen != value)
                {
                    ToggleBackpackCallback?.Invoke(value);
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

        /// <summary>
        /// Initialize the backpack manager.
        /// </summary>
        /// <param name="inventoryName">the name of the backpack</param>
        /// <param name="gridSize">the size (in pixel) of each grid of backpack items in UI panels.</param>
        /// <param name="toggleBackpackKeyDownHandler">This handler should return a signal which toggles the backpack(e.g. return Input.GetKeyDown(KeyCode.B);)</param>
        /// <param name="rotateItemKeyDownHandler">This handler should return a signal which rotates the backpack item(e.g. return Input.GetKeyDown(KeyCode.R);)</param>
        /// <param name="instantiateBackpackGridHandler">This handler should instantiate a prefab with BackpackGrid component.</param>
        /// <param name="instantiateBackpackItemHandler">This handler should instantiate a prefab with BackpackItem component.</param>
        /// <param name="instantiateBackpackItemGridHandler">This handler should instantiate a prefab with BackpackItemGrid component.</param>
        /// <param name="instantiateBackpackItemVirtualOccupationQuadHandler">This handler should instantiate a image for indicating the occupation.</param>
        public Backpack(
            string inventoryName, DragArea dragArea, int gridSize, int rows, int columns, bool x_Mirror, bool z_Mirror, bool unlockPartialGrids, int unlockedGridCount,
            KeyDownDelegate toggleBackpackKeyDownHandler,
            KeyDownDelegate rotateItemKeyDownHandler,
            InstantiatePrefabDelegate instantiateBackpackGridHandler,
            InstantiatePrefabDelegate instantiateBackpackItemHandler,
            InstantiatePrefabDelegate instantiateBackpackItemGridHandler,
            InstantiatePrefabDelegate instantiateBackpackItemVirtualOccupationQuadHandler
        ) : base(inventoryName, dragArea, gridSize, rows, columns, x_Mirror, z_Mirror, unlockPartialGrids, unlockedGridCount, rotateItemKeyDownHandler,
            (gridPos) => new GridPosR(gridPos.x, -gridPos.z),
            (gridPos_matrix) => new GridPosR(gridPos_matrix.x, -gridPos_matrix.z),
            (gridPos) => new GridPosR(gridPos.x, -gridPos.z),
            (gridPos_matrix) => new GridPosR(gridPos_matrix.x, -gridPos_matrix.z))
        {
            ToggleBackpackKeyDownHandler = toggleBackpackKeyDownHandler;
            InstantiateBackpackGridHandler = instantiateBackpackGridHandler;
            InstantiateBackpackItemHandler = instantiateBackpackItemHandler;
            InstantiateBackpackItemGridHandler = instantiateBackpackItemGridHandler;
            InstantiateBackpackItemVirtualOccupationQuadHandler = instantiateBackpackItemVirtualOccupationQuadHandler;
        }

        public void Update()
        {
            if (ToggleBackpackKeyDownHandler != null && ToggleBackpackKeyDownHandler.Invoke())
            {
                IsOpen = !IsOpen;
            }

            if (ToggleDebugKeyDownHandler != null && ToggleDebugKeyDownHandler.Invoke())
            {
                IsDebug = !IsDebug;
            }
        }

        public BackpackGrid CreateBackpackGrid(Transform transform)
        {
            if (InstantiateBackpackGridHandler != null)
            {
                MonoBehaviour mono = InstantiateBackpackGridHandler?.Invoke(transform);
                if (mono != null)
                {
                    try
                    {
                        BackpackGrid res = (BackpackGrid) mono;
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

        public BackpackItem CreateBackpackItem(Transform transform)
        {
            if (InstantiateBackpackItemHandler != null)
            {
                MonoBehaviour mono = InstantiateBackpackItemHandler?.Invoke(transform);
                if (mono != null)
                {
                    try
                    {
                        BackpackItem res = (BackpackItem) mono;
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

        public BackpackItemGrid CreateBackpackItemGrid(Transform transform)
        {
            if (InstantiateBackpackItemGridHandler != null)
            {
                MonoBehaviour mono = InstantiateBackpackItemGridHandler?.Invoke(transform);
                if (mono != null)
                {
                    try
                    {
                        BackpackItemGrid res = (BackpackItemGrid) mono;
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

        public BackpackVirtualOccupationQuad CreateBackpackItemVirtualOccupationQuad(Transform transform)
        {
            if (InstantiateBackpackItemVirtualOccupationQuadHandler != null)
            {
                MonoBehaviour mono = InstantiateBackpackItemVirtualOccupationQuadHandler?.Invoke(transform);
                if (mono != null)
                {
                    try
                    {
                        BackpackVirtualOccupationQuad res = (BackpackVirtualOccupationQuad) mono;
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
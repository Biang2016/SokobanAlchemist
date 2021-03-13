using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BiangLibrary.DragHover;
using BiangLibrary.GameDataFormat.Grid;
using UnityEngine;
using UnityEngine.Events;

namespace BiangLibrary.AdvancedInventory
{
    [Serializable]
    public abstract class Inventory
    {
        public string InventoryName;
        public DragAreaIndicator DragAreaIndicator;
        public bool EnableLog = false;

        public delegate bool KeyDownDelegate();

        public delegate void LogErrorDelegate(string log);

        public delegate GridPosR CoordinateTransformationDelegate(GridPosR gp);

        public CoordinateTransformationDelegate CoordinateTransformationHandler_FromPosToMatrixIndex { get; private set; }
        public CoordinateTransformationDelegate CoordinateTransformationHandler_FromMatrixIndexToPos { get; private set; }
        public CoordinateTransformationDelegate CoordinateTransformationHandler_FromPosToMatrixIndex_Diff { get; private set; }
        public CoordinateTransformationDelegate CoordinateTransformationHandler_FromMatrixIndexToPos_Diff { get; private set; }

        public delegate MonoBehaviour InstantiatePrefabDelegate(Transform parent);

        /// <summary>
        /// This handler should return a signal whether rotate the uiInventory(e.g. return Input.GetKeyDown(KeyCode.R);)
        /// </summary>
        public KeyDownDelegate RotateItemKeyDownHandler;

        /// <summary>
        /// This handler should return a signal whether toggles the uiInventory debug mode(e.g. return Input.GetKeyDown(KeyCode.U);) 
        /// </summary>
        public KeyDownDelegate ToggleDebugKeyDownHandler;

        public InventoryGrid[,] InventoryGridMatrix; // column, row
        public InventoryItem[,] InventoryItemMatrix; // column, row

        public InventoryInfo InventoryInfo = new InventoryInfo();

        public int GridSize { get; private set; }
        public int Rows { get; private set; }
        public int Columns { get; private set; }
        public bool X_Mirror { get; private set; }
        public bool Z_Mirror { get; private set; }

        public bool UnlockedPartialGrids { get; private set; }

        private int unlockedGridCount = 0;

        public int UnlockedGridCount
        {
            get { return unlockedGridCount; }

            private set
            {
                if (unlockedGridCount != value)
                {
                    unlockedGridCount = value;
                    RefreshInventoryGrids();
                }
            }
        }

        public bool DragOutDrop { get; private set; }

        public LogErrorDelegate LogErrorHandler;

        /// <summary>
        /// Create a new inventory
        /// </summary>
        /// <param name="inventoryName">The inventory name</param>
        /// <param name="dragAreaIndicator"></param>
        /// <param name="gridSize">the gridSize of inventory</param>
        /// <param name="rows">how many rows in total</param>
        /// <param name="columns">how many columns in total</param>
        /// <param name="x_Mirror">is X axis mirrored</param>
        /// <param name="z_Mirror">is Z axis mirrored</param>
        /// <param name="unlockedPartialGrids">is there any grid locked at the beginning</param>
        /// <param name="unlockedGridCount">how many grids are locked at the beginning</param>
        /// <param name="dragOutDrop">allows item to be dragged and dropped outside inventory</param>
        /// <param name="enableLog">enable inventory log</param>
        /// <param name="rotateItemKeyDownHandler">this delegate should return a bool whether the rotate item key is pressed down</param>
        /// <param name="coordinateTransformationHandler_FromPosToMatrixIndex"></param>
        /// <param name="coordinateTransformationHandler_FromMatrixIndexToPos"></param>
        /// <param name="coordinateTransformationHandler_FromPosToMatrixIndex_Diff"></param>
        /// <param name="coordinateTransformationHandler_FromMatrixIndexToPos_Diff"></param>
        protected Inventory(
            string inventoryName,
            DragAreaIndicator dragAreaIndicator,
            int gridSize,
            int rows,
            int columns,
            bool x_Mirror,
            bool z_Mirror,
            bool unlockedPartialGrids,
            int unlockedGridCount,
            bool dragOutDrop,
            bool enableLog,
            KeyDownDelegate rotateItemKeyDownHandler,
            CoordinateTransformationDelegate coordinateTransformationHandler_FromPosToMatrixIndex,
            CoordinateTransformationDelegate coordinateTransformationHandler_FromMatrixIndexToPos,
            CoordinateTransformationDelegate coordinateTransformationHandler_FromPosToMatrixIndex_Diff,
            CoordinateTransformationDelegate coordinateTransformationHandler_FromMatrixIndexToPos_Diff
        )
        {
            InventoryName = inventoryName;
            DragAreaIndicator = dragAreaIndicator;
            GridSize = gridSize;
            Rows = rows;
            Columns = columns;
            X_Mirror = x_Mirror;
            Z_Mirror = z_Mirror;
            UnlockedPartialGrids = unlockedPartialGrids;
            this.unlockedGridCount = unlockedGridCount;
            DragOutDrop = dragOutDrop;
            EnableLog = enableLog;

            RotateItemKeyDownHandler = rotateItemKeyDownHandler;
            CoordinateTransformationHandler_FromPosToMatrixIndex = coordinateTransformationHandler_FromPosToMatrixIndex;
            CoordinateTransformationHandler_FromMatrixIndexToPos = coordinateTransformationHandler_FromMatrixIndexToPos;
            CoordinateTransformationHandler_FromPosToMatrixIndex_Diff = coordinateTransformationHandler_FromPosToMatrixIndex_Diff;
            CoordinateTransformationHandler_FromMatrixIndexToPos_Diff = coordinateTransformationHandler_FromMatrixIndexToPos_Diff;

            InventoryGridMatrix = new InventoryGrid[Columns, Rows];
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    InventoryGrid ig = new InventoryGrid();
                    ig.State = InventoryGrid.States.Unavailable;
                    InventoryGridMatrix[col, row] = ig;
                }
            }

            InventoryItemMatrix = new InventoryItem[Columns, Rows];
        }

        public void UnlockGrids(int number)
        {
            this.unlockedGridCount = Mathf.Clamp(unlockedGridCount + number, 0, Columns * Rows);
            RefreshInventoryGrids();
        }

        public void RemoveAllInventoryItems()
        {
            foreach (InventoryItem inventoryItem in InventoryInfo.InventoryItems_IncludingPreview.ToArray().ToList())
            {
                RemoveItem(inventoryItem, true);
            }
        }

        public void LogError(string log)
        {
            LogErrorHandler?.Invoke($"BiangLibrary.AdvancedInventory Error: {log}");
        }

        public bool ContainsGP(GridPos gp)
        {
            if (gp.x < 0 || gp.x >= Columns || gp.z < 0 || gp.z >= Rows)
            {
                return false;
            }

            return true;
        }

        public bool ContainsIndex(int col, int row)
        {
            if (col < 0 || col >= Columns || row < 0 || row >= Rows)
            {
                return false;
            }

            return true;
        }

        public void RefreshInventoryGrids()
        {
            RefreshConflictAndIsolation();
            int count = 0;
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    count++;
                    InventoryGridMatrix[col, row].State = (count > unlockedGridCount && UnlockedPartialGrids)
                        ? InventoryGrid.States.Locked
                        : (InventoryItemMatrix[col, row] != null ? InventoryGrid.States.Unavailable : InventoryGrid.States.Available);
                }
            }
        }

        public bool TryAddItem(InventoryItem item)
        {
            bool canPlaceDirectly = CheckSpaceAvailable(item.OccupiedGridPositions_Matrix);
            if (canPlaceDirectly)
            {
                return AddItem(item);
            }

            bool placeFound = FindSpaceToPutItem(item, false);
            if (placeFound)
            {
                return AddItem(item);
            }
            else
            {
                return false;
            }
        }

        public bool FindSpaceToPutItem(InventoryItem item, bool forPreviewItem)
        {
            GridPosR oriGPR = item.GridPos_Matrix;
            foreach (GridPosR.Orientation orientation in Enum.GetValues(typeof(GridPosR.Orientation)))
            {
                item.GridPos_Matrix.orientation = (GridPosR.Orientation) (((int) item.GridPos_Matrix.orientation + (int) orientation) % 4);
                for (int z = 0; z < Rows; z++)
                {
                    for (int x = 0; x < Columns; x++)
                    {
                        item.GridPos_Matrix.z = z;
                        item.GridPos_Matrix.x = x;
                        if (CheckSpaceAvailable(item.OccupiedGridPositions_Matrix, forPreviewItem))
                        {
                            item.OnRefreshViewHandler?.Invoke();
                            return true;
                        }
                    }
                }
            }

            item.GridPos_Matrix = oriGPR;
            return false;
        }

        public bool CheckSpaceAvailable(List<GridPos> realGridPoses, bool forPreviewItem = false)
        {
            foreach (GridPos gp in realGridPoses)
            {
                GridPos addedGP = gp;

                if (!ContainsGP(addedGP))
                {
                    return false;
                }

                if (!forPreviewItem)
                {
                    if (!InventoryGridMatrix[addedGP.x, addedGP.z].Available)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public UnityAction<InventoryItem> OnAddItemSucAction;

        public bool AddPreviewItem(InventoryItem item)
        {
            return AddItem(item, true, true);
        }

        private bool AddItem(InventoryItem item, bool isPreviewItem = false, bool needCallback = true)
        {
            if (InventoryInfo.AddInventoryItem(item, isPreviewItem))
            {
                item.Inventory = this;
                if (needCallback) OnAddItemSucAction?.Invoke(item);
                StringBuilder sb = new StringBuilder();
                foreach (GridPos gp_matrix in item.OccupiedGridPositions_Matrix)
                {
                    sb.Append(gp_matrix.ToString());
                    if (!isPreviewItem) InventoryGridMatrix[gp_matrix.x, gp_matrix.z].State = InventoryGrid.States.Unavailable;
                }

                if (EnableLog) Debug.Log((isPreviewItem ? $"AddPreviewItem: {item} " : $"AddItem: {item} ") + sb);
                sb.Clear();
                RefreshConflictAndIsolation();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ResetGrids(List<GridPos> oldOccupiedGPs)
        {
            foreach (GridPos gp in oldOccupiedGPs)
            {
                if (InventoryGridMatrix[gp.x, gp.z].State != InventoryGrid.States.Locked)
                {
                    InventoryGridMatrix[gp.x, gp.z].State = InventoryGrid.States.Available;
                }
            }
        }

        public UnityAction<InventoryItem> OnRemoveItemSucAction;

        public void RemoveItem(InventoryItem item, bool needCallback)
        {
            if (InventoryInfo.HasItem(item))
            {
                StringBuilder sb = new StringBuilder();
                foreach (GridPos gp_matrix in item.OccupiedGridPositions_Matrix)
                {
                    sb.Append(gp_matrix.ToString());
                }

                if (InventoryInfo.HasFormalItem(item))
                {
                    foreach (GridPos gp_matrix in item.OccupiedGridPositions_Matrix)
                    {
                        if (InventoryGridMatrix[gp_matrix.x, gp_matrix.z].State != InventoryGrid.States.Locked)
                        {
                            InventoryGridMatrix[gp_matrix.x, gp_matrix.z].State = InventoryGrid.States.Available;
                        }
                    }

                    if (EnableLog) Debug.Log($"RemoveItem: {item} " + sb);
                }
                else if (InventoryInfo.HasPreviewItem(item))
                {
                    if (EnableLog) Debug.Log($"RemovePreviewItem: {item} " + sb);
                }

                InventoryInfo.RemoveInventoryItem(item);
                if (needCallback) OnRemoveItemSucAction?.Invoke(item);
                RefreshConflictAndIsolation();
            }
        }

        public void PickUpItem(InventoryItem item)
        {
            if (InventoryInfo.HasFormalItem(item))
            {
                StringBuilder sb = new StringBuilder();
                foreach (GridPos gp_matrix in item.OccupiedGridPositions_Matrix)
                {
                    sb.Append(gp_matrix.ToString());
                    if (InventoryItemMatrix[gp_matrix.x, gp_matrix.z] == item)
                    {
                        InventoryGridMatrix[gp_matrix.x, gp_matrix.z].State = InventoryGrid.States.TempUnavailable;
                        InventoryItemMatrix[gp_matrix.x, gp_matrix.z] = null;
                    }
                }

                if (EnableLog) Debug.Log($"PickUpItem: {item} " + sb);
            }
        }

        public void PutDownItem(InventoryItem item)
        {
            if (!InventoryInfo.HasItem(item)) return;
            if (InventoryInfo.HasPreviewItem(item))
            {
                AddItem(item, false, false);
            }
            else
            {
                ClearTempUnavailableGridState();
                StringBuilder sb = new StringBuilder();
                foreach (GridPos gp_matrix in item.OccupiedGridPositions_Matrix)
                {
                    sb.Append(gp_matrix.ToString());
                    InventoryItemMatrix[gp_matrix.x, gp_matrix.z] = item;
                    InventoryGridMatrix[gp_matrix.x, gp_matrix.z].State = InventoryGrid.States.Unavailable;
                }

                if (EnableLog) Debug.Log($"PutDownItem: {item} " + sb);
                RefreshConflictAndIsolation();
            }
        }

        public void ClearTempUnavailableGridState()
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    if (InventoryGridMatrix[col, row].State == InventoryGrid.States.TempUnavailable)
                    {
                        InventoryGridMatrix[col, row].State = InventoryGrid.States.Available;
                    }
                }
            }
        }

        public void RefreshConflictAndIsolation()
        {
            RefreshConflictAndIsolation(out List<InventoryItem> conflictItems, out List<InventoryItem> isolatedItems);
        }

        public void RefreshConflictAndIsolation(out List<InventoryItem> conflictItems, out List<InventoryItem> isolatedItems)
        {
            foreach (InventoryItem item in InventoryInfo.InventoryItems_IncludingPreview)
            {
                item.OnIsolatedHandler?.Invoke(false);
                item.OnResetConflictHandler?.Invoke();
            }

            List<GridPos> coreGPs = new List<GridPos>();
            List<InventoryItem> notConflictItems = new List<InventoryItem>();

            // Find conflict items
            List<GridPos> conflictGridPositions = new List<GridPos>();
            conflictItems = new List<InventoryItem>();

            for (int col = 0; col < Columns; col++)
            {
                for (int row = 0; row < Rows; row++)
                {
                    InventoryItemMatrix[col, row] = null;
                }
            }

            foreach (InventoryItem item in InventoryInfo.InventoryItems_IncludingPreview)
            {
                bool isRootItem = item.AmIRootItemInIsolationCalculationHandler != null && item.AmIRootItemInIsolationCalculationHandler.Invoke();
                bool hasConflict = false;
                foreach (GridPos gp_matrix in item.OccupiedGridPositions_Matrix)
                {
                    if (!ContainsGP(gp_matrix))
                    {
                        hasConflict = true;
                    }
                    else
                    {
                        if (InventoryItemMatrix[gp_matrix.x, gp_matrix.z] != null)
                        {
                            hasConflict = true;
                            conflictGridPositions.Add(gp_matrix);
                        }
                        else
                        {
                            InventoryItemMatrix[gp_matrix.x, gp_matrix.z] = item;
                            if (isRootItem) coreGPs.Add(gp_matrix);
                        }
                    }
                }

                if (hasConflict)
                {
                    conflictItems.Add(item);
                }
                else
                {
                    notConflictItems.Add(item);
                }
            }

            foreach (GridPos gp in conflictGridPositions)
            {
                AddForbidComponentIndicator(gp);
            }

            // Find isolated components
            List<GridPos> isolatedGridPositions = new List<GridPos>();
            isolatedItems = new List<InventoryItem>();

            int[,] connectedMatrix = new int[Columns, Rows];

            foreach (InventoryItem item in notConflictItems)
            {
                foreach (GridPos gp_matrix in item.OccupiedGridPositions_Matrix)
                {
                    connectedMatrix[gp_matrix.x, gp_matrix.z] = 1;
                }
            }

            Queue<GridPos> connectedQueue = new Queue<GridPos>();

            foreach (GridPos coreGP in coreGPs)
            {
                connectedMatrix[coreGP.x, coreGP.z] = 2;
                connectedQueue.Enqueue(coreGP);
            }

            void connectPos(int col, int row)
            {
                if (!ContainsGP(new GridPos(col, row)))
                {
                    return;
                }
                else
                {
                    if (connectedMatrix[col, row] == 1)
                    {
                        connectedQueue.Enqueue(new GridPos(col, row));
                        connectedMatrix[col, row] = 2;
                    }
                }
            }

            while (connectedQueue.Count > 0)
            {
                GridPos gp = connectedQueue.Dequeue();
                connectPos(gp.x + 1, gp.z);
                connectPos(gp.x - 1, gp.z);
                connectPos(gp.x, gp.z - 1);
                connectPos(gp.x, gp.z + 1);
            }

            for (int col = 0; col < Columns; col++)
            {
                for (int row = 0; row < Rows; row++)
                {
                    if (connectedMatrix[col, row] == 1)
                    {
                        isolatedGridPositions.Add((new GridPos(col, row)));
                        InventoryItem isolatedItem = InventoryItemMatrix[col, row];
                        if (!isolatedItems.Contains(isolatedItem))
                        {
                            isolatedItems.Add(isolatedItem);
                        }
                    }
                }
            }

            foreach (GridPos gp in isolatedGridPositions)
            {
                AddIsolatedComponentIndicator(gp);
            }
        }

        private void AddForbidComponentIndicator(GridPos gp_matrix)
        {
            InventoryItem item = InventoryItemMatrix[gp_matrix.x, gp_matrix.z];
            if (item != null)
            {
                GridPos gp_local_noRotate = gp_matrix - (GridPos) item.GridPos_Matrix;
                GridPos gp_local_rotate = GridPos.RotateGridPos(gp_local_noRotate, (GridPosR.Orientation) ((4 - (int) item.GridPos_Matrix.orientation) % 4));
                item.OnConflictedHandler?.Invoke(gp_local_rotate);
            }
        }

        private void AddIsolatedComponentIndicator(GridPos gp_matrix)
        {
            InventoryItem item = InventoryItemMatrix[gp_matrix.x, gp_matrix.z];
            item?.OnIsolatedHandler?.Invoke(true);
        }

        public void MoveAllItemTogether(GridPos delta_local_GP)
        {
            GridPos delta_matrix = CoordinateTransformationHandler_FromPosToMatrixIndex_Diff(delta_local_GP);
            foreach (InventoryItem item in InventoryInfo.InventoryItems)
            {
                foreach (GridPos gp_matrix in item.OccupiedGridPositions_Matrix)
                {
                    GridPos newGP = gp_matrix + delta_matrix;
                    if (newGP.x >= Columns || newGP.x < 0)
                    {
                        MoveAllItemTogether(new GridPos(0, delta_matrix.z));
                        return;
                    }

                    if (newGP.z >= Rows || newGP.z < 0)
                    {
                        MoveAllItemTogether(new GridPos(delta_matrix.x, 0));
                        return;
                    }
                }
            }

            foreach (InventoryItem item in InventoryInfo.InventoryItems)
            {
                GridPosR newGridPos_Matrix = item.GridPos_Matrix + delta_matrix;
                item.SetGridPosition(newGridPos_Matrix);
            }

            RefreshConflictAndIsolation();
        }
    }

    [Serializable]
    public class InventoryInfo
    {
        private List<InventoryItem> inventoryItems = new List<InventoryItem>();
        public List<InventoryItem> InventoryItems => inventoryItems;

        /// <summary>
        /// Only for move-in item, not for pick-up item from the same inventory
        /// </summary>
        private InventoryItem previewInventoryItem;

        public InventoryItem PreviewInventoryItem => previewInventoryItem;

        private List<InventoryItem> inventoryItems_IncludingPreview = new List<InventoryItem>();
        public List<InventoryItem> InventoryItems_IncludingPreview => inventoryItems_IncludingPreview;

        private Dictionary<uint, InventoryItem> inventoryItemDict = new Dictionary<uint, InventoryItem>();

        public void Clear()
        {
            inventoryItems.Clear();
            previewInventoryItem = null;
            inventoryItems_IncludingPreview.Clear();
            inventoryItemDict.Clear();
        }

        /// <summary>
        /// Add item to inventory, including the previewing item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool AddInventoryItem(InventoryItem item, bool previewItem = false)
        {
            // Convert preview item to formal item
            if (!previewItem && previewInventoryItem != null && item == previewInventoryItem)
            {
                previewInventoryItem = null;
                inventoryItems.Add(item);
                return true;
            }

            if (inventoryItemDict.ContainsKey(item.GUID)) return false;

            inventoryItemDict.Add(item.GUID, item);
            if (previewItem)
            {
                ClearPreviewItem();
                previewInventoryItem = item;
            }
            else
            {
                inventoryItems.Add(item);
            }

            inventoryItems_IncludingPreview.Add(item);
            return true;
        }

        public bool RemoveInventoryItem(InventoryItem item)
        {
            if (previewInventoryItem != null && item == previewInventoryItem)
            {
                previewInventoryItem = null;
                if (inventoryItemDict.ContainsKey(item.GUID))
                {
                    inventoryItems_IncludingPreview.Remove(item);
                    inventoryItemDict.Remove(item.GUID);
                }

                return true;
            }
            else
            {
                if (inventoryItemDict.ContainsKey(item.GUID))
                {
                    inventoryItems.Remove(item);
                    inventoryItems_IncludingPreview.Remove(item);
                    inventoryItemDict.Remove(item.GUID);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool HasFormalItem(InventoryItem item)
        {
            if (item == null) return false;
            return inventoryItemDict.ContainsKey(item.GUID) && previewInventoryItem != item;
        }

        public bool HasItem(InventoryItem item)
        {
            if (item == null) return false;
            return inventoryItemDict.ContainsKey(item.GUID);
        }

        public bool HasPreviewItem(InventoryItem item)
        {
            if (item == null) return false;
            return previewInventoryItem == item;
        }

        public void ClearPreviewItem()
        {
            if (previewInventoryItem != null)
            {
                inventoryItemDict.Remove(previewInventoryItem.GUID);
                inventoryItems_IncludingPreview.Remove(previewInventoryItem);
                previewInventoryItem = null;
            }
        }
    }
}
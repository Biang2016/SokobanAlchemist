using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using UnityEngine;

namespace BiangLibrary.AdvancedInventory
{
    public interface IInventoryItemContentInfo
    {
        /// <summary>
        /// The relative occupied grids of this item.
        /// For example:
        /// {(0,0), (0,1), (0,2)} is a list of GridPos for a three-grid-size item.
        /// {(0,0), (0,1), (1,0), (1,1)} is a list of GridPos for a square item (edge length = 2).
        /// </summary>
        List<GridPos> IInventoryItemContentInfo_OriginalOccupiedGridPositions { get; }

        /// <summary>
        /// If needed, you can define a category name for this item.
        /// </summary>
        string ItemCategoryName { get; }

        /// <summary>
        /// If needed, you can define a name for this item.
        /// </summary>
        string ItemName { get; }

        /// <summary>
        /// If needed, you can define a quality for this item.
        /// </summary>
        string ItemQuality { get; }

        /// <summary>
        /// If needed, you can define basic information for this item.
        /// </summary>
        string ItemBasicInfo { get; }

        /// <summary>
        /// If needed, you can define detailed information for this item.
        /// </summary>
        string ItemDetailedInfo { get; }

        /// <summary>
        /// This sprite is for items which might be displayed in UI
        /// </summary>
        Sprite ItemSprite { get; }

        /// <summary>
        /// This sprite (1x1 size) is for items which might be displayed in UI (square grid)
        /// </summary>
        Sprite ItemSprite_1x1 { get; }

        /// <summary>
        /// If needed, you can define a color for this item.
        /// </summary>
        Color ItemColor { get; }

        bool Rotatable { get; }
    }
}
using System.Collections.Generic;
using BiangStudio.GameDataFormat.Grid;
using UnityEngine;

namespace BiangStudio.ShapedInventory
{
    public interface IInventoryItemContentInfo
    {
        List<GridPos> IInventoryItemContentInfo_OriginalOccupiedGridPositions { get; }
        string ItemCategoryName { get; }
        string ItemName { get; }
        string ItemQuality { get; }
        string ItemBasicInfo { get; }
        string ItemDetailedInfo { get; }
        string ItemSpriteKey { get; }
        Color ItemColor { get; }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace BiangStudio.GridBackpack
{
    public class BackpackItemVirtualOccupationRoot : MonoBehaviour
    {
        internal List<BackpackVirtualOccupationQuad> backpackVirtualOccupationQuads = new List<BackpackVirtualOccupationQuad>();

        internal void Clear()
        {
            foreach (BackpackVirtualOccupationQuad quad in backpackVirtualOccupationQuads)
            {
                quad.PoolRecycle();
            }
        }
    }
}
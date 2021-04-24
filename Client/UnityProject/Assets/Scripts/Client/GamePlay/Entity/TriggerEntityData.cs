using System.Collections;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using UnityEngine;

public class TriggerEntityData : IClone<TriggerEntityData>
{
    public GridPos3D WorldGP;
    public GridPos3D LocalGP;

    public EntityData EntityData = new EntityData();

    public TriggerEntityData Clone()
    {
        TriggerEntityData cloneData = new TriggerEntityData();
        cloneData.WorldGP = WorldGP;
        cloneData.LocalGP = LocalGP;
        cloneData.EntityData = EntityData.Clone();
        return cloneData;
    }
}
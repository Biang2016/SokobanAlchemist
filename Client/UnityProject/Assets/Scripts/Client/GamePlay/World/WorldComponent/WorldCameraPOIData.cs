using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;

public class WorldCameraPOIData : IClone<WorldCameraPOIData>
{
    public List<GridPos3D> POIs = new List<GridPos3D>();

    public WorldCameraPOIData Clone()
    {
        WorldCameraPOIData newData = new WorldCameraPOIData();
        newData.POIs = POIs.Clone();
        return newData;
    }
}
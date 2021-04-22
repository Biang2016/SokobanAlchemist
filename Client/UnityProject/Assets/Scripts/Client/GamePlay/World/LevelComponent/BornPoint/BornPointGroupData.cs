using System.Collections.Generic;
using BiangLibrary.CloneVariant;

public class BornPointGroupData : IClone<BornPointGroupData>
{
    public SortedDictionary<string, BornPointData> PlayerBornPoints = new SortedDictionary<string, BornPointData>();

    public BornPointGroupData Clone()
    {
        BornPointGroupData newData = new BornPointGroupData();
        newData.PlayerBornPoints = PlayerBornPoints.Clone<string, BornPointData, string, LevelComponentData>();
        return newData;
    }
}
using System.Collections.Generic;
using BiangStudio.CloneVariant;

public class BornPointGroupData : IClone<BornPointGroupData>
{
    public List<BornPointData> BornPoints = new List<BornPointData>();

    public BornPointGroupData Clone()
    {
        BornPointGroupData newData = new BornPointGroupData();
        newData.BornPoints = BornPoints.Clone();
        return newData;
    }
}
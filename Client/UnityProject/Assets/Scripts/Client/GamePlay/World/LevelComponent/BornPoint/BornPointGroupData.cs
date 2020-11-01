using System.Collections.Generic;
using BiangStudio.CloneVariant;

public class BornPointGroupData : IClone<BornPointGroupData>
{
    public List<BornPointData> EnemyBornPoints = new List<BornPointData>();
    public SortedDictionary<string, BornPointData> PlayerBornPoints = new SortedDictionary<string, BornPointData>();

    public BornPointGroupData Clone()
    {
        BornPointGroupData newData = new BornPointGroupData();
        newData.EnemyBornPoints = EnemyBornPoints.Clone();
        newData.PlayerBornPoints = PlayerBornPoints.Clone();
        return newData;
    }
}
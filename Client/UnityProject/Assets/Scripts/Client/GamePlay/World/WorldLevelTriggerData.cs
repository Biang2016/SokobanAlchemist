using System.Collections.Generic;
using BiangStudio.CloneVariant;

public class WorldLevelTriggerData : IClone<WorldLevelTriggerData>
{
    public List<LevelTriggerBase.Data> TriggerDataList = new List<LevelTriggerBase.Data>();

    public WorldLevelTriggerData Clone()
    {
        WorldLevelTriggerData newData = new WorldLevelTriggerData();
        newData.TriggerDataList = TriggerDataList.Clone();
        return newData;
    }
}
using System.Collections.Generic;
using BiangStudio.CloneVariant;

public class WorldModuleLevelTriggerData : IClone<WorldModuleLevelTriggerData>
{
    public List<LevelTriggerBase.Data> TriggerDataList = new List<LevelTriggerBase.Data>();

    public WorldModuleLevelTriggerData Clone()
    {
        WorldModuleLevelTriggerData newData = new WorldModuleLevelTriggerData();
        newData.TriggerDataList = TriggerDataList.Clone();
        return newData;
    }
}
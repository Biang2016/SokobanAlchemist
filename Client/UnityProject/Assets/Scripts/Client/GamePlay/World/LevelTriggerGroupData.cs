using System.Collections.Generic;
using BiangStudio.CloneVariant;

public class LevelTriggerGroupData : IClone<LevelTriggerGroupData>
{
    public List<LevelTriggerBase.Data> TriggerDataList = new List<LevelTriggerBase.Data>();

    public LevelTriggerGroupData Clone()
    {
        LevelTriggerGroupData newData = new LevelTriggerGroupData();
        newData.TriggerDataList = TriggerDataList.Clone();
        return newData;
    }
}
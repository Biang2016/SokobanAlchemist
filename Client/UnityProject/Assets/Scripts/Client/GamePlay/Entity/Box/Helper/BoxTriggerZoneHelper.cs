using System.Collections.Generic;
using System.Linq;

public class BoxTriggerZoneHelper : BoxMonoHelper
{
    private List<BoxTriggerZone> BoxTriggerZones;

    void Awake()
    {
        BoxTriggerZones = GetComponentsInChildren<BoxTriggerZone>().ToList();
        foreach (BoxTriggerZone zone in BoxTriggerZones)
        {
            zone.BoxTriggerZoneHelper = this;
        }
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
        ActorStayTimeDict.Clear();
    }

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
    }

    public Dictionary<uint, float> ActorStayTimeDict = new Dictionary<uint, float>();
}
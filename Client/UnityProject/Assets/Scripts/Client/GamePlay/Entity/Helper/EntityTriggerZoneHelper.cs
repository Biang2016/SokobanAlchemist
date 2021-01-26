using System.Collections.Generic;
using System.Linq;

public class EntityTriggerZoneHelper : EntityMonoHelper
{
    private List<EntityTriggerZone> EntityTriggerZones;

    void Awake()
    {
        EntityTriggerZones = GetComponentsInChildren<EntityTriggerZone>().ToList();
        foreach (EntityTriggerZone zone in EntityTriggerZones)
        {
            zone.EntityTriggerZoneHelper = this;
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
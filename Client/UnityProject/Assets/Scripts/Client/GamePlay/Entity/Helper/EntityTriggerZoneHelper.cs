using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityTriggerZoneHelper : EntityMonoHelper, IEntityTriggerZone
{
    private List<EntityTriggerZone> EntityTriggerZones;

    void Awake()
    {
        EntityTriggerZones = GetComponentsInChildren<EntityTriggerZone>().ToList();
        foreach (EntityTriggerZone zone in EntityTriggerZones)
        {
            zone.IEntityTriggerZone = this;
        }
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
    }

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
    }

    public void OnTriggerZoneEnter(Collider c)
    {
        foreach (EntityPassiveSkill ps in Entity.EntityPassiveSkills)
        {
            ps.OnTriggerZoneEnter(c);
        }
    }

    public void OnTriggerZoneStay(Collider c)
    {
        foreach (EntityPassiveSkill ps in Entity.EntityPassiveSkills)
        {
            ps.OnTriggerZoneStay(c);
        }
    }

    public void OnTriggerZoneExit(Collider c)
    {
        foreach (EntityPassiveSkill ps in Entity.EntityPassiveSkills)
        {
            ps.OnTriggerZoneExit(c);
        }
    }
}
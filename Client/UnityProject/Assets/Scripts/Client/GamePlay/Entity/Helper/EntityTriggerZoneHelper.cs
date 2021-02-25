using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityTriggerZoneHelper : EntityMonoHelper, IEntityTriggerZoneHelper
{
    private List<EntityTriggerZone> EntityTriggerZones;

    void Awake()
    {
        EntityTriggerZones = GetComponentsInChildren<EntityTriggerZone>(true).ToList();
        foreach (EntityTriggerZone zone in EntityTriggerZones)
        {
            zone.IEntityTriggerZone = this;
        }

        SetActive(false);
    }

    public void SetActive(bool active)
    {
        foreach (EntityTriggerZone zone in EntityTriggerZones)
        {
            zone.Collider.enabled = active;
        }
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
        SetActive(true);
    }

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
        SetActive(false);
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
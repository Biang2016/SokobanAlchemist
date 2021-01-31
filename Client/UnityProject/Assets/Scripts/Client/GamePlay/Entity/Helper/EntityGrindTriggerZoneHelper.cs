using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityGrindTriggerZoneHelper : EntityMonoHelper, IEntityTriggerZone
{
    private List<EntityTriggerZone> EntityTriggerZones;

    void Awake()
    {
        EntityTriggerZones = GetComponentsInChildren<EntityTriggerZone>(true).ToList();
        foreach (EntityTriggerZone zone in EntityTriggerZones)
        {
            zone.IEntityTriggerZone = this;
        }
    }

    public void SetActive(bool active)
    {
        foreach (EntityTriggerZone trigger in EntityTriggerZones)
        {
            trigger.Collider.enabled = active;
        }
    }

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
        SetActive(false);
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
        SetActive(false);
    }

    public void OnTriggerZoneEnter(Collider c)
    {
        foreach (EntityPassiveSkill eps in Entity.EntityPassiveSkills)
        {
            eps.OnGrindTriggerZoneEnter(c);
        }
    }

    public void OnTriggerZoneStay(Collider c)
    {
        foreach (EntityPassiveSkill eps in Entity.EntityPassiveSkills)
        {
            eps.OnGrindTriggerZoneStay(c);
        }
    }

    public void OnTriggerZoneExit(Collider c)
    {
        foreach (EntityPassiveSkill eps in Entity.EntityPassiveSkills)
        {
            eps.OnGrindTriggerZoneExit(c);
        }
    }
}
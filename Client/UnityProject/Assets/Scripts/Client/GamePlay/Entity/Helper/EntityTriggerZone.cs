using UnityEngine;

public class EntityTriggerZone : MonoBehaviour
{
    [HideInInspector]
    public EntityTriggerZoneHelper EntityTriggerZoneHelper;

    public void OnTriggerEnter(Collider c)
    {
        foreach (EntityPassiveSkill ps in EntityTriggerZoneHelper.Entity.EntityPassiveSkills)
        {
            ps.OnBoxTriggerZoneEnter(c);
        }
    }

    public void OnTriggerStay(Collider c)
    {
        foreach (EntityPassiveSkill ps in EntityTriggerZoneHelper.Entity.EntityPassiveSkills)
        {
            ps.OnBoxTriggerZoneStay(c);
        }
    }

    public void OnTriggerExit(Collider c)
    {
        foreach (EntityPassiveSkill ps in EntityTriggerZoneHelper.Entity.EntityPassiveSkills)
        {
            ps.OnBoxTriggerZoneExit(c);
        }
    }
}
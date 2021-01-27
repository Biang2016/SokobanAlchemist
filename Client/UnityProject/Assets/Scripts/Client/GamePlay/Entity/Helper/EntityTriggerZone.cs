using UnityEngine;

public class EntityTriggerZone : MonoBehaviour
{
    [HideInInspector]
    public EntityTriggerZoneHelper EntityTriggerZoneHelper;

    public void OnTriggerEnter(Collider c)
    {
        foreach (EntityPassiveSkill ps in EntityTriggerZoneHelper.Entity.EntityPassiveSkills)
        {
            ps.OnTriggerZoneEnter(c);
        }
    }

    public void OnTriggerStay(Collider c)
    {
        foreach (EntityPassiveSkill ps in EntityTriggerZoneHelper.Entity.EntityPassiveSkills)
        {
            ps.OnTriggerZoneStay(c);
        }
    }

    public void OnTriggerExit(Collider c)
    {
        foreach (EntityPassiveSkill ps in EntityTriggerZoneHelper.Entity.EntityPassiveSkills)
        {
            ps.OnTriggerZoneExit(c);
        }
    }
}
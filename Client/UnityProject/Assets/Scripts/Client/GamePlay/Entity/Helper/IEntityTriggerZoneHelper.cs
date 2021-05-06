using UnityEngine;

public interface IEntityTriggerZoneHelper
{
    void OnTriggerZoneEnter(Collider c, EntityTriggerZone entityTriggerZone);

    void OnTriggerZoneStay(Collider c, EntityTriggerZone entityTriggerZone);

    void OnTriggerZoneExit(Collider c, EntityTriggerZone entityTriggerZone);
}
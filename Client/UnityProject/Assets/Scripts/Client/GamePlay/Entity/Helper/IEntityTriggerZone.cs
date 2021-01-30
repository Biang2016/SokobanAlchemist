using UnityEngine;

public interface IEntityTriggerZone
{
    void OnTriggerZoneEnter(Collider c);

    void OnTriggerZoneStay(Collider c);

    void OnTriggerZoneExit(Collider c);
}
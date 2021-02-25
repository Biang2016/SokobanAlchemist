using UnityEngine;

public interface IEntityTriggerZoneHelper
{
    void OnTriggerZoneEnter(Collider c);

    void OnTriggerZoneStay(Collider c);

    void OnTriggerZoneExit(Collider c);
}
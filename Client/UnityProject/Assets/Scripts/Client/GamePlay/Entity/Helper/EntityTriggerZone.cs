using UnityEngine;

public class EntityTriggerZone : MonoBehaviour
{
    [HideInInspector]
    public IEntityTriggerZone IEntityTriggerZone;

    public Collider Collider;

    public void OnTriggerEnter(Collider c)
    {
        IEntityTriggerZone.OnTriggerZoneEnter(c);
    }

    public void OnTriggerStay(Collider c)
    {
        IEntityTriggerZone.OnTriggerZoneStay(c);
    }

    public void OnTriggerExit(Collider c)
    {
        IEntityTriggerZone.OnTriggerZoneExit(c);
    }
}
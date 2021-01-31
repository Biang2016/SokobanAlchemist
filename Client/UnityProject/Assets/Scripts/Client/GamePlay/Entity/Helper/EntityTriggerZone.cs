using UnityEngine;

public class EntityTriggerZone : MonoBehaviour
{
    [HideInInspector]
    public IEntityTriggerZone IEntityTriggerZone;

    private Collider m_collider;

    internal Collider Collider
    {
        get
        {
            if (m_collider) return m_collider;
            else
            {
                m_collider = GetComponent<Collider>();
                return m_collider;
            }
        }
    }

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
using UnityEngine;

public class EntityTriggerZone : MonoBehaviour
{
    [HideInInspector]
    public IEntityTriggerZoneHelper IEntityTriggerZone;

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
        if (!BattleManager.Instance.IsStart) return;
        IEntityTriggerZone.OnTriggerZoneEnter(c);
    }

    public void OnTriggerStay(Collider c)
    {
        if (!BattleManager.Instance.IsStart) return;
        IEntityTriggerZone.OnTriggerZoneStay(c);
    }

    public void OnTriggerExit(Collider c)
    {
        if (!BattleManager.Instance.IsStart) return;
        IEntityTriggerZone.OnTriggerZoneExit(c);
    }
}
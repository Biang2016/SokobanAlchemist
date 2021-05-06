using UnityEngine;

public abstract class EntityMonoHelper : MonoBehaviour
{
    private bool hasEntity = false;
    private Entity entity;

    internal Entity Entity
    {
        get
        {
            if (!hasEntity)
            {
                entity = GetComponentInParent<Entity>();
                hasEntity = true;
            }

            return entity;
        }
    }

    public virtual void OnHelperUsed()
    {
    }

    public virtual void OnHelperRecycled()
    {
    }

    public virtual void ApplyEntityExtraStates(EntityDataExtraStates entityDataExtraStates)
    {
    }
}
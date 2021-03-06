using System.Collections.Generic;
using UnityEngine;

public abstract class EntityMonoHelper : MonoBehaviour
{
    private Entity entity;

    internal Entity Entity
    {
        get
        {
            if (entity == null)
            {
                entity = GetComponentInParent<Entity>();
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

    #region Utils

    private IEnumerable<string> GetAllBoxTypeNames => ConfigManager.GetAllBoxTypeNames();
    private IEnumerable<string> GetAllFXTypeNames => ConfigManager.GetAllFXTypeNames();
    private IEnumerable<string> GetAllBoxIconTypeNames => ConfigManager.GetAllBoxIconTypeNames();

    #endregion
}
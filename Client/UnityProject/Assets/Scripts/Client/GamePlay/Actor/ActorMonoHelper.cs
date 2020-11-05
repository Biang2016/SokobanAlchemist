using System.Collections.Generic;
using UnityEngine;

public class ActorMonoHelper : MonoBehaviour
{
    private Actor actor;

    internal Actor Actor
    {
        get
        {
            if (actor == null) actor = GetComponentInParent<Actor>();
            return actor;
        }
    }

    public virtual void OnUsed()
    {
    }

    public virtual void OnRecycled()
    {
    }

    #region Utils

    private IEnumerable<string> GetAllBoxTypeNames => ConfigManager.GetAllBoxTypeNames();
    private IEnumerable<string> GetAllFXTypeNames => ConfigManager.GetAllFXTypeNames();

    #endregion
}
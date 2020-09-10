using UnityEngine;

public class ActorHelper : MonoBehaviour
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

    public virtual void OnRecycled()
    {
    }
}
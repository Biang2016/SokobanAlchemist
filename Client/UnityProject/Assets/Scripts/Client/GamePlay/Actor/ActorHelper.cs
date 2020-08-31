using UnityEngine;

public class ActorHelper : MonoBehaviour
{
    internal Actor Actor;

    protected virtual void Awake()
    {
        Actor = GetComponentInParent<Actor>();
    }
}
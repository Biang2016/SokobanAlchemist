using System.Collections;
using UnityEngine;

public class AnimatorRootMotion : MonoBehaviour
{
    public Actor Actor;
    public Animator Anim;

    void OnAnimatorMove()
    {
        Actor.transform.position += Anim.deltaPosition;
    }
}
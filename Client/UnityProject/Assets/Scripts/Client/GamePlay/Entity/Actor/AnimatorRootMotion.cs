using System.Collections;
using UnityEngine;

public class AnimatorRootMotion : MonoBehaviour
{
    public Actor Actor;
    public Animator Anim;

    public float DeltaPositionFactor = 1.0f;

    void Start()
    {
        Anim.applyRootMotion = false;
    }

    void OnAnimatorMove()
    {
        Actor.transform.position += Anim.deltaPosition * DeltaPositionFactor;
    }
}
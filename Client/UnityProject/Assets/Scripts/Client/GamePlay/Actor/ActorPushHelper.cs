using UnityEngine;
using DG.Tweening;

public class ActorPushHelper : ActorHelper
{
    public Collider Collider;
    public GameObject Model;
    public Animator AnimCollider;
    public Animator AnimModel;
    public ActorPushHelperTrigger ActorPushHelperTrigger;

    public void PushTriggerOut()
    {
        AnimCollider.ResetTrigger("Reset");
        AnimCollider.SetTrigger("MoveOut");
    }

    public void PushTriggerReset()
    {
        AnimCollider.SetTrigger("Reset");
        AnimCollider.ResetTrigger("MoveOut");
    }
}
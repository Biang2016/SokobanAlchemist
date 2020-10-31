using UnityEngine;
using DG.Tweening;

public class ActorPushHelper : ActorMonoHelper
{
    public Collider Collider;
    public Animator AnimCollider;
    public Animator AnimModel;
    public ActorPushHelperTrigger ActorPushHelperTrigger;

    public override void OnUsed()
    {
        base.OnUsed();
        ActorPushHelperTrigger.OnUsed();
    }

    public override void OnRecycled()
    {
        PushTriggerReset();
        ActorPushHelperTrigger.OnRecycled();
        base.OnRecycled();
    }

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
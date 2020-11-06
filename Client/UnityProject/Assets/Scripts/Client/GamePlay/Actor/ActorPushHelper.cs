using UnityEngine;
using DG.Tweening;

public class ActorPushHelper : ActorMonoHelper
{
    public Collider Collider;
    public Animator AnimCollider;
    public ActorPushHelperTrigger ActorPushHelperTrigger;

    public override void OnUsed()
    {
        base.OnUsed();
        ActorPushHelperTrigger.OnUsed();
    }

    public override void OnRecycled()
    {
        TriggerOut = false;
        ActorPushHelperTrigger.OnRecycled();
        base.OnRecycled();
    }

    private bool triggerOut = false;

    public bool TriggerOut
    {
        get { return triggerOut; }
        set
        {
            if (triggerOut != value)
            {
                triggerOut = value;
                if (value)
                {
                    AnimCollider.ResetTrigger("Reset");
                    AnimCollider.SetTrigger("MoveOut");
                }
                else
                {
                    AnimCollider.SetTrigger("Reset");
                    AnimCollider.ResetTrigger("MoveOut");
                }
            }
        }
    }
}
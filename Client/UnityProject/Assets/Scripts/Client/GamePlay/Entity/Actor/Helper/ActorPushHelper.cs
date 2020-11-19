using UnityEngine;

public class ActorPushHelper : ActorMonoHelper
{
    public Collider Collider;
    public Animator AnimCollider;
    public ActorPushHelperTrigger ActorPushHelperTrigger;

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
        ActorPushHelperTrigger.OnUsed();
    }

    public override void OnHelperRecycled()
    {
        if (!Actor.ActorForbidPushBox) TriggerOut = false;
        ActorPushHelperTrigger.OnRecycled();
        base.OnHelperRecycled();
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
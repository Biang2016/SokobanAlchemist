using UnityEngine;

public class ActorPushHelper : ActorMonoHelper
{
    public Animator AnimCollider;
    public ActorPushHelperTrigger_Enter ActorPushHelperTrigger_Enter;
    public ActorPushHelperTrigger_Exit ActorPushHelperTrigger_Exit;

    public Box curPushingBox = null; // 一次只推一个箱子

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
        ActorPushHelperTrigger_Enter.OnUsed();
        ActorPushHelperTrigger_Exit.OnUsed();
    }

    public override void OnHelperRecycled()
    {
        TriggerOut = false;
        ActorPushHelperTrigger_Enter.OnRecycled();
        ActorPushHelperTrigger_Exit.OnRecycled();
        curPushingBox = null;
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
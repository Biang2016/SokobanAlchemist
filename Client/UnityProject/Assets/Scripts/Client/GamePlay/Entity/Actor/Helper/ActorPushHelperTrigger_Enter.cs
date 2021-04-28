using UnityEngine;

public class ActorPushHelperTrigger_Enter : MonoBehaviour
{
    public ActorPushHelper ActorPushHelper;
    public BoxCollider BoxCollider;

    private bool isRecycled = false;

    internal void OnUsed()
    {
        isRecycled = false;
        BoxCollider.enabled = true;
    }

    internal void OnRecycled()
    {
        isRecycled = true;
        BoxCollider.enabled = false;
    }

    void OnTriggerStay(Collider collider)
    {
        if (isRecycled) return;
        if (collider.gameObject.layer == LayerManager.Instance.Layer_BoxIndicator)
        {
            Box box = collider.gameObject.GetComponentInParent<Box>();
            if (box && box.Pushable && ActorPushHelper.Actor.ActorBoxInteractHelper.CanInteract(InteractSkillType.Push, box.EntityTypeIndex))
            {
                ActorPushHelper.curPushingBox = box;
                box.Push(ActorPushHelper.Actor.CurMoveAttempt, ActorPushHelper.Actor);
                ActorPushHelper.Actor.ActorArtHelper.SetIsPushing(true);
            }
        }
    }
}
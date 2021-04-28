using UnityEngine;

public class ActorPushHelperTrigger_Exit : MonoBehaviour
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

    void OnTriggerExit(Collider collider)
    {
        if (isRecycled) return;
        if (collider.gameObject.layer == LayerManager.Instance.Layer_BoxIndicator)
        {
            Box box = collider.gameObject.GetComponentInParent<Box>();
            if (box && box.Pushable && ActorPushHelper.Actor.ActorBoxInteractHelper.CanInteract(InteractSkillType.Push, box.EntityTypeIndex))
            {
                ActorPushHelper.Actor.ActorArtHelper.SetIsPushing(false);
                if (ActorPushHelper.curPushingBox != null && ActorPushHelper.curPushingBox == box)
                {
                    box.PushCancel(ActorPushHelper.Actor);
                    ActorPushHelper.curPushingBox = null;
                }
            }
        }
    }
}
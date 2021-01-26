using UnityEngine;

public class ActorPushHelperTrigger : MonoBehaviour
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
        curPushingBox = null;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (isRecycled) return;
    }

    public Box curPushingBox = null; // 一次只推一个箱子

    void OnTriggerStay(Collider collider)
    {
        if (isRecycled) return;
        if (collider.gameObject.layer == LayerManager.Instance.Layer_BoxIndicator)
        {
            Box box = collider.gameObject.GetComponentInParent<Box>();
            if (box && box.Pushable && ActorPushHelper.Actor.ActorBoxInteractHelper.CanInteract(InteractSkillType.Push, box.BoxTypeIndex))
            {
                curPushingBox = box;
                box.Push(ActorPushHelper.Actor.CurMoveAttempt, ActorPushHelper.Actor);
                //Debug.Log("BoxPush " + Mathf.RoundToInt(Time.fixedTime / Time.fixedDeltaTime));
            }
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (isRecycled) return;
        if (collider.gameObject.layer == LayerManager.Instance.Layer_BoxIndicator)
        {
            Box box = collider.gameObject.GetComponentInParent<Box>();
            if (box && box.Pushable && ActorPushHelper.Actor.ActorBoxInteractHelper.CanInteract(InteractSkillType.Push, box.BoxTypeIndex))
            {
                //if (curPushingBox != null && curPushingBox == box)
                //{
                //    box.PushCanceled();
                //    curPushingBox = null;
                //    //Debug.Log("BoxExit " + Mathf.RoundToInt(Time.fixedTime / Time.fixedDeltaTime));
                //}
            }
        }
    }

    void FixedUpdate()
    {
        if (isRecycled) return;
    }
}
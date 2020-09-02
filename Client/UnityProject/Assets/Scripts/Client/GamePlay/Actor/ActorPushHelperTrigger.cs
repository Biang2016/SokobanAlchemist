using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;

public class ActorPushHelperTrigger : MonoBehaviour
{
    public ActorPushHelper ActorPushHelper;

    [ReadOnly]
    public HashSet<Box> PushingBoxList = new HashSet<Box>();

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer == LayerManager.Instance.Layer_Box)
        {
            Box box = collider.gameObject.GetComponentInParent<Box>();
            if (box && box.Pushable())
            {
                PushingBoxList.Add(box);
                ActorPushHelper.Actor.PushState = PushingBoxList.Count > 0 ? Actor.PushStates.Pushing : Actor.PushStates.None;
            }
        }
    }

    public Box curPushingBox = null; // 一次只推一个箱子

    void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.layer == LayerManager.Instance.Layer_Box)
        {
            if (curPushingBox) return;
            Box box = collider.gameObject.GetComponentInParent<Box>();
            if (box && box.Pushable())
            {
                curPushingBox = box;
                ActorPushHelper.AnimModel.ResetTrigger("Reset");
                ActorPushHelper.AnimModel.SetTrigger("MoveOut");
                box.Push(ActorPushHelper.Actor.CurMoveAttempt);
            }
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.layer == LayerManager.Instance.Layer_Box)
        {
            Box box = collider.gameObject.GetComponentInParent<Box>();
            if (box && box.Pushable())
            {
                ActorPushHelper.AnimModel.SetTrigger("Reset");
                ActorPushHelper.AnimModel.ResetTrigger("MoveOut");
                box.PushCanceled();
                PushingBoxList.Remove(box);
                ActorPushHelper.Actor.PushState = PushingBoxList.Count > 0 ? Actor.PushStates.Pushing : Actor.PushStates.None;
            }
        }
    }

    void FixedUpdate()
    {
        curPushingBox = null;
    }
}
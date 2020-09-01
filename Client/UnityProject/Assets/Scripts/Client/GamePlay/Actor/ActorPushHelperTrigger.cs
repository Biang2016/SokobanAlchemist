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
            }
        }
    }

    private Box curPushingBox = null;

    void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.layer == LayerManager.Instance.Layer_Box)
        {
            if (curPushingBox) return;
            Box box = collider.gameObject.GetComponentInParent<Box>();
            if (box && box.Pushable())
            {
                curPushingBox = box;
                ActorPushHelper.Model.transform.DOPause();
                ActorPushHelper.Model.transform.DOLocalMove(ActorPushHelper.DefaultModelPos + Vector3.forward * 0.5f, 0.2f);
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
                ActorPushHelper.Model.transform.DOLocalMove(ActorPushHelper.DefaultModelPos, 0.2f);
                box.PushCanceled();
                PushingBoxList.Remove(box);
            }
        }
    }

    void FixedUpdate()
    {
        curPushingBox = null;
    }
}
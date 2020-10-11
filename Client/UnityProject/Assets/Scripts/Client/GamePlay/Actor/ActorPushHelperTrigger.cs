using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;

public class ActorPushHelperTrigger : MonoBehaviour
{
    public ActorPushHelper ActorPushHelper;
    public BoxCollider BoxCollider;

    [ReadOnly]
    public HashSet<Box> PushingBoxList = new HashSet<Box>();

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
        PushingBoxList.Clear();
        curPushingBox = null;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (isRecycled) return;
        if (collider.gameObject.layer == LayerManager.Instance.Layer_BoxIndicator)
        {
            Box box = collider.gameObject.GetComponentInParent<Box>();
            if (box && box.Pushable && ActorPushHelper.Actor.ActorSkillHelper.CanInteract(InteractSkillType.Push, box.BoxTypeIndex))
            {
                PushingBoxList.Add(box);
            }
        }
    }

    public Box curPushingBox = null; // 一次只推一个箱子

    void OnTriggerStay(Collider collider)
    {
        if (isRecycled) return;
        if (collider.gameObject.layer == LayerManager.Instance.Layer_BoxIndicator)
        {
            if (curPushingBox) return;
            Box box = collider.gameObject.GetComponentInParent<Box>();
            if (box && box.Pushable && ActorPushHelper.Actor.ActorSkillHelper.CanInteract(InteractSkillType.Push, box.BoxTypeIndex))
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
        if (isRecycled) return;
        if (collider.gameObject.layer == LayerManager.Instance.Layer_BoxIndicator)
        {
            Box box = collider.gameObject.GetComponentInParent<Box>();
            if (box && box.Pushable && ActorPushHelper.Actor.ActorSkillHelper.CanInteract(InteractSkillType.Push, box.BoxTypeIndex))
            {
                ActorPushHelper.AnimModel.SetTrigger("Reset");
                ActorPushHelper.AnimModel.ResetTrigger("MoveOut");
                box.PushCanceled();
                PushingBoxList.Remove(box);
            }
        }
    }

    void FixedUpdate()
    {
        if (isRecycled) return;
        curPushingBox = null;
    }
}
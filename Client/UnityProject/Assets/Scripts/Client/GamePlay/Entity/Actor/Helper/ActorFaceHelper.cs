using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class ActorFaceHelper : ActorMonoHelper
{
    [ReadOnly]
    public HashSet<Box> FacingBoxList = new HashSet<Box>();

    public override void OnHelperRecycled()
    {
        FacingBoxList.Clear();
        base.OnHelperRecycled();
    }

    void FixedUpdate()
    {
        if (Actor.IsRecycled) return;
        Actor.PushState = Actor.PushStates.None;
        foreach (Box b in FacingBoxList)
        {
            if (b.State == Box.States.PushingCanceling || b.State == Box.States.BeingPushed)
            {
                Actor.PushState = Actor.PushStates.Pushing;
            }
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (Actor.IsRecycled) return;
        if (collider.gameObject.layer == LayerManager.Instance.Layer_BoxIndicator)
        {
            Box box = collider.gameObject.GetComponentInParent<Box>();
            if (box && box.Pushable && Actor.ActorSkillHelper.CanInteract(InteractSkillType.Push, box.BoxTypeIndex))
            {
                FacingBoxList.Add(box);
            }
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (Actor.IsRecycled) return;
        if (collider.gameObject.layer == LayerManager.Instance.Layer_BoxIndicator)
        {
            Box box = collider.gameObject.GetComponentInParent<Box>();
            if (box && box.Pushable && Actor.ActorSkillHelper.CanInteract(InteractSkillType.Push, box.BoxTypeIndex))
            {
                FacingBoxList.Remove(box);
            }
        }
    }
}
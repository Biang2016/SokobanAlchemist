using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class ActorFaceHelper : ActorHelper
{
    [ReadOnly]
    public HashSet<Box> FacingBoxList = new HashSet<Box>();

    void FixedUpdate()
    {
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
        if (collider.gameObject.layer == LayerManager.Instance.Layer_Box)
        {
            Box box = collider.gameObject.GetComponentInParent<Box>();
            if (box && box.Pushable())
            {
                FacingBoxList.Add(box);
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
                FacingBoxList.Remove(box);
            }
        }
    }
}
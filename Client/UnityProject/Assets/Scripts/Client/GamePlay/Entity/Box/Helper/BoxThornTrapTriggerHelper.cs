using UnityEngine;
using System.Collections.Generic;

public class BoxThornTrapTriggerHelper : BoxMonoHelper
{
    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
        ActorStayTimeDict.Clear();
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
    }

    public Dictionary<uint, float> ActorStayTimeDict = new Dictionary<uint, float>();

    public void OnTriggerEnter(Collider collider)
    {
        foreach (BoxFunctionBase bf in Box.BoxFunctions)
        {
            bf.OnBoxThornTrapTriggerEnter(collider);
        }
    }

    public void OnTriggerStay(Collider collider)
    {
        foreach (BoxFunctionBase bf in Box.BoxFunctions)
        {
            bf.OnBoxThornTrapTriggerStay(collider);
        }
    }

    public void OnTriggerExit(Collider collider)
    {
        foreach (BoxFunctionBase bf in Box.BoxFunctions)
        {
            bf.OnBoxThornTrapTriggerExit(collider);
        }
    }
}
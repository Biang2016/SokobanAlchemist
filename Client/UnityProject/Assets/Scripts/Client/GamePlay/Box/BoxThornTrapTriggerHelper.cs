using UnityEngine;
using System.Collections.Generic;

public class BoxThornTrapTriggerHelper : MonoBehaviour, IBoxHelper
{
    public Box Box;

    public void PoolRecycle()
    {
        ActorStayTimeDict.Clear();
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
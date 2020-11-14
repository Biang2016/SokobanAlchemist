using System.Collections.Generic;
using UnityEngine;

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
        foreach (BoxPassiveSkill bf in Box.BoxPassiveSkills)
        {
            bf.OnBoxThornTrapTriggerEnter(collider);
        }
    }

    public void OnTriggerStay(Collider collider)
    {
        foreach (BoxPassiveSkill bf in Box.BoxPassiveSkills)
        {
            bf.OnBoxThornTrapTriggerStay(collider);
        }
    }

    public void OnTriggerExit(Collider collider)
    {
        foreach (BoxPassiveSkill bf in Box.BoxPassiveSkills)
        {
            bf.OnBoxThornTrapTriggerExit(collider);
        }
    }
}
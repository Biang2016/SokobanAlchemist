using UnityEngine;
using System.Collections;

public class BoxTriggerZone : MonoBehaviour
{
    [HideInInspector]
    public BoxTriggerZoneHelper BoxTriggerZoneHelper;

    public void OnTriggerEnter(Collider c)
    {
        foreach (BoxPassiveSkill bf in BoxTriggerZoneHelper.Box.BoxPassiveSkills)
        {
            bf.OnBoxTriggerZoneEnter(c);
        }
    }

    public void OnTriggerStay(Collider c)
    {
        foreach (BoxPassiveSkill bf in BoxTriggerZoneHelper.Box.BoxPassiveSkills)
        {
            bf.OnBoxTriggerZoneStay(c);
        }
    }

    public void OnTriggerExit(Collider c)
    {
        foreach (BoxPassiveSkill bf in BoxTriggerZoneHelper.Box.BoxPassiveSkills)
        {
            bf.OnBoxTriggerZoneExit(c);
        }
    }
}
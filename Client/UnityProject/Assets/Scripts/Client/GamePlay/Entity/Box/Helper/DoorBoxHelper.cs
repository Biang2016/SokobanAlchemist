using System;
using UnityEngine;

public class DoorBoxHelper : BoxMonoHelper
{
    public Animator DoorAnim;

    private bool open;

    public bool Open
    {
        get { return open; }
        set
        {
            if (open != value)
            {
                open = value;
                DoorAnim.SetTrigger(open ? "Open" : "Close");
            }
        }
    }

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
        DoorAnim.SetTrigger("Close");
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
    }
}
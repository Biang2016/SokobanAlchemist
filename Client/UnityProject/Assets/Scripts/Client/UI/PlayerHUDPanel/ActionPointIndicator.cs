using BiangLibrary.ObjectPool;
using UnityEngine;
using UnityEngine.UI;

public class ActionPointIndicator : PoolObject
{
    public Animator Anim;

    public Image Image;
    public Color EnableColor;
    public Color DisableColor;

    private bool available = false;

    public bool Available
    {
        get { return available; }
        set
        {
            if (available != value)
            {
                available = value;
                if (available)
                {
                    JumpToEnable();
                }
                else
                {
                    JumpToDisable();
                }
            }
        }
    }

    public override void OnRecycled()
    {
        base.OnRecycled();
        Available = true;
    }

    public override void OnUsed()
    {
        base.OnUsed();
        Available = true;
    }

    public void Jump()
    {
        Anim.SetTrigger("Jump");
    }

    public void JumpRed()
    {
        if (Available)
        {
            Anim.SetTrigger("JumpRed_Enable");
        }
        else
        {
            Anim.SetTrigger("JumpRed_Disable");
        }
    }

    private void JumpToEnable()
    {
        Anim.SetTrigger("JumpToEnable");
    }

    private void JumpToDisable()
    {
        Anim.SetTrigger("JumpToDisable");
    }
}
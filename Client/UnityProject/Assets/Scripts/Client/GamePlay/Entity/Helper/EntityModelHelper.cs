using BiangLibrary.GameDataFormat.Grid;
using UnityEngine;

public class EntityModelHelper : EntityMonoHelper
{
    [SerializeField]
    internal Animator ModelAnim;

    internal bool StretchAnim = false;

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
        if (ModelAnim != null)
        {
            if (Entity is Actor)
            {
                StretchAnim = false;
                ModelAnim.enabled = false;
            }

            if (Entity is Box box)
            {
                StretchAnim = box.KickOrThrowable;
                ModelAnim.enabled = StretchAnim;
            }
        }
        else
        {
            StretchAnim = false;
        }
    }

    private bool x_Stretch;

    private bool X_Stretch
    {
        get { return x_Stretch; }
        set
        {
            if (x_Stretch != value)
            {
                x_Stretch = value;
                ModelAnim.SetBool("X_Stretch", x_Stretch);
                if (value) ModelAnim.SetTrigger("X_Stretch_Start");
            }
        }
    }

    private bool z_Stretch;

    private bool Z_Stretch
    {
        get { return z_Stretch; }
        set
        {
            if (z_Stretch != value)
            {
                z_Stretch = value;
                ModelAnim.SetBool("Z_Stretch", z_Stretch);
                if (value) ModelAnim.SetTrigger("Z_Stretch_Start");
            }
        }
    }

    public void SetVelocity(float x, float z)
    {
        if (!StretchAnim) return;
        if (Entity.EntityOrientation == GridPosR.Orientation.Left || Entity.EntityOrientation == GridPosR.Orientation.Right)
        {
            X_Stretch = z > 0.5f || z < -0.5f;
            Z_Stretch = x > 0.5f || x < -0.5f;
        }
        else
        {
            X_Stretch = x > 0.5f || x < -0.5f;
            Z_Stretch = z > 0.5f || z < -0.5f;
        }
    }

    public void EndStretch()
    {
        if (!StretchAnim) return;
        X_Stretch = false;
        Z_Stretch = false;
    }
}
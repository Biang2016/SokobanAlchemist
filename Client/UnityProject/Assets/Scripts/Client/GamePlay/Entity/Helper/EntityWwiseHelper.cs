using Sirenix.OdinInspector;

public class EntityWwiseHelper : EntityMonoHelper
{
    [BoxGroup("ForBox")]
    public AK.Wwise.Event OnBeingKicked;

    [BoxGroup("ForBox")]
    public AK.Wwise.Event OnBeingPushed;

    [BoxGroup("ForBox")]
    public AK.Wwise.Event OnSliding;

    [BoxGroup("ForBox")]
    public AK.Wwise.Event OnSlideStop;

    [BoxGroup("ForBox")]
    public AK.Wwise.Event OnBeingLift;

    [BoxGroup("ForBox")]
    public AK.Wwise.Event OnBeingDropped;

    [BoxGroup("ForBox")]
    public AK.Wwise.Event OnBeingLit;

    [BoxGroup("ForBox")]
    public AK.Wwise.Event OnBurning;

    [BoxGroup("ForBox")]
    public AK.Wwise.Event OnDestroyed_Common;

    [BoxGroup("ForBox")]
    public AK.Wwise.Event OnDestroyed_ByFire;

    [BoxGroup("ForBox")]
    public AK.Wwise.Event OnDestroyed_ByExplode;

    [BoxGroup("ForBox")]
    public AK.Wwise.Event OnDestroyed_ByCollide;

    [BoxGroup("ForActor")]
    public AK.Wwise.Event OnKick;

    [BoxGroup("ForActor")]
    public AK.Wwise.Event OnVault;

    [BoxGroup("ForActor")]
    public AK.Wwise.Event OnDash;

    [BoxGroup("ForActor")]
    public AK.Wwise.Event OnDie;

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
    }
}
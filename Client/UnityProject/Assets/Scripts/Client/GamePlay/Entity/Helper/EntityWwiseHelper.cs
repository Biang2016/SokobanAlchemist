using AK.Wwise;
using Sirenix.OdinInspector;

public class EntityWwiseHelper : EntityMonoHelper
{
    [BoxGroup("ForBox")]
    public Event OnBeingKicked;

    [BoxGroup("ForBox")]
    public Event OnCollideActively;

    [BoxGroup("Actor")]
    public Event OnCollidePassively;

    [BoxGroup("ForBox")]
    public Event OnBeingPushed;

    [BoxGroup("ForBox")]
    [ReadOnly]
    public Event OnSliding;

    [BoxGroup("ForBox")]
    public Event OnSlideStop;

    [BoxGroup("ForBox")]
    public Event OnBeingLift;

    [BoxGroup("ForBox")]
    public Event OnBeingMerged;

    [BoxGroup("ForBox")]
    public Event OnBeingDropped;

    [BoxGroup("ForBox")]
    public Event OnThrownUp;

    [BoxGroup("Common")]
    public Event OnBeingLit;

    [BoxGroup("Common")]
    [ReadOnly]
    public Event OnBurning;

    [BoxGroup("Common")]
    public Event OnBurningEnd;

    [BoxGroup("Common")]
    public Event OnBeingFrozen;

    [BoxGroup("Common")]
    public Event OnFrozenEnd;

    [BoxGroup("Common")]
    public Event OnDestroyed_Common;

    [BoxGroup("Common")]
    public Event OnDestroyed_ByFiringDamage;

    [BoxGroup("Common")]
    public Event OnDestroyed_ByFrozenDamage;

    [BoxGroup("Common")]
    public Event OnDestroyed_ByExplodeDamage;

    [BoxGroup("Common")]
    public Event OnDestroyed_ByCollideDamage;

    [BoxGroup("ForActor")]
    public Event OnBeingDamaged;

    [BoxGroup("ForActor")]
    public Event OnBeingHealed;

    [BoxGroup("ForActor")]
    public Event[] OnSkillPreparing = new Event[9];

    [BoxGroup("ForActor")]
    public Event[] OnSkillCast = new Event[9];

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
    }
}
using AK.Wwise;
using Sirenix.OdinInspector;

public class EntityWwiseHelper : EntityMonoHelper
{
    [BoxGroup("ForBox")]
    public Event OnBeingKicked;

    [BoxGroup("ForBox")]
    public Event OnCollideActively;

    [BoxGroup("ForBox")]
    public Event OnBeingPushed;

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
    public Event OnCollidePassively;

    [BoxGroup("ForActor")]
    public Event OnGainActionPoint;

    [BoxGroup("ForActor")]
    public Event OnGainMaxActionPoint;

    [BoxGroup("ForActor")]
    public Event OnGainMaxHealth;

    [BoxGroup("ForActor_Player")]
    public Event OnGainGold;

    [BoxGroup("ForActor_Player")]
    public Event OnGainFireElement;

    [BoxGroup("ForActor_Player")]
    public Event OnGainIceElement;

    [BoxGroup("ForActor_Player")]
    public Event OnGainLightningElement;

    [BoxGroup("ForActor_Player")]
    public Event OnLowHealthWarning;

    [BoxGroup("ForActor_Player")]
    public Event OnActionPointNotEnough;

    [BoxGroup("ForActor_Player")]
    public Event OnGoldNotEnough;

    [BoxGroup("ForActor_Player")]
    public Event OnElementsNotEnough;

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
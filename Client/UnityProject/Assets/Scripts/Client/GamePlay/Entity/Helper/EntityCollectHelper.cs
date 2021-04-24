using System.Collections;
using DG.Tweening;
using UnityEngine;

public class EntityCollectHelper : EntityMonoHelper
{
    [SerializeField]
    private SphereCollider DetectTrigger;

    public bool EnableDetector
    {
        get { return DetectTrigger.enabled; }
        set { DetectTrigger.enabled = value; }
    }

    private float detectRadius = 0f;

    public float DetectRadius
    {
        get { return detectRadius; }
        set
        {
            if (detectRadius != value)
            {
                detectRadius = value;
                DetectTrigger.radius = value;
            }
        }
    }

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
        EnableDetector = false;
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
        EnableDetector = true;
    }

    public void OnDie()
    {
        EnableDetector = false;
    }

    public void OnReborn()
    {
        EnableDetector = true;
    }

    public void Initialize()
    {
        DetectRadius = Entity.EntityStatPropSet.CollectDetectRadius.GetModifiedValue;
    }

    private void OnTriggerEnter(Collider trigger)
    {
        CollectableItem ci = trigger.gameObject.GetComponentInParent<CollectableItem>();
        if (ci != null)
        {
            ci.SetChasingTarget(Entity.transform, () =>
            {
                EntitySkillAction action = ci.EntitySkillAction_OnCollect?.Clone();
                if (action != null && action is EntitySkillAction.IEntityAction entityAction)
                {
                    entityAction.ExecuteOnEntity(Entity);
                }

                ci.PoolRecycle();
            });
        }
    }
}
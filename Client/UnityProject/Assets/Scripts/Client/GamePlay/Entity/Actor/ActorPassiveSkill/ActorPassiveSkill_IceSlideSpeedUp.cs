using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class ActorPassiveSkill_IceSlideSpeedUp : ActorPassiveSkill
{
    protected override string Description => "角色在冰面时施加Buff";

    [LabelText("施加Buff")]
    public ActorBuff RawActorBuff;

    [LabelText("Buff实例")]
    [NonSerialized]
    [HideInEditorMode]
    [ShowInInspector]
    private ActorBuff ActorBuff; // 实际施加的buff，取消施加时置空

    public override void OnTick(float tickDeltaTime)
    {
        base.OnTick(tickDeltaTime);
        Ray ray = new Ray(Actor.transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 0.7f, LayerManager.Instance.LayerMask_BoxIndicator))
        {
            Box box = hit.collider.gameObject.GetComponentInParent<Box>();
            if (box && box.BoxStatPropSet.IsFrozen)
            {
                if (ActorBuff == null)
                {
                    ActorBuff = (ActorBuff) RawActorBuff.Clone();
                    Actor.ActorBuffHelper.AddBuff(ActorBuff);
                }
            }
            else
            {
                if (ActorBuff != null)
                {
                    Actor.ActorBuffHelper.RemoveBuff(ActorBuff);
                    ActorBuff = null;
                }
            }
        }
        else
        {
            if (ActorBuff != null)
            {
                Actor.ActorBuffHelper.RemoveBuff(ActorBuff);
                ActorBuff = null;
            }
        }
    }

    protected override void ChildClone(ActorPassiveSkill newPS)
    {
        base.ChildClone(newPS);
        ActorPassiveSkill_IceSlideSpeedUp ps = ((ActorPassiveSkill_IceSlideSpeedUp) newPS);
        ps.RawActorBuff = (ActorBuff) RawActorBuff.Clone();
    }

    public override void CopyDataFrom(ActorPassiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        ActorPassiveSkill_IceSlideSpeedUp ps = ((ActorPassiveSkill_IceSlideSpeedUp) srcData);
        RawActorBuff = (ActorBuff) ps.RawActorBuff.Clone();
    }
}
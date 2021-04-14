using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class ActorPassiveSkill_IceSlideSpeedUp : ActorPassiveSkill
{
    protected override string Description => "角色在冰面时施加Buff";

    [SerializeReference]
    [LabelText("施加Buff")]
    public EntityBuff RawEntityBuff;

    [LabelText("Buff实例")]
    [NonSerialized]
    [HideInEditorMode]
    [ShowInInspector]
    private EntityBuff EntityBuff; // 实际施加的buff，取消施加时置空

    public override void OnTick(float tickDeltaTime)
    {
        base.OnTick(tickDeltaTime);
        Ray ray = new Ray(Actor.transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 0.7f, LayerManager.Instance.LayerMask_BoxIndicator))
        {
            Box box = hit.collider.gameObject.GetComponentInParent<Box>();
            if (box && box.EntityStatPropSet.IsFrozen)
            {
                if (EntityBuff == null)
                {
                    EntityBuff = RawEntityBuff.Clone();
                    Actor.EntityBuffHelper.AddBuff(EntityBuff);
                }
            }
            else
            {
                if (EntityBuff != null)
                {
                    Actor.EntityBuffHelper.RemoveBuff(EntityBuff);
                    EntityBuff = null;
                }
            }
        }
        else
        {
            if (EntityBuff != null)
            {
                Actor.EntityBuffHelper.RemoveBuff(EntityBuff);
                EntityBuff = null;
            }
        }
    }

    protected override void ChildClone(EntitySkill cloneData)
    {
        base.ChildClone(cloneData);
        ActorPassiveSkill_IceSlideSpeedUp ps = ((ActorPassiveSkill_IceSlideSpeedUp) cloneData);
        ps.RawEntityBuff = RawEntityBuff.Clone();
    }

    public override void CopyDataFrom(EntitySkill srcData)
    {
        base.CopyDataFrom(srcData);
        ActorPassiveSkill_IceSlideSpeedUp ps = ((ActorPassiveSkill_IceSlideSpeedUp) srcData);
        RawEntityBuff.CopyDataFrom(ps.RawEntityBuff);
    }
}
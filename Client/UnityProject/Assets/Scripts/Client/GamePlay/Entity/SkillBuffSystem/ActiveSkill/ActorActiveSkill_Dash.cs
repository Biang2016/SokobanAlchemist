using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class ActorActiveSkill_Dash : EntityActiveSkill
{
    protected override string Description => "冲刺";

    [LabelText("冲刺最大距离")]
    public int DashMaxDistance = 4;

    protected override bool ValidateSkillTrigger()
    {
        if (Entity is Actor actor)
        {
            if (actor.ThrowState != Actor.ThrowStates.None) return false;
            if (actor.CannotAct && !actor.IsFrozen) return false;

            Ray ray = new Ray(actor.transform.position - actor.transform.forward * 0.49f, actor.transform.forward);
            //Debug.DrawRay(ray.origin, ray.direction, Color.red, 0.3f);
            if (Physics.Raycast(ray, out RaycastHit hit, 1.49f, LayerManager.Instance.LayerMask_BoxIndicator, QueryTriggerInteraction.Collide))
            {
                Box box = hit.collider.gameObject.GetComponentInParent<Box>();
                if (box && !box.Passable && actor.ActorBoxInteractHelper.CanInteract(InteractSkillType.Push, box.EntityTypeIndex))
                {
                    return false;
                }
                else
                {
                    if (!base.ValidateSkillTrigger()) return false;
                    return true;
                }
            }
            else
            {
                if (!base.ValidateSkillTrigger()) return false;
                return true;
            }
        }

        return base.ValidateSkillTrigger();
    }

    protected override IEnumerator Cast(float castDuration)
    {
        if (Entity is Actor actor)
        {
            if (actor.IsFrozen)
            {
                actor.EntityStatPropSet.FrozenValue.SetValue(actor.EntityStatPropSet.FrozenValue.Value - 200, "DashOrVault");
            }
            else
            {
                actor.temp_DashMaxDistance = DashMaxDistance;
                actor.ActorArtHelper.Dash();
            }
        }

        yield return base.Cast(castDuration);
    }

    protected override void ChildClone(EntitySkill cloneData)
    {
        base.ChildClone(cloneData);
        ActorActiveSkill_Dash newEAS = (ActorActiveSkill_Dash) cloneData;
        newEAS.DashMaxDistance = DashMaxDistance;
    }

    public override void CopyDataFrom(EntitySkill srcData)
    {
        base.CopyDataFrom(srcData);
        ActorActiveSkill_Dash srcEAS = (ActorActiveSkill_Dash) srcData;
        DashMaxDistance = srcEAS.DashMaxDistance;
    }
}
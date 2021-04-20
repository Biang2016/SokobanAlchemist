using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class ActorActiveSkill_Vault : EntityActiveSkill
{
    protected override string Description => "翻越";

    protected override bool ValidateSkillTrigger_Subject(TargetEntityType targetEntityType)
    {
        if (!base.ValidateSkillTrigger_Subject(targetEntityType)) return false;
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
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        return false;
    }

    protected override IEnumerator Cast(TargetEntityType targetEntityType, float castDuration)
    {
        if (Entity is Actor actor)
        {
            if (actor.IsFrozen)
            {
                actor.EntityStatPropSet.FrozenValue.SetValue(actor.EntityStatPropSet.FrozenValue.Value - 200, "DashOrVault");
            }
            else
            {
                actor.ActorArtHelper.Vault();
            }
        }

        yield return base.Cast(targetEntityType, castDuration);
    }

    protected override void ChildClone(EntitySkill cloneData)
    {
        base.ChildClone(cloneData);
    }

    public override void CopyDataFrom(EntitySkill srcData)
    {
        base.CopyDataFrom(srcData);
    }
}
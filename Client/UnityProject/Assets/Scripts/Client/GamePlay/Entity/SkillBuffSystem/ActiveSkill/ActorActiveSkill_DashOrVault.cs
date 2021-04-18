using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class ActorActiveSkill_DashOrVault : EntityActiveSkill
{
    protected override string Description => "冲刺或翻越";

    [LabelText("冲刺最大距离")]
    public int DashMaxDistance = 4;

    private string actionType = "Vault";

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
                    actionType = "Vault";
                    if (!base.ValidateSkillTrigger()) return false;
                    return true;
                }
                else
                {
                    actionType = "Dash";
                    if (!base.ValidateSkillTrigger()) return false;
                    return true;
                }
            }
            else
            {
                actionType = "Dash";
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
                if (actionType == "Vault")
                {
                    actor.ActorArtHelper.Vault();
                }
                else if (actionType == "Dash")
                {
                    actor.temp_DashMaxDistance = DashMaxDistance;
                    actor.ActorArtHelper.Dash();
                }
            }
        }

        yield return base.Cast(castDuration);
    }

    protected override void ChildClone(EntitySkill cloneData)
    {
        base.ChildClone(cloneData);
        ActorActiveSkill_DashOrVault newEAS = (ActorActiveSkill_DashOrVault) cloneData;
        newEAS.DashMaxDistance = DashMaxDistance;
    }

    public override void CopyDataFrom(EntitySkill srcData)
    {
        base.CopyDataFrom(srcData);
        ActorActiveSkill_DashOrVault srcEAS = (ActorActiveSkill_DashOrVault) srcData;
        DashMaxDistance = srcEAS.DashMaxDistance;
    }
}
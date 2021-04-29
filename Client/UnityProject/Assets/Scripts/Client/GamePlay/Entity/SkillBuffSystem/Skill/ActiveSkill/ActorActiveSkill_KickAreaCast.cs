using System;
using System.Collections;
using BiangLibrary.GamePlay.UI;
using UnityEngine;

[Serializable]
public class ActorActiveSkill_KickAreaCast : EntityActiveSkill_AreaCast
{
    protected override string Description => "踢AreaCast";

    protected override bool ValidateSkillTrigger_Subject(TargetEntityType targetEntityType)
    {
        if (!base.ValidateSkillTrigger_Subject(targetEntityType)) return false;
        if (Entity is Actor actor)
        {
            if (!actor.CannotAct)
            {
                Ray ray = new Ray(actor.transform.position - actor.transform.forward * 0.49f, actor.transform.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, 1.49f, LayerManager.Instance.LayerMask_BoxIndicator, QueryTriggerInteraction.Collide))
                {
                    Box box = hit.collider.gameObject.GetComponentInParent<Box>();
                    if (box && box.Kickable && actor.ActorBoxInteractHelper.CanInteract(InteractSkillType.Kick, box.EntityTypeIndex))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    protected override IEnumerator Cast(TargetEntityType targetEntityType, float castDuration)
    {
        if (Entity is Actor actor)
        {
            actor.ActorArtHelper.Kick();
            //yield return new WaitForSeconds(0.066f);
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
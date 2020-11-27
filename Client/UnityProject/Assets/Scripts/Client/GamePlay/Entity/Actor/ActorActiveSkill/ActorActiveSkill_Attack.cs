using System;
using System.Collections;
using System.Collections.Generic;
using BiangStudio.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class ActorActiveSkill_Attack : ActorActiveSkill_AreaCast
{
    protected override string Description => "普通攻击";

    protected override IEnumerator Cast(float castDuration)
    {
        int targetCount = 0;
        HashSet<uint> actorGUIDSet = new HashSet<uint>();
        HashSet<uint> boxGUIDSet = new HashSet<uint>();
        foreach (GridPos3D gp in RealSkillEffectGPs)
        {
            Collider[] colliders_PlayerLayer = Physics.OverlapSphere(gp.ToVector3(), 0.3f, LayerManager.Instance.GetTargetActorLayerMask(Actor.Camp, TargetCamp));
            if (targetCount < GetValue(ActorSkillPropertyType.MaxTargetCount))
            {
                foreach (Collider c in colliders_PlayerLayer)
                {
                    Actor actor = c.GetComponentInParent<Actor>();
                    if (actor != null && !actorGUIDSet.Contains(actor.GUID))
                    {
                        actorGUIDSet.Add(actor.GUID);
                        actor.ActorBattleHelper.Damage(Actor, GetValue(ActorSkillPropertyType.Damage));
                        actor.ActorStatPropSet.FiringValue.Value += GetValue(ActorSkillPropertyType.Attach_FiringValue);
                        actor.ActorStatPropSet.FrozenValue.Value += GetValue(ActorSkillPropertyType.Attach_FrozenValue);
                        targetCount++;
                    }
                }
            }

            Collider[] colliders_BoxLayer = Physics.OverlapSphere(gp.ToVector3(), 0.3f, LayerManager.Instance.LayerMask_BoxIndicator);
            foreach (Collider c in colliders_BoxLayer)
            {
                Box box = c.GetComponentInParent<Box>();
                if (box != null && !boxGUIDSet.Contains(box.GUID))
                {
                    boxGUIDSet.Add(box.GUID);
                    box.BoxStatPropSet.FiringValue.Value += GetValue(ActorSkillPropertyType.Attach_FiringValue);
                    box.BoxStatPropSet.FrozenValue.Value += GetValue(ActorSkillPropertyType.Attach_FrozenValue);
                }
            }
        }

        yield return base.Cast(castDuration);
    }

    protected override void ChildClone(ActorActiveSkill cloneData)
    {
        base.ChildClone(cloneData);
        ActorActiveSkill_Attack newAAS = (ActorActiveSkill_Attack) cloneData;
    }

    public override void CopyDataFrom(ActorActiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        ActorActiveSkill_Attack srcAAS = (ActorActiveSkill_Attack) srcData;
    }
}
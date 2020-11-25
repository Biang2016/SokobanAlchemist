using System;
using System.Collections.Generic;
using BiangStudio.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class ActorActiveSkill_Attack : ActorActiveSkill_AreaCast
{
    protected override string Description => "普通攻击";

    [LabelText("攻击伤害")]
    public ActorPropertyType APT_AttackDamage;

    internal ActorPropertyValue AttackDamage = new ActorPropertyValue();

    [LabelText("攻击附加燃烧值")]
    public ActorPropertyType APT_AttackAttach_FiringValue;

    internal ActorPropertyValue AttackAttach_FiringValue = new ActorPropertyValue();

    [LabelText("攻击附加冰冻值")]
    public ActorPropertyType APT_AttackAttach_FrozenValue;

    internal ActorPropertyValue AttackAttach_FrozenValue = new ActorPropertyValue();

    public override void OnInit()
    {
        base.OnInit();
        BindActorProperty(AttackDamage, APT_AttackDamage);
        BindActorProperty(AttackAttach_FiringValue, APT_AttackAttach_FiringValue);
        BindActorProperty(AttackAttach_FrozenValue, APT_AttackAttach_FrozenValue);
    }

    public override void OnUnInit()
    {
        base.OnUnInit();
        UnBindActorProperty(AttackDamage, APT_AttackDamage);
        UnBindActorProperty(AttackAttach_FiringValue, APT_AttackAttach_FiringValue);
        UnBindActorProperty(AttackAttach_FrozenValue, APT_AttackAttach_FrozenValue);
    }

    protected override void Cast()
    {
        base.Cast();
        int targetCount = 0;
        HashSet<uint> actorGUIDSet = new HashSet<uint>();
        foreach (GridPos3D gp in RealSkillEffectGPs)
        {
            Collider[] colliders = Physics.OverlapSphere(gp.ToVector3(), 0.3f, LayerManager.Instance.GetTargetActorLayerMask(Actor.Camp, TargetCamp));
            foreach (Collider c in colliders)
            {
                Actor actor = c.GetComponentInParent<Actor>();
                if (actor != null && !actorGUIDSet.Contains(actor.GUID))
                {
                    actorGUIDSet.Add(actor.GUID);
                    actor.ActorBattleHelper.Damage(Actor, AttackDamage.Value);
                    actor.ActorStatPropSet.FiringValue.Value += AttackAttach_FiringValue.Value;
                    actor.ActorStatPropSet.FrozenValue.Value += AttackAttach_FrozenValue.Value;
                    targetCount++;
                    if (targetCount >= MaxTargetCount) return;
                }
            }
        }
    }

    protected override void ChildClone(ActorActiveSkill newAS)
    {
        base.ChildClone(newAS);
        ActorActiveSkill_Attack asAttack = (ActorActiveSkill_Attack) newAS;
        asAttack.APT_AttackDamage = APT_AttackDamage;
        asAttack.APT_AttackAttach_FiringValue = APT_AttackAttach_FiringValue;
        asAttack.APT_AttackAttach_FrozenValue = APT_AttackAttach_FrozenValue;
    }

    public override void CopyDataFrom(ActorActiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        ActorActiveSkill_Attack asAttack = (ActorActiveSkill_Attack) srcData;
        APT_AttackDamage = asAttack.APT_AttackDamage;
        APT_AttackAttach_FiringValue = asAttack.APT_AttackAttach_FiringValue;
        APT_AttackAttach_FrozenValue = asAttack.APT_AttackAttach_FrozenValue;
    }
}
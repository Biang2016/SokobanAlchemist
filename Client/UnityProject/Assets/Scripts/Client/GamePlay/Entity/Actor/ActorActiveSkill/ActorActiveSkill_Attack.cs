using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class ActorActiveSkill_Attack : ActorActiveSkill_AreaCast
{
    protected override string Description => "普通攻击";

    [LabelText("攻击伤害")]
    public ActorPropertyType APT_AttackDamage;

    internal ActorPropertyValue AttackDamage;

    [LabelText("攻击燃烧伤害")]
    public ActorPropertyType APT_AttackDamage_Firing;

    internal ActorPropertyValue AttackDamage_Firing;

    [LabelText("攻击冰冻伤害")]
    public ActorPropertyType APT_AttackDamage_Frozen;

    internal ActorPropertyValue AttackDamage_Frozen;

    public override void OnInit()
    {
        base.OnInit();
        BindActorProperty(AttackDamage, APT_AttackDamage);
        BindActorProperty(AttackDamage_Firing, APT_AttackDamage_Firing);
        BindActorProperty(AttackDamage_Frozen, APT_AttackDamage_Frozen);
    }

    public override void OnUnInit()
    {
        base.OnUnInit();
        UnBindActorProperty(AttackDamage, APT_AttackDamage);
        UnBindActorProperty(AttackDamage_Firing, APT_AttackDamage_Firing);
        UnBindActorProperty(AttackDamage_Frozen, APT_AttackDamage_Frozen);
    }

    public override bool TriggerActiveSkill()
    {
        if (base.TriggerActiveSkill())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    protected override void WingUp()
    {
        base.WingUp();
    }

    protected override void Cast()
    {
        base.Cast();
    }

    protected override void Recover()
    {
        base.Recover();
    }

    public override void FixedUpdate(float fixedDeltaTime)
    {
        base.FixedUpdate(fixedDeltaTime);
    }

    protected override void ChildClone(ActorActiveSkill newAS)
    {
        base.ChildClone(newAS);
        ActorActiveSkill_Attack asAttack = (ActorActiveSkill_Attack) newAS;
        asAttack.APT_AttackDamage = APT_AttackDamage;
        asAttack.APT_AttackDamage_Firing = APT_AttackDamage_Firing;
        asAttack.APT_AttackDamage_Frozen = APT_AttackDamage_Frozen;
    }

    public override void CopyDataFrom(ActorActiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        ActorActiveSkill_Attack asAttack = (ActorActiveSkill_Attack) srcData;
        APT_AttackDamage = asAttack.APT_AttackDamage;
        APT_AttackDamage_Firing = asAttack.APT_AttackDamage_Firing;
        APT_AttackDamage_Frozen = asAttack.APT_AttackDamage_Frozen;
    }
}
using System;
using Sirenix.OdinInspector;

[assembly: Sirenix.Serialization.BindTypeNameToType("BoxFunction_LiftGainHealth", typeof(BoxPassiveSkill_LiftGainHealth))]

[Serializable]
public class BoxPassiveSkill_LiftGainHealth : BoxPassiveSkill
{
    protected override string Description => "举箱子回复生命";

    [LabelText("回复生命")]
    public int GainHealthWhenLifted;

    public override void OnBeingLift(Actor actor)
    {
        base.OnBeingLift(actor);
        actor.ActorBattleHelper.Heal(actor, GainHealthWhenLifted);
    }

    protected override void ChildClone(BoxPassiveSkill newBF)
    {
        base.ChildClone(newBF);
        BoxPassiveSkill_LiftGainHealth bf = ((BoxPassiveSkill_LiftGainHealth) newBF);
        bf.GainHealthWhenLifted = GainHealthWhenLifted;
    }

    public override void CopyDataFrom(BoxPassiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        BoxPassiveSkill_LiftGainHealth bf = ((BoxPassiveSkill_LiftGainHealth) srcData);
        GainHealthWhenLifted = bf.GainHealthWhenLifted;
    }
}
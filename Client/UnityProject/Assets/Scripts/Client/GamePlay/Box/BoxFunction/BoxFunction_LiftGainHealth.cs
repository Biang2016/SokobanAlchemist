using System;
using Sirenix.OdinInspector;

[Serializable]
public class BoxFunction_LiftGainHealth : BoxFunctionBase
{
    protected override string BoxFunctionDisplayName => "举箱子回复生命";

    [LabelText("回复生命")]
    public int GainHealthWhenLifted;

    public override void OnBeingLift(Actor actor)
    {
        base.OnBeingLift(actor);
        actor.ActorBattleHelper.Heal(actor, GainHealthWhenLifted);
    }

    protected override void ChildClone(BoxFunctionBase newBF)
    {
        base.ChildClone(newBF);
        BoxFunction_LiftGainHealth bf = ((BoxFunction_LiftGainHealth) newBF);
        bf.GainHealthWhenLifted = GainHealthWhenLifted;
    }

    public override void CopyDataFrom(BoxFunctionBase srcData)
    {
        base.CopyDataFrom(srcData);
        BoxFunction_LiftGainHealth bf = ((BoxFunction_LiftGainHealth) srcData);
        GainHealthWhenLifted = bf.GainHealthWhenLifted;
    }
}
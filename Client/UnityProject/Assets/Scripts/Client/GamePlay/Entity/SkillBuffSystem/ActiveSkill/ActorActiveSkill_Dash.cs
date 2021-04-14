using System;
using System.Collections;
using Sirenix.OdinInspector;

[Serializable]
public class ActorActiveSkill_Dash : EntityActiveSkill
{
    protected override string Description => "冲刺";

    [LabelText("冲刺最大距离")]
    public int DashMaxDistance = 4;

    protected override IEnumerator Cast(float castDuration)
    {
        if (Entity is Actor actor)
        {
            actor.Dash(DashMaxDistance);
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
using System;
using System.Collections;
using Sirenix.OdinInspector;

[Serializable]
public class ActorActiveSkill_DashOrVault : EntityActiveSkill
{
    protected override string Description => "冲刺或翻越";

    [LabelText("冲刺最大距离")]
    public int DashMaxDistance = 4;

    protected override IEnumerator Cast(float castDuration)
    {
        if (Entity is Actor actor)
        {
            actor.VaultOrDash(DashMaxDistance);
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
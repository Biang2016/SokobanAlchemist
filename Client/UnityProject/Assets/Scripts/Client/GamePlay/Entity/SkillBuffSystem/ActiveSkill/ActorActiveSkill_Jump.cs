using System;
using System.Collections;
using Sirenix.OdinInspector;

[Serializable]
public class ActorActiveSkill_Jump : EntityActiveSkill
{
    protected override string Description => "跳跃";

    [LabelText("起跳高度")]
    public int JumpHeight = 1;

    protected override IEnumerator Cast(float castDuration)
    {
        if (Entity is Actor actor)
        {
            actor.SetJumpUpTargetHeight(actor.ActiveJumpForce, JumpHeight, false);
        }

        yield return base.Cast(castDuration);
    }

    protected override void ChildClone(EntityActiveSkill cloneData)
    {
        base.ChildClone(cloneData);
        ActorActiveSkill_Jump newEAS = (ActorActiveSkill_Jump) cloneData;
        newEAS.JumpHeight = JumpHeight;
    }

    public override void CopyDataFrom(EntityActiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        ActorActiveSkill_Jump srcEAS = (ActorActiveSkill_Jump) srcData;
        JumpHeight = srcEAS.JumpHeight;
    }
}
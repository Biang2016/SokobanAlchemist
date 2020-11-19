using System;
using Sirenix.OdinInspector;

[Serializable]
public abstract class ActorActiveSkill_AreaCast : ActorActiveSkill
{
    public enum CastType
    {
        FrontSingle,
        SphereCast,
        BoxCast,
        FrontRayCast,
        FrontFanCast
    }

    [LabelText("判定范围类型")]
    public CastType AreaCastType;

    public override void OnInit()
    {
        base.OnInit();
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

    protected override void ChildClone(ActorActiveSkill newAS)
    {
        base.ChildClone(newAS);
        ActorActiveSkill_AreaCast asAreaCast = (ActorActiveSkill_AreaCast) newAS;
        asAreaCast.AreaCastType = AreaCastType;
    }

    public override void CopyDataFrom(ActorActiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        ActorActiveSkill_AreaCast asAreaCast = (ActorActiveSkill_AreaCast) srcData;
        AreaCastType = asAreaCast.AreaCastType;
    }
}
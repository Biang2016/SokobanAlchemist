using System;
using Sirenix.OdinInspector;

[Serializable]
public class EntityPassiveSkill_ProbablyShow : EntityPassiveSkill
{
    protected override string Description => "此Entity有概率出现";

    [LabelText("出现概率%")]
    public uint ShowProbabilityPercent = 100;

    protected override void ChildClone(EntityPassiveSkill newPS)
    {
        base.ChildClone(newPS);
        EntityPassiveSkill_ProbablyShow newPSC = (EntityPassiveSkill_ProbablyShow) newPS;
        newPSC.ShowProbabilityPercent = ShowProbabilityPercent;
    }

    public override void CopyDataFrom(EntityPassiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        EntityPassiveSkill_ProbablyShow srcPSC = (EntityPassiveSkill_ProbablyShow) srcData;
        ShowProbabilityPercent = srcPSC.ShowProbabilityPercent;
    }
}
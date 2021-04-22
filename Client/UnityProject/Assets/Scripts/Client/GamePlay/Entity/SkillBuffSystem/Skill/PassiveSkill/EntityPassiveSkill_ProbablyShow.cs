using System;
using Sirenix.OdinInspector;

[Serializable]
public class EntityPassiveSkill_ProbablyShow : EntityPassiveSkill
{
    protected override string Description => "此Entity有概率出现";

    [LabelText("出现概率%")]
    public uint ShowProbabilityPercent = 100;

    protected override void ChildClone(EntitySkill cloneData)
    {
        base.ChildClone(cloneData);
        EntityPassiveSkill_ProbablyShow newPSC = (EntityPassiveSkill_ProbablyShow) cloneData;
        newPSC.ShowProbabilityPercent = ShowProbabilityPercent;
    }

    public override void CopyDataFrom(EntitySkill srcData)
    {
        base.CopyDataFrom(srcData);
        EntityPassiveSkill_ProbablyShow srcPSC = (EntityPassiveSkill_ProbablyShow) srcData;
        ShowProbabilityPercent = srcPSC.ShowProbabilityPercent;
    }
}
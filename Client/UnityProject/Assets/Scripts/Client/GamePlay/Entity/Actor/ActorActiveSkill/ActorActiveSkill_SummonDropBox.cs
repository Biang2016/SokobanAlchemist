using System;
using System.Collections.Generic;
using BiangStudio.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class ActorActiveSkill_SummonDropBox : ActorActiveSkill_AreaCast
{
    protected override string Description => "天降箱子";

    protected override void Cast()
    {
        base.Cast();
        foreach (GridPos3D gp in RealSkillEffectGPs)
        {
        }
    }

    protected override void ChildClone(ActorActiveSkill cloneData)
    {
        base.ChildClone(cloneData);
        ActorActiveSkill_SummonDropBox newAAS = (ActorActiveSkill_SummonDropBox) cloneData;
    }

    public override void CopyDataFrom(ActorActiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        ActorActiveSkill_SummonDropBox srcAAS = (ActorActiveSkill_SummonDropBox) srcData;
    }
}
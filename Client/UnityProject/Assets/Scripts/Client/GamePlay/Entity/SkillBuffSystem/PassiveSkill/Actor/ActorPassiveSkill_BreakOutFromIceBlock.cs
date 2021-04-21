using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class ActorPassiveSkill_BreakOutFromIceBlock : ActorPassiveSkill
{
    protected override string Description => "冻结一段时间自动挣脱，且获得Buff";

    [LabelText("冻结多少秒后")]
    public float FrozenMaxTime = 3f;

    [SerializeReference]
    [LabelText("施加Buff")]
    public List<EntityBuff> RawEntityBuffs = new List<EntityBuff>();

    private float frozenTimeTick = 0f;

    public override void OnTick(float tickDeltaTime)
    {
        base.OnTick(tickDeltaTime);
        if (Actor.IsFrozen)
        {
            frozenTimeTick += tickDeltaTime;
            if (frozenTimeTick >= FrozenMaxTime)
            {
                frozenTimeTick = 0f;
                foreach (EntityBuff rawEntityBuff in RawEntityBuffs)
                {
                    Actor.ActorBuffHelper.AddBuff(rawEntityBuff.Clone());
                }
            }
        }
        else
        {
            frozenTimeTick = 0f;
        }
    }

    protected override void ChildClone(EntitySkill cloneData)
    {
        base.ChildClone(cloneData);
        ActorPassiveSkill_BreakOutFromIceBlock ps = ((ActorPassiveSkill_BreakOutFromIceBlock) cloneData);
        ps.FrozenMaxTime = FrozenMaxTime;
        ps.RawEntityBuffs = RawEntityBuffs.Clone<EntityBuff, EntityBuff>();
    }

    public override void CopyDataFrom(EntitySkill srcData)
    {
        base.CopyDataFrom(srcData);
        ActorPassiveSkill_BreakOutFromIceBlock ps = ((ActorPassiveSkill_BreakOutFromIceBlock) srcData);
        FrozenMaxTime = ps.FrozenMaxTime;
        for (int i = 0; i < ps.RawEntityBuffs.Count; i++)
        {
            RawEntityBuffs[i].CopyDataFrom(ps.RawEntityBuffs[i]);
        }
    }
}
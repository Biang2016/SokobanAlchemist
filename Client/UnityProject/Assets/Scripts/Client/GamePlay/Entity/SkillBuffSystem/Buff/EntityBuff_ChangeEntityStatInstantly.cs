using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntityBuff_ChangeEntityStatInstantly : EntityBuff
{
    protected override string Description => "瞬间更改状态值. buff施加后, 不残留在Entity身上, 无移除的概念。但此buff有可能被既有buff免疫或抵消等";

    [LabelText("状态值类型")]
    public EntityStatType EntityStatType;

    [LabelText("变化量")]
    public int Delta;

    [LabelText("增加比率%")]
    public int Percent;

    public override void OnAdded(Entity entity)
    {
        base.OnAdded(entity);
        if (!entity.IsNotNullAndAlive()) return;
        float valueBefore = entity.EntityStatPropSet.StatDict[EntityStatType].Value;
        valueBefore += Delta;
        valueBefore *= (100 + Percent) / 100f;

        entity.EntityStatPropSet.StatDict[EntityStatType].SetValue(Mathf.RoundToInt(valueBefore), "ChangeEntityStatInstantly");
    }

    protected override void ChildClone(EntityBuff newBuff)
    {
        base.ChildClone(newBuff);
        EntityBuff_ChangeEntityStatInstantly buff = ((EntityBuff_ChangeEntityStatInstantly) newBuff);
        buff.EntityStatType = EntityStatType;
        buff.Delta = Delta;
        buff.Percent = Percent;
    }
}
using System;

[Serializable]
public class EntityBuff_DestroySelf : EntityBuff
{
    protected override string Description => "毁灭自己";

    public override void OnAdded(Entity entity, string extraInfo)
    {
        base.OnAdded(entity);
        if (!entity.IsNotNullAndAlive()) return;
        entity.PassiveSkillMarkAsDestroyed = true;
    }

    protected override void ChildClone(EntityBuff newBuff)
    {
        base.ChildClone(newBuff);
        EntityBuff_DestroySelf buff = ((EntityBuff_DestroySelf) newBuff);
    }

    public override void CopyDataFrom(EntityBuff srcData)
    {
        base.CopyDataFrom(srcData);
        EntityBuff_DestroySelf srcBuff = ((EntityBuff_DestroySelf) srcData);
    }
}
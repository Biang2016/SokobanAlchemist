using System;
using BiangStudio.CloneVariant;
using Sirenix.OdinInspector;

[Serializable]
public class ActorBuff : IClone<ActorBuff>
{
    #region GUID

    [ReadOnly]
    [HideInEditorMode]
    public uint GUID;

    private static uint guidGenerator = (uint) ConfigManager.GUID_Separator.Buff;

    private uint GetGUID()
    {
        return guidGenerator++;
    }

    #endregion

    public ActorBuff()
    {
        GUID = GetGUID();
    }

    public ActorBuff Clone()
    {
        Type type = GetType();
        ActorBuff newBuff = (ActorBuff) Activator.CreateInstance(type);
        ChildClone(newBuff);
        return newBuff;
    }

    protected virtual void ChildClone(ActorBuff newBuff)
    {
    }
}

[Serializable]
public class ActorBuff_ChangeMoveSpeed : ActorBuff
{
    [LabelText("速度增加比率%")]
    public int Percent;

    protected override void ChildClone(ActorBuff newBuff)
    {
        base.ChildClone(newBuff);
        ActorBuff_ChangeMoveSpeed buff = ((ActorBuff_ChangeMoveSpeed) newBuff);
        buff.Percent = Percent;
    }
}
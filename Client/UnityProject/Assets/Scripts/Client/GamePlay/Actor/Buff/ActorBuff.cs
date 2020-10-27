using System;
using System.Collections.Generic;
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

    [ValueDropdown("GetAllFXTypeNames")]
    [LabelText("Buff特效")]
    public string BuffFX;

    [LabelText("Buff特效尺寸")]
    public float BuffFXScale = 1.0f;

    public ActorBuff()
    {
        GUID = GetGUID();
    }

    public virtual void OnAdded(Actor actor)
    {
    }

    public virtual void OnFixedUpdate(Actor actor, float passedTime, float remainTime)
    {
    }

    public virtual void OnRemoved(Actor actor)
    {
    }

    public ActorBuff Clone()
    {
        Type type = GetType();
        ActorBuff newBuff = (ActorBuff) Activator.CreateInstance(type);
        newBuff.BuffFX = BuffFX;
        newBuff.BuffFXScale = BuffFXScale;
        ChildClone(newBuff);
        return newBuff;
    }

    protected virtual void ChildClone(ActorBuff newBuff)
    {
    }

    #region Utils

    private IEnumerable<string> GetAllBoxTypeNames => ConfigManager.GetAllBoxTypeNames();

    private IEnumerable<string> GetAllFXTypeNames => ConfigManager.GetAllFXTypeNames();

    #endregion
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

[Serializable]
public class ActorBuff_FreezeToIceBlock : ActorBuff
{
    [BoxGroup("冻结成冰块")]
    [LabelText("持续时间(一阶段)")]
    public float FreezeDuration1;

    [BoxGroup("冻结成冰块")]
    [LabelText("持续时间(二阶段)")]
    public float FreezeDuration2;

    [BoxGroup("冻结成冰块")]
    [LabelText("持续时间(二阶段)")]
    public float FreezeDuration3;

    protected override void ChildClone(ActorBuff newBuff)
    {
        base.ChildClone(newBuff);
        ActorBuff_FreezeToIceBlock buff = ((ActorBuff_FreezeToIceBlock) newBuff);
        buff.FreezeDuration1 = FreezeDuration1;
        buff.FreezeDuration2 = FreezeDuration2;
        buff.FreezeDuration3 = FreezeDuration3;
    }

    public override void OnFixedUpdate(Actor actor, float passedTime, float remainTime)
    {
        base.OnFixedUpdate(actor, passedTime, remainTime);
        if (passedTime > FreezeDuration1 + FreezeDuration2 + FreezeDuration3)
        {
            // restore
        }
        else if (passedTime > FreezeDuration1 + FreezeDuration2)
        {
            // lower stage
        }
        else if (passedTime > FreezeDuration1)
        {
            // higher stage
        }
        else
        {
            // full
        }
    }
}

[Serializable]
public class ActorBuff_GainLifeInstantly : ActorBuff
{
    [LabelText("获取生命值")]
    public int GainHealth;

    protected override void ChildClone(ActorBuff newBuff)
    {
        base.ChildClone(newBuff);
        ActorBuff_GainLifeInstantly buff = ((ActorBuff_GainLifeInstantly) newBuff);
        buff.GainHealth = GainHealth;
    }

    public override void OnAdded(Actor actor)
    {
        base.OnAdded(actor);
        actor.ActorBattleHelper.Heal(actor, GainHealth);
    }
}
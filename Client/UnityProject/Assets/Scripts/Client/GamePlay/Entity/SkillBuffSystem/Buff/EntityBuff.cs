using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

[Serializable]
public abstract class EntityBuff : IClone<EntityBuff>
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

    internal uint CasterActorGUID;

    [LabelText("Buff描述")]
    [ShowInInspector]
    [PropertyOrder(-1)]
    protected abstract string Description { get; }

    [LabelText("Buff标签")]
    public EntityBuffAttribute EntityBuffAttribute;

    protected string validateBuffAttributeInfo = "";

    [LabelText("永久Buff")]
    public bool IsPermanent;

    [LabelText("Buff持续时间")]
    [HideIf("IsPermanent")]
    public float Duration;

    private bool ValidateDuration(float duration)
    {
        if (!IsPermanent && duration.Equals(0))
        {
            return false;
        }

        return true;
    }

    [LabelText("Buff特效")]
    public FXConfig BuffFX = new FXConfig();

    protected EntityBuff()
    {
        GUID = GetGUID();
    }

    public virtual void OnAdded(Entity entity, string extraInfo = null)
    {
    }

    public virtual void OnFixedUpdate(Entity entity, float passedTime, float remainTime)
    {
    }

    public virtual void OnRemoved(Entity entity)
    {
    }

    public EntityBuff Clone()
    {
        Type type = GetType();
        EntityBuff newBuff = (EntityBuff) Activator.CreateInstance(type);
        newBuff.EntityBuffAttribute = EntityBuffAttribute;
        newBuff.IsPermanent = IsPermanent;
        newBuff.Duration = Duration;
        newBuff.BuffFX = BuffFX;
        ChildClone(newBuff);
        return newBuff;
    }

    protected virtual void ChildClone(EntityBuff newBuff)
    {
    }

    public virtual void CopyDataFrom(EntityBuff srcData)
    {
        EntityBuffAttribute = srcData.EntityBuffAttribute;
        IsPermanent = srcData.IsPermanent;
        Duration = srcData.Duration;
        BuffFX = srcData.BuffFX;
    }
}
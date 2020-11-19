using System;
using System.Collections.Generic;
using BiangStudio.CloneVariant;
using Sirenix.OdinInspector;

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

    [LabelText("Buff描述")]
    [ShowInInspector]
    [PropertyOrder(-1)]
    protected abstract string Description { get; }

    [LabelText("Buff标签")]
    [ValidateInput("ValidateBuffAttribute", "$validateBuffAttributeInfo")]
    public BuffAttribute BuffAttribute;

    protected string validateBuffAttributeInfo = "";

    protected virtual bool ValidateBuffAttribute(BuffAttribute buffAttribute)
    {
        return true;
    }

    [LabelText("永久Buff")]
    [HideIf("BuffAttribute", BuffAttribute.InstantEffect)]
    public bool IsPermanent;

    [LabelText("Buff持续时间")]
    [HideIf("IsPermanent")]
    [HideIf("BuffAttribute", BuffAttribute.InstantEffect)]
    [ValidateInput("ValidateDuration", "若非【瞬时效果】或永久Buff，持续时间不可为0")]
    public float Duration;

    private bool ValidateDuration(float duration)
    {
        if (!IsPermanent && duration.Equals(0))
        {
            return false;
        }

        return true;
    }

    [ValueDropdown("GetAllFXTypeNames")]
    [LabelText("Buff特效")]
    public string BuffFX;

    [LabelText("Buff特效尺寸")]
    public float BuffFXScale = 1.0f;

    protected EntityBuff()
    {
        GUID = GetGUID();
    }

    public virtual void OnAdded(Entity entity)
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
        newBuff.BuffAttribute = BuffAttribute;
        newBuff.IsPermanent = IsPermanent;
        newBuff.Duration = Duration;
        newBuff.BuffFX = BuffFX;
        newBuff.BuffFXScale = BuffFXScale;
        ChildClone(newBuff);
        return newBuff;
    }

    protected virtual void ChildClone(EntityBuff newBuff)
    {
    }

    #region Utils

    private IEnumerable<string> GetAllBoxTypeNames => ConfigManager.GetAllBoxTypeNames();

    private IEnumerable<string> GetAllFXTypeNames => ConfigManager.GetAllFXTypeNames();

    #endregion
}
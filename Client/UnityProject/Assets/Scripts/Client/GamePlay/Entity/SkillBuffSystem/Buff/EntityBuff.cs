using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;
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

    public string BuffAlias = "";

    #endregion

    internal uint CasterActorGUID;

    [LabelText("Buff描述")]
    [ShowInInspector]
    [PropertyOrder(-1)]
    protected abstract string Description { get; }

    [LabelText("Buff标签")]
    public EntityBuffAttribute EntityBuffAttribute;

    #region 白名单黑名单

    [LabelText("启用黑白名单")]
    public bool EnableWhiteBlackList = false;

    [LabelText("具有以下Buff之一才能施加本Buff")]
    [ShowIf("EnableWhiteBlackList")]
    public List<string> AllowAddBuffAliasList = new List<string>();

    [ShowIf("EnableWhiteBlackList")]
    [LabelText("具有以下Buff之一禁止施加本Buff")]
    public List<string> ForbidAddBuffAliasList = new List<string>();

    #endregion

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

    [LabelText("@\"Buff特效\t\t\"+BuffFX")]
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
        newBuff.BuffAlias = BuffAlias;
        newBuff.EntityBuffAttribute = EntityBuffAttribute;
        newBuff.EnableWhiteBlackList = EnableWhiteBlackList;
        newBuff.AllowAddBuffAliasList = AllowAddBuffAliasList.Clone();
        newBuff.ForbidAddBuffAliasList = ForbidAddBuffAliasList.Clone();
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
        BuffAlias = srcData.BuffAlias;
        EntityBuffAttribute = srcData.EntityBuffAttribute;

        EnableWhiteBlackList = srcData.EnableWhiteBlackList;
        for (int i = 0; i < srcData.AllowAddBuffAliasList.Count; i++)
        {
            if (i < AllowAddBuffAliasList.Count) AllowAddBuffAliasList[i] = srcData.AllowAddBuffAliasList[i];
            else AllowAddBuffAliasList.Add(srcData.AllowAddBuffAliasList[i]);
        }

        while (AllowAddBuffAliasList.Count > srcData.AllowAddBuffAliasList.Count)
        {
            AllowAddBuffAliasList.RemoveAt(AllowAddBuffAliasList.Count - 1);
        }

        for (int i = 0; i < srcData.ForbidAddBuffAliasList.Count; i++)
        {
            if (i < ForbidAddBuffAliasList.Count) ForbidAddBuffAliasList[i] = srcData.ForbidAddBuffAliasList[i];
            else ForbidAddBuffAliasList.Add(srcData.ForbidAddBuffAliasList[i]);
        }

        while (ForbidAddBuffAliasList.Count > srcData.ForbidAddBuffAliasList.Count)
        {
            ForbidAddBuffAliasList.RemoveAt(ForbidAddBuffAliasList.Count - 1);
        }

        IsPermanent = srcData.IsPermanent;
        Duration = srcData.Duration;
        BuffFX = srcData.BuffFX;
    }
}
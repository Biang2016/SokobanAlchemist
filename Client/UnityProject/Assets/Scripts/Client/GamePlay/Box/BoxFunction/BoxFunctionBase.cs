using System;
using System.Collections.Generic;
using BiangStudio.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class BoxFunctionBase : IClone<BoxFunctionBase>
{
    internal Box Box;

    protected abstract string BoxFunctionDisplayName { get; }

    [InfoBox("@BoxFunctionDisplayName")]
    [LabelText("特例类型")]
    [EnumToggleButtons]
    public BoxFunctionBaseSpecialCaseType SpecialCaseType = BoxFunctionBaseSpecialCaseType.None;

    public enum BoxFunctionBaseSpecialCaseType
    {
        [LabelText("无")]
        None,

        [LabelText("模组特例")]
        Module,

        [LabelText("模组特例")]
        World,
    }

    private IEnumerable<string> GetAllBoxTypeNames => ConfigManager.GetAllBoxTypeNames();

    private IEnumerable<string> GetAllEnemyNames => ConfigManager.GetAllEnemyNames();

    public virtual void OnInit()
    {
    }

    public virtual void OnRegisterLevelEventID()
    {
    }

    public virtual void OnUnRegisterLevelEventID()
    {
    }

    public virtual void OnBeingLift(Actor actor)
    {
    }

    public virtual void OnFlyingCollisionEnter(Collision collision)
    {
    }

    public virtual void OnBeingKickedCollisionEnter(Collision collision)
    {
    }

    public virtual void OnBoxThornTrapTriggerEnter(Collider collider)
    {
    }

    public virtual void OnBoxThornTrapTriggerStay(Collider collider)
    {
    }

    public virtual void OnBoxThornTrapTriggerExit(Collider collider)
    {
    }

    public virtual void OnDeleteBox()
    {
    }

    public BoxFunctionBase Clone()
    {
        Type type = GetType();
        BoxFunctionBase newBF = (BoxFunctionBase) Activator.CreateInstance(type);
        ChildClone(newBF);
        return newBF;
    }

    protected virtual void ChildClone(BoxFunctionBase newBF)
    {
    }

    public virtual void CopyDataFrom(BoxFunctionBase srcData)
    {
    }
}
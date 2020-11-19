using System;
using System.Collections.Generic;
using BiangStudio.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[assembly: Sirenix.Serialization.BindTypeNameToType("BoxFunctionBase", typeof(BoxPassiveSkill))]
[assembly: Sirenix.Serialization.BindTypeNameToType("BoxPassiveSkill.BoxFunctionBaseSpecialCaseType", typeof(BoxPassiveSkill.BoxPassiveSkillBaseSpecialCaseType))]

[Serializable]
public abstract class BoxPassiveSkill : IClone<BoxPassiveSkill>
{
    internal Box Box;

    [LabelText("技能描述")]
    [ShowInInspector]
    [PropertyOrder(-1)]
    protected abstract string Description { get; }

    [LabelText("特例类型")]
    [EnumToggleButtons]
    public BoxPassiveSkillBaseSpecialCaseType SpecialCaseType = BoxPassiveSkillBaseSpecialCaseType.None;

    public enum BoxPassiveSkillBaseSpecialCaseType
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

    public BoxPassiveSkill Clone()
    {
        Type type = GetType();
        BoxPassiveSkill newPS = (BoxPassiveSkill) Activator.CreateInstance(type);
        ChildClone(newPS);
        return newPS;
    }

    protected virtual void ChildClone(BoxPassiveSkill newPS)
    {
    }

    public virtual void CopyDataFrom(BoxPassiveSkill srcData)
    {
    }
}
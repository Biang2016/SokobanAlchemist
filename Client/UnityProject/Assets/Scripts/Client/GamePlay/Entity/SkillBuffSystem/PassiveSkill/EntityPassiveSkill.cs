using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class EntityPassiveSkill : IClone<EntityPassiveSkill>
{
    internal Entity Entity;

    public bool IsAddedDuringGamePlay = false; // 是否是在游戏过程中添加的，以便在回收之后判断要不要清掉

    [ReadOnly]
    [HideInEditorMode]
    public uint InitWorldModuleGUID; // 创建时所属的世界模组GUID

    [LabelText("技能描述")]
    [ShowInInspector]
    [PropertyOrder(-1)]
    protected abstract string Description { get; }

    #region Conditions

    public virtual void OnInit()
    {
    }

    public virtual void OnUnInit()
    {
    }

    public virtual void OnTick(float deltaTime)
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

    public virtual void OnBeingKicked(Actor actor)
    {
    }

    public virtual void OnFlyingCollisionEnter(Collision collision)
    {
    }

    public virtual void OnBeingKickedCollisionEnter(Collision collision, Box.KickAxis kickLocalAxis)
    {
    }

    public virtual void OnDroppingFromAirCollisionEnter(Collision collision)
    {
    }

    public virtual void OnTriggerZoneEnter(Collider collider)
    {
    }

    public virtual void OnTriggerZoneStay(Collider collider)
    {
    }

    public virtual void OnTriggerZoneExit(Collider collider)
    {
    }

    public virtual void OnGrindTriggerZoneEnter(Collider collider)
    {
    }

    public virtual void OnGrindTriggerZoneStay(Collider collider)
    {
    }

    public virtual void OnGrindTriggerZoneExit(Collider collider)
    {
    }

    public virtual void OnBeforeDestroyEntity()
    {
    }

    public virtual void OnBeforeMergeBox()
    {
    }

    public virtual void OnDestroyEntity()
    {
    }

    public virtual void OnMergeBox()
    {
    }

    public virtual void OnDestroyEntityByElementDamage(EntityBuffAttribute entityBuffAttribute)
    {
    }

    public virtual void OnBeingFueled()
    {
    }

    #endregion

    public EntityPassiveSkill Clone()
    {
        Type type = GetType();
        EntityPassiveSkill newPS = (EntityPassiveSkill) Activator.CreateInstance(type);
        ChildClone(newPS);
        return newPS;
    }

    protected virtual void ChildClone(EntityPassiveSkill newPS)
    {
    }

    public virtual void CopyDataFrom(EntityPassiveSkill srcData)
    {
    }
}
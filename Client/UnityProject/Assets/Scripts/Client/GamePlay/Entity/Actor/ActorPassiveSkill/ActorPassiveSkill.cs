using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class ActorPassiveSkill : IClone<ActorPassiveSkill>
{
    internal Actor Actor;

    [LabelText("技能描述")]
    [ShowInInspector]
    [PropertyOrder(-1)]
    protected abstract string Description { get; }

    private IEnumerable<string> GetAllBoxTypeNames => ConfigManager.GetAllBoxTypeNames();

    private IEnumerable<string> GetAllEnemyNames => ConfigManager.GetAllEnemyNames();

    public virtual void OnInit()
    {
    }

    public virtual void OnUnInit()
    {
    }

    public virtual void OnRegisterLevelEventID()
    {
    }

    public virtual void OnUnRegisterLevelEventID()
    {
    }

    public virtual void OnFlyingCollisionEnter(Collision collision)
    {
    }

    public virtual void OnKickedBoxCollisionEnter(Collision collision)
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

    public virtual void OnTick(float tickDeltaTime)
    {
    }

    public virtual void OnActorDie()
    {
    }

    public ActorPassiveSkill Clone()
    {
        Type type = GetType();
        ActorPassiveSkill newPS = (ActorPassiveSkill) Activator.CreateInstance(type);
        ChildClone(newPS);
        return newPS;
    }

    protected virtual void ChildClone(ActorPassiveSkill newPS)
    {
    }

    public virtual void CopyDataFrom(ActorPassiveSkill srcData)
    {
    }
}
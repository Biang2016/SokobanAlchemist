using System;
using System.Collections.Generic;
using BiangStudio.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class ActorPassiveSkill : IClone<ActorPassiveSkill>
{
    internal Actor Actor;

    [InfoBox("@ActorPassiveSkillDisplayName")]
    protected abstract string ActorPassiveSkillDisplayName { get; }

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

    public virtual void OnFlyingCollisionEnter(Collision collision)
    {
    }

    public virtual void OnKickedBoxCollisionEnter(Collision collision)
    {
    }

    public virtual void OnThornTrapTriggerEnter(Collider collider)
    {
    }

    public virtual void OnThornTrapTriggerStay(Collider collider)
    {
    }

    public virtual void OnThornTrapTriggerExit(Collider collider)
    {
    }

    public virtual void OnTick()
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
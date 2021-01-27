﻿using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using UnityEngine;

[Serializable]
public abstract class EntityPassiveSkillAction : IClone<EntityPassiveSkillAction>
{
    protected virtual string Description => "Entity被动技能行为基类";

    public interface IPureAction
    {
        void Execute();
    }

    public interface IActorOperationAction
    {
        void OnOperation(Actor actor);
    }

    public interface ICollideAction
    {
        void OnCollide(Collision collision);
    }

    public interface ITriggerAction
    {
        void OnTriggerEnter(Collider collider);
        void OnTriggerStay(Collider collider);
        void OnTriggerExit(Collider collider);
    }

    internal Entity Entity;

    private IEnumerable<string> GetAllBoxTypeNames => ConfigManager.GetAllBoxTypeNames();

    private IEnumerable<string> GetAllEnemyNames => ConfigManager.GetAllEnemyNames();

    private IEnumerable<string> GetAllFXTypeNames => ConfigManager.GetAllFXTypeNames();

    public EntityPassiveSkillAction Clone()
    {
        Type type = GetType();
        EntityPassiveSkillAction newAction = (EntityPassiveSkillAction) Activator.CreateInstance(type);
        ChildClone(newAction);
        return newAction;
    }

    protected virtual void ChildClone(EntityPassiveSkillAction newAction)
    {
    }

    public virtual void CopyDataFrom(EntityPassiveSkillAction srcData)
    {
    }
}
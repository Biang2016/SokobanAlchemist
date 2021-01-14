using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using UnityEngine;

[Serializable]
public abstract class BoxPassiveSkillAction : IClone<BoxPassiveSkillAction>
{
    protected virtual string Description => "箱子被动技能行为基类";

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

    public interface ITriggerEnterAction
    {
        void OnTriggerEnter(Collider collider);
    }

    public interface ITriggerStayAction
    {
        void OnTriggerStay(Collider collider);
    }

    public interface ITriggerExitAction
    {
        void OnTriggerExit(Collider collider);
    }

    internal Box Box;

    private IEnumerable<string> GetAllBoxTypeNames => ConfigManager.GetAllBoxTypeNames();

    private IEnumerable<string> GetAllEnemyNames => ConfigManager.GetAllEnemyNames();

    private IEnumerable<string> GetAllFXTypeNames => ConfigManager.GetAllFXTypeNames();

    public BoxPassiveSkillAction Clone()
    {
        Type type = GetType();
        BoxPassiveSkillAction newAction = (BoxPassiveSkillAction) Activator.CreateInstance(type);
        ChildClone(newAction);
        return newAction;
    }

    protected virtual void ChildClone(BoxPassiveSkillAction newAction)
    {
    }

    public virtual void CopyDataFrom(BoxPassiveSkillAction srcData)
    {
    }
}
using System;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class EntitySkillAction : IClone<EntitySkillAction>
{
    [ReadOnly]
    [HideInEditorMode]
    public uint InitWorldModuleGUID; // 创建时所属的世界模组GUID

    public abstract void OnRecycled();

    public virtual void Init(uint initWorldModuleGUID)
    {
        InitWorldModuleGUID = initWorldModuleGUID;
    }

    public virtual void UnInit()
    {
    }

    protected virtual string Description => "Entity被动技能行为基类";

    public interface IPureAction
    {
        void Execute();
    }

    public interface IEntityAction
    {
        void OnExert(Entity entity);
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

    public EntitySkillAction Clone()
    {
        Type type = GetType();
        EntitySkillAction newAction = (EntitySkillAction) Activator.CreateInstance(type);
        ChildClone(newAction);
        return newAction;
    }

    protected virtual void ChildClone(EntitySkillAction newAction)
    {
    }

    public virtual void CopyDataFrom(EntitySkillAction srcData)
    {
    }
}
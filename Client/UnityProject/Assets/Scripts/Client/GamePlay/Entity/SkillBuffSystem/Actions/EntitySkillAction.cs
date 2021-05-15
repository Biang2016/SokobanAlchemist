using System;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class EntitySkillAction : IClone<EntitySkillAction>
{
    [ShowInInspector]
    [HideInEditorMode]
    internal Entity Entity;

    [ReadOnly]
    [HideInEditorMode]
    public uint InitWorldModuleGUID; // 创建时所属的世界模组GUID

    [ReadOnly]
    [HideInEditorMode]
    public string InitStaticLayoutGUID = ""; // 创建时所属的静态布局GUID

    public abstract void OnRecycled();

    public virtual void Init(Entity entity)
    {
        Entity = entity;
        InitWorldModuleGUID = entity.InitWorldModuleGUID;
        InitStaticLayoutGUID = entity.CurrentEntityData.InitStaticLayoutGUID;
    }

    public virtual void UnInit()
    {
        Entity = null;
    }

    public virtual void OnUpdate(float deltaTime)
    {
    }

    protected virtual string Description => "Entity被动技能行为基类";

    public interface IAction : IClone<EntitySkillAction>
    {
    }

    public interface IPureAction : IAction
    {
        void Execute();
    }

    public interface IBuffAction : IAction
    {
        void ExecuteOnBuff(EntityBuff buff);
    }

    public interface IWorldGPAction : IAction
    {
        void ExecuteOnWorldGP(GridPos3D worldGP);
    }

    public interface IEntityAction : IAction
    {
        void ExecuteOnEntity(Entity entity);
    }

    public interface ICollideAction : IAction
    {
        void ExecuteOnCollide(Collision collision);
    }

    public interface ITriggerAction : IAction
    {
        void ExecuteOnTriggerEnter(Collider collider);
        void ExecuteOnTriggerStay(Collider collider);
        void ExecuteOnTriggerExit(Collider collider);
    }

    public EntitySkillAction Clone()
    {
        Type type = GetType();
        EntitySkillAction newAction = (EntitySkillAction) Activator.CreateInstance(type);
        newAction.InitWorldModuleGUID = InitWorldModuleGUID;
        newAction.InitStaticLayoutGUID = InitStaticLayoutGUID;
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
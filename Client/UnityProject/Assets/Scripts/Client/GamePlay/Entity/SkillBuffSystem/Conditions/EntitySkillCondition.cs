using System;
using System.Collections;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class EntitySkillCondition : IClone<EntitySkillCondition>
{
    [ShowInInspector]
    [HideInEditorMode]
    protected Entity Entity;

    public virtual void OnInit(Entity entity)
    {
        Entity = entity;
    }

    public virtual void OnUnInit()
    {
        Entity = null;
    }

    public virtual void OnTick(float tickInterval)
    {
    }

    public EntitySkillCondition Clone()
    {
        Type type = GetType();
        EntitySkillCondition cloneData = (EntitySkillCondition) Activator.CreateInstance(type);
        ChildClone(cloneData);
        return cloneData;
    }

    protected virtual void ChildClone(EntitySkillCondition cloneData)
    {
    }

    public virtual void CopyDataFrom(EntitySkillCondition srcData)
    {
    }

    public interface IPureCondition
    {
        bool OnCheckCondition();
    }

    public interface IWorldGPCondition
    {
        bool OnCheckConditionOnWorldGP(GridPos3D worldGP);
    }

    public interface IEntityCondition
    {
        bool OnCheckConditionOnEntity(Entity entity);
    }
}
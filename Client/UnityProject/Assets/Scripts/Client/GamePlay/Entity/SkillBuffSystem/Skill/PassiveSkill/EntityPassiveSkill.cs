using System;
using BiangLibrary.GameDataFormat.Grid;
using UnityEngine;

[Serializable]
public abstract class EntityPassiveSkill : EntitySkill
{
    #region Conditions

    public override void OnInit()
    {
        base.OnInit();
    }

    public override void OnUnInit()
    {
        base.OnUnInit();
    }

    public override void OnTick(float tickInterval)
    {
        base.OnTick(tickInterval);
    }

    public virtual void OnRegisterLevelEventID()
    {
    }

    public virtual void OnUnRegisterLevelEventID()
    {
    }

    public virtual void OnBeingLift(Entity entity)
    {
    }

    public virtual void OnBeingKicked(Entity entity)
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

    public virtual void OnEntityStatValueChange(EntityStatType entityStatType, int before, int after, int min, int max)
    {
    }

    public virtual void OnEntityPropertyValueChange(EntityPropertyType entityPropertyType, int before, int after)
    {
    }

    public virtual void OnFuelEntered(EntityFlamethrowerHelper.FlamethrowerFuelData fuelData)
    {
    }

    public virtual void OnPassGrid(GridPos3D gridGP)
    {
    }

    public virtual void OnKick()
    {
    }

    public virtual void OnKillEnemy()
    {
    }

    public virtual void OnKillBox()
    {
    }

    public virtual void OnGetDamaged(EntityBuff buff)
    {
    }

    #endregion

    protected override void ChildClone(EntitySkill cloneData)
    {
    }

    public override void CopyDataFrom(EntitySkill srcData)
    {
    }
}
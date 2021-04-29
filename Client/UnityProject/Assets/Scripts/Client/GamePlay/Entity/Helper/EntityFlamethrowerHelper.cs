using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

public class EntityFlamethrowerHelper : EntityMonoHelper, IEntityTriggerZoneHelper
{
    public EntityFlamethrowerFuelTrigger EntityFlamethrowerFuelTrigger;

    public List<EntityFlamethrower> EntityFlamethrowers = new List<EntityFlamethrower>();

    [LabelText("喷火器技能")]
    public EntitySkillSO FlamethrowerSkillSO;

    [LabelText("技能容器实时")]
    [ShowInInspector]
    [HideInEditorMode]
    public EntityPassiveSkill_Conditional FlamethrowerSkill;

    [LabelText("燃料数据实时")]
    [ShowInInspector]
    [HideInEditorMode]
    private FlamethrowerFuelData CurrentFlamethrowerFuelData;

    #region SFX

    [BoxGroup("SFX")]
    public AK.Wwise.Event OnStart_Fire_Small;

    [BoxGroup("SFX")]
    public AK.Wwise.Event OnStop_Fire_Small;

    [BoxGroup("SFX")]
    public AK.Wwise.Event OnStart_Fire_Medium;

    [BoxGroup("SFX")]
    public AK.Wwise.Event OnStop_Fire_Medium;

    [BoxGroup("SFX")]
    public AK.Wwise.Event OnStart_Fire_Big;

    [BoxGroup("SFX")]
    public AK.Wwise.Event OnStop_Fire_Big;

    [BoxGroup("SFX")]
    public AK.Wwise.Event OnStart_Ice;

    [BoxGroup("SFX")]
    public AK.Wwise.Event OnStop_Ice;

    [BoxGroup("SFX")]
    public AK.Wwise.Event OnStart_Lightning;

    [BoxGroup("SFX")]
    public AK.Wwise.Event OnStop_Lightning;

    private void StartFlameSound(FlameType flameType)
    {
        switch (flameType)
        {
            case FlameType.Fire_Small:
            {
                OnStart_Fire_Small?.Post(gameObject);
                break;
            }
            case FlameType.Fire_Medium:
            {
                OnStart_Fire_Medium?.Post(gameObject);
                break;
            }
            case FlameType.Fire_Big:
            {
                OnStart_Fire_Big?.Post(gameObject);
                break;
            }
            case FlameType.Ice:
            {
                OnStart_Ice?.Post(gameObject);
                break;
            }
            case FlameType.Lightning:
            {
                OnStart_Lightning?.Post(gameObject);
                break;
            }
        }
    }

    private void StopFlameSound(FlameType flameType)
    {
        switch (CurrentFlamethrowerFuelData.FlameType)
        {
            case FlameType.Fire_Small:
            {
                OnStop_Fire_Small?.Post(gameObject);
                break;
            }
            case FlameType.Fire_Medium:
            {
                OnStop_Fire_Medium?.Post(gameObject);
                break;
            }
            case FlameType.Fire_Big:
            {
                OnStop_Fire_Big?.Post(gameObject);
                break;
            }
            case FlameType.Ice:
            {
                OnStop_Ice?.Post(gameObject);
                break;
            }
            case FlameType.Lightning:
            {
                OnStop_Lightning?.Post(gameObject);
                break;
            }
        }
    }

    #endregion

    void Awake()
    {
        foreach (EntityFlamethrower ef in EntityFlamethrowers)
        {
            ef.EntityTriggerZone_Flame.IEntityTriggerZone = this;
        }

        EntityFlamethrowerFuelTrigger.Init();
        EntityFlamethrowerFuelTrigger.EnableTrigger(false);
        TurnOffFire(true);
    }

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
        EntityFlamethrowerFuelTrigger.EnableTrigger(false);
        TurnOffFire(false);
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
        FXDurationTick = 0;
        EntityFlamethrowerFuelTrigger.EnableTrigger(true);
    }

    public void OnMoving()
    {
        //EntityFlamethrowerFuelTrigger.EnableTrigger(false);
    }

    public void OnRigidbodyStop()
    {
        //EntityFlamethrowerFuelTrigger.EnableTrigger(true);
    }

    private float FXDurationTick = 0f;

    void FixedUpdate()
    {
        if (Entity.IsNotNullAndAlive() && FireOn)
        {
            FXDurationTick += Time.fixedDeltaTime;
            if (FXDurationTick > CurrentFlamethrowerFuelData.FlameDuration)
            {
                TurnOffFire(false);
            }
        }
    }

    private bool FireOn = false;

    public void TurnOnFire(FlamethrowerFuelData fuelData)
    {
        if (FireOn) StopFlameSound(CurrentFlamethrowerFuelData.FlameType);

        FXDurationTick = 0;
        FireOn = true;

        foreach (EntityFlamethrower ef in EntityFlamethrowers)
        {
            ef.EntityTriggerZone_Flame.Collider.enabled = true;
        }

        CurrentFlamethrowerFuelData = fuelData;

        // 将上一次设好的技能全部清空
        FlamethrowerSkill?.OnUnRegisterLevelEventID();
        FlamethrowerSkill?.OnUnInit();
        FlamethrowerSkill = (EntityPassiveSkill_Conditional) FlamethrowerSkillSO.EntitySkill.Clone(); // 再次创建空技能容器
        FlamethrowerSkill.RawEntitySkillActions = CurrentFlamethrowerFuelData.RawEntitySkillActions_ForFlamethrower;
        FlamethrowerSkill.Entity = Entity;
        FlamethrowerSkill.OnInit();
        FlamethrowerSkill.OnRegisterLevelEventID();

        foreach (EntityFlamethrower ef in EntityFlamethrowers)
        {
            ParticleSystem.MainModule main = ef.FirePS.main;

            // 改变火焰粒子长度
            main.startSpeed = new ParticleSystem.MinMaxCurve(fuelData.FlameLength * 0.9f, fuelData.FlameLength * 1.65f);
            // 改变火焰粒子颜色

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = ef.FirePS.colorOverLifetime;
            colorOverLifetime.color = CurrentFlamethrowerFuelData.FlameColor;

            // 改变烟雾位置
            ef.SmokePS.transform.localPosition = new Vector3(fuelData.FlameLength - 0.8f, ef.SmokePS.transform.localPosition.y, ef.SmokePS.transform.localPosition.z);

            // 改变伤害框大小
            Vector3 damageColliderCenter = ((BoxCollider) ef.EntityTriggerZone_Flame.Collider).center;
            Vector3 damageColliderSize = ((BoxCollider) ef.EntityTriggerZone_Flame.Collider).size;
            ((BoxCollider) ef.EntityTriggerZone_Flame.Collider).center = new Vector3(damageColliderCenter.x, damageColliderCenter.y, fuelData.FlameLength / 2f + 0.5f);
            ((BoxCollider) ef.EntityTriggerZone_Flame.Collider).size = new Vector3(damageColliderSize.x, damageColliderSize.y, fuelData.FlameLength - 0.5f);

            // 改变火焰喷射粒子密度
            ParticleSystem.EmissionModule emission = ef.FirePS.emission;
            emission.rateOverTime = fuelData.FlameLength / 2f * 15f;
            ParticleSystem.EmissionModule emission_smoke = ef.SmokePS.emission;
            emission_smoke.rateOverTime = Mathf.Sqrt(fuelData.FlameLength) * 2.5f;

            ef.FirePS.Play(true);
            ef.FireLight.gameObject.SetActive(true);
        }

        StartFlameSound(CurrentFlamethrowerFuelData.FlameType);
    }

    public void TurnOffFire(bool forced)
    {
        if (!forced && !FireOn) return;
        FireOn = false;

        if (FlamethrowerSkill != null)
        {
            FlamethrowerSkill.OnUnRegisterLevelEventID();
            FlamethrowerSkill.OnUnInit();
            FlamethrowerSkill = null;
        }

        if (CurrentFlamethrowerFuelData != null) StopFlameSound(CurrentFlamethrowerFuelData.FlameType);

        CurrentFlamethrowerFuelData = null;
        foreach (EntityFlamethrower ef in EntityFlamethrowers)
        {
            ef.EntityTriggerZone_Flame.Collider.enabled = false;
            ef.FirePS.Stop(true);
            ef.FireLight.gameObject.SetActive(false);
        }
    }

    // todo 此处有风险，两个Trigger进出次序如果很极限的话，有可能exit不触发就换技能了
    public void OnTriggerZoneEnter(Collider c)
    {
        FlamethrowerSkill?.OnTriggerZoneEnter(c);
    }

    public void OnTriggerZoneStay(Collider c)
    {
        FlamethrowerSkill?.OnTriggerZoneStay(c);
    }

    public void OnTriggerZoneExit(Collider c)
    {
        FlamethrowerSkill?.OnTriggerZoneExit(c);
    }

    [Serializable]
    public class FlamethrowerFuelData : IClone<FlamethrowerFuelData>
    {
        public FlameType FlameType;

        public int FlameLength;

        public int FlameDuration;

        public Gradient FlameColor;

        [SerializeReference]
        [LabelText("Flame Effect")]
        [ListDrawerSettings(ListElementLabelName = "Description")]
        public List<EntitySkillAction> RawEntitySkillActions_ForFlamethrower = new List<EntitySkillAction>(); // 干数据，禁修改

        public FlamethrowerFuelData Clone()
        {
            FlamethrowerFuelData newData = new FlamethrowerFuelData();
            newData.FlameType = FlameType;
            newData.FlameLength = FlameLength;
            newData.FlameDuration = FlameDuration;
            newData.FlameColor = FlameColor;
            newData.RawEntitySkillActions_ForFlamethrower = RawEntitySkillActions_ForFlamethrower.Clone<EntitySkillAction, EntitySkillAction>();
            return newData;
        }
    }
}

public enum FlameType
{
    Fire_Small,
    Fire_Medium,
    Fire_Big,
    Ice,
    Lightning
}
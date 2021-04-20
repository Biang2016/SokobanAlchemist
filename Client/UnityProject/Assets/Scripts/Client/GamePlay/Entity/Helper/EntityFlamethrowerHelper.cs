using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

public class EntityFlamethrowerHelper : EntityMonoHelper, IEntityTriggerZoneHelper
{
    public EntityFlamethrowerFuelTrigger EntityFlamethrowerFuelTrigger;

    public List<EntityFlamethrower> EntityFlamethrowers = new List<EntityFlamethrower>();

    [LabelText("技能容器")]
    public EntityPassiveSkill_Conditional FlamethrowerPassiveSkill_Raw;

    [LabelText("技能容器实时")]
    [ShowInInspector]
    [HideInEditorMode]
    public EntityPassiveSkill_Conditional FlamethrowerPassiveSkill;

    [LabelText("燃料数据实时")]
    [ShowInInspector]
    [HideInEditorMode]
    private FlamethrowerFuelData CurrentFlamethrowerFuelData;

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
        //if (FireOn) return; // todo 燃料叠加或替换？
        FXDurationTick = 0;
        FireOn = true;

        foreach (EntityFlamethrower ef in EntityFlamethrowers)
        {
            ef.EntityTriggerZone_Flame.Collider.enabled = true;
        }

        CurrentFlamethrowerFuelData = fuelData;

        // 将上一次设好的技能全部清空
        FlamethrowerPassiveSkill?.OnUnRegisterLevelEventID();
        FlamethrowerPassiveSkill?.OnUnInit();
        FlamethrowerPassiveSkill = (EntityPassiveSkill_Conditional) FlamethrowerPassiveSkill_Raw.Clone(); // 再次创建空技能容器
        FlamethrowerPassiveSkill.RawEntityPassiveSkillActions = CurrentFlamethrowerFuelData.RawEntityPassiveSkillActions_ForFlamethrower;
        FlamethrowerPassiveSkill.Entity = Entity;
        FlamethrowerPassiveSkill.OnInit();
        FlamethrowerPassiveSkill.OnRegisterLevelEventID();

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
    }

    public void TurnOffFire(bool forced)
    {
        if (!forced && !FireOn) return;
        FireOn = false;

        if (FlamethrowerPassiveSkill != null)
        {
            FlamethrowerPassiveSkill.OnUnRegisterLevelEventID();
            FlamethrowerPassiveSkill.OnUnInit();
            FlamethrowerPassiveSkill = null;
        }

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
        FlamethrowerPassiveSkill?.OnTriggerZoneEnter(c);
    }

    public void OnTriggerZoneStay(Collider c)
    {
        FlamethrowerPassiveSkill?.OnTriggerZoneStay(c);
    }

    public void OnTriggerZoneExit(Collider c)
    {
        FlamethrowerPassiveSkill?.OnTriggerZoneExit(c);
    }

    [Serializable]
    public class FlamethrowerFuelData : IClone<FlamethrowerFuelData>
    {
        [LabelText("火焰长度")]
        public int FlameLength;

        [LabelText("火焰持续时间")]
        public int FlameDuration;

        [LabelText("火焰色")]
        public Gradient FlameColor;

        [SerializeReference]
        [LabelText("火焰效果")]
        [ListDrawerSettings(ListElementLabelName = "Description")]
        public List<EntityPassiveSkillAction> RawEntityPassiveSkillActions_ForFlamethrower = new List<EntityPassiveSkillAction>(); // 干数据，禁修改

        public FlamethrowerFuelData Clone()
        {
            FlamethrowerFuelData newData = new FlamethrowerFuelData();
            newData.FlameLength = FlameLength;
            newData.FlameDuration = FlameDuration;
            newData.FlameColor = FlameColor;
            newData.RawEntityPassiveSkillActions_ForFlamethrower = RawEntityPassiveSkillActions_ForFlamethrower.Clone<EntityPassiveSkillAction, EntityPassiveSkillAction>();
            return newData;
        }
    }
}
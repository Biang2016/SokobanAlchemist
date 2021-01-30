using System;
using System.Collections.Generic;
using System.Linq;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

public class EntityFlamethrowerHelper : EntityMonoHelper, IEntityTriggerZone
{
    public Collider FuelTrigger;

    public ParticleSystem FirePS; // Main
    public ParticleSystem SmokePS;
    public ParticleSystem SparkPS;
    public Light FireLight;

    public EntityTriggerZone EntityTriggerZone_Flame;

    [LabelText("技能容器")]
    public EntityPassiveSkill_Conditional FlamethrowerPassiveSkill_Raw;

    [LabelText("技能容器实时")]
    [ShowInInspector]
    public EntityPassiveSkill_Conditional FlamethrowerPassiveSkill;

    [LabelText("燃料数据实时")]
    [ShowInInspector]
    private FlamethrowerFuelData CurrentFlamethrowerFuelData;

    void Awake()
    {
        EntityTriggerZone_Flame.IEntityTriggerZone = this;
        FuelTrigger.enabled = false;
        TurnOffFire(true);
    }

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
        FuelTrigger.enabled = false;
        TurnOffFire(false);
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
        FXDurationTick = 0;
        FuelTrigger.enabled = true;
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

    private void OnTriggerEnter(Collider collider)
    {
        if (!Entity.IsNotNullAndAlive()) return;
        Entity entity = collider.GetComponentInParent<Entity>();
        if (entity.IsNotNullAndAlive() && entity is Box box)
        {
            if (box.State == Box.States.BeingKicked) // 只有踢状态的箱子可以触发此功能
            {
                if (box.FrozenActor != null)
                {
                    // todo 特例，冻结敌人的箱子推入，还没想好逻辑
                }
                else
                {
                    // 从对象配置里面读取关联的被动技能行为，并拷贝作为本技能的行为
                    if (box.RawFlamethrowerFuelData?.RawEntityPassiveSkillActions_ForFlamethrower != null && box.RawFlamethrowerFuelData.RawEntityPassiveSkillActions_ForFlamethrower.Count > 0)
                    {
                        TurnOnFire(box.RawFlamethrowerFuelData.Clone());
                        box.DestroyBox();
                    }
                }
            }
        }
    }

    private bool FireOn = false;

    public void TurnOnFire(FlamethrowerFuelData fuelData)
    {
        //if (FireOn) return; // todo 燃料叠加或替换？
        FXDurationTick = 0;
        FireOn = true;

        EntityTriggerZone_Flame.Collider.enabled = true;
        CurrentFlamethrowerFuelData = fuelData;

        // 将上一次设好的技能全部清空
        FlamethrowerPassiveSkill?.OnUnRegisterLevelEventID();
        FlamethrowerPassiveSkill?.OnUnInit();
        FlamethrowerPassiveSkill = (EntityPassiveSkill_Conditional) FlamethrowerPassiveSkill_Raw.Clone(); // 再次创建空技能容器
        FlamethrowerPassiveSkill.RawEntityPassiveSkillActions = CurrentFlamethrowerFuelData.RawEntityPassiveSkillActions_ForFlamethrower;
        FlamethrowerPassiveSkill.Entity = Entity;
        FlamethrowerPassiveSkill.OnInit();
        FlamethrowerPassiveSkill.OnRegisterLevelEventID();

        ParticleSystem.MainModule main = FirePS.main;

        // 改变火焰粒子长度
        main.startSpeed = new ParticleSystem.MinMaxCurve(fuelData.FlameLength * 0.9f, fuelData.FlameLength * 1.65f);
        // 改变火焰粒子颜色

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = FirePS.colorOverLifetime;
        colorOverLifetime.color = CurrentFlamethrowerFuelData.FlameColor;

        // 改变烟雾位置
        SmokePS.transform.localPosition = new Vector3(fuelData.FlameLength - 0.8f, SmokePS.transform.localPosition.y, SmokePS.transform.localPosition.z);

        // 改变伤害框大小
        Vector3 damageColliderCenter = ((BoxCollider) EntityTriggerZone_Flame.Collider).center;
        Vector3 damageColliderSize = ((BoxCollider) EntityTriggerZone_Flame.Collider).size;
        ((BoxCollider) EntityTriggerZone_Flame.Collider).center = new Vector3(damageColliderCenter.x, damageColliderCenter.y, fuelData.FlameLength / 2f + 0.5f);
        ((BoxCollider) EntityTriggerZone_Flame.Collider).size = new Vector3(damageColliderSize.x, damageColliderSize.y, fuelData.FlameLength - 0.5f);

        // 改变火焰喷射粒子密度
        ParticleSystem.EmissionModule emission = FirePS.emission;
        emission.rateOverTime = fuelData.FlameLength / 2f * 15f;
        ParticleSystem.EmissionModule emission_smoke = SmokePS.emission;
        emission_smoke.rateOverTime = Mathf.Sqrt(fuelData.FlameLength) * 2.5f;

        FirePS.Play(true);
        FireLight.gameObject.SetActive(true);
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
        EntityTriggerZone_Flame.Collider.enabled = false;

        FirePS.Stop(true);
        FireLight.gameObject.SetActive(false);
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
            newData.RawEntityPassiveSkillActions_ForFlamethrower = RawEntityPassiveSkillActions_ForFlamethrower.Clone();
            return newData;
        }
    }
}
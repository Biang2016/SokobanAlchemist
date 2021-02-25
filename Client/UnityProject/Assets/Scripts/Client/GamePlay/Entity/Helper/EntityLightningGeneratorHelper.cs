using System;
using System.Collections.Generic;
using System.Linq;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class EntityLightningGeneratorHelper : EntityMonoHelper, IEntityTriggerZoneHelper
{
    [LabelText("技能容器")]
    public EntityPassiveSkill_Conditional LightningPassiveSkill;

    private List<EntityLightning> EntityLightnings = new List<EntityLightning>();

    [SerializeField]
    private Transform LightningStartPivot;

    void Awake()
    {
    }

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
        foreach (EntityLightning lightning in EntityLightnings)
        {
            lightning.PoolRecycle();
        }

        EntityLightnings.Clear();
        LightningPassiveSkill.Entity = null;
        LightningPassiveSkill.OnUnRegisterLevelEventID();
        LightningPassiveSkill.OnUnInit();
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
        LightningPassiveSkill.Entity = Entity;
        LightningPassiveSkill.OnInit();
        LightningPassiveSkill.OnRegisterLevelEventID();
    }

    private List<EntityLightning> cached_removeLightnings = new List<EntityLightning>(4);

    void FixedUpdate()
    {
        if (Entity.IsNotNullAndAlive())
        {
            cached_removeLightnings.Clear();

            // 删除无法连接的闪电
            foreach (EntityLightning lightning in EntityLightnings)
            {
                if (!lightning.EndGeneratorHelper.Entity.IsNotNullAndAlive())
                {
                    cached_removeLightnings.Add(lightning);
                }
                else
                {
                    if ((lightning.EndGeneratorHelper.LightningStartPivot.position - LightningStartPivot.position).magnitude > 5f)
                    {
                        cached_removeLightnings.Add(lightning);
                    }
                }
            }

            foreach (EntityLightning generator in cached_removeLightnings)
            {
                EntityLightnings.Remove(generator);
                generator.PoolRecycle();
            }

            // 寻找新的连接
            Collider[] colliders = Physics.OverlapSphere(LightningStartPivot.position, 5f, LayerManager.Instance.LayerMask_BoxIndicator | LayerManager.Instance.LayerMask_HitBox_Enemy | LayerManager.Instance.LayerMask_HitBox_Player);
            foreach (Collider c in colliders)
            {
                Entity entity = c.GetComponentInParent<Entity>();
                if (entity != null && entity != Entity && entity.EntityLightningGeneratorHelpers != null && entity.EntityLightningGeneratorHelpers.Count > 0)
                {
                    foreach (EntityLightningGeneratorHelper helper in entity.EntityLightningGeneratorHelpers)
                    {
                        bool alreadyConnect = false;
                        foreach (EntityLightning lightning in EntityLightnings)
                        {
                            if (lightning.EndGeneratorHelper == helper)
                            {
                                alreadyConnect = true;
                                break;
                            }
                        }

                        if (!alreadyConnect)
                        {
                            if ((helper.LightningStartPivot.position - LightningStartPivot.position).magnitude <= 5f)
                            {
                                EntityLightning lightning = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.EntityLightning].AllocateGameObject<EntityLightning>(transform);
                                lightning.Initialize(LightningStartPivot, helper.LightningStartPivot);
                                lightning.EntityTriggerZone_Lightning.IEntityTriggerZone = this;
                                lightning.StartGeneratorHelper = this;
                                lightning.EndGeneratorHelper = helper;
                                EntityLightnings.Add(lightning);
                            }
                        }
                    }
                }
            }
        }
    }

    // todo 此处有风险，两个Trigger进出次序如果很极限的话，有可能exit不触发就换技能了
    public void OnTriggerZoneEnter(Collider c)
    {
        LightningPassiveSkill?.OnTriggerZoneEnter(c);
    }

    public void OnTriggerZoneStay(Collider c)
    {
        LightningPassiveSkill?.OnTriggerZoneStay(c);
    }

    public void OnTriggerZoneExit(Collider c)
    {
        LightningPassiveSkill?.OnTriggerZoneExit(c);
    }
}
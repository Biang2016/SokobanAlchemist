using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class EntityLightningGeneratorHelper : EntityMonoHelper, IEntityTriggerZoneHelper
{
    [LabelText("闪电技能")]
    public EntitySkillSO LightningSkillSO;

    private EntityPassiveSkill LightningSkill;

    private List<EntityLightning> EntityLightnings = new List<EntityLightning>();

    [SerializeField]
    private Transform LightningStartPivot;

    void Awake()
    {
        LightningSkill = (EntityPassiveSkill) LightningSkillSO.EntitySkill.Clone();
    }

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
        foreach (EntityLightning lightning in EntityLightnings)
        {
            lightning.PoolRecycle();
        }

        EntityLightnings.Clear();
        LightningSkill.Entity = null;
        LightningSkill.OnUnRegisterLevelEventID();
        LightningSkill.OnUnInit();
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
        LightningSkill.Entity = Entity;
        LightningSkill.OnInit();
        LightningSkill.OnRegisterLevelEventID();
    }

    private List<EntityLightning> cached_removeLightnings = new List<EntityLightning>(4);

    private static Collider[] cached_Colliders = new Collider[256];

    private int FixedUpdateInterval = 10;
    private int FixedUpdateIntervalTick = 0;

    void FixedUpdate()
    {
        if (FixedUpdateIntervalTick < FixedUpdateInterval)
        {
            FixedUpdateIntervalTick++;
        }
        else
        {
            FixedUpdateIntervalTick = 0;
            if (Entity.IsNotNullAndAlive() && !Entity.IsFrozen)
            {
                cached_removeLightnings.Clear();

                // 删除无法连接的闪电
                foreach (EntityLightning lightning in EntityLightnings)
                {
                    if (!lightning.EndGeneratorHelper.Entity.IsNotNullAndAlive() || !lightning.EndGeneratorHelper.gameObject.activeInHierarchy || lightning.EndGeneratorHelper.Entity.IsFrozen)
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
                int length = Physics.OverlapSphereNonAlloc(LightningStartPivot.position, 5f, cached_Colliders, LayerManager.Instance.LayerMask_BoxIndicator | LayerManager.Instance.LayerMask_ActorIndicator_Enemy | LayerManager.Instance.LayerMask_ActorIndicator_Player);
                for (int i = 0; i < length; i++)
                {
                    Collider c = cached_Colliders[i];
                    Entity entity = c.GetComponentInParent<Entity>();
                    if (entity != null && entity != Entity && entity.EntityLightningGeneratorHelpers != null && entity.EntityLightningGeneratorHelpers.Count > 0)
                    {
                        if (entity.GUID < Entity.GUID) continue; // 每个电塔只连接GUID更大的
                        foreach (EntityLightningGeneratorHelper helper in entity.EntityLightningGeneratorHelpers)
                        {
                            if (!helper.gameObject.activeInHierarchy) continue;
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
            else
            {
                cached_removeLightnings.Clear();
                foreach (EntityLightning generator in EntityLightnings)
                {
                    cached_removeLightnings.Add(generator);
                }

                foreach (EntityLightning generator in cached_removeLightnings)
                {
                    EntityLightnings.Remove(generator);
                    generator.PoolRecycle();
                }
            }
        }
    }

    // todo 此处有风险，两个Trigger进出次序如果很极限的话，有可能exit不触发就换技能了
    public void OnTriggerZoneEnter(Collider c)
    {
        if (Entity.IsNotNullAndAlive())
        {
            LightningSkill?.OnTriggerZoneEnter(c);
        }
    }

    public void OnTriggerZoneStay(Collider c)
    {
        if (Entity.IsNotNullAndAlive())
        {
            LightningSkill?.OnTriggerZoneStay(c);
        }
    }

    public void OnTriggerZoneExit(Collider c)
    {
        if (Entity.IsNotNullAndAlive())
        {
            LightningSkill?.OnTriggerZoneExit(c);
        }
    }
}
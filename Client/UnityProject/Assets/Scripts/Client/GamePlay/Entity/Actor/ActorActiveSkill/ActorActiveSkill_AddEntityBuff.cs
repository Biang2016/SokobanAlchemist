using System;
using System.Collections;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public class ActorActiveSkill_AddEntityBuff : ActorActiveSkill_AreaCast, ISerializationCallbackReceiver
{
    protected override string Description => "给区域内角色或箱子施加Buff";

    [NonSerialized]
    [ShowInInspector]
    [BoxGroup("Buff")]
    [LabelText("Buff列表")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<EntityBuff> RawEntityBuffs = new List<EntityBuff>(); // 干数据，禁修改

    [HideInInspector]
    public byte[] RawEntityBuffData;

    public void OnBeforeSerialize()
    {
        if (RawEntityBuffs == null) RawEntityBuffs = new List<EntityBuff>();
        RawEntityBuffData = SerializationUtility.SerializeValue(RawEntityBuffs, DataFormat.JSON);
    }

    public void OnAfterDeserialize()
    {
        RawEntityBuffs = SerializationUtility.DeserializeValue<List<EntityBuff>>(RawEntityBuffData, DataFormat.JSON);
    }

    public override void OnInit()
    {
        base.OnInit();
    }

    public override void OnUnInit()
    {
        base.OnUnInit();
    }

    protected override IEnumerator Cast(float castDuration)
    {
        int targetCount = 0;
        HashSet<uint> entityGUIDSet = new HashSet<uint>();
        bool needBreak = false;
        foreach (GridPos3D gp in RealSkillEffectGPs)
        {
            Collider[] colliders_player = Physics.OverlapSphere(gp, 0.3f, LayerManager.Instance.GetTargetActorLayerMask(Actor.Camp, TargetCamp));
            foreach (Collider c in colliders_player)
            {
                Actor actor = c.GetComponentInParent<Actor>();
                if (actor != null && !entityGUIDSet.Contains(actor.GUID))
                {
                    entityGUIDSet.Add(actor.GUID);
                    actor.ActorStatPropSet.FiringValue.Value += GetValue(ActorSkillPropertyType.Attach_FiringValue);
                    actor.ActorStatPropSet.FrozenValue.Value += GetValue(ActorSkillPropertyType.Attach_FrozenValue);
                    foreach (EntityBuff buff in RawEntityBuffs)
                    {
                        if (buff is ActorBuff actorBuff)
                        {
                            actor.ActorBuffHelper.AddBuff(actorBuff.Clone());
                        }
                    }

                    targetCount++;
                    if (targetCount > GetValue(ActorSkillPropertyType.MaxTargetCount))
                    {
                        needBreak = true;
                        break;
                    }
                }
            }

            if (needBreak) break;
            Collider[] colliders_box = Physics.OverlapSphere(gp, 0.3f, LayerManager.Instance.LayerMask_BoxIndicator);
            foreach (Collider c in colliders_box)
            {
                Box box = c.GetComponentInParent<Box>();
                if (box != null && !entityGUIDSet.Contains(box.GUID))
                {
                    entityGUIDSet.Add(box.GUID);
                    box.BoxStatPropSet.FiringValue.Value += GetValue(ActorSkillPropertyType.Attach_FiringValue);
                    box.BoxStatPropSet.FrozenValue.Value += GetValue(ActorSkillPropertyType.Attach_FrozenValue);
                    foreach (EntityBuff buff in RawEntityBuffs)
                    {
                        if (buff is BoxBuff boxBuff)
                        {
                            box.BoxBuffHelper.AddBuff(boxBuff.Clone());
                        }
                    }

                    targetCount++;
                    if (targetCount > GetValue(ActorSkillPropertyType.MaxTargetCount))
                    {
                        needBreak = true;
                        break;
                    }
                }
            }

            if (needBreak) break;
        }

        yield return base.Cast(castDuration);
    }

    protected override void ChildClone(ActorActiveSkill cloneData)
    {
        base.ChildClone(cloneData);
        ActorActiveSkill_AddEntityBuff newAAS = (ActorActiveSkill_AddEntityBuff) cloneData;
        newAAS.RawEntityBuffs = RawEntityBuffs.Clone();
    }

    public override void CopyDataFrom(ActorActiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        ActorActiveSkill_AddEntityBuff srcAAS = (ActorActiveSkill_AddEntityBuff) srcData;
        RawEntityBuffs = srcAAS.RawEntityBuffs.Clone();
    }
}
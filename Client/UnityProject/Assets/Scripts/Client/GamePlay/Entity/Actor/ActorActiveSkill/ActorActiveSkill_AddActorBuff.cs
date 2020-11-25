using System;
using System.Collections.Generic;
using BiangStudio.CloneVariant;
using BiangStudio.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public class ActorActiveSkill_AddActorBuff : ActorActiveSkill_AreaCast, ISerializationCallbackReceiver
{
    protected override string Description => "给区域内角色施加Buff";

    [NonSerialized]
    [ShowInInspector]
    [BoxGroup("Buff")]
    [LabelText("Buff列表")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<ActorBuff> RawActorDefaultBuffs = new List<ActorBuff>(); // 干数据，禁修改

    [HideInInspector]
    public byte[] RawActorDefaultBuffData;

    public void OnBeforeSerialize()
    {
        if (RawActorDefaultBuffs == null) RawActorDefaultBuffs = new List<ActorBuff>();
        RawActorDefaultBuffData = SerializationUtility.SerializeValue(RawActorDefaultBuffs, DataFormat.JSON);
    }

    public void OnAfterDeserialize()
    {
        RawActorDefaultBuffs = SerializationUtility.DeserializeValue<List<ActorBuff>>(RawActorDefaultBuffData, DataFormat.JSON);
    }

    public override void OnInit()
    {
        base.OnInit();
    }

    public override void OnUnInit()
    {
        base.OnUnInit();
    }

    protected override void Cast()
    {
        base.Cast();
        int targetCount = 0;
        HashSet<uint> actorGUIDSet = new HashSet<uint>();
        foreach (GridPos3D gp in RealSkillEffectGPs)
        {
            Collider[] colliders = Physics.OverlapSphere(gp.ToVector3(), 0.3f, LayerManager.Instance.GetTargetActorLayerMask(Actor.Camp, TargetCamp));
            foreach (Collider c in colliders)
            {
                Actor actor = c.GetComponentInParent<Actor>();
                if (actor != null && !actorGUIDSet.Contains(actor.GUID))
                {
                    actorGUIDSet.Add(actor.GUID);
                    foreach (ActorBuff buff in RawActorDefaultBuffs)
                    {
                        actor.ActorBuffHelper.AddBuff(buff.Clone());
                    }

                    targetCount++;
                    if (targetCount >= MaxTargetCount) return;
                }
            }
        }
    }

    protected override void ChildClone(ActorActiveSkill newAS)
    {
        base.ChildClone(newAS);
        ActorActiveSkill_AddActorBuff asAddActorBuff = (ActorActiveSkill_AddActorBuff) newAS;
        asAddActorBuff.RawActorDefaultBuffs = RawActorDefaultBuffs.Clone();
    }

    public override void CopyDataFrom(ActorActiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        ActorActiveSkill_AddActorBuff asAddActorBuff = (ActorActiveSkill_AddActorBuff) srcData;
        RawActorDefaultBuffs = asAddActorBuff.RawActorDefaultBuffs.Clone();
    }
}
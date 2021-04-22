using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntitySkillAction_RadiusAddEntitiesBuff : EntitySkillAction, EntitySkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "给范围内Entity施加Buff";

    [LabelText("判定半径")]
    public float AddBuffRadius = 2;

    [LabelText("采用精准Grid距离")]
    public bool ExactGPDistance = true;

    [LabelText("生效于相对阵营")]
    public RelativeCamp EffectiveOnRelativeCamp;

    [SerializeReference]
    [LabelText("作用效果")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<EntityBuff> RawEntityBuffs = new List<EntityBuff>(); // 干数据，禁修改

    public void Execute()
    {
        HashSet<uint> entityGUIDSet = new HashSet<uint>();
        if (Entity is Box box)
        {
            foreach (GridPos3D offset in box.GetEntityOccupationGPs_Rotated())
            {
                Vector3 boxIndicatorPos = Entity.transform.position + offset;
                BattleManager.Instance.AddBuffToEntities(boxIndicatorPos, box.LastInteractActor.IsNotNullAndAlive() ? box.LastInteractActor.Camp : box.Camp, AddBuffRadius, ExactGPDistance, EffectiveOnRelativeCamp, RawEntityBuffs, entityGUIDSet);
            }
        }
        else if (Entity is Actor actor)
        {
            BattleManager.Instance.AddBuffToEntities(actor.transform.position, actor.Camp, AddBuffRadius, ExactGPDistance, EffectiveOnRelativeCamp, RawEntityBuffs, entityGUIDSet);
        }
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_RadiusAddEntitiesBuff action = ((EntitySkillAction_RadiusAddEntitiesBuff) newAction);
        action.AddBuffRadius = AddBuffRadius;
        action.ExactGPDistance = ExactGPDistance;
        action.EffectiveOnRelativeCamp = EffectiveOnRelativeCamp;
        action.RawEntityBuffs = RawEntityBuffs.Clone<EntityBuff, EntityBuff>();
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_RadiusAddEntitiesBuff action = ((EntitySkillAction_RadiusAddEntitiesBuff) srcData);
        AddBuffRadius = action.AddBuffRadius;
        ExactGPDistance = action.ExactGPDistance;
        EffectiveOnRelativeCamp = action.EffectiveOnRelativeCamp;

        if (RawEntityBuffs.Count != action.RawEntityBuffs.Count)
        {
            Debug.LogError("EntitySkillAction_RadiusAddEntitiesBuff CopyDataFrom RawEntityBuffs数量不一致");
        }
        else
        {
            for (int i = 0; i < RawEntityBuffs.Count; i++)
            {
                RawEntityBuffs[i].CopyDataFrom(action.RawEntityBuffs[i]);
            }
        }
    }
}
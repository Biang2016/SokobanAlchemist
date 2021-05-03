using System;
using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class EntitySkillAction_DropSkillScroll : EntitySkillAction, EntitySkillAction.IPureAction
{
    [LabelText("技能卷轴类型概率")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<SkillScrollProbability> SkillScrollProbabilityList = new List<SkillScrollProbability>();

    public override void OnRecycled()
    {
    }

    protected override string Description => "掉落技能卷轴";

    private static List<EntitySkill> cached_SkillScrollGUIDList = new List<EntitySkill>(16);

    public void Execute()
    {
        WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(Entity.WorldGP);
        if (module)
        {
            cached_SkillScrollGUIDList.Clear();
            foreach (SkillScrollProbability skillScrollProbability in SkillScrollProbabilityList)
            {
                float dropProbability = Random.Range(skillScrollProbability.ProbabilityMin, skillScrollProbability.ProbabilityMax);
                EntitySkill rawEntitySkill = skillScrollProbability.GetRandomRawSkill();
                if (rawEntitySkill != null)
                {
                    if (!BattleManager.Instance.Player1.HasLearnedSkill(rawEntitySkill.SkillGUID))
                    {
                        while (dropProbability > 1)
                        {
                            dropProbability -= 1;
                            cached_SkillScrollGUIDList.Add(rawEntitySkill);
                        }

                        if (dropProbability.ProbabilityBool())
                        {
                            cached_SkillScrollGUIDList.Add(rawEntitySkill);
                        }
                    }
                }
            }

            int dropConeAngle = 0;
            if (cached_SkillScrollGUIDList.Count == 1) dropConeAngle = 0;
            else if (cached_SkillScrollGUIDList.Count <= 4) dropConeAngle = 15;
            else if (cached_SkillScrollGUIDList.Count <= 10) dropConeAngle = 30;
            else dropConeAngle = 45;

            foreach (EntitySkill rawEntitySkill in cached_SkillScrollGUIDList)
            {
                ushort scrollBoxTypeIndex = ConfigManager.GetTypeIndex(TypeDefineType.Box, rawEntitySkill.SkillScrollType.TypeName);
                GridPos3D worldGP = Entity.transform.position.ToGridPos3D();
                if (WorldManager.Instance.CurrentWorld.GenerateEntityOnWorldGPWithoutOccupy(scrollBoxTypeIndex, (GridPosR.Orientation) Random.Range(0, 4), worldGP, out Entity dropEntity))
                {
                    Vector2 horizontalVel = Random.insideUnitCircle.normalized * Mathf.Tan(dropConeAngle * Mathf.Deg2Rad);
                    Vector3 dropVel = Vector3.up + new Vector3(horizontalVel.x, 0, horizontalVel.y);
                    Box dropBox = (Box) dropEntity;

                    bool sucChanged = false;
                    foreach (EntityPassiveSkill eps in dropBox.EntityPassiveSkills)
                    {
                        if (eps is EntityPassiveSkill_Conditional conditional)
                        {
                            foreach (EntitySkillAction action in conditional.EntitySkillActions)
                            {
                                if (action is EntitySkillAction_TriggerZoneAction triggerZoneAction)
                                {
                                    foreach (EntitySkillAction stayAction in triggerZoneAction.EntityActions_Enter)
                                    {
                                        if (stayAction is EntitySkillAction_ShowLearnSkillPanel learnSkillAction)
                                        {
                                            learnSkillAction.SkillGUID = rawEntitySkill.SkillGUID;
                                            sucChanged = true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (sucChanged)
                    {
                        dropBox.DropOutFromEntity(dropVel.normalized * ClientGameManager.Instance.dropSkillScrollSpeed); // 抛射速度写死
                    }
                    else
                    {
                        dropBox.DestroySelf();
                    }
                }
            }
        }
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_DropSkillScroll action = ((EntitySkillAction_DropSkillScroll) newAction);
        action.SkillScrollProbabilityList = SkillScrollProbabilityList.Clone<SkillScrollProbability, SkillScrollProbability>();
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_DropSkillScroll action = ((EntitySkillAction_DropSkillScroll) srcData);
        SkillScrollProbabilityList = action.SkillScrollProbabilityList.Clone<SkillScrollProbability, SkillScrollProbability>();
    }
}
using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntityPassiveSkillAction_RadiusAddEntitiesBuff : EntityPassiveSkillAction, EntityPassiveSkillAction.IPureAction
{
    protected override string Description => "给范围内Entity施加Buff";

    [LabelText("判定半径")]
    public float AddBuffRadius = 2;

    [LabelText("作用于箱子")]
    public bool EnableToBoxes;

    [LabelText("作用于角色")]
    public bool EnableToActors;

    [ShowIf("EnableToActors")]
    [LabelText("生效于相对阵营")]
    public RelativeCamp EffectiveOnRelativeCamp;

    [SerializeReference]
    [LabelText("作用效果")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<EntityBuff> EntityBuffs = new List<EntityBuff>();

    public void Execute()
    {
        if (EnableToBoxes)
        {
            HashSet<uint> boxList = new HashSet<uint>();
            if (Entity is Box box)
            {
                foreach (GridPos3D offset in box.GetBoxOccupationGPs_Rotated())
                {
                    Vector3 boxIndicatorPos = Entity.transform.position + offset;
                    ExertOnBoxes(boxIndicatorPos);
                }
            }
            else if (Entity is Actor actor)
            {
                ExertOnBoxes(actor.transform.position);
            }

            void ExertOnBoxes(Vector3 center)
            {
                Collider[] colliders = Physics.OverlapSphere(center, AddBuffRadius, LayerManager.Instance.LayerMask_BoxIndicator);
                foreach (Collider collider in colliders)
                {
                    if ((collider.transform.position - center).magnitude > AddBuffRadius) continue;
                    Box targetBox = collider.gameObject.GetComponentInParent<Box>();
                    if (targetBox != null && !boxList.Contains(targetBox.GUID))
                    {
                        boxList.Add(targetBox.GUID);
                        foreach (EntityBuff entityBuff in EntityBuffs)
                        {
                            if (!targetBox.EntityBuffHelper.AddBuff(entityBuff.Clone()))
                            {
                                Debug.Log($"Failed to AddBuff: {entityBuff.GetType().Name} to {targetBox.name}");
                            }
                        }
                    }
                }
            }
        }

        if (EnableToActors)
        {
            HashSet<uint> actorList = new HashSet<uint>();
            if (Entity is Box box)
            {
                foreach (GridPos3D offset in box.GetBoxOccupationGPs_Rotated())
                {
                    Vector3 boxIndicatorPos = Entity.transform.position + offset;
                    ExertOnActors(boxIndicatorPos, box.LastTouchActor);
                }
            }
            else if (Entity is Actor actor)
            {
                ExertOnActors(actor.transform.position, actor);
            }

            void ExertOnActors(Vector3 center, Actor executeActor)
            {
                Collider[] colliders = Physics.OverlapSphere(center, AddBuffRadius, LayerManager.Instance.LayerMask_HitBox_Enemy | LayerManager.Instance.LayerMask_HitBox_Player);
                foreach (Collider collider in colliders)
                {
                    Actor actor = collider.gameObject.GetComponentInParent<Actor>();
                    if (actor != null && !actorList.Contains(actor.GUID))
                    {
                        actorList.Add(actor.GUID);
                        if (executeActor != null)
                        {
                            if (EffectiveOnRelativeCamp == RelativeCamp.FriendCamp && !actor.IsSameCampOf(executeActor))
                            {
                                continue;
                            }
                            else if (EffectiveOnRelativeCamp == RelativeCamp.OpponentCamp && !actor.IsOpponentCampOf(executeActor))
                            {
                                continue;
                            }
                            else if (EffectiveOnRelativeCamp == RelativeCamp.NeutralCamp && !actor.IsNeutralCampOf(executeActor))
                            {
                                continue;
                            }
                            else if (EffectiveOnRelativeCamp == RelativeCamp.AllCamp)
                            {
                            }
                            else if (EffectiveOnRelativeCamp == RelativeCamp.None)
                            {
                                continue;
                            }
                        }

                        foreach (EntityBuff entityBuff in EntityBuffs)
                        {
                            if (!actor.EntityBuffHelper.AddBuff(entityBuff.Clone()))
                            {
                                Debug.Log($"Failed to AddBuff: {entityBuff.GetType().Name} to {actor.name}");
                            }
                        }
                    }
                }
            }
        }
    }

    protected override void ChildClone(EntityPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        EntityPassiveSkillAction_RadiusAddEntitiesBuff action = ((EntityPassiveSkillAction_RadiusAddEntitiesBuff) newAction);
        action.EntityBuffs = EntityBuffs.Clone();
        action.EnableToBoxes = EnableToBoxes;
        action.EnableToActors = EnableToActors;
        action.EffectiveOnRelativeCamp = EffectiveOnRelativeCamp;
        action.AddBuffRadius = AddBuffRadius;
    }

    public override void CopyDataFrom(EntityPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntityPassiveSkillAction_RadiusAddEntitiesBuff action = ((EntityPassiveSkillAction_RadiusAddEntitiesBuff) srcData);
        EntityBuffs = action.EntityBuffs.Clone();
        EnableToBoxes = action.EnableToBoxes;
        EnableToActors = action.EnableToActors;
        EffectiveOnRelativeCamp = action.EffectiveOnRelativeCamp;
        AddBuffRadius = action.AddBuffRadius;
    }
}
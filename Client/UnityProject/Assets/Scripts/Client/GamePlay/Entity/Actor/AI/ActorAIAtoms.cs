using System.Collections;
using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.GameDataFormat.Grid;
using FlowCanvas.Nodes;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;
using WaitUntil = FlowCanvas.Nodes.WaitUntil;

public static class ActorAIAtoms
{
    #region 状态

    [Category("敌兵/状态")]
    [Name("更改状态")]
    [Description("更改状态")]
    public class BT_Enemy_SetEnemyBehaviourState : BTNode
    {
        public override string name => $"状态设为 [{ActorBehaviourState.value}]";

        [Name("更改为状态")]
        public BBParameter<Actor.ActorBehaviourStates> ActorBehaviourState;

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return Status.Failure;
            Actor.ActorBehaviourState = ActorBehaviourState.value;
            return Status.Success;
        }
    }

    [Category("敌兵/状态")]
    [Name("检查行为状态")]
    [Description("检查行为状态")]
    public class BT_Enemy_IsJumping_ConditionTask : ConditionTask
    {
        [Name("行为状态")]
        public BBParameter<Actor.ActorBehaviourStates> ActorBehaviourState;

        protected override string info => $"是{ActorBehaviourState.value}状态";

        protected override bool OnCheck()
        {
            return Actor.ActorBehaviourState == ActorBehaviourState.value;
        }
    }

    #endregion

    #region 目标搜索

    [Category("敌兵/目标搜索")]
    [Name("是否有目标")]
    [Description("是否有目标")]
    public class BT_Enemy_CheckHasTarget : ConditionTask
    {
        protected override string info
        {
            get
            {
                string targetEntityTypeDesc = "";
                if (TargetEntityTypes.value != null)
                {
                    foreach (ActorAIAgent.TargetEntityType targetEntityType in TargetEntityTypes.value)
                    {
                        targetEntityTypeDesc += "[" + targetEntityType + "]";
                    }
                }

                if (NeedEntity.value)
                {
                    targetEntityTypeDesc += "实体";
                }

                return $"有{targetEntityTypeDesc}目标";
            }
        }

        [Name("哪种目标")]
        public BBParameter<List<ActorAIAgent.TargetEntityType>> TargetEntityTypes;

        [Name("是否实体")]
        public BBParameter<bool> NeedEntity;

        protected override bool OnCheck()
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return false;
            bool res = true;
            if (TargetEntityTypes.value != null)
            {
                foreach (ActorAIAgent.TargetEntityType targetEntityType in TargetEntityTypes.value)
                {
                    if (NeedEntity.value)
                    {
                        res &= Actor.ActorAIAgent.AIAgentTargetDict[targetEntityType].TargetEntity.IsNotNullAndAlive();
                    }
                    else
                    {
                        res &= Actor.ActorAIAgent.AIAgentTargetDict[targetEntityType].HasTarget;
                    }
                }
            }

            return res;
        }
    }

    [Category("敌兵/目标搜索")]
    [Name("搜索并设置目标")]
    [Description("搜索并设置目标")]
    public class BT_Enemy_SetTargetEntity : BTNode
    {
        public override string name
        {
            get
            {
                string actorDesc = string.IsNullOrWhiteSpace(ActorTypeName.value) ? "全部角色" : ActorTypeName.value;
                string targetEntityTypeDesc = "";
                if (TargetEntityTypes.value != null)
                {
                    foreach (ActorAIAgent.TargetEntityType targetEntityType in TargetEntityTypes.value)
                    {
                        targetEntityTypeDesc += "[" + targetEntityType + "]";
                    }
                }

                return $"范围{string.Format(SearchRadius.value.ToString("F1"))}中搜索{actorDesc}，并设置为{targetEntityTypeDesc}目标";
            }
        }

        [Name("搜索范围")]
        public BBParameter<float> SearchRadius;

        [Name("相对阵营")]
        public BBParameter<RelativeCamp> RelativeCamp;

        [Name("角色类型指定(空则都可)")]
        public BBParameter<string> ActorTypeName;

        [Name("设为哪种目标")]
        public BBParameter<List<ActorAIAgent.TargetEntityType>> TargetEntityTypes;

        [Name("忽略Y轴")]
        public BBParameter<bool> Y_Ignore;

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return Status.Failure;
            Actor target = null;
            if (Y_Ignore.value)
            {
                bool groundBeneath = Actor.CheckIsGrounded(20f, out GridPos3D nearestGround);
                if (groundBeneath)
                {
                    target = BattleManager.Instance.SearchNearestActor(nearestGround, Actor.Camp, SearchRadius.value, RelativeCamp.value, ActorTypeName.value);
                }
            }
            else
            {
                target = BattleManager.Instance.SearchNearestActor(Actor.EntityBaseCenter, Actor.Camp, SearchRadius.value, RelativeCamp.value, ActorTypeName.value);
            }

            if (!target.IsNotNullAndAlive()) return Status.Failure;
            if (TargetEntityTypes.value != null)
            {
                foreach (ActorAIAgent.TargetEntityType targetEntityType in TargetEntityTypes.value)
                {
                    Actor.ActorAIAgent.AIAgentTargetDict[targetEntityType].TargetEntity = target;
                }
            }

            return Status.Success;
        }
    }

    [Category("敌兵/目标搜索")]
    [Name("删除目标")]
    [Description("删除目标")]
    public class BT_Enemy_ClearTargetEntity : BTNode
    {
        public override string name
        {
            get
            {
                string targetEntityTypeDesc = "";
                if (TargetEntityTypes.value != null)
                {
                    foreach (ActorAIAgent.TargetEntityType targetEntityType in TargetEntityTypes.value)
                    {
                        targetEntityTypeDesc += "[" + targetEntityType + "]";
                    }
                }

                return $"删除{targetEntityTypeDesc}目标";
            }
        }

        [Name("哪种目标")]
        public BBParameter<List<ActorAIAgent.TargetEntityType>> TargetEntityTypes;

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return Status.Failure;
            foreach (ActorAIAgent.TargetEntityType targetEntityType in TargetEntityTypes.value)
            {
                Actor.ActorAIAgent.AIAgentTargetDict[targetEntityType].ClearTarget();
            }

            return Status.Success;
        }
    }

    #endregion

    #region 寻路

    [Category("敌兵/寻路")]
    [Name("是否能够移动到目标")]
    [Description("是否能够移动到目标")]
    public class BT_Enemy_CheckCanMoveToMainPlayer : ConditionTask
    {
        protected override string info => $"是否能够移动到{TargetEntityType.value}目标";

        [Name("保持最小距离")]
        public BBParameter<float> KeepDistanceMin;

        [Name("保持最大距离")]
        public BBParameter<float> KeepDistanceMax;

        [Name("警戒距离")]
        public BBParameter<float> GuardingRange;

        [Name("哪种目标")]
        public BBParameter<ActorAIAgent.TargetEntityType> TargetEntityType;

        protected override bool OnCheck()
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return false;
            ActorAIAgent.AIAgentTarget target = Actor.ActorAIAgent.AIAgentTargetDict[TargetEntityType.value];
            if (!target.HasTarget) return false;
            GridPos3D targetGP = target.TargetGP;
            if (Actor.GetGridDistanceTo(targetGP, false) > GuardingRange.value) return false;
            Profiler.BeginSample("FindPath");
            bool suc = ActorPathFinding.FindPath(Actor.WorldGP_PF, ((Vector3) targetGP).ConvertWorldPositionToPathFindingNodeGP(Actor.ActorWidth), Actor.transform.position, null, KeepDistanceMin.value, KeepDistanceMax.value, ActorPathFinding.DestinationType.Actor, Actor.ActorWidth, Actor.ActorHeight, Actor.GUID);
            Profiler.EndSample();
            return suc;
        }
    }

    [Category("敌兵/寻路")]
    [Name("寻路至目标")]
    [Description("寻路至目标")]
    public class BT_Enemy_SetDestinationToMainPlayer : BTNode
    {
        public override string name => $"寻路至{TargetEntityType.value}目标";

        [Name("保持最小距离")]
        public BBParameter<float> KeepDistanceMin;

        [Name("保持最大距离")]
        public BBParameter<float> KeepDistanceMax;

        [Name("哪种目标")]
        public BBParameter<ActorAIAgent.TargetEntityType> TargetEntityType;

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return Status.Failure;
            if (Actor.ActorAIAgent.IsPathFinding) return Status.Failure;
            ActorAIAgent.AIAgentTarget target = Actor.ActorAIAgent.AIAgentTargetDict[TargetEntityType.value];
            if (!target.HasTarget) return Status.Failure;
            GridPos3D targetGP = target.TargetGP;
            return Actor.ActorAIAgent.SetDestinationToWorldGP(targetGP, KeepDistanceMin.value, KeepDistanceMax.value);
        }
    }

    [Category("敌兵/寻路")]
    [Name("寻路终点是否是目标坐标")]
    [Description("寻路终点是否是目标坐标")]
    public class BT_Enemy_CheckDestIsMainPlayer : ConditionTask
    {
        protected override string info => $"寻路终点是否是{TargetEntityType.value}目标坐标";

        [Name("容许偏差半径")]
        public BBParameter<float> ToleranceRadius;

        [Name("哪种目标")]
        public BBParameter<ActorAIAgent.TargetEntityType> TargetEntityType;

        protected override bool OnCheck()
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null || !Actor.ActorAIAgent.IsPathFinding) return false;
            ActorAIAgent.AIAgentTarget target = Actor.ActorAIAgent.AIAgentTargetDict[TargetEntityType.value];
            if (!target.HasTarget) return false;
            GridPos3D targetGP = target.TargetGP;
            return ((targetGP - Actor.ActorAIAgent.GetCurrentPathFindingDestination().ConvertPathFindingNodeGPToWorldPosition(Actor.ActorWidth)).magnitude <= ToleranceRadius.value);
        }
    }

    [Category("敌兵/寻路")]
    [Name("设置闲逛目标点")]
    [Description("设置闲逛目标点")]
    public class BT_Enemy_Idle : BTNode
    {
        [Name("闲逛半径")]
        public BBParameter<int> IdleRadius;

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return Status.Failure;
            if (Actor.ThrowState == Actor.ThrowStates.Lifting && Actor.CurrentLiftBox != null)
            {
                Actor.ThrowCharge();
                Actor.CurThrowPointOffset = Actor.transform.forward * 3f;
                Actor.Throw();
            }

            if (Actor.ActorAIAgent.IsPathFinding) return Status.Failure;
            bool suc = ActorPathFinding.FindRandomAccessibleDestination(Actor.WorldGP_PF, Actor.transform.position, IdleRadius.value, out GridPos3D destination_PF, Actor.ActorWidth, Actor.ActorHeight, Actor.GUID);
            if (suc)
            {
                destination_PF.ConvertPathFindingNodeGPToWorldPosition(Actor.ActorWidth);
                Actor.ActorAIAgent.AIAgentTargetDict[ActorAIAgent.TargetEntityType.Navigate].TargetGP = destination_PF.ConvertPathFindingNodeGPToWorldPosition(Actor.ActorWidth).ToGridPos3D();
                ActorAIAgent.SetDestinationRetCode retCode = Actor.ActorAIAgent.SetDestination(destination_PF, 0f, 0.5f, false, ActorPathFinding.DestinationType.EmptyGrid);
                switch (retCode)
                {
                    case ActorAIAgent.SetDestinationRetCode.AlreadyArrived:
                    case ActorAIAgent.SetDestinationRetCode.TooClose:
                    {
                        return Status.Success;
                    }
                    case ActorAIAgent.SetDestinationRetCode.Suc:
                    {
                        return Status.Success;
                    }
                    case ActorAIAgent.SetDestinationRetCode.Failed:
                    {
                        return Status.Failure;
                    }
                }

                return Status.Success;
            }
            else
            {
                return Status.Failure;
            }
        }
    }

    [Category("敌兵/寻路")]
    [Name("结束寻路")]
    [Description("结束寻路")]
    public class BT_Enemy_TerminatePathFinding : BTNode
    {
        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return Status.Failure;
            if (Actor.ActorAIAgent.IsPathFinding)
            {
                Actor.ActorAIAgent.InterruptCurrentPathFinding();
            }

            return Status.Success;
        }
    }

    [Category("敌兵/寻路")]
    [Name("强制结束寻路")]
    [Description("强制结束寻路")]
    public class BT_Enemy_ForceTerminatePathFinding : BTNode
    {
        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return Status.Failure;
            if (Actor.ActorAIAgent.IsPathFinding)
            {
                Actor.ActorAIAgent.ClearPathFinding();
            }

            return Status.Success;
        }
    }

    [Category("敌兵/寻路")]
    [Name("结束寻路")]
    [Description("结束寻路")]
    public class FL_Enemy_TerminatePathFinding : CallableActionNode
    {
        public override void Invoke()
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return;
            if (Actor.ActorAIAgent.IsPathFinding)
            {
                Actor.ActorAIAgent.InterruptCurrentPathFinding();
            }
        }
    }

    [Category("敌兵/寻路")]
    [Name("强制结束寻路")]
    [Description("强制结束寻路")]
    public class FL_Enemy_ForceTerminatePathFinding : CallableActionNode
    {
        public override void Invoke()
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return;
            if (Actor.ActorAIAgent.IsPathFinding)
            {
                Actor.ActorAIAgent.ClearPathFinding();
            }
        }
    }

    [Category("敌兵/寻路")]
    [Name("有寻路任务")]
    [Description("有寻路任务")]
    public class BT_Enemy_HasNavTask : ConditionTask
    {
        protected override bool OnCheck()
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return false;
            return Actor.ActorAIAgent.IsPathFinding;
        }
    }

    [Category("敌兵/寻路")]
    [Name("有寻路任务但卡在一个地方")]
    [Description("有寻路任务但卡在一个地方")]
    public class BT_Enemy_StuckWithNavTask : ConditionTask
    {
        [Name("卡住时长/ms")]
        public BBParameter<int> StuckTime;

        protected override bool OnCheck()
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return false;
            if (!Actor.ActorAIAgent.IsPathFinding) return false;
            return Actor.ActorAIAgent.StuckWithNavTask_Tick > (StuckTime.value / 1000f);
        }
    }

    [Category("敌兵/寻路")]
    [Name("卡在Box里")]
    [Description("卡在Box里")]
    public class BT_Enemy_StuckWithBoxes : ConditionTask
    {
        [Name("卡住时长/ms")]
        public BBParameter<int> StuckTime;

        protected override bool OnCheck()
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return false;
            return Actor.ActorAIAgent.StuckWithBoxesDuration > StuckTime.value / 1000f;
        }
    }

    [Category("敌兵/寻路")]
    [Name("销毁卡住的Boxes")]
    [Description("销毁卡住的Boxes")]
    public class BT_Enemy_DestroyStuckBoxes : BTNode
    {
        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return Status.Failure;
            Actor.ActorAIAgent.DestroyStuckBoxes();
            return Status.Success;
        }
    }

    [Category("敌兵/寻路")]
    [Name("目标在距离内")]
    [Description("目标在距离内")]
    public class BT_Enemy_PlayerInGuardRangeCondition : ConditionTask
    {
        protected override string info => $"[{TargetEntityType.value}]目标在距离内";

        [Name("阈值")]
        public BBParameter<float> RangeRadius;

        [Name("哪种目标")]
        public BBParameter<ActorAIAgent.TargetEntityType> TargetEntityType;

        [Name("忽略Y轴")]
        public BBParameter<bool> Y_Ignore;

        protected override bool OnCheck()
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return false;
            ActorAIAgent.AIAgentTarget target = Actor.ActorAIAgent.AIAgentTargetDict[TargetEntityType.value];
            if (!target.HasTarget) return false;
            GridPos3D targetGP = target.TargetGP;
            return Actor.GetGridDistanceTo(targetGP, Y_Ignore.value) <= RangeRadius.value;
        }
    }

    [Category("敌兵/寻路")]
    [Name("目标在距离内")]
    [Description("目标在距离内")]
    public class BT_Enemy_PlayerInGuardRangeConditionBTNode : BTNode
    {
        public override string name => $"[{TargetEntityType.value}]目标在距离内";

        [Name("阈值")]
        public BBParameter<float> RangeRadius;

        [Name("哪种目标")]
        public BBParameter<ActorAIAgent.TargetEntityType> TargetEntityType;

        [Name("忽略Y轴")]
        public BBParameter<bool> Y_Ignore;

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return Status.Failure;
            ActorAIAgent.AIAgentTarget target = Actor.ActorAIAgent.AIAgentTargetDict[TargetEntityType.value];
            if (!target.HasTarget) return Status.Failure;
            GridPos3D targetGP = target.TargetGP;
            bool inside = Actor.GetGridDistanceTo(targetGP, Y_Ignore.value) <= RangeRadius.value;
            return inside ? Status.Success : Status.Failure;
        }
    }

    #endregion

    [Category("敌兵/生命")]
    [Name("生命值大等于")]
    [Description("生命值大等于")]
    public class BT_Enemy_LifeCondition : ConditionTask
    {
        [Name("阈值")]
        public BBParameter<int> LifeThreshold;

        protected override bool OnCheck()
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return false;
            if (Actor.ActorBattleHelper == null) return false;
            return Actor.EntityStatPropSet.HealthDurability.Value >= LifeThreshold.value;
        }
    }

    [Category("敌兵/生命")]
    [Name("生命值大于")]
    [Description("生命值大于")]
    public class BT_Enemy_LifeConditionLargerThan : ConditionTask
    {
        [Name("阈值")]
        public BBParameter<int> LifeThreshold;

        protected override bool OnCheck()
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return false;
            if (Actor.ActorBattleHelper == null) return false;
            return Actor.EntityStatPropSet.HealthDurability.Value > LifeThreshold.value;
        }
    }

    [Category("敌兵/生命")]
    [Name("生命值大于%")]
    [Description("生命值大于%")]
    public class BT_Enemy_LifeConditionLargerThanPercent : ConditionTask
    {
        [Name("阈值%")]
        public BBParameter<int> LifePercentThreshold;

        protected override bool OnCheck()
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return false;
            if (Actor.ActorBattleHelper == null) return false;
            return Actor.EntityStatPropSet.HealthDurability.Value > LifePercentThreshold.value / 100f * Actor.EntityStatPropSet.MaxHealthDurability.GetModifiedValue;
        }
    }

    #region 战斗

    [Category("敌兵/战斗")]
    [Name("面向目标")]
    [Description("面向目标")]
    public class BT_Enemy_FaceToPlayer : BTNode
    {
        public override string name => $"面向{TargetEntityType.value}目标";

        [Name("哪种目标")]
        public BBParameter<ActorAIAgent.TargetEntityType> TargetEntityType;

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return Status.Failure;
            if (Actor.IsFrozen) return Status.Failure;
            if (Actor.ActorAIAgent.IsPathFinding) return Status.Failure;
            ActorAIAgent.AIAgentTarget target = Actor.ActorAIAgent.AIAgentTargetDict[TargetEntityType.value];
            if (!target.HasTarget) return Status.Failure;
            Vector3 diff = target.TargetGP - Actor.EntityBaseCenter;
            Actor.CurForward = diff.GetSingleDirectionVectorXZ();
            return Status.Success;
        }
    }

    [Category("敌兵/战斗")]
    [Name("释放技能")]
    [Description("释放技能")]
    public class BT_Enemy_TriggerSkill : BTNode
    {
        public override string name => $"向{TargetEntityType.value}目标释放技能 [{EntitySkillIndex.value}]";

        [Name("技能编号")]
        public BBParameter<EntitySkillIndex> EntitySkillIndex;

        [Name("哪种目标")]
        public BBParameter<ActorAIAgent.TargetEntityType> TargetEntityType;

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            return TriggerSkill(Actor, EntitySkillIndex.value, TargetEntityType.value);
        }

        public static Status TriggerSkill(Actor actor, EntitySkillIndex skillIndex, ActorAIAgent.TargetEntityType targetEntityType)
        {
            if (!actor.IsNotNullAndAlive() || actor.ActorAIAgent == null) return Status.Failure;
            if (actor.CannotAct) return Status.Failure;
            Entity targetEntity = actor.ActorAIAgent.AIAgentTargetDict[targetEntityType].TargetEntity;
            if (!targetEntity.IsNotNullAndAlive()) return Status.Failure;
            if (actor.EntityActiveSkillDict.TryGetValue(skillIndex, out EntityActiveSkill eas))
            {
                bool triggerSuc = eas.CheckCanTriggerSkill();
                if (triggerSuc)
                {
                    eas.TriggerActiveSkill();
                    return Status.Success;
                }
                else
                {
                    return Status.Failure;
                }
            }

            return Status.Failure;
        }
    }

    [Category("敌兵/战斗")]
    [Name("按动画释放技能")]
    [Description("按动画释放技能")]
    public class BT_Enemy_TriggerSkillByAnimation : BTNode
    {
        [Name("技能编号")]
        public BBParameter<EntitySkillIndex> EntitySkillIndex;

        public override string name => $"按动画释放技能 [{EntitySkillIndex.value}]";

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            return TriggerSkill(Actor, EntitySkillIndex.value);
        }

        public static Status TriggerSkill(Actor actor, EntitySkillIndex skillIndex)
        {
            if (!actor.IsNotNullAndAlive() || actor.ActorAIAgent == null) return Status.Failure;
            if (actor.CannotAct) return Status.Failure;
            if (actor.EntityActiveSkillDict.TryGetValue(skillIndex, out EntityActiveSkill eas))
            {
                bool triggerSuc = eas.CheckCanTriggerSkill();
                if (triggerSuc)
                {
                    actor.ActorArtHelper.PlaySkill(skillIndex);
                    return Status.Success;
                }
                else
                {
                    return Status.Failure;
                }
            }

            return Status.Success;
        }
    }

    [Category("敌兵/战斗")]
    [Name("释放技能+确认")]
    [Description("释放技能+确认")]
    public class BT_Enemy_TriggerSkill_ConditionTask : ConditionTask
    {
        protected override string info => $"向{TargetEntityType.value}目标释放技能 [{EntitySkillIndex.value}]且成功";

        [Name("技能编号")]
        public BBParameter<EntitySkillIndex> EntitySkillIndex;

        [Name("哪种目标")]
        public BBParameter<ActorAIAgent.TargetEntityType> TargetEntityType;

        protected override bool OnCheck()
        {
            Status status = BT_Enemy_TriggerSkill.TriggerSkill(Actor, EntitySkillIndex.value, TargetEntityType.value);
            return status == Status.Success;
        }
    }

    [Category("敌兵/战斗")]
    [Name("释放技能")]
    [Description("释放技能")]
    public class FL_Enemy_TriggerSkillAction : CallableActionNode
    {
        public override string name => $"向{TargetEntityType.value}目标释放技能 [{EntitySkillIndex.value}]";

        [Name("技能编号")]
        public BBParameter<EntitySkillIndex> EntitySkillIndex;

        [Name("哪种目标")]
        public BBParameter<ActorAIAgent.TargetEntityType> TargetEntityType;

        public override void Invoke()
        {
            BT_Enemy_TriggerSkill.TriggerSkill(Actor, EntitySkillIndex.value, TargetEntityType.value);
        }
    }

    [Category("敌兵/战斗")]
    [Name("等待技能结束")]
    [Description("等待技能结束")]
    public class BT_Enemy_WaitSkillEnd : BTNode
    {
        [Name("技能编号")]
        public BBParameter<EntitySkillIndex> EntitySkillIndex;

        public override string name => $"等待技能结束 [{EntitySkillIndex.value}]";

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return Status.Failure;
            if (Actor.EntityActiveSkillDict.TryGetValue(EntitySkillIndex.value, out EntityActiveSkill eas))
            {
                if (eas.SkillPhase == ActiveSkillPhase.CoolingDown || eas.SkillPhase == ActiveSkillPhase.Ready)
                {
                    return Status.Success;
                }
                else
                {
                    return Status.Running;
                }
            }

            return Status.Failure;
        }
    }

    [Category("敌兵/战斗")]
    [Name("等待技能结束")]
    [Description("等待技能结束")]
    public class FL_Enemy_WaitSkillEnd : WaitUntil
    {
        [Name("技能编号")]
        public BBParameter<EntitySkillIndex> EntitySkillIndex;

        public override string name => $"等待技能结束 [{EntitySkillIndex.value}]";

        public override IEnumerator Invoke()
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) yield return null;
            if (Actor.EntityActiveSkillDict.TryGetValue(EntitySkillIndex.value, out EntityActiveSkill eas))
            {
                while (eas.SkillPhase != ActiveSkillPhase.CoolingDown && eas.SkillPhase != ActiveSkillPhase.Ready)
                {
                    yield return null;
                }
            }
        }
    }

    [Category("敌兵/战斗")]
    [Name("强行打断技能")]
    [Description("强行打断技能")]
    public class BT_Enemy_InterruptSkill : BTNode
    {
        [Name("技能编号")]
        public BBParameter<EntitySkillIndex> EntitySkillIndex;

        public override string name => $"强行打断技能 [{EntitySkillIndex.value}]";

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return Status.Failure;
            if (Actor.EntityActiveSkillDict.TryGetValue(EntitySkillIndex.value, out EntityActiveSkill eas))
            {
                eas.Interrupt();
                return Status.Success;
            }

            return Status.Failure;
        }
    }

    [Category("敌兵/战斗")]
    [Name("设置原地起跳参数")]
    [Description("设置原地起跳参数")]
    public class BT_Enemy_JumpUp : BTNode
    {
        [Name("起跳力度")]
        public BBParameter<float> JumpForce;

        [Name("起跳高度（格）")]
        public BBParameter<int> JumpHeight;

        public override string name => $"[设置]按{JumpForce.value}力度原地起跳{JumpHeight.value}格";

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return Status.Failure;
            if (Actor.CannotAct && !Actor.IsGrounded) return Status.Failure;
            if (Actor.IsExecutingAirSkills()) return Status.Running;
            Actor.SetJumpUpTargetHeight(JumpForce.value, JumpHeight.value, true);
            return Status.Success;
        }
    }

    [Category("敌兵/战斗")]
    [Name("原地起跳Update")]
    [Description("原地起跳Update")]
    public class BT_Enemy_JumpingUpTick : BTNode
    {
        public override string name => $"原地起跳Update";

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return Status.Failure;
            if (Actor.CannotAct) return Status.Failure;
            if (Actor.ActorBehaviourState == Actor.ActorBehaviourStates.Jump)
            {
                if (!Actor.JumpReachClimax)
                {
                    return Status.Running;
                }
                else
                {
                    return Status.Success;
                }
            }
            else
            {
                return Status.Failure;
            }
        }
    }

    [Category("敌兵/战斗")]
    [Name("设置身体下砸")]
    [Description("[设置]身体下砸")]
    public class BT_Enemy_SmashDown : BTNode
    {
        [Name("砸下力度")]
        public BBParameter<float> SmashDownForce;

        public override string name => $"[设置]身体以{SmashDownForce.value}力度下砸";

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return Status.Failure;
            if ((Actor.ActorBehaviourState == Actor.ActorBehaviourStates.Jump && Actor.JumpReachClimax) || Actor.ActorBehaviourState == Actor.ActorBehaviourStates.InAirMoving)
            {
                if (Actor.CheckIsGrounded(20f, out GridPos3D nearestGroundGP))
                {
                    Actor.SetSmashDownTargetPos(nearestGroundGP + GridPos3D.Up, SmashDownForce.value);
                    return Status.Success;
                }
                else
                {
                    return Status.Failure;
                }
            }

            return Status.Failure;
        }
    }

    [Category("敌兵/战斗")]
    [Name("身体下砸Update")]
    [Description("身体下砸Update")]
    public class BT_Enemy_SmashingDownTick : BTNode
    {
        public override string name => $"身体下砸Update";

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return Status.Failure;
            if (Actor.ActorBehaviourState != Actor.ActorBehaviourStates.SmashDown) return Status.Success;
            if (Actor.IsGrounded)
            {
                return Status.Success;
            }
            else
            {
                return Status.Running;
            }
        }
    }

    [Category("敌兵/战斗")]
    [Name("设置悬空移动目标点")]
    [Description("设置悬空移动目标点")]
    public class BT_Enemy_InAirSetMoveTargetTop : BTNode
    {
        [Name("目标类型")]
        public BBParameter<ActorAIAgent.TargetEntityType> TargetEntityType;

        [Name("距离偏差容错")]
        public BBParameter<float> Tolerance;

        [Name("移动速度")]
        public BBParameter<float> MoveSpeed;

        public override string name => $"[设置]悬空速度{MoveSpeed.value}移到[{TargetEntityType.value}]头顶{Tolerance.value}范围";

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return Status.Failure;
            if (Actor.ActorBehaviourState != Actor.ActorBehaviourStates.Jump && Actor.ActorBehaviourState != Actor.ActorBehaviourStates.InAirMoving) return Status.Failure;
            ActorAIAgent.AIAgentTarget target = Actor.ActorAIAgent.AIAgentTargetDict[TargetEntityType.value];
            if (!target.HasTarget) return Status.Failure;
            Vector3 randomTolerance = Random.insideUnitSphere * Tolerance.value;
            randomTolerance.y = 0;
            Vector3 targetPos = target.TargetGP + randomTolerance;
            targetPos.y = Actor.transform.position.y;
            Actor.InAirSetMoveTargetPos(targetPos, MoveSpeed.value);
            return Status.Success;
        }
    }

    [Category("敌兵/战斗")]
    [Name("悬空移动Update")]
    [Description("悬空移动Update")]
    public class BT_Enemy_InAirMoveToTargetPos : BTNode
    {
        public override string name => $"悬空移动Update";

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return Status.Failure;
            if (Actor.ActorBehaviourState != Actor.ActorBehaviourStates.InAirMoving) return Status.Failure;
            Vector3 distanceXZ = Actor.transform.position - Actor.InAirMoveTargetPos;
            distanceXZ.y = 0;
            if (distanceXZ.magnitude <= 0.5f) return Status.Success;
            return Status.Running;
        }
    }

    #endregion

    #region 通用

    [Category("通用")]
    [Name("震屏")]
    [Description("震屏")]
    public class BT_Enemy_CameraShake : BTNode
    {
        [Name("伤害当量")]
        public BBParameter<int> EquivalentDamage;

        [Name("考虑和玩家的距离")]
        public BBParameter<bool> ConsiderDistanceToPlayer;

        public override string name => $"震屏";

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (!Actor.IsNotNullAndAlive() || Actor.ActorAIAgent == null) return Status.Failure;
            float distanceFromPlayer = 0f;
            if (ConsiderDistanceToPlayer.value)
            {
                distanceFromPlayer = Actor.GetGridDistanceTo(BattleManager.Instance.Player1, true);
            }

            CameraManager.Instance.FieldCamera.CameraShake(EquivalentDamage.value, distanceFromPlayer);
            return Status.Success;
        }
    }

    #endregion
}
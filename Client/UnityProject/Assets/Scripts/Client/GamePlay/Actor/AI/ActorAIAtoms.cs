using System.Collections.Generic;
using BiangStudio;
using BiangStudio.GameDataFormat.Grid;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

public static class ActorAIAtoms
{
    [Category("敌兵")]
    [Name("移动至距离玩家一定范围")]
    [Description("移动至距离玩家一定范围")]
    public class BT_Enemy_MoveToMainPlayer : BTNode
    {
        [Name("保持最小距离")]
        public BBParameter<float> KeepDistanceMin;

        [Name("保持最大距离")]
        public BBParameter<float> KeepDistanceMax;

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (Actor == null || Actor.ActorAIAgent == null) return Status.Failure;
            Actor player = BattleManager.Instance.Player1;
            ActorAIAgent.SetDestinationRetCode retCode = Actor.ActorAIAgent.SetDestination(player.CurGP, KeepDistanceMin.value, KeepDistanceMax.value, false, ActorPathFinding.DestinationType.Actor);
            switch (retCode)
            {
                case ActorAIAgent.SetDestinationRetCode.AlreadyArrived:
                {
                    return Status.Success;
                }
                case ActorAIAgent.SetDestinationRetCode.TooClose:
                {
                    // 逃脱寻路
                    List<GridPos3D> runDestList = new List<GridPos3D>();

                    for (int angle = 0; angle <= 360; angle += 10)
                    {
                        float radianAngle = Mathf.Deg2Rad * angle;
                        Vector3 dest = player.CurGP.ToVector3() + new Vector3((KeepDistanceMin.value + 1) * Mathf.Sin(radianAngle), 0, (KeepDistanceMin.value + 1) * Mathf.Cos(radianAngle));
                        GridPos3D destGP = dest.ToGridPos3D();
                        runDestList.Add(destGP);
                    }

                    runDestList.Sort((gp1, gp2) =>
                    {
                        float dist1 = (gp1.ToVector3() + player.transform.position - Actor.transform.position).magnitude;
                        float dist2 = (gp2.ToVector3() + player.transform.position - Actor.transform.position).magnitude;
                        return dist1.CompareTo(dist2);
                    });

                    foreach (GridPos3D gp in runDestList)
                    {
                        ActorAIAgent.SetDestinationRetCode rc = Actor.ActorAIAgent.SetDestination(gp, 0, 1, false, ActorPathFinding.DestinationType.EmptyGrid);
                        if (rc == ActorAIAgent.SetDestinationRetCode.Suc)
                        {
                            return Status.Running;
                        }
                    }

                    return Status.Failure;
                }
                case ActorAIAgent.SetDestinationRetCode.Suc:
                {
                    return Status.Running;
                }
                case ActorAIAgent.SetDestinationRetCode.Failed:
                {
                    return Status.Failure;
                }
            }

            return Status.Failure;
        }
    }

    [Category("敌兵")]
    [Name("是否能够移动到玩家")]
    [Description("是否能够移动到玩家")]
    public class BT_Enemy_CheckCanMoveToMainPlayer : ConditionTask
    {
        [Name("保持最小距离")]
        public BBParameter<float> KeepDistanceMin;

        [Name("保持最大距离")]
        public BBParameter<float> KeepDistanceMax;

        [Name("警戒距离")]
        public BBParameter<float> GuardingRange;

        protected override bool OnCheck()
        {
            if (Actor == null || Actor.ActorAIAgent == null) return false;
            Actor player = BattleManager.Instance.Player1;
            if ((player.transform.position - Actor.transform.position).magnitude > GuardingRange.value) return false;
            LinkedList<GridPos3D> path = ActorPathFinding.FindPath(Actor.CurGP, player.CurGP, KeepDistanceMin.value, KeepDistanceMax.value, ActorPathFinding.DestinationType.Actor);
            if (path != null) return true;
            return false;
        }
    }

    [Category("敌兵")]
    [Name("举着指定类型箱子")]
    [Description("举着指定类型箱子")]
    public class BT_Enemy_LiftCondition : ConditionTask
    {
        [Name("箱子类型名称")]
        public BBParameter<List<string>> LiftBoxTypeNames;

        protected override bool OnCheck()
        {
            if (Actor == null || Actor.ActorAIAgent == null) return false;
            if (LiftBoxTypeNames.value == null || LiftBoxTypeNames.value.Count == 0) return false;
            if (Actor.CurrentLiftBox != null)
            {
                string boxName = ConfigManager.GetBoxTypeName(Actor.CurrentLiftBox.BoxTypeIndex);
                return boxName != null && LiftBoxTypeNames.value.Contains(boxName);
            }

            return false;
        }
    }

    [Category("敌兵")]
    [Name("附近有指定类型箱子")]
    [Description("附近有指定类型箱子")]
    public class BT_Enemy_SearchBoxCondition : ConditionTask
    {
        [Name("箱子类型名称")]
        public BBParameter<List<string>> LiftBoxTypeNames;

        [Name("搜索形状")]
        public BBParameter<World.SearchRangeShape> SearchRangeShape;

        [Name("搜索半径")]
        public BBParameter<int> SearchRadius;

        [Name("排除距离玩家一定范围内的箱子")]
        public BBParameter<float> ExceptRadiusAroundPlayer;

        protected override bool OnCheck()
        {
            if (Actor == null || Actor.ActorAIAgent == null) return false;
            if (LiftBoxTypeNames.value == null || LiftBoxTypeNames.value.Count == 0) return false;
            List<Box> boxes = WorldManager.Instance.CurrentWorld.SearchBoxInRange(Actor.CurGP, SearchRadius.value, LiftBoxTypeNames.value, SearchRangeShape.value, ExceptRadiusAroundPlayer.value);
            if (boxes.Count == 0) return false;
            return true;
        }
    }

    [Category("敌兵")]
    [Name("搜索指定类型箱子并设定为目标")]
    [Description("搜索指定类型箱子并设定为目标")]
    public class BT_Enemy_SearchBoxAndSetTarget : BTNode
    {
        [Name("箱子类型名称")]
        public BBParameter<List<string>> LiftBoxTypeNames;

        [Name("搜索形状")]
        public BBParameter<World.SearchRangeShape> SearchRangeShape;

        [Name("搜索半径")]
        public BBParameter<int> SearchRadius;

        [Name("最短距离")]
        public BBParameter<bool> MinimumDistance;

        [Name("排除距离玩家一定范围内的箱子")]
        public BBParameter<float> ExceptRadiusAroundPlayer;

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (Actor == null || Actor.ActorAIAgent == null) return Status.Failure;
            if (LiftBoxTypeNames.value == null || LiftBoxTypeNames.value.Count == 0) return Status.Failure;
            List<Box> boxes = WorldManager.Instance.CurrentWorld.SearchBoxInRange(Actor.CurGP, SearchRadius.value, LiftBoxTypeNames.value, SearchRangeShape.value, ExceptRadiusAroundPlayer.value);
            if (boxes.Count == 0) return Status.Failure;
            int minDistance = int.MaxValue;
            Box nearestBox = null;
            if (MinimumDistance.value)
            {
                foreach (Box box in boxes)
                {
                    LinkedList<GridPos3D> path = ActorPathFinding.FindPath(Actor.CurGP, box.GridPos3D, 0, 0, ActorPathFinding.DestinationType.Box);
                    if (path != null && path.Count != 0)
                    {
                        int dist = path.Count;
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            nearestBox = box;
                        }
                    }
                }
            }
            else
            {
                List<Box> randomList = CommonUtils.GetRandomFromList(boxes, 1);
                if (randomList != null && randomList.Count > 0)
                {
                    nearestBox = randomList[0];
                }
            }

            if (nearestBox == null) return Status.Failure;
            Actor.ActorAIAgent.TargetBox = nearestBox;
            Actor.ActorAIAgent.TargetBoxGP = nearestBox.GridPos3D;
            return Status.Success;
        }
    }

    [Category("敌兵")]
    [Name("已搜索到箱子且箱子还在那儿")]
    [Description("已搜索到箱子且箱子还在那儿")]
    public class BT_Enemy_HasSearchedBox : ConditionTask
    {
        [Name("排除距离玩家一定范围内的箱子")]
        public BBParameter<float> ExceptRadiusAroundPlayer;

        protected override bool OnCheck()
        {
            if (Actor == null || Actor.ActorAIAgent == null) return false;
            if (Actor.ActorAIAgent.TargetBox == null || Actor.ActorAIAgent.TargetBox.GridPos3D != Actor.ActorAIAgent.TargetBoxGP) return false;
            if ((Actor.ActorAIAgent.TargetBox.transform.position - BattleManager.Instance.Player1.transform.position).magnitude <= ExceptRadiusAroundPlayer.value)
            {
                Actor.ActorAIAgent.TargetBox = null;
                Actor.ActorAIAgent.TargetBoxGP = GridPos3D.Zero;
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    [Category("敌兵")]
    [Name("有寻路任务但卡在一个地方")]
    [Description("有寻路任务但卡在一个地方")]
    public class BT_Enemy_StuckWithNavTask : ConditionTask
    {
        [Name("卡住时长/ms")]
        public BBParameter<int> StuckTime;

        protected override bool OnCheck()
        {
            if (Actor == null || Actor.ActorAIAgent == null) return false;
            return Actor.ActorAIAgent.StuckWithNavTask_Tick > (StuckTime.value / 1000f);
        }
    }

    [Category("敌兵")]
    [Name("移动至目标箱子附近")]
    [Description("移动至目标箱子附近")]
    public class BT_Enemy_MoveTowardsBox : BTNode
    {
        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (Actor == null || Actor.ActorAIAgent == null) return Status.Failure;
            if (Actor.ActorAIAgent.TargetBox == null || Actor.ActorAIAgent.TargetBox.GridPos3D != Actor.ActorAIAgent.TargetBoxGP) return Status.Failure;
            ActorAIAgent.SetDestinationRetCode retCode = Actor.ActorAIAgent.SetDestination(Actor.ActorAIAgent.TargetBoxGP, 0f, 0f, true, ActorPathFinding.DestinationType.Box);
            switch (retCode)
            {
                case ActorAIAgent.SetDestinationRetCode.AlreadyArrived:
                case ActorAIAgent.SetDestinationRetCode.TooClose:
                {
                    return Status.Success;
                }
                case ActorAIAgent.SetDestinationRetCode.Suc:
                {
                    return Status.Running;
                }
                case ActorAIAgent.SetDestinationRetCode.Failed:
                {
                    return Status.Failure;
                }
            }

            return Status.Failure;
        }
    }

    [Category("敌兵")]
    [Name("举起面前箱子")]
    [Description("举起面前箱子")]
    public class BT_Enemy_LiftBox : BTNode
    {
        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (Actor == null || Actor.ActorAIAgent == null) return Status.Failure;
            if (Actor.ThrowState != Actor.ThrowStates.None) return Status.Failure;
            GridPos3D boxGP = Actor.CurGP + Actor.CurForward.ToGridPos3D();
            if (Actor.ActorAIAgent.TargetBox != null && Actor.ActorAIAgent.TargetBox.GridPos3D == Actor.ActorAIAgent.TargetBoxGP && boxGP == Actor.ActorAIAgent.TargetBoxGP)
            {
                Box box = WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(boxGP, out WorldModule module, out GridPos3D _);
                if (box != null && box == Actor.ActorAIAgent.TargetBox)
                {
                    Actor.Lift();
                    if (Actor.ThrowState == Actor.ThrowStates.Raising)
                    {
                        return Status.Success;
                    }
                }

                return Status.Failure;
            }
            else
            {
                Actor.ActorAIAgent.TargetBox = null;
                Actor.ActorAIAgent.TargetBoxGP = GridPos3D.Zero;
                return Status.Failure;
            }
        }
    }

    [Category("敌兵")]
    [Name("扔箱子")]
    [Description("扔箱子")]
    public class BT_Enemy_ThrowBox : BTNode
    {
        [Name("扔箱子方向散射角")]
        [Description("扔箱子方向散射角")]
        public BBParameter<float> BoxDropAngleRange;

        [Name("扔箱子射程浮动")]
        [Description("扔箱子射程浮动")]
        public BBParameter<float> BoxDropRadiusRange;

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (Actor == null || Actor.ActorAIAgent == null) return Status.Failure;
            if (Actor.ThrowState != Actor.ThrowStates.Lifting && Actor.CurrentLiftBox == null) return Status.Failure;
            Actor.ThrowCharge();
            Vector3 direction = BattleManager.Instance.Player1.transform.position - Actor.transform.position;
            float correctDistance = direction.magnitude;
            float randomDistance = correctDistance + Random.Range(-BoxDropRadiusRange.value, BoxDropRadiusRange.value);
            float angleOffset = Random.Range(-BoxDropAngleRange.value, BoxDropAngleRange.value);
            Vector3 randomDirection = Quaternion.Euler(0, angleOffset, 0) * direction.normalized;
            Actor.CurThrowPointOffset = randomDirection * randomDistance;
            Actor.Throw();
            return Status.Success;
        }
    }

    [Category("敌兵")]
    [Name("踢箱子")]
    [Description("踢箱子")]
    public class BT_Enemy_KickBox : BTNode
    {
        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (Actor == null || Actor.ActorAIAgent == null) return Status.Failure;
            if (Actor.ThrowState != Actor.ThrowStates.None && Actor.CurrentLiftBox != null) return Status.Failure;
            Actor.Kick();
            return Status.Success;
        }
    }

    [Category("敌兵")]
    [Name("闲逛")]
    [Description("闲逛")]
    public class BT_Enemy_Idle : BTNode
    {
        [Name("闲逛半径")]
        public BBParameter<int> IdleRadius;

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (Actor == null || Actor.ActorAIAgent == null) return Status.Failure;
            if (Actor.ThrowState == Actor.ThrowStates.Lifting && Actor.CurrentLiftBox != null)
            {
                Actor.ThrowCharge();
                Actor.CurThrowPointOffset = Actor.transform.forward * 3f;
                Actor.Throw();
            }

            if (Actor.ActorAIAgent.IsPathFinding)
            {
                return Status.Running;
            }

            bool suc = ActorPathFinding.FindRandomAccessibleDestination(Actor.CurGP, IdleRadius.value, out GridPos3D destination);
            if (suc)
            {
                ActorAIAgent.SetDestinationRetCode retCode = Actor.ActorAIAgent.SetDestination(destination, 0f, 0.5f, false, ActorPathFinding.DestinationType.EmptyGrid);
                switch (retCode)
                {
                    case ActorAIAgent.SetDestinationRetCode.AlreadyArrived:
                    case ActorAIAgent.SetDestinationRetCode.TooClose:
                    {
                        return Status.Success;
                    }
                    case ActorAIAgent.SetDestinationRetCode.Suc:
                    {
                        return Status.Running;
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

    [Category("敌兵")]
    [Name("结束寻路")]
    [Description("结束寻路")]
    public class BT_Enemy_TerminatePathFinding : BTNode
    {
        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (Actor == null || Actor.ActorAIAgent == null) return Status.Failure;
            if (Actor.ActorAIAgent.IsPathFinding)
            {
                Actor.ActorAIAgent.ClearPathFinding();
            }

            return Status.Success;
        }
    }

    [Category("敌兵")]
    [Name("生命值大等于")]
    [Description("生命值大等于")]
    public class BT_Enemy_LifeCondition : ConditionTask
    {
        [Name("阈值")]
        public BBParameter<int> LifeThreshold;

        protected override bool OnCheck()
        {
            if (Actor == null || Actor.ActorAIAgent == null) return false;
            if (Actor.ActorBattleHelper == null) return false;
            return Actor.ActorBattleHelper.Health >= LifeThreshold.value;
        }
    }

    [Category("敌兵")]
    [Name("玩家在距离内")]
    [Description("玩家在距离内")]
    public class BT_Enemy_PlayerInGuardRangeCondition : ConditionTask
    {
        [Name("阈值")]
        public BBParameter<float> RangeRadius;

        protected override bool OnCheck()
        {
            if (Actor == null || Actor.ActorAIAgent == null) return false;
            return (BattleManager.Instance.Player1.transform.position - Actor.transform.position).magnitude <= RangeRadius.value;
        }
    }
}
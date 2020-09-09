using System.Collections.Generic;
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
            if (Actor == null) return Status.Failure;
            Actor player = BattleManager.Instance.MainPlayers[(int) PlayerNumber.Player1];
            ActorAIAgent.SetDestinationRetCode retCode = Actor.ActorAIAgent.SetDestination(player.CurGP, KeepDistanceMin.value, KeepDistanceMax.value);
            switch (retCode)
            {
                case ActorAIAgent.SetDestinationRetCode.AlreadyArrived:
                {
                    Actor.ActorAIAgent.EnableMove = false;
                    return Status.Success;
                }
                case ActorAIAgent.SetDestinationRetCode.TooClose:
                {
                    // 逃脱寻路
                    List<GridPos3D> runDestList = new List<GridPos3D>();
                    for (int x = Mathf.RoundToInt(-KeepDistanceMin.value + 1); x <= Mathf.RoundToInt(KeepDistanceMin.value + 1); x++)
                    {
                        int absZ = Mathf.RoundToInt(KeepDistanceMin.value + 1 - Mathf.Abs(x));
                        runDestList.Add(new GridPos3D(x, 0, absZ));
                        runDestList.Add(new GridPos3D(x, 0, -absZ));
                    }

                    runDestList.Sort((gp1, gp2) =>
                    {
                        float dist1 = (gp1.ToVector3() + player.transform.position - Actor.transform.position).magnitude;
                        float dist2 = (gp2.ToVector3() + player.transform.position - Actor.transform.position).magnitude;
                        return dist1.CompareTo(dist2);
                    });

                    foreach (GridPos3D gp in runDestList)
                    {
                        ActorAIAgent.SetDestinationRetCode rc = Actor.ActorAIAgent.SetDestination(player.CurGP + gp, 0, 1);
                        if (rc == ActorAIAgent.SetDestinationRetCode.Suc)
                        {
                            Actor.ActorAIAgent.EnableMove = true;
                            return Status.Running;
                        }
                    }

                    Actor.ActorAIAgent.EnableMove = false;

                    return Status.Failure;
                }
                case ActorAIAgent.SetDestinationRetCode.Suc:
                {
                    Actor.ActorAIAgent.EnableMove = true;
                    return Status.Running;
                }
                case ActorAIAgent.SetDestinationRetCode.Failed:
                {
                    Actor.ActorAIAgent.EnableMove = false;
                    return Status.Failure;
                }
            }

            return Status.Failure;
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
            if (Actor == null) return false;
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
    [Name("搜索并移动至指定类型箱子附近")]
    [Description("搜索并移动至指定类型箱子附近")]
    public class BT_Enemy_MoveTowardsBox : BTNode
    {
        [Name("箱子类型名称")]
        public BBParameter<List<string>> LiftBoxTypeNames;

        [Name("搜索形状")]
        public BBParameter<World.SearchRangeShape> SearchRangeShape;

        [Name("搜索半径")]
        public BBParameter<int> SearchRadius;

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (Actor == null) return Status.Failure;
            if (LiftBoxTypeNames.value == null || LiftBoxTypeNames.value.Count == 0) return Status.Failure;
            List<Box> boxes = WorldManager.Instance.CurrentWorld.SearchBoxInRange(Actor.CurGP, SearchRadius.value, LiftBoxTypeNames.value, SearchRangeShape.value);
            if (boxes.Count == 0) return Status.Failure;
            int minDistance = int.MaxValue;
            Box nearestBox = null;
            foreach (Box box in boxes)
            {
                LinkedList<GridPos3D> path = ActorPathFinding.FindPath(Actor.CurGP, box.GridPos3D);
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

            if (nearestBox == null) return Status.Failure;
            ActorAIAgent.SetDestinationRetCode retCode = Actor.ActorAIAgent.SetDestination(nearestBox.GridPos3D, 0.8f, 1.1f);
            switch (retCode)
            {
                case ActorAIAgent.SetDestinationRetCode.AlreadyArrived:
                {
                    Actor.ActorAIAgent.EnableMove = false;
                    return Status.Success;
                }
                case ActorAIAgent.SetDestinationRetCode.Suc:
                {
                    Actor.ActorAIAgent.EnableMove = true;
                    return Status.Running;
                }
                case ActorAIAgent.SetDestinationRetCode.Failed:
                {
                    Actor.ActorAIAgent.EnableMove = false;
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
        [Name("箱子类型名称")]
        public BBParameter<List<string>> LiftBoxTypeNames;

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (Actor == null) return Status.Failure;
            if (LiftBoxTypeNames.value == null) return Status.Failure;
            if (Actor.ThrowState != Actor.ThrowStates.None) return Status.Failure;
            GridPos3D boxGP = Actor.CurGP + Actor.CurForward.ToGridPos3D();
            Box box = WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(boxGP, out WorldModule module, out GridPos3D _);
            if (box != null)
            {
                string boxName = ConfigManager.GetBoxTypeName(box.BoxTypeIndex);
                if (boxName == null || !LiftBoxTypeNames.value.Contains(boxName)) return Status.Failure;

                Actor.Lift();
                if (Actor.ThrowState == Actor.ThrowStates.Raising)
                {
                    return Status.Success;
                }
            }

            return Status.Failure;
        }
    }

    [Category("敌兵")]
    [Name("扔箱子")]
    [Description("扔箱子")]
    public class BT_Enemy_ThrowBox : BTNode
    {
        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (Actor == null) return Status.Failure;
            if (Actor.ThrowState != Actor.ThrowStates.Lifting && Actor.CurrentLiftBox == null) return Status.Failure;
            Actor.ThrowCharge();
            Actor.CurThrowPointOffset = BattleManager.Instance.MainPlayers[(int) PlayerNumber.Player1].transform.position - Actor.transform.position;
            Actor.Throw();
            return Status.Success;
        }
    }

    [Category("敌兵")]
    [Name("闲逛")]
    [Description("闲逛")]
    public class BT_Enemy_Idle : BTNode
    {
        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (Actor == null) return Status.Failure;
            if (Actor.ThrowState != Actor.ThrowStates.Lifting && Actor.CurrentLiftBox == null) return Status.Failure;
            Actor.ThrowCharge();
            Actor.CurThrowPointOffset = BattleManager.Instance.MainPlayers[(int) PlayerNumber.Player1].transform.position - Actor.transform.position;
            Actor.Throw();
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
            if (Actor == null) return false;
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
            if (Actor == null) return false;
            return (BattleManager.Instance.MainPlayers[(int) PlayerNumber.Player1].transform.position - Actor.transform.position).magnitude <= RangeRadius.value;
        }
    }
}
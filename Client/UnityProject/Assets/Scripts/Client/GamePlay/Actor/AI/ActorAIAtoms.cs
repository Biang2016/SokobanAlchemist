using System.Collections.Generic;
using BiangStudio.GameDataFormat.Grid;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

public static class ActorAIAtoms
{
    [Category("敌兵")]
    [Name("朝玩家移动")]
    [Description("朝玩家移动")]
    public class BT_MoveToMainPlayer : BTNode
    {
        [Name("默认保持距离Min")]
        public BBParameter<float> KeepDistanceMin;

        [Name("默认保持距离Max")]
        public BBParameter<float> KeepDistanceMax;

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (Actor == null) return Status.Failure;
            Actor.ActorAIAgent.SetDestination(BattleManager.Instance.MainPlayers[(int) PlayerNumber.Player1].CurGP, KeepDistanceMin.value, KeepDistanceMax.value);
            Actor.ActorAIAgent.EnableMove = true;
            return Status.Success;
        }
    }

    [Category("敌兵")]
    [Name("是否举着某类箱子")]
    [Description("是否举着某类箱子")]
    public class BT_LiftCondition : ConditionTask
    {
        [Name("箱子类型名称(或All/None)")]
        public BBParameter<string> BoxName;

        protected override bool OnCheck()
        {
            if (Actor == null) return false;
            if (string.IsNullOrWhiteSpace(BoxName.value)) return false;

            if (BoxName.value == "All")
            {
                return Actor.ThrowState == Actor.ThrowStates.Lifting;
            }

            if (BoxName.value == "None")
            {
                return Actor.ThrowState == Actor.ThrowStates.None;
            }

            if (Actor.CurrentLiftBox != null)
            {
                string boxName = ConfigManager.GetBoxTypeName(Actor.CurrentLiftBox.BoxTypeIndex);
                return boxName != null && boxName == BoxName.value;
            }

            return false;
        }
    }

    [Category("敌兵")]
    [Name("搜索并移动至某类箱子附近")]
    [Description("搜索并移动至某类箱子附近")]
    public class BT_MoveTowardsBox : BTNode
    {
        [Name("箱子类型名称(或All/None)")]
        public BBParameter<string> BoxName;

        [Name("搜索形状")]
        public BBParameter<World.SearchRangeShape> SearchRangeShape;

        [Name("搜索半径")]
        public BBParameter<int> SearchRadius;

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (Actor == null) return Status.Failure;
            List<Box> boxes = WorldManager.Instance.CurrentWorld.SearchBoxInRange(Actor.CurGP, SearchRadius.value, BoxName.value, SearchRangeShape.value);
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
            ActorAIAgent.SetDestinationRetCode retCode = Actor.ActorAIAgent.SetDestination(nearestBox.GridPos3D, 1f, 1.1f);
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
    public class BT_LiftBox : BTNode
    {
        [Name("箱子类型名称(或All/None)")]
        public BBParameter<string> BoxName;

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (Actor == null) return Status.Failure;
            if (Actor.ThrowState != Actor.ThrowStates.None) return Status.Failure;
            GridPos3D boxGP = Actor.CurGP + Actor.CurForward.ToGridPos3D();
            Box box = WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(boxGP, out WorldModule module, out GridPos3D _);
            if (box != null)
            {
                byte boxTypeIndex = ConfigManager.GetBoxTypeIndex(BoxName.value);
                if (boxTypeIndex != box.BoxTypeIndex) return Status.Failure;

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
    public class BT_ThrowBox : BTNode
    {
        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            return Status.Failure;
            //if (Actor == null) return Status.Failure;
            //if (Actor.ThrowState != Actor.ThrowStates.None) return Status.Failure;
            //GridPos3D boxGP = Actor.CurGP + Actor.CurForward.ToGridPos3D();
            //Box box = WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(boxGP, out WorldModule module, out GridPos3D _);
            //if (box != null)
            //{
            //    byte boxTypeIndex = ConfigManager.GetBoxTypeIndex(BoxName.value);
            //    if (boxTypeIndex != box.BoxTypeIndex) return Status.Failure;

            //    Actor.Lift();
            //    if (Actor.ThrowState == Actor.ThrowStates.Raising)
            //    {
            //        return Status.Success;
            //    }
            //}

            //return Status.Failure;
        }
    }
}
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

public static class ActorAIAtoms
{
    [Category("敌兵")]
    [Name("朝玩家移动")]
    [Description("朝玩家移动")]
    public class MoveToMainPlayer : BTNode
    {
        [Name("默认保持距离")]
        public BBParameter<float> KeepDistance;

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            if (Actor == null) return Status.Failure;
            Actor.ActorAIAgent.SetDestination(BattleManager.Instance.MainPlayers[(int) PlayerNumber.Player1].CurGP, KeepDistance.value);
            Actor.ActorAIAgent.EnableMove = true;
            return Status.Success;
        }
    }
}
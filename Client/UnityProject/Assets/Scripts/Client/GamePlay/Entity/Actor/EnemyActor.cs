using UnityEngine;

#if UNITY_EDITOR

#endif
public class EnemyActor : Actor
{
    internal int AIUpdateInterval = 10;
    private int AIUpdateIntervalTick = 0;

    public override void OnUsed()
    {
        base.OnUsed();
        AIUpdateIntervalTick = 0;
    }

    protected override void FixedUpdate()
    {
        if (!IsRecycled)
        {
            if (BattleManager.Instance.Player1 != null)
            {
                float distanceFromMainPlayer = (transform.position - BattleManager.Instance.Player1.transform.position).magnitude;
                if (distanceFromMainPlayer > 30f)
                {
                    AIUpdateInterval = 50;
                }
                else if (distanceFromMainPlayer > 20f)
                {
                    AIUpdateInterval = 20;
                }
                else if (distanceFromMainPlayer > 10)
                {
                    AIUpdateInterval = 15;
                }
                else
                {
                    AIUpdateInterval = 10;
                }
            }

            if (BattleManager.Instance.IsStart)
            {
                if (AIUpdateIntervalTick < AIUpdateInterval)
                {
                    AIUpdateIntervalTick++;
                }
                else
                {
                    GraphOwner.graph.UpdateGraph(AIUpdateInterval * Time.fixedDeltaTime);
                    AIUpdateIntervalTick = 0;
                }

                ActorAIAgent.FixedUpdate();
            }

            MoveInternal();
            ActorAIAgent.FixedUpdateAfterMove();
        }

        base.FixedUpdate();
    }

#if UNITY_EDITOR
    //void OnDrawGizmos()
    //{
    //    if (Application.isPlaying)
    //    {
    //        if (Selection.Contains(gameObject))
    //        {
    //            float guardRange = (float) GraphOwner.blackboard.GetVariable("GuardRange").value;
    //            float chasingRange = (float) GraphOwner.blackboard.GetVariable("ChasingRange").value;
    //            float keepDistanceMin = (float) GraphOwner.blackboard.GetVariable("KeepDistanceMin").value;
    //            float keepDistanceMax = (float) GraphOwner.blackboard.GetVariable("KeepDistanceMax").value;
    //            Gizmos.color = new Color(0, 1, 0, 1f);
    //            Gizmos.DrawWireSphere(transform.position, guardRange);
    //            Gizmos.color = new Color(1, 0, 0, 1f);
    //            Gizmos.DrawWireSphere(transform.position, chasingRange);
    //            Gizmos.color = new Color(1, 0, 1f, 1f);
    //            Gizmos.DrawWireSphere(transform.position, keepDistanceMin);
    //            Gizmos.color = new Color(1, 0, 1f, 1f);
    //            Gizmos.DrawWireSphere(transform.position, keepDistanceMax);
    //        }
    //    }
    //}

#endif
}
using UnityEngine;

public class EnemyActor : Actor
{
    internal float AIUpdateInterval = 0.3f;
    private float AIUpdateIntervalTick = 0;

    public override void OnUsed()
    {
        base.OnUsed();
        AIUpdateIntervalTick = 0;
    }

    protected override void Tick(float interval)
    {
        if (!IsRecycled)
        {
            if (BattleManager.Instance.Player1 != null)
            {
                float distanceFromMainPlayer = (transform.position - BattleManager.Instance.Player1.transform.position).magnitude;
                if (FrequentUpdate)
                {
                    AIUpdateInterval = 0.1f;
                }
                else
                {
                    if (distanceFromMainPlayer > 30f)
                    {
                        AIUpdateInterval = 1f;
                    }
                    else if (distanceFromMainPlayer > 20f)
                    {
                        AIUpdateInterval = 0.4f;
                    }
                    else
                    {
                        AIUpdateInterval = 0.3f;
                    }
                }
              
            }

            if (BattleManager.Instance.IsStart)
            {
                if (AIUpdateIntervalTick < AIUpdateInterval)
                {
                    AIUpdateIntervalTick += interval;
                }
                else
                {
                    GraphOwner.graph.UpdateGraph(AIUpdateInterval);
                    ActorAIAgent.AITick(AIUpdateInterval);
                    AIUpdateIntervalTick -= AIUpdateInterval;
                }

                ActorAIAgent.ActorTick(interval);
            }

            MoveInternal();
            ActorAIAgent.ActorTickAfterMove(interval);
        }

        base.Tick(interval);
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
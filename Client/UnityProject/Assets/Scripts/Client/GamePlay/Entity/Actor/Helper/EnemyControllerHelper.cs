using NodeCanvas.Framework;

public class EnemyControllerHelper : ActorMonoHelper
{
    internal GraphOwner GraphOwner;
    internal ActorAIAgent ActorAIAgent;

    internal float AIUpdateInterval = 0.3f;
    private float AIUpdateIntervalTick = 0;

    public void OnSetup()
    {
        ActorAIAgent = new ActorAIAgent(Actor);
        GraphOwner = Actor.GetComponent<GraphOwner>();
        ActorAIAgent.Start();
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
        AIUpdateIntervalTick = 0;
    }

    public void OnTick(float interval)
    {
        if (!Actor.IsRecycled)
        {
            if (BattleManager.Instance.Player1 != null)
            {
                float distanceFromMainPlayer = (transform.position - BattleManager.Instance.Player1.transform.position).magnitude;
                if (Actor.FrequentUpdate)
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

            Actor.MoveInternal();
            ActorAIAgent.ActorTickAfterMove(interval);
        }
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
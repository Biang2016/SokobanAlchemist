using NodeCanvas.Framework;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif
public class EnemyActor : Actor
{
    internal int AIUpdateInterval = 10;
    private int AIUpdateIntervalTick = 0;

    protected override void FixedUpdate()
    {
        if (!IsRecycled)
        {
            if (AIUpdateIntervalTick < AIUpdateInterval)
            {
                AIUpdateIntervalTick++;
            }
            else
            {
                GraphOwner.UpdateBehaviour();
                AIUpdateIntervalTick = 0;
            }

            ActorAIAgent.Update();
            MoveInternal();
        }

        base.FixedUpdate();
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (Selection.Contains(gameObject))
        {
            float guardRange = (float) GraphOwner.blackboard.GetVariable("GuardRange").value;
            float chasingRange = (float) GraphOwner.blackboard.GetVariable("ChasingRange").value;
            float keepDistanceMin = (float) GraphOwner.blackboard.GetVariable("KeepDistanceMin").value;
            float keepDistanceMax = (float) GraphOwner.blackboard.GetVariable("KeepDistanceMax").value;
            Gizmos.color = new Color(0, 1, 0, 1f);
            Gizmos.DrawWireSphere(transform.position, guardRange);
            Gizmos.color = new Color(1, 0, 0, 1f);
            Gizmos.DrawWireSphere(transform.position, chasingRange);
            Gizmos.color = new Color(1, 0, 1f, 1f);
            Gizmos.DrawWireSphere(transform.position, keepDistanceMin);
            Gizmos.color = new Color(1, 0, 1f, 1f);
            Gizmos.DrawWireSphere(transform.position, keepDistanceMax);
        }
    }

#endif
}
using System.Collections.Generic;
using BiangStudio.GameDataFormat.Grid;
using UnityEngine;

public class ActorAIAgent
{
    internal Actor Actor;

    public ActorAIAgent(Actor actor)
    {
        Actor = actor;
    }

    public void Update()
    {
        if (EnableRotate) RotateTowardsTarget();
        if (EnableMove) MoveToDestination();
    }

    public bool EnableRotate = false;
    public float RotateSpeed;
    private Vector3 currentRotateTarget;

    public void SetRotateTarget(Vector3 target)
    {
        target.y = Actor.transform.position.y;
        currentRotateTarget = target;
    }

    public void RotateTowardsTarget()
    {
        Vector3 diff = currentRotateTarget - Actor.transform.position;
        Quaternion rotation = Quaternion.LookRotation(diff);
        Actor.transform.rotation = Quaternion.Lerp(Actor.transform.rotation, rotation, Time.deltaTime * diff.magnitude * RotateSpeed);
    }

    public bool EnableMove = false;
    private float KeepDistance;
    private GridPos3D currentDestination;
    private LinkedList<GridPos3D> currentPath;
    private LinkedListNode<GridPos3D> currentNode;
    private LinkedListNode<GridPos3D> nextNode;

    public enum SetDestinationRetCode
    {
        AlreadyArrived,
        Suc,
        Failed,
    }

    public SetDestinationRetCode SetDestination(GridPos3D dest, float keepDistance)
    {
        currentDestination = dest;
        KeepDistance = keepDistance;
        if ((Actor.CurGP.ToVector3() - dest.ToVector3()).magnitude <= keepDistance) return SetDestinationRetCode.AlreadyArrived;
        currentPath = ActorPathFinding.FindPath(Actor.CurGP, dest);
        if (currentPath != null)
        {
            currentNode = currentPath.First;
            nextNode = currentPath.First.Next;
            return SetDestinationRetCode.Suc;
        }
        else
        {
            currentNode = null;
            nextNode = null;
            return SetDestinationRetCode.Failed;
        }
    }

    public void MoveToDestination()
    {
        Vector3 diff = currentPath.Last.Value.ToVector3() - Actor.transform.position;
        if (currentPath != null && diff.magnitude <= KeepDistance)
        {
            Actor.CurForward = diff.normalized.ToGridPos3D().ToVector3();
            Actor.CurMoveAttempt = Vector3.zero;
        }
        else
        {
            if (nextNode != null)
            {
                if ((Actor.transform.position - nextNode.Value.ToVector3()).magnitude < 0.01f)
                {
                    currentNode = nextNode;
                    nextNode = nextNode.Next;
                }
            }

            if (nextNode != null && currentNode != null)
            {
                Actor.CurMoveAttempt = (nextNode.Value.ToVector3() - Actor.transform.position).normalized;
            }
            else
            {
                Actor.CurMoveAttempt = Vector3.zero;
            }
        }
    }
}
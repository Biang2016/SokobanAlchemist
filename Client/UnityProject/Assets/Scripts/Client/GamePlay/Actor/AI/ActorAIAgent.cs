using System.Collections.Generic;
using BiangStudio.GameDataFormat.Grid;
using UnityEngine;

public class ActorAIAgent
{
    internal Actor Actor;
    private List<Marker> NavTrackMarkers = new List<Marker>();

    public ActorAIAgent(Actor actor)
    {
        Actor = actor;
        Stop();
    }

    private bool isStop = false;

    public void Stop()
    {
        isStop = true;
        ClearNavTrackMarkers();
    }

    private void ClearNavTrackMarkers()
    {
        foreach (Marker marker in NavTrackMarkers)
        {
            marker.PoolRecycle();
        }

        NavTrackMarkers.Clear();
    }

    public void Start()
    {
        isStop = false;
    }

    public void Update()
    {
        if (isStop) return;
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
    private float KeepDistanceMin;
    private float KeepDistanceMax;
    private GridPos3D currentDestination;
    private LinkedList<GridPos3D> currentPath;
    private LinkedListNode<GridPos3D> currentNode;
    private LinkedListNode<GridPos3D> nextNode;

    public enum SetDestinationRetCode
    {
        AlreadyArrived,
        TooClose,
        Suc,
        Failed,
    }

    public SetDestinationRetCode SetDestination(GridPos3D dest, float keepDistanceMin, float keepDistanceMax)
    {
        currentDestination = dest;
        KeepDistanceMin = keepDistanceMin;
        KeepDistanceMax = keepDistanceMax;
        float dist = (Actor.CurGP.ToVector3() - dest.ToVector3()).magnitude;
        if (dist <= keepDistanceMax && dist >= keepDistanceMin)
        {
            currentPath = null;
            currentNode = null;
            nextNode = null;
            EnableMove = false;
            return SetDestinationRetCode.AlreadyArrived;
        }

        if (dist < keepDistanceMin)
        {
            currentPath = null;
            currentNode = null;
            nextNode = null;
            EnableMove = false;
            return SetDestinationRetCode.TooClose;
        }

        currentPath = ActorPathFinding.FindPath(Actor.CurGP, dest);
        if (currentPath != null)
        {
            currentNode = currentPath.First;
            nextNode = currentPath.First.Next;
            ClearNavTrackMarkers();
            foreach (GridPos3D gp in currentPath)
            {
                Marker marker = Marker.BaseInitialize(MarkerType.NavTrackMarker, Actor.NavTrackMarkerRoot);
                marker.transform.position = gp.ToVector3();
                NavTrackMarkers.Add(marker);
            }

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
        Actor.CurMoveAttempt = Vector3.zero;
        if (currentPath != null)
        {
            Vector3 diff = currentPath.Last.Value.ToVector3() - Actor.transform.position;
            if (diff.magnitude <= (KeepDistanceMax + KeepDistanceMin) / 2f)
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
}
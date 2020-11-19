using System.Collections.Generic;
using BiangStudio.GameDataFormat.Grid;
using UnityEngine;

public class ActorAIAgent
{
    internal Actor Actor;
    private List<Marker> NavTrackMarkers = new List<Marker>();

    public Box TargetBox;
    public GridPos3D TargetBoxGP;

    public float StuckWithNavTask_Tick = 0;

    public ActorAIAgent(Actor actor)
    {
        Actor = actor;
        Stop();
    }

    private bool isStop = false;

    public void Stop()
    {
        isStop = true;
        ClearPathFinding();
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

    public void SetNavTrackMarkersShown(bool setShown)
    {
        foreach (Marker marker in NavTrackMarkers)
        {
            marker.SetShown(setShown);
        }
    }

    public void Start()
    {
        isStop = false;
        Actor.GraphOwner?.StartBehaviour();
    }

    public void FixedUpdate()
    {
        if (isStop) return;
        MoveToDestination();
    }

    public void FixedUpdateAfterMove()
    {
        if (isStop) return;
        if (Actor.CurWorldGP == Actor.LastWorldGP && currentPath != null)
        {
            StuckWithNavTask_Tick += Time.fixedDeltaTime;
        }
        else
        {
            StuckWithNavTask_Tick = 0;
        }
    }

    private float KeepDistanceMin;
    private float KeepDistanceMax;
    private bool LastNodeOccupied;
    public bool IsPathFinding;
    private GridPos3D currentDestination;
    private LinkedList<GridPos3D> currentPath;
    private LinkedListNode<GridPos3D> currentNode;
    private LinkedListNode<GridPos3D> nextNode;

    public void ClearPathFinding()
    {
        IsPathFinding = false;
        currentPath = null;
        currentNode = null;
        nextNode = null;
        Actor.CurMoveAttempt = Vector3.zero;
        currentDestination = GridPos3D.Zero;
    }

    public enum SetDestinationRetCode
    {
        AlreadyArrived,
        TooClose,
        Suc,
        Failed,
    }

    public SetDestinationRetCode SetDestination(GridPos3D dest, float keepDistanceMin, float keepDistanceMax, bool lastNodeOccupied, ActorPathFinding.DestinationType destinationType)
    {
        currentDestination = dest;
        KeepDistanceMin = keepDistanceMin;
        KeepDistanceMax = keepDistanceMax;
        LastNodeOccupied = lastNodeOccupied;
        float dist = (Actor.CurWorldGP.ToVector3() - currentDestination.ToVector3()).magnitude;
        if (dist <= KeepDistanceMax + (KeepDistanceMax.Equals(0) && LastNodeOccupied ? 1 : 0) && dist >= KeepDistanceMin)
        {
            Vector3 forward = dest.ToVector3() - Actor.transform.position;
            if (forward.normalized.magnitude > 0)
            {
                Actor.CurForward = forward.normalized;
            }

            ClearPathFinding();
            return SetDestinationRetCode.AlreadyArrived;
        }

        if (dist <= KeepDistanceMin)
        {
            ClearPathFinding();
            return SetDestinationRetCode.TooClose;
        }

        currentPath = ActorPathFinding.FindPath(Actor.CurWorldGP, currentDestination, KeepDistanceMin, KeepDistanceMax, destinationType);
        if (currentPath != null)
        {
            IsPathFinding = true;
            currentNode = currentPath.First;
            nextNode = currentPath.First.Next;

            // 绘制Debug寻路点
            ClearNavTrackMarkers();
            if (ConfigManager.ShowEnemyPathFinding)
            {
                int count = 0;
                foreach (GridPos3D gp in currentPath)
                {
                    MarkerType mt = count == currentPath.Count - 1 ? MarkerType.NavTrackMarker_Final : MarkerType.NavTrackMarker;
                    count++;
                    Marker marker = Marker.BaseInitialize(mt, BattleManager.Instance.NavTrackMarkerRoot);
                    marker.transform.position = gp.ToVector3();
                    NavTrackMarkers.Add(marker);
                }
            }

            return SetDestinationRetCode.Suc;
        }
        else
        {
            ClearPathFinding();
            return SetDestinationRetCode.Failed;
        }
    }

    public void MoveToDestination()
    {
        Actor.CurMoveAttempt = Vector3.zero;
        if (currentPath != null)
        {
            if (nextNode != null)
            {
                Box box = WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(nextNode.Value, out WorldModule module, out GridPos3D _);
                if (box && !box.Passable)
                {
                    ClearPathFinding(); // 有箱子挡路，停止寻路
                }
                else
                {
                    Vector3 diff = nextNode.Value.ToVector3() - Actor.transform.position;

                    if (diff.magnitude < 0.01f)
                    {
                        bool checkArriveDest = nextNode == currentPath.Last || (LastNodeOccupied && nextNode.Next == currentPath.Last);
                        if (checkArriveDest)
                        {
                            if (LastNodeOccupied && nextNode.Next != null && nextNode.Next == currentPath.Last)
                            {
                                Actor.CurForward = (nextNode.Next.Value - nextNode.Value).ToVector3().normalized;
                            }

                            ClearPathFinding();
                            return;
                        }

                        currentNode = nextNode;
                        nextNode = nextNode.Next;
                    }

                    if (nextNode != null)
                    {
                        Actor.CurMoveAttempt = (nextNode.Value.ToVector3() - Actor.transform.position).normalized;
                        if (Mathf.Abs(Actor.CurMoveAttempt.x) > Mathf.Abs(Actor.CurMoveAttempt.z))
                        {
                            Actor.CurMoveAttempt.z = 0;
                        }
                        else
                        {
                            Actor.CurMoveAttempt.x = 0;
                        }
                    }
                }
            }
        }
    }

    public GridPos3D GetCurrentPathFindingDestination()
    {
        return currentDestination;
    }
}
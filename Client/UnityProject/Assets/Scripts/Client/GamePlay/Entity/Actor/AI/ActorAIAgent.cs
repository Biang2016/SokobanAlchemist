using System.Collections.Generic;
using BiangStudio;
using BiangStudio.GameDataFormat.Grid;
using UnityEngine;

public class ActorAIAgent
{
    internal static bool ENABLE_ACTOR_AI_AGENT_LOG = false;

    internal Actor Actor;
    private List<Marker> NavTrackMarkers = new List<Marker>();

    public Box TargetBox;
    public GridPos3D TargetBoxGP;

    public Actor TargetActor;
    public GridPos3D TargetActorGP;

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
        TargetBox = null;
        TargetBoxGP = GridPos3D.Zero;
        TargetActor = null;
        TargetActorGP = GridPos3D.Zero;
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

    public void InterruptCurrentPathFinding()
    {
        if (!IsPathFinding) return;
        if (currentPath == null) return;

        while (currentPath.Last != nextNode)
        {
            currentPath.RemoveLast();
        }
    }

    /// <summary>
    /// 清除当前寻路，但是要求是完成下一个Node，完成后自动Clear，否则会出现停止瞬间snap到grid的bug
    /// </summary>
    public void ClearPathFinding()
    {
        IsPathFinding = false;
        currentPath = null;
        currentNode = null;
        nextNode = null;
        Actor.CurMoveAttempt = Vector3.zero;
        currentDestination = GridPos3D.Zero;
        if (ENABLE_ACTOR_AI_AGENT_LOG) Debug.Log($"{Actor.name} [AIAgent] ClearPathFinding");
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
            bool interruptPathFinding = false;
            if (dist - keepDistanceMin >= 1)
            {
                interruptPathFinding = 0.5f.ProbabilityBool();
            }
            else
            {
                interruptPathFinding = true;
            }

            if (interruptPathFinding)
            {
                Vector3 forward = dest.ToVector3() - Actor.transform.position;
                if (forward.normalized.magnitude > 0)
                {
                    Actor.CurForward = forward.normalized;
                }

                InterruptCurrentPathFinding();
                return SetDestinationRetCode.AlreadyArrived;
            }
        }

        if (dist <= KeepDistanceMin)
        {
            InterruptCurrentPathFinding();
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

            StuckWithNavTask_Tick = 0;
            return SetDestinationRetCode.Suc;
        }
        else
        {
            InterruptCurrentPathFinding();
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
                // 有箱子或Actor挡路，停止寻路
                Box box = WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(nextNode.Value, out WorldModule module, out GridPos3D _);
                bool actorOccupied = WorldManager.Instance.CurrentWorld.CheckActorOccupiedGrid(nextNode.Value, Actor.GUID);
                if ((box && !box.Passable) || actorOccupied)
                {
                    Vector3 diff = currentNode.Value.ToVector3() - Actor.transform.position;
                    if (diff.magnitude < 0.2f)
                    {
                        ClearPathFinding();
                    }
                    else
                    {
                        InterruptCurrentPathFinding();
                    }
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
                    else if (diff.magnitude > 1.1f && !diff.x.Equals(0) && !diff.z.Equals(0)) // 由于某些意外，下一个路径点和目前离得较远，会发生角色原地打转不寻路的bug，此处强行重置
                    {
                        ClearPathFinding();
                    }

                    if (nextNode != null)
                    {
                        Actor.CurMoveAttempt = (nextNode.Value.ToVector3() - Actor.transform.position).normalized;
                        Actor.CurMoveAttempt = Actor.CurMoveAttempt.GetSingleDirectionVectorXZ();
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
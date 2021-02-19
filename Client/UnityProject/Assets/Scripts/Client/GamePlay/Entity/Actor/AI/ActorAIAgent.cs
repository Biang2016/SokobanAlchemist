using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.GameDataFormat.Grid;
using JetBrains.Annotations;
using UnityEngine;

public class ActorAIAgent
{
    internal static bool ENABLE_ACTOR_AI_AGENT_LOG = false;

    internal Actor Actor;
    private List<Marker> NavTrackMarkers = new List<Marker>();

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
    }

    public void FixedUpdate()
    {
        if (isStop) return;
        if (!BattleManager.Instance.IsStart) return;
        if (!Actor.GraphOwner.isRunning) Actor.GraphOwner?.StartBehaviour();
        MoveToDestination();
    }

    public void FixedUpdateAfterMove()
    {
        if (isStop) return;
        if (Actor.WorldGP == Actor.LastWorldGP && IsPathFinding)
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
    public bool IsPathFinding => CurrentPath.Count > 0;
    private GridPos3D currentDestination;

    [NotNull]
    public List<ActorPathFinding.Node> CurrentPath = new List<ActorPathFinding.Node>(50);

    private ActorPathFinding.Node currentNode;
    private ActorPathFinding.Node nextNode;

    public void InterruptCurrentPathFinding()
    {
        if (!IsPathFinding) return;
        while (CurrentPath.Count > 0 && CurrentPath[CurrentPath.Count - 1] != nextNode)
        {
            ActorPathFinding.Node lastNode = CurrentPath[CurrentPath.Count - 1];
            lastNode.Release();
            CurrentPath.RemoveAt(CurrentPath.Count - 1);
        }
    }

    /// <summary>
    /// 清除当前寻路，但是要求是完成下一个Node，完成后自动Clear，否则会出现停止瞬间snap到grid的bug
    /// </summary>
    public void ClearPathFinding()
    {
        foreach (ActorPathFinding.Node node in CurrentPath)
        {
            node.Release();
        }

        CurrentPath.Clear();
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
        float dist = (Actor.WorldGP - currentDestination).magnitude;
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
                Vector3 forward = dest - Actor.transform.position;
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

        bool suc = ActorPathFinding.FindPath(Actor.WorldGP, currentDestination, CurrentPath, KeepDistanceMin, KeepDistanceMax, destinationType);
        if (IsPathFinding)
        {
            currentNode = CurrentPath.Count > 0 ? CurrentPath[0] : null;
            nextNode = CurrentPath.Count > 1 ? CurrentPath[1] : null;

            // 绘制Debug寻路点
            ClearNavTrackMarkers();
            if (ConfigManager.ShowEnemyPathFinding)
            {
                int count = 0;
                foreach (ActorPathFinding.Node node in CurrentPath)
                {
                    MarkerType mt = count == CurrentPath.Count - 1 ? MarkerType.NavTrackMarker_Final : MarkerType.NavTrackMarker;
                    count++;
                    Marker marker = Marker.BaseInitialize(mt, BattleManager.Instance.NavTrackMarkerRoot);
                    marker.transform.position = node.GridPos3D;
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
        if (IsPathFinding)
        {
            if (nextNode != null)
            {
                // 有箱子或Actor挡路，停止寻路
                Box box = WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(nextNode.GridPos3D, out WorldModule module, out GridPos3D _);
                bool actorOccupied = WorldManager.Instance.CurrentWorld.CheckActorOccupiedGrid(nextNode.GridPos3D, Actor.GUID);
                if ((box && !box.Passable) || actorOccupied)
                {
                    Vector3 diff = currentNode.GridPos3D - Actor.transform.position;
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
                    Vector3 diff = nextNode.GridPos3D - Actor.transform.position;
                    if (diff.magnitude < 0.1f)
                    {
                        int nextNodeIndex = CurrentPath.IndexOf(nextNode);
                        ActorPathFinding.Node nextNodeAfterNextNode = CurrentPath.Count > nextNodeIndex + 1 ? CurrentPath[nextNodeIndex + 1] : null;
                        bool checkArriveDest = nextNode == CurrentPath[CurrentPath.Count - 1] || (LastNodeOccupied && nextNodeAfterNextNode == CurrentPath[CurrentPath.Count - 1]);
                        if (checkArriveDest)
                        {
                            if (LastNodeOccupied && nextNodeAfterNextNode != null && nextNodeAfterNextNode == CurrentPath[CurrentPath.Count - 1])
                            {
                                Actor.CurForward = (nextNodeAfterNextNode.GridPos3D - nextNode.GridPos3D).normalized;
                            }

                            ClearPathFinding();
                            return;
                        }

                        currentNode = nextNode;
                        nextNode = nextNodeAfterNextNode;
                    }
                    else if (diff.magnitude > 1.1f && !diff.x.Equals(0) && !diff.z.Equals(0)) // 由于某些意外，下一个路径点和目前离得较远，会发生角色原地打转不寻路的bug，此处强行重置
                    {
                        ClearPathFinding();
                    }

                    if (nextNode != null)
                    {
                        Actor.CurMoveAttempt = (nextNode.GridPos3D - Actor.transform.position).normalized;
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
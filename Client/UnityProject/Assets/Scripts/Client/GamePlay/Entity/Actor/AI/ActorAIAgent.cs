using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.GameDataFormat.Grid;
using JetBrains.Annotations;
using NodeCanvas.Framework;
using UnityEngine;
using UnityEngine.Profiling;

public class ActorAIAgent
{
    internal Actor Actor;
    internal static bool ENABLE_ACTOR_AI_AGENT_LOG = false;

    public ActorAIAgent(Actor actor)
    {
        Actor = actor;
        Stop();
    }

    public void Start()
    {
        isStop = false;
    }

    public void ActorTick(float interval)
    {
        if (isStop) return;
        if (!Actor.GraphOwner.isRunning) Actor.GraphOwner?.StartBehaviour();
        RefreshTargetGP();
        RefreshThreatEntityRank();
        RefreshExceptionJudgment(interval);

        MoveToDestination();
    }

    public void ActorTickAfterMove(float interval)
    {
        if (isStop) return;
        if (Actor.WorldGP == Actor.LastWorldGP && IsPathFinding)
        {
            StuckWithNavTask_Tick += interval;
        }
        else
        {
            StuckWithNavTask_Tick = 0;
        }
    }

    public void AITick(float interval)
    {
        RecheckPathFindingPassable();
    }

    private bool isStop;

    public void Stop()
    {
        isStop = true;

        foreach (KeyValuePair<TargetEntityType, AIAgentTarget> kv in AIAgentTargetDict)
        {
            kv.Value.ClearTarget();
        }

        ClearPathFinding();
        ClearNavTrackMarkers();
        ClearExceptionJudgment();
    }

    #region 目标搜索

    public enum TargetEntityType
    {
        Navigate,
        Attack,
        Guard,
        Follow,
    }

    public class AIAgentTarget
    {
        public Entity TargetEntity
        {
            get { return targetEntity; }
            set
            {
                targetEntity = value;
                if (value != null)
                {
                    targetGP = targetEntity.EntityBaseCenter.ToGridPos3D();
                }
            }
        }

        private Entity targetEntity;

        public GridPos3D TargetGP
        {
            get { return targetGP; }
            set
            {
                targetGP = value;
                targetEntity = null;
            }
        }

        private GridPos3D targetGP;

        public bool HasTarget => targetEntity.IsNotNullAndAlive() || targetGP != GridPos3D.One * -1;

        public void RefreshTargetGP()
        {
            if (targetEntity.IsNotNullAndAlive())
            {
                targetGP = targetEntity.EntityBaseCenter.ToGridPos3D();
            }
            else
            {
                targetEntity = null;
            }
        }

        public void ClearTarget()
        {
            targetEntity = null;
            targetGP = GridPos3D.One * -1;
        }
    }

    public Dictionary<TargetEntityType, AIAgentTarget> AIAgentTargetDict = new Dictionary<TargetEntityType, AIAgentTarget>
    {
        {TargetEntityType.Navigate, new AIAgentTarget()},
        {TargetEntityType.Attack, new AIAgentTarget()},
        {TargetEntityType.Guard, new AIAgentTarget()},
        {TargetEntityType.Follow, new AIAgentTarget()},
    };

    public Dictionary<Actor, int> ThreatActorRank = new Dictionary<Actor, int>();
    private List<Actor> removeActorsFromThreatEntityRank = new List<Actor>(4);

    public void RefreshTargetGP()
    {
        foreach (KeyValuePair<TargetEntityType, AIAgentTarget> kv in AIAgentTargetDict)
        {
            kv.Value.RefreshTargetGP();
        }
    }

    public void RefreshThreatEntityRank()
    {
        removeActorsFromThreatEntityRank.Clear();
        foreach (KeyValuePair<Actor, int> kv in ThreatActorRank)
        {
            if (!kv.Key.IsNotNullAndAlive()) removeActorsFromThreatEntityRank.Add(kv.Key);
        }

        foreach (Actor ator in removeActorsFromThreatEntityRank)
        {
            ThreatActorRank.Remove(ator);
        }
    }

    public Actor GetThreatActor()
    {
        int maxThreat = int.MinValue;
        Actor threatActor = null;
        foreach (KeyValuePair<Actor, int> kv in ThreatActorRank)
        {
            if (kv.Key.IsNotNullAndAlive())
            {
                if (kv.Value > maxThreat)
                {
                    maxThreat = kv.Value;
                    threatActor = kv.Key;
                }
            }
        }

        return threatActor;
    }

    #endregion

    #region NavTrackMarkers

    private List<Marker> NavTrackMarkers = new List<Marker>();

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

    /// <summary>
    /// 绘制Debug寻路点
    /// </summary>
    private void DrawNavTrackMarkers()
    {
        ClearNavTrackMarkers();
        if (ConfigManager.ShowEnemyPathFinding)
        {
            int count = 0;
            foreach (ActorPathFinding.Node node in CurrentPath)
            {
                MarkerType mt = count == CurrentPath.Count - 1 ? MarkerType.NavTrackMarker_Final : MarkerType.NavTrackMarker;
                count++;
                Marker marker = Marker.BaseInitialize(mt, BattleManager.Instance.NavTrackMarkerRoot);
                marker.transform.position = node.GridPos3D_PF.ConvertPathFindingNodeGPToWorldPosition(Actor.ActorWidth);
                NavTrackMarkers.Add(marker);
            }
        }
    }

    #endregion

    #region PathFinding

    public float StuckWithNavTask_Tick;
    private bool LastNodeOccupied;
    public bool IsPathFinding => CurrentPath.Count > 0;
    private GridPos3D currentDestination_PF;

    [NotNull]
    public List<ActorPathFinding.Node> CurrentPath = new List<ActorPathFinding.Node>(50);

    private ActorPathFinding.Node currentNode;
    private ActorPathFinding.Node nextNode;

    internal int NextStraightNodeCount = 1;

    public void InterruptCurrentPathFinding()
    {
        if (!IsPathFinding) return;
        while (CurrentPath.Count > 0 && CurrentPath[CurrentPath.Count - 1] != nextNode)
        {
            ActorPathFinding.Node lastNode = CurrentPath[CurrentPath.Count - 1];
            lastNode.Release();
            CurrentPath.RemoveAt(CurrentPath.Count - 1);
        }

        RecalculateNextStraightNodeCount();
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
        currentDestination_PF = GridPos3D.Zero;
        NextStraightNodeCount = 0;
        ClearNavTrackMarkers();
        if (ENABLE_ACTOR_AI_AGENT_LOG) Debug.Log($"{Actor.name} [AIAgent] ClearPathFinding");
    }

    public enum SetDestinationRetCode
    {
        AlreadyArrived,
        TooClose,
        Suc,
        Failed,
    }

    public Status SetDestinationToWorldGP(GridPos3D worldGP, float keepDistanceMin, float keepDistanceMax)
    {
        SetDestinationRetCode retCode = SetDestination(((Vector3) worldGP).ConvertWorldPositionToPathFindingNodeGP(Actor.ActorWidth), keepDistanceMin, keepDistanceMax, false, ActorPathFinding.DestinationType.Actor);
        switch (retCode)
        {
            case SetDestinationRetCode.AlreadyArrived:
            case SetDestinationRetCode.TooClose:
            {
                return Status.Success;
            }
            case SetDestinationRetCode.Suc:
            {
                return Status.Success;
            }
            case SetDestinationRetCode.Failed:
            {
                return Status.Failure;
            }
        }

        return Status.Failure;
    }

    public SetDestinationRetCode SetDestination(GridPos3D destination_PF, float keepDistanceMin, float keepDistanceMax, bool lastNodeOccupied, ActorPathFinding.DestinationType destinationType)
    {
        currentDestination_PF = destination_PF;
        LastNodeOccupied = lastNodeOccupied;
        float dist = (Actor.WorldGP_PF - currentDestination_PF).magnitude;
        if (dist <= keepDistanceMax + (keepDistanceMax.Equals(0) && LastNodeOccupied ? 1 : 0) && dist >= keepDistanceMin)
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
                Vector3 forward = (destination_PF - Actor.WorldGP_PF).Normalized();
                if (forward.normalized.magnitude > 0)
                {
                    Actor.CurForward = forward.normalized;
                }

                InterruptCurrentPathFinding();
                return SetDestinationRetCode.AlreadyArrived;
            }
        }

        if (dist <= keepDistanceMin)
        {
            InterruptCurrentPathFinding();
            return SetDestinationRetCode.AlreadyArrived;
        }

        Profiler.BeginSample("FindPath");
        //string pathFindingDesc = $"nav to {destination_PF}";
        //ActorPathFinding.InvokeTimes = 0;
        bool suc = ActorPathFinding.FindPath(Actor.WorldGP_PF, currentDestination_PF, Actor.transform.position, CurrentPath, keepDistanceMin, keepDistanceMax, destinationType, Actor.ActorWidth, Actor.ActorHeight, Actor.GUID);
        //if (ActorPathFinding.InvokeTimes > 500)
        //{
        //Debug.Log($"{Actor.name} ActorPathFinding.InvokeTimes: {ActorPathFinding.InvokeTimes} {pathFindingDesc}");
        //}

        Profiler.EndSample();
        if (IsPathFinding)
        {
            currentNode = CurrentPath.Count > 0 ? CurrentPath[0] : null;
            nextNode = CurrentPath.Count > 1 ? CurrentPath[1] : null;
            RecalculateNextStraightNodeCount();
            DrawNavTrackMarkers();
            StuckWithNavTask_Tick = 0;
            return SetDestinationRetCode.Suc;
        }
        else
        {
            InterruptCurrentPathFinding();
            return SetDestinationRetCode.Failed;
        }
    }

    /// <summary>
    /// 计算当前面前的直线行走节点数量
    /// </summary>
    private void RecalculateNextStraightNodeCount()
    {
        NextStraightNodeCount = 0;
        if (currentNode == null || nextNode == null) return;
        GridPos3D diff = nextNode.GridPos3D_PF - currentNode.GridPos3D_PF;
        int nextNodeIndex = CurrentPath.IndexOf(nextNode);
        for (int i = nextNodeIndex; i < CurrentPath.Count; i++)
        {
            if (CurrentPath[i].GridPos3D_PF - CurrentPath[i - 1].GridPos3D_PF == diff)
            {
                NextStraightNodeCount++;
            }
            else
            {
                break;
            }
        }
    }

    private float arriveThreshold = 0.2f;

    public void RecheckPathFindingPassable()
    {
        Profiler.BeginSample("AISA_RecheckPathFindingPassable");
        Actor.CurMoveAttempt = Vector3.zero;
        if (IsPathFinding)
        {
            if (nextNode != null)
            {
                if (!ActorPathFinding.CheckSpaceAvailableForActorOccupation(nextNode.GridPos3D_PF, Actor.transform.position, Actor.ActorWidth, Actor.ActorHeight, Actor.GUID)) // 有箱子或Actor挡路，停止寻路
                {
                    Vector3 diff = currentNode.GridPos3D_PF - Actor.WorldGP_PF;
                    if (diff.magnitude < arriveThreshold)
                    {
                        ClearPathFinding(); // 已经到达当前节点 -> 结束寻路
                    }
                    else
                    {
                        InterruptCurrentPathFinding(); // 未到达 -> 打断
                    }
                }
            }
        }

        Profiler.EndSample();
    }

    public void MoveToDestination()
    {
        Actor.CurMoveAttempt = Vector3.zero;
        if (IsPathFinding)
        {
            if (nextNode != null)
            {
                Vector3 diff = nextNode.GridPos3D_PF - Actor.WorldGP_PF;
                if (diff.magnitude < arriveThreshold) // 已经到达下一节点
                {
                    int nextNodeIndex = CurrentPath.IndexOf(nextNode);
                    ActorPathFinding.Node nextNodeAfterNextNode = CurrentPath.Count > nextNodeIndex + 1 ? CurrentPath[nextNodeIndex + 1] : null;
                    bool checkArriveDest = nextNode == CurrentPath[CurrentPath.Count - 1] || (LastNodeOccupied && nextNodeAfterNextNode == CurrentPath[CurrentPath.Count - 1]);
                    if (checkArriveDest)
                    {
                        if (LastNodeOccupied && nextNodeAfterNextNode != null && nextNodeAfterNextNode == CurrentPath[CurrentPath.Count - 1])
                        {
                            Actor.CurForward = (nextNodeAfterNextNode.GridPos3D_PF - nextNode.GridPos3D_PF).Normalized();
                        }

                        ClearPathFinding();
                        return;
                    }

                    currentNode = nextNode;
                    nextNode = nextNodeAfterNextNode;
                    RecalculateNextStraightNodeCount();
                }
                else if (diff.magnitude > 1 + arriveThreshold && !diff.x.Equals(0) && !diff.z.Equals(0)) // 由于某些意外，下一个路径点和目前离得较远，会发生角色原地打转不寻路的bug，此处强行重置
                {
                    ClearPathFinding();
                }

                if (nextNode != null)
                {
                    Actor.CurMoveAttempt = (nextNode.GridPos3D_PF - Actor.WorldGP_PF).Normalized();
                    Actor.CurMoveAttempt = Actor.CurMoveAttempt.GetSingleDirectionVectorXZ();
                }
            }
        }
    }

    public GridPos3D GetCurrentPathFindingDestination()
    {
        return currentDestination_PF;
    }

    #endregion

    #region Exceptions

    internal bool IsStuckWithBoxes;
    internal float StuckWithBoxesDuration;

    public void ClearExceptionJudgment()
    {
        IsStuckWithBoxes = false;
        StuckWithBoxesDuration = 0;
    }

    private void RefreshExceptionJudgment(float interval)
    {
        IsStuckWithBoxes = CheckIsStuckWithBoxes();
        if (IsStuckWithBoxes)
        {
            StuckWithBoxesDuration += interval;
        }
        else
        {
            StuckWithBoxesDuration = 0;
        }
    }

    public bool CheckIsStuckWithBoxes()
    {
        if (Actor.IsFrozen) return false;
        foreach (GridPos3D offset in Actor.GetEntityOccupationGPs_Rotated())
        {
            GridPos3D gridPos = Actor.WorldGP + offset;
            Entity entity = WorldManager.Instance.CurrentWorld.GetImpassableEntityByGridPosition(gridPos, Actor.GUID, out WorldModule _, out GridPos3D _);
            if (entity is Box && entity.IsNotNullAndAlive())
            {
                return true;
            }
        }

        return false;
    }

    public void DestroyStuckBoxes()
    {
        foreach (GridPos3D offset in Actor.GetEntityOccupationGPs_Rotated())
        {
            GridPos3D gridPos = Actor.WorldGP + offset;
            Entity entity = WorldManager.Instance.CurrentWorld.GetImpassableEntityByGridPosition(gridPos, Actor.GUID, out WorldModule _, out GridPos3D _);
            if (entity is Box && entity.IsNotNullAndAlive())
            {
                entity.EntityBuffHelper.Damage(10000, EntityBuffAttribute.ExplodeDamage, 0);
            }
        }
    }

    #endregion
}
using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;

public class ActorControllerHelper : ActorMonoHelper
{
    public Dictionary<TargetEntityType, AgentTarget> AgentTargetDict = new Dictionary<TargetEntityType, AgentTarget>
    {
        {TargetEntityType.Navigate, new AgentTarget {TargetEntityType = TargetEntityType.Navigate}},
        {TargetEntityType.Attack, new AgentTarget {TargetEntityType = TargetEntityType.Attack}},
        {TargetEntityType.Guard, new AgentTarget {TargetEntityType = TargetEntityType.Guard}},
        {TargetEntityType.Follow, new AgentTarget {TargetEntityType = TargetEntityType.Follow}},
        {TargetEntityType.Self, new AgentTarget {TargetEntityType = TargetEntityType.Self}},
    };

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
        AgentTargetDict[TargetEntityType.Self].Setup(Entity);
    }

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
        foreach (KeyValuePair<TargetEntityType, AgentTarget> kv in AgentTargetDict)
        {
            kv.Value.ClearTarget();
        }
    }

    public virtual void OnTick(float interval)
    {
        RefreshTargetGP();
    }

    public void RefreshTargetGP()
    {
        foreach (KeyValuePair<TargetEntityType, AgentTarget> kv in AgentTargetDict)
        {
            kv.Value.RefreshTargetGP();
        }
    }

    public virtual void OnFixedUpdate()
    {
    }

    public virtual void OnUpdate()
    {
    }
}

public class AgentTarget
{
    public TargetEntityType TargetEntityType;

    public Entity TargetEntity
    {
        get { return targetEntity; }
        set
        {
            if (value != null)
            {
                if (value.CanBeThreatened)
                {
                    targetEntity = value;
                    targetGP = targetEntity.EntityBaseCenter.ToGridPos3D();
                }
                else
                {
                    targetEntity = null;
                    targetGP = GridPos3D.One * -1;
                }
            }
            else
            {
                targetEntity = null;
                targetGP = GridPos3D.One * -1;
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

    public void Setup(Entity _targetEntity)
    {
        targetEntity = _targetEntity;
    }

    public bool HasTarget
    {
        get { return (targetEntity.IsNotNullAndAlive() && targetEntity.CanBeThreatened) || targetGP != GridPos3D.One * -1; }
    }

    public void RefreshTargetGP()
    {
        if (targetEntity.IsNotNullAndAlive())
        {
            if (TargetEntityType != TargetEntityType.Self && !targetEntity.CanBeThreatened)
            {
                TargetEntity = null;
            }
            else
            {
                targetGP = targetEntity.EntityBaseCenter.ToGridPos3D();
            }
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

public enum TargetEntityType
{
    Navigate,
    Attack,
    Guard,
    Follow,
    Self,
}
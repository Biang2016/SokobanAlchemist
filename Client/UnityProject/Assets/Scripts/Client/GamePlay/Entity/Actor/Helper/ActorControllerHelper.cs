using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using NodeCanvas.Framework;

public class ActorControllerHelper : ActorMonoHelper
{
    public Dictionary<TargetEntityType, AgentTarget> AgentTargetDict = new Dictionary<TargetEntityType, AgentTarget>
    {
        {TargetEntityType.Navigate, new AgentTarget()},
        {TargetEntityType.Attack, new AgentTarget()},
        {TargetEntityType.Guard, new AgentTarget()},
        {TargetEntityType.Follow, new AgentTarget()},
        {TargetEntityType.Self, new AgentTarget()},
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
}

public class AgentTarget
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

    public void Setup(Entity _targetEntity)
    {
        targetEntity = _targetEntity;
    }

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

public enum TargetEntityType
{
    Navigate,
    Attack,
    Guard,
    Follow,
    Self,
}
using System.Collections.Generic;
using UnityEngine;

public class CompositeEnemyActor : EnemyActor
{
    public Dictionary<string, EnemyActor> ChildEnemyActors = new Dictionary<string, EnemyActor>();

    public override void OnUsed()
    {
        base.OnUsed();
    }

    protected override void FixedUpdate()
    {
        if (!IsRecycled)
        {
            
        }

        base.FixedUpdate();
    }
}
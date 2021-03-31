using UnityEngine;

public class CompositeEnemyActor : EnemyActor
{
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
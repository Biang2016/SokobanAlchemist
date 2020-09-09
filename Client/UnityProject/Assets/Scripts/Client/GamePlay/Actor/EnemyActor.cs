public class EnemyActor : Actor
{
    internal int AIUpdateInterval = 10;
    private int AIUpdateIntervalTick = 0;

    protected override void FixedUpdate()
    {
        if (!IsRecycled)
        {
            if (AIUpdateIntervalTick < AIUpdateInterval)
            {
                AIUpdateIntervalTick++;
            }
            else
            {
                GraphOwner.UpdateBehaviour();
                AIUpdateIntervalTick = 0;
            }

            ActorAIAgent.Update();
            MoveInternal();
        }

        base.FixedUpdate();
    }
}
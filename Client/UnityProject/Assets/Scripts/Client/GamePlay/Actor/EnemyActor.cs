public class EnemyActor : Actor
{
    public int AIUpdateInterval = 10;
    private int AIUpdateIntervalTick = 0;

    protected override void FixedUpdate()
    {
        if (AIUpdateIntervalTick < AIUpdateInterval)
        {
            AIUpdateIntervalTick++;
        }
        else
        {
            BehaviourTreeOwner.UpdateBehaviour();
            AIUpdateIntervalTick = 0;
        }

        ActorAIAgent.Update();
        MoveInternal();
        base.FixedUpdate();
    }
}
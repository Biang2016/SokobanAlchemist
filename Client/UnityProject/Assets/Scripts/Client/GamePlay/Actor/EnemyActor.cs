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
            ActorAIAgent.AITick();
            AIUpdateIntervalTick = 0;
        }

        MoveInternal();
        base.FixedUpdate();
    }
}
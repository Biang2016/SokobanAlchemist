using BiangLibrary.Singleton;

public class GameStateManager : TSingletonBaseManager<GameStateManager>
{
    private GameState state = GameState.Default;

    public void SetState(GameState newState)
    {
        if (state != newState)
        {
            switch (state)
            {
                case GameState.Fighting:
                {
                    break;
                }
                case GameState.Building:
                {
                    break;
                }
                case GameState.ESC:
                {
                    break;
                }
            }

            state = newState;
            switch (state)
            {
                case GameState.Fighting:
                {
                    Resume();
                    break;
                }
                case GameState.Building:
                {
                    Pause();
                    break;
                }
                case GameState.ESC:
                {
                    Pause();
                    break;
                }
            }
        }
    }

    public GameState GetState()
    {
        return state;
    }

    private void Pause()
    {
    }

    private void Resume()
    {
    }
}

public enum GameState
{
    Default,
    Fighting,
    Building,
    ESC,
}
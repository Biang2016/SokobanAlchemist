using BiangStudio.GamePlay;
using BiangStudio.GamePlay.UI;
using BiangStudio.Log;
using BiangStudio.Messenger;
using BiangStudio.Singleton;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager : MonoSingleton<ClientGameManager>
{
    #region Managers

    #region Mono

    private AudioManager AudioManager => AudioManager.Instance;
    private CameraManager CameraManager => CameraManager.Instance;
    private UIManager UIManager => UIManager.Instance;

    #endregion

    #region TSingletonBaseManager

    #region Resources

    private ConfigManager ConfigManager => ConfigManager.Instance;
    private LayerManager LayerManager => LayerManager.Instance;
    private PrefabManager PrefabManager => PrefabManager.Instance;
    private GameObjectPoolManager GameObjectPoolManager => GameObjectPoolManager.Instance;

    #endregion

    #region Framework

    private ControlManager ControlManager => ControlManager.Instance;
    private GameStateManager GameStateManager => GameStateManager.Instance;
    private RoutineManager RoutineManager => RoutineManager.Instance;

    #endregion

    #region GamePlay

    #region Level

    private WorldManager WorldManager => WorldManager.Instance;
    private BattleManager BattleManager => BattleManager.Instance;

    public Messenger BattleMessenger => BattleManager.BattleMessenger;

    private FXManager FXManager => FXManager.Instance;

    #endregion

    #endregion

    #endregion

    #endregion

    public DebugConsole DebugConsole;

    [LabelText("开局世界类型")]
    public string StartWorldName;

    private void Awake()
    {
        UIManager.Init(
            (prefabName) => Instantiate(PrefabManager.GetPrefab(prefabName)),
            Debug.LogError,
            () => ControlManager.Instance.Common_MouseLeft.Down,
            () => ControlManager.Instance.Common_MouseRight.Down,
            () => ControlManager.Instance.Common_Exit.Down,
            () => ControlManager.Instance.Common_Confirm.Down,
            () => ControlManager.Instance.Common_Tab.Down
        );

        ConfigManager.Awake();
        LayerManager.Awake();
        PrefabManager.Awake();
        GameObjectPoolManager.Init(new GameObject("GameObjectPoolRoot").transform);
        GameObjectPoolManager.Awake();

        RoutineManager.LogErrorHandler = Debug.LogError;
        RoutineManager.Awake();
        GameStateManager.Awake();
        DebugConsole.OnDebugConsoleKeyDownHandler = () => ControlManager.Instance.Common_Debug.Down;
        DebugConsole.OnDebugConsoleToggleHandler = (enable) => { ControlManager.Instance.EnableBattleInputActions(!enable); };

        WorldManager.Awake();
        BattleManager.Awake();
        FXManager.Awake();

        ControlManager.Awake();
    }

    private void Start()
    {
        ConfigManager.Start();
        LayerManager.Start();
        PrefabManager.Start();
        GameObjectPoolManager.Start();

        RoutineManager.Start();
        GameStateManager.Start();

        WorldManager.Start();
        BattleManager.Start();
        FXManager.Start();

        ControlManager.Start();

        UIManager.Instance.ShowUIForms<DebugPanel>();
#if !DEBUG
        UIManager.Instance.CloseUIForm<DebugPanel>();
#endif

        StartGame();
    }

    private void Update()
    {
        if (ControlManager.Common_RestartGame.Up)
        {
            SceneManager.LoadScene(0);
            return;
        }

        ConfigManager.Update(Time.deltaTime);
        LayerManager.Update(Time.deltaTime);
        PrefabManager.Update(Time.deltaTime);
        GameObjectPoolManager.Update(Time.deltaTime);

        RoutineManager.Update(Time.deltaTime, Time.frameCount);
        GameStateManager.Update(Time.deltaTime);

        WorldManager.Update(Time.deltaTime);
        BattleManager.Update(Time.deltaTime);
        FXManager.Update(Time.deltaTime);

        ControlManager.Update(Time.deltaTime);
    }

    void LateUpdate()
    {
        ConfigManager.LateUpdate(Time.deltaTime);
        LayerManager.LateUpdate(Time.deltaTime);
        PrefabManager.LateUpdate(Time.deltaTime);
        GameObjectPoolManager.LateUpdate(Time.deltaTime);

        RoutineManager.LateUpdate(Time.deltaTime);
        GameStateManager.LateUpdate(Time.deltaTime);

        WorldManager.LateUpdate(Time.deltaTime);
        BattleManager.LateUpdate(Time.deltaTime);
        FXManager.LateUpdate(Time.deltaTime);

        ControlManager.LateUpdate(Time.deltaTime);
    }

    void FixedUpdate()
    {
        ConfigManager.FixedUpdate(Time.fixedDeltaTime);
        LayerManager.FixedUpdate(Time.fixedDeltaTime);
        PrefabManager.FixedUpdate(Time.fixedDeltaTime);
        GameObjectPoolManager.FixedUpdate(Time.fixedDeltaTime);

        RoutineManager.FixedUpdate(Time.fixedDeltaTime);
        GameStateManager.FixedUpdate(Time.fixedDeltaTime);

        WorldManager.FixedUpdate(Time.fixedDeltaTime);
        BattleManager.FixedUpdate(Time.fixedDeltaTime);
        FXManager.FixedUpdate(Time.fixedDeltaTime);

        ControlManager.FixedUpdate(Time.fixedDeltaTime);
    }

    private void StartGame()
    {
        BattleManager.Instance.StartBattle();
    }

    private void ShutDownGame()
    {
    }
}
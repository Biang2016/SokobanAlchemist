using System.Collections.Generic;
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
    private ActiveSkillAgent ActiveSkillAgent => ActiveSkillAgent.Instance;

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

    private BattleManager BattleManager => BattleManager.Instance;
    private WorldManager WorldManager => WorldManager.Instance;
    private ProjectileManager ProjectileManager => ProjectileManager.Instance;
    private UIBattleTipManager UIBattleTipManager => UIBattleTipManager.Instance;
    public Messenger BattleMessenger => BattleManager.BattleMessenger;

    private FXManager FXManager => FXManager.Instance;

    #endregion

    #endregion

    #endregion

    #endregion

    public DebugConsole DebugConsole;

    [LabelText("开局世界类型")]
    [ValueDropdown("GetAllWorldNames")]
    public string StartWorldName = "None";

    [Button("刷新世界名称列表")]
    private void RefreshWorldNameList()
    {
        ConfigManager.LoadAllConfigs();
        GetAllWorldNames = ConfigManager.GetAllWorldNames();
    }

    private IEnumerable<string> GetAllWorldNames;

    public static string DebugChangeWorldName = null;
    public static string DebugChangeWorldBornPointAlias = null;

    internal int FixedFrameRate;
    internal int CurrentFixedFrameCount;
    internal int CurrentFixedFrameCount_Mod_FixedFrameRate;

    private void Awake()
    {
        CurrentFixedFrameCount = 0;
        FixedFrameRate = Mathf.RoundToInt(1f / Time.fixedDeltaTime);
        UIManager.Init(
            (prefabName) => Instantiate(PrefabManager.GetPrefab(prefabName)),
            Debug.LogError,
            () => ControlManager.Instance.Common_MouseLeft.Down,
            () => ControlManager.Instance.Common_MouseRight.Down,
            () => ControlManager.Instance.Common_Exit.Down,
            () => ControlManager.Instance.Common_Confirm.Down,
            () => ControlManager.Instance.Common_Tab.Down
        );

        ControlManager.Awake();

        ConfigManager.Awake();
        LayerManager.Awake();
        PrefabManager.Awake();
        if (!GameObjectPoolManager.IsInit)
        {
            Transform root = new GameObject("GameObjectPool").transform;
            DontDestroyOnLoad(root.gameObject);
            GameObjectPoolManager.Init(root);
            GameObjectPoolManager.Awake();
        }

        RoutineManager.LogErrorHandler = Debug.LogError;
        RoutineManager.Awake();
        GameStateManager.Awake();
        DebugConsole.OnDebugConsoleKeyDownHandler = () => ControlManager.Instance.Common_Debug.Down;
        DebugConsole.OnDebugConsoleToggleHandler = (enable) => { ControlManager.Instance.EnableBattleInputActions(!enable); };

        BattleManager.Awake();
        WorldManager.Awake();
        ProjectileManager.Awake();
        ProjectileManager.Init(new GameObject("ProjectileRoot").transform);
        UIBattleTipManager.Awake();
        FXManager.Awake();
        FXManager.Init(new GameObject("FXRoot").transform);
    }

    private void Start()
    {
        ControlManager.Start();

        ConfigManager.Start();
        LayerManager.Start();
        PrefabManager.Start();
        GameObjectPoolManager.Start();

        RoutineManager.Start();
        GameStateManager.Start();

        BattleManager.Start();
        WorldManager.Start();
        ProjectileManager.Start();
        UIBattleTipManager.Start();
        FXManager.Start();

        UIManager.Instance.ShowUIForms<DebugPanel>();
#if !DEBUG
        UIManager.Instance.CloseUIForm<DebugPanel>();
#endif
        UIManager.Instance.ShowUIForms<InGameUIPanel>();

        StartGame();
    }

    private void Update()
    {
        ControlManager.Update(Time.deltaTime);

        ConfigManager.Update(Time.deltaTime);
        LayerManager.Update(Time.deltaTime);
        PrefabManager.Update(Time.deltaTime);
        GameObjectPoolManager.Update(Time.deltaTime);

        RoutineManager.Update(Time.deltaTime, Time.frameCount);
        GameStateManager.Update(Time.deltaTime);

        BattleManager.Update(Time.deltaTime);
        WorldManager.Update(Time.deltaTime);
        ProjectileManager.Update(Time.deltaTime);
        UIBattleTipManager.Update(Time.deltaTime);
        FXManager.Update(Time.deltaTime);
    }

    void LateUpdate()
    {
        ControlManager.LateUpdate(Time.deltaTime);

        ConfigManager.LateUpdate(Time.deltaTime);
        LayerManager.LateUpdate(Time.deltaTime);
        PrefabManager.LateUpdate(Time.deltaTime);
        GameObjectPoolManager.LateUpdate(Time.deltaTime);

        RoutineManager.LateUpdate(Time.deltaTime);
        GameStateManager.LateUpdate(Time.deltaTime);

        BattleManager.LateUpdate(Time.deltaTime);
        WorldManager.LateUpdate(Time.deltaTime);
        ProjectileManager.LateUpdate(Time.deltaTime);
        UIBattleTipManager.LateUpdate(Time.deltaTime);
        FXManager.LateUpdate(Time.deltaTime);
    }

    void FixedUpdate()
    {
        CurrentFixedFrameCount++;
        CurrentFixedFrameCount_Mod_FixedFrameRate = CurrentFixedFrameCount % FixedFrameRate;
        ControlManager.FixedUpdate(Time.fixedDeltaTime);
        if (ControlManager.Common_RestartGame.Up)
        {
            ReloadGame();
            return;
        }

        if (ControlManager.Battle_LeftRotateCamera.Up)
        {
            CameraManager.Instance.FieldCamera.CameraLeftRotate();
        }

        if (ControlManager.Battle_RightRotateCamera.Up)
        {
            CameraManager.Instance.FieldCamera.CameraRightRotate();
        }

        ConfigManager.FixedUpdate(Time.fixedDeltaTime);
        LayerManager.FixedUpdate(Time.fixedDeltaTime);
        PrefabManager.FixedUpdate(Time.fixedDeltaTime);
        GameObjectPoolManager.FixedUpdate(Time.fixedDeltaTime);

        RoutineManager.FixedUpdate(Time.fixedDeltaTime);
        GameStateManager.FixedUpdate(Time.fixedDeltaTime);

        BattleManager.FixedUpdate(Time.fixedDeltaTime);
        WorldManager.FixedUpdate(Time.fixedDeltaTime);
        ProjectileManager.FixedUpdate(Time.fixedDeltaTime);
        UIBattleTipManager.FixedUpdate(Time.fixedDeltaTime);
        FXManager.FixedUpdate(Time.fixedDeltaTime);
    }

    private void StartGame()
    {
        BattleManager.Instance.StartBattle();
    }

    public void ReloadGame()
    {
        ShutDownGame();
        SceneManager.LoadScene("MainScene");
    }

    private void ShutDownGame()
    {
        ControlManager.ShutDown();

        ActiveSkillAgent.StopAllCoroutines();
        
        FXManager.ShutDown();
        UIBattleTipManager.ShutDown();
        ProjectileManager.ShutDown();
        BattleManager.ShutDown();
        WorldManager.ShutDown();

        DebugConsole.OnDebugConsoleToggleHandler = null;
        DebugConsole.OnDebugConsoleKeyDownHandler = null;
        GameStateManager.ShutDown();
        RoutineManager.ShutDown();

        GameObjectPoolManager.ShutDown();
        PrefabManager.ShutDown();
        LayerManager.ShutDown();
        ConfigManager.ShutDown();
    }
}
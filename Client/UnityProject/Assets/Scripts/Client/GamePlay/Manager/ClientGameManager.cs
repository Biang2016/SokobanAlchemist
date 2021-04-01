using System.Collections;
using System.Collections.Generic;
using BiangLibrary.GamePlay;
using BiangLibrary.GamePlay.UI;
using BiangLibrary.Log;
using BiangLibrary.Messenger;
using BiangLibrary.Singleton;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager : MonoSingleton<ClientGameManager>
{
    public Material BoxMarchingSquareTerrainMat;
    public BoxMarchingTextureConfigMatrix BoxMarchingTextureConfigMatrix;

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
    private GameSaveManager GameSaveManager => GameSaveManager.Instance;
    public Messenger BattleMessenger => BattleManager.BattleMessenger;

    private FXManager FXManager => FXManager.Instance;

    #endregion

    #endregion

    #endregion

    #endregion

    public DebugConsole DebugConsole;
    public DebugPanel DebugPanel;
    public LoadingMapPanel LoadingMapPanel;
    public NoticePanel NoticePanel;

    public bool WarmUpPool_Editor = true;

    [LabelText("@\"开局世界类型\t\"+StartWorldName")]
    public TypeSelectHelper StartWorldName = new TypeSelectHelper {TypeDefineType = TypeDefineType.World};

    public static string DebugChangeWorldName = null;
    public static string DebugChangeWorldBornPointAlias = null;

    internal int FixedFrameRate;
    internal int FixedFrameRate_01X;
    internal int FixedFrameRate_5X;
    internal int CurrentFixedFrameCount;
    internal int CurrentFixedFrameCount_Mod_FixedFrameRate;
    internal int CurrentFixedFrameCount_Mod_FixedFrameRate_01X;
    internal int CurrentFixedFrameCount_Mod_FixedFrameRate_5X;

    private void Awake()
    {
        Instance = this;
        CurrentFixedFrameCount = 0;
        FixedFrameRate = Mathf.RoundToInt(1f / Time.fixedDeltaTime);
        FixedFrameRate_01X = Mathf.RoundToInt(FixedFrameRate * 0.1f);
        FixedFrameRate_5X = FixedFrameRate * 5;
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
        GameSaveManager.Awake();
        FXManager.Awake();
        FXManager.Init(new GameObject("FXRoot").transform);
    }

    private void Start()
    {
        CreateTextureArray();
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
        GameSaveManager.Start();
        FXManager.Start();

        StartCoroutine(Co_StartGame());
    }

    public void CreateTextureArray()
    {
        TextureFormat tf = BoxMarchingTextureConfigMatrix.Matrix[0, 0].format;
        Texture2DArray array = new Texture2DArray(1024, 1024, (1 + ConfigManager.TERRAIN_TYPE_COUNT) * ConfigManager.TERRAIN_TYPE_COUNT / 2, tf, false);
        int index = 0;
        for (int i = 0; i < ConfigManager.TERRAIN_TYPE_COUNT; i++)
        for (int j = 0; j < ConfigManager.TERRAIN_TYPE_COUNT; j++)
        {
            Texture2D texture = BoxMarchingTextureConfigMatrix.Matrix[i, j];
            if (texture != null)
            {
                Graphics.CopyTexture(texture, 0, 0, array, index, 0);
                index++;
            }
        }

        BoxMarchingSquareTerrainMat.SetTexture("_Albedo", array);
    }

    public bool IsGameLoading = false;

    private IEnumerator Co_StartGame()
    {
        IsGameLoading = true;
        NoticePanel = UIManager.Instance.ShowUIForms<NoticePanel>();
        LoadingMapPanel = UIManager.Instance.ShowUIForms<LoadingMapPanel>();
        LoadingMapPanel.Clear();
        LoadingMapPanel.SetBackgroundAlpha(0f);
        LoadingMapPanel.SetProgress(0, "Start Loading");
        yield return new WaitForSeconds(0.1f);
#if UNITY_EDITOR
        if (WarmUpPool_Editor)
        {
#endif
            LoadingMapPanel.SetProgress(0.01f, "Warm Up Pool");
            yield return GameObjectPoolManager.WarmUpPool();
#if UNITY_EDITOR
        }
#endif

        LoadingMapPanel.SetBackgroundAlpha(1f);
        LoadingMapPanel.SetProgress(0.5f, "StartGame");
        yield return WorldManager.StartGame();

        LoadingMapPanel.SetProgress(1f, "Completed");
        yield return new WaitForSeconds(0.1f);
        BattleManager.Instance.StartBattle();
        LoadingMapPanel.CloseUIForm();
        UIManager.Instance.ShowUIForms<PlayerStatHUDPanel>();
        DebugPanel = UIManager.Instance.ShowUIForms<DebugPanel>();

#if !DEBUG
        UIManager.Instance.CloseUIForm<DebugPanel>();
#endif
        UIManager.Instance.ShowUIForms<InGameUIPanel>();
        yield return WorldManager.OnAfterStartGame();
        IsGameLoading = false;
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
        GameSaveManager.Update(Time.deltaTime);
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
        GameSaveManager.LateUpdate(Time.deltaTime);
        FXManager.LateUpdate(Time.deltaTime);
    }

    void FixedUpdate()
    {
        CurrentFixedFrameCount++;
        CurrentFixedFrameCount_Mod_FixedFrameRate = CurrentFixedFrameCount % FixedFrameRate;
        CurrentFixedFrameCount_Mod_FixedFrameRate_01X = CurrentFixedFrameCount % FixedFrameRate_01X;
        CurrentFixedFrameCount_Mod_FixedFrameRate_5X = CurrentFixedFrameCount % FixedFrameRate_5X;
        ControlManager.FixedUpdate(Time.fixedDeltaTime);
        if (ControlManager.Common_RestartGame.Up && !IsGameLoading)
        {
            if (WorldManager.Instance.CurrentWorld is OpenWorld openWorld)
            {
                if (openWorld.IsInsideMicroWorld)
                {
                    openWorld.RestartMicroWorld(false);
                    return;
                }
            }
        }

#if DEBUG
        if (Input.GetKeyUp(KeyCode.F10) && !IsGameLoading)
        {
            ReloadGame();
            return;
        }

        if (Input.GetKey(KeyCode.Equals))
        {
            Time.timeScale = 0.1f;
        }
        else
        {
            Time.timeScale = 1f;
        }

#endif
        if (Input.GetKey(KeyCode.B))
        {
            if (WorldManager.Instance.CurrentWorld != null && WorldManager.Instance.CurrentWorld is OpenWorld openWorld)
            {
                if (openWorld.IsInsideMicroWorld)
                {
                    openWorld.ReturnToOpenWorldFormMicroWorld(false);
                }
            }
        }

        if (ControlManager.Battle_LeftRotateCamera.Up)
        {
            CameraManager.Instance.FieldCamera.CameraLeftRotate();
        }

        if (ControlManager.Battle_RightRotateCamera.Up)
        {
            CameraManager.Instance.FieldCamera.CameraRightRotate();
        }

        if (ControlManager.Common_ToggleUI.Up)
        {
            UIManager.Instance.UICamera.enabled = !UIManager.Instance.UICamera.enabled;
        }

        if (ControlManager.Common_ToggleDebugButton.Up)
        {
            DebugPanel.DebugToggleButton.gameObject.SetActive(!DebugPanel.DebugToggleButton.gameObject.activeInHierarchy);
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
        GameSaveManager.FixedUpdate(Time.fixedDeltaTime);
        FXManager.FixedUpdate(Time.fixedDeltaTime);
    }

    public void SwitchWorld(string worldName)
    {
        ClientGameManager.DebugChangeWorldName = worldName;
        ClientGameManager.Instance.ReloadGame();
    }

    public void ReloadGame()
    {
        OnReloadScene();
        UIMaskMgr.Instance.OnReloadScene();
        UIManager.Instance.OnReloadScene();
        CameraManager.Instance.OnReloadScene();
        AudioManager.Instance.OnReloadScene();
        ActiveSkillAgent.Instance.OnReloadScene();
        ShutDownGame();
        SceneManager.LoadScene("MainScene");
    }

    private void ShutDownGame()
    {
        GameStateManager.ShutDown(); // 设置游戏状态为ShutDown

        ControlManager.ShutDown();

        ActiveSkillAgent.StopAllCoroutines();

        FXManager.ShutDown();
        GameSaveManager.ShutDown();
        UIBattleTipManager.ShutDown();
        ProjectileManager.ShutDown();
        BattleManager.ShutDown();
        WorldManager.ShutDown();

        DebugConsole.OnDebugConsoleToggleHandler = null;
        DebugConsole.OnDebugConsoleKeyDownHandler = null;
        RoutineManager.ShutDown();
        GameObjectPoolManager.ShutDown();
        PrefabManager.ShutDown();
        LayerManager.ShutDown();
        ConfigManager.ShutDown();
    }
}
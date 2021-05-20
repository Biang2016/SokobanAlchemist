using System.Collections;
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
    public float dropSpeed = 8f;
    public float dropSkillScrollSpeed = 8f;

    public Material BoxMarchingSquareTerrainMat;
    public BoxMarchingTextureConfigMatrix BoxMarchingTextureConfigMatrix;

    #region Managers

    #region Mono

    private WwiseAudioManager WwiseAudioManager => WwiseAudioManager.Instance;
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
    public InGameUIPanel InGameUIPanel;
    public KeyBindingPanel KeyBindingPanel;
    public EntitySkillPreviewPanel EntitySkillPreviewPanel;
    public LoadingMapPanel LoadingMapPanel;
    public PlayerStatHUDPanel PlayerStatHUDPanel;
    public NoticePanel NoticePanel;
    public LearnSkillUpgradePanel LearnSkillUpgradePanel;

    public bool WarmUpPool_Editor = true;

    [HideLabel]
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
            () => ControlManager.Instance.Common_MouseLeft.Up,
            () => ControlManager.Instance.Common_MouseRight.Up,
            () => false,
            () => false,
            () => false
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
        DebugConsole.OnDebugConsoleKeyDownHandler = () => ControlManager.Instance.Common_DebugConsole.Down;

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

        NoticePanel = UIManager.Instance.ShowUIForms<NoticePanel>();
        LoadingMapPanel = UIManager.Instance.WarmUpUIForms<LoadingMapPanel>();
        LearnSkillUpgradePanel = UIManager.Instance.ShowUIForms<LearnSkillUpgradePanel>();
        InGameUIPanel = UIManager.Instance.ShowUIForms<InGameUIPanel>();
        UIManager.Instance.ShowUIForms<StartMenuPanel>();
        WwiseAudioManager.WwiseBGMConfiguration.BGM_Start();
    }

    public void StartGame(string gameSaveName)
    {
        StartCoroutine(Co_StartGame(gameSaveName));
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

    internal bool IsGameLoading = false;

    internal IEnumerator Co_StartGame(string gameSaveName)
    {
        IsGameLoading = true;
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
        PlayerStatHUDPanel = UIManager.Instance.ShowUIForms<PlayerStatHUDPanel>();
        yield return WorldManager.StartGame(gameSaveName);

        LoadingMapPanel.SetProgress(1f, "Completed");
        yield return new WaitForSeconds(0.1f);
        BattleManager.Instance.StartBattle();
        LoadingMapPanel.CloseUIForm();
        DebugPanel = UIManager.Instance.ShowUIForms<DebugPanel>();
        KeyBindingPanel = UIManager.Instance.WarmUpUIForms<KeyBindingPanel>();
        EntitySkillPreviewPanel = UIManager.Instance.WarmUpUIForms<EntitySkillPreviewPanel>();
#if !DEBUG
        UIManager.Instance.CloseUIForm<DebugPanel>();
#endif
        yield return WorldManager.OnAfterStartGame();
        IsGameLoading = false;
    }

    private void Update()
    {
        ControlManager.Update(Time.deltaTime);
        if (ControlManager.Menu_ExitMenuPanel.Up)
        {
            if (!UIManager.IsUIShown<StartMenuPanel>()
                && !UIManager.IsUIShown<ConfirmPanel>()
                && !UIManager.IsUIShown<TransportWorldPanel>()
                && !UIManager.IsUIShown<EntitySkillPreviewPanel>()
                && !UIManager.IsUIShown<KeyBindingPanel>()
                && !UIManager.IsUIShown<LoadingMapPanel>()
                && !LearnSkillUpgradePanel.HasPage)
            {
                if (BattleManager.Instance.IsStart)
                {
                    UIManager.Instance.ToggleUIForm<ExitMenuPanel>();
                }
            }
        }

        if (BattleManager.Instance.IsStart)
        {
            if (ControlManager.Battle_RestartGame.Up && !IsGameLoading)
            {
                if (RestartDungeon()) return;
            }

            if (ControlManager.Common_ReloadGame.Up && !IsGameLoading)
            {
                if (!UIManager.IsUIShown<ConfirmPanel>())
                {
                    ConfirmPanel confirmPanel = UIManager.Instance.ShowUIForms<ConfirmPanel>();
                    confirmPanel.Initialize("Wanna reload the game? You'll lose all the progress", "Reload", "Cancel",
                        () =>
                        {
                            StartCoroutine(Co_ReloadGame());
                            confirmPanel.CloseUIForm();
                        },
                        () => { confirmPanel.CloseUIForm(); }
                    );
                    return;
                }
            }

            if (ControlManager.Battle_ReturnToOpenWorld.Up && !IsGameLoading)
            {
                if (ReturnToOpenWorld()) return;
            }

            if (!LearnSkillUpgradePanel.HasPage)
            {
                if (ControlManager.Battle_LeftSwitch.Up)
                {
                    CameraManager.Instance.FieldCamera.CameraLeftRotate();
                }

                if (ControlManager.Battle_RightSwitch.Up)
                {
                    CameraManager.Instance.FieldCamera.CameraRightRotate();
                }
            }

            if (ControlManager.Menu_KeyBindPanel.Down)
            {
                if (!UIManager.IsUIShown<ExitMenuPanel>())
                {
                    UIManager.Instance.ShowUIForms<KeyBindingPanel>();
                }
            }

            if (ControlManager.Menu_KeyBindPanel.Up)
            {
                KeyBindingPanel.CloseUIForm();
            }

            if (ControlManager.Common_ToggleDebugPanel.Up)
            {
                UIManager.Instance.ToggleUIForm<DebugPanel>();
            }

            if (ControlManager.Menu_SkillPreviewPanel.Up)
            {
                if (!UIManager.IsUIShown<ExitMenuPanel>())
                {
                    UIManager.Instance.ToggleUIForm<EntitySkillPreviewPanel>();
                    if (EntitySkillPreviewPanel.IsShown)
                    {
                        if (BattleManager.Instance.Player1 != null)
                        {
                            EntitySkillPreviewPanel.Initialize(BattleManager.Instance.Player1);
                        }
                    }
                }
            }

            if (ControlManager.Common_ToggleUI.Up)
            {
                UIManager.Instance.UICamera.enabled = !UIManager.Instance.UICamera.enabled;
            }

            if (DebugPanel != null && DebugPanel.IsShown && !UIManager.IsUIShown<ExitMenuPanel>())
            {
                if (ControlManager.Battle_SlowDownGame.Pressed)
                {
                    Time.timeScale = 0.1f;
                }
                else
                {
                    Time.timeScale = 1f;
                }
            }
        }

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

        if (Input.GetKeyUp(KeyCode.P))
        {
            BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, "OnBossSpiderLegAppear");
        }
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

    public bool ReturnToOpenWorld()
    {
        if (!UIManager.IsUIShown<ConfirmPanel>())
        {
            bool isExitMenuPanelShown = UIManager.Instance.IsUIShown<ExitMenuPanel>();
            if (isExitMenuPanelShown) UIManager.Instance.CloseUIForm<ExitMenuPanel>();
            if (WorldManager.Instance.CurrentWorld != null && WorldManager.Instance.CurrentWorld is OpenWorld openWorld)
            {
                if (openWorld.InsideDungeon)
                {
                    ConfirmPanel confirmPanel = UIManager.Instance.ShowUIForms<ConfirmPanel>();
                    confirmPanel.Initialize("Wanna give up the dungeon? You'll lose the rewards that you've got", "Let me go", "Cancel",
                        () =>
                        {
                            openWorld.ReturnToOpenWorld(false);
                            confirmPanel.CloseUIForm();
                        },
                        () =>
                        {
                            confirmPanel.CloseUIForm();
                            if (isExitMenuPanelShown) UIManager.Instance.ShowUIForms<ExitMenuPanel>();
                        }
                    );
                    return true;
                }
            }
            else
            {
                SwitchWorld_ReloadGame(ConfigManager.GetTypeName(TypeDefineType.World, ConfigManager.World_OpenWorldIndex));
                return true;
            }
        }

        return false;
    }

    public bool RestartDungeon()
    {
        if (!UIManager.IsUIShown<ConfirmPanel>())
        {
            bool isExitMenuPanelShown = UIManager.Instance.IsUIShown<ExitMenuPanel>();
            if (isExitMenuPanelShown) UIManager.Instance.CloseUIForm<ExitMenuPanel>();
            if (WorldManager.Instance.CurrentWorld is OpenWorld openWorld)
            {
                if (openWorld.InsideDungeon)
                {
                    ConfirmPanel confirmPanel = UIManager.Instance.ShowUIForms<ConfirmPanel>();
                    confirmPanel.Initialize("Wanna restart the dungeon? You'll lose the rewards that you've got", "Restart", "Cancel",
                        () =>
                        {
                            openWorld.RestartDungeon();
                            confirmPanel.CloseUIForm();
                        },
                        () =>
                        {
                            confirmPanel.CloseUIForm();
                            if (isExitMenuPanelShown) UIManager.Instance.ShowUIForms<ExitMenuPanel>();
                        }
                    );
                    return true;
                }
            }
            else
            {
                StartCoroutine(Co_ReloadGame());
                return true;
            }
        }

        return false;
    }

    public void ExitToMainMenu()
    {
        if (!UIManager.Instance.IsUIShown<ConfirmPanel>())
        {
            UIManager.Instance.CloseUIForm<ExitMenuPanel>();
            ConfirmPanel confirmPanel = UIManager.Instance.ShowUIForms<ConfirmPanel>();
            confirmPanel.Initialize("If you back to menu, you'll lose all the progress", "Go to menu", "Cancel",
                () =>
                {
                    confirmPanel.CloseUIForm();
                    UIManager.Instance.CloseUIForm<PlayerStatHUDPanel>();
                    ReloadGame();
                },
                () =>
                {
                    confirmPanel.CloseUIForm();
                    UIManager.Instance.ShowUIForms<ExitMenuPanel>();
                }
            );
            return;
        }
    }

    public void ReloadGame()
    {
        StartCoroutine(Co_ReloadGame());
    }

    public void SwitchWorld_ReloadGame(string worldName)
    {
        DebugChangeWorldName = worldName;
        StartCoroutine(Co_ReloadGame());
    }

    public void ChangeWorld(string worldName, bool dungeonComplete, EntityData transportBoxEntityData = null)
    {
        if (WorldManager.Instance.CurrentWorld is OpenWorld openWorld)
        {
            ushort worldNameIndex = ConfigManager.GetTypeIndex(TypeDefineType.World, worldName);
            if (worldNameIndex == ConfigManager.World_OpenWorldIndex)
            {
                openWorld.ReturnToOpenWorld(dungeonComplete);
            }
            else
            {
                openWorld.TransportPlayerToDungeon(worldNameIndex, transportBoxEntityData);
            }
        }
        else
        {
            SwitchWorld_ReloadGame(worldName);
        }
    }

    public IEnumerator Co_ReloadGame()
    {
        yield return ShutDownGame();
        OnReloadScene();
        UIMaskMgr.OnReloadScene();
        UIManager.OnReloadScene();
        WwiseAudioManager.OnReloadScene();
        CameraManager.OnReloadScene();
        ActiveSkillAgent.OnReloadScene();
        SceneManager.LoadScene("MainScene");
    }

    private IEnumerator ShutDownGame()
    {
        BattleManager.IsStart = false;
        WwiseAudioManager.ShutDown();

        GameStateManager.ShutDown(); // 设置游戏状态为ShutDown

        ControlManager.ShutDown();

        ActiveSkillAgent.StopAllCoroutines();

        FXManager.ShutDown();
        GameSaveManager.ShutDown();
        UIBattleTipManager.ShutDown();
        ProjectileManager.ShutDown();
        WorldManager.ShutDown();
        yield return WorldManager.Co_ShutDown();
        BattleManager.ShutDown();

        DebugConsole.OnDebugConsoleToggleHandler = null;
        DebugConsole.OnDebugConsoleKeyDownHandler = null;
        RoutineManager.ShutDown();
        GameObjectPoolManager.ShutDown();
        PrefabManager.ShutDown();
        LayerManager.ShutDown();
        ConfigManager.ShutDown();
    }
}
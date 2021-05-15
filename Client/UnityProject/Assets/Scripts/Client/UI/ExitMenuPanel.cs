using BiangLibrary.GamePlay.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ExitMenuPanel : BaseUIPanel
{
    void Awake()
    {
        UIType.InitUIType(
            false,
            false,
            false,
            UIFormTypes.Normal,
            UIFormShowModes.Normal,
            UIFormLucencyTypes.ImPenetrable);
    }

    void Start()
    {
        ControlManager.Instance.OnControlSchemeChanged += (before, after) =>
        {
            if (after == ControlManager.ControlScheme.GamePad)
            {
                if (IsShown)
                {
                    InitButtons();
                }
            }
        };
    }

    public Animator ExitMenuAnim;

    public Button ExitToOpenWorldButton;
    public Button RestartDungeonButton;
    public Button SaveGameButton;
    public Button ExitToMenuButton;
    public Button ExitToDesktopButton;

    public AK.Wwise.Event OnDisplay;
    public AK.Wwise.Event OnHide;

    private bool PlayButtonHoverSound = false;

    public override void Display()
    {
        base.Display();
        OnDisplay?.Post(gameObject);
        ExitMenuAnim.SetTrigger("Play");

        ControlManager.Instance.BattleActionEnabled = false;

        PlayButtonHoverSound = false;
        InitButtons();
        PlayButtonHoverSound = true;

        Time.timeScale = 0;
    }

    private void InitButtons()
    {
        bool insideDungeon = WorldManager.Instance.CurrentWorld != null && ((WorldManager.Instance.CurrentWorld is OpenWorld {InsideDungeon: true}) || !(WorldManager.Instance.CurrentWorld is OpenWorld));
        ExitToOpenWorldButton.gameObject.SetActive(insideDungeon);
        RestartDungeonButton.gameObject.SetActive(insideDungeon);
        if (insideDungeon)
        {
            ExitToOpenWorldButton.Select();
        }
        else
        {
            SaveGameButton.Select();
        }
    }

    protected override void ChildUpdate()
    {
        base.ChildUpdate();
        if (ControlManager.Instance.Menu_Cancel.Up)
        {
            CloseUIForm();
        }
    }

    public override void Hide()
    {
        OnHide?.Post(gameObject);
        ControlManager.Instance.BattleActionEnabled = true;
        EventSystem.current.SetSelectedGameObject(null);
        Time.timeScale = 1;
        base.Hide();
    }

    public void OnButtonClick()
    {
        WwiseAudioManager.Instance.PlayCommonAudioSound(WwiseAudioManager.CommonAudioEvent.UI_ButtonClick, WwiseAudioManager.Instance.gameObject);
    }

    public void OnButtonHover()
    {
        if (PlayButtonHoverSound)
        {
            WwiseAudioManager.Instance.PlayCommonAudioSound(WwiseAudioManager.CommonAudioEvent.UI_ButtonHover, WwiseAudioManager.Instance.gameObject);
        }
    }

    public void OnExitToOpenWorldButtonClick()
    {
        ClientGameManager.Instance.ReturnToOpenWorld();
    }

    public void OnRestartDungeonButtonClick()
    {
        ClientGameManager.Instance.RestartDungeon();
    }

    public void OnSaveGameButtonClick()
    {
        if (WorldManager.Instance.CurrentWorld is OpenWorld openWorld)
        {
            openWorld.SaveGame("Slot1");
            CloseUIForm();
        }
    }

    public void OnExitToMenuButtonClick()
    {
        ClientGameManager.Instance.ExitToMainMenu();
    }

    public void OnExitToDesktopButtonClick()
    {
        Application.Quit();
    }
}
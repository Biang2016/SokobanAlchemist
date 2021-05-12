using BiangLibrary.GamePlay.UI;
using UnityEngine;
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

    public Animator ExitMenuAnim;

    public Button ExitToOpenWorldButton;
    public Button RestartDungeonButton;
    public Button ExitToMenuButton;
    public Button ExitToDesktopButton;

    public AK.Wwise.Event OnDisplay;
    public AK.Wwise.Event OnHide;

    public override void Display()
    {
        base.Display();
        OnDisplay?.Post(gameObject);
        ExitMenuAnim.SetTrigger("Play");
        if (WorldManager.Instance.CurrentWorld != null && WorldManager.Instance.CurrentWorld is OpenWorld openWorld)
        {
            ExitToOpenWorldButton.gameObject.SetActive(openWorld.InsideDungeon);
            RestartDungeonButton.gameObject.SetActive(openWorld.InsideDungeon);
        }
    }

    public override void Hide()
    {
        OnHide?.Post(gameObject);
        base.Hide();
    }

    public void OnButtonHover()
    {
        WwiseAudioManager.Instance.PlayCommonAudioSound(WwiseAudioManager.CommonAudioEvent.UI_ButtonHover, WwiseAudioManager.Instance.gameObject);
    }

    public void OnExitToOpenWorldButtonClick()
    {
        WwiseAudioManager.Instance.PlayCommonAudioSound(WwiseAudioManager.CommonAudioEvent.UI_ButtonClick, WwiseAudioManager.Instance.gameObject);
        ClientGameManager.Instance.ReturnToOpenWorld();
    }

    public void OnRestartDungeonButtonClick()
    {
        WwiseAudioManager.Instance.PlayCommonAudioSound(WwiseAudioManager.CommonAudioEvent.UI_ButtonClick, WwiseAudioManager.Instance.gameObject);
        ClientGameManager.Instance.RestartDungeon();
    }

    public void OnExitToMenuButtonClick()
    {
        WwiseAudioManager.Instance.PlayCommonAudioSound(WwiseAudioManager.CommonAudioEvent.UI_ButtonClick, WwiseAudioManager.Instance.gameObject);
        if (!UIManager.Instance.IsUIShown<ConfirmPanel>())
        {
            ConfirmPanel confirmPanel = UIManager.Instance.ShowUIForms<ConfirmPanel>();
            confirmPanel.Initialize("If you back to menu, you'll lose all the progress", "Go to menu", "Cancel",
                () =>
                {
                    confirmPanel.CloseUIForm();
                    UIManager.Instance.CloseUIForm<PlayerStatHUDPanel>();
                    UIManager.Instance.CloseUIForm<ExitMenuPanel>();
                    ClientGameManager.Instance.ReloadGame();
                },
                () => { confirmPanel.CloseUIForm(); }
            );
            return;
        }
    }

    public void OnExitToDesktopButtonClick()
    {
        WwiseAudioManager.Instance.PlayCommonAudioSound(WwiseAudioManager.CommonAudioEvent.UI_ButtonClick, WwiseAudioManager.Instance.gameObject);
        Application.Quit();
    }
}
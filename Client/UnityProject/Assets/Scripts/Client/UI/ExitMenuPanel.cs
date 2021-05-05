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

    public Button ExitToMenuButton;
    public Button ExitToDesktopButton;

    public override void Display()
    {
        base.Display();
        ExitMenuAnim.SetTrigger("Play");
    }

    public void OnExitToMenuButtonClick()
    {
        if (!UIManager.Instance.IsUIShown<ConfirmPanel>())
        {
            ConfirmPanel confirmPanel = UIManager.Instance.ShowUIForms<ConfirmPanel>();
            confirmPanel.Initialize("If you back to menu, you'll lose all the progress", "Go to menu", "Cancel",
                () =>
                {
                    StartCoroutine(ClientGameManager.Instance.ReloadGame());
                    confirmPanel.CloseUIForm();
                },
                () => { confirmPanel.CloseUIForm(); }
            );
            return;
        }
    }

    public void OnExitToDesktopButtonClick()
    {
        Application.Quit();
    }
}
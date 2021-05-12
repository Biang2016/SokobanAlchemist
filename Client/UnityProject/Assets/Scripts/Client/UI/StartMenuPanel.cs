using System.Collections;
using System.IO;
using BiangLibrary.GamePlay.UI;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuPanel : BaseUIPanel
{
    void Awake()
    {
        UIType.InitUIType(
            false,
            false,
            false,
            UIFormTypes.Normal,
            UIFormShowModes.Normal,
            UIFormLucencyTypes.Penetrable);
    }

    public Button FirstSelectedButton;
    public Button CreditSelfButton;

    public Button CreditButton;
    public Button StartButton;
    public Button ExitButton;

    public Animator StartMenuAnim;
    public Animator CreditAnim;

    public AK.Wwise.Event OnPlayAnim;

    public override void Display()
    {
        base.Display();
        WwiseAudioManager.Instance.WwiseBGMConfiguration.SwitchBGMTheme(BGM_Theme.StartMenu);
        StartMenuAnim.SetTrigger("Play");
        OnPlayAnim?.Post(gameObject);

        FirstSelectedButton.Select();
    }

    public void OnButtonClick()
    {
        WwiseAudioManager.Instance.PlayCommonAudioSound(WwiseAudioManager.CommonAudioEvent.UI_ButtonClick, WwiseAudioManager.Instance.gameObject);
    }

    public void OnButtonHover()
    {
        WwiseAudioManager.Instance.PlayCommonAudioSound(WwiseAudioManager.CommonAudioEvent.UI_ButtonHover, WwiseAudioManager.Instance.gameObject);
    }

    public void OnSettingButtonClick()
    {
    }

    public void OnStartButtonClick()
    {
        string folder = $"{Application.streamingAssetsPath}/GameSaves";
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        string file = $"{folder}/GameSave_Slot1.save";
        if (File.Exists(file))
        {
            ConfirmPanel cp = UIManager.Instance.ShowUIForms<ConfirmPanel>();
            cp.Initialize("Continue old game (Y) or Start a new game (N)?", "Y", "N",
                () =>
                {
                    cp.CloseUIForm();
                    ClientGameManager.Instance.StartGame("Slot1");
                },
                () =>
                {
                    cp.CloseUIForm();
                    ClientGameManager.Instance.StartGame("");
                }
            );
        }
        else
        {
            ClientGameManager.Instance.StartGame("");
        }

        CloseUIForm();
    }

    public void OnLoadButtonClick()
    {
        ClientGameManager.Instance.StartGame("Slot1");
        CloseUIForm();
    }

    public void OnCreditButtonClick()
    {
        CreditAnim.SetTrigger("Show");
        CreditSelfButton.Select();
    }

    public void OnCreditSelfButtonClick()
    {
        CreditAnim.SetTrigger("Hide");
        CreditButton.Select();
    }

    public void OnExitButtonClick()
    {
        Application.Quit();
    }

    public void DebugLog(string log)
    {
        Debug.Log(log);
    }
}
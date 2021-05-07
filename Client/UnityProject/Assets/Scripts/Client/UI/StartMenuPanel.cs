using System.Collections;
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

    public Animator StartMenuAnim;
    public Animator CreditAnim;

    public AK.Wwise.Event OnPlayAnim;

    public override void Display()
    {
        base.Display();
        WwiseAudioManager.Instance.WwiseBGMConfiguration.SwitchBGMTheme(BGM_Theme.StartMenu);
        StartMenuAnim.SetTrigger("Play");
        OnPlayAnim?.Post(gameObject);
    }

    public void OnSettingButtonClick()
    {
        WwiseAudioManager.Instance.Common_UI_ButtonClick?.Post(WwiseAudioManager.Instance.gameObject);
    }

    public void OnStartButtonClick()
    {
        WwiseAudioManager.Instance.Common_UI_ButtonClick?.Post(WwiseAudioManager.Instance.gameObject);
        ClientGameManager.Instance.StartGame();
        CloseUIForm();
    }

    public void OnCreditButtonClick()
    {
        WwiseAudioManager.Instance.Common_UI_ButtonClick?.Post(WwiseAudioManager.Instance.gameObject);
        CreditAnim.SetTrigger("Show");
    }

    public void OnCreditSelfButtonClick()
    {
        CreditAnim.SetTrigger("Hide");
    }

    public void OnExitButtonClick()
    {
        WwiseAudioManager.Instance.Common_UI_ButtonClick?.Post(WwiseAudioManager.Instance.gameObject);
        Application.Quit();
    }

    public void DebugLog(string log)
    {
        Debug.Log(log);
    }
}
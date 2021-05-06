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

    public Button SettingButton;
    public Button StartButton;
    public Button CreditButton;
    public Button ExitButton;

    public AK.Wwise.Event OnPlayAnim;

    public override void Display()
    {
        base.Display();
        StartMenuAnim.SetTrigger("Play");
        OnPlayAnim?.Post(gameObject);
    }

    public void OnSettingButtonClick()
    {
    }

    public void OnStartButtonClick()
    {
        ClientGameManager.Instance.StartGame();
        CloseUIForm();
    }

    public void OnCreditButtonClick()
    {
        CreditAnim.SetTrigger("Show");
    }

    public void OnCreditSelfButtonClick()
    {
        CreditAnim.SetTrigger("Hide");
    }

    public void OnExitButtonClick()
    {
        Application.Quit();
    }
}
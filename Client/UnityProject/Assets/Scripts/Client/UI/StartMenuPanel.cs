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

    protected override void ChildUpdate()
    {
        base.ChildUpdate();
        if (ControlManager.Instance.Menu_Cancel.Up)
        {
            if (IsCreditShown) OnCreditSelfButtonClick();
            else if (IsKeyBindShown) OnKeyBindSelfButtonClick();
        }
    }

    public Button FirstSelectedButton;

    public Button CreditButton;
    public Button CreditSelfButton;
    public Button StartButton;
    public Button ExitButton;
    public Button SettingButton;
    public Button KeyBindButton;
    public Button KeyBindSelfButton;

    public Animator StartMenuAnim;
    public Animator CreditAnim;
    public Animator KeyBindAnim;

    public AK.Wwise.Event OnPlayAnim;

    private bool PlayButtonHoverSound = false;

    public Image KeyBindImage;
    public Sprite Sprite_Keyboard;
    public Sprite Sprite_Controller;

    public override void Display()
    {
        base.Display();
        StartCoroutine(Co_StartMenuShow());
        PlayButtonHoverSound = false;
        InitButtons();
        PlayButtonHoverSound = true;
        ControlManager.Instance.BattleActionEnabled = false;
    }

    private IEnumerator Co_StartMenuShow()
    {
        yield return new WaitForSeconds(1f);
        StartMenuAnim.SetTrigger("Play");
    }

    public override void Hide()
    {
        base.Hide();
        ControlManager.Instance.BattleActionEnabled = true;
    }

    public void SetAllInteractable(bool interactable)
    {
        CreditSelfButton.interactable = interactable;
        StartButton.interactable = interactable;
        ExitButton.interactable = interactable;
        SettingButton.interactable = interactable;
        KeyBindButton.interactable = interactable;
    }

    public void PlayAnimSound()
    {
        OnPlayAnim?.Post(gameObject);
    }

    public void InitButtons()
    {
        FirstSelectedButton.Select();
    }

    public void InitButtons_Setting()
    {
        SettingButton.Select();
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

    public void OnStartButtonClick()
    {
        if (!Directory.Exists(ConfigManager.GameSavePath)) Directory.CreateDirectory(ConfigManager.GameSavePath);
        string file = $"{ConfigManager.GameSavePath}/GameSave_Slot1.save";
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

    private bool IsCreditShown = false;

    public void OnCreditButtonClick()
    {
        CreditAnim.SetTrigger("Show");
        CreditSelfButton.Select();
        IsCreditShown = true;
    }

    public void OnCreditSelfButtonClick()
    {
        CreditAnim.SetTrigger("Hide");
        CreditButton.Select();
        IsCreditShown = false;
    }

    public void OnExitButtonClick()
    {
        Application.Quit();
    }

    public void OnSettingButtonClick()
    {
        SetAllInteractable(false);
        UIManager.Instance.ShowUIForms<SettingPanel>();
    }

    private bool IsKeyBindShown = false;

    public void OnKeyBindButtonClick()
    {
        KeyBindAnim.SetTrigger("Show");
        KeyBindSelfButton.Select();
        IsKeyBindShown = true;
        if (ControlManager.Instance.CurrentControlScheme == ControlManager.ControlScheme.KeyboardMouse)
        {
            KeyBindImage.sprite = Sprite_Keyboard;
        }
        else if (ControlManager.Instance.CurrentControlScheme == ControlManager.ControlScheme.GamePad)
        {
            KeyBindImage.sprite = Sprite_Controller;
        }
    }

    public void OnKeyBindSelfButtonClick()
    {
        KeyBindAnim.SetTrigger("Hide");
        KeyBindButton.Select();
        IsKeyBindShown = false;
    }
}
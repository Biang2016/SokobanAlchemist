using System.Collections;
using BiangLibrary.GamePlay.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingPanel : BaseUIPanel
{
    private int CurrentResolutionIndex = 0;
    private int CurrentDisplayModeIndex = 0;

    public AK.Wwise.Event OnDisplay;
    public AK.Wwise.Event OnHide;
    public Animator SettingPanelAnim;

    private bool PlayButtonHoverSound = false;

    public Button LeftResolutionButton;
    public Button RightResolutionButton;
    public Button ConfirmButton;
    public TextMeshProUGUI ResolutionText;

    public Button LeftFullScreenModeButton;
    public Button RightFullScreenModeButton;
    public TextMeshProUGUI FullScreenModeText;

    public Slider SoundVolumeSlider;
    public Slider MusicVolumeSlider;

    public int CurrentSelectedRowIndex = 0;
    public UITextColorSwap[] UITextColorSwaps;

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
        CurrentResolutionIndex = 0;
        if (PlayerPrefs.HasKey(PlayerPrefKey.RESOLUTION))
        {
            CurrentResolutionIndex = PlayerPrefs.GetInt(PlayerPrefKey.RESOLUTION);
        }

        CurrentDisplayModeIndex = 0;
        if (PlayerPrefs.HasKey(PlayerPrefKey.DISPLAY_MODE))
        {
            CurrentDisplayModeIndex = PlayerPrefs.GetInt(PlayerPrefKey.DISPLAY_MODE);
        }

        OnRefreshText();

        if (PlayerPrefs.HasKey(PlayerPrefKey.SOUND_VOLUME))
        {
            SoundVolumeSlider.value = PlayerPrefs.GetFloat(PlayerPrefKey.SOUND_VOLUME);
        }
        else
        {
            SoundVolumeSlider.value = 100f;
        }

        if (PlayerPrefs.HasKey(PlayerPrefKey.MUSIC_VOLUME))
        {
            MusicVolumeSlider.value = PlayerPrefs.GetFloat(PlayerPrefKey.MUSIC_VOLUME);
        }
        else
        {
            MusicVolumeSlider.value = 100f;
        }

        ControlManager.Instance.OnControlSchemeChanged += (before, after) =>
        {
            if (after == ControlManager.ControlScheme.KeyboardMouse)
            {
                foreach (UITextColorSwap uiTextColorSwap in UITextColorSwaps)
                {
                    uiTextColorSwap.OnMouseExitButton();
                }
            }
            else if (after == ControlManager.ControlScheme.GamePad)
            {
                CurrentSelectedRowIndex = 0;
                RefreshSelectedRows();
            }
        };
    }

    public override void Display()
    {
        base.Display();
        OnDisplay?.Post(gameObject);
        SettingPanelAnim.SetTrigger("Play");
        if (ControlManager.Instance.CurrentControlScheme == ControlManager.ControlScheme.GamePad)
        {
            for (int index = 0; index < UITextColorSwaps.Length; index++)
            {
                UITextColorSwap uiTextColorSwap = UITextColorSwaps[index];
                if (index == CurrentSelectedRowIndex) uiTextColorSwap.OnMouseEnterButton();
                else uiTextColorSwap.OnMouseExitButton();
            }
        }
        else if (ControlManager.Instance.CurrentControlScheme == ControlManager.ControlScheme.KeyboardMouse)
        {
            foreach (UITextColorSwap uiTextColorSwap in UITextColorSwaps)
            {
                uiTextColorSwap.OnMouseExitButton();
            }
        }
    }

    protected override void ChildUpdate()
    {
        base.ChildUpdate();
        if (ControlManager.Instance.Menu_UpPrevious.Up) SelectPreviousRow();
        if (ControlManager.Instance.Menu_DownNext.Up) SelectNextRow();
        if (ControlManager.Instance.CurrentControlScheme == ControlManager.ControlScheme.GamePad)
        {
            if (ControlManager.Instance.Menu_LeftSwitch.Up)
            {
                switch (CurrentSelectedRowIndex)
                {
                    case 0: // Resolution
                    {
                        OnLeftResolutionButtonClick();
                        WwiseAudioManager.Instance.PlayCommonAudioSound(WwiseAudioManager.CommonAudioEvent.UI_ButtonClick, WwiseAudioManager.Instance.gameObject);
                        break;
                    }
                    case 1: // DisplayMode
                    {
                        OnLeftDisplayModeButtonClick();
                        WwiseAudioManager.Instance.PlayCommonAudioSound(WwiseAudioManager.CommonAudioEvent.UI_ButtonClick, WwiseAudioManager.Instance.gameObject);
                        break;
                    }
                    case 2: // SoundVolume
                    {
                        SoundVolumeSlider.value -= 10;
                        WwiseAudioManager.Instance.PlayCommonAudioSound(WwiseAudioManager.CommonAudioEvent.UI_ButtonClick, WwiseAudioManager.Instance.gameObject);
                        break;
                    }
                    case 3: // MusicVolume
                    {
                        MusicVolumeSlider.value -= 10;
                        WwiseAudioManager.Instance.PlayCommonAudioSound(WwiseAudioManager.CommonAudioEvent.UI_ButtonClick, WwiseAudioManager.Instance.gameObject);
                        break;
                    }
                }
            }

            if (ControlManager.Instance.Menu_RightSwitch.Up)
            {
                switch (CurrentSelectedRowIndex)
                {
                    case 0: // Resolution
                    {
                        OnRightResolutionButtonClick();
                        WwiseAudioManager.Instance.PlayCommonAudioSound(WwiseAudioManager.CommonAudioEvent.UI_ButtonClick, WwiseAudioManager.Instance.gameObject);
                        break;
                    }
                    case 1: // DisplayMode
                    {
                        OnRightDisplayModeButtonClick();
                        WwiseAudioManager.Instance.PlayCommonAudioSound(WwiseAudioManager.CommonAudioEvent.UI_ButtonClick, WwiseAudioManager.Instance.gameObject);
                        break;
                    }
                    case 2: // SoundVolume
                    {
                        SoundVolumeSlider.value += 10;
                        WwiseAudioManager.Instance.PlayCommonAudioSound(WwiseAudioManager.CommonAudioEvent.UI_ButtonClick, WwiseAudioManager.Instance.gameObject);
                        break;
                    }
                    case 3: // MusicVolume
                    {
                        MusicVolumeSlider.value += 10;
                        WwiseAudioManager.Instance.PlayCommonAudioSound(WwiseAudioManager.CommonAudioEvent.UI_ButtonClick, WwiseAudioManager.Instance.gameObject);
                        break;
                    }
                }
            }
        }
    }

    public override void Hide()
    {
        OnHide?.Post(gameObject);
        EventSystem.current.SetSelectedGameObject(null);
        if (UIManager.Instance.IsUIShown<StartMenuPanel>())
        {
            UIManager.Instance.GetBaseUIForm<StartMenuPanel>().SetAllInteractable(true);
            if (ControlManager.Instance.CurrentControlScheme == ControlManager.ControlScheme.GamePad)
            {
                UIManager.Instance.GetBaseUIForm<StartMenuPanel>().InitButtons_Setting();
            }
        }

        if (UIManager.Instance.IsUIShown<ExitMenuPanel>())
        {
            UIManager.Instance.GetBaseUIForm<ExitMenuPanel>().SetAllInteractable(true);
            if (ControlManager.Instance.CurrentControlScheme == ControlManager.ControlScheme.GamePad)
            {
                UIManager.Instance.GetBaseUIForm<ExitMenuPanel>().InitButtons_Setting();
            }
        }

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

    public void OnLeftResolutionButtonClick()
    {
        CurrentResolutionIndex--;
        if (CurrentResolutionIndex < 0) CurrentResolutionIndex = SettingManager.Instance.Resolutions.Length - 1;
        OnRefreshText();
    }

    public void OnRightResolutionButtonClick()
    {
        CurrentResolutionIndex++;
        if (CurrentResolutionIndex >= SettingManager.Instance.Resolutions.Length) CurrentResolutionIndex = 0;
        OnRefreshText();
    }

    public void OnLeftDisplayModeButtonClick()
    {
        CurrentDisplayModeIndex--;
        if (CurrentDisplayModeIndex < 0) CurrentDisplayModeIndex = SettingManager.Instance.FullScreenModes.Length - 1;
        OnRefreshText();
    }

    public void OnRightDisplayModeButtonClick()
    {
        CurrentDisplayModeIndex++;
        if (CurrentDisplayModeIndex >= SettingManager.Instance.FullScreenModes.Length) CurrentDisplayModeIndex = 0;
        OnRefreshText();
    }

    private void OnRefreshText()
    {
        Resolution resolution = SettingManager.Instance.Resolutions[CurrentResolutionIndex];
        FullScreenMode fullScreenMode = SettingManager.Instance.FullScreenModes[CurrentDisplayModeIndex];
        ResolutionText.text = $"{resolution.width} x {resolution.height} ({resolution.refreshRate}Hz)";
        FullScreenModeText.text = $"{fullScreenMode}";
    }

    public void OnConfirmButtonClick()
    {
        SettingManager.Instance.SetResolutionAndDisplayMode(CurrentResolutionIndex, CurrentDisplayModeIndex);
        CloseUIForm();
    }

    public void OnSoundVolumeSliderValueChanged()
    {
        SettingManager.Instance.SetSoundVolume(SoundVolumeSlider.value);
    }

    public void OnMusicVolumeSliderValueChanged()
    {
        SettingManager.Instance.SetMusicVolume(MusicVolumeSlider.value);
    }

    #region Selected Row

    public void SelectPreviousRow()
    {
        CurrentSelectedRowIndex--;
        if (CurrentSelectedRowIndex < 0) CurrentSelectedRowIndex = UITextColorSwaps.Length;
        WwiseAudioManager.Instance.PlayCommonAudioSound(WwiseAudioManager.CommonAudioEvent.UI_ButtonClick, WwiseAudioManager.Instance.gameObject);
        RefreshSelectedRows();
    }

    public void SelectNextRow()
    {
        CurrentSelectedRowIndex++;
        if (CurrentSelectedRowIndex > UITextColorSwaps.Length) CurrentSelectedRowIndex = 0;
        WwiseAudioManager.Instance.PlayCommonAudioSound(WwiseAudioManager.CommonAudioEvent.UI_ButtonClick, WwiseAudioManager.Instance.gameObject);
        RefreshSelectedRows();
    }

    private void RefreshSelectedRows()
    {
        for (int index = 0; index < UITextColorSwaps.Length; index++)
        {
            UITextColorSwap uiTextColorSwap = UITextColorSwaps[index];
            if (index == CurrentSelectedRowIndex) uiTextColorSwap.OnMouseEnterButton();
            else uiTextColorSwap.OnMouseExitButton();
        }

        if (CurrentSelectedRowIndex == UITextColorSwaps.Length)
        {
            ConfirmButton.Select();
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    #endregion
}
using System.Collections;
using System.Collections.Generic;
using BiangLibrary.Singleton;
using UnityEngine;

public class SettingManager : MonoSingleton<SettingManager>
{
    internal Resolution[] Resolutions => Screen.resolutions;
    internal int CurrentResolutionIndex = 0;

    internal FullScreenMode[] FullScreenModes = new[] {FullScreenMode.ExclusiveFullScreen, FullScreenMode.FullScreenWindow, FullScreenMode.Windowed};
    internal int CurrentDisplayModeIndex = 0;

    void Awake()
    {
        CurrentResolutionIndex = Resolutions.Length - 1;
        if (PlayerPrefs.HasKey(PlayerPrefKey.RESOLUTION))
        {
            CurrentResolutionIndex = PlayerPrefs.GetInt(PlayerPrefKey.RESOLUTION);
        }

        CurrentDisplayModeIndex = 1;
        if (PlayerPrefs.HasKey(PlayerPrefKey.DISPLAY_MODE))
        {
            CurrentDisplayModeIndex = PlayerPrefs.GetInt(PlayerPrefKey.DISPLAY_MODE);
        }

        SetResolutionAndDisplayMode(CurrentResolutionIndex, CurrentDisplayModeIndex);

        if (PlayerPrefs.HasKey(PlayerPrefKey.MASTER_VOLUME))
        {
            WwiseAudioManager.Instance.Master_Volume.SetGlobalValue(PlayerPrefs.GetFloat(PlayerPrefKey.MASTER_VOLUME));
        }
        else
        {
            WwiseAudioManager.Instance.Master_Volume.SetGlobalValue(100f);
        }

        if (PlayerPrefs.HasKey(PlayerPrefKey.MUSIC_VOLUME))
        {
            WwiseAudioManager.Instance.Music_Volume.SetGlobalValue(PlayerPrefs.GetFloat(PlayerPrefKey.MUSIC_VOLUME));
        }
        else
        {
            WwiseAudioManager.Instance.Music_Volume.SetGlobalValue(100f);
        }

        if (PlayerPrefs.HasKey(PlayerPrefKey.SOUND_VOLUME))
        {
            WwiseAudioManager.Instance.UI_Volume.SetGlobalValue(PlayerPrefs.GetFloat(PlayerPrefKey.SOUND_VOLUME));
            WwiseAudioManager.Instance.World_Volume.SetGlobalValue(PlayerPrefs.GetFloat(PlayerPrefKey.SOUND_VOLUME));
        }
        else
        {
            WwiseAudioManager.Instance.UI_Volume.SetGlobalValue(100f);
            WwiseAudioManager.Instance.World_Volume.SetGlobalValue(100f);
        }
    }

    public void SetResolutionAndDisplayMode(int resolutionIndex, int displayModeIndex)
    {
        CurrentResolutionIndex = resolutionIndex;
        CurrentDisplayModeIndex = displayModeIndex;
        Resolution resolution = Resolutions[CurrentResolutionIndex];
        FullScreenMode fullScreenMode = FullScreenModes[CurrentDisplayModeIndex];
        PlayerPrefs.SetInt(PlayerPrefKey.RESOLUTION, CurrentResolutionIndex);
        PlayerPrefs.SetInt(PlayerPrefKey.DISPLAY_MODE, CurrentDisplayModeIndex);
        Screen.SetResolution(resolution.width, resolution.height, fullScreenMode, resolution.refreshRate);
    }

    public void SetMasterVolume(float value)
    {
        PlayerPrefs.SetFloat(PlayerPrefKey.MASTER_VOLUME, value);
        WwiseAudioManager.Instance.Master_Volume.SetGlobalValue(value);
    }

    public void SetSoundVolume(float value)
    {
        PlayerPrefs.SetFloat(PlayerPrefKey.SOUND_VOLUME, value);
        WwiseAudioManager.Instance.UI_Volume.SetGlobalValue(value);
        WwiseAudioManager.Instance.World_Volume.SetGlobalValue(value);
    }

    public void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat(PlayerPrefKey.MUSIC_VOLUME, value);
        WwiseAudioManager.Instance.Music_Volume.SetGlobalValue(value);
    }
}
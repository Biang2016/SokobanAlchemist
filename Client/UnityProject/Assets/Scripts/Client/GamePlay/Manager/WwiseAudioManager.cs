using System.Collections.Generic;
using AK.Wwise;
using BiangLibrary.Singleton;
using UnityEngine;

public class WwiseAudioManager : MonoSingleton<WwiseAudioManager>
{
    public WwiseBGMConfiguration WwiseBGMConfiguration;

    public AK.Wwise.RTPC InMenu_LowPass;
    public AK.Wwise.RTPC Master_Volume;
    public AK.Wwise.RTPC Music_Volume;
    public AK.Wwise.RTPC UI_Volume;
    public AK.Wwise.RTPC World_Volume;

    void Awake()
    {
        GetBGMThemeConfigDict();
        if (PlayerPrefs.HasKey("InMenu_LowPass"))
        {
            InMenu_LowPass.SetGlobalValue(PlayerPrefs.GetFloat("InMenu_LowPass"));
        }
        else
        {
            InMenu_LowPass.SetGlobalValue(100f);
        }

        if (PlayerPrefs.HasKey("Master_Volume"))
        {
            Master_Volume.SetGlobalValue(PlayerPrefs.GetFloat("Master_Volume"));
        }
        else
        {
            Master_Volume.SetGlobalValue(100f);
        }

        if (PlayerPrefs.HasKey("Music_Volume"))
        {
            Music_Volume.SetGlobalValue(PlayerPrefs.GetFloat("Music_Volume"));
        }
        else
        {
            Music_Volume.SetGlobalValue(100f);
        }

        if (PlayerPrefs.HasKey("UI_Volume"))
        {
            UI_Volume.SetGlobalValue(PlayerPrefs.GetFloat("UI_Volume"));
        }
        else
        {
            UI_Volume.SetGlobalValue(100f);
        }

        if (PlayerPrefs.HasKey("World_Volume"))
        {
            World_Volume.SetGlobalValue(PlayerPrefs.GetFloat("World_Volume"));
        }
        else
        {
            World_Volume.SetGlobalValue(100f);
        }
    }

    public void BGM_Start()
    {
        WwiseBGMConfiguration.BGM_StartEvent.Post(gameObject);
    }

    public void BGM_Stop()
    {
        WwiseBGMConfiguration.BGM_StopEvent.Post(gameObject);
    }

    public void SwitchBGMTheme(BGM_Theme bgmTheme)
    {
        if (BGMThemeConfigDict.TryGetValue(bgmTheme, out State state))
        {
            state.SetValue();
        }
    }

    private Dictionary<BGM_Theme, State> BGMThemeConfigDict = new Dictionary<BGM_Theme, State>();

    private void GetBGMThemeConfigDict()
    {
        if (BGMThemeConfigDict.Count == 0)
        {
            foreach (WwiseBGMConfiguration.BGMThemConfig bgmThemeConfig in WwiseBGMConfiguration.BGMThemeConfigs)
            {
                if (!BGMThemeConfigDict.ContainsKey(bgmThemeConfig.BGM_Theme))
                {
                    BGMThemeConfigDict.Add(bgmThemeConfig.BGM_Theme, bgmThemeConfig.State);
                }
                else
                {
                    Debug.LogError($"BGMThemeConfigs duplicated keys: {bgmThemeConfig.BGM_Theme}");
                }
            }
        }
    }

    public void ShutDown()
    {
        WwiseBGMConfiguration.StopAllEvent.Post(gameObject);
    }
}
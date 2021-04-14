using System.Collections.Generic;
using AK.Wwise;
using BiangLibrary.Singleton;
using UnityEngine;

public class WwiseAudioManager : MonoSingleton<WwiseAudioManager>
{
    public WwiseBGMConfiguration WwiseBGMConfiguration;

    public AK.Wwise.RTPC Master_Volume;
    public AK.Wwise.RTPC BGM_Volume;
    public AK.Wwise.RTPC SFX_Volume;
    public AK.Wwise.RTPC SFX_Entity_Volume;
    public AK.Wwise.RTPC SFX_UI_Volume;

    void Awake()
    {
        GetBGMThemeConfigDict();
        if (PlayerPrefs.HasKey("BGM_Volume"))
        {
            BGM_Volume.SetGlobalValue(PlayerPrefs.GetFloat("BGM_Volume"));
        }
        else
        {
            BGM_Volume.SetGlobalValue(100f);
        }

        if (PlayerPrefs.HasKey("SFX_Volume"))
        {
            SFX_Volume.SetGlobalValue(PlayerPrefs.GetFloat("SFX_Volume"));
        }
        else
        {
            SFX_Volume.SetGlobalValue(100f);
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
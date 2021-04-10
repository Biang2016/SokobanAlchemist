using System.Collections;
using System.Collections.Generic;
using AK.Wwise;
using BiangLibrary.Singleton;
using UnityEngine;

public class WwiseAudioManager : MonoSingleton<WwiseAudioManager>
{
    public WwiseBGMConfiguration WwiseBGMConfiguration;

    void Awake()
    {
        GetBGMThemeConfigDict();
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
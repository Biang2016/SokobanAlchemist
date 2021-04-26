using System.Collections.Generic;
using AK.Wwise;
using BiangLibrary.Singleton;
using UnityEngine;

public class WwiseAudioManager : MonoSingleton<WwiseAudioManager>
{
    public WwiseBGMConfiguration WwiseBGMConfiguration;

    public AK.Wwise.Event StopAllEvent;

    public void ShutDown()
    {
        StopAllEvent.Post(gameObject);
    }

    public RTPC InMenu_LowPass;
    public RTPC Master_Volume;
    public RTPC Music_Volume;
    public RTPC UI_Volume;
    public RTPC World_Volume;

    void Awake()
    {
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
}
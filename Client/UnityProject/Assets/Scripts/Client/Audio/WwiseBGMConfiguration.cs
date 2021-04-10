using System;
using System.Collections.Generic;
using AK.Wwise;
using UnityEngine;

public class WwiseBGMConfiguration : MonoBehaviour
{
    public AK.Wwise.Event StopAllEvent;
    public AK.Wwise.Event BGM_StartEvent;
    public AK.Wwise.Event BGM_StopEvent;

    public List<BGMThemConfig> BGMThemeConfigs = new List<BGMThemConfig>();

    [Serializable]
    public class BGMThemConfig
    {
        public BGM_Theme BGM_Theme;
        public State State;
    }
}
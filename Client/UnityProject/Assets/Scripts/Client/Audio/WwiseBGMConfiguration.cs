using System;
using System.Collections.Generic;
using AK.Wwise;
using Sirenix.OdinInspector;
using UnityEngine;

public class WwiseBGMConfiguration : MonoBehaviour
{
    public AK.Wwise.Event StopAllEvent;
    public AK.Wwise.Event BGM_StartEvent;
    public AK.Wwise.Event BGM_StopEvent;

    public List<BGMThemConfig> BGMThemeConfigs = new List<BGMThemConfig>();

    [FoldoutGroup("CombatState")]
    public State CombatState_None;

    [FoldoutGroup("CombatState")]
    public State CombatState_InBossCombat;

    [FoldoutGroup("CombatState")]
    public State CombatState_InCombat;

    [FoldoutGroup("CombatState")]
    public State CombatState_InEliteCombat;

    [FoldoutGroup("SpiderLegPhase")]
    public State SpiderLegPhase_0;

    [FoldoutGroup("SpiderLegPhase")]
    public State SpiderLegPhase_1;

    [FoldoutGroup("SpiderLegPhase")]
    public State SpiderLegPhase_2;

    [FoldoutGroup("SpiderLegPhase")]
    public State SpiderLegPhase_3;

    [Serializable]
    public class BGMThemConfig
    {
        public BGM_Theme BGM_Theme;
        public State State;
    }
}
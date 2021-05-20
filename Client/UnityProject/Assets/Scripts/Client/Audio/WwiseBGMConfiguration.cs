using System;
using System.Collections.Generic;
using AK.Wwise;
using Sirenix.OdinInspector;
using UnityEngine;

public class WwiseBGMConfiguration : MonoBehaviour
{
    public AK.Wwise.Event BGM_StartEvent;
    public AK.Wwise.Event BGM_StopEvent;

    public List<BGMThemeConfig> BGMThemeConfigs = new List<BGMThemeConfig>();

    private Dictionary<BGM_Theme, State> BGMThemeConfigDict = new Dictionary<BGM_Theme, State>();

    void Awake()
    {
        GetBGMThemeConfigDict();
    }

    public void BGM_Start()
    {
        BGM_StartEvent.Post(gameObject);
    }

    public void BGM_Stop()
    {
        BGM_StopEvent.Post(gameObject);
    }

    private void GetBGMThemeConfigDict()
    {
        if (BGMThemeConfigDict.Count == 0)
        {
            foreach (BGMThemeConfig bgmThemeConfig in BGMThemeConfigs)
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

    public BGM_Theme CurrentBGMTheme;

    [ShowInInspector]
    [HideInEditorMode]
    public CombatState currentCombatState => BattleManager.Instance.CombatState;

    public void SwitchBGMTheme(BGM_Theme bgmTheme)
    {
        if (BGMThemeConfigDict.TryGetValue(bgmTheme, out State state))
        {
            if (CurrentBGMTheme != bgmTheme)
            {
                //Debug.Log($"BGM switch to {bgmTheme}");
            }

            CurrentBGMTheme = bgmTheme;
            state.SetValue();
        }
    }

    [FoldoutGroup("CombatState")]
    public State CombatState_InCamp;

    [FoldoutGroup("CombatState")]
    public State CombatState_Exploring;

    [FoldoutGroup("CombatState")]
    public State CombatState_InCombat;

    [FoldoutGroup("CombatState")]
    public State CombatState_InEliteCombat;

    [FoldoutGroup("CombatState")]
    public State CombatState_InBossCombat;

    [FoldoutGroup("SpiderLegPhase")]
    public State SpiderLegPhase_1;

    [FoldoutGroup("SpiderLegPhase")]
    public State SpiderLegPhase_2;

    [FoldoutGroup("SpiderLegPhase")]
    public State SpiderLegPhase_3;

    public RTPC CombatEnemyNumber;
    public RTPC PlayerHealthPercent;
    public RTPC PlayerMovementSpeed;
    public RTPC SpiderLegEnemyDistanceToPlayer;

    [Serializable]
    public class BGMThemeConfig
    {
        public BGM_Theme BGM_Theme;
        public State State;
    }

    [ShowInInspector]
    [HideInEditorMode]
    public float spiderLegEnemyDistance => SpiderLegEnemyDistanceToPlayer.GetGlobalValue();

    [ShowInInspector]
    [HideInEditorMode]
    public float combatEnemyNumber => CombatEnemyNumber.GetGlobalValue();

    [ShowInInspector]
    [HideInEditorMode]
    public float playerHealthPercent => PlayerHealthPercent.GetGlobalValue();

    [ShowInInspector]
    [HideInEditorMode]
    public float playerMovementSpeed => PlayerMovementSpeed.GetGlobalValue();

    public void SetCombatState(CombatState combatState)
    {
        switch (combatState)
        {
            case CombatState.InCamp:
            {
                CombatState_InCamp.SetValue();
                break;
            }
            case CombatState.Exploring:
            {
                CombatState_Exploring.SetValue();
                break;
            }
            case CombatState.InCombat:
            {
                CombatState_InCombat.SetValue();
                break;
            }
            case CombatState.InEliteCombat:
            {
                CombatState_InEliteCombat.SetValue();
                break;
            }
            case CombatState.InBossCombat:
            {
                CombatState_InBossCombat.SetValue();
                break;
            }
            case CombatState.InSpiderLegCombat_Phase_1:
            {
                SpiderLegPhase_1.SetValue();
                break;
            }
            case CombatState.InSpiderLegCombat_Phase_2:
            {
                SpiderLegPhase_2.SetValue();
                break;
            }
            case CombatState.InSpiderLegCombat_Phase_3:
            {
                SpiderLegPhase_3.SetValue();
                break;
            }
        }
    }
}
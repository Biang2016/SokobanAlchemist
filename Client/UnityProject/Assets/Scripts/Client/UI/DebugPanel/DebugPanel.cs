﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BiangLibrary.GamePlay.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DebugPanel : BaseUIPanel
{
    public Button DebugToggleButton;
    public Gradient FrameRateGradient;
    public Text fpsText;

    private Dictionary<string, DebugPanelComponent> DebugComponentDictTree = new Dictionary<string, DebugPanelComponent>();
    private List<DebugPanelColumn> DebugButtonColumns = new List<DebugPanelColumn>();
    public Transform ColumnContainer;

    public UnityAction OnShortcutKeyDown = null;

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
        Init();
    }

    public void Clear()
    {
        foreach (KeyValuePair<string, DebugPanelComponent> kv in DebugComponentDictTree)
        {
            kv.Value.PoolRecycle();
        }

        DebugComponentDictTree.Clear();

        foreach (DebugPanelColumn column in DebugButtonColumns)
        {
            column.PoolRecycle();
        }

        DebugButtonColumns.Clear();
        OnShortcutKeyDown = null;
    }

    public void Init()
    {
        IsButtonsShow = false;
        Type type = typeof(DebugPanel);
        List<(MethodInfo, DebugControllerAttribute)> dcas = new List<(MethodInfo, DebugControllerAttribute)>();
        foreach (MethodInfo m in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
        {
            foreach (Attribute a in m.GetCustomAttributes(false))
            {
                if (a is DebugControllerAttribute dca)
                {
                    dcas.Add((m, dca));
                }
            }
        }

        dcas.Sort((pair1, pair2) => { return pair1.Item2.Priority.CompareTo(pair2.Item2.Priority); });

        foreach ((MethodInfo, DebugControllerAttribute) pair in dcas)
        {
            MethodInfo m = pair.Item1;
            switch (pair.Item2)
            {
                case DebugButtonAttribute dba:
                {
                    if (string.IsNullOrEmpty(dba.MethodName))
                    {
                        UnityAction action = () => { m.Invoke(this, new object[] { }); };
                        AddButton(dba.ButtonName, dba.Shortcut, 0, DebugComponentDictTree, action, true);
#if DEBUG || DEVELOPMENT_BUILD
                        if (dba.Shortcut != KeyCode.None)
                        {
                            OnShortcutKeyDown += () =>
                            {
                                if (Input.GetKeyDown(dba.Shortcut))
                                {
                                    action?.Invoke();
                                }
                            };
                        }
#endif
                    }
                    else
                    {
                        bool methodFound = false;
                        foreach (MethodInfo method in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
                        {
                            if (method.Name.Equals(dba.MethodName))
                            {
                                methodFound = true;
                                try
                                {
                                    List<string> strList = (List<string>) method.Invoke(this, new object[] { });
                                    foreach (string s in strList)
                                    {
                                        string buttonName = string.Format(dba.ButtonName, s);
                                        AddButton(buttonName, KeyCode.None, 0, DebugComponentDictTree, () => { m.Invoke(this, new object[] {s}); }, true);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Debug.LogError(e);
                                    throw;
                                }
                            }
                        }

                        if (!methodFound)
                        {
                            Debug.LogError($"[DebugPanel] 无法找到名为{dba.MethodName}的函数");
                        }
                    }

                    break;
                }
                case DebugToggleButtonAttribute dtba:
                {
                    AddButton(dtba.ButtonName, KeyCode.None, 0, DebugComponentDictTree, () => { m.Invoke(this, new object[] { }); }, true);
                    break;
                }
                case DebugSliderAttribute dsa:
                {
                    AddSlider(dsa.SliderName, 0, dsa.DefaultValue, dsa.Min, dsa.Max, DebugComponentDictTree, (float value) => { m.Invoke(this, new object[] {value}); });
                    break;
                }
            }
        }
    }

    public float showTime = 0.5f;
    private int count = 0;
    private float deltaTime = 0f;

    void Update()
    {
        count++;
        deltaTime += Time.deltaTime;
        if (deltaTime >= showTime)
        {
            float fps = count / deltaTime;
            count = 0;
            deltaTime = 0f;
            fpsText.text = fps.ToString("###");
            DebugToggleButton.image.color = FrameRateGradient.Evaluate(fps / 144f);
        }

        OnShortcutKeyDown?.Invoke();
    }

    private bool isButtonsShow = false;

    private bool IsButtonsShow
    {
        get { return isButtonsShow; }
        set
        {
            isButtonsShow = value;
            ColumnContainer.gameObject.SetActive(value);
        }
    }

    public void ToggleDebugPanel()
    {
        IsButtonsShow = !IsButtonsShow;
    }

    private void AddButton(string buttonName, KeyCode shortcut, int layerDepth, Dictionary<string, DebugPanelComponent> currentTree, UnityAction action, bool functional)
    {
        if (layerDepth >= DebugButtonColumns.Count)
        {
            DebugPanelColumn column = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.DebugPanelColumn].AllocateGameObject<DebugPanelColumn>(ColumnContainer);
            DebugButtonColumns.Add(column);
        }

        string[] paths = buttonName.Split('/');

        DebugPanelColumn currentColumn = DebugButtonColumns[layerDepth];
        if (!currentTree.ContainsKey(paths[0]))
        {
            DebugPanelButton btn = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.DebugPanelButton].AllocateGameObject<DebugPanelButton>(currentColumn.transform);
            bool isLeave = paths.Length == 1 && functional;
            btn.Initialize(
                paths[0],
                isLeave ? shortcut : KeyCode.None,
                isLeave ? action : ToggleSubColumn(btn, currentTree));
            btn.gameObject.SetActive(layerDepth == 0);
            currentTree.Add(paths[0], btn);
        }

        if (paths.Length > 1)
        {
            string remainingPath = buttonName.Replace(paths[0] + "/", "");
            AddButton(remainingPath, shortcut, layerDepth + 1, currentTree[paths[0]].DebugComponentDictTree, action, functional);
        }
    }

    private static UnityAction ToggleSubColumn(DebugPanelComponent cpm, Dictionary<string, DebugPanelComponent> currentTree)
    {
        return () =>
        {
            cpm.IsOpen = !cpm.IsOpen;
            foreach (KeyValuePair<string, DebugPanelComponent> kv in currentTree)
            {
                if (kv.Value != cpm && cpm.IsOpen)
                {
                    kv.Value.IsOpen = false;
                }
            }
        };
    }

    private void AddSlider(string sliderName, int layerDepth, float defaultValue, float min, float max, Dictionary<string, DebugPanelComponent> currentTree, UnityAction<float> action)
    {
        if (layerDepth >= DebugButtonColumns.Count)
        {
            DebugPanelColumn column = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.DebugPanelColumn].AllocateGameObject<DebugPanelColumn>(ColumnContainer);
            DebugButtonColumns.Add(column);
        }

        string[] paths = sliderName.Split('/');
        DebugPanelColumn currentColumn = DebugButtonColumns[layerDepth];
        if (paths.Length == 0)
        {
            return;
        }
        else if (paths.Length == 1)
        {
            if (!currentTree.ContainsKey(paths[0]))
            {
                DebugPanelSlider slider = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.DebugPanelSlider].AllocateGameObject<DebugPanelSlider>(currentColumn.transform);
                slider.Initialize(paths[0], defaultValue, min, max, action);
                slider.gameObject.SetActive(layerDepth == 0);
                currentTree.Add(paths[0], slider);
            }
        }
        else
        {
            if (currentTree.ContainsKey(paths[0]))
            {
                string remainingPath = sliderName.Replace(paths[0] + "/", "");
                AddSlider(remainingPath, layerDepth + 1, defaultValue, min, max, currentTree[paths[0]].DebugComponentDictTree, action);
                return;
            }
            else
            {
                string buttonPath = sliderName.Replace("/" + paths[paths.Length - 1], "");
                AddButton(buttonPath, KeyCode.None, layerDepth, currentTree, null, false);
                AddSlider(sliderName, layerDepth, defaultValue, min, max, currentTree, action);
                return;
            }
        }
    }

    [DebugButton("SwitchWorld/{0}", "GetAllWorldNames", -10)]
    public void ChangeWorld(string worldName)
    {
        ClientGameManager.Instance.ChangeWorld(worldName, true);
    }

    public List<string> GetAllWorldNames()
    {
        return ConfigManager.GetAllTypeNames(TypeDefineType.World, false);
    }

    [DebugButton("SwitchBornPoint/{0}", "GetAllWorldPlayerBornPoints", -9)]
    public void ChangeWorldPlayerBornPoint(string playerBornPointAlias)
    {
        BattleManager.Instance.Player1.TransportPlayerGridPos(WorldManager.Instance.CurrentWorld.WorldData.WorldBornPointGroupData_Runtime.PlayerBornPointDataAliasDict[playerBornPointAlias].WorldGP);
    }

    public List<string> GetAllWorldPlayerBornPoints()
    {
        return WorldManager.Instance.CurrentWorld.WorldData.WorldBornPointGroupData_Runtime.PlayerBornPointDataAliasDict.Keys.ToList();
    }

    [DebugSlider("Player/KickForce", 15, 0, 1000)]
    public void ChangeKickForce(float value)
    {
        for (int i = 0;
            i < BattleManager.Instance.MainPlayers.Length;
            i++)
        {
            Actor player = BattleManager.Instance.MainPlayers[i];
            if (player != null) player.KickForce = value;
        }
    }

    [DebugButton("Player/AddHealth*10")]
    public void AddHealth10()
    {
        BattleManager.Instance.Player1.EntityStatPropSet.MaxHealthDurability.AddModifier(new Property.PlusModifier {Delta = 10});
        BattleManager.Instance.Player1.EntityStatPropSet.HealthDurability.SetValue(BattleManager.Instance.Player1.EntityStatPropSet.HealthDurability.Value + 10, "DebugPanelAddHealth10");
    }

    [DebugButton("Player/AddHealth*100", KeyCode.Alpha7)]
    public void AddHealth100()
    {
        BattleManager.Instance.Player1.EntityStatPropSet.MaxHealthDurability.AddModifier(new Property.PlusModifier {Delta = 100});
        BattleManager.Instance.Player1.EntityStatPropSet.HealthDurability.SetValue(BattleManager.Instance.Player1.EntityStatPropSet.HealthDurability.Value + 100, "DebugPanelAddHealth100");
    }

    [DebugButton("Player/AddAllElement*100", KeyCode.Alpha5)]
    public void AddAllElement100()
    {
        BattleManager.Instance.Player1.EntityStatPropSet.Gold.SetValue(BattleManager.Instance.Player1.EntityStatPropSet.Gold.Value + 10);
        BattleManager.Instance.Player1.EntityStatPropSet.FireElementFragment.SetValue(BattleManager.Instance.Player1.EntityStatPropSet.FireElementFragment.Value + 100);
        BattleManager.Instance.Player1.EntityStatPropSet.IceElementFragment.SetValue(BattleManager.Instance.Player1.EntityStatPropSet.IceElementFragment.Value + 100);
        BattleManager.Instance.Player1.EntityStatPropSet.LightningElementFragment.SetValue(BattleManager.Instance.Player1.EntityStatPropSet.LightningElementFragment.Value + 100);
    }

    [DebugButton("Player/AddFire*1")]
    public void AddFire()
    {
        BattleManager.Instance.Player1.EntityStatPropSet.FireElementFragment.SetValue(BattleManager.Instance.Player1.EntityStatPropSet.FireElementFragment.Value + 1);
    }

    [DebugButton("Player/AddIce*1")]
    public void AddIce()
    {
        BattleManager.Instance.Player1.EntityStatPropSet.IceElementFragment.SetValue(BattleManager.Instance.Player1.EntityStatPropSet.IceElementFragment.Value + 1);
    }

    [DebugButton("Player/AddLightning*1")]
    public void AddLightning()
    {
        BattleManager.Instance.Player1.EntityStatPropSet.LightningElementFragment.SetValue(BattleManager.Instance.Player1.EntityStatPropSet.LightningElementFragment.Value + 1);
    }

    [DebugButton("Player/TakeDamage*10", KeyCode.Alpha6)]
    public void TakeDamage10()
    {
        BattleManager.Instance.Player1.EntityStatPropSet.HealthDurability.SetValue(BattleManager.Instance.Player1.EntityStatPropSet.HealthDurability.Value - 10, "DebugPanelTakeDamage10");
    }

    [DebugButton("Player/AddAction*10", KeyCode.Alpha8)]
    public void AddAction10()
    {
        int maxActionPointDelta = BattleManager.Instance.Player1.EntityStatPropSet.ActionPoint.Value + 10 - BattleManager.Instance.Player1.EntityStatPropSet.MaxActionPoint.GetModifiedValue;
        maxActionPointDelta = Mathf.Max(0, maxActionPointDelta);
        BattleManager.Instance.Player1.EntityStatPropSet.MaxActionPoint.AddModifier(new Property.PlusModifier {Delta = maxActionPointDelta});
        BattleManager.Instance.Player1.EntityStatPropSet.ActionPoint.SetValue(BattleManager.Instance.Player1.EntityStatPropSet.ActionPoint.Value + 10, "DebugPanelAddAction10");
    }

    [DebugButton("Player/AddActionRecover*100", KeyCode.Alpha0)]
    public void AddActionRecover100()
    {
        BattleManager.Instance.Player1.EntityStatPropSet.ActionPointRecovery.AddModifier(new Property.PlusModifier {Delta = 100});
    }

    [DebugToggleButton("Log/ToggleActorMoveLog")]
    public void ToggleActorMoveLog()
    {
        Actor.ENABLE_ACTOR_MOVE_LOG = !Actor.ENABLE_ACTOR_MOVE_LOG;
    }

    [DebugToggleButton("Log/ToggleBoxLog")]
    public void ToggleBoxLog()
    {
        Box.ENABLE_BOX_MOVE_LOG = !Box.ENABLE_BOX_MOVE_LOG;
    }

    [DebugToggleButton("Log/ToggleAIPathFindingLog")]
    public void ToggleAIPathFindingLog()
    {
        ActorAIAgent.ENABLE_ACTOR_AI_AGENT_LOG = !ActorAIAgent.ENABLE_ACTOR_AI_AGENT_LOG;
    }

    [DebugToggleButton("Enemy/TogglePathFinding")]
    public void ToggleEnemyPathFinding()
    {
        ConfigManager.ShowEnemyPathFinding = !ConfigManager.ShowEnemyPathFinding;
        foreach (Actor enemy in BattleManager.Instance.Enemies)
        {
            enemy.ActorAIAgent.SetNavTrackMarkersShown(ConfigManager.ShowEnemyPathFinding);
        }
    }

    [DebugButton("Enemy/KillAllEnemy", KeyCode.Alpha9)]
    public void KillAllEnemy()
    {
        foreach (Actor enemy in BattleManager.Instance.Enemies)
        {
            enemy.EntityStatPropSet.HealthDurability.SetValue(enemy.EntityStatPropSet.HealthDurability.Value - 99999, "DebugPanelKillAllEnemy");
        }
    }

    [DebugSlider("Boxes/ThrowFriction", 10f, 0, 20f, 10)]
    public void ChangeBoxThrowDrag(float value)
    {
        ConfigManager.BoxThrowDragFactor_Cheat = value;
    }

    [DebugSlider("Boxes/KickFriction", 1, 0, 20f, 10)]
    public void ChangeBoxKickDrag(float value)
    {
        ConfigManager.BoxKickDragFactor_Cheat = value;
    }

    [DebugSlider("Boxes/WeightFactor", 1, 0.1f, 10f, 10)]
    public void ChangeBoxWeight(float value)
    {
        ConfigManager.BoxWeightFactor_Cheat = value;
    }

    [DebugSlider("Game/TimeScale", 1f, 0, 1f)]
    public void ChangeTimeScale(float value)
    {
        Time.timeScale = value;
    }

    [DebugSlider("Audio/InMenu_LowPass", 100, 0, 100)]
    public void Set_InMenu_LowPass(float value)
    {
        PlayerPrefs.SetFloat("InMenu_LowPass", value);
        WwiseAudioManager.Instance.InMenu_LowPass.SetGlobalValue(value);
    }

    [DebugSlider("Audio/Master_Volume", 100, 0, 100)]
    public void Set_Master_Volume(float value)
    {
        PlayerPrefs.SetFloat("Master_Volume", value);
        WwiseAudioManager.Instance.Master_Volume.SetGlobalValue(value);
    }

    [DebugSlider("Audio/Music_Volume", 100, 0, 100)]
    public void Set_Music_Volume(float value)
    {
        PlayerPrefs.SetFloat("Music_Volume", value);
        WwiseAudioManager.Instance.Music_Volume.SetGlobalValue(value);
    }

    [DebugSlider("Audio/UI_Volume", 100, 0, 100)]
    public void Set_UI_Volume(float value)
    {
        PlayerPrefs.SetFloat("UI_Volume", value);
        WwiseAudioManager.Instance.UI_Volume.SetGlobalValue(value);
    }

    [DebugSlider("Audio/World_Volume", 100, 0, 100)]
    public void Set_World_Volume(float value)
    {
        PlayerPrefs.SetFloat("World_Volume", value);
        WwiseAudioManager.Instance.World_Volume.SetGlobalValue(value);
    }
}
using System;
using System.Collections.Generic;
using System.Reflection;
using BiangStudio.GamePlay.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DebugPanel : BaseUIPanel
{
    public Button DebugToggleButton;
    public Gradient FrameRateGradient;
    public Text fpsText;
    private float deltaTime;

    private Dictionary<string, DebugPanelComponent> DebugComponentDictTree = new Dictionary<string, DebugPanelComponent>();
    private List<DebugPanelColumn> DebugButtonColumns = new List<DebugPanelColumn>();
    public Transform ColumnContainer;

    void Awake()
    {
        UIType.InitUIType(
            false,
            false,
            false,
            UIFormTypes.Fixed,
            UIFormShowModes.Normal,
            UIFormLucencyTypes.Penetrable);
    }

    void Start()
    {
        IsButtonsShow = false;
        Type type = typeof(DebugPanel);
        foreach (MethodInfo m in type.GetMethods())
        {
            foreach (Attribute a in m.GetCustomAttributes(true))
            {
                if (a is DebugButtonAttribute dba)
                {
                    AddButton(dba.ButtonName, 0, DebugComponentDictTree, () => { m.Invoke(this, new object[] { }); }, true);
                }
            }
        }

        foreach (MethodInfo m in type.GetMethods())
        {
            foreach (Attribute a in m.GetCustomAttributes(true))
            {
                if (a is DebugSliderAttribute dsa)
                {
                    AddSlider(dsa.SliderName, 0, dsa.DefaultValue, dsa.Min, dsa.Max, DebugComponentDictTree, (float value) => { m.Invoke(this, new object[] {value}); });
                }
            }
        }
    }

    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        fpsText.text = fps.ToString("###");
        DebugToggleButton.image.color = FrameRateGradient.Evaluate(fps / 144f);
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

    private void AddButton(string buttonName, int layerDepth, Dictionary<string, DebugPanelComponent> currentTree, UnityAction action, bool functional)
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
            btn.Initialize(paths[0], (paths.Length == 1 && functional)
                ? action
                : ToggleSubColumn(btn, currentTree));
            btn.gameObject.SetActive(layerDepth == 0);
            currentTree.Add(paths[0], btn);
        }

        if (paths.Length > 1)
        {
            string remainingPath = buttonName.Replace(paths[0] + "/", "");
            AddButton(remainingPath, layerDepth + 1, currentTree[paths[0]].DebugComponentDictTree, action, functional);
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
                AddButton(buttonPath, layerDepth, currentTree, null, false);
                AddSlider(sliderName, layerDepth, defaultValue, min, max, currentTree, action);
                return;
            }
        }
    }

    [DebugSlider("PlayerBattle/MoveSpeed", 3.5f, 1, 30)]
    public void ChangeMoveSpeed(float value)
    {
        for (int i = 0; i < BattleManager.Instance.MainPlayers.Length; i++)
        {
            PlayerActor player = BattleManager.Instance.MainPlayers[i];
            if (player != null) player.MoveSpeed = value;
        }
    }

    [DebugSlider("PlayerBattle/KickForce", 150, 0, 1000)]
    public void ChangeKickForce(float value)
    {
        for (int i = 0; i < BattleManager.Instance.MainPlayers.Length; i++)
        {
            PlayerActor player = BattleManager.Instance.MainPlayers[i];
            if (player != null) player.KickForce = value;
        }
    }

    [DebugSlider("Boxes/ThrowFriction", 10f, 0, 20f)]
    public void ChangeBoxThrowDrag(float value)
    {
        ConfigManager.BoxThrowDragFactor_Cheat = value;
    }

    [DebugSlider("Boxes/KickFriction", 1, 0, 20f)]
    public void ChangeBoxKickDrag(float value)
    {
        ConfigManager.BoxKickDragFactor_Cheat = value;
    }

    [DebugSlider("Boxes/WeightFactor", 1, 0.1f, 10f)]
    public void ChangeBoxWeight(float value)
    {
        ConfigManager.BoxWeightFactor_Cheat = value;
    }

    [DebugButton("SwitchWorld/ChessWorld")]
    public void ChangeChessWorld()
    {
        ClientGameManager.DebugChangeWorldName = "ChessWorld";
        SceneManager.LoadScene("MainScene");
    }

    [DebugButton("SwitchWorld/ArenaWorld")]
    public void ChangeArenaWorld()
    {
        ClientGameManager.DebugChangeWorldName = "ArenaWorld";
        SceneManager.LoadScene("MainScene");
    }

    [DebugButton("SwitchWorld/BasicWorld")]
    public void ChangeWilliamsWorld()
    {
        ClientGameManager.DebugChangeWorldName = "BasicWorld";
        SceneManager.LoadScene("MainScene");
    }
}
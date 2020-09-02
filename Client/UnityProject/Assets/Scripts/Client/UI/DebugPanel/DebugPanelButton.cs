using System.Collections.Generic;
using UnityEngine;
using BiangStudio.ObjectPool;
using UnityEngine.Events;
using UnityEngine.UI;

public class DebugPanelButton : DebugPanelComponent
{
    public Button Button;
    public Text Text;
    public Color CloseColor;
    public Color OpenColor;

    public override void OnRecycled()
    {
        Button.onClick.RemoveAllListeners();
        base.OnRecycled();
    }

    public void Initialize(string buttonName, UnityAction action)
    {
        name = "btn_" + buttonName;
        IsOpen = false;
        Button.image.color = CloseColor;
        Text.text = buttonName;
        Button.onClick.RemoveAllListeners();
        if (action != null) Button.onClick.AddListener(action);
    }

    private bool isOpen;

    public override bool IsOpen
    {
        get { return isOpen; }
        set
        {
            Button.image.color = value ? OpenColor : CloseColor;
            foreach (KeyValuePair<string, DebugPanelComponent> kv in DebugComponentDictTree)
            {
                kv.Value.gameObject.SetActive(value);
                kv.Value.IsOpen = false;
            }

            isOpen = value;
        }
    }
}
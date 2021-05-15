using System.Collections;
using System.Collections.Generic;
using BiangLibrary.GamePlay.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TransportWorldPanel : BaseUIPanel
{
    void Awake()
    {
        UIType.InitUIType(
            false,
            true,
            false,
            UIFormTypes.Normal,
            UIFormShowModes.Normal,
            UIFormLucencyTypes.Penetrable);
    }

    public Animator Anim;

    //[SerializeField]
    //private Image WorldIcon;

    [SerializeField]
    private Text WorldDescription;

    [SerializeField]
    private Text WorldCost;

    [SerializeField]
    private Image TransportButtonIcon;

    [SerializeField]
    private Sprite TransportButtonIconNormalSprite_Keyboard;

    [SerializeField]
    private Sprite TransportButtonIconGrayOutSprite_Keyboard;

    [SerializeField]
    private Sprite TransportButtonIconNormalSprite_Controller;

    [SerializeField]
    private Sprite TransportButtonIconGrayOutSprite_Controller;

    [SerializeField]
    private TextMeshProUGUI ESCText;

    private UnityAction current_TransportCallBack;
    private UnityAction current_TransportAction;
    private WorldData current_RawWorldData;

    private Stack<TransportInfo> TransportInfoStack = new Stack<TransportInfo>();

    public AK.Wwise.Event OnDisplay;

    private class TransportInfo
    {
        public WorldData RawWorldData;
        public UnityAction TransportCallback;
        public int GoldCost;
    }

    public void Initialize(WorldData rawWorldData, UnityAction transportCallback, int goldCost)
    {
        TransportInfoStack.Push(new TransportInfo
        {
            RawWorldData = rawWorldData,
            TransportCallback = transportCallback,
            GoldCost = goldCost
        });

        Anim.SetTrigger("Jump");
        OnDisplay?.Post(gameObject);

        current_RawWorldData = rawWorldData;
        current_TransportCallBack = transportCallback;

        //Sprite sprite = ConfigManager.GetEntitySkillIconByName(rawWorldData.WorldIcon.TypeName);
        //WorldIcon.sprite = sprite;
        WorldDescription.text = $"*{rawWorldData.WorldName_EN}*\n{rawWorldData.WorldDescription_EN}";
        if (goldCost > 0) WorldCost.text = goldCost.ToString();
        else WorldCost.text = "Free";

        bool canAfford = BattleManager.Instance.Player1.EntityStatPropSet.Gold.Value >= goldCost;
        switch (ControlManager.Instance.CurrentControlScheme)
        {
            case ControlManager.ControlScheme.GamePad:
            {
                ESCText.text = "<sprite name=Start> x";
                TransportButtonIcon.sprite = canAfford ? TransportButtonIconNormalSprite_Controller : TransportButtonIconGrayOutSprite_Controller;
                break;
            }
            case ControlManager.ControlScheme.KeyboardMouse:
            {
                ESCText.text = "ESC x";
                TransportButtonIcon.sprite = canAfford ? TransportButtonIconNormalSprite_Keyboard : TransportButtonIconGrayOutSprite_Keyboard;
                break;
            }
        }

        if (canAfford)
        {
            current_TransportAction = () =>
            {
                current_TransportAction = null;
                current_TransportCallBack?.Invoke();
                CloseUIForm();
            };
        }
        else
        {
            current_TransportAction = () => { BattleManager.Instance.Player1.EntityStatPropSet.Gold.m_NotifyActionSet.OnValueNotEnoughWarning?.Invoke(); };
        }
    }

    protected override void ChildUpdate()
    {
        base.ChildUpdate();
        if (ControlManager.Instance.Battle_InteractiveKey.Down)
        {
            current_TransportAction?.Invoke();
        }

        if (ControlManager.Instance.Menu_ExitMenuPanel.Up)
        {
            CloseUIForm();
        }
    }

    public override void Display()
    {
        base.Display();
        UIManager.Instance.CloseUIForm<InGameUIPanel>();
    }

    public override void Hide()
    {
        base.Hide();
        if (TransportInfoStack.Count > 0) TransportInfoStack.Pop();
        if (TransportInfoStack.Count == 0)
        {
            UIManager.Instance.ShowUIForms<InGameUIPanel>();
            current_TransportAction = null;
            current_TransportCallBack = null;
            current_RawWorldData = null;
            Anim.SetTrigger("Hide");
        }
        else
        {
            TransportInfo transportInfo = TransportInfoStack.Pop();
            UIManager.Instance.ShowUIForms<TransportWorldPanel>();
            Initialize(transportInfo.RawWorldData, transportInfo.TransportCallback, transportInfo.GoldCost);
        }
    }
}
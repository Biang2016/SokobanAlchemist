using System.Collections;
using System.Collections.Generic;
using BiangLibrary.GamePlay.UI;
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
    private Image ButtonIcon;

    [SerializeField]
    private Sprite ButtonIconNormalSprite;

    [SerializeField]
    private Sprite ButtonIconGrayOutSprite;

    private UnityAction current_LearnCallBack;
    private UnityAction current_LearnAction;
    private WorldData current_RawWorldData;

    private Stack<TransportInfo> TransportInfoStack = new Stack<TransportInfo>();

    public AK.Wwise.Event OnDisplay;

    private class TransportInfo
    {
        public WorldData RawWorldData;
        public UnityAction LearnCallback;
        public int GoldCost;
    }

    public void Initialize(WorldData rawWorldData, UnityAction learnCallback, int goldCost)
    {
        TransportInfoStack.Push(new TransportInfo
        {
            RawWorldData = rawWorldData,
            LearnCallback = learnCallback,
            GoldCost = goldCost
        });

        Anim.SetTrigger("Jump");
        OnDisplay?.Post(gameObject);

        current_RawWorldData = rawWorldData;
        current_LearnCallBack = learnCallback;

        //Sprite sprite = ConfigManager.GetEntitySkillIconByName(rawWorldData.WorldIcon.TypeName);
        //WorldIcon.sprite = sprite;
        WorldDescription.text = $"*{rawWorldData.WorldName_EN}*\n{rawWorldData.WorldDescription_EN}";
        if (goldCost > 0) WorldCost.text = goldCost.ToString();
        else WorldCost.text = "Free";

        bool canAfford = BattleManager.Instance.Player1.EntityStatPropSet.Gold.Value >= goldCost;
        ButtonIcon.sprite = canAfford ? ButtonIconNormalSprite : ButtonIconGrayOutSprite;

        if (canAfford)
        {
            current_LearnAction = () =>
            {
                current_LearnAction = null;
                current_LearnCallBack?.Invoke();
                CloseUIForm();
            };
        }
        else
        {
            current_LearnAction = () => { BattleManager.Instance.Player1.EntityStatPropSet.Gold.m_NotifyActionSet.OnValueNotEnoughWarning?.Invoke(); };
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (ControlManager.Instance.Common_InteractiveKey.Down)
        {
            current_LearnAction?.Invoke();
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
            current_LearnAction = null;
            current_LearnCallBack = null;
            current_RawWorldData = null;
            Anim.SetTrigger("Hide");
        }
        else
        {
            TransportInfo transportInfo = TransportInfoStack.Pop();
            UIManager.Instance.ShowUIForms<TransportWorldPanel>();
            Initialize(transportInfo.RawWorldData, transportInfo.LearnCallback, transportInfo.GoldCost);
        }
    }
}
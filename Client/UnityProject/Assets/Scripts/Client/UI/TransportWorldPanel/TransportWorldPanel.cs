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
            false,
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

    internal int openStackTimes = 0;
    private UnityAction current_LearnCallBack;
    private UnityAction current_LearnAction;
    private WorldData current_RawWorldData;

    private Stack<TransportInfo> TransportInfoStack = new Stack<TransportInfo>();

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

        openStackTimes++;

        current_RawWorldData = rawWorldData;
        current_LearnCallBack = learnCallback;

        //Sprite sprite = ConfigManager.GetEntitySkillIconByName(rawWorldData.WorldIcon.TypeName);
        //WorldIcon.sprite = sprite;
        WorldDescription.text = $"*{rawWorldData.WorldName_EN}*\n{rawWorldData.WorldDescription_EN}";
        WorldCost.gameObject.SetActive(goldCost > 0);
        if (goldCost > 0) WorldCost.text = goldCost.ToString();
        else WorldCost.text = "Free";

        bool canAfford = BattleManager.Instance.Player1.EntityStatPropSet.Gold.Value >= goldCost;
        ButtonIcon.sprite = canAfford ? ButtonIconNormalSprite : ButtonIconGrayOutSprite;

        if (canAfford)
        {
            current_LearnAction = () =>
            {
                TransportInfoStack.Pop();
                current_LearnAction = null;
                current_LearnCallBack?.Invoke();
                CloseUIForm();
            };
        }
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.F))
        {
            current_LearnAction?.Invoke();
        }
    }

    public override void Display()
    {
        base.Display();
        UIManager.Instance.UI3DRoot.gameObject.SetActive(false);
    }

    public override void Hide()
    {
        openStackTimes--;
        base.Hide();

        if (openStackTimes == 0)
        {
            UIManager.Instance.UI3DRoot.gameObject.SetActive(true);
            current_LearnAction = null;
            current_LearnCallBack = null;
            current_RawWorldData = null;
        }

        if (openStackTimes > 0)
        {
            UIManager.Instance.ShowUIForms<TransportWorldPanel>();
            TransportInfo transportInfo = TransportInfoStack.Pop();
            Initialize(transportInfo.RawWorldData, transportInfo.LearnCallback, transportInfo.GoldCost);
            openStackTimes--;
        }
    }
}
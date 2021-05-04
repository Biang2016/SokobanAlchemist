using System.Collections;
using System.Collections.Generic;
using BiangLibrary.GamePlay.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LearnUpgradePanel : BaseUIPanel
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

    [SerializeField]
    private Image UpgradeIcon;

    [SerializeField]
    private Text UpgradeName;

    [SerializeField]
    private Text UpgradeDescription;

    [SerializeField]
    private Text UpgradeCost;

    [SerializeField]
    private Image ButtonIcon;

    [SerializeField]
    private Sprite ButtonIconNormalSprite;

    [SerializeField]
    private Sprite ButtonIconGrayOutSprite;

    internal int openStackTimes = 0;
    private UnityAction current_LearnCallBack;
    private UnityAction current_LearnAction;
    private EntityUpgrade current_EntityUpgrade;
    private Stack<UpgradeInfo> UpgradeInfoStack = new Stack<UpgradeInfo>();

    private class UpgradeInfo
    {
        public EntityUpgrade EntityUpgrade;
        public UnityAction LearnCallback;
        public int GoldCost;
    }

    public void Initialize(EntityUpgrade entityUpgrade, int goldCost)
    {
    }

    public void Initialize(EntityUpgrade entityUpgrade, UnityAction learnCallback, int goldCost)
    {
        UpgradeInfoStack.Push(new UpgradeInfo
        {
            EntityUpgrade = entityUpgrade,
            LearnCallback = learnCallback,
            GoldCost = goldCost
        });

        Anim.SetTrigger("Jump");

        openStackTimes++;

        current_LearnCallBack = learnCallback;
        current_EntityUpgrade = entityUpgrade.Clone();

        Sprite sprite = ConfigManager.GetEntitySkillIconByName(entityUpgrade.UpgradeIcon.TypeName);
        UpgradeIcon.sprite = sprite;
        UpgradeName.text = entityUpgrade.UpgradeName_EN;
        UpgradeDescription.text = entityUpgrade.UpgradeDescription_EN;
        UpgradeCost.gameObject.SetActive(goldCost > 0);
        if (goldCost > 0) UpgradeCost.text = goldCost.ToString();
        else UpgradeCost.text = "Free";

        bool canAfford = BattleManager.Instance.Player1.EntityStatPropSet.Gold.Value >= goldCost;
        ButtonIcon.sprite = canAfford ? ButtonIconNormalSprite : ButtonIconGrayOutSprite;

        if (canAfford)
        {
            current_LearnAction = () =>
            {
                UpgradeInfoStack.Pop();
                BattleManager.Instance.Player1.GetUpgraded(current_EntityUpgrade);
                ClientGameManager.Instance.NoticePanel.ShowTip("Successfully upgrade!", NoticePanel.TipPositionType.Center, 1f);
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
        UIManager.Instance.CloseUIForm<InGameUIPanel>();
    }

    public override void Hide()
    {
        openStackTimes--;
        base.Hide();

        if (openStackTimes == 0)
        {
            UIManager.Instance.ShowUIForms<InGameUIPanel>();
            current_LearnAction = null;
            current_LearnCallBack = null;
            current_EntityUpgrade = null;
            Anim.SetTrigger("Hide");
        }

        if (openStackTimes > 0)
        {
            UIManager.Instance.ShowUIForms<LearnUpgradePanel>();
            UpgradeInfo upgradeInfo = UpgradeInfoStack.Pop();
            Initialize(upgradeInfo.EntityUpgrade, upgradeInfo.LearnCallback, upgradeInfo.GoldCost);
            openStackTimes--;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using BiangLibrary.GamePlay.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LearnSkillPanel : BaseUIPanel
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
    private Image SkillIcon;

    [SerializeField]
    private Text SkillName;

    [SerializeField]
    private Text SkillKeyBind;

    [SerializeField]
    private Text SkillDescription;

    [SerializeField]
    private Text SkillCost;

    [SerializeField]
    private Image ButtonIcon;

    [SerializeField]
    private Sprite ButtonIconNormalSprite;

    [SerializeField]
    private Sprite ButtonIconGrayOutSprite;

    internal int openStackTimes = 0;
    private UnityAction current_LearnCallBack;
    private UnityAction current_LearnAction;
    private EntitySkill current_EntitySkill;

    private Stack<SkillInfo> SkillInfoStack = new Stack<SkillInfo>();

    private class SkillInfo
    {
        public string SkillGUID;
        public UnityAction LearnCallback;
        public PlayerControllerHelper.KeyBind KeyBind;
        public int GoldCost;
    }

    public void Initialize(string skillGUID, UnityAction learnCallback, PlayerControllerHelper.KeyBind keyBind, int goldCost)
    {
        SkillInfoStack.Push(new SkillInfo
        {
            SkillGUID = skillGUID,
            LearnCallback = learnCallback,
            KeyBind = keyBind,
            GoldCost = goldCost
        });

        Anim.SetTrigger("Jump");

        openStackTimes++;

        current_LearnCallBack = learnCallback;
        current_EntitySkill = ConfigManager.GetEntitySkill(skillGUID);

        if (current_EntitySkill != null)
        {
            bool specifyKeyBind = current_EntitySkill is EntityActiveSkill;

            string keyBindStr = specifyKeyBind ? PlayerControllerHelper.KeyMappingStrDict[keyBind] : "";
            Sprite sprite = ConfigManager.GetEntitySkillIconByName(current_EntitySkill.SkillIcon.TypeName);
            SkillIcon.sprite = sprite;
            SkillDescription.text = current_EntitySkill.GetSkillDescription_EN;
            SkillName.text = current_EntitySkill.SkillName_EN;
            SkillKeyBind.gameObject.SetActive(!string.IsNullOrWhiteSpace(keyBindStr));
            SkillKeyBind.text = keyBindStr;
            SkillCost.gameObject.SetActive(goldCost > 0);
            if (goldCost > 0) SkillCost.text = goldCost.ToString();
            else SkillCost.text = "Free";

            bool canAfford = BattleManager.Instance.Player1.EntityStatPropSet.Gold.Value >= goldCost;
            ButtonIcon.sprite = canAfford ? ButtonIconNormalSprite : ButtonIconGrayOutSprite;

            if (current_EntitySkill is EntityPassiveSkill EPS)
            {
                if (canAfford)
                {
                    current_LearnAction = () =>
                    {
                        SkillInfoStack.Pop();
                        BattleManager.Instance.Player1.AddNewPassiveSkill(EPS);
                        ClientGameManager.Instance.NoticePanel.ShowTip("Successfully learn skill!", NoticePanel.TipPositionType.Center, 1f);
                        current_LearnAction = null;
                        current_LearnCallBack?.Invoke();
                        CloseUIForm();
                    };
                }
            }
            else if (current_EntitySkill is EntityActiveSkill EAS)
            {
                if (canAfford)
                {
                    current_LearnAction = () =>
                    {
                        SkillInfoStack.Pop();
                        if (BattleManager.Instance.Player1.AddNewActiveSkill(EAS))
                        {
                            BattleManager.Instance.Player1.BindActiveSkillToKey(EAS, keyBind, true);
                            ClientGameManager.Instance.NoticePanel.ShowTip("Successfully learn skill!", NoticePanel.TipPositionType.Center, 1f);
                        }

                        current_LearnAction = null;
                        current_LearnCallBack?.Invoke();
                        CloseUIForm();
                    };
                }
            }
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
            current_EntitySkill = null;
        }

        if (openStackTimes > 0)
        {
            UIManager.Instance.ShowUIForms<LearnSkillPanel>();
            SkillInfo skillInfo = SkillInfoStack.Pop();
            Initialize(skillInfo.SkillGUID, skillInfo.LearnCallback, skillInfo.KeyBind, skillInfo.GoldCost);
            openStackTimes--;
        }
    }
}
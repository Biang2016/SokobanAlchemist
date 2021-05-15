using System.Collections;
using BiangLibrary.ObjectPool;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;

public class LearnSkillUpgradePage : PoolObject
{
    #region GUID

    [ReadOnly]
    [HideInEditorMode]
    [ShowInInspector]
    internal uint GUID;

    private static uint guidGenerator = (uint) ConfigManager.GUID_Separator.LearnSkillUpgradePage;

    protected uint GetGUID()
    {
        return guidGenerator++;
    }

    #endregion

    public Animator Anim;

    [SerializeField]
    private TextMeshProUGUI ESCText;

    [SerializeField]
    private GameObject QESwitching;

    [SerializeField]
    private TextMeshProUGUI QESwitchingTextTMP;

    [SerializeField]
    private Image Icon;

    [SerializeField]
    private Text TitleText;

    [SerializeField]
    private Text NameText;

    [SerializeField]
    private Text KeyBindText;

    [SerializeField]
    private Text DescriptionText;

    [SerializeField]
    private Text CostText;

    [SerializeField]
    private Image LearnButtonIcon;

    [SerializeField]
    private Sprite LearnButtonIconNormalSprite_Keyboard;

    [SerializeField]
    private Sprite LearnButtonIconGrayOutSprite_Keyboard;

    [SerializeField]
    private Sprite LearnButtonIconNormalSprite_Controller;

    [SerializeField]
    private Sprite LearnButtonIconGrayOutSprite_Controller;

    private UnityAction current_LearnCallBack;
    internal UnityAction current_LearnAction;

    public override void OnRecycled()
    {
        ClientGameManager.Instance.LearnSkillUpgradePanel.PageDict.Remove(GUID);
        base.OnRecycled();
        Anim.SetTrigger("Hide");
        current_LearnCallBack = null;
        current_LearnAction = null;
    }

    public override void OnUsed()
    {
        base.OnUsed();
    }

    public void Initialize(LearnInfo learnInfo)
    {
        GUID = GetGUID();
        ClientGameManager.Instance.LearnSkillUpgradePanel.PageDict.Add(GUID, this);
        current_LearnCallBack = learnInfo.LearnCallback;
        string leftSwitchStr = ControlManager.Instance.GetControlDescText(ButtonNames.Battle_LeftSwitch, false);
        string rightSwitchStr = ControlManager.Instance.GetControlDescText(ButtonNames.Battle_RightSwitch, false);

        if (ControlManager.Instance.CurrentControlScheme == ControlManager.ControlScheme.GamePad)
        {
            leftSwitchStr = $"<sprite name={leftSwitchStr}>";
            rightSwitchStr = $"<sprite name={rightSwitchStr}>";
        }

        QESwitchingTextTMP.text = $"↑\n{leftSwitchStr}\n{rightSwitchStr}\n↓";
        string escStr = ControlManager.Instance.GetControlDescText(ButtonNames.Common_Exit, false);
        if (ControlManager.Instance.CurrentControlScheme == ControlManager.ControlScheme.GamePad)
        {
            ESCText.text = $"<sprite name={escStr}> x";
        }
        else
        {
            ESCText.text = $"ESC x";
        }

        bool canAfford = false;
        switch (learnInfo.LearnType)
        {
            case LearnType.Skill:
            {
                TitleText.text = "New Skill";
                EntitySkill rawEntitySkill = ConfigManager.GetRawEntitySkill(learnInfo.SkillGUID);
                Assert.IsNotNull(rawEntitySkill);

                Sprite sprite = ConfigManager.GetEntitySkillIconByName(rawEntitySkill.SkillIcon.TypeName);
                Icon.sprite = sprite;
                DescriptionText.text = rawEntitySkill.GetSkillDescription_EN;
                NameText.text = rawEntitySkill.SkillName_EN;
                KeyBindText.text = "";
                if (rawEntitySkill is EntityActiveSkill _rawEAS && _rawEAS.NeedBindKey)
                {
                    PlayerControllerHelper.KeyMappingDict.TryGetValue(_rawEAS.SkillKeyBind, out ButtonNames keyBindButtonName);
                    string keyBindStr = ControlManager.Instance.GetControlDescText(keyBindButtonName, false);
                    KeyBindText.text = keyBindStr;
                }

                if (learnInfo.GoldCost > 0) CostText.text = learnInfo.GoldCost.ToString();
                else CostText.text = "Free";

                canAfford = BattleManager.Instance.Player1.EntityStatPropSet.Gold.Value >= learnInfo.GoldCost;
                if (canAfford)
                {
                    if (rawEntitySkill is EntityPassiveSkill rawEPS)
                    {
                        current_LearnAction = () =>
                        {
                            EntityPassiveSkill EPS = (EntityPassiveSkill) rawEPS.Clone();
                            BattleManager.Instance.Player1.AddNewPassiveSkill(EPS);
                            ClientGameManager.Instance.NoticePanel.ShowTip("Successfully learn skill!", NoticePanel.TipPositionType.Center, 1f);
                            current_LearnAction = null;
                            current_LearnCallBack?.Invoke();
                        };
                    }
                    else if (rawEntitySkill is EntityActiveSkill rawEAS)
                    {
                        current_LearnAction = () =>
                        {
                            EntityActiveSkill EAS = (EntityActiveSkill) rawEAS.Clone();
                            if (BattleManager.Instance.Player1.AddNewActiveSkill(EAS))
                            {
                                if (EAS.NeedBindKey) BattleManager.Instance.Player1.BindActiveSkillToKey(EAS, EAS.SkillKeyBind, true);
                                ClientGameManager.Instance.NoticePanel.ShowTip("Successfully learn skill!", NoticePanel.TipPositionType.Center, 1f);
                            }

                            current_LearnAction = null;
                            current_LearnCallBack?.Invoke();
                        };
                    }
                }
                else
                {
                    current_LearnAction = () => { BattleManager.Instance.Player1.EntityStatPropSet.Gold.m_NotifyActionSet.OnValueNotEnoughWarning?.Invoke(); };
                }

                break;
            }
            case LearnType.Upgrade:
            {
                TitleText.text = "New Upgrade";
                KeyBindText.text = "";
                Assert.IsNotNull(learnInfo.EntityUpgrade);
                Sprite sprite = ConfigManager.GetEntitySkillIconByName(learnInfo.EntityUpgrade.UpgradeIcon.TypeName);
                Icon.sprite = sprite;
                NameText.text = learnInfo.EntityUpgrade.UpgradeName_EN;
                DescriptionText.text = learnInfo.EntityUpgrade.UpgradeDescription_EN;
                if (learnInfo.GoldCost > 0) CostText.text = learnInfo.GoldCost.ToString();
                else CostText.text = "Free";

                canAfford = BattleManager.Instance.Player1.EntityStatPropSet.Gold.Value >= learnInfo.GoldCost;
                if (canAfford)
                {
                    current_LearnAction = () =>
                    {
                        BattleManager.Instance.Player1.GetUpgraded(learnInfo.EntityUpgrade);
                        ClientGameManager.Instance.NoticePanel.ShowTip("Successfully upgrade!", NoticePanel.TipPositionType.Center, 1f);
                        current_LearnAction = null;
                        current_LearnCallBack?.Invoke();
                    };
                }
                else
                {
                    current_LearnAction = () => { BattleManager.Instance.Player1.EntityStatPropSet.Gold.m_NotifyActionSet.OnValueNotEnoughWarning?.Invoke(); };
                }

                break;
            }
        }

        switch (ControlManager.Instance.CurrentControlScheme)
        {
            case ControlManager.ControlScheme.GamePad:
            {
                LearnButtonIcon.sprite = canAfford ? LearnButtonIconNormalSprite_Controller : LearnButtonIconGrayOutSprite_Controller;
                break;
            }
            case ControlManager.ControlScheme.KeyboardMouse:
            {
                LearnButtonIcon.sprite = canAfford ? LearnButtonIconNormalSprite_Keyboard : LearnButtonIconGrayOutSprite_Keyboard;
                break;
            }
        }
    }

    private RectTransform RectTransform;
    public RectTransform ScaleContainer;
    private Vector2 InitSizeDelta = Vector2.zero;

    void Awake()
    {
        RectTransform = (RectTransform) transform;
        InitSizeDelta = RectTransform.sizeDelta;
    }

    public IEnumerator Co_SetScale(float targetScale, float duration)
    {
        float tick = 0f;
        float startScale = ScaleContainer.transform.localScale.x;
        while (tick < duration)
        {
            tick += Time.deltaTime;
            float currentScale = Mathf.Lerp(startScale, targetScale, tick / duration);
            Vector2 scaleSizeDelta = InitSizeDelta * currentScale;
            RectTransform.sizeDelta = scaleSizeDelta;
            ScaleContainer.transform.localScale = Vector3.one * currentScale;
            yield return null;
        }
    }

    public IEnumerator Co_Remove()
    {
        Anim.SetTrigger("Hide");
        yield return Co_SetScale(0f, 0.2f);
        PoolRecycle();
    }

    private bool isFirst = false;

    public bool IsFirst
    {
        get { return isFirst; }
        set { isFirst = value; }
    }

    private bool isSelected = false;

    public bool IsSelected
    {
        get { return isSelected; }
        set
        {
            isSelected = value;
            QESwitching.SetActive(value && ClientGameManager.Instance.LearnSkillUpgradePanel.PageCount > 1);
            ESCText.gameObject.SetActive(value);
        }
    }
}
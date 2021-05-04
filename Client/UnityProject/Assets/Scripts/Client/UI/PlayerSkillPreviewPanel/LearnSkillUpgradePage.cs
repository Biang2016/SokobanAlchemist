using System.Collections;
using System.Collections.Generic;
using BiangLibrary.GamePlay.UI;
using BiangLibrary.ObjectPool;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Validation;
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
    private GameObject ESCIcon;

    [SerializeField]
    private GameObject QESwitching;

    [SerializeField]
    private Image Icon;

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
    private Sprite LearnButtonIconNormalSprite;

    [SerializeField]
    private Sprite LearnButtonIconGrayOutSprite;

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
        switch (learnInfo.LearnType)
        {
            case LearnType.Skill:
            {
                EntitySkill rawEntitySkill = ConfigManager.GetRawEntitySkill(learnInfo.SkillGUID);
                Assert.IsNotNull(rawEntitySkill);
                bool specifyKeyBind = rawEntitySkill is EntityActiveSkill;

                string keyBindStr = specifyKeyBind ? PlayerControllerHelper.KeyMappingStrDict[learnInfo.KeyBind] : "";
                Sprite sprite = ConfigManager.GetEntitySkillIconByName(rawEntitySkill.SkillIcon.TypeName);
                Icon.sprite = sprite;
                DescriptionText.text = rawEntitySkill.GetSkillDescription_EN;
                NameText.text = rawEntitySkill.SkillName_EN;
                KeyBindText.text = keyBindStr;
                if (learnInfo.GoldCost > 0) CostText.text = learnInfo.GoldCost.ToString();
                else CostText.text = "Free";

                bool canAfford = BattleManager.Instance.Player1.EntityStatPropSet.Gold.Value >= learnInfo.GoldCost;
                LearnButtonIcon.sprite = canAfford ? LearnButtonIconNormalSprite : LearnButtonIconGrayOutSprite;

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
                                BattleManager.Instance.Player1.BindActiveSkillToKey(EAS, learnInfo.KeyBind, true);
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
                KeyBindText.text = "";
                Assert.IsNotNull(learnInfo.EntityUpgrade);
                Sprite sprite = ConfigManager.GetEntitySkillIconByName(learnInfo.EntityUpgrade.UpgradeIcon.TypeName);
                Icon.sprite = sprite;
                NameText.text = learnInfo.EntityUpgrade.UpgradeName_EN;
                DescriptionText.text = learnInfo.EntityUpgrade.UpgradeDescription_EN;
                if (learnInfo.GoldCost > 0) CostText.text = learnInfo.GoldCost.ToString();
                else CostText.text = "Free";

                bool canAfford = BattleManager.Instance.Player1.EntityStatPropSet.Gold.Value >= learnInfo.GoldCost;
                LearnButtonIcon.sprite = canAfford ? LearnButtonIconNormalSprite : LearnButtonIconGrayOutSprite;

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
            ESCIcon.SetActive(value);
        }
    }
}
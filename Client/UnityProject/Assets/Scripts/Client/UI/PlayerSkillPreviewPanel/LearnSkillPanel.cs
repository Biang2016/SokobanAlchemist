using System.Collections;
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
  
    public Transform EntitySkillRowContainer;
    public Button LearnButton_Passive;

    public GameObject LearnButtons_Active;
    public Text SelectKeyBindText;
    public Button LearnButton_H;
    public Button LearnButton_J;
    public Button LearnButton_K;
    public Button LearnButton_L;

    private EntitySkillRow m_EntitySkillRow;

    internal int openStackTimes = 0;
    private UnityAction current_LearnCallBack;
    private EntitySkill current_EntitySkill;

    public void Initialize(string skillGUID, UnityAction learnCallback, bool specifyKeyBind, PlayerControllerHelper.KeyBind keyBind)
    {
        Anim.SetTrigger("Jump");

        openStackTimes++;

        current_LearnCallBack = learnCallback;
        current_EntitySkill = ConfigManager.GetEntitySkill(skillGUID);

        m_EntitySkillRow?.PoolRecycle();
        m_EntitySkillRow = null;
        if (current_EntitySkill != null)
        {
            m_EntitySkillRow = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.EntitySkillRow].AllocateGameObject<EntitySkillRow>(EntitySkillRowContainer);
            m_EntitySkillRow.Initialize(current_EntitySkill, specifyKeyBind ? PlayerControllerHelper.KeyMappingStrDict[keyBind] : "");

            if (current_EntitySkill is EntityPassiveSkill EPS)
            {
                LearnButton_Passive.gameObject.SetActive(true);
                LearnButtons_Active.SetActive(false);

                LearnButton_Passive.onClick.RemoveAllListeners();
                LearnButton_Passive.onClick.AddListener(() =>
                {
                    BattleManager.Instance.Player1.AddNewPassiveSkill(EPS);
                    ClientGameManager.Instance.NoticePanel.ShowTip("Successfully learn skill!", NoticePanel.TipPositionType.RightCenter, 1f);
                    current_LearnCallBack?.Invoke();
                    CloseUIForm();
                });
            }
            else if (current_EntitySkill is EntityActiveSkill EAS)
            {
                if (specifyKeyBind)
                {
                    LearnButton_Passive.gameObject.SetActive(true);
                    LearnButtons_Active.SetActive(false);

                    LearnButton_Passive.onClick.RemoveAllListeners();
                    LearnButton_Passive.onClick.AddListener(() => { OnButtonClick(EAS, keyBind); });
                }
                else
                {
                    LearnButton_Passive.gameObject.SetActive(false);
                    LearnButtons_Active.SetActive(true);

                    LearnButton_H.onClick.RemoveAllListeners();
                    LearnButton_J.onClick.RemoveAllListeners();
                    LearnButton_K.onClick.RemoveAllListeners();
                    LearnButton_L.onClick.RemoveAllListeners();

                    LearnButton_H.onClick.AddListener(() => { OnButtonClick(EAS, PlayerControllerHelper.KeyBind.H_LeftTrigger); });
                    LearnButton_J.onClick.AddListener(() => { OnButtonClick(EAS, PlayerControllerHelper.KeyBind.J_RightTrigger); });
                    LearnButton_K.onClick.AddListener(() => { OnButtonClick(EAS, PlayerControllerHelper.KeyBind.K); });
                    LearnButton_L.onClick.AddListener(() => { OnButtonClick(EAS, PlayerControllerHelper.KeyBind.L); });
                }
            }
        }

        void OnButtonClick(EntityActiveSkill EAS, PlayerControllerHelper.KeyBind keyBind)
        {
            if (BattleManager.Instance.Player1.AddNewActiveSkill(EAS))
            {
                BattleManager.Instance.Player1.BindActiveSkillToKey(EAS, keyBind, true);
                ClientGameManager.Instance.NoticePanel.ShowTip("Successfully learn skill!", NoticePanel.TipPositionType.RightCenter, 1f);
            }

            current_LearnCallBack?.Invoke();
            CloseUIForm();
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
        LearnButton_Passive.onClick.RemoveAllListeners();
        LearnButton_H.onClick.RemoveAllListeners();
        LearnButton_J.onClick.RemoveAllListeners();
        LearnButton_K.onClick.RemoveAllListeners();
        LearnButton_L.onClick.RemoveAllListeners();
        UIManager.Instance.UI3DRoot.gameObject.SetActive(true);
        current_LearnCallBack = null;
        current_EntitySkill = null;

        if (openStackTimes > 0)
        {
            UIManager.Instance.ShowUIForms<LearnSkillPanel>();
        }
    }
}
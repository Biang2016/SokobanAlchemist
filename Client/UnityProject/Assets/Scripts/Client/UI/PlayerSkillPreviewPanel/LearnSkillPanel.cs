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

    public Transform EntitySkillRowContainer;
    public Button LearnButton_Passive;

    public GameObject LearnButtons_Active;
    public Text SelectKeyBindText;
    public Button LearnButton_H;
    public Button LearnButton_J;
    public Button LearnButton_K;
    public Button LearnButton_L;

    private EntitySkillRow m_EntitySkillRow;

    public void Initialize(string skillGUID, UnityAction learnCallback, bool specifyKeyBind, PlayerControllerHelper.KeyBind keyBind)
    {
        m_EntitySkillRow?.PoolRecycle();
        m_EntitySkillRow = null;
        EntitySkill rawEntitySkill = ConfigManager.GetRawEntitySkill(skillGUID);
        if (rawEntitySkill != null)
        {
            m_EntitySkillRow = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.EntitySkillRow].AllocateGameObject<EntitySkillRow>(EntitySkillRowContainer);
            m_EntitySkillRow.Initialize(rawEntitySkill, specifyKeyBind ? PlayerControllerHelper.KeyMappingStrDict[keyBind] : "");

            if (rawEntitySkill is EntityPassiveSkill rawEPS)
            {
                LearnButton_Passive.gameObject.SetActive(true);
                LearnButtons_Active.SetActive(false);

                LearnButton_Passive.onClick.RemoveAllListeners();
                LearnButton_Passive.onClick.AddListener(() =>
                {
                    BattleManager.Instance.Player1.AddNewPassiveSkill((EntityPassiveSkill) rawEPS.Clone());
                    ClientGameManager.Instance.NoticePanel.ShowTip("Successfully learn skill!", NoticePanel.TipPositionType.RightCenter, 1f);
                    learnCallback?.Invoke();
                    CloseUIForm();
                });
            }
            else if (rawEntitySkill is EntityActiveSkill rawEAS)
            {
                if (specifyKeyBind)
                {
                    LearnButton_Passive.gameObject.SetActive(true);
                    LearnButtons_Active.SetActive(false);

                    LearnButton_Passive.onClick.RemoveAllListeners();
                    LearnButton_Passive.onClick.AddListener(() => { OnButtonClick(rawEAS, keyBind); });
                }
                else
                {
                    LearnButton_Passive.gameObject.SetActive(false);
                    LearnButtons_Active.SetActive(true);

                    LearnButton_H.onClick.RemoveAllListeners();
                    LearnButton_J.onClick.RemoveAllListeners();
                    LearnButton_K.onClick.RemoveAllListeners();
                    LearnButton_L.onClick.RemoveAllListeners();

                    LearnButton_H.onClick.AddListener(() => { OnButtonClick(rawEAS, PlayerControllerHelper.KeyBind.H_LeftTrigger); });
                    LearnButton_J.onClick.AddListener(() => { OnButtonClick(rawEAS, PlayerControllerHelper.KeyBind.J_RightTrigger); });
                    LearnButton_K.onClick.AddListener(() => { OnButtonClick(rawEAS, PlayerControllerHelper.KeyBind.K); });
                    LearnButton_L.onClick.AddListener(() => { OnButtonClick(rawEAS, PlayerControllerHelper.KeyBind.L); });
                }
            }
        }

        void OnButtonClick(EntityActiveSkill rawEAS, PlayerControllerHelper.KeyBind keyBind)
        {
            EntityActiveSkill eas = (EntityActiveSkill) rawEAS.Clone();
            if (BattleManager.Instance.Player1.AddNewActiveSkill(eas))
            {
                BattleManager.Instance.Player1.BindActiveSkillToKey(eas, keyBind, true);
                ClientGameManager.Instance.NoticePanel.ShowTip("Successfully learn skill!", NoticePanel.TipPositionType.RightCenter, 1f);
            }

            learnCallback?.Invoke();
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
        base.Hide();
        LearnButton_Passive.onClick.RemoveAllListeners();
        LearnButton_H.onClick.RemoveAllListeners();
        LearnButton_J.onClick.RemoveAllListeners();
        LearnButton_K.onClick.RemoveAllListeners();
        LearnButton_L.onClick.RemoveAllListeners();
        UIManager.Instance.UI3DRoot.gameObject.SetActive(true);
    }
}
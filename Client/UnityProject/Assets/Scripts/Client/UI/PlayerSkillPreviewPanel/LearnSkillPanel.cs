using System.Collections;
using BiangLibrary.GamePlay.UI;
using UnityEngine;
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

    public void Initialize(string skillGUID)
    {
        m_EntitySkillRow?.PoolRecycle();
        m_EntitySkillRow = null;
        EntitySkill rawEntitySkill = ConfigManager.GetRawEntitySkill(skillGUID);
        if (rawEntitySkill != null)
        {
            m_EntitySkillRow = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.EntitySkillRow].AllocateGameObject<EntitySkillRow>(EntitySkillRowContainer);
            m_EntitySkillRow.Initialize(rawEntitySkill, "");

            if (rawEntitySkill is EntityPassiveSkill rawEPS)
            {
                LearnButton_Passive.gameObject.SetActive(true);
                LearnButtons_Active.SetActive(false);

                LearnButton_Passive.onClick.RemoveAllListeners();
                LearnButton_Passive.onClick.AddListener(() =>
                {
                    BattleManager.Instance.Player1.AddNewPassiveSkill((EntityPassiveSkill) rawEPS.Clone());
                    ClientGameManager.Instance.NoticePanel.ShowTip("Successfully learn skill!", NoticePanel.TipPositionType.RightCenter, 1f);
                    CloseUIForm();
                });
            }
            else if (rawEntitySkill is EntityActiveSkill rawEAS)
            {
                LearnButton_Passive.gameObject.SetActive(false);
                LearnButtons_Active.SetActive(true);

                LearnButton_H.onClick.RemoveAllListeners();
                LearnButton_J.onClick.RemoveAllListeners();
                LearnButton_K.onClick.RemoveAllListeners();
                LearnButton_L.onClick.RemoveAllListeners();

                LearnButton_H.onClick.AddListener(() => { OnButtonClick(rawEAS, 2); });
                LearnButton_J.onClick.AddListener(() => { OnButtonClick(rawEAS, 3); });
                LearnButton_K.onClick.AddListener(() => { OnButtonClick(rawEAS, 4); });
                LearnButton_L.onClick.AddListener(() => { OnButtonClick(rawEAS, 5); });
            }
        }

        void OnButtonClick(EntityActiveSkill rawEAS, int keyBind)
        {
            BattleManager.Instance.Player1.AddNewActiveSkill((EntityActiveSkill) rawEAS.Clone(), keyBind, false);
            ClientGameManager.Instance.NoticePanel.ShowTip("Successfully learn skill!", NoticePanel.TipPositionType.RightCenter, 1f);
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
        UIManager.Instance.UI3DRoot.gameObject.SetActive(true);
    }
}
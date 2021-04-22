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
        EntitySkill entitySkill = ConfigManager.GetRawEntitySkill(skillGUID);
        if (entitySkill != null)
        {
            m_EntitySkillRow = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.EntitySkillRow].AllocateGameObject<EntitySkillRow>(EntitySkillRowContainer);
            m_EntitySkillRow.Initialize(entitySkill, "");

            if (entitySkill is EntityPassiveSkill)
            {
                LearnButton_Passive.gameObject.SetActive(true);
                LearnButtons_Active.SetActive(false);

                LearnButton_Passive.onClick.RemoveAllListeners();
                LearnButton_Passive.onClick.AddListener(() =>
                {
                    BattleManager.Instance.Player1.ActorSkillLearningHelper.LearnSkill(skillGUID, -1);
                    ClientGameManager.Instance.NoticePanel.ShowTip("Successfully learn skill!", NoticePanel.TipPositionType.RightCenter, 1f);
                    CloseUIForm();
                });
            }
            else if (entitySkill is EntityActiveSkill)
            {
                LearnButton_Passive.gameObject.SetActive(false);
                LearnButtons_Active.SetActive(true);

                LearnButton_H.onClick.RemoveAllListeners();
                LearnButton_J.onClick.RemoveAllListeners();
                LearnButton_K.onClick.RemoveAllListeners();
                LearnButton_L.onClick.RemoveAllListeners();

                LearnButton_H.onClick.AddListener(() =>
                {
                    BattleManager.Instance.Player1.ActorSkillLearningHelper.LearnSkill(skillGUID, 2);
                    ClientGameManager.Instance.NoticePanel.ShowTip("Successfully learn skill!", NoticePanel.TipPositionType.RightCenter, 1f);
                    CloseUIForm();
                });
                LearnButton_J.onClick.AddListener(() =>
                {
                    BattleManager.Instance.Player1.ActorSkillLearningHelper.LearnSkill(skillGUID, 3);
                    ClientGameManager.Instance.NoticePanel.ShowTip("Successfully learn skill!", NoticePanel.TipPositionType.RightCenter, 1f);
                    CloseUIForm();
                });
                LearnButton_K.onClick.AddListener(() =>
                {
                    BattleManager.Instance.Player1.ActorSkillLearningHelper.LearnSkill(skillGUID, 4);
                    ClientGameManager.Instance.NoticePanel.ShowTip("Successfully learn skill!", NoticePanel.TipPositionType.RightCenter, 1f);
                    CloseUIForm();
                });
                LearnButton_L.onClick.AddListener(() =>
                {
                    BattleManager.Instance.Player1.ActorSkillLearningHelper.LearnSkill(skillGUID, 5);
                    ClientGameManager.Instance.NoticePanel.ShowTip("Successfully learn skill!", NoticePanel.TipPositionType.RightCenter, 1f);
                    CloseUIForm();
                });
            }
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
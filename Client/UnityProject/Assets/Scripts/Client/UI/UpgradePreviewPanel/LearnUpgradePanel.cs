using System.Collections;
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
    public Transform EntityUpgradeRowContainer;
    public Button LearnButton;

    private EntityUpgradeRow m_EntityUpgradeRow;

    internal int openStackTimes = 0;
    private UnityAction current_LearnCallBack;
    private EntityUpgrade current_EntityUpgrade;

    public void Initialize(EntityUpgrade entityUpgrade, UnityAction learnCallback)
    {
        Anim.SetTrigger("Jump");

        openStackTimes++;

        current_EntityUpgrade = entityUpgrade.Clone();
        current_LearnCallBack = learnCallback;

        m_EntityUpgradeRow?.PoolRecycle();
        m_EntityUpgradeRow = null;

        m_EntityUpgradeRow = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.EntityUpgradeRow].AllocateGameObject<EntityUpgradeRow>(EntityUpgradeRowContainer);
        m_EntityUpgradeRow.Initialize(current_EntityUpgrade);

        LearnButton.onClick.RemoveAllListeners();
        LearnButton.onClick.AddListener(() =>
        {
            BattleManager.Instance.Player1.GetUpgraded(current_EntityUpgrade);
            ClientGameManager.Instance.NoticePanel.ShowTip("Successfully upgrade!", NoticePanel.TipPositionType.RightCenter, 1f);
            current_LearnCallBack?.Invoke();
            CloseUIForm();
        });
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
            LearnButton.onClick.RemoveAllListeners();
            UIManager.Instance.UI3DRoot.gameObject.SetActive(true);
            current_LearnCallBack = null;
            current_EntityUpgrade = null;
        }

        if (openStackTimes > 0)
        {
            UIManager.Instance.ShowUIForms<LearnUpgradePanel>();
        }
    }
}
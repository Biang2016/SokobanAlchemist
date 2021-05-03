using System.Collections;
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
    public Transform TransportWorldRowContainer;
    public Button TransportButton;

    private TransportWorldRow m_TransportWorldRow;

    internal int openStackTimes = 0;
    private UnityAction current_LearnCallBack;
    private WorldData current_RawWorldData;

    public void Initialize(WorldData rawWorldData, int goldCost, UnityAction learnCallback)
    {
        Anim.SetTrigger("Jump");

        openStackTimes++;

        current_RawWorldData = rawWorldData;
        current_LearnCallBack = learnCallback;

        m_TransportWorldRow?.PoolRecycle();
        m_TransportWorldRow = null;

        m_TransportWorldRow = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.TransportWorldRow].AllocateGameObject<TransportWorldRow>(TransportWorldRowContainer);
        m_TransportWorldRow.Initialize(current_RawWorldData, goldCost);

        bool canAfford = BattleManager.Instance.Player1.EntityStatPropSet.Gold.Value >= goldCost;
        TransportButton.interactable = canAfford;
        if (canAfford)
        {
            TransportButton.onClick.RemoveAllListeners();
            TransportButton.onClick.AddListener(() =>
            {
                current_LearnCallBack?.Invoke();
                CloseUIForm();
            });
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
            TransportButton.onClick.RemoveAllListeners();
            UIManager.Instance.UI3DRoot.gameObject.SetActive(true);
            current_LearnCallBack = null;
            current_RawWorldData = null;
        }

        if (openStackTimes > 0)
        {
            UIManager.Instance.ShowUIForms<TransportWorldPanel>();
        }
    }
}
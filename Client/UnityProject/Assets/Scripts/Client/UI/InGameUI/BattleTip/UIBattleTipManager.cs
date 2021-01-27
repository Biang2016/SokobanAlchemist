using System.Collections;
using System.Collections.Generic;
using BiangLibrary.GamePlay.UI;
using BiangLibrary.Messenger;
using BiangLibrary.Singleton;
using UnityEngine;

public class UIBattleTipManager : TSingletonBaseManager<UIBattleTipManager>
{
    private Messenger Messenger => ClientGameManager.Instance.BattleMessenger;
    public List<UIBattleTip> UIBattleTipList = new List<UIBattleTip>();

    public bool EnableUIBattleTip = true;

    public override void Awake()
    {
        base.Awake();
        RegisterEvent();
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        if (ControlManager.Instance.Battle_ToggleBattleTip.Up)
        {
            EnableUIBattleTip = !EnableUIBattleTip;
        }
    }

    public override void ShutDown()
    {
        base.ShutDown();
        foreach (UIBattleTip uiBattleTip in UIBattleTipList)
        {
            uiBattleTip.PoolRecycle();
        }

        UIBattleTipList.Clear();
        UnRegisterEvent();
    }

    private void RegisterEvent()
    {
        Messenger.AddListener<NumeralUIBattleTipData>((uint) ENUM_BattleEvent.Battle_ActorNumeralTip, HandleNumeralTip);
        Messenger.AddListener<CommonUIBattleTipData>((uint) ENUM_BattleEvent.Battle_ActorCommonTip, HandleCommonTip);
    }

    private void UnRegisterEvent()
    {
        Messenger.RemoveListener<NumeralUIBattleTipData>((uint) ENUM_BattleEvent.Battle_ActorNumeralTip, HandleNumeralTip);
        Messenger.RemoveListener<CommonUIBattleTipData>((uint) ENUM_BattleEvent.Battle_ActorCommonTip, HandleCommonTip);
    }

    private void HandleNumeralTip(NumeralUIBattleTipData data)
    {
        if (!EnableUIBattleTip) return;
        if (data.MainNum == 0 && data.SubNum == 0) return;
        UIBattleTipInfo info = new UIBattleTipInfo(
            0,
            data.BattleTipType,
            data.Receiver,
            data.MainNum,
            data.SubNum,
            0.2f,
            data.SubNumType,
            "",
            data.ReceiverPosition + Vector3.up * 1.5f,
            Vector2.zero,
            Vector2.one * 1f,
            0.5f);
        CreateTip(info);
    }

    private void HandleCommonTip(CommonUIBattleTipData data)
    {
        if (!EnableUIBattleTip) return;
        UIBattleTipInfo info = new UIBattleTipInfo(
            0,
            data.BattleTipType,
            data.Receiver,
            0,
            0,
            0.2f,
            0,
            "",
            data.ReceiverPosition + Vector3.up * 1.5f,
            Vector2.zero,
            Vector2.one * 1f,
            0.5f);
        CreateTip(info);
    }

    private void CreateTip(UIBattleTipInfo info)
    {
        int maxSortingOrder = 0;
        foreach (UIBattleTip uiBattleTip in UIBattleTipList)
        {
            if (uiBattleTip.SortingOrder > maxSortingOrder)
            {
                maxSortingOrder = uiBattleTip.SortingOrder;
            }
        }

        BattleTipPrefabType btType = BattleTipPrefabType.UIBattleTip_PlayerGetDamaged;
        switch (info.BattleTipType)
        {
            case BattleTipType.Damage:
            {
                switch (info.ReceiverCamp)
                {
                    case Camp.Player:
                    {
                        btType = info.DiffHP < 5 ? BattleTipPrefabType.UIBattleTip_PlayerGetDamaged : BattleTipPrefabType.UIBattleTip_PlayerGetGreatDamaged;
                        break;
                    }
                    case Camp.Friend:
                    {
                        btType = info.DiffHP < 5 ? BattleTipPrefabType.UIBattleTip_FriendGetDamaged : BattleTipPrefabType.UIBattleTip_FriendGetGreatDamaged;
                        break;
                    }
                    case Camp.Enemy:
                    case Camp.Neutral:
                    {
                        btType = info.DiffHP < 5 ? BattleTipPrefabType.UIBattleTip_EnemyGetDamaged : BattleTipPrefabType.UIBattleTip_EnemyGetGreatDamaged;
                        break;
                    }
                    case Camp.Box:
                    {
                        btType = BattleTipPrefabType.None;
                        break;
                    }
                }

                break;
            }
            case BattleTipType.AddHp:
            {
                switch (info.ReceiverCamp)
                {
                    case Camp.Player:
                    {
                        btType = BattleTipPrefabType.UIBattleTip_PlayerGetHealed;
                        break;
                    }
                    case Camp.Friend:
                    {
                        btType = BattleTipPrefabType.UIBattleTip_FriendGetHealed;
                        break;
                    }
                    case Camp.Enemy:
                    case Camp.Neutral:
                    {
                        btType = BattleTipPrefabType.UIBattleTip_EnemyGetHealed;
                        break;
                    }
                    case Camp.Box:
                    {
                        btType = BattleTipPrefabType.None;
                        break;
                    }
                }

                break;
            }
        }

        if (btType == BattleTipPrefabType.None) return;
        ClientGameManager.Instance.StartCoroutine(Co_ShowUIBattleTip(btType, info, maxSortingOrder));
    }

    IEnumerator Co_ShowUIBattleTip(BattleTipPrefabType btType, UIBattleTipInfo info, int maxSortingOrder)
    {
        yield return null;
        UIBattleTip tip = GameObjectPoolManager.Instance.BattleUIDict[btType].AllocateGameObject<UIBattleTip>(UIManager.Instance.UI3DRoot);
        tip.Initialize(info, maxSortingOrder + 1);
        UIBattleTipList.Add(tip);
    }
}
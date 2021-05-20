using System.Collections;
using System.Collections.Generic;
using BiangLibrary.GamePlay.UI;
using BiangLibrary.Messenger;
using BiangLibrary.Singleton;
using Sirenix.Utilities;
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
    }

    private void UnRegisterEvent()
    {
        Messenger.RemoveListener<NumeralUIBattleTipData>((uint) ENUM_BattleEvent.Battle_ActorNumeralTip, HandleNumeralTip);
    }

    private void HandleNumeralTip(NumeralUIBattleTipData data)
    {
        if (!EnableUIBattleTip) return;
        if (data.MainNum == 0 && data.ExtraInfo_After.IsNullOrWhitespace() && data.ExtraInfo_Before.IsNullOrWhitespace()) return;
        UIBattleTipInfo info = new UIBattleTipInfo(
            0,
            data.BattleTipType,
            data.Receiver,
            data.MainNum,
            data.ExtraInfo_Before,
            data.ExtraInfo_After,
            0.2f,
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
                        btType = info.DiffValue < 5 ? BattleTipPrefabType.UIBattleTip_PlayerGetDamaged : BattleTipPrefabType.UIBattleTip_PlayerGetGreatDamaged;
                        break;
                    }
                    case Camp.Friend:
                    {
                        btType = info.DiffValue < 5 ? BattleTipPrefabType.UIBattleTip_FriendGetDamaged : BattleTipPrefabType.UIBattleTip_FriendGetGreatDamaged;
                        break;
                    }
                    case Camp.Enemy:
                    case Camp.Neutral:
                    {
                        btType = info.DiffValue < 5 ? BattleTipPrefabType.UIBattleTip_EnemyGetDamaged : BattleTipPrefabType.UIBattleTip_EnemyGetGreatDamaged;
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
            case BattleTipType.Heal:
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
            case BattleTipType.MaxHealth:
            {
                btType = BattleTipPrefabType.UIBattleTip_GainMaxHealthTip;
                break;
            }
            case BattleTipType.ActionPoint:
            {
                btType = BattleTipPrefabType.UIBattleTip_GainActionPointTip;
                break;
            }
            case BattleTipType.MaxActionPoint:
            {
                btType = BattleTipPrefabType.UIBattleTip_GainMaxActionPointTip;
                break;
            }
            case BattleTipType.Gold:
            {
                btType = BattleTipPrefabType.UIBattleTip_GainGoldTip;
                break;
            }
            case BattleTipType.FireElementFragment:
            {
                btType = BattleTipPrefabType.UIBattleTip_GainFireElementFragmentTip;
                break;
            }
            case BattleTipType.IceElementFragment:
            {
                btType = BattleTipPrefabType.UIBattleTip_GainIceElementFragmentTip;
                break;
            }
            case BattleTipType.LightningElementFragment:
            {
                btType = BattleTipPrefabType.UIBattleTip_GainLightningElementFragmentTip;
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
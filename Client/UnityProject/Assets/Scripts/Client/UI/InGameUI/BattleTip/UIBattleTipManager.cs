using System.Collections.Generic;
using BiangStudio.GamePlay.UI;
using BiangStudio.Messenger;
using BiangStudio.Singleton;
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
        Messenger.AddListener<AttackData>((uint) ENUM_BattleEvent.Battle_ActorAttackTip, HandleAttackTip);
        Messenger.AddListener<uint, BattleTipType>((uint) ENUM_BattleEvent.Battle_ActorCommonTip, HandleCommonTip);
    }

    private void UnRegisterEvent()
    {
        Messenger.RemoveListener<AttackData>((uint) ENUM_BattleEvent.Battle_ActorAttackTip, HandleAttackTip);
        Messenger.RemoveListener<uint, BattleTipType>((uint) ENUM_BattleEvent.Battle_ActorCommonTip, HandleCommonTip);
    }

    private void HandleAttackTip(AttackData attackData)
    {
        if (!EnableUIBattleTip) return;
        UIBattleTipInfo info = new UIBattleTipInfo(
            0,
            attackData.BattleTipType,
            GetAttackerType(attackData.Attacker, attackData.Hitter, attackData.BattleTipType),
            attackData.DecHp,
            attackData.ElementHP,
            0.2f,
            attackData.ElementType,
            "",
            attackData.Hitter.transform.position + Vector3.up * 1.5f,
            Vector2.zero,
            Vector2.one * 1f,
            0.5f);
        CreateTip(info);
    }

    private void HandleCommonTip(uint actorGUID, BattleTipType battleTipType)
    {
        if (!EnableUIBattleTip) return;
        if ((int) battleTipType >= (int) BattleTipType.FollowDummySeparate)
        {
            return;
        }

        AttackerType attackerType = AttackerType.None;

        Actor mc_owner = BattleManager.Instance.FindActor(actorGUID);
        if (BattleManager.Instance.Player1 != null && mc_owner != null)
        {
            attackerType = GetAttackerType(mc_owner, BattleManager.Instance.Player1, battleTipType);
        }

        if (attackerType == AttackerType.NoTip)
        {
            return;
        }

        UIBattleTipInfo info = new UIBattleTipInfo(
            0,
            battleTipType,
            attackerType,
            0,
            0,
            0.2f,
            0,
            "",
            mc_owner.transform.position + Vector3.up * 1.5f,
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

        BattleTipPrefabType btType = BattleTipPrefabType.SelfAttack;

        if (info.AttackerType == AttackerType.LocalPlayer)
        {
            if (info.BattleTipType == BattleTipType.CriticalAttack)
            {
                btType = BattleTipPrefabType.SelfCriticalAttack;
            }
            else if (info.BattleTipType == BattleTipType.Attack)
            {
                btType = BattleTipPrefabType.SelfAttack;
            }
        }
        else if (info.AttackerType == AttackerType.Enemy)
        {
            if (info.BattleTipType == BattleTipType.Damage)
            {
                btType = BattleTipPrefabType.SelfDamage;
            }
        }

        UIBattleTip tip = GameObjectPoolManager.Instance.BattleUIDict[btType].AllocateGameObject<UIBattleTip>(UIManager.Instance.UI3DRoot);
        tip.Initialize(info, maxSortingOrder + 1);
        UIBattleTipList.Add(tip);
    }

    private AttackerType GetAttackerType(Actor attacker, Actor hitter, BattleTipType battleTipType)
    {
        //不走攻击类型判定
        if ((int) battleTipType > (int) BattleTipType.NoAttackSeparate)
        {
            return AttackerType.None;
        }

        if (attacker != null)
        {
            //主角
            if (attacker.IsPlayer)
            {
                if (hitter.IsPlayer)
                {
                    return AttackerType.LocalPlayerSelfDamage;
                }
                else
                {
                    return AttackerType.LocalPlayer;
                }
            }

            //同个阵营
            if (hitter != null && attacker.IsFriend(hitter))
            {
                return AttackerType.LocalPlayerSelfDamage;
            }

            //队友
            if (attacker.IsPlayerOrFriend)
            {
                return AttackerType.Team;
            }

            //敌人
            if (hitter != null && attacker.IsOpponent(hitter))
            {
                return AttackerType.Enemy;
            }
        }

        return AttackerType.None;
    }
}
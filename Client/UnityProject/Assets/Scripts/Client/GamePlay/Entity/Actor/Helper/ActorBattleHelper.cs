using System.Collections;
using BiangLibrary;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.GamePlay.UI;
using UnityEngine;
using UnityEngine.Events;

public class ActorBattleHelper : ActorMonoHelper
{
    internal Box LastAttackBox;

    [SerializeField]
    private BoxCollider BoxCollider;

    public Transform HealthBarPivot;
    public InGameHealthBar InGameHealthBar;

    public override void OnHelperRecycled()
    {
        BoxCollider.enabled = false;
        InGameHealthBar?.PoolRecycle();
        InGameHealthBar = null;
        IsDead = false;
        base.OnHelperRecycled();
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
        IsDead = false;
    }

    public void Initialize()
    {
        BoxCollider.enabled = true;
        Transform trans = UIManager.Instance.ShowUIForms<InGameUIPanel>().transform;
        InGameHealthBar = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.InGameHealthBar].AllocateGameObject<InGameHealthBar>(trans);
        InGameHealthBar.Initialize(this, 100, 30);
    }

    void FixedUpdate()
    {
        if (!Actor.IsRecycled)
        {
            Actor.EntityStatPropSet.FixedUpdate(Time.fixedDeltaTime);
        }
    }

    #region Life & Health

    public UnityAction<int> OnDamaged;

    public void ShowDamageNumFX(int damage)
    {
        if (damage == 0) return;
        ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_ActorNumeralTip, new NumeralUIBattleTipData(Actor.Camp, Actor.transform.position, damage, BattleTipType.Damage, 0, 0));
        OnDamaged?.Invoke(damage);
        FX injureFX = FXManager.Instance.PlayFX(Actor.InjureFX, Actor.transform.position);
        if (injureFX) injureFX.transform.localScale = Vector3.one * Actor.InjureFXScale;
    }

    public UnityAction<int> OnHealed;

    public void ShowHealNumFX(int addHealth)
    {
        if (addHealth == 0) return;
        ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_ActorNumeralTip, new NumeralUIBattleTipData(Actor.Camp, Actor.transform.position, addHealth, BattleTipType.Heal, 0, 0));
        OnHealed?.Invoke(addHealth);

        FX healFX = FXManager.Instance.PlayFX(Actor.HealFX, Actor.transform.position);
        if (healFX) healFX.transform.localScale = Vector3.one * Actor.HealFXScale;
    }

    #endregion

    #region Die

    public bool IsDead = false;

    public void DestroyActor(UnityAction callBack = null, bool forModuleRecycle = false)
    {
        IsDead = true;
        if (!forModuleRecycle)
        {
            foreach (EntityPassiveSkill ps in Actor.EntityPassiveSkills)
            {
                ps.OnBeforeDestroyEntity();
            }

            StartCoroutine(Co_DelayDestroyActor(callBack, forModuleRecycle));
        }
    }

    IEnumerator Co_DelayDestroyActor(UnityAction callBack, bool forModuleRecycle = false)
    {
        yield return new WaitForSeconds(0.1f);
        foreach (EntityPassiveSkill ps in Actor.EntityPassiveSkills)
        {
            ps.OnDestroyEntity();
        }

        if (Actor.IsPlayerCamp)
        {
            BattleManager.Instance.LoseGame();
        }
        else
        {
            if (LastAttackBox != null && LastAttackBox.LastTouchActor.IsNotNullAndAlive())
            {
                if (LastAttackBox.LastTouchActor.IsPlayerCamp)
                {
                    // todo 玩家击杀
                }
            }

            if (Actor.ActorFrozenHelper.FrozenBox)
            {
                Actor.ActorFrozenHelper.FrozenBox.DestroyBox(null, forModuleRecycle);
                Actor.ActorFrozenHelper.FrozenBox = null;
            }

            if (!forModuleRecycle)
            {
                FX hit = FXManager.Instance.PlayFX(Actor.DieFX, transform.position);
                if (hit) hit.transform.localScale = Vector3.one * Actor.DieFXScale;
            }

            Actor.PoolRecycle();
        }

        callBack?.Invoke();
    }

    #endregion

    #region Money

    public UnityAction<int> OnGainGold;

    public void ShowGainGoldNumFX(int gold)
    {
        if (gold == 0) return;
        ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_ActorNumeralTip, new NumeralUIBattleTipData(Actor.Camp, Actor.transform.position, gold, BattleTipType.Gold, 0, 0));
        OnGainGold?.Invoke(gold);
    }

    #endregion
}
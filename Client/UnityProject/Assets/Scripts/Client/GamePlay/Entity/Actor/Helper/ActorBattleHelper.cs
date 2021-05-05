using BiangLibrary.GamePlay.UI;
using UnityEngine;
using UnityEngine.Events;

public class ActorBattleHelper : ActorMonoHelper
{
    internal Box LastAttackBox;

    public Transform HealthBarPivot;
    public InGameHealthBar InGameHealthBar;

    public override void OnHelperRecycled()
    {
        OnDamaged = null;
        OnHealed = null;
        OnGainActionPoint = null;
        OnGainMaxActionPoint = null;
        OnGainMaxHealth = null;
        OnGainGold = null;
        OnSpendGold = null;
        OnGainFireElementFragment = null;
        OnGainIceElementFragment = null;
        OnGainLightningElementFragmentGold = null;

        InGameHealthBar?.PoolRecycle();
        InGameHealthBar = null;
        base.OnHelperRecycled();
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
    }

    public void Initialize()
    {
        Transform trans = UIManager.Instance.ShowUIForms<InGameUIPanel>().transform;
        InGameHealthBar = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.InGameHealthBar].AllocateGameObject<InGameHealthBar>(trans);
        InGameHealthBar.Initialize(this, 100, 30);
    }

    void FixedUpdate()
    {
    }

    #region Life & Health

    public UnityAction<int> OnDamaged;

    public void ShowDamageNumFX(int damage)
    {
        if (damage == 0) return;
        ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_ActorNumeralTip, new NumeralUIBattleTipData(Actor.Camp, Actor.transform.position, damage, BattleTipType.Damage, "", ""));
        OnDamaged?.Invoke(damage);
        FX injureFX = FXManager.Instance.PlayFX(Actor.InjureFX, Actor.transform.position);
    }

    public UnityAction<int> OnHealed;

    public void ShowHealNumFX(int addHealth)
    {
        if (addHealth == 0) return;
        ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_ActorNumeralTip, new NumeralUIBattleTipData(Actor.Camp, Actor.transform.position, addHealth, BattleTipType.Heal, "", ""));
        OnHealed?.Invoke(addHealth);

        FX healFX = FXManager.Instance.PlayFX(Actor.HealFX, Actor.transform.position);
    }

    #endregion

    #region 财产

    public UnityAction<int> OnGainActionPoint;

    public void ShowGainActionPointNumFX(int gain)
    {
        if (gain == 0) return;
        ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_ActorNumeralTip, new NumeralUIBattleTipData(Actor.Camp, Actor.transform.position, gain, BattleTipType.ActionPoint, "AP", ""));
        OnGainActionPoint?.Invoke(gain);
    }

    public UnityAction<int> OnGainMaxActionPoint;

    public void ShowGainMaxActionPointNumFX(int gain)
    {
        if (gain == 0) return;
        ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_ActorNumeralTip, new NumeralUIBattleTipData(Actor.Camp, Actor.transform.position, gain, BattleTipType.MaxActionPoint, "AP", " Max"));
        OnGainMaxActionPoint?.Invoke(gain);
    }

    public UnityAction<int> OnGainMaxHealth;

    public void ShowGainMaxHealthNumFX(int gain)
    {
        if (gain == 0) return;
        ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_ActorNumeralTip, new NumeralUIBattleTipData(Actor.Camp, Actor.transform.position, gain, BattleTipType.MaxHealth, "", " Max"));
        OnGainMaxHealth?.Invoke(gain);
    }

    public UnityAction<int> OnGainGold;

    public void ShowGainGoldNumFX(int gain)
    {
        if (gain == 0) return;
        ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_ActorNumeralTip, new NumeralUIBattleTipData(Actor.Camp, Actor.transform.position, gain, BattleTipType.Gold, "", ""));
        OnGainGold?.Invoke(gain);
    }

    public UnityAction<int> OnSpendGold;

    public void ShowSpendGoldNumFX(int spend)
    {
        if (spend == 0) return;
        ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_ActorNumeralTip, new NumeralUIBattleTipData(Actor.Camp, Actor.transform.position, -spend, BattleTipType.Gold, "", ""));
        OnSpendGold?.Invoke(spend);
    }

    public UnityAction<int> OnGainFireElementFragment;

    public void ShowGainFireElementFragmentNumFX(int gain)
    {
        if (gain == 0) return;
        ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_ActorNumeralTip, new NumeralUIBattleTipData(Actor.Camp, Actor.transform.position, gain, BattleTipType.FireElementFragment, "", ""));
        OnGainFireElementFragment?.Invoke(gain);
    }

    public UnityAction<int> OnGainIceElementFragment;

    public void ShowGainIceElementFragmentNumFX(int gain)
    {
        if (gain == 0) return;
        ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_ActorNumeralTip, new NumeralUIBattleTipData(Actor.Camp, Actor.transform.position, gain, BattleTipType.IceElementFragment, "", ""));
        OnGainIceElementFragment?.Invoke(gain);
    }

    public UnityAction<int> OnGainLightningElementFragmentGold;

    public void ShowGainLightningElementFragmentNumFX(int gain)
    {
        if (gain == 0) return;
        ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_ActorNumeralTip, new NumeralUIBattleTipData(Actor.Camp, Actor.transform.position, gain, BattleTipType.LightningElementFragment, "", ""));
        OnGainLightningElementFragmentGold?.Invoke(gain);
    }

    #endregion
}
using BiangStudio;
using BiangStudio.GameDataFormat.Grid;
using BiangStudio.GamePlay.UI;
using UnityEngine;
using UnityEngine.Events;

public class ActorBattleHelper : ActorMonoHelper
{
    internal Box LastAttackBox;

    [SerializeField]
    private BoxCollider BoxCollider;

    public Transform HealthBarPivot;
    public InGameHealthBar InGameHealthBar;

    private float immuneTimeAfterDamaged_Ticker = 0;

    public override void OnRecycled()
    {
        BoxCollider.enabled = false;
        InGameHealthBar?.PoolRecycle();
        InGameHealthBar = null;
        base.OnRecycled();
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
            Actor.ActorStatPropSet.FixedUpdate(Time.fixedDeltaTime);
            if (immuneTimeAfterDamaged_Ticker > 0)
            {
                immuneTimeAfterDamaged_Ticker -= Time.fixedDeltaTime;
            }
        }
    }

    #region Life & Health

    public UnityAction<Actor, int> OnDamaged;

    public void Damage(Actor attacker, int damage)
    {
        if (immuneTimeAfterDamaged_Ticker > 0) return;
        if (damage == 0) return;
        immuneTimeAfterDamaged_Ticker = Actor.ImmuneTimeAfterDamaged;
        ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_ActorNumeralTip, new NumeralUIBattleTipData(attacker, Actor, damage, BattleTipType.Damage, 0, 0));
        Actor.ActorStatPropSet.Health.Value -= damage;
        OnDamaged?.Invoke(attacker, damage);

        FX injureFX = FXManager.Instance.PlayFX(Actor.InjureFX, Actor.transform.position);
        if (injureFX) injureFX.transform.localScale = Vector3.one * Actor.InjureFXScale;
    }

    public void Damage(Actor attacker, float damage)
    {
        Damage(attacker, Mathf.FloorToInt(damage));
    }

    public UnityAction<Actor, int> OnHealed;

    public void Heal(Actor healer, int addHealth)
    {
        if (addHealth == 0) return;
        ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_ActorNumeralTip, new NumeralUIBattleTipData(healer, Actor, addHealth, BattleTipType.AddHp, 0, 0));
        Actor.ActorStatPropSet.Health.Value += addHealth;
        OnHealed?.Invoke(healer, addHealth);

        FX healFX = FXManager.Instance.PlayFX(Actor.HealFX, Actor.transform.position);
        if (healFX) healFX.transform.localScale = Vector3.one * Actor.HealFXScale;
    }

    public void Heal(Actor healer, float addHealth)
    {
        Heal(healer, Mathf.FloorToInt(addHealth));
    }

    public void AddLife(int addLife)
    {
        if (addLife == 0) return;
        //ClientGameManager.Instance.BattleMessenger.Broadcast((uint)ENUM_BattleEvent.Battle_ActorAttackTip, new AttackData(attacker, Actor, damage, BattleTipType.Damage, 0, 0));
        Actor.ActorStatPropSet.Life.Value += addLife;
        FX gainLifeFX = FXManager.Instance.PlayFX(Actor.GainLifeFX, Actor.transform.position);
        if (gainLifeFX) gainLifeFX.transform.localScale = Vector3.one * Actor.GainLifeFXScale;
    }

    public void LoseLife()
    {
        Actor.ActorStatPropSet.Life.Value--;
    }

    #endregion

    #region Die

    public void Die()
    {
        DropDieBox();

        if (Actor.IsPlayer)
        {
            BattleManager.Instance.LoseGame();
        }
        else
        {
            if (LastAttackBox != null && LastAttackBox.LastTouchActor != null)
            {
                if (LastAttackBox.LastTouchActor.IsPlayer)
                {
                    // todo 玩家击杀
                }
            }

            if (Actor.ActorFrozenHelper.FrozenBox)
            {
                Actor.ActorFrozenHelper.FrozenBox.DeleteSelf();
                Actor.ActorFrozenHelper.FrozenBox = null;
            }

            FX hit = FXManager.Instance.PlayFX(Actor.DieFX, transform.position);
            if (hit) hit.transform.localScale = Vector3.one * Actor.DieFXScale;
            Actor.PoolRecycle();
        }
    }

    private void DropDieBox()
    {
        WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByGridPosition(Actor.CurWorldGP);
        if (module)
        {
            ushort boxIndex = ConfigManager.GetBoxTypeIndex(Actor.DieDropBoxTypeName);
            if (boxIndex == 0) return;
            if (Actor.DieDropBoxProbabilityPercent.ProbabilityBool())
            {
                Box box = GameObjectPoolManager.Instance.BoxDict[boxIndex].AllocateGameObject<Box>(transform);
                string boxName = Actor.DieDropBoxTypeName;
                GridPos3D gp = Actor.CurWorldGP;
                GridPos3D localGP = module.WorldGPToLocalGP(gp);
                box.Setup(boxIndex);
                box.Initialize(localGP, module, 0, false, Box.LerpType.DropFromDeadActor);
                box.name = $"{boxName}_{gp}";
                box.DropFromDeadActor();
            }
        }
    }

    #endregion

}
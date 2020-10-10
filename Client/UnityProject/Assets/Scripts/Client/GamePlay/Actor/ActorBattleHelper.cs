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
        totalLife = 0;
        life = 0;
        maxHealth = 0;
        health = 0;
        OnHealthChanged = null;
        OnLifeChanged = null;
        BoxCollider.enabled = false;
        InGameHealthBar?.PoolRecycle();
        InGameHealthBar = null;
        base.OnRecycled();
    }

    public void Initialize(int totalLife, int maxHealth)
    {
        this.totalLife = totalLife;
        life = totalLife;
        this.maxHealth = maxHealth;
        health = maxHealth;
        BoxCollider.enabled = true;
        Transform trans = UIManager.Instance.ShowUIForms<InGameUIPanel>().transform;
        InGameHealthBar = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.InGameHealthBar].AllocateGameObject<InGameHealthBar>(trans);
        InGameHealthBar.Initialize(this, 100, 30);
    }

    void FixedUpdate()
    {
        if (immuneTimeAfterDamaged_Ticker > 0)
        {
            immuneTimeAfterDamaged_Ticker -= Time.fixedDeltaTime;
        }
    }

    public void ResetState()
    {
        Life = TotalLife;
        Health = MaxHealth;
    }

    public UnityAction<int, int> OnHealthChanged;
    private int maxHealth;

    public int MaxHealth
    {
        get { return maxHealth; }
        set
        {
            if (maxHealth != value)
            {
                maxHealth = value;
                OnHealthChanged?.Invoke(health, maxHealth);
                if (maxHealth <= 0)
                {
                    Die();
                }
            }
        }
    }

    private int health;

    public int Health
    {
        get { return health; }
        set
        {
            if (health != value)
            {
                health = value;
                OnHealthChanged?.Invoke(health, maxHealth);
                if (health <= 0)
                {
                    Die();
                }
            }
        }
    }

    public UnityAction<int, int> OnLifeChanged;
    private int totalLife;

    public int TotalLife
    {
        get { return totalLife; }
        set
        {
            if (totalLife != value)
            {
                totalLife = value;
                OnLifeChanged?.Invoke(life, totalLife);
                if (totalLife <= 0)
                {
                    DestroyActor();
                }
            }
        }
    }

    private int life;

    public int Life
    {
        get { return life; }
        set
        {
            if (life != value)
            {
                life = value;
                OnLifeChanged?.Invoke(life, totalLife);
                if (life <= 0)
                {
                    DestroyActor();
                }
            }
        }
    }

    public UnityAction<Actor, int> OnDamaged;

    public void Damage(Actor attacker, int damage)
    {
        if (immuneTimeAfterDamaged_Ticker > 0) return;
        immuneTimeAfterDamaged_Ticker = Actor.ImmuneTimeAfterDamaged;
        ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_ActorAttackTip, new AttackData(attacker, Actor, damage, BattleTipType.Damage, 0, 0));
        Health -= damage;
        OnDamaged?.Invoke(attacker, damage);

        FX kickFX = FXManager.Instance.PlayFX(Actor.InjureFX, Actor.transform.position);
        if (kickFX) kickFX.transform.localScale = Vector3.one * Actor.InjureFXScale;
    }

    public void Damage(Actor attacker, float damage)
    {
        Damage(attacker, Mathf.FloorToInt(damage));
    }

    public void AddLife(int addLife)
    {
        //ClientGameManager.Instance.BattleMessenger.Broadcast((uint)ENUM_BattleEvent.Battle_ActorAttackTip, new AttackData(attacker, Actor, damage, BattleTipType.Damage, 0, 0));
        TotalLife += addLife;
        Life += addLife;
        FX gainLifeFX = FXManager.Instance.PlayFX(Actor.GainLifeFX, Actor.transform.position);
        if (gainLifeFX) gainLifeFX.transform.localScale = Vector3.one * Actor.GainLifeFXScale;
    }

    public void Die()
    {
        Life--;
        Health = MaxHealth;
    }

    public void DestroyActor()
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
                }
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
}
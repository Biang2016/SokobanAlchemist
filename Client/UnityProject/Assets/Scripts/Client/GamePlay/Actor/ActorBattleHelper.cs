using BiangStudio.GameDataFormat.Grid;
using UnityEngine;
using UnityEngine.Events;

public class ActorBattleHelper : ActorHelper
{
    internal Box LastAttackBox;

    public override void OnRecycled()
    {
        totalLife = 0;
        life = 0;
        maxHealth = 0;
        health = 0;
        OnHealthChanged = null;
        OnLifeChanged = null;
        base.OnRecycled();
    }

    public void Initialize(int totalLife, int maxHealth)
    {
        this.totalLife = totalLife;
        life = totalLife;
        this.maxHealth = maxHealth;
        health = maxHealth;
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

    public void Damage(int damage)
    {
        Health -= damage;
    }

    public void Damage(float damage)
    {
        Damage(Mathf.FloorToInt(damage));
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
            BattleManager.Instance.ResetBattle();
        }
        else
        {
            if (LastAttackBox != null && LastAttackBox.LastTouchActor != null)
            {
                if (LastAttackBox.LastTouchActor.IsPlayer)
                {
                   
                }
            }

            ProjectileHit hit = ProjectileManager.Instance.PlayProjectileHit(Actor.DieFX, transform.position);
            if (hit) hit.transform.localScale = Vector3.one * Actor.DieFXScale;
            Actor.PoolRecycle();
        }
    }

    private void DropDieBox()
    {
        WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByGridPosition(Actor.CurGP);
        if (module)
        {
            byte boxIndex = ConfigManager.GetBoxTypeIndex(Actor.DieDropBoxTypeName);
            if (boxIndex == 0) return;
            Box box = GameObjectPoolManager.Instance.BoxDict[boxIndex].AllocateGameObject<Box>(transform);
            string boxName = Actor.DieDropBoxTypeName;
            box.BoxTypeIndex = boxIndex;
            GridPos3D gp = Actor.CurGP;
            GridPos3D localGP = gp - module.ModuleGP * WorldModule.MODULE_SIZE;
            box.Initialize(localGP, module, 0, false, false);
            box.name = $"{boxName}_{gp}";
            box.DropFromDeadActor();
        }
    }
}
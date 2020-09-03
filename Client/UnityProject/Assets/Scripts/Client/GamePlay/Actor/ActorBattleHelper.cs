using UnityEngine;
using UnityEngine.Events;

public class ActorBattleHelper : ActorHelper
{
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
                    Lose();
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
                    Lose();
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

    public void Lose()
    {
        BattleManager.Instance.ResetBattle();
    }

    void OnTriggerEnter(Collider collider)
    {
        Box box = collider.gameObject.GetComponentInParent<Box>();
        if (box != null)
        {
            if (box.State == Box.States.Flying || box.State == Box.States.BeingKicked)
            {
                if (box.LastTouchActor != Actor)
                {
                    Damage(box.FinalWeight * 10f);
                }
            }
        }
    }
}
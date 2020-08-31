using System.Collections.Generic;
using System.Linq;
using BiangStudio.ObjectPool;
using DG.Tweening;
using UnityEngine;

public class Actor : PoolObject
{
    public Rigidbody RigidBody;
    public PushTrigger PushTrigger;

    public Vector3 CurMoveAttempt;

    private List<SmoothMove> SmoothMoves = new List<SmoothMove>();

    public Camp Camp;

    public bool IsPlayer => Camp == Camp.Player;

    void Awake()
    {
        SmoothMoves = GetComponentsInChildren<SmoothMove>().ToList();
        foreach (SmoothMove sm in SmoothMoves)
        {
            sm.enabled = false;
        }

        ClientGameManager.Instance.BattleMessenger.AddListener<Actor>((uint) Enum_Events.OnPlayerLoaded, OnLoaded);
    }

    public void OnLoaded(Actor actor)
    {
        if (actor == this)
        {
            foreach (SmoothMove sm in SmoothMoves)
            {
                sm.enabled = true;
            }
        }
    }

    public void SetShown(bool shown)
    {
    }
}
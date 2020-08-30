using BiangStudio.ObjectPool;
using DG.Tweening;
using UnityEngine;

public class Actor : PoolObject
{
    public Rigidbody RigidBody;
    public PushTrigger PushTrigger;

    public Vector3 CurMoveAttempt;

    public Camp Camp;

    public bool IsPlayer => Camp == Camp.Player;

    public void SetShown(bool shown)
    {
    }
}
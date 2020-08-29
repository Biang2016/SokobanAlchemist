using BiangStudio.ObjectPool;
using UnityEngine;

public class Actor : PoolObject
{
    public Camp Camp;

    public bool IsPlayer => Camp == Camp.Player;

    public void SetShown(bool shown)
    {
    }
}
using System.Collections.Generic;
using BiangStudio.ObjectPool;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class Entity : PoolObject, ISerializationCallbackReceiver
{
    #region GUID

    [ReadOnly]
    [HideInEditorMode]
    public uint GUID;

    public int GUID_Mod_FixedFrameRate;

    private static uint guidGenerator = (uint) ConfigManager.GUID_Separator.Entity;

    protected uint GetGUID()
    {
        return guidGenerator++;
    }

    #endregion

    public override void OnRecycled()
    {
        base.OnRecycled();
        StopAllCoroutines();
    }

    public virtual void OnBeforeSerialize()
    {
    }

    public virtual void OnAfterDeserialize()
    {
    }

    protected virtual void FixedUpdate()
    {
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
    }

    #region Utils

    private IEnumerable<string> GetAllBoxTypeNames => ConfigManager.GetAllBoxTypeNames();
    private IEnumerable<string> GetAllFXTypeNames => ConfigManager.GetAllFXTypeNames();

    #endregion
}
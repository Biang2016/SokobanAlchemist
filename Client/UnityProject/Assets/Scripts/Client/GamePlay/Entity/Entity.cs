using System.Collections.Generic;
using UnityEngine;
using BiangStudio.ObjectPool;
using Sirenix.OdinInspector;

public abstract class Entity : PoolObject, ISerializationCallbackReceiver
{
    #region GUID

    [ReadOnly]
    [HideInEditorMode]
    public uint GUID;

    private static uint guidGenerator = (uint) ConfigManager.GUID_Separator.Entity;

    protected uint GetGUID()
    {
        return guidGenerator++;
    }

    #endregion

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
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class EntityBuffHelper : EntityMonoHelper
{
    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
        BuffAttributeDict.Clear();
        foreach (KeyValuePair<uint, FX> kv in BuffFXDict)
        {
            kv.Value.OnFXEnd = null;
            kv.Value.PoolRecycle();
        }

        BuffFXDict.Clear();
        foreach (KeyValuePair<int, FX> kv in AbnormalBuffFXDict)
        {
            kv.Value.OnFXEnd = null;
            kv.Value.PoolRecycle();
        }

        AbnormalBuffFXDict.Clear();
        BuffDict.Clear();
        BuffRemainTimeDict.Clear();
        BuffPassedTimeDict.Clear();
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
        foreach (BuffAttribute attribute in Enum.GetValues(typeof(BuffAttribute)))
        {
            BuffAttributeDict.Add(attribute, new List<EntityBuff>());
        }
    }

    protected Dictionary<BuffAttribute, List<EntityBuff>> BuffAttributeDict = new Dictionary<BuffAttribute, List<EntityBuff>>();
    protected Dictionary<int, FX> AbnormalBuffFXDict = new Dictionary<int, FX>();
    protected Dictionary<uint, FX> BuffFXDict = new Dictionary<uint, FX>();

    [ShowInInspector]
    protected Dictionary<uint, EntityBuff> BuffDict = new Dictionary<uint, EntityBuff>();

    protected Dictionary<uint, float> BuffRemainTimeDict = new Dictionary<uint, float>();
    protected Dictionary<uint, float> BuffPassedTimeDict = new Dictionary<uint, float>();

    public bool IsBeingRepulsed => HasBuff(BuffAttribute.Repulse);

    private bool BuffRelationshipProcess(EntityBuff newBuff)
    {
        bool canAdd = true;
        bool canAddButSetOff = false;
        List<EntityBuff> buffsNeedToRemove = new List<EntityBuff>();
        List<EntityBuff> buffsNeedToSetOff = new List<EntityBuff>();
        foreach (KeyValuePair<BuffAttribute, List<EntityBuff>> kv in BuffAttributeDict)
        {
            if (kv.Value.Count == 0) continue;
            BuffAttributeRelationship relationship = ConfigManager.BuffAttributeMatrix[(int) kv.Key, (int) newBuff.BuffAttribute];
            switch (relationship)
            {
                case BuffAttributeRelationship.Compatible:
                {
                    break;
                }
                case BuffAttributeRelationship.Mutex:
                {
                    foreach (EntityBuff oldBuff in kv.Value)
                    {
                        buffsNeedToRemove.Add(oldBuff);
                    }

                    break;
                }
                case BuffAttributeRelationship.Repel:
                {
                    canAdd = false;
                    break;
                }
                case BuffAttributeRelationship.SetOff:
                {
                    canAddButSetOff = true;
                    foreach (EntityBuff oldBuff in kv.Value)
                    {
                        buffsNeedToSetOff.Add(oldBuff);
                    }

                    break;
                }
                case BuffAttributeRelationship.MaxDominant:
                {
                    if (kv.Key == newBuff.BuffAttribute)
                    {
                        MaxDominantBuffProcess(newBuff, kv.Value);
                    }
                    else
                    {
                        Debug.LogError($"【Buff相克矩阵】{kv.Key}和{newBuff.BuffAttribute}之间的关系有误，异种BuffAttribute之间的关系不允许选用{relationship}");
                    }

                    break;
                }
            }
        }

        if (canAdd)
        {
            if (canAddButSetOff)
            {
                foreach (EntityBuff setOffBuff in buffsNeedToSetOff)
                {
                    RemoveBuff(setOffBuff.GUID);
                }

                return false;
            }
            else
            {
                foreach (EntityBuff removeBuff in buffsNeedToRemove)
                {
                    RemoveBuff(removeBuff.GUID);
                }

                return true;
            }
        }
        else
        {
            return false;
        }
    }

    protected virtual void MaxDominantBuffProcess(EntityBuff newBuff, List<EntityBuff> existedBuffList)
    {
    }

    public bool AddBuff(EntityBuff newBuff)
    {
        bool suc = BuffRelationshipProcess(newBuff);
        if (suc)
        {
            newBuff.OnAdded(Entity);
            PlayBuffFX(newBuff);
            if (newBuff.BuffAttribute != BuffAttribute.InstantEffect)
            {
                BuffDict.Add(newBuff.GUID, newBuff);
                BuffAttributeDict[newBuff.BuffAttribute].Add(newBuff);
                if (!newBuff.IsPermanent)
                {
                    BuffRemainTimeDict.Add(newBuff.GUID, newBuff.Duration);
                    BuffPassedTimeDict.Add(newBuff.GUID, 0);
                }
            }
        }

        return suc;
    }

    public void RemoveBuff(EntityBuff buff)
    {
        if (BuffDict.ContainsKey(buff.GUID))
        {
            RemoveBuff(buff.GUID);
        }
    }

    public void RemoveBuff(uint removeKey)
    {
        EntityBuff buff = BuffDict[removeKey];
        buff.OnRemoved(Entity);
        BuffAttributeDict[buff.BuffAttribute].Remove(buff);
        BuffDict.Remove(removeKey);
        BuffRemainTimeDict.Remove(removeKey);
        BuffPassedTimeDict.Remove(removeKey);
        if (BuffFXDict.ContainsKey(removeKey))
        {
            BuffFXDict[removeKey].OnFXEnd = null;
            BuffFXDict.Remove(removeKey);
        }
    }

    public bool HasBuff(BuffAttribute buffAttribute)
    {
        if (BuffAttributeDict.TryGetValue(buffAttribute, out List<EntityBuff> buffList))
        {
            if (buffList.Count > 0) return true;
        }

        return false;
    }

    public void PlayAbnormalStatFX(int statType, string fxName, float scale)
    {
        if (string.IsNullOrEmpty(fxName)) return;
        if (fxName == "None") return;
        if (AbnormalBuffFXDict.TryGetValue(statType, out FX fx))
        {
            fx.transform.localScale = Vector3.one * scale;
            return;
        }

        FX newFX = FXManager.Instance.PlayFX(fxName, transform.position, scale);
        newFX.transform.parent = Entity.transform;
        AbnormalBuffFXDict[statType] = newFX;
        newFX.OnFXEnd = () => { AbnormalBuffFXDict.Remove(statType); };
    }

    public void RemoveAbnormalStatFX(int statType)
    {
        if (AbnormalBuffFXDict.TryGetValue(statType, out FX fx))
        {
            fx.PoolRecycle();
        }
    }

    private void PlayBuffFX(EntityBuff buff)
    {
        if (string.IsNullOrEmpty(buff.BuffFX)) return;
        if (buff.BuffFX == "None") return;
        FX fx = FXManager.Instance.PlayFX(buff.BuffFX, transform.position, buff.BuffFXScale);
        fx.transform.parent = Entity.transform;
        if (buff.BuffAttribute != BuffAttribute.InstantEffect)
        {
            BuffFXDict.Add(buff.GUID, fx);
            if (fx is LoopFX)
            {
            }
            else
            {
                fx.OnFXEnd = () =>
                {
                    BuffFXDict.Remove(buff.GUID);
                    PlayBuffFX(buff);
                };
            }
        }
    }

    List<uint> removeKeys = new List<uint>();

    public void BuffFixedUpdate()
    {
        removeKeys.Clear();
        foreach (KeyValuePair<uint, EntityBuff> kv in BuffDict)
        {
            if (BuffRemainTimeDict.ContainsKey(kv.Key))
            {
                BuffRemainTimeDict[kv.Key] -= Time.fixedDeltaTime;
                BuffPassedTimeDict[kv.Key] += Time.fixedDeltaTime;
                kv.Value.OnFixedUpdate(Entity, BuffPassedTimeDict[kv.Key], BuffRemainTimeDict[kv.Key]);
                if (BuffRemainTimeDict[kv.Key] <= 0)
                {
                    removeKeys.Add(kv.Key);
                }
            }
        }

        foreach (uint removeKey in removeKeys)
        {
            RemoveBuff(removeKey);
        }
    }
}
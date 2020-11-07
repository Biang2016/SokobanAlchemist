using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ActorBuffHelper : ActorMonoHelper
{
    private Dictionary<ActorBuffAttribute, List<ActorBuff>> BuffAttributeDict = new Dictionary<ActorBuffAttribute, List<ActorBuff>>();
    private Dictionary<ActorStat.StatType, FX> AbnormalBuffFXDict = new Dictionary<ActorStat.StatType, FX>();
    private Dictionary<uint, FX> BuffFXDict = new Dictionary<uint, FX>();

    [ShowInInspector]
    private Dictionary<uint, ActorBuff> BuffDict = new Dictionary<uint, ActorBuff>();
    private Dictionary<uint, float> BuffRemainTimeDict = new Dictionary<uint, float>();
    private Dictionary<uint, float> BuffPassedTimeDict = new Dictionary<uint, float>();

    public override void OnRecycled()
    {
        base.OnRecycled();
        BuffAttributeDict.Clear();
        foreach (KeyValuePair<uint, FX> kv in BuffFXDict)
        {
            kv.Value.OnFXEnd = null;
            kv.Value.PoolRecycle();
        }

        BuffFXDict.Clear();
        foreach (KeyValuePair<ActorStat.StatType, FX> kv in AbnormalBuffFXDict)
        {
            kv.Value.OnFXEnd = null;
            kv.Value.PoolRecycle();
        }

        AbnormalBuffFXDict.Clear();
        BuffDict.Clear();
        BuffRemainTimeDict.Clear();
        BuffPassedTimeDict.Clear();
    }

    public override void OnUsed()
    {
        base.OnUsed();
        foreach (ActorBuffAttribute attribute in Enum.GetValues(typeof(ActorBuffAttribute)))
        {
            BuffAttributeDict.Add(attribute, new List<ActorBuff>());
        }
    }

    private bool BuffRelationshipProcess(ActorBuff newBuff)
    {
        bool canAdd = true;
        bool canAddButSetOff = false;
        List<ActorBuff> buffsNeedToRemove = new List<ActorBuff>();
        List<ActorBuff> buffsNeedToSetOff = new List<ActorBuff>();
        foreach (KeyValuePair<ActorBuffAttribute, List<ActorBuff>> kv in BuffAttributeDict)
        {
            if (kv.Value.Count == 0) continue;
            ActorBuffAttributeRelationship relationship = ConfigManager.ActorBuffAttributeMatrix[(int) kv.Key, (int) newBuff.ActorBuffAttribute];
            switch (relationship)
            {
                case ActorBuffAttributeRelationship.Compatible:
                {
                    break;
                }
                case ActorBuffAttributeRelationship.Mutex:
                {
                    foreach (ActorBuff oldBuff in kv.Value)
                    {
                        buffsNeedToRemove.Add(oldBuff);
                    }

                    break;
                }
                case ActorBuffAttributeRelationship.Repel:
                {
                    canAdd = false;
                    break;
                }
                case ActorBuffAttributeRelationship.SetOff:
                {
                    canAddButSetOff = true;
                    foreach (ActorBuff oldBuff in kv.Value)
                    {
                        buffsNeedToSetOff.Add(oldBuff);
                    }

                    break;
                }
                case ActorBuffAttributeRelationship.MaxDominant:
                {
                    if (kv.Key == newBuff.ActorBuffAttribute)
                    {
                        if (newBuff is ActorBuff_ActorPropertyMultiplyModifier newBuff_multi)
                        {
                            ActorProperty.MultiplyModifier newModifier = newBuff_multi.MultiplyModifier;
                            foreach (ActorBuff oldBuff in kv.Value)
                            {
                                if (oldBuff is ActorBuff_ActorPropertyMultiplyModifier oldBuff_multi)
                                {
                                    ActorProperty.MultiplyModifier oldModifier = oldBuff_multi.MultiplyModifier;
                                    if (newBuff_multi.PropertyType == oldBuff_multi.PropertyType)
                                    {
                                        if (newModifier.CanCover(oldModifier))
                                        {
                                            oldModifier.CoverModifiersGUID.Add(newModifier.GUID);
                                        }
                                        else
                                        {
                                            newModifier.CoverModifiersGUID.Add(oldModifier.GUID);
                                        }
                                    }
                                }
                            }
                        }
                        else if (newBuff is ActorBuff_ActorPropertyPlusModifier newBuff_plus)
                        {
                            ActorProperty.PlusModifier newModifier = newBuff_plus.PlusModifier;
                            foreach (ActorBuff oldBuff in kv.Value)
                            {
                                if (oldBuff is ActorBuff_ActorPropertyPlusModifier oldBuff_multi)
                                {
                                    ActorProperty.PlusModifier oldModifier = oldBuff_multi.PlusModifier;
                                    if (newBuff_plus.PropertyType == oldBuff_multi.PropertyType)
                                    {
                                        if (newModifier.CanCover(oldModifier))
                                        {
                                            oldModifier.CoverModifiersGUID.Add(newModifier.GUID);
                                        }
                                        else
                                        {
                                            newModifier.CoverModifiersGUID.Add(oldModifier.GUID);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError($"【角色Buff相克矩阵】{kv.Key}和{newBuff.ActorBuffAttribute}之间的关系有误，异种BuffAttribute之间的关系不允许选用{relationship}");
                    }

                    break;
                }
            }
        }

        if (canAdd)
        {
            if (canAddButSetOff)
            {
                foreach (ActorBuff setOffBuff in buffsNeedToSetOff)
                {
                    RemoveBuff(setOffBuff.GUID);
                }

                return false;
            }
            else
            {
                foreach (ActorBuff removeBuff in buffsNeedToRemove)
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

    public bool AddBuff(ActorBuff newBuff)
    {
        bool suc = BuffRelationshipProcess(newBuff);
        if (suc)
        {
            newBuff.OnAdded(Actor);
            PlayBuffFX(newBuff);
            if (newBuff.ActorBuffAttribute != ActorBuffAttribute.InstantEffect)
            {
                BuffDict.Add(newBuff.GUID, newBuff);
                BuffAttributeDict[newBuff.ActorBuffAttribute].Add(newBuff);
                if (!newBuff.IsPermanent)
                {
                    BuffRemainTimeDict.Add(newBuff.GUID, newBuff.Duration);
                    BuffPassedTimeDict.Add(newBuff.GUID, 0);
                }
            }
        }

        return suc;
    }

    public void RemoveBuff(ActorBuff buff)
    {
        if (BuffDict.ContainsKey(buff.GUID))
        {
            RemoveBuff(buff.GUID);
        }
    }

    public void RemoveBuff(uint removeKey)
    {
        ActorBuff buff = BuffDict[removeKey];
        buff.OnRemoved(Actor);
        BuffAttributeDict[buff.ActorBuffAttribute].Remove(buff);
        BuffDict.Remove(removeKey);
        BuffRemainTimeDict.Remove(removeKey);
        BuffPassedTimeDict.Remove(removeKey);
        if (BuffFXDict.ContainsKey(removeKey))
        {
            BuffFXDict[removeKey].OnFXEnd = null;
            BuffFXDict.Remove(removeKey);
        }
    }

    public void PlayAbnormalStatFX(ActorStat.StatType statType, string fxName, float scale)
    {
        if (string.IsNullOrEmpty(fxName)) return;
        if (fxName == "None") return;
        if (AbnormalBuffFXDict.ContainsKey(statType)) return;
        FX newFX = FXManager.Instance.PlayFX(fxName, transform.position, scale);
        AbnormalBuffFXDict[statType] = newFX;
        newFX.OnFXEnd = () => { AbnormalBuffFXDict.Remove(statType); };
    }

    private void PlayBuffFX(ActorBuff buff)
    {
        if (string.IsNullOrEmpty(buff.BuffFX)) return;
        if (buff.BuffFX == "None") return;
        FX fx = FXManager.Instance.PlayFX(buff.BuffFX, transform.position, buff.BuffFXScale);
        if (buff.ActorBuffAttribute != ActorBuffAttribute.InstantEffect)
        {
            BuffFXDict.Add(buff.GUID, fx);
            fx.OnFXEnd = () =>
            {
                BuffFXDict.Remove(buff.GUID);
                PlayBuffFX(buff);
            };
        }
    }

    List<uint> removeKeys = new List<uint>();

    void FixedUpdate()
    {
        removeKeys.Clear();
        foreach (KeyValuePair<uint, ActorBuff> kv in BuffDict)
        {
            if (BuffRemainTimeDict.ContainsKey(kv.Key))
            {
                BuffRemainTimeDict[kv.Key] -= Time.fixedDeltaTime;
                BuffPassedTimeDict[kv.Key] += Time.fixedDeltaTime;
                kv.Value.OnFixedUpdate(Actor, BuffPassedTimeDict[kv.Key], BuffRemainTimeDict[kv.Key]);
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
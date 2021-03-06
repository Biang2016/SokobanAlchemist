using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

public class EntityBuffHelper : EntityMonoHelper
{
    public static HashSet<EntityPropertyType> BoxBuffEnums_Property = new HashSet<EntityPropertyType>();
    public static HashSet<EntityStatType> BoxBuffEnums_Stat = new HashSet<EntityStatType>();
    public static HashSet<EntityPropertyType> ActorBuffEnums_Property = new HashSet<EntityPropertyType>();
    public static HashSet<EntityStatType> ActorBuffEnums_Stat = new HashSet<EntityStatType>();

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
        foreach (KeyValuePair<EntityBuffAttribute, List<EntityBuff>> kv in EntityBuffAttributeDict)
        {
            kv.Value.Clear();
        }

        foreach (KeyValuePair<uint, List<FX>> kv in BuffFXDict)
        {
            foreach (FX fx in kv.Value)
            {
                fx.OnFXEnd = null;
                fx.PoolRecycle();
            }
        }

        BuffFXDict.Clear();
        foreach (KeyValuePair<int, List<FX>> kv in AbnormalBuffFXDict)
        {
            foreach (FX fx in kv.Value)
            {
                fx.OnFXEnd = null;
                fx.PoolRecycle();
            }
        }

        AbnormalBuffFXDict.Clear();
        BuffDict.Clear();
        BuffRemainTimeDict.Clear();
        BuffPassedTimeDict.Clear();
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
        if (EntityBuffAttributeDict.Count == 0)
        {
            foreach (EntityBuffAttribute attribute in ConfigManager.GetAllBuffAttributeTypes())
            {
                EntityBuffAttributeDict.Add(attribute, new List<EntityBuff>());
            }
        }
    }

    protected Dictionary<EntityBuffAttribute, List<EntityBuff>> EntityBuffAttributeDict = new Dictionary<EntityBuffAttribute, List<EntityBuff>>(30);
    protected Dictionary<int, List<FX>> AbnormalBuffFXDict = new Dictionary<int, List<FX>>(10);
    protected Dictionary<uint, List<FX>> BuffFXDict = new Dictionary<uint, List<FX>>(10);

    [ShowInInspector]
    protected Dictionary<uint, EntityBuff> BuffDict = new Dictionary<uint, EntityBuff>(10);

    protected Dictionary<uint, float> BuffRemainTimeDict = new Dictionary<uint, float>(10);
    protected Dictionary<uint, float> BuffPassedTimeDict = new Dictionary<uint, float>(10);

    public bool IsStun => HasBuff(EntityBuffAttribute.Stun);
    public bool IsHiding => HasBuff(EntityBuffAttribute.Hiding);
    public bool IsBeingRepulsed => HasBuff(EntityBuffAttribute.Repulse);
    public bool IsBeingGround => HasBuff(EntityBuffAttribute.Grind);

    private bool BuffRelationshipProcess(EntityBuff newBuff)
    {
        bool canAdd = true;
        bool canAddButSetOff = false;
        List<EntityBuff> buffsNeedToRemove = new List<EntityBuff>();
        List<EntityBuff> buffsNeedToSetOff = new List<EntityBuff>();
        foreach (KeyValuePair<EntityBuffAttribute, List<EntityBuff>> kv in EntityBuffAttributeDict)
        {
            if (kv.Value.Count == 0) continue;
            EntityBuffAttributeRelationship relationship = ConfigManager.EntityBuffAttributeMatrix[(int) kv.Key, (int)newBuff.EntityBuffAttribute];
            switch (relationship)
            {
                case EntityBuffAttributeRelationship.Compatible:
                {
                    break;
                }
                case EntityBuffAttributeRelationship.Disperse:
                {
                    foreach (EntityBuff oldBuff in kv.Value)
                    {
                        buffsNeedToRemove.Add(oldBuff);
                    }

                    break;
                }
                case EntityBuffAttributeRelationship.Repel:
                {
                    canAdd = false;
                    break;
                }
                case EntityBuffAttributeRelationship.SetOff:
                {
                    canAddButSetOff = true;
                    foreach (EntityBuff oldBuff in kv.Value)
                    {
                        buffsNeedToSetOff.Add(oldBuff);
                    }

                    break;
                }
                case EntityBuffAttributeRelationship.MaxDominant:
                {
                    if (kv.Key == newBuff.EntityBuffAttribute)
                    {
                        MaxDominantBuffProcess(newBuff, kv.Value);
                    }
                    else
                    {
                        Debug.LogError($"【Buff相克矩阵】{kv.Key}和{newBuff.EntityBuffAttribute}之间的关系有误，异种BuffAttribute之间的关系不允许选用{relationship}");
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

    protected void MaxDominantBuffProcess(EntityBuff newBuff, List<EntityBuff> existedBuffList)
    {
        if (newBuff is EntityBuff_EntityPropertyMultiplyModifier newBuff_multi)
        {
            Property.MultiplyModifier newModifier = newBuff_multi.MultiplyModifier;
            foreach (EntityBuff oldBuff in existedBuffList)
            {
                if (oldBuff is EntityBuff_EntityPropertyMultiplyModifier oldBuff_multi)
                {
                    Property.MultiplyModifier oldModifier = oldBuff_multi.MultiplyModifier;
                    if (newBuff_multi.EntityPropertyType == oldBuff_multi.EntityPropertyType)
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
        else if (newBuff is EntityBuff_EntityPropertyPlusModifier newBuff_plus)
        {
            Property.PlusModifier newModifier = newBuff_plus.PlusModifier;
            foreach (EntityBuff oldBuff in existedBuffList)
            {
                if (oldBuff is EntityBuff_EntityPropertyPlusModifier oldBuff_multi)
                {
                    Property.PlusModifier oldModifier = oldBuff_multi.PlusModifier;
                    if (newBuff_plus.EntityPropertyType == oldBuff_multi.EntityPropertyType)
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

    private void CalculateDefense(EntityBuff_ChangeEntityStatInstantly newBuff)
    {
        if (newBuff.Delta < 0)
        {
            switch (newBuff.EntityBuffAttribute)
            {
                case EntityBuffAttribute.CollideDamage:
                {
                    if (Entity is Actor) newBuff.Delta = Mathf.Min(0, newBuff.Delta + Entity.EntityStatPropSet.ActorCollideDamageDefense.GetModifiedValue);
                    break;
                }
                case EntityBuffAttribute.ExplodeDamage:
                {
                    newBuff.Delta = Mathf.Min(0, newBuff.Delta + Entity.EntityStatPropSet.ExplodeDamageDefense.GetModifiedValue);
                    break;
                }
                case EntityBuffAttribute.FiringDamage:
                {
                    newBuff.Delta = Mathf.Min(0, newBuff.Delta + Entity.EntityStatPropSet.FiringDamageDefense.GetModifiedValue);
                    break;
                }
            }
        }
    }

    private bool CheckBuffPropertyTypeValid(EntityBuff newBuff)
    {
        if (newBuff is EntityBuff_ChangeEntityStatInstantly buffType1)
        {
            if (Entity is Box box) return BoxBuffEnums_Stat.Contains(buffType1.EntityStatType);
            if (Entity is Actor actor) return ActorBuffEnums_Stat.Contains(buffType1.EntityStatType);
        }

        if (newBuff is EntityBuff_EntityPropertyMultiplyModifier buffType2)
        {
            if (Entity is Box box) return BoxBuffEnums_Property.Contains(buffType2.EntityPropertyType);
            if (Entity is Actor actor) return ActorBuffEnums_Property.Contains(buffType2.EntityPropertyType);
        }

        if (newBuff is EntityBuff_EntityPropertyPlusModifier buffType3)
        {
            if (Entity is Box box) return BoxBuffEnums_Property.Contains(buffType3.EntityPropertyType);
            if (Entity is Actor actor) return ActorBuffEnums_Property.Contains(buffType3.EntityPropertyType);
        }

        return true;
    }

    public bool AddBuff(EntityBuff newBuff, string extraInfo = null)
    {
        if (!Entity.IsNotNullAndAlive()) return false;
        if (newBuff is EntityBuff_ChangeEntityStatInstantly statBuff)
        {
            if (statBuff.EntityBuffAttribute.IsDamageBuff())
            {
                extraInfo = $"Damage-{statBuff.EntityBuffAttribute}";
                CalculateDefense(statBuff);
            }
        }

        bool suc = BuffRelationshipProcess(newBuff) && CheckBuffPropertyTypeValid((newBuff));
        if (Entity.name.Contains("LightningEnemy") && !suc)
        {
            int a = 0;
        }
        if (suc)
        {
            newBuff.OnAdded(Entity, extraInfo);
            PlayBuffFX(newBuff);
            if (!Entity.IsNotNullAndAlive()) return true;
            if (newBuff.Duration > 0 || newBuff.IsPermanent)
            {
                BuffDict.Add(newBuff.GUID, newBuff);
                EntityBuffAttributeDict[newBuff.EntityBuffAttribute].Add(newBuff);
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
        EntityBuffAttributeDict[buff.EntityBuffAttribute].Remove(buff);
        BuffDict.Remove(removeKey);
        BuffRemainTimeDict.Remove(removeKey);
        BuffPassedTimeDict.Remove(removeKey);
        if (BuffFXDict.TryGetValue(removeKey, out List<FX> fxs))
        {
            foreach (FX fx in fxs)
            {
                fx.OnFXEnd = null;
                fx.PoolRecycle();
            }

            fxs.Clear();
        }
    }

    public bool HasBuff(EntityBuffAttribute attribute)
    {
        if (EntityBuffAttributeDict.TryGetValue(attribute, out List<EntityBuff> buffList))
        {
            if (buffList.Count > 0) return true;
        }

        return false;
    }

    public void PlayAbnormalStatFX(int statType, string fxName, float scale)
    {
        if (string.IsNullOrEmpty(fxName)) return;
        if (fxName == "None") return;
        if (AbnormalBuffFXDict.TryGetValue(statType, out List<FX> fxs))
        {
            foreach (FX fx in fxs)
            {
                fx.transform.localScale = Vector3.one * scale;
            }

            if (fxs.Count > 0) return;
        }

        if (Entity is Box box)
        {
            foreach (GridPos3D offset in box.GetEntityOccupationGPs_Rotated())
            {
                PlayFX(transform.position + offset);
            }
        }
        else
        {
            PlayFX(transform.position);
        }

        void PlayFX(Vector3 position)
        {
            FX fx = FXManager.Instance.PlayFX(fxName, position, scale);
            fx.transform.parent = Entity.transform;
            if (!AbnormalBuffFXDict.ContainsKey(statType)) AbnormalBuffFXDict.Add(statType, new List<FX>());
            AbnormalBuffFXDict[statType].Add(fx);
            if (fx is LoopFX)
            {
                // 已经是循环FX不需要loop
                fx.OnFXEnd = () => { AbnormalBuffFXDict[statType].Remove(fx); };
            }
            else
            {
                // 不是循环FX则自动loop
                fx.OnFXEnd = () =>
                {
                    AbnormalBuffFXDict[statType].Remove(fx);
                    if (!Entity.IsRecycled) PlayFX(Entity.transform.position);
                };
            }
        }
    }

    public void RemoveAbnormalStatFX(int statType)
    {
        if (AbnormalBuffFXDict.TryGetValue(statType, out List<FX> fxs))
        {
            foreach (FX fx in fxs)
            {
                fx.OnFXEnd = null;
                fx.PoolRecycle();
            }

            fxs.Clear();
        }
    }

    private void PlayBuffFX(EntityBuff buff)
    {
        if (string.IsNullOrEmpty(buff.BuffFX)) return;
        if (buff.BuffFX == "None") return;
        FX fx = FXManager.Instance.PlayFX(buff.BuffFX, transform.position, buff.BuffFXScale);
        fx.transform.parent = Entity.transform;
        if (buff.Duration > 0 || buff.IsPermanent)
        {
            if (!BuffFXDict.ContainsKey(buff.GUID)) BuffFXDict.Add(buff.GUID, new List<FX>());
            BuffFXDict[buff.GUID].Add(fx);
            if (fx is LoopFX)
            {
                // 已经是循环FX不需要loop
            }
            else
            {
                // 不是循环FX则自动loop
                fx.OnFXEnd = () =>
                {
                    BuffFXDict[buff.GUID].Remove(fx);
                    PlayBuffFX(buff);
                };
            }
        }
    }

    List<uint> removeKeys = new List<uint>();

    public void BuffFixedUpdate(float fixedDeltaTime)
    {
        removeKeys.Clear();
        foreach (KeyValuePair<uint, EntityBuff> kv in BuffDict)
        {
            if (BuffRemainTimeDict.ContainsKey(kv.Key))
            {
                BuffRemainTimeDict[kv.Key] -= fixedDeltaTime;
                BuffPassedTimeDict[kv.Key] += fixedDeltaTime;
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

    public void Damage(int damage, EntityBuffAttribute damageAttribute)
    {
        AddBuff(new EntityBuff_ChangeEntityStatInstantly
        {
            BuffFX = "None",
            BuffFXScale = 1,
            Delta = -damage,
            Duration = 0,
            EntityBuffAttribute = damageAttribute,
            EntityStatType = EntityStatType.HealthDurability,
            IsPermanent = false,
            Percent = 0
        });
    }
}
using System.Collections.Generic;
using UnityEngine;

public class ActorBuffHelper : ActorMonoHelper
{
    private SortedDictionary<uint, FX> BuffFXDict = new SortedDictionary<uint, FX>();
    private SortedDictionary<uint, ActorBuff> BuffDict = new SortedDictionary<uint, ActorBuff>();
    private SortedDictionary<uint, float> BuffRemainTimeDict = new SortedDictionary<uint, float>();
    private SortedDictionary<uint, float> BuffPassedTimeDict = new SortedDictionary<uint, float>();

    public override void OnRecycled()
    {
        base.OnRecycled();
        BuffDict.Clear();
        BuffRemainTimeDict.Clear();
        BuffPassedTimeDict.Clear();
    }

    public void AddPermanentBuff(ActorBuff buff)
    {
        ActorBuff cloneBuff = buff.Clone();
        BuffDict.Add(cloneBuff.GUID, cloneBuff);
        if (!string.IsNullOrEmpty(cloneBuff.BuffFX)) PlayBuffFX(cloneBuff);
    }

    private void PlayBuffFX(ActorBuff buff)
    {
        FX fx = FXManager.Instance.PlayFX(buff.BuffFX, transform.position, buff.BuffFXScale);
        BuffFXDict.Add(buff.GUID, fx);
        fx.OnFXEnd = () =>
        {
            BuffFXDict.Remove(buff.GUID);
            PlayBuffFX(buff);
        };
    }

    public void AddBuff(ActorBuff buff, float duration)
    {
        ActorBuff cloneBuff = buff.Clone();
        BuffDict.Add(cloneBuff.GUID, cloneBuff);
        BuffRemainTimeDict.Add(cloneBuff.GUID, duration);
        BuffPassedTimeDict.Add(cloneBuff.GUID, 0);
        cloneBuff.OnAdded(Actor);
        if (!string.IsNullOrEmpty(cloneBuff.BuffFX)) PlayBuffFX(cloneBuff);
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
        BuffDict[removeKey].OnRemoved(Actor);
        BuffDict.Remove(removeKey);
        BuffRemainTimeDict.Remove(removeKey);
        BuffPassedTimeDict.Remove(removeKey);
        if (BuffFXDict.ContainsKey(removeKey))
        {
            BuffFXDict[removeKey].OnFXEnd = null;
            BuffFXDict.Remove(removeKey);
        }
    }

    void FixedUpdate()
    {
        List<uint> removeKeys = new List<uint>();
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

    public void AdjustFinalSpeed(float rawFinalSpeed, out float modifiedFinalSpeed)
    {
        modifiedFinalSpeed = rawFinalSpeed;
        foreach (KeyValuePair<uint, ActorBuff> kv in BuffDict)
        {
            switch (kv.Value)
            {
                case ActorBuff_ChangeMoveSpeed b:
                {
                    modifiedFinalSpeed *= (100 + b.Percent) / 100f;
                    break;
                }
            }
        }
    }
}
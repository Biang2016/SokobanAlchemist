using System.Collections.Generic;
using UnityEngine;

public class ActorBuffHelper : ActorMonoHelper
{
    private SortedDictionary<uint, GameObject> BuffFXDict = new SortedDictionary<uint, GameObject>();
    private SortedDictionary<uint, ActorBuff> BuffDict = new SortedDictionary<uint, ActorBuff>();
    private SortedDictionary<uint, float> BuffRemainTimeDict = new SortedDictionary<uint, float>();

    public override void OnRecycled()
    {
        base.OnRecycled();
        BuffDict.Clear();
        BuffRemainTimeDict.Clear();
    }

    public void AddPermanentBuff(ActorBuff buff)
    {
        ActorBuff cloneBuff = buff.Clone();
        BuffDict.Add(cloneBuff.GUID, cloneBuff);
    }

    public void AddBuff(ActorBuff buff, float duration)
    {
        ActorBuff cloneBuff = buff.Clone();
        BuffDict.Add(cloneBuff.GUID, cloneBuff);
        BuffRemainTimeDict.Add(cloneBuff.GUID, duration);
    }

    void FixedUpdate()
    {
        List<uint> removeKeys = new List<uint>();
        foreach (KeyValuePair<uint, ActorBuff> kv in BuffDict)
        {
            if (BuffRemainTimeDict.ContainsKey(kv.Key))
            {
                BuffRemainTimeDict[kv.Key] -= Time.fixedDeltaTime;
                if (BuffRemainTimeDict[kv.Key] <= 0)
                {
                    removeKeys.Add(kv.Key);
                }
            }
        }

        foreach (uint removeKey in removeKeys)
        {
            BuffDict.Remove(removeKey);
            BuffRemainTimeDict.Remove(removeKey);
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
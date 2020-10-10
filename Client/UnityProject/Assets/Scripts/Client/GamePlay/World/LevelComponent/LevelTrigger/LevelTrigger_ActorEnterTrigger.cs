using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class LevelTrigger_ActorEnterTrigger : LevelTriggerBase
{
    [LabelText("配置")]
    public Data childData = new Data {LevelTriggerType = LevelTriggerType.LevelTrigger_ActorEnterTrigger};

    public override LevelTriggerBase.Data TriggerData
    {
        get { return childData; }
        set { childData = (Data) value; }
    }

    [Serializable]
    public new class Data : LevelTriggerBase.Data
    {
        [ValueDropdown("GetAllActorNames")]
        [LabelText("角色类型")]
        public string RequiredActorType;

        private IEnumerable<string> GetAllActorNames => ConfigManager.GetAllActorNames();

        [LabelText("角色至少停留时间/s")]
        public float RequiredStayDuration;

        protected override void ChildClone(LevelComponentData newData)
        {
            base.ChildClone(newData);
            Data data = ((Data) newData);
            data.RequiredActorType = RequiredActorType;
            data.RequiredStayDuration = RequiredStayDuration;
        }
    }

    private SortedDictionary<uint, Actor> StayActorDict = new SortedDictionary<uint, Actor>();
    private SortedDictionary<uint, float> StayActorTimeDict = new SortedDictionary<uint, float>();

    public override void OnRecycled()
    {
        base.OnRecycled();
        StayActorDict.Clear();
        StayActorTimeDict.Clear();
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Player || collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Enemy)
        {
            Actor actor = collider.gameObject.GetComponentInParent<Actor>();
            if (actor != null)
            {
                if (actor.ActorType == childData.RequiredActorType)
                {
                    if (!StayActorDict.ContainsKey(actor.GUID))
                    {
                        StayActorDict.Add(actor.GUID, actor);
                        StayActorTimeDict.Add(actor.GUID, 0);
                    }
                }
            }
        }
    }

    void FixedUpdate()
    {
        foreach (KeyValuePair<uint, Actor> kv in StayActorDict)
        {
            if (StayActorTimeDict.ContainsKey(kv.Key))
            {
                StayActorTimeDict[kv.Key] += Time.fixedDeltaTime;
                if (StayActorTimeDict[kv.Key] >= childData.RequiredStayDuration)
                {
                    TriggerEvent();
                    StayActorTimeDict[kv.Key] = 0;
                }
            }
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Player || collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Enemy)
        {
            Actor actor = collider.gameObject.GetComponentInParent<Actor>();
            if (actor != null)
            {
                if (actor.ActorType == childData.RequiredActorType)
                {
                    StayActorDict.Remove(actor.GUID);
                    StayActorTimeDict.Remove(actor.GUID);
                }
            }
        }
    }
}
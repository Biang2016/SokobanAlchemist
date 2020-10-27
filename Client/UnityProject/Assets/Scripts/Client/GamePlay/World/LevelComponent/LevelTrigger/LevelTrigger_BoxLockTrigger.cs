using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class LevelTrigger_BoxLockTrigger : LevelTriggerBase
{
    [LabelText("配置")]
    public Data childData = new Data();

    public override LevelTriggerBase.Data TriggerData
    {
        get { return childData; }
        set { childData = (Data) value; }
    }

    [Serializable]
    public new class Data : LevelTriggerBase.Data
    {
        [BoxName]
        [LabelText("指定箱子类型")]
        [ValueDropdown("GetAllBoxTypeNames", DropdownTitle = "选择箱子类型")]
        public string RequireBoxTypeName = "None";

        [LabelText("箱子至少停留时间/s")]
        [FormerlySerializedAs("RequireStayDuration")]
        public float RequiredStayDuration;

        protected override void ChildClone(LevelComponentData newData)
        {
            base.ChildClone(newData);
            Data data = ((Data) newData);
            data.RequireBoxTypeName = RequireBoxTypeName;
            data.RequiredStayDuration = RequiredStayDuration;
        }
    }

    private SortedDictionary<uint, Box> StayBoxDict = new SortedDictionary<uint, Box>();
    private SortedDictionary<uint, float> StayBoxTimeDict = new SortedDictionary<uint, float>();

    public override void OnRecycled()
    {
        base.OnRecycled();
        StayBoxDict.Clear();
        StayBoxTimeDict.Clear();
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer == LayerManager.Instance.Layer_BoxIndicator)
        {
            Box box = collider.gameObject.GetComponentInParent<Box>();
            if (box != null)
            {
                if (ConfigManager.GetBoxTypeName(box.BoxTypeIndex) == childData.RequireBoxTypeName)
                {
                    if (!StayBoxDict.ContainsKey(box.GUID))
                    {
                        StayBoxDict.Add(box.GUID, box);
                        StayBoxTimeDict.Add(box.GUID, 0);
                    }
                }
            }
        }
    }

    void FixedUpdate()
    {
        bool trigger = false;
        List<uint> triggeredBoxList = new List<uint>();
        foreach (KeyValuePair<uint, Box> kv in StayBoxDict)
        {
            if (StayBoxTimeDict.ContainsKey(kv.Key))
            {
                StayBoxTimeDict[kv.Key] += Time.fixedDeltaTime;
                if (StayBoxTimeDict[kv.Key] >= childData.RequiredStayDuration)
                {
                    trigger = true;
                    if (TriggerData.KeepTriggering)
                    {
                        StayBoxTimeDict[kv.Key] = 0;
                    }
                    else
                    {
                        triggeredBoxList.Add(kv.Key);
                    }
                }
            }
        }

        foreach (uint guid in triggeredBoxList)
        {
            StayBoxDict.Remove(guid);
            StayBoxTimeDict.Remove(guid);
        }

        if (trigger) TriggerEvent();
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.layer == LayerManager.Instance.Layer_BoxIndicator)
        {
            Box box = collider.gameObject.GetComponentInParent<Box>();
            if (box != null)
            {
                if (ConfigManager.GetBoxTypeName(box.BoxTypeIndex) == childData.RequireBoxTypeName)
                {
                    StayBoxDict.Remove(box.GUID);
                    StayBoxTimeDict.Remove(box.GUID);
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using BiangStudio;
using Sirenix.OdinInspector;
using UnityEngine;

public class LevelTrigger_ActorEnterTrigger : LevelTriggerBase
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
        [ValueDropdown("GetAllActorNames")]
        [LabelText("角色类型")]
        public string RequiredActorType = "None";

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

    private List<Actor> StayActorList = new List<Actor>();
    private Dictionary<uint, float> StayActorTimeDict = new Dictionary<uint, float>();

    public override void OnRecycled()
    {
        base.OnRecycled();
        StayActorList.Clear();
        StayActorTimeDict.Clear();
    }

    void OnTriggerEnter(Collider collider)
    {
        if (!IsRecycled)
        {
            if (collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Player || collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Enemy)
            {
                Actor actor = collider.gameObject.GetComponentInParent<Actor>();
                if (actor != null)
                {
                    if (actor.ActorType == childData.RequiredActorType)
                    {
                        if (!StayActorList.Contains(actor))
                        {
                            StayActorList.Add(actor);
                            StayActorTimeDict.Add(actor.GUID, 0);
                        }
                    }
                }
            }
        }
    }

    List<Actor> triggeredActorList = new List<Actor>();

    void FixedUpdate()
    {
        if (!IsRecycled)
        {
            bool trigger = false;
            triggeredActorList.Clear();
            foreach (Actor actor in StayActorList)
            {
                if (StayActorTimeDict.ContainsKey(actor.GUID))
                {
                    StayActorTimeDict[actor.GUID] += Time.fixedDeltaTime;
                    if (StayActorTimeDict[actor.GUID] >= childData.RequiredStayDuration)
                    {
                        trigger = true;
                        if (TriggerData != null)
                        {
                            if (TriggerData.KeepTriggering)
                            {
                                StayActorTimeDict[actor.GUID] = 0;
                            }
                            else
                            {
                                triggeredActorList.Add(actor);
                            }
                        }
                    }
                }
            }

            foreach (Actor actor in triggeredActorList)
            {
                StayActorList.Remove(actor);
                StayActorTimeDict.Remove(actor.GUID);
            }

            if (trigger) TriggerEvent();
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (!IsRecycled)
        {
            if (collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Player || collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Enemy)
            {
                Actor actor = collider.gameObject.GetComponentInParent<Actor>();
                if (actor != null)
                {
                    if (actor.ActorType == childData.RequiredActorType)
                    {
                        StayActorList.Remove(actor);
                        StayActorTimeDict.Remove(actor.GUID);
                    }
                }
            }
        }
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Color color = "#E85520".HTMLColorToColor();
        Gizmos.color = color;
        transform.DrawSpecialTip(Vector3.zero, color, Color.white, "AET");
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        Gizmos.DrawSphere(transform.position + Vector3.right * 0.25f + Vector3.forward * 0.25f, 0.15f);
    }
#endif
}
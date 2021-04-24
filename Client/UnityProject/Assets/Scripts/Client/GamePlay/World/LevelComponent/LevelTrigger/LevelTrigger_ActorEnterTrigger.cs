using System;
using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

public class LevelTrigger_ActorEnterTrigger : LevelTriggerBase
{
    [BoxGroup("Trigger配置")]
    [HideLabel]
    public Data childData = new Data();

    public override LevelTriggerBase.Data TriggerData
    {
        get { return childData; }
        set { childData = (Data) value; }
    }

    [Serializable]
    public new class Data : LevelTriggerBase.Data
    {
        [LabelText("@\"角色类型\t\"+RequiredActorType")]
        [PropertyOrder(-1)]
        public TypeSelectHelper RequiredActorType = new TypeSelectHelper {TypeDefineType = TypeDefineType.Actor};

        [LabelText("角色至少停留时间/s")]
        [PropertyOrder(-1)]
        public float RequiredStayDuration;

        protected override void ChildClone(LevelComponentData newData)
        {
            base.ChildClone(newData);
            Data data = ((Data) newData);
            data.RequiredActorType = RequiredActorType.Clone();
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
            if (collider.gameObject.layer == LayerManager.Instance.Layer_ActorIndicator_Player || collider.gameObject.layer == LayerManager.Instance.Layer_ActorIndicator_Enemy)
            {
                Actor actor = collider.gameObject.GetComponentInParent<Actor>();
                if (actor.IsNotNullAndAlive())
                {
                    if (actor.ActorType.Equals(PlayerNumber.Player1.ToString())
                        || (actor.ActorType == childData.RequiredActorType.TypeName))
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
            if (collider.gameObject.layer == LayerManager.Instance.Layer_ActorIndicator_Player || collider.gameObject.layer == LayerManager.Instance.Layer_ActorIndicator_Enemy)
            {
                Actor actor = collider.gameObject.GetComponentInParent<Actor>();
                if (actor != null) // 此处不判断角色是否死亡
                {
                    if (actor.ActorType.Equals(PlayerNumber.Player1.ToString())
                        || (actor.ActorType == childData.RequiredActorType.TypeName))
                    {
                        StayActorList.Remove(actor);
                        StayActorTimeDict.Remove(actor.GUID);
                        CancelStateValue();
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
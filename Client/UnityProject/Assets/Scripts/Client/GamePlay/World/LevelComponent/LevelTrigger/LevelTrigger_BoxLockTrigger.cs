using System;
using BiangLibrary;
using Sirenix.OdinInspector;
using UnityEngine;

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
        [LabelText("@\"指定箱子类型\t\"+RequireBoxTypeName")]
        public TypeSelectHelper RequireBoxTypeName = new TypeSelectHelper {TypeDefineType = TypeDefineType.Box};

        [LabelText("箱子至少停留时间/s")]
        public float RequiredStayDuration;

        protected override void ChildClone(LevelComponentData newData)
        {
            base.ChildClone(newData);
            Data data = ((Data) newData);
            data.RequireBoxTypeName = RequireBoxTypeName.Clone();
            data.RequiredStayDuration = RequiredStayDuration;
        }
    }

    private Box StayBox;
    private float StayBoxTick = 0;

    public override void OnRecycled()
    {
        base.OnRecycled();
        StayBox = null;
        StayBoxTick = 0;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (!IsRecycled)
        {
            if (collider.gameObject.layer == LayerManager.Instance.Layer_BoxIndicator)
            {
                Box box = collider.gameObject.GetComponentInParent<Box>();
                if (box != null)
                {
                    if (ConfigManager.GetTypeName(TypeDefineType.Box, box.EntityTypeIndex) == childData.RequireBoxTypeName.TypeName)
                    {
                        StayBox = box;
                        StayBoxTick = 0;
                    }
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (!IsRecycled)
        {
            if (StayBox)
            {
                StayBoxTick += Time.fixedDeltaTime;
                if (StayBoxTick >= childData.RequiredStayDuration)
                {
                    TriggerEvent();
                    if (TriggerData != null)
                    {
                        if (TriggerData.KeepTriggering)
                        {
                            StayBoxTick = 0;
                        }
                    }
                }
            }
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (!IsRecycled)
        {
            if (collider.gameObject.layer == LayerManager.Instance.Layer_BoxIndicator)
            {
                Box box = collider.gameObject.GetComponentInParent<Box>();
                if (box != null)
                {
                    if (ConfigManager.GetTypeName(TypeDefineType.Box, box.EntityTypeIndex) == childData.RequireBoxTypeName.TypeName)
                    {
                        if (StayBox == box)
                        {
                            StayBox = null;
                            StayBoxTick = 0;
                            CancelStateValue();
                        }
                    }
                }
            }
        }
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Color color = "#0081FF".HTMLColorToColor();
        Gizmos.color = color;
        transform.DrawSpecialTip(Vector3.zero, color, Color.white, "BLT");
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        Gizmos.DrawSphere(transform.position + Vector3.right * 0.25f + Vector3.forward * 0.25f, 0.15f);
    }

#endif
}
using System;
using System.Runtime.InteropServices;
using BiangLibrary;
using Sirenix.OdinInspector;
using UnityEngine;

public class LevelTrigger_BoxLockTrigger : LevelTriggerBase
{
    public GameObject[] ColoredModels;

    [BoxGroup("Trigger配置")]
    [HideLabel]
    public Data childData = new Data();

    public enum BoxLockTriggerColor
    {
        Blue = 0,
        Purple = 1,
        Green = 2,
        Yellow = 3,
        Red = 4
    }

    public override LevelTriggerBase.Data TriggerData
    {
        get { return childData; }
        set { childData = (Data) value; }
    }

    [Serializable]
    public new class Data : LevelTriggerBase.Data
    {
        [LabelText("锁颜色")]
        public BoxLockTriggerColor BoxLockTriggerColor;

        [LabelText("@\"指定推入的箱子类型\t\"+RequireBoxTypeName")]
        [PropertyOrder(-1)]
        public TypeSelectHelper RequireBoxTypeName = new TypeSelectHelper {TypeDefineType = TypeDefineType.Box};

        [LabelText("箱子至少停留时间后触发/s")]
        [PropertyOrder(-1)]
        public float RequiredStayDuration;

        protected override void ChildClone(LevelComponentData newData)
        {
            base.ChildClone(newData);
            Data data = ((Data) newData);
            data.BoxLockTriggerColor = BoxLockTriggerColor;
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

    [Button("刷新颜色", ButtonSizes.Large)]
    [GUIColor(0, 1, 0)]
    protected override void InitializeColor()
    {
        base.InitializeColor();
        for (int i = 0; i < ColoredModels.Length; i++)
        {
            ColoredModels[i].SetActive(i == (int) childData.BoxLockTriggerColor);
        }
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
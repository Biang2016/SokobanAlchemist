using System;
using System.Collections.Generic;
using System.Linq;
using BiangStudio.CloneVariant;
using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ObjectPool;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class LevelTriggerBase : PoolObject
{
    public abstract Data TriggerData { get; set; }

    public BoxCollider Trigger;
    public Renderer Renderer;

    [ShowInInspector]
    [ReadOnly]
    private int HasTriggeredTimes = 0;

    [Serializable]
    public class Data : IClone<Data>
    {
        [ReadOnly]
        public LevelTriggerType LevelTriggerType;

        [ReadOnly]
        public GridPos3D GridPos;

        [LabelText("触发时发送事件ID")]
        public int TriggerEmitEventID;

        [LabelText("最大触发次数")]
        public int MaxTriggerTime;

        [LabelText("触发特效")]
        [ValueDropdown("GetAllFXTypeNames", DropdownTitle = "选择FX类型")]
        public string TriggerFX;

        [LabelText("触发特效尺寸")]
        public float TriggerFXScale = 1.0f;

        [LabelText("材质颜色")]
        public Color TriggerColor;

        public Data Clone()
        {
            Type type = GetType();
            Data data = (Data) Activator.CreateInstance(type);
            data.GridPos = GridPos;
            data.TriggerEmitEventID = TriggerEmitEventID;
            data.MaxTriggerTime = MaxTriggerTime;
            data.TriggerFX = TriggerFX;
            data.TriggerFXScale = TriggerFXScale;
            data.TriggerColor = TriggerColor;
            ChildClone(data);
            return data;
        }

        protected virtual void ChildClone(Data newData)
        {
        }

        #region Utils

        private IEnumerable<string> GetAllBoxTypeNames()
        {
            ConfigManager.LoadAllConfigs();
            List<string> res = ConfigManager.BoxTypeDefineDict.TypeIndexDict.Keys.ToList();
            res.Insert(0, "None");
            return res;
        }

        private IEnumerable<string> GetAllFXTypeNames()
        {
            ConfigManager.LoadAllConfigs();
            List<string> res = ConfigManager.FXTypeDefineDict.TypeIndexDict.Keys.ToList();
            res.Insert(0, "None");
            return res;
        }

        #endregion
    }

    public override void OnRecycled()
    {
        base.OnRecycled();
        HasTriggeredTimes = 0;
    }

    public void Initialize(Data data)
    {
        TriggerData = data;
        GridPos3D.ApplyGridPosToLocalTrans(data.GridPos, transform, 1);
        InitializeColor();
    }

    [Button("预览颜色")]
    private void InitializeColor()
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        Renderer.GetPropertyBlock(mpb);
        mpb.SetColor("_Color", TriggerData.TriggerColor);
        Renderer.SetPropertyBlock(mpb);
    }

    protected virtual void TriggerEvent()
    {
        HasTriggeredTimes++;
        if (HasTriggeredTimes > TriggerData.MaxTriggerTime) return;
        FXManager.Instance.PlayFX(TriggerData.TriggerFX, TriggerData.GridPos.ToVector3(), TriggerData.TriggerFXScale);
        ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_TriggerLevelEventID, TriggerData.TriggerEmitEventID);
        Debug.Log(TriggerData.TriggerEmitEventID);
    }
}

public enum LevelTriggerType
{
    LevelTrigger_BoxLockTrigger,
}
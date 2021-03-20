using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BiangLibrary;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.ObjectPool;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class LevelTriggerBase : PoolObject
{
    internal uint WorldModuleGUID;

    public abstract Data TriggerData { get; set; }

    public BoxCollider Trigger;
    public Renderer Renderer;

    [ShowInInspector]
    [ReadOnly]
    private int HasTriggeredTimes = 0;

    [Serializable]
    public class Data : LevelComponentData
    {
        [HideInInspector]
        public ushort LevelTriggerTypeIndex;

        [BoxGroup("监听事件")]
        [LabelText("收到事件后出现(空则默认出现)")]
        public string AppearLevelEventAlias;

        [BoxGroup("监听事件")]
        [LabelText("收到事件后消失(空则不会消失)")]
        public string DisappearLevelEventAlias;

        [BoxGroup("触发事件并发送")]
        [LabelText("触发时发送事件花名")]
        public string TriggerEmitEventAlias;

        [BoxGroup("触发事件并发送")]
        [LabelText("最大触发次数")]
        public int MaxTriggerTime;

        [BoxGroup("触发事件并发送")]
        [LabelText("Stay持续触发")]
        public bool KeepTriggering = false;

        [BoxGroup("触发事件并发送")]
        [LabelText("@\"触发特效\t\"+TriggerFX")]
        public FXConfig TriggerFX = new FXConfig();

        [BoxGroup("设置战场状态")]
        [InfoBox("进入设为True离开设为False")]
        [LabelText("触发时设置战场状态花名")]
        public string TriggerSetStateAlias;

        internal bool isTriggerSetStateAliasEmpty => string.IsNullOrWhiteSpace(TriggerSetStateAlias);

        [LabelText("材质颜色")]
        public Color TriggerColor;

        protected override void ChildClone(LevelComponentData newData)
        {
            base.ChildClone(newData);
            Data data = ((Data) newData);
            data.LevelTriggerTypeIndex = LevelTriggerTypeIndex;
            data.AppearLevelEventAlias = AppearLevelEventAlias;
            data.DisappearLevelEventAlias = DisappearLevelEventAlias;
            data.TriggerEmitEventAlias = TriggerEmitEventAlias;
            data.TriggerSetStateAlias = TriggerSetStateAlias;
            data.MaxTriggerTime = MaxTriggerTime;
            data.KeepTriggering = KeepTriggering;
            data.TriggerFX = TriggerFX;
            data.TriggerColor = TriggerColor;
        }
    }

    public override void OnUsed()
    {
        base.OnUsed();
        HasTriggeredTimes = 0;
    }

    public override void OnRecycled()
    {
        base.OnRecycled();
        WorldModuleGUID = 0;
        HasTriggeredTimes = 0;
        UnRegisterEvent();
        SetShown(true);
        TriggerData = null;
    }

    protected virtual void SetShown(bool shown)
    {
        Trigger.enabled = shown;
        Renderer.enabled = shown;
    }

    public void InitializeInWorldModule(Data data, uint worldModuleGUID)
    {
        WorldModuleGUID = worldModuleGUID;
        HasTriggeredTimes = 0;
        TriggerData = data;
        GridPos3D.ApplyGridPosToLocalTrans(data.LocalGP, transform, 1);
        InitializeColor();
        RegisterEvent();
    }

    private void RegisterEvent()
    {
        if (!string.IsNullOrWhiteSpace(TriggerData.DisappearLevelEventAlias))
        {
            ClientGameManager.Instance.BattleMessenger.AddListener<string>((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, OnDisappearEvent);
        }
    }

    private void UnRegisterEvent()
    {
        if (!string.IsNullOrWhiteSpace(TriggerData.DisappearLevelEventAlias))
        {
            ClientGameManager.Instance.BattleMessenger.RemoveListener<string>((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, OnDisappearEvent);
        }
    }

    private void OnDisappearEvent(string eventAlias)
    {
        if (eventAlias == TriggerData.DisappearLevelEventAlias)
        {
            PoolRecycle();
        }
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
        if (HasTriggeredTimes <= TriggerData.MaxTriggerTime)
        {
            FXManager.Instance.PlayFX(TriggerData.TriggerFX, transform.position);
            ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, TriggerData.TriggerEmitEventAlias);
        }

        if (!IsRecycled && !TriggerData.isTriggerSetStateAliasEmpty)
        {
            BattleManager.Instance.SetStateBool(WorldModuleGUID, TriggerData.TriggerSetStateAlias, true);
        }

        //Debug.Log("LevelTriggerEventAlias:" + TriggerData.TriggerEmitEventAlias);
    }

    protected virtual void CancelStateValue()
    {
        if (!IsRecycled && !TriggerData.isTriggerSetStateAliasEmpty)
        {
            BattleManager.Instance.SetStateBool(WorldModuleGUID, TriggerData.TriggerSetStateAlias, false);
        }
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            if (IsUnderWorldModuleSpecialBoxesRoot)
            {
                transform.DrawSpecialTip(Vector3.left * 0.5f, "#0AFFF1".HTMLColorToColor(), Color.cyan, "模特");
            }
        }
    }

    private bool IsUnderWorldModuleSpecialBoxesRoot = false;

    void OnTransformParentChanged()
    {
        RefreshIsUnderWorldOrModuleBoxesRoot();
    }

    internal void RefreshIsUnderWorldOrModuleBoxesRoot()
    {
        if (!Application.isPlaying)
        {
            IsUnderWorldModuleSpecialBoxesRoot = transform.HasAncestorName($"@_{WorldModuleHierarchyRootType.WorldModuleLevelTriggersRoot}");
        }
    }

#endif
}
using System;
using System.Collections.Generic;
using BiangStudio;
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
    public class Data : LevelComponentData
    {
        [BoxGroup("监听事件")]
        [LabelText("收到事件后出现(空则默认出现)")]
        public string AppearLevelEventAlias;

        [BoxGroup("监听事件")]
        [LabelText("收到事件后消失(空则不会消失)")]
        public string DisappearLevelEventAlias;

        [ReadOnly]
        [BoxGroup("触发事件并发送")]
        [LabelText("LevelTrigger类型")]
        public LevelTriggerType LevelTriggerType;

        [BoxGroup("触发事件并发送")]
        [LabelText("触发时发送事件花名")]
        public string TriggerEmitEventAlias;

        [BoxGroup("触发事件并发送")]
        [LabelText("最大触发次数")]
        public int MaxTriggerTime;

        [BoxGroup("触发事件并发送")]
        [LabelText("持续触发")]
        public bool KeepTriggering = false;

        [BoxGroup("触发事件并发送")]
        [LabelText("触发特效")]
        [ValueDropdown("GetAllFXTypeNames", DropdownTitle = "选择FX类型")]
        public string TriggerFX;

        [BoxGroup("触发事件并发送")]
        [LabelText("触发特效尺寸")]
        public float TriggerFXScale = 1.0f;

        [LabelText("材质颜色")]
        public Color TriggerColor;

        protected override void ChildClone(LevelComponentData newData)
        {
            base.ChildClone(newData);
            Data data = ((Data) newData);
            data.AppearLevelEventAlias = AppearLevelEventAlias;
            data.DisappearLevelEventAlias = DisappearLevelEventAlias;
            data.LevelTriggerType = LevelTriggerType;
            data.TriggerEmitEventAlias = TriggerEmitEventAlias;
            data.MaxTriggerTime = MaxTriggerTime;
            data.KeepTriggering = KeepTriggering;
            data.TriggerFX = TriggerFX;
            data.TriggerFXScale = TriggerFXScale;
            data.TriggerColor = TriggerColor;
        }

        #region Utils

        private IEnumerable<string> GetAllBoxTypeNames => ConfigManager.GetAllBoxTypeNames();

        private IEnumerable<string> GetAllFXTypeNames => ConfigManager.GetAllFXTypeNames();

        private IEnumerable<string> GetAllEnemyNames => ConfigManager.GetAllEnemyNames();

        #endregion
    }

    public override void OnUsed()
    {
        base.OnUsed();
        HasTriggeredTimes = 0;
    }

    public override void OnRecycled()
    {
        base.OnRecycled();
        HasTriggeredTimes = 0;
        UnRegisterEvent();
        SetShown(true);
    }

    protected virtual void SetShown(bool shown)
    {
        Trigger.enabled = shown;
        Renderer.enabled = shown;
    }

    public void InitializeInWorld(Data data)
    {
        HasTriggeredTimes = 0;
        TriggerData = data;
        GridPos3D.ApplyGridPosToLocalTrans(data.WorldGP, transform, 1);
        InitializeColor();
        RegisterEvent();
    }

    public void InitializeInWorldModule(Data data)
    {
        HasTriggeredTimes = 0;
        TriggerData = data;
        GridPos3D.ApplyGridPosToLocalTrans(data.LocalGP, transform, 1);
        InitializeColor();
        RegisterEvent();
    }

    private void RegisterEvent()
    {
        if (!string.IsNullOrWhiteSpace(TriggerData.AppearLevelEventAlias))
        {
            SetShown(false);
            ClientGameManager.Instance.BattleMessenger.AddListener<string>((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, OnAppearEvent);
        }

        if (!string.IsNullOrWhiteSpace(TriggerData.DisappearLevelEventAlias))
        {
            ClientGameManager.Instance.BattleMessenger.AddListener<string>((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, OnDisappearEvent);
        }
    }

    private void UnRegisterEvent()
    {
        if (!string.IsNullOrWhiteSpace(TriggerData.AppearLevelEventAlias))
        {
            ClientGameManager.Instance.BattleMessenger.RemoveListener<string>((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, OnAppearEvent);
        }

        if (!string.IsNullOrWhiteSpace(TriggerData.DisappearLevelEventAlias))
        {
            ClientGameManager.Instance.BattleMessenger.RemoveListener<string>((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, OnDisappearEvent);
        }
    }

    private void OnAppearEvent(string eventAlias)
    {
        if (eventAlias == TriggerData.AppearLevelEventAlias)
        {
            SetShown(true);
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
        if (HasTriggeredTimes > TriggerData.MaxTriggerTime) return;
        FXManager.Instance.PlayFX(TriggerData.TriggerFX, transform.position, TriggerData.TriggerFXScale);
        ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, TriggerData.TriggerEmitEventAlias);
        //Debug.Log("LevelTriggerEventAlias:" + TriggerData.TriggerEmitEventAlias);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            if (IsUnderWorldModuleSpecialBoxesRoot)
            {
                transform.DrawSpecialTip(Vector3.up, CommonUtils.HTMLColorToColor("#0AFFF1"), Color.cyan, "模特");
            }

            if (IsUnderWorldSpecialBoxesRoot)
            {
                transform.DrawSpecialTip(Vector3.up, CommonUtils.HTMLColorToColor("#FF8000"), Color.yellow, "世特");
            }
        }
    }

    private bool IsUnderWorldModuleSpecialBoxesRoot = false;
    private bool IsUnderWorldSpecialBoxesRoot = false;

    void OnTransformParentChanged()
    {
        RefreshIsUnderWorldOrModuleBoxesRoot();
    }

    internal void RefreshIsUnderWorldOrModuleBoxesRoot()
    {
        if (!Application.isPlaying)
        {
            IsUnderWorldSpecialBoxesRoot = transform.HasAncestorName($"@_{WorldModuleHierarchyRootType.WorldModuleLevelTriggersRoot}");
            IsUnderWorldSpecialBoxesRoot = transform.HasAncestorName($"@_{WorldHierarchyRootType.WorldLevelTriggersRoot}");
        }
    }
#endif
}

public enum LevelTriggerType
{
    LevelTrigger_BoxLockTrigger,
    LevelTrigger_ActorEnterTrigger,
}
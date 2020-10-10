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
        [ReadOnly]
        [LabelText("LevelTrigger类型")]
        public LevelTriggerType LevelTriggerType;

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

        protected override void ChildClone(LevelComponentData newData)
        {
            base.ChildClone(newData);
            Data data = ((Data) newData);
            data.LevelTriggerType = LevelTriggerType;
            data.TriggerEmitEventID = TriggerEmitEventID;
            data.MaxTriggerTime = MaxTriggerTime;
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

    public override void OnRecycled()
    {
        base.OnRecycled();
        HasTriggeredTimes = 0;
    }

    public void InitializeInWorld(Data data)
    {
        TriggerData = data;
        GridPos3D.ApplyGridPosToLocalTrans(data.WorldGP, transform, 1);
        InitializeColor();
    }

    public void InitializeInWorldModule(Data data)
    {
        TriggerData = data;
        GridPos3D.ApplyGridPosToLocalTrans(data.LocalGP, transform, 1);
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
        FXManager.Instance.PlayFX(TriggerData.TriggerFX, transform.position, TriggerData.TriggerFXScale);
        ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_TriggerLevelEventID, TriggerData.TriggerEmitEventID);
        Debug.Log("LevelTriggerEventID:" + TriggerData.TriggerEmitEventID);
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
#if UNITY_EDITOR

            if (transform.HasAncestorName($"@_{WorldModuleHierarchyRootType.WorldModuleLevelTriggersRoot}"))
            {
                transform.DrawSpecialTip(Vector3.up, CommonUtils.HTMLColorToColor("#0AFFF1"), Color.cyan, "模组特例");
            }

            if (transform.HasAncestorName($"@_{WorldHierarchyRootType.WorldLevelTriggersRoot}"))
            {
                transform.DrawSpecialTip(Vector3.up, CommonUtils.HTMLColorToColor("#FF8000"), Color.yellow, "世界特例");
            }

#endif
        }
    }
}

public enum LevelTriggerType
{
    LevelTrigger_BoxLockTrigger,
    LevelTrigger_ActorEnterTrigger,
}
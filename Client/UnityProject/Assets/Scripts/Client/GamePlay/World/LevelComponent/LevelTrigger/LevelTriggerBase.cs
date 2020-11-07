using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
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
            data.LevelTriggerTypeIndex = LevelTriggerTypeIndex;
            data.AppearLevelEventAlias = AppearLevelEventAlias;
            data.DisappearLevelEventAlias = DisappearLevelEventAlias;
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
        TriggerData = null;
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
        if (HasTriggeredTimes > TriggerData.MaxTriggerTime) return;
        FXManager.Instance.PlayFX(TriggerData.TriggerFX, transform.position, TriggerData.TriggerFXScale);
        ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, TriggerData.TriggerEmitEventAlias);
        //Debug.Log("LevelTriggerEventAlias:" + TriggerData.TriggerEmitEventAlias);
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            if (IsUnderWorldModuleSpecialBoxesRoot)
            {
                transform.DrawSpecialTip(Vector3.up, "#0AFFF1".HTMLColorToColor(), Color.cyan, "模特");
            }

            if (IsUnderWorldSpecialBoxesRoot)
            {
                transform.DrawSpecialTip(Vector3.up, "#FF8000".HTMLColorToColor(), Color.yellow, "世特");
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

    public bool RenameBoxTypeName(string srcBoxName, string targetBoxName, StringBuilder info)
    {
        bool isDirty = false;
        foreach (FieldInfo fi in TriggerData.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            foreach (Attribute a in fi.GetCustomAttributes(false))
            {
                if (a is BoxNameAttribute)
                {
                    if (fi.FieldType == typeof(string))
                    {
                        string fieldValue = (string) fi.GetValue(TriggerData);
                        if (fieldValue == srcBoxName)
                        {
                            isDirty = true;
                            info.Append($"替换{name}.TriggerData.{fi.Name} -> '{targetBoxName}'\n");
                            fi.SetValue(TriggerData, targetBoxName);
                        }
                    }
                }
                else if (a is BoxNameListAttribute)
                {
                    if (fi.FieldType == typeof(List<string>))
                    {
                        List<string> fieldValueList = (List<string>) fi.GetValue(TriggerData);
                        for (int i = 0; i < fieldValueList.Count; i++)
                        {
                            string fieldValue = fieldValueList[i];
                            if (fieldValue == srcBoxName)
                            {
                                isDirty = true;
                                info.Append($"替换于{name}.TriggerData.{fi.Name}\n");
                                fieldValueList[i] = targetBoxName;
                            }
                        }
                    }
                }
            }
        }

        return isDirty;
    }

    public bool DeleteBoxTypeName(string srcBoxName, StringBuilder info)
    {
        bool isDirty = false;
        foreach (FieldInfo fi in TriggerData.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            foreach (Attribute a in fi.GetCustomAttributes(false))
            {
                if (a is BoxNameAttribute)
                {
                    if (fi.FieldType == typeof(string))
                    {
                        string fieldValue = (string) fi.GetValue(TriggerData);
                        if (fieldValue == srcBoxName)
                        {
                            isDirty = true;
                            info.Append($"替换{name}.TriggerData.{fi.Name} -> 'None'\n");
                            fi.SetValue(TriggerData, "None");
                        }
                    }
                }
                else if (a is BoxNameListAttribute)
                {
                    if (fi.FieldType == typeof(List<string>))
                    {
                        List<string> fieldValueList = (List<string>) fi.GetValue(TriggerData);
                        for (int i = 0; i < fieldValueList.Count; i++)
                        {
                            string fieldValue = fieldValueList[i];
                            if (fieldValue == srcBoxName)
                            {
                                isDirty = true;
                                info.Append($"移除自{name}.TriggerData.{fi.Name}\n");
                                fieldValueList.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }
            }
        }

        return isDirty;
    }
#endif
}
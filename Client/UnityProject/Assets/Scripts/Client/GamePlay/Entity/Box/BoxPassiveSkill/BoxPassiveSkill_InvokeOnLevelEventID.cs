using System;
using System.Collections.Generic;
using BiangStudio.CloneVariant;
using Sirenix.OdinInspector;

[Serializable]
public abstract class BoxPassiveSkill_InvokeOnLevelEventID : BoxPassiveSkill
{
    protected override string Description => "关卡事件触发基类";

    [BoxGroup("事件监听与触发")]
    [LabelText("多个事件联合触发")]
    public bool MultiEventTrigger = false;

    [BoxGroup("事件监听与触发")]
    [ShowIf("MultiEventTrigger")]
    [LabelText("监听关卡事件花名列表(联合触发)")]
    public List<string> ListenLevelEventAliasList = new List<string>();

    [ShowInInspector]
    [HideInEditorMode]
    [LabelText("联合触发记录")]
    private List<bool> multiTriggerFlags = new List<bool>();

    [BoxGroup("事件监听与触发")]
    [HideIf("MultiEventTrigger")]
    [LabelText("监听关卡事件花名(单个触发)")]
    public string ListenLevelEventAlias;

    [BoxGroup("事件监听与触发")]
    [LabelText("最大触发次数")]
    public int MaxTriggeredTimes = 1;

    [ShowInInspector]
    [HideInEditorMode]
    [LabelText("已触发次数")]
    private int triggeredTimes = 0;

    public override void OnRegisterLevelEventID()
    {
        triggeredTimes = 0;
        ClientGameManager.Instance.BattleMessenger.AddListener<string>((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, OnEvent);
        multiTriggerFlags.Clear();
        foreach (string alias in ListenLevelEventAliasList)
        {
            multiTriggerFlags.Add(false);
        }
    }

    public override void OnUnRegisterLevelEventID()
    {
        triggeredTimes = 0;
        ClientGameManager.Instance.BattleMessenger.RemoveListener<string>((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, OnEvent);
        multiTriggerFlags.Clear();
    }

    private void OnEvent(string eventAlias)
    {
        if (MultiEventTrigger)
        {
            for (int index = 0; index < ListenLevelEventAliasList.Count; index++)
            {
                string alias = ListenLevelEventAliasList[index];
                if (eventAlias == alias && !multiTriggerFlags[index])
                {
                    multiTriggerFlags[index] = true;
                }
            }

            bool trigger = true;
            foreach (bool flag in multiTriggerFlags)
            {
                if (!flag) trigger = false;
            }

            if (trigger)
            {
                ExecuteFunction();
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(ListenLevelEventAlias))
            {
                if (ListenLevelEventAlias.Equals(eventAlias))
                {
                    ExecuteFunction();
                }
            }
        }
    }

    private void ExecuteFunction()
    {
        if (triggeredTimes < MaxTriggeredTimes)
        {
            OnEventExecute();
            triggeredTimes++;
            for (int i = 0; i < multiTriggerFlags.Count; i++)
            {
                multiTriggerFlags[i] = false;
            }
        }
    }

    protected abstract void OnEventExecute();

    protected override void ChildClone(BoxPassiveSkill newBF)
    {
        base.ChildClone(newBF);
        BoxPassiveSkill_InvokeOnLevelEventID bf = ((BoxPassiveSkill_InvokeOnLevelEventID) newBF);
        bf.MultiEventTrigger = MultiEventTrigger;
        bf.ListenLevelEventAliasList = ListenLevelEventAliasList.Clone();
        bf.ListenLevelEventAlias = ListenLevelEventAlias;
        bf.MaxTriggeredTimes = MaxTriggeredTimes;
    }

    public override void CopyDataFrom(BoxPassiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        BoxPassiveSkill_InvokeOnLevelEventID bf = ((BoxPassiveSkill_InvokeOnLevelEventID) srcData);
        MultiEventTrigger = bf.MultiEventTrigger;
        ListenLevelEventAliasList = bf.ListenLevelEventAliasList.Clone();
        ListenLevelEventAlias = bf.ListenLevelEventAlias;
        MaxTriggeredTimes = bf.MaxTriggeredTimes;
    }
}
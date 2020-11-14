using System.Collections.Generic;
using FlowCanvas;
using FlowCanvas.Nodes;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

public class ModuleAIAtoms
{
    [Name("关卡事件触发")]
    [Category("Events")]
    public class Flow_OnLevelEventTrigger : EventNode
    {
        [RequiredField]
        public BBParameter<string> LevelEventAlias = "$关卡事件花名$";

        private FlowOutput OnTriggered;

        public override string name => $"{base.name} [{LevelEventAlias.value}]";

        protected override void RegisterPorts()
        {
            OnTriggered = AddFlowOutput("OnTriggered");
            if (ClientGameManager.Instance != null)
            {
                ClientGameManager.Instance.BattleMessenger.AddListener<string>((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, OnEvent);
            }
        }

        private void OnEvent(string levelEventAlias)
        {
            if (LevelEventAlias.value == levelEventAlias)
            {
                OnTriggered.Call(new Flow());
            }
        }
    }

    [Name("发送关卡事件")]
    [Category("Events")]
    public class Flow_BroadcastLevelEvent : CallableActionNode
    {
        [RequiredField]
        public BBParameter<string> LevelEventAlias = "$关卡事件花名$";

        public override string name => $"{base.name} [{LevelEventAlias.value}]";

        public override void Invoke()
        {
            ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, LevelEventAlias.value);
        }
    }

    [Name("读取战场状态")]
    [Category("States")]
    public class Flow_GetBattleState : CallableFunctionNode<bool>
    {
        [RequiredField]
        public BBParameter<string> BattleStateAlias = "$战场状态花名$";

        public override string name => $"{base.name} [{BattleStateAlias.value}]";

        public override bool Invoke()
        {
            return BattleManager.Instance.GetStateBool(BattleStateAlias.value);
        }
    }

    [Name("读取多个战场状态")]
    [Category("States")]
    public class Flow_GetMultiBattleStateAnd : CallableFunctionNode<bool>
    {
        [RequiredField]
        public BBParameter<List<string>> BattleStateAliasList = new BBParameter<List<string>>(new List<string>());

        [Name("同时为True才返回True")]
        public BBParameter<bool> And;

        public override string name => $"{base.name} {(And.value ? "且" : "或")} 状态数:[{BattleStateAliasList.value.Count}]";

        public override bool Invoke()
        {
            if (And.value)
            {
                foreach (string stateAlias in BattleStateAliasList.value)
                {
                    bool state = BattleManager.Instance.GetStateBool(stateAlias);
                    if (!state) return false;
                }

                return true;
            }
            else
            {
                foreach (string stateAlias in BattleStateAliasList.value)
                {
                    bool state = BattleManager.Instance.GetStateBool(stateAlias);
                    if (state) return true;
                }

                return false;
            }
        }
    }

    [Name("设置战场状态")]
    [Category("States")]
    public class Flow_SetBattleState : CallableActionNode
    {
        [RequiredField]
        [Name("战场状态花名")]
        public BBParameter<string> BattleStateAlias = "$战场状态花名$";

        [RequiredField]
        [Name("战场状态")]
        public BBParameter<bool> BattleStateValue = false;

        public override string name => $"{base.name} SET [{BattleStateAlias.value}] {BattleStateValue.value}";

        public override void Invoke()
        {
            BattleManager.Instance.SetStateBool(BattleStateAlias.value, BattleStateValue.value);
        }
    }

    [Name("显示NoticeTip")]
    [Category("玩家交互")]
    public class Flow_ShowNoticeTip : CallableActionNode
    {
        [RequiredField]
        [Name("显示NoticeTip")]
        public BBParameter<string> NoticeTip = "$提示内容$";

        [RequiredField]
        [Name("显示位置")]
        public BBParameter<NoticePanel.TipPositionType> TipPositionType;

        [RequiredField]
        [Name("持续时长(负数为永久)")]
        public BBParameter<float> Duration;

        public override string name => $"{base.name} [{NoticeTip.value}]";

        public override void Invoke()
        {
            BattleManager.Instance.NoticePanel.ShowTip(NoticeTip.value, TipPositionType.value, Duration.value);
        }
    }

    [Name("关闭NoticeTip")]
    [Category("玩家交互")]
    public class Flow_HideNoticeTip : CallableActionNode
    {
        public override string name => $"{base.name}";

        public override void Invoke()
        {
            BattleManager.Instance.NoticePanel.HideTip();
        }
    }
}
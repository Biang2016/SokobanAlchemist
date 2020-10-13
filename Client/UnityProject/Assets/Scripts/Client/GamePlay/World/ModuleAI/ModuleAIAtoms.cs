using FlowCanvas;
using FlowCanvas.Nodes;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

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
                ClientGameManager.Instance.BattleMessenger.AddListener<string>((uint)ENUM_BattleEvent.Battle_TriggerLevelEventAlias, OnEvent);
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
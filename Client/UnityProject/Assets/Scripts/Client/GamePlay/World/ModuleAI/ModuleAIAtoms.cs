using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
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

        private bool registerEvent = false;

        protected override void RegisterPorts()
        {
            OnTriggered = AddFlowOutput("OnTriggered");
            if (ClientGameManager.Instance != null)
            {
                ClientGameManager.Instance.BattleMessenger.AddListener<string>((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, OnEvent);
                registerEvent = true;
            }
        }

        private void OnEvent(string levelEventAlias)
        {
            if (!graph.isRunning && registerEvent)
            {
                if (ClientGameManager.Instance != null)
                {
                    ClientGameManager.Instance.BattleMessenger.RemoveListener<string>((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, OnEvent);
                    registerEvent = false;
                }

                return;
            }

            if (LevelEventAlias.value.CheckEventAliasOrStateBool(levelEventAlias, WorldModule, null))
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
            ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, LevelEventAlias.value.FormatEventAliasOrStateBool(WorldModule, null));
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
            return BattleManager.Instance.GetStateBool(BattleStateAlias.value.FormatEventAliasOrStateBool(WorldModule, null));
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
                    bool state = BattleManager.Instance.GetStateBool(stateAlias.FormatEventAliasOrStateBool(WorldModule, null));
                    if (!state) return false;
                }

                return true;
            }
            else
            {
                foreach (string stateAlias in BattleStateAliasList.value)
                {
                    bool state = BattleManager.Instance.GetStateBool(stateAlias.FormatEventAliasOrStateBool(WorldModule, null));
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
            BattleManager.Instance.SetStateBool(WorldModule.WorldModuleData.GUID, BattleStateAlias.value.FormatEventAliasOrStateBool(WorldModule, null), BattleStateValue.value);
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
            ClientGameManager.Instance.NoticePanel.ShowTip(NoticeTip.value, TipPositionType.value, Duration.value);
        }
    }

    [Name("关闭NoticeTip")]
    [Category("玩家交互")]
    public class Flow_HideNoticeTip : CallableActionNode
    {
        public override string name => $"{base.name}";

        public override void Invoke()
        {
            ClientGameManager.Instance.NoticePanel.HideTip();
        }
    }

    [Name("切换BGM")]
    [Category("音乐")]
    public class Flow_SwitchBGM : CallableActionNode
    {
        [RequiredField]
        [Name("切换BGM")]
        public BBParameter<string> BGM_Name = "bgm/???";

        [Name("淡入时长")]
        public BBParameter<float> FadeInDuration;

        [Name("音量")]
        public BBParameter<float> Volume;

        public override string name => $"{base.name} [{BGM_Name.value}]";

        public override void Invoke()
        {
            //AudioManager.Instance.BGMFadeIn(BGM_Name.value, FadeInDuration.value, Volume.value, true);
        }
    }

    [Name("播放音效")]
    [Category("音乐")]
    public class Flow_PlaySound : CallableActionNode
    {
        [RequiredField]
        [Name("音效")]
        public BBParameter<WwiseAudioManager.CommonAudioEvent> CommonAudioEvent;

        public override string name => $"{base.name} [{CommonAudioEvent.value}]";

        public override void Invoke()
        {
            WwiseAudioManager.Instance.PlayCommonAudioSound(CommonAudioEvent.value, WorldModule.gameObject);
        }
    }

    [Name("所有箱子的火焰是否都扑灭了")]
    [Category("States")]
    public class Flow_CheckFirePutOutForEveryBox : CallableFunctionNode<bool>
    {
        public override bool Invoke()
        {
            for (int x = 0; x < WorldModule.MODULE_SIZE; x++)
            for (int y = 0; y < WorldModule.MODULE_SIZE; y++)
            for (int z = 0; z < WorldModule.MODULE_SIZE; z++)
            {
                Entity entity = WorldModule[TypeDefineType.Box, new GridPos3D(x, y, z)];
                if (entity is Box box)
                {
                    if (box.EntityStatPropSet.IsFiring) return false;
                }
            }

            return true;
        }
    }
}
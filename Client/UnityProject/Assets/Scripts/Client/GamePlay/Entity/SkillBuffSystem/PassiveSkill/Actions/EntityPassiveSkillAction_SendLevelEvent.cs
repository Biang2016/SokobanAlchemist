using System;
using Sirenix.OdinInspector;

[Serializable]
public class EntitySkillAction_SendLevelEvent : EntitySkillAction, EntitySkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "发送事件花名";

    [LabelText("触发时发送事件花名")]
    public string EmitEventAlias;

    [LabelText("触发时设置战场状态花名")]
    public string SetStateAlias;

    [LabelText("战场状态值")]
    public bool SetStateAliasValue = false;

    public void Execute()
    {
        if (!string.IsNullOrWhiteSpace(EmitEventAlias))
        {
            ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, EmitEventAlias.FormatEventAliasOrStateBool(InitWorldModuleGUID));
        }

        if (!string.IsNullOrWhiteSpace(SetStateAlias))
        {
            BattleManager.Instance.SetStateBool(InitWorldModuleGUID, SetStateAlias.FormatEventAliasOrStateBool(InitWorldModuleGUID), SetStateAliasValue);
        }
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_SendLevelEvent bf = ((EntitySkillAction_SendLevelEvent) newAction);
        bf.EmitEventAlias = EmitEventAlias;
        bf.SetStateAlias = SetStateAlias;
        bf.SetStateAliasValue = SetStateAliasValue;
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_SendLevelEvent bf = ((EntitySkillAction_SendLevelEvent) srcData);
        EmitEventAlias = bf.EmitEventAlias;
        SetStateAlias = bf.SetStateAlias;
        SetStateAliasValue = bf.SetStateAliasValue;
    }
}
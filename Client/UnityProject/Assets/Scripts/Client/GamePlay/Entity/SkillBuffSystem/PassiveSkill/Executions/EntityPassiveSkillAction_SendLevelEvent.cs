using System;
using Sirenix.OdinInspector;

[Serializable]
public class EntityPassiveSkillAction_SendLevelEvent : EntityPassiveSkillAction, EntityPassiveSkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "发送事件花名";

    [LabelText("触发时发送事件花名")]
    public string EmitEventAlias;

    [LabelText("触发时设置战场状态花名")]
    public string SetStateAlias;

    public bool SetStateAliasValue = false;

    public void Execute()
    {
        if (!string.IsNullOrWhiteSpace(EmitEventAlias))
        {
            ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, EmitEventAlias);
        }

        if (!string.IsNullOrWhiteSpace(SetStateAlias))
        {
            WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(Entity.WorldGP);
            if (module)
            {
                BattleManager.Instance.SetStateBool(module.GUID, SetStateAlias, SetStateAliasValue);
            }
        }
    }

    protected override void ChildClone(EntityPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        EntityPassiveSkillAction_SendLevelEvent bf = ((EntityPassiveSkillAction_SendLevelEvent) newAction);
        bf.EmitEventAlias = EmitEventAlias;
        bf.SetStateAlias = SetStateAlias;
        bf.SetStateAliasValue = SetStateAliasValue;
    }

    public override void CopyDataFrom(EntityPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntityPassiveSkillAction_SendLevelEvent bf = ((EntityPassiveSkillAction_SendLevelEvent) srcData);
        EmitEventAlias = bf.EmitEventAlias;
        SetStateAlias = bf.SetStateAlias;
        SetStateAliasValue = bf.SetStateAliasValue;
    }
}
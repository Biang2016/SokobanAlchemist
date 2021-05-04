using System;
using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.CloneVariant;
using BiangLibrary.GamePlay.UI;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntitySkillAction_ShowTransportWorldPanel : EntitySkillAction, EntitySkillAction.IPureAction
{
    public override void OnRecycled()
    {
        selectedWorld = null;
    }

    protected override string Description => "显示传送世界面板";

    [LabelText("世界类型概率")]
    public List<WorldNameWithProbability> WorldProbList = new List<WorldNameWithProbability>();

    private WorldNameWithProbability selectedWorld;

    public void Execute()
    {
        if (UIManager.Instance.IsUIShown<TransportWorldPanel>()) return;
        if (selectedWorld == null)
        {
            selectedWorld = CommonUtils.GetRandomFromList(WorldProbList);
            WorldProbList.Clear();
            WorldProbList.Add(selectedWorld);
           
        }
        WorldData rawWorldData = ConfigManager.GetRawWorldDataConfig(ConfigManager.GetTypeIndex(TypeDefineType.World, selectedWorld.WorldTypeName.TypeName));
        if (rawWorldData != null)
        {
            TransportWorldPanel transportWorldPanel = UIManager.Instance.ShowUIForms<TransportWorldPanel>();
            transportWorldPanel.Initialize(rawWorldData, OnTransport, selectedWorld.GoldCost);
        }
    }

    private void OnTransport()
    {
        BattleManager.Instance.Player1.EntityStatPropSet.Gold.SetValue(BattleManager.Instance.Player1.EntityStatPropSet.Gold.Value - selectedWorld.GoldCost);
        ClientGameManager.Instance.ChangeWorld(selectedWorld.WorldTypeName.TypeName, true);
        selectedWorld = null;
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_ShowTransportWorldPanel action = ((EntitySkillAction_ShowTransportWorldPanel) newAction);
        action.WorldProbList = WorldProbList.Clone<WorldNameWithProbability, WorldNameWithProbability>();
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_ShowTransportWorldPanel action = ((EntitySkillAction_ShowTransportWorldPanel) srcData);
        WorldProbList = action.WorldProbList.Clone<WorldNameWithProbability, WorldNameWithProbability>();
    }
}
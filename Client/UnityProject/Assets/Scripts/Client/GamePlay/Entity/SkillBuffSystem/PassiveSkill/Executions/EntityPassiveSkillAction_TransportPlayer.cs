using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntityPassiveSkillAction_TransportPlayer : BoxPassiveSkillAction, EntityPassiveSkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "传送玩家";

    [BoxName]
    [LabelText("传送至新世界")]
    [ValueDropdown("GetAllWorldNames", IsUniqueList = true, DropdownTitle = "选择世界", DrawDropdownForListElements = false, ExcludeExistingValuesInList = true)]
    public string TransportPlayerToWorld = "None";

    public void Execute()
    {
        if ((WorldManager.Instance.CurrentWorld is OpenWorld openWorld))
        {
            ushort worldTypeIndex = ConfigManager.GetWorldTypeIndex(TransportPlayerToWorld);
            if (worldTypeIndex != 0)
            {
                openWorld.TransportPlayerToMicroWorld(worldTypeIndex);
            }
        }
        else
        {
            Debug.LogWarning("传送功能暂时只能用于OpenWorld");
        }
    }

    protected override void ChildClone(EntityPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        EntityPassiveSkillAction_TransportPlayer action = ((EntityPassiveSkillAction_TransportPlayer) newAction);
        action.TransportPlayerToWorld = TransportPlayerToWorld;
    }

    public override void CopyDataFrom(EntityPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntityPassiveSkillAction_TransportPlayer action = ((EntityPassiveSkillAction_TransportPlayer) srcData);
        TransportPlayerToWorld = action.TransportPlayerToWorld;
    }
}
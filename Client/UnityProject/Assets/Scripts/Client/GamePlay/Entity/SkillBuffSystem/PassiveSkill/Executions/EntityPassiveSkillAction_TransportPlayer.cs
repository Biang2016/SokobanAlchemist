﻿using System;
using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;

[Serializable]
public class EntityPassiveSkillAction_TransportPlayer : BoxPassiveSkillAction, EntityPassiveSkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "传送玩家";

    [LabelText("世界类型概率")]
    public List<WorldNameWithProbability> WorldProbList = new List<WorldNameWithProbability>();

    public void Execute()
    {
        WorldNameWithProbability randomResult = CommonUtils.GetRandomFromList(WorldProbList);
        if ((WorldManager.Instance.CurrentWorld is OpenWorld openWorld))
        {
            ushort worldTypeIndex = ConfigManager.GetTypeIndex(TypeDefineType.World, randomResult.WorldTypeName.TypeName);
            if (worldTypeIndex != 0)
            {
                if (worldTypeIndex == ConfigManager.World_OpenWorldIndex)
                {
                    openWorld.ReturnToOpenWorldFormMicroWorld(false);
                }
                else
                {
                    openWorld.TransportPlayerToMicroWorld(worldTypeIndex);
                }
            }
        }
        else
        {
            ClientGameManager.Instance.SwitchWorld(randomResult.WorldTypeName.TypeName);
        }
    }

    protected override void ChildClone(EntityPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        EntityPassiveSkillAction_TransportPlayer action = ((EntityPassiveSkillAction_TransportPlayer) newAction);
        action.WorldProbList = WorldProbList.Clone<WorldNameWithProbability, WorldNameWithProbability>();
    }

    public override void CopyDataFrom(EntityPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntityPassiveSkillAction_TransportPlayer action = ((EntityPassiveSkillAction_TransportPlayer) srcData);
        WorldProbList = action.WorldProbList.Clone<WorldNameWithProbability, WorldNameWithProbability>();
    }
}
using System;
using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntityPassiveSkillAction_TransportPlayer : BoxPassiveSkillAction, EntityPassiveSkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "传送玩家";

    public enum TransportType
    {
        TransportToMicroWorld,
        TransportBackToOpenWorld,
        ResetTransportState,
    }

    [LabelText("传送类型")]
    public TransportType m_TransportType = TransportType.TransportToMicroWorld;

    [ShowIf("m_TransportType", TransportType.TransportToMicroWorld)]
    [LabelText("世界类型概率")]
    public List<WorldNameWithProbability> WorldProbList = new List<WorldNameWithProbability>();

    public void Execute()
    {
        switch (m_TransportType)
        {
            case TransportType.TransportToMicroWorld:
            {
                if (!BattleManager.Instance.Player1.IsInMicroWorld)
                {
                    if ((WorldManager.Instance.CurrentWorld is OpenWorld openWorld))
                    {
                        WorldNameWithProbability randomResult = CommonUtils.GetRandomFromList(WorldProbList);
                        ushort worldTypeIndex = ConfigManager.GetTypeIndex(TypeDefineType.World, randomResult.WorldTypeName.TypeName);
                        if (worldTypeIndex != 0)
                        {
                            openWorld.TransportPlayerToMicroWorld(worldTypeIndex);
                            BattleManager.Instance.Player1.IsInMicroWorld = true;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("传送功能暂时只能用于OpenWorld");
                    }
                }

                break;
            }
            case TransportType.TransportBackToOpenWorld:
            {
                if ((WorldManager.Instance.CurrentWorld is OpenWorld openWorld))
                {
                    if (openWorld.IsInsideMicroWorld)
                    {
                        openWorld.ReturnToOpenWorldFormMicroWorld();
                    }
                }
                else
                {
                    Debug.LogWarning("传送功能暂时只能用于OpenWorld");
                }

                break;
            }
            case TransportType.ResetTransportState:
            {
                BattleManager.Instance.Player1.IsInMicroWorld = false;
                break;
            }
        }
    }

    protected override void ChildClone(EntityPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        EntityPassiveSkillAction_TransportPlayer action = ((EntityPassiveSkillAction_TransportPlayer) newAction);
        action.m_TransportType = m_TransportType;
        action.WorldProbList = WorldProbList.Clone();
    }

    public override void CopyDataFrom(EntityPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntityPassiveSkillAction_TransportPlayer action = ((EntityPassiveSkillAction_TransportPlayer) srcData);
        m_TransportType = action.m_TransportType;
        WorldProbList = action.WorldProbList.Clone();
    }
}
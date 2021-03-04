using System;
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

    [BoxName]
    [LabelText("传送至新世界")]
    [ShowIf("m_TransportType", TransportType.TransportToMicroWorld)]
    [ValueDropdown("GetAllWorldNames", IsUniqueList = true, DropdownTitle = "选择世界", DrawDropdownForListElements = false, ExcludeExistingValuesInList = true)]
    public string TransportPlayerToWorld = "None";

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
                        ushort worldTypeIndex = ConfigManager.GetWorldTypeIndex(TransportPlayerToWorld);
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
                    openWorld.ReturnToOpenWorldFormMicroWorld();
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
        action.TransportPlayerToWorld = TransportPlayerToWorld;
    }

    public override void CopyDataFrom(EntityPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntityPassiveSkillAction_TransportPlayer action = ((EntityPassiveSkillAction_TransportPlayer) srcData);
        m_TransportType = action.m_TransportType;
        TransportPlayerToWorld = action.TransportPlayerToWorld;
    }
}
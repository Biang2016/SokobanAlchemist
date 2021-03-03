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
        if (!(WorldManager.Instance.CurrentWorld is OpenWorld openWorld))
        {
            Debug.LogWarning("传送功能暂时只能用于OpenWorld");
        }

        ushort worldTypeIndex = ConfigManager.GetWorldTypeIndex(TransportPlayerToWorld);
        if (worldTypeIndex != 0)
        {
            WorldManager.Instance.CurrentWorld.StartCoroutine(Co_TransportPlayer(worldTypeIndex));
        }
    }

    IEnumerator Co_TransportPlayer(ushort worldTypeIndex)
    {
        WorldData worldData = ConfigManager.GetWorldDataConfig(worldTypeIndex);
        GridPos3D transportPlayerBornPoint = GridPos3D.Zero;
        foreach (GridPos3D worldModuleGP in worldData.WorldModuleGPOrder)
        {
            ushort worldModuleTypeIndex = worldData.ModuleMatrix[worldModuleGP.x, worldModuleGP.y, worldModuleGP.z];
            GridPos3D realModuleGP = new GridPos3D(worldModuleGP.x, World.WORLD_HEIGHT / 2 + worldModuleGP.y, worldModuleGP.z);
            if (worldModuleTypeIndex != 0)
            {
                if (worldModuleGP.y >= World.WORLD_HEIGHT / 2)
                {
                    Debug.LogError($"静态世界不允许超过{World.WORLD_HEIGHT / 2}个模组高度");
                    continue;
                }
                else
                {
                    yield return WorldManager.Instance.CurrentWorld.GenerateWorldModule(worldModuleTypeIndex, realModuleGP.x, realModuleGP.y, realModuleGP.z);
                    WorldModule module = WorldManager.Instance.CurrentWorld.WorldModuleMatrix[realModuleGP.x, realModuleGP.y, realModuleGP.z];
                    WorldManager.Instance.CurrentWorld.WorldData.WorldBornPointGroupData_Runtime.Init_LoadModuleData(realModuleGP, module.WorldModuleData);
                    yield return WorldManager.Instance.CurrentWorld.WorldData.WorldBornPointGroupData_Runtime.Dynamic_LoadModuleData(realModuleGP);
                    SortedDictionary<string, BornPointData> playerBornPoints = module.WorldModuleData.WorldModuleBornPointGroupData.PlayerBornPoints;
                    if (playerBornPoints.Count > 0)
                    {
                        if (transportPlayerBornPoint == GridPos3D.Zero) transportPlayerBornPoint = module.LocalGPToWorldGP(playerBornPoints[playerBornPoints.Keys.ToList()[0]].LocalGP);
                    }

                    List<BornPointData> moduleBPData = WorldManager.Instance.CurrentWorld.WorldData.WorldBornPointGroupData_Runtime.TryLoadModuleBPData(realModuleGP);
                    if (moduleBPData != null)
                    {
                        foreach (BornPointData bp in moduleBPData)
                        {
                            if (bp.ActorCategory == ActorCategory.Player)
                            {
                                string playerBPAlias = module.WorldModuleData.WorldModuleTypeName;
                                if (!WorldManager.Instance.CurrentWorld.WorldData.WorldBornPointGroupData_Runtime.PlayerBornPointDataAliasDict.ContainsKey(playerBPAlias))
                                {
                                    WorldManager.Instance.CurrentWorld.WorldData.WorldBornPointGroupData_Runtime.PlayerBornPointDataAliasDict.Add(playerBPAlias, bp);
                                }
                            }
                        }
                    }
                }
            }
        }

        if (transportPlayerBornPoint == GridPos3D.Zero)
        {
            Debug.LogWarning("传送的模组没有默认玩家出生点");
        }

        BattleManager.Instance.Player1.TransportPlayerGridPos(transportPlayerBornPoint);

        if (WorldManager.Instance.CurrentWorld is OpenWorld openWorld)
        {
            openWorld.IsInsideMicroModules = true;
        }

        ClientGameManager.Instance.DebugPanel.Clear();
        ClientGameManager.Instance.DebugPanel.Init();
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
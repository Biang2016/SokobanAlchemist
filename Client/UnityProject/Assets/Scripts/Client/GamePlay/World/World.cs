using System.Collections.Generic;
using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ObjectPool;
using UnityEngine;

public class World : PoolObject
{
    public const int WORLD_SIZE = 16;
    public const int WORLD_HEIGHT = 8;

    public WorldData WorldData;

    [HideInInspector]
    public WorldModule[,,] WorldModuleMatrix = new WorldModule[WORLD_SIZE, WORLD_HEIGHT, WORLD_SIZE];

    public WorldModule[,,] DeadZoneWorldModuleMatrix = new WorldModule[WORLD_SIZE + 2, WORLD_HEIGHT + 2, WORLD_SIZE + 2];

    private List<WorldCameraPOI> POIs = new List<WorldCameraPOI>();
    private List<LevelTriggerBase> WorldLevelTriggers = new List<LevelTriggerBase>();

    #region Root

    private Transform WorldModuleRoot;
    private Transform DeadZoneModuleRoot;
    private Transform WorldCameraPOIRoot;
    private Transform WorldLevelTriggerRoot;

    #endregion

    void Awake()
    {
        WorldModuleRoot = new GameObject("WorldModuleRoot").transform;
        WorldModuleRoot.parent = transform;
        DeadZoneModuleRoot = new GameObject("DeadZoneModuleRoot").transform;
        DeadZoneModuleRoot.parent = transform;
        WorldCameraPOIRoot = new GameObject("WorldCameraPOIRoot").transform;
        WorldCameraPOIRoot.parent = transform;
        WorldLevelTriggerRoot = new GameObject("WorldLevelTriggerRoot").transform;
        WorldLevelTriggerRoot.parent = transform;
    }

    public void Clear()
    {
        for (int x = 0; x < WorldModuleMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < WorldModuleMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < WorldModuleMatrix.GetLength(2); z++)
                {
                    WorldModuleMatrix[x, y, z]?.Clear();
                    WorldModuleMatrix[x, y, z]?.PoolRecycle();
                    WorldModuleMatrix[x, y, z] = null;
                }
            }
        }

        for (int x = 0; x < DeadZoneWorldModuleMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < DeadZoneWorldModuleMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < DeadZoneWorldModuleMatrix.GetLength(2); z++)
                {
                    DeadZoneWorldModuleMatrix[x, y, z]?.Clear();
                    DeadZoneWorldModuleMatrix[x, y, z]?.PoolRecycle();
                    DeadZoneWorldModuleMatrix[x, y, z] = null;
                }
            }
        }

        foreach (WorldCameraPOI poi in POIs)
        {
            poi.PoolRecycle();
        }

        POIs.Clear();
        foreach (LevelTriggerBase trigger in WorldLevelTriggers)
        {
            trigger.PoolRecycle();
        }

        WorldLevelTriggers.Clear();
        WorldData = null;
    }

    public void Initialize(WorldData worldData)
    {
        WorldData = worldData;
        for (int x = 0; x < worldData.ModuleMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < worldData.ModuleMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < worldData.ModuleMatrix.GetLength(2); z++)
                {
                    ushort worldModuleTypeIndex = worldData.ModuleMatrix[x, y, z];
                    if (worldModuleTypeIndex != 0)
                    {
                        GenerateWorldModule(worldModuleTypeIndex, x, y, z, worldData.ModuleBoxExtraSerializeDataMatrix[x, y, z]);
                    }
                }
            }
        }

        #region DeadZoneWorldModules

        for (int x = 0; x < WORLD_SIZE; x++)
        {
            for (int y = 0; y < WORLD_HEIGHT; y++)
            {
                for (int z = 1; z < WORLD_SIZE; z++)
                {
                    ushort index = worldData.ModuleMatrix[x, y, z];
                    ushort index_before = worldData.ModuleMatrix[x, y, z - 1];
                    if (index == 0 && index_before != 0)
                    {
                        GenerateWorldModule(WorldManager.DeadZoneIndex, x, y, z);
                    }

                    if (index != 0 && index_before == 0)
                    {
                        GenerateWorldModule(WorldManager.DeadZoneIndex, x, y, z - 1);
                    }

                    if (z == 1 && index_before != 0)
                    {
                        GenerateWorldModule(WorldManager.DeadZoneIndex, x, y, -1);
                    }

                    if (z == WORLD_SIZE - 1 && index != 0)
                    {
                        GenerateWorldModule(WorldManager.DeadZoneIndex, x, y, WORLD_SIZE);
                    }
                }
            }
        }

        for (int x = 0; x < WORLD_SIZE; x++)
        {
            for (int z = 0; z < WORLD_SIZE; z++)
            {
                for (int y = 1; y < WORLD_HEIGHT; y++)
                {
                    ushort index = worldData.ModuleMatrix[x, y, z];
                    ushort index_before = worldData.ModuleMatrix[x, y - 1, z];
                    if (index == 0 && index_before != 0)
                    {
                        GenerateWorldModule(WorldManager.DeadZoneIndex, x, y, z);
                    }

                    if (index != 0 && index_before == 0)
                    {
                        GenerateWorldModule(WorldManager.DeadZoneIndex, x, y - 1, z);
                    }

                    if (y == 1 && index_before != 0)
                    {
                        GenerateWorldModule(WorldManager.DeadZoneIndex, x, -1, z);
                    }

                    if (y == WORLD_HEIGHT - 1 && index != 0)
                    {
                        GenerateWorldModule(WorldManager.DeadZoneIndex, x, WORLD_HEIGHT, z);
                    }
                }
            }
        }

        for (int y = 0; y < WORLD_HEIGHT; y++)
        {
            for (int z = 0; z < WORLD_SIZE; z++)
            {
                for (int x = 1; x < WORLD_SIZE; x++)
                {
                    ushort index = worldData.ModuleMatrix[x, y, z];
                    ushort index_before = worldData.ModuleMatrix[x - 1, y, z];
                    if (index == 0 && index_before != 0)
                    {
                        GenerateWorldModule(WorldManager.DeadZoneIndex, x, y, z);
                    }

                    if (index != 0 && index_before == 0)
                    {
                        GenerateWorldModule(WorldManager.DeadZoneIndex, x - 1, y, z);
                    }

                    if (x == 1 && index_before != 0)
                    {
                        GenerateWorldModule(WorldManager.DeadZoneIndex, -1, y, z);
                    }

                    if (x == WORLD_SIZE - 1 && index != 0)
                    {
                        GenerateWorldModule(WorldManager.DeadZoneIndex, WORLD_SIZE, y, z);
                    }
                }
            }
        }

        #endregion

        foreach (GridPos3D gp in WorldData.WorldCameraPOIData.POIs)
        {
            WorldCameraPOI poi = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.WorldCameraPOI].AllocateGameObject<WorldCameraPOI>(WorldCameraPOIRoot);
            GridPos3D.ApplyGridPosToLocalTrans(gp, poi.transform, 1);
            POIs.Add(poi);
            ClientGameManager.Instance.BattleMessenger.Broadcast((uint) Enum_Events.OnWorldCameraPOILoaded, poi);
        }

        foreach (LevelTriggerBase.Data triggerData in WorldData.WorldLevelTriggerGroupData.TriggerDataList)
        {
            LevelTriggerBase trigger = GameObjectPoolManager.Instance.LevelTriggerDict[triggerData.LevelTriggerTypeIndex].AllocateGameObject<LevelTriggerBase>(WorldLevelTriggerRoot);
            trigger.InitializeInWorld((LevelTriggerBase.Data) triggerData.Clone());
            WorldLevelTriggers.Add(trigger);
        }

        foreach (Box.WorldSpecialBoxData worldSpecialBoxData in WorldData.WorldSpecialBoxDataList)
        {
            GridPos3D worldGP = worldSpecialBoxData.WorldGP;
            WorldModule module = GetModuleByGridPosition(worldGP);
            if (module != null)
            {
                module.GenerateBox(worldSpecialBoxData.BoxTypeIndex, module.WorldGPToLocalGP(worldGP), null, worldSpecialBoxData.BoxExtraSerializeDataFromWorld);
            }
        }

        //todo 未来加卸载模组时需要跑一遍这里
        worldData.WorldBornPointGroupData.InitTempData();
        for (int x = 0; x < worldData.ModuleMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < worldData.ModuleMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < worldData.ModuleMatrix.GetLength(2); z++)
                {
                    WorldModule module = WorldModuleMatrix[x, y, z];
                    if (module != null) worldData.WorldBornPointGroupData.AddModuleData(module, new GridPos3D(x, y, z));
                }
            }
        }

        BattleManager.Instance.CreateActorsByBornPointGroupData(worldData.WorldBornPointGroupData, worldData.DefaultWorldActorBornPointAlias);
    }

    private void GenerateWorldModule(ushort worldModuleTypeIndex, int x, int y, int z, List<Box.BoxExtraSerializeData> worldBoxExtraSerializeDataList = null)
    {
        bool isDeadModule = worldModuleTypeIndex == WorldManager.DeadZoneIndex;
        if (isDeadModule && DeadZoneWorldModuleMatrix[x + 1, y + 1, z + 1] != null) return;
        WorldModule wm = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.WorldModule].AllocateGameObject<WorldModule>(isDeadModule ? DeadZoneModuleRoot : WorldModuleRoot);
        WorldModuleData data = ConfigManager.GetWorldModuleDataConfig(worldModuleTypeIndex);

        wm.name = $"WM_{data.WorldModuleTypeName}({x}, {y}, {z})";
        if (isDeadModule)
        {
            DeadZoneWorldModuleMatrix[x + 1, y + 1, z + 1] = wm;
        }
        else
        {
            WorldModuleMatrix[x, y, z] = wm;
        }

        GridPos3D gp = new GridPos3D(x, y, z);
        GridPos3D.ApplyGridPosToLocalTrans(gp, wm.transform, WorldModule.MODULE_SIZE);
        wm.Initialize(data, gp, this, worldBoxExtraSerializeDataList);
    }

    #region MoveBox Calculators

    public Box GetBoxByGridPosition(GridPos3D gp, out WorldModule module, out GridPos3D localGP, bool ignoreUnaccessibleModule = true)
    {
        module = GetModuleByGridPosition(gp, ignoreUnaccessibleModule);
        if (module != null && (!ignoreUnaccessibleModule || module.IsAccessible))
        {
            localGP = module.WorldGPToLocalGP(gp);
            return module.BoxMatrix[localGP.x, localGP.y, localGP.z];
        }
        else
        {
            localGP = GridPos3D.Zero;
            return null;
        }
    }

    public WorldModule GetModuleByGridPosition(GridPos3D gp, bool ignoreUnaccessibleModule = true)
    {
        GridPos3D gp_module = new GridPos3D(Mathf.FloorToInt((float) gp.x / WorldModule.MODULE_SIZE), Mathf.FloorToInt((float) gp.y / WorldModule.MODULE_SIZE), Mathf.FloorToInt((float) gp.z / WorldModule.MODULE_SIZE));
        if (gp_module.x >= 0 && gp_module.x < WORLD_SIZE && gp_module.y >= 0 && gp_module.y < WORLD_HEIGHT && gp_module.z >= 0 && gp_module.z < WORLD_SIZE)
        {
            WorldModule module = WorldModuleMatrix[gp_module.x, gp_module.y, gp_module.z];
            if (module != null && (!ignoreUnaccessibleModule || module.IsAccessible))
            {
                return module;
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }

    public void MoveBox(GridPos3D srcGP, GridPos3D targetGP, Box.States sucState, bool needLerp = true, bool needLerpModel = false)
    {
        Box box_src = GetBoxByGridPosition(srcGP, out WorldModule module_src, out GridPos3D localGP_src);
        Box box_target = GetBoxByGridPosition(targetGP, out WorldModule module_target, out GridPos3D localGP_target);
        if (module_src == null || module_target == null || box_src == null || box_target != null) return;
        box_src.State = sucState;
        module_src.BoxMatrix[localGP_src.x, localGP_src.y, localGP_src.z] = null;
        module_target.BoxMatrix[localGP_target.x, localGP_target.y, localGP_target.z] = box_src;
        CheckDropAbove(box_src);
        box_src.Initialize(localGP_target, module_target, needLerp ? 0.2f : 0f, box_src.ArtOnly, Box.LerpType.Push, needLerpModel);
    }

    public void RemoveBoxFromGrid(Box box)
    {
        if (box.WorldModule)
        {
            WorldModule module = box.WorldModule;
            GridPos3D localGridPos3D = box.LocalGP;
            if (module.BoxMatrix[localGridPos3D.x, localGridPos3D.y, localGridPos3D.z] == box)
            {
                module.BoxMatrix[localGridPos3D.x, localGridPos3D.y, localGridPos3D.z] = null;
                CheckDropAbove(box);
                box.WorldModule = null;
            }

            WorldManager.Instance.OtherBoxDict.Add(box.GUID, box);
        }
    }

    public void DeleteBox(Box box)
    {
        RemoveBoxFromGrid(box);
        WorldManager.Instance.OtherBoxDict.Remove(box.GUID);
        box.PoolRecycle();
    }

    public void BoxReturnToWorldFromPhysics(Box box)
    {
        WorldManager.Instance.OtherBoxDict.Remove(box.GUID);
        GridPos3D gp = GridPos3D.GetGridPosByTrans(box.transform, 1);

        if (!tryPutBox(gp))
        {
            bool canPutAside = tryPutBox(gp + new GridPos3D(1, 0, 0))
                               || tryPutBox(gp + new GridPos3D(-1, 0, 0))
                               || tryPutBox(gp + new GridPos3D(0, 1, 0))
                               || tryPutBox(gp + new GridPos3D(0, -1, 0))
                               || tryPutBox(gp + new GridPos3D(0, 0, 1))
                               || tryPutBox(gp + new GridPos3D(0, 0, -1));
            if (!canPutAside) box.PoolRecycle(); // 无处可放即销毁
        }

        bool tryPutBox(GridPos3D worldGP)
        {
            WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByGridPosition(worldGP);
            if (module != null)
            {
                GridPos3D localGP = module.WorldGPToLocalGP(worldGP);
                Box existBox = module.BoxMatrix[localGP.x, localGP.y, localGP.z];
                if (existBox == null)
                {
                    module.BoxMatrix[localGP.x, localGP.y, localGP.z] = box;
                    Box.LerpType lerpType = Box.LerpType.Throw;
                    switch (box.State)
                    {
                        case Box.States.Flying:
                        {
                            lerpType = Box.LerpType.Throw;
                            break;
                        }
                        case Box.States.BeingKicked:
                        {
                            lerpType = Box.LerpType.Kick;
                            break;
                        }
                        case Box.States.DroppingFromDeadActor:
                        {
                            lerpType = Box.LerpType.DropFromDeadActor;
                            break;
                        }
                        case Box.States.Putting:
                        {
                            lerpType = Box.LerpType.Put;
                            break;
                        }
                    }

                    box.Initialize(localGP, module, 0.3f, box.ArtOnly, lerpType);
                    return true;
                }
                else
                {
                    if (existBox != box)
                    {
                        //string logStr = $"{box.name}想要前往的位置{localGP}非空, 存在{existBox.name}";
                        //Debug.LogError(logStr);
                        return false;
                    }
                }
            }

            return false;
        }
    }

    public void CheckDropSelf(Box box)
    {
        if (box && box.Droppable)
        {
            WorldModule module = box.WorldModule;
            GridPos3D localGridPos3D = box.LocalGP;
            if (localGridPos3D.y > 0)
            {
                Box boxBeneath = module.BoxMatrix[localGridPos3D.x, localGridPos3D.y - 1, localGridPos3D.z];
                if (boxBeneath == null)
                {
                    GridPos3D localGP = new GridPos3D(localGridPos3D.x, localGridPos3D.y - 1, localGridPos3D.z);
                    box.WorldModule.BoxMatrix[box.LocalGP.x, box.LocalGP.y, box.LocalGP.z] = null;
                    module.BoxMatrix[localGridPos3D.x, localGridPos3D.y - 1, localGridPos3D.z] = box;
                    CheckDropAbove(box);
                    box.Initialize(localGP, module, 0.1f, box.ArtOnly, Box.LerpType.Drop);
                }
            }
            else
            {
                if (module.ModuleGP.y > 0)
                {
                    WorldModule moduleBeneath = WorldModuleMatrix[module.ModuleGP.x, module.ModuleGP.y - 1, module.ModuleGP.z];
                    if (moduleBeneath && moduleBeneath.IsAccessible)
                    {
                        Box boxBeneath = moduleBeneath.BoxMatrix[localGridPos3D.x, WorldModule.MODULE_SIZE - 1, localGridPos3D.z];
                        if (boxBeneath == null)
                        {
                            GridPos3D localGP = new GridPos3D(localGridPos3D.x, WorldModule.MODULE_SIZE - 1, localGridPos3D.z);
                            box.WorldModule.BoxMatrix[box.LocalGP.x, box.LocalGP.y, box.LocalGP.z] = null;
                            moduleBeneath.BoxMatrix[localGridPos3D.x, WorldModule.MODULE_SIZE - 1, localGridPos3D.z] = box;
                            CheckDropAbove(box);
                            box.Initialize(localGP, moduleBeneath, 0.3f, box.ArtOnly, Box.LerpType.Drop);
                        }
                    }
                }
            }
        }
    }

    public void CheckDropAbove(Box box)
    {
        WorldModule module = box.WorldModule;
        GridPos3D localGridPos3D = box.LocalGP;
        if (localGridPos3D.y < WorldModule.MODULE_SIZE - 1)
        {
            Box boxAbove = module.BoxMatrix[localGridPos3D.x, localGridPos3D.y + 1, localGridPos3D.z];
            CheckDropSelf(boxAbove);
        }
        else
        {
            if (module.ModuleGP.y < WORLD_HEIGHT - 1)
            {
                WorldModule moduleAbove = WorldModuleMatrix[module.ModuleGP.x, module.ModuleGP.y + 1, module.ModuleGP.z];
                if (moduleAbove && moduleAbove.IsAccessible)
                {
                    Box boxAbove = moduleAbove.BoxMatrix[localGridPos3D.x, 0, localGridPos3D.z];
                    if (boxAbove)
                    {
                        CheckDropSelf(boxAbove);
                    }
                }
            }
        }
    }

    public enum SearchRangeShape
    {
        Circle,
        Square,
    }

    public List<Box> SearchBoxInRange(GridPos3D center, int radius, List<string> boxTypeNames, SearchRangeShape shape, float exceptRadiusAroundPlayer)
    {
        List<Box> res = new List<Box>();
        if (boxTypeNames == null || boxTypeNames.Count == 0) return res;

        GetBoxByGridPosition(center, out WorldModule module, out GridPos3D localGP);
        if (module)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int z = -radius; z <= radius; z++)
                {
                    if (shape == SearchRangeShape.Circle)
                    {
                        if (x * x + z * z > radius * radius) continue;
                    }
                    else if (shape == SearchRangeShape.Square)
                    {
                    }

                    GridPos3D gp = center + new GridPos3D(x, 0, z);
                    Box box = GetBoxByGridPosition(gp, out WorldModule tarModule, out GridPos3D _);
                    if (box != null)
                    {
                        if ((gp.ToVector3() - BattleManager.Instance.Player1.transform.position).magnitude > exceptRadiusAroundPlayer)
                        {
                            string boxName = ConfigManager.GetBoxTypeName(box.BoxTypeIndex);
                            if (boxName != null && boxTypeNames.Contains(boxName))
                            {
                                res.Add(box);
                            }
                        }
                    }
                }
            }
        }

        return res;
    }

    #endregion
}
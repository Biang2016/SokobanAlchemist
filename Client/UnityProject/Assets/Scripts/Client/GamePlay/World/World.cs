using System.Collections;
using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.Messenger;
using BiangLibrary.ObjectPool;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class World : PoolObject
{
    public const int WORLD_SIZE = 16;
    public const int WORLD_HEIGHT = 8;

    public WorldData WorldData;

    [HideInInspector]
    public WorldModule[,,] WorldModuleMatrix = new WorldModule[WORLD_SIZE, WORLD_HEIGHT, WORLD_SIZE];

    public WorldModule[,,] BorderWorldModuleMatrix = new WorldModule[WORLD_SIZE + 2, WORLD_HEIGHT + 2, WORLD_SIZE + 2];

    private List<WorldCameraPOI> POIs = new List<WorldCameraPOI>();
    private List<LevelTriggerBase> WorldLevelTriggers = new List<LevelTriggerBase>();

    public List<BoxPassiveSkill_LevelEventTriggerAppear> EventTriggerAppearBoxPassiveSkillList = new List<BoxPassiveSkill_LevelEventTriggerAppear>();

    #region Root

    private Transform WorldModuleRoot;
    private Transform BorderWorldModuleRoot;
    private Transform WorldCameraPOIRoot;
    private Transform WorldLevelTriggerRoot;

    #endregion

    void Awake()
    {
        WorldModuleRoot = new GameObject("WorldModuleRoot").transform;
        WorldModuleRoot.parent = transform;
        BorderWorldModuleRoot = new GameObject("BorderWorldModuleRoot").transform;
        BorderWorldModuleRoot.parent = transform;
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

        for (int x = 0; x < BorderWorldModuleMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < BorderWorldModuleMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < BorderWorldModuleMatrix.GetLength(2); z++)
                {
                    BorderWorldModuleMatrix[x, y, z]?.Clear();
                    BorderWorldModuleMatrix[x, y, z]?.PoolRecycle();
                    BorderWorldModuleMatrix[x, y, z] = null;
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
        foreach (BoxPassiveSkill_LevelEventTriggerAppear bf in EventTriggerAppearBoxPassiveSkillList)
        {
            bf.ClearAndUnRegister();
        }

        EventTriggerAppearBoxPassiveSkillList.Clear();
        WorldData = null;
    }

    public void Initialize(WorldData worldData)
    {
        WorldData = worldData;
        for (int x = 0; x < WorldData.ModuleMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < WorldData.ModuleMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < WorldData.ModuleMatrix.GetLength(2); z++)
                {
                    ushort worldModuleTypeIndex = WorldData.ModuleMatrix[x, y, z];
                    if (worldModuleTypeIndex != 0)
                    {
                        GenerateWorldModule(worldModuleTypeIndex, x, y, z, WorldData.ModuleBoxExtraSerializeDataMatrix[x, y, z]);
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
                    ushort index = WorldData.ModuleMatrix[x, y, z];
                    ushort index_before = WorldData.ModuleMatrix[x, y, z - 1];
                    if (index == 0 && index_before != 0)
                    {
                        GenerateWorldModule(ConfigManager.WorldModule_HiddenWallIndex, x, y, z);
                    }

                    if (index != 0 && index_before == 0)
                    {
                        GenerateWorldModule(ConfigManager.WorldModule_HiddenWallIndex, x, y, z - 1);
                    }

                    if (z == 1 && index_before != 0)
                    {
                        GenerateWorldModule(ConfigManager.WorldModule_HiddenWallIndex, x, y, -1);
                    }

                    if (z == WORLD_SIZE - 1 && index != 0)
                    {
                        GenerateWorldModule(ConfigManager.WorldModule_HiddenWallIndex, x, y, WORLD_SIZE);
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
                    ushort index = WorldData.ModuleMatrix[x, y, z];
                    ushort index_before = WorldData.ModuleMatrix[x, y - 1, z];
                    if (index == 0 && index_before != 0)
                    {
                        GenerateWorldModule(ConfigManager.WorldModule_DeadZoneIndex, x, y, z);
                    }

                    if (index != 0 && index_before == 0)
                    {
                        GenerateWorldModule(ConfigManager.WorldModule_DeadZoneIndex, x, y - 1, z);
                    }

                    if (y == 1 && index_before != 0)
                    {
                        GenerateWorldModule(ConfigManager.WorldModule_DeadZoneIndex, x, -1, z);
                    }

                    if (y == WORLD_HEIGHT - 1 && index != 0)
                    {
                        GenerateWorldModule(ConfigManager.WorldModule_DeadZoneIndex, x, WORLD_HEIGHT, z);
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
                    ushort index = WorldData.ModuleMatrix[x, y, z];
                    ushort index_before = WorldData.ModuleMatrix[x - 1, y, z];
                    if (index == 0 && index_before != 0)
                    {
                        GenerateWorldModule(ConfigManager.WorldModule_HiddenWallIndex, x, y, z);
                    }

                    if (index != 0 && index_before == 0)
                    {
                        GenerateWorldModule(ConfigManager.WorldModule_HiddenWallIndex, x - 1, y, z);
                    }

                    if (x == 1 && index_before != 0)
                    {
                        GenerateWorldModule(ConfigManager.WorldModule_HiddenWallIndex, -1, y, z);
                    }

                    if (x == WORLD_SIZE - 1 && index != 0)
                    {
                        GenerateWorldModule(ConfigManager.WorldModule_HiddenWallIndex, WORLD_SIZE, y, z);
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
            LevelTriggerBase.Data dataClone = (LevelTriggerBase.Data) triggerData.Clone();
            if (string.IsNullOrEmpty(dataClone.AppearLevelEventAlias))
            {
                GenerateLevelTrigger(dataClone);
            }
            else
            {
                Callback<string> cb = null;
                cb = (eventAlias) =>
                {
                    if (eventAlias.Equals(dataClone.AppearLevelEventAlias))
                    {
                        GenerateLevelTrigger(dataClone);
                        ClientGameManager.Instance.BattleMessenger.RemoveListener<string>((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, cb);
                    }
                };
                ClientGameManager.Instance.BattleMessenger.AddListener<string>((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, cb);
            }
        }

        foreach (BoxPassiveSkill_LevelEventTriggerAppear.Data data in WorldData.WorldSpecialBoxEventTriggerAppearBoxDataList)
        {
            BoxPassiveSkill_LevelEventTriggerAppear.Data dataClone = (BoxPassiveSkill_LevelEventTriggerAppear.Data) data.Clone();
            BoxPassiveSkill_LevelEventTriggerAppear bf = dataClone.BoxPassiveSkill_LevelEventTriggerAppear;
            bf.GenerateBoxAction = () =>
            {
                GridPos3D worldGP = data.WorldGP;
                Box box = GetBoxByGridPosition(worldGP, out WorldModule module, out GridPos3D localGP);
                box?.DestroyBox(); // 强行删除该格占用Box
                if (module)
                {
                    module.GenerateBox(dataClone.BoxTypeIndex, localGP.x, localGP.y, localGP.z, data.BoxOrientation, true, null, dataClone.WorldSpecialBoxData.BoxExtraSerializeDataFromWorld);

                    // Box生成后此BoxPassiveSkill及注册的事件均作废
                    bf.ClearAndUnRegister();
                    EventTriggerAppearBoxPassiveSkillList.Remove(bf);
                }
            };
            bf.OnRegisterLevelEventID();
            EventTriggerAppearBoxPassiveSkillList.Add(bf);
        }

        foreach (Box_LevelEditor.WorldSpecialBoxData worldSpecialBoxData in WorldData.WorldSpecialBoxDataList)
        {
            GridPos3D worldGP = worldSpecialBoxData.WorldGP;
            WorldModule module = GetModuleByGridPosition(worldGP);
            if (module != null)
            {
                module.GenerateBox(worldSpecialBoxData.BoxTypeIndex, module.WorldGPToLocalGP(worldGP), worldSpecialBoxData.BoxOrientation, false, null, worldSpecialBoxData.BoxExtraSerializeDataFromWorld);
            }
        }

        //todo 未来加卸载模组时需要跑一遍这里
        WorldData.WorldBornPointGroupData.InitTempData();
        foreach (GridPos3D worldModuleGP in WorldData.WorldModuleGPOrder)
        {
            WorldModule module = WorldModuleMatrix[worldModuleGP.x, worldModuleGP.y, worldModuleGP.z];
            if (module != null) WorldData.WorldBornPointGroupData.AddModuleData(module, worldModuleGP);
        }

        BattleManager.Instance.CreateActorsByBornPointGroupData(WorldData.WorldBornPointGroupData, WorldData.DefaultWorldActorBornPointAlias);
    }

    public void GenerateLevelTrigger(LevelTriggerBase.Data dataClone)
    {
        LevelTriggerBase trigger = GameObjectPoolManager.Instance.LevelTriggerDict[dataClone.LevelTriggerTypeIndex].AllocateGameObject<LevelTriggerBase>(WorldLevelTriggerRoot);
        trigger.InitializeInWorld(dataClone);
        WorldLevelTriggers.Add(trigger);
    }

    private void GenerateWorldModule(ushort worldModuleTypeIndex, int x, int y, int z, List<Box_LevelEditor.BoxExtraSerializeData> worldBoxExtraSerializeDataList = null)
    {
        bool isBorderModule = worldModuleTypeIndex == ConfigManager.WorldModule_DeadZoneIndex || worldModuleTypeIndex == ConfigManager.WorldModule_HiddenWallIndex;
        if (isBorderModule && BorderWorldModuleMatrix[x + 1, y + 1, z + 1] != null) return;
        WorldModule wm = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.WorldModule].AllocateGameObject<WorldModule>(isBorderModule ? BorderWorldModuleRoot : WorldModuleRoot);
        WorldModuleData data = ConfigManager.GetWorldModuleDataConfig(worldModuleTypeIndex);

        wm.name = $"WM_{data.WorldModuleTypeName}({x}, {y}, {z})";
        if (isBorderModule)
        {
            BorderWorldModuleMatrix[x + 1, y + 1, z + 1] = wm;
        }
        else
        {
            WorldModuleMatrix[x, y, z] = wm;
        }

        GridPos3D gp = new GridPos3D(x, y, z);
        GridPos3D.ApplyGridPosToLocalTrans(gp, wm.transform, WorldModule.MODULE_SIZE);
        wm.Initialize(data, gp, this, worldBoxExtraSerializeDataList);
    }

    #region Utils

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

    public List<Box> GetAroundBox(GridPos3D worldGP)
    {
        HashSet<Box> addedBoxes = new HashSet<Box>();
        List<Box> boxes = new List<Box>();
        Box leftBox = GetBoxByGridPosition(worldGP + new GridPos3D(-1, 0, 0), out WorldModule _, out GridPos3D _);
        if (leftBox)
        {
            if (!addedBoxes.Contains(leftBox))
            {
                boxes.Add(leftBox);
                addedBoxes.Add(leftBox);
            }
        }

        Box rightBox = GetBoxByGridPosition(worldGP + new GridPos3D(1, 0, 0), out WorldModule _, out GridPos3D _);
        if (rightBox)
        {
            if (!addedBoxes.Contains(rightBox))
            {
                boxes.Add(rightBox);
                addedBoxes.Add(rightBox);
            }
        }

        Box frontBox = GetBoxByGridPosition(worldGP + new GridPos3D(0, 0, 1), out WorldModule _, out GridPos3D _);
        if (frontBox)
        {
            if (!addedBoxes.Contains(frontBox))
            {
                boxes.Add(frontBox);
                addedBoxes.Add(frontBox);
            }
        }

        Box backBox = GetBoxByGridPosition(worldGP + new GridPos3D(0, 0, -1), out WorldModule _, out GridPos3D _);
        if (backBox)
        {
            if (!addedBoxes.Contains(backBox))
            {
                boxes.Add(backBox);
                addedBoxes.Add(backBox);
            }
        }

        Box leftFrontBox = GetBoxByGridPosition(worldGP + new GridPos3D(-1, 0, 1), out WorldModule _, out GridPos3D _);
        if (leftFrontBox)
        {
            if (!addedBoxes.Contains(leftFrontBox))
            {
                boxes.Add(leftFrontBox);
                addedBoxes.Add(leftFrontBox);
            }
        }

        Box rightFrontBox = GetBoxByGridPosition(worldGP + new GridPos3D(1, 0, 1), out WorldModule _, out GridPos3D _);
        if (rightFrontBox)
        {
            if (!addedBoxes.Contains(rightFrontBox))
            {
                boxes.Add(rightFrontBox);
                addedBoxes.Add(rightFrontBox);
            }
        }

        Box leftBackBox = GetBoxByGridPosition(worldGP + new GridPos3D(-1, 0, -1), out WorldModule _, out GridPos3D _);
        if (leftBackBox)
        {
            if (!addedBoxes.Contains(leftBackBox))
            {
                boxes.Add(leftBackBox);
                addedBoxes.Add(leftBackBox);
            }
        }

        Box rightBackBox = GetBoxByGridPosition(worldGP + new GridPos3D(1, 0, -1), out WorldModule _, out GridPos3D _);
        if (rightBackBox)
        {
            if (!addedBoxes.Contains(rightBackBox))
            {
                boxes.Add(rightBackBox);
                addedBoxes.Add(rightBackBox);
            }
        }

        return boxes;
    }

    public List<Box> GetAdjacentBox(GridPos3D worldGP)
    {
        HashSet<Box> addedBoxes = new HashSet<Box>();
        List<Box> boxes = new List<Box>();
        Box leftBox = GetBoxByGridPosition(worldGP + new GridPos3D(-1, 0, 0), out WorldModule _, out GridPos3D _);
        if (leftBox)
        {
            if (!addedBoxes.Contains(leftBox))
            {
                boxes.Add(leftBox);
                addedBoxes.Add(leftBox);
            }
        }

        Box rightBox = GetBoxByGridPosition(worldGP + new GridPos3D(1, 0, 0), out WorldModule _, out GridPos3D _);
        if (rightBox)
        {
            if (!addedBoxes.Contains(rightBox))
            {
                boxes.Add(rightBox);
                addedBoxes.Add(rightBox);
            }
        }

        Box frontBox = GetBoxByGridPosition(worldGP + new GridPos3D(0, 0, 1), out WorldModule _, out GridPos3D _);
        if (frontBox)
        {
            if (!addedBoxes.Contains(frontBox))
            {
                boxes.Add(frontBox);
                addedBoxes.Add(frontBox);
            }
        }

        Box backBox = GetBoxByGridPosition(worldGP + new GridPos3D(0, 0, -1), out WorldModule _, out GridPos3D _);
        if (backBox)
        {
            if (!addedBoxes.Contains(backBox))
            {
                boxes.Add(backBox);
                addedBoxes.Add(backBox);
            }
        }

        return boxes;
    }

    public bool BoxProject(GridPos3D dir, GridPos3D origin, int maxDistance, bool touchBox, out GridPos3D worldGP, out Box firstBox)
    {
        firstBox = null;
        worldGP = GridPos3D.Zero;
        if (maxDistance < 0) return false;
        int distance = 0;
        do
        {
            firstBox = GetBoxByGridPosition(origin + distance * dir, out WorldModule _, out GridPos3D _, false);
            distance++;
        } while (firstBox == null && distance <= maxDistance);

        if (firstBox != null)
        {
            worldGP = touchBox ? firstBox.WorldGP : firstBox.WorldGP - dir;
            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion

    public bool DropBoxOnTopLayer(ushort boxTypeIndex, GridPosR.Orientation boxOrientation, GridPos3D dir, GridPos3D origin, int maxDistance, out Box dropBox)
    {
        dropBox = null;
        if (boxTypeIndex == 0) return false;
        if (BoxProject(dir, origin, maxDistance, false, out _, out Box _))
        {
            WorldModule module = GetModuleByGridPosition(origin, true);
            if (module != null)
            {
                dropBox = GameObjectPoolManager.Instance.BoxDict[boxTypeIndex].AllocateGameObject<Box>(transform);
                string boxName = ConfigManager.GetBoxTypeName(boxTypeIndex);
                GridPos3D gp = origin;
                GridPos3D localGP = module.WorldGPToLocalGP(gp);
                dropBox.Setup(boxTypeIndex, boxOrientation);
                dropBox.Initialize(localGP, module, 0, false, Box.LerpType.DropFromAir);
                dropBox.name = $"{boxName}_{gp}";
                dropBox.DropFromAir();
                return true;
            }
        }

        return false;
    }

    #region MoveBox Calculators

    private Collider[] actorOccupiedGridSphereOverlapTempResultCache = new Collider[100];

    public bool CheckActorOccupiedGrid(GridPos3D targetGP, uint excludeActorGUID = 0)
    {
        int overlapCount = Physics.OverlapBoxNonAlloc(targetGP, 0.3f * Vector3.one, actorOccupiedGridSphereOverlapTempResultCache, Quaternion.identity, LayerManager.Instance.LayerMask_HitBox_Player | LayerManager.Instance.LayerMask_HitBox_Enemy, QueryTriggerInteraction.Collide);
        if (overlapCount > 0)
        {
            for (int index = 0; index < overlapCount; index++)
            {
                Collider result = actorOccupiedGridSphereOverlapTempResultCache[index];
                Actor actor = result.GetComponentInParent<Actor>();
                if (actor != null && actor.GUID != excludeActorGUID) return true;
            }

            return false;
        }

        return false;
    }

    public bool CheckCanMoveBoxColumn(
        GridPos3D srcGP, GridPos3D direction,
        HashSet<Box> boxes_moveable,
        uint excludeActorGUID = 0)
    {
        Box box_src = GetBoxByGridPosition(srcGP, out WorldModule module_src, out GridPos3D localGP_src);
        if (box_src == null) return true;
        foreach (GridPos3D offset in box_src.GetBoxOccupationGPs_Rotated())
        {
            GridPos3D gridGP = offset + box_src.WorldGP;
            GridPos3D gridGP_after = gridGP + direction;
            Box box_after = GetBoxByGridPosition(gridGP_after, out WorldModule module_after, out GridPos3D localGP_after);
            bool cannotMove = (box_after != box_src && box_after != null && !boxes_moveable.Contains(box_after))
                              || module_after == null
                              || CheckActorOccupiedGrid(gridGP_after, excludeActorGUID);
            if (cannotMove)
            {
                box_src.StartTryingDropSelf();
                return false;
            }
        }

        boxes_moveable.Add(box_src);

        List<Box> aboveNearestBoxList = new List<Box>();
        foreach (GridPos3D offset in box_src.GetBoxOccupationGPs_Rotated())
        {
            GridPos3D gridGP = offset + box_src.WorldGP;

            GridPos3D upVector = GridPos3D.Up;
            Box box_above = GetBoxByGridPosition(gridGP + upVector, out _, out _);
            while (box_above != null && boxes_moveable.Contains(box_above)) // 如果上下箱子是同一个箱子，说明这个箱子检查过了，跳过，再往上找
            {
                upVector += GridPos3D.Up;
                box_above = GetBoxByGridPosition(gridGP + upVector, out _, out _);
            }

            if (box_above != null && box_above.Droppable) // 只有droppable的箱子会跟着一起移动
            {
                if (!aboveNearestBoxList.Contains(box_above))
                {
                    aboveNearestBoxList.Add(box_above);
                }
            }
        }

        // 先检查推动方向坐标最大的那个格子
        if (direction.x == 1) aboveNearestBoxList.Sort((a, b) => -a.BoxBoundsInt.xMax.CompareTo(b.BoxBoundsInt.xMax));
        if (direction.x == -1) aboveNearestBoxList.Sort((a, b) => a.BoxBoundsInt.xMin.CompareTo(b.BoxBoundsInt.xMin));
        if (direction.y == 1) aboveNearestBoxList.Sort((a, b) => -a.BoxBoundsInt.yMax.CompareTo(b.BoxBoundsInt.yMax));
        if (direction.y == -1) aboveNearestBoxList.Sort((a, b) => a.BoxBoundsInt.yMin.CompareTo(b.BoxBoundsInt.yMin));
        if (direction.z == 1) aboveNearestBoxList.Sort((a, b) => -a.BoxBoundsInt.zMax.CompareTo(b.BoxBoundsInt.zMax));
        if (direction.z == -1) aboveNearestBoxList.Sort((a, b) => a.BoxBoundsInt.zMin.CompareTo(b.BoxBoundsInt.zMin));

        foreach (Box aboveNearestBox in aboveNearestBoxList)
        {
            CheckCanMoveBoxColumn(aboveNearestBox.WorldGP, direction, boxes_moveable, excludeActorGUID); // 箱子的每个格子都递归竖直往上检查
        }

        return true;
    }

    public void BoxColumnTransformDOPause(GridPos3D baseBoxGP, GridPos3D moveDirection)
    {
        HashSet<Box> moveableBoxes = new HashSet<Box>();
        CheckCanMoveBoxColumn(baseBoxGP, moveDirection, moveableBoxes);
        foreach (Box moveableBox in moveableBoxes)
        {
            moveableBox.transform.DOPause();
        }
    }

    public bool MoveBoxColumn(GridPos3D srcGP, GridPos3D direction, Box.States sucState, bool needLerp = true, bool needLerpModel = false, uint excludeActorGUID = 0)
    {
        HashSet<Box> boxes_moveable = new HashSet<Box>();
        bool valid = CheckCanMoveBoxColumn(srcGP, direction, boxes_moveable, excludeActorGUID);
        if (!valid) return false;

        foreach (Box box_moveable in boxes_moveable)
        {
            foreach (GridPos3D offset in box_moveable.GetBoxOccupationGPs_Rotated())
            {
                GridPos3D gridWorldGP_before = offset + box_moveable.WorldGP;
                GridPos3D gridWorldGP_after = gridWorldGP_before + direction;
                GetBoxByGridPosition(gridWorldGP_before, out WorldModule module_before, out GridPos3D boxGridLocalGP_before);
                GetBoxByGridPosition(gridWorldGP_after, out WorldModule module_after, out GridPos3D boxGridLocalGP_after);
                module_before.BoxMatrix[boxGridLocalGP_before.x, boxGridLocalGP_before.y, boxGridLocalGP_before.z] = null;
                module_after.BoxMatrix[boxGridLocalGP_after.x, boxGridLocalGP_after.y, boxGridLocalGP_after.z] = box_moveable;
            }

            box_moveable.State = sucState;
            WorldModule newModule = GetModuleByGridPosition(box_moveable.WorldGP + direction);
            Assert.IsNotNull(newModule);
            GridPos3D localGP = newModule.WorldGPToLocalGP(box_moveable.WorldGP + direction);
            box_moveable.Initialize(localGP, newModule, needLerp ? 0.2f : 0f, box_moveable.ArtOnly, Box.LerpType.Push, needLerpModel, false);
        }

        return true;
    }

    public void RemoveBoxFromGrid(Box box)
    {
        if (box.WorldModule)
        {
            foreach (GridPos3D offset in box.GetBoxOccupationGPs_Rotated())
            {
                GridPos3D gridWorldGP = offset + box.WorldGP;
                if (GetBoxByGridPosition(gridWorldGP, out WorldModule module, out GridPos3D localGP) == box)
                {
                    if (module.BoxMatrix[localGP.x, localGP.y, localGP.z] == box)
                    {
                        module.BoxMatrix[localGP.x, localGP.y, localGP.z] = null;
                    }
                }
            }

            CheckDropAbove(box);
            box.WorldModule = null;
            box.IsInGridSystem = false;

            if (!WorldManager.Instance.OtherBoxDict.ContainsKey(box.GUID))
            {
                WorldManager.Instance.OtherBoxDict.Add(box.GUID, box);
            }
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
            foreach (GridPos3D offset in box.GetBoxOccupationGPs_Rotated())
            {
                GridPos3D gridWorldGP = offset + worldGP;
                Box existBox = GetBoxByGridPosition(gridWorldGP, out WorldModule module, out GridPos3D localGP);
                if (module != null)
                {
                    if (existBox != null && existBox != box)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            foreach (GridPos3D offset in box.GetBoxOccupationGPs_Rotated())
            {
                GridPos3D gridWorldGP = offset + worldGP;
                GetBoxByGridPosition(gridWorldGP, out WorldModule module, out GridPos3D localGP);
                module.BoxMatrix[localGP.x, localGP.y, localGP.z] = box;
            }

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

            WorldModule module_box_core = GetModuleByGridPosition(worldGP);
            box.Initialize(module_box_core.WorldGPToLocalGP(worldGP), module_box_core, 0.3f, box.ArtOnly, lerpType);
            return true;
        }
    }

    public void CheckDropSelf(Box box)
    {
        if (box && box.Droppable && box.State != Box.States.DroppingFromAir)
        {
            bool spaceAvailable = true; // logically nothing beneath
            bool spaceTempAvailable = true; // there is not any entity moving/kicking beneath
            foreach (GridPos3D offset in box.GetBoxOccupationGPs_Rotated())
            {
                GridPos3D gridWorldGP = offset + box.WorldGP;
                GridPos3D beneathBoxGridWorldGP = gridWorldGP + GridPos3D.Down;

                Box boxBeneath = GetBoxByGridPosition(beneathBoxGridWorldGP, out WorldModule module, out GridPos3D localGP);
                if (module == null || (boxBeneath != null && boxBeneath != box))
                {
                    spaceAvailable = false;
                    break;
                }
            }

            foreach (GridPos3D offset in box.GetBoxOccupationGPs_Rotated())
            {
                Vector3 gridWorldPos = offset + box.WorldGP;
                Collider[] colliders = Physics.OverlapBox(gridWorldPos + Vector3.down * 0.5f, Vector3.one * 0.4f, Quaternion.identity, LayerManager.Instance.LayerMask_BoxIndicator | LayerManager.Instance.LayerMask_HitBox_Player | LayerManager.Instance.LayerMask_HitBox_Enemy);
                foreach (Collider collider in colliders)
                {
                    Entity entityBeneath = collider.GetComponentInParent<Entity>();
                    if (entityBeneath != null && entityBeneath.GUID != box.GUID)
                    {
                        if (entityBeneath is Actor)
                        {
                            int a = 0;
                        }

                        spaceTempAvailable = false;
                        break;
                    }
                }
            }

            if (spaceAvailable)
            {
                if (!spaceTempAvailable)
                {
                    box.StartTryingDropSelf();
                }
                else
                {
                    foreach (GridPos3D offset in box.GetBoxOccupationGPs_Rotated())
                    {
                        GridPos3D gridWorldGP = offset + box.WorldGP;
                        GridPos3D beneathBoxGridWorldGP = gridWorldGP + GridPos3D.Down;
                        GetBoxByGridPosition(gridWorldGP, out WorldModule module_before, out GridPos3D boxGridLocalGP_before);
                        GetBoxByGridPosition(beneathBoxGridWorldGP, out WorldModule module_after, out GridPos3D boxGridLocalGP_after);
                        module_before.BoxMatrix[boxGridLocalGP_before.x, boxGridLocalGP_before.y, boxGridLocalGP_before.z] = null;
                        module_after.BoxMatrix[boxGridLocalGP_after.x, boxGridLocalGP_after.y, boxGridLocalGP_after.z] = box;
                    }

                    CheckDropAbove(box); // 递归，检查上方箱子是否坠落
                    GridPos3D boxNewWorldGP = box.WorldGP + GridPos3D.Down;
                    GetBoxByGridPosition(boxNewWorldGP, out WorldModule newModule, out GridPos3D localGP);
                    box.Initialize(localGP, newModule, 0.1f, box.ArtOnly, Box.LerpType.Drop);
                }
            }
        }
    }

    public void CheckDropAbove(Box box)
    {
        HashSet<Box> boxSet = new HashSet<Box>(); // 避免上方的箱子也是Mega箱子，从而导致验算两遍
        foreach (GridPos3D offset in box.GetBoxOccupationGPs_Rotated())
        {
            GridPos3D gridWorldGP = offset + box.WorldGP + GridPos3D.Up;
            Box boxAbove = GetBoxByGridPosition(gridWorldGP, out WorldModule _, out GridPos3D _);
            if (boxAbove != null && !boxSet.Contains(boxAbove))
            {
                boxSet.Add(boxAbove);
                CheckDropSelf(boxAbove);
            }
        }
    }

    #endregion
}
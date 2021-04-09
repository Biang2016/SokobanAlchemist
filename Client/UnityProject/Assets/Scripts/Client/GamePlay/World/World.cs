using System;
using System.Collections;
using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.ObjectPool;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class World : PoolObject
{
    public const int WORLD_SIZE = 128;
    public const int WORLD_HEIGHT = 8;

    public WorldData WorldData;

    [HideInInspector]
    public WorldModule[,,] WorldModuleMatrix = new WorldModule[WORLD_SIZE, WORLD_HEIGHT, WORLD_SIZE];

    public WorldModule[,,] BorderWorldModuleMatrix = new WorldModule[WORLD_SIZE + 2, WORLD_HEIGHT + 2, WORLD_SIZE + 2];

    public string WorldGUID;

    #region Root

    protected Transform WorldModuleRoot;
    protected Transform BorderWorldModuleRoot;

    #endregion

    void Awake()
    {
        WorldModuleRoot = new GameObject("WorldModuleRoot").transform;
        WorldModuleRoot.parent = transform;
        BorderWorldModuleRoot = new GameObject("BorderWorldModuleRoot").transform;
        BorderWorldModuleRoot.parent = transform;
    }

    public IEnumerator Clear()
    {
        for (int x = 0; x < WorldModuleMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < WorldModuleMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < WorldModuleMatrix.GetLength(2); z++)
                {
                    WorldModule module = WorldModuleMatrix[x, y, z];
                    if (module != null)
                    {
                        yield return module.Clear(true);
                        module.PoolRecycle();
                        WorldModuleMatrix[x, y, z] = null;
                    }
                }
            }
        }

        for (int x = 0; x < BorderWorldModuleMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < BorderWorldModuleMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < BorderWorldModuleMatrix.GetLength(2); z++)
                {
                    WorldModule module = BorderWorldModuleMatrix[x, y, z];
                    if (module != null)
                    {
                        yield return module.Clear(true);
                        module.PoolRecycle();
                        BorderWorldModuleMatrix[x, y, z] = null;
                    }
                }
            }
        }

        WorldData = null;
    }

    public virtual IEnumerator Initialize(WorldData worldData)
    {
        ActorPathFinding.InitializeSpaceAvailableForActorHeight(WorldModuleMatrix.GetLength(0) * WorldModule.MODULE_SIZE, WorldModuleMatrix.GetLength(1) * WorldModule.MODULE_SIZE, WorldModuleMatrix.GetLength(2) * WorldModule.MODULE_SIZE);

        WorldGUID = Guid.NewGuid().ToString("P"); // e.g: (ade24d16-db0f-40af-8794-1e08e2040df3);
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
                        yield return GenerateWorldModule(worldModuleTypeIndex, x, y, z);
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
                        yield return GenerateWorldModule(ConfigManager.WorldModule_HiddenWallIndex, x, y, z);
                    }

                    if (index != 0 && index_before == 0)
                    {
                        yield return GenerateWorldModule(ConfigManager.WorldModule_HiddenWallIndex, x, y, z - 1);
                    }

                    if (z == 1 && index_before != 0)
                    {
                        yield return GenerateWorldModule(ConfigManager.WorldModule_HiddenWallIndex, x, y, -1);
                    }

                    if (z == WORLD_SIZE - 1 && index != 0)
                    {
                        yield return GenerateWorldModule(ConfigManager.WorldModule_HiddenWallIndex, x, y, WORLD_SIZE);
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
                        yield return GenerateWorldModule(ConfigManager.WorldModule_DeadZoneIndex, x, y, z);
                    }

                    if (index != 0 && index_before == 0)
                    {
                        yield return GenerateWorldModule(ConfigManager.WorldModule_DeadZoneIndex, x, y - 1, z);
                    }

                    if (y == 1 && index_before != 0)
                    {
                        yield return GenerateWorldModule(ConfigManager.WorldModule_DeadZoneIndex, x, -1, z);
                    }

                    if (y == WORLD_HEIGHT - 1 && index != 0)
                    {
                        yield return GenerateWorldModule(ConfigManager.WorldModule_DeadZoneIndex, x, WORLD_HEIGHT, z);
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
                        yield return GenerateWorldModule(ConfigManager.WorldModule_HiddenWallIndex, x, y, z);
                    }

                    if (index != 0 && index_before == 0)
                    {
                        yield return GenerateWorldModule(ConfigManager.WorldModule_HiddenWallIndex, x - 1, y, z);
                    }

                    if (x == 1 && index_before != 0)
                    {
                        yield return GenerateWorldModule(ConfigManager.WorldModule_HiddenWallIndex, -1, y, z);
                    }

                    if (x == WORLD_SIZE - 1 && index != 0)
                    {
                        yield return GenerateWorldModule(ConfigManager.WorldModule_HiddenWallIndex, WORLD_SIZE, y, z);
                    }
                }
            }
        }

        #endregion

        WorldData.WorldBornPointGroupData_Runtime.Init();
        foreach (GridPos3D worldModuleGP in WorldData.WorldModuleGPOrder)
        {
            WorldModule module = WorldModuleMatrix[worldModuleGP.x, worldModuleGP.y, worldModuleGP.z];
            if (module != null) WorldData.WorldBornPointGroupData_Runtime.Init_LoadModuleData(worldModuleGP, module.WorldModuleData);
        }

        // 加载模组默认玩家BP
        foreach (GridPos3D worldModuleGP in WorldData.WorldModuleGPOrder)
        {
            foreach (BornPointData bp in WorldData.WorldBornPointGroupData_Runtime.TryLoadModuleBPData(worldModuleGP))
            {
                WorldModule module = WorldModuleMatrix[worldModuleGP.x, worldModuleGP.y, worldModuleGP.z];
                string playerBPAlias = module.WorldModuleData.WorldModuleTypeName;
                if (!WorldData.WorldBornPointGroupData_Runtime.PlayerBornPointDataAliasDict.ContainsKey(playerBPAlias))
                {
                    WorldData.WorldBornPointGroupData_Runtime.PlayerBornPointDataAliasDict.Add(playerBPAlias, bp);
                }
            }
        }

        if (!WorldData.WorldBornPointGroupData_Runtime.PlayerBornPointDataAliasDict.ContainsKey(WorldData.DefaultWorldActorBornPointAlias))
        {
            Debug.LogError("世界默认出生点花名配置有误");
        }

        BattleManager.Instance.CreatePlayerByBornPointData(WorldData.WorldBornPointGroupData_Runtime.PlayerBornPointDataAliasDict[WorldData.DefaultWorldActorBornPointAlias]); // 生成主角
    }

    public virtual IEnumerator GenerateWorldModule(ushort worldModuleTypeIndex, int x, int y, int z, int loadBoxNumPerFrame = 99999)
    {
        bool isBorderModule = worldModuleTypeIndex == ConfigManager.WorldModule_DeadZoneIndex || worldModuleTypeIndex == ConfigManager.WorldModule_HiddenWallIndex;
        if (isBorderModule && BorderWorldModuleMatrix[x + 1, y + 1, z + 1] != null) yield break;
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
        yield return wm.Initialize(data, gp, this, loadBoxNumPerFrame);
    }

    protected virtual IEnumerator GenerateWorldModuleByCustomizedData(WorldModuleData data, int x, int y, int z, int loadEntityNumPerFrame)
    {
        WorldModule wm = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.OpenWorldModule].AllocateGameObject<WorldModule>(WorldModuleRoot);
        wm.name = $"WM_{data.WorldModuleTypeName}({x}, {y}, {z})";
        WorldModuleMatrix[x, y, z] = wm;
        GridPos3D gp = new GridPos3D(x, y, z);
        GridPos3D.ApplyGridPosToLocalTrans(gp, wm.transform, WorldModule.MODULE_SIZE);
        yield return wm.Initialize(data, gp, this, loadEntityNumPerFrame);
    }

    #region Utils

    private bool[] cached_occupyBeneath = new bool[Actor.ACTOR_MAX_HEIGHT];
    private bool[] cached_occupyAbove = new bool[Actor.ACTOR_MAX_HEIGHT];

    public void RefreshActorPathFindingSpaceAvailableCache(GridPos3D worldGP, Entity newEntity)
    {
        bool thisOccupy = newEntity != null;
        if (newEntity != null && (newEntity is Box {Passable: true})) thisOccupy = false;
        for (int i = 1; i <= Actor.ACTOR_MAX_HEIGHT; i++)
        {
            cached_occupyBeneath[i - 1] = ImpassableBoxOccupy(worldGP + GridPos3D.Down * i);
        }

        for (int i = 1; i <= Actor.ACTOR_MAX_HEIGHT; i++)
        {
            cached_occupyAbove[i - 1] = ImpassableBoxOccupy(worldGP + GridPos3D.Up * i);
        }

        if (thisOccupy)
        {
            // 上方检查
            for (int i = 1; i <= Actor.ACTOR_MAX_HEIGHT; i++)
            {
                bool aboveAvailable = true;
                for (int j = 1; j <= i; j++)
                {
                    if (cached_occupyAbove[j - 1])
                    {
                        aboveAvailable = false;
                        break;
                    }
                }

                ActorPathFinding.SetSpaceAvailableForActorHeight(worldGP + GridPos3D.Up, i, aboveAvailable); // 上方一个格子有了地板，重新检查各种高度Actor能否站立
            }

            // 自己检查
            for (int i = 1; i <= Actor.ACTOR_MAX_HEIGHT; i++)
            {
                ActorPathFinding.SetSpaceAvailableForActorHeight(worldGP, i, false); // 自己设为false
            }

            // 下方检查
            for (int i = 1; i <= Actor.ACTOR_MAX_HEIGHT - 1; i++) // 下方若干个格子
            {
                for (int k = i + 1; k <= Actor.ACTOR_MAX_HEIGHT; k++) // 超过一定高度的Actor就记为false了
                {
                    ActorPathFinding.SetSpaceAvailableForActorHeight(worldGP + GridPos3D.Down * i, k, false);
                }
            }
        }
        else
        {
            // 上方检查
            for (int i = 1; i <= Actor.ACTOR_MAX_HEIGHT; i++) // 上方一个格子没了地板，无法站Actor
            {
                ActorPathFinding.SetSpaceAvailableForActorHeight(worldGP + GridPos3D.Up, i, false);
            }

            // 自己检查
            if (cached_occupyBeneath[0])
            {
                for (int i = 1; i <= Actor.ACTOR_MAX_HEIGHT; i++)
                {
                    bool thisAvailable = true;
                    for (int j = 1; j <= i; j++)
                    {
                        if (cached_occupyAbove[i - 1])
                        {
                            thisAvailable = false;
                            break;
                        }
                    }

                    ActorPathFinding.SetSpaceAvailableForActorHeight(worldGP, i, thisAvailable); // 有了地板，重新检查各种高度Actor能否站立
                }
            }
            else //下方无地板，直接全部标为False
            {
                for (int i = 1; i <= Actor.ACTOR_MAX_HEIGHT; i++)
                {
                    ActorPathFinding.SetSpaceAvailableForActorHeight(worldGP, i, false);
                }
            }

            // 下方检查
            for (int i = 1; i <= Actor.ACTOR_MAX_HEIGHT - 1; i++)
            {
                bool hasFloor = cached_occupyBeneath[i];
                if (!hasFloor) continue; // 没有Floor说明原来就是False
                if (cached_occupyBeneath[i - 1]) continue; // 自己就是Occupy的说明原来就是False
                for (int k = i + 1; k <= Actor.ACTOR_MAX_HEIGHT; k++) // 超过一定高度的Actor就记为false了
                {
                    bool beneathAvailable = true;
                    for (int j = 0; j < i - 1; j++)
                    {
                        if (cached_occupyBeneath[j])
                        {
                            beneathAvailable = false;
                            break;
                        }
                    }

                    if (beneathAvailable)
                    {
                        for (int j = 0; j < (k - i) - 1; j++)
                        {
                            if (cached_occupyAbove[j])
                            {
                                beneathAvailable = false;
                                break;
                            }
                        }
                    }

                    ActorPathFinding.SetSpaceAvailableForActorHeight(worldGP + GridPos3D.Down * i, k, beneathAvailable);
                }
            }
        }
    }

    private bool ImpassableBoxOccupy(GridPos3D worldGP)
    {
        Entity entity = WorldManager.Instance.CurrentWorld.GetImpassableEntityByGridPosition(worldGP, 0, out WorldModule _, out GridPos3D _);
        if (entity != null && entity is Box) return true;
        return false;
    }

    public Box GetBoxByGridPosition(GridPos3D gp, out WorldModule module, out GridPos3D localGP, bool ignoreUnaccessibleModule = true, bool ignorePassableBox = false)
    {
        Entity entity = GetEntityByGridPositionCore(gp, 0, out module, out localGP, ignoreUnaccessibleModule, ignorePassableBox);
        if (entity is Box box)
        {
            if (ignorePassableBox && box.Passable) return null;
            return box;
        }

        return null;
    }

    public Entity GetImpassableEntityByGridPosition(GridPos3D gp, uint ignoreActorGUID, out WorldModule module, out GridPos3D localGP)
    {
        return GetEntityByGridPositionCore(gp, ignoreActorGUID, out module, out localGP, false, true);
    }

    private Entity GetEntityByGridPositionCore(GridPos3D gp, uint ignoreActorGUID, out WorldModule module, out GridPos3D localGP, bool ignoreUnaccessibleModule = true, bool ignorePassableBox = false)
    {
        module = GetModuleByWorldGP(gp, ignoreUnaccessibleModule);
        if (module != null && (!ignoreUnaccessibleModule || module.IsAccessible))
        {
            localGP = module.WorldGPToLocalGP(gp);
            Entity entity = module[ignorePassableBox ? TypeDefineType.Actor : TypeDefineType.Box, localGP];
            if (entity != null && entity.GUID == ignoreActorGUID) return null;
            return entity;
        }
        else
        {
            localGP = GridPos3D.Zero;
            return null;
        }
    }

    public Actor GetActorByGridPosition(GridPos3D gp, out WorldModule module, out GridPos3D localGP)
    {
        module = GetModuleByWorldGP(gp);
        if (module != null && module.IsAccessible)
        {
            localGP = module.WorldGPToLocalGP(gp);
            return module.GetActorOccupation(localGP);
        }
        else
        {
            localGP = GridPos3D.Zero;
            return null;
        }
    }

    public GridPos3D GetWorldGPByModuleGP(GridPos3D moduleGP, GridPos3D localGP)
    {
        return new GridPos3D(moduleGP.x * WorldModule.MODULE_SIZE + localGP.x, moduleGP.y * WorldModule.MODULE_SIZE + localGP.y, moduleGP.z * WorldModule.MODULE_SIZE + localGP.z);
    }

    public GridPos3D GetModuleGPByWorldGP(GridPos3D worldGP)
    {
        GridPos3D gp_module = new GridPos3D(Mathf.FloorToInt((float) worldGP.x / WorldModule.MODULE_SIZE), Mathf.FloorToInt((float) worldGP.y / WorldModule.MODULE_SIZE), Mathf.FloorToInt((float) worldGP.z / WorldModule.MODULE_SIZE));
        return gp_module;
    }

    public WorldModule GetModuleByWorldGP(GridPos3D worldGP, bool ignoreUnaccessibleModule = true)
    {
        GridPos3D gp_module = GetModuleGPByWorldGP(worldGP);
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

        Box upBox = GetBoxByGridPosition(worldGP + new GridPos3D(0, 1, 0), out WorldModule _, out GridPos3D _);
        if (upBox)
        {
            if (!addedBoxes.Contains(upBox))
            {
                boxes.Add(upBox);
                addedBoxes.Add(upBox);
            }
        }

        Box downBox = GetBoxByGridPosition(worldGP + new GridPos3D(0, -1, 0), out WorldModule _, out GridPos3D _);
        if (downBox)
        {
            if (!addedBoxes.Contains(downBox))
            {
                boxes.Add(downBox);
                addedBoxes.Add(downBox);
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
        GridPos3D currentCastGP = origin;
        do
        {
            currentCastGP += dir;
            firstBox = GetBoxByGridPosition(currentCastGP, out WorldModule _, out GridPos3D _, false, false);
            distance++;
        } while (firstBox == null && distance <= maxDistance);

        if (firstBox != null)
        {
            worldGP = touchBox ? currentCastGP : currentCastGP - dir;
            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion

    #region MoveBox Calculators

    public bool CheckCanMoveBoxColumn(
        GridPos3D srcGP, GridPos3D direction,
        HashSet<Box> boxes_moveable,
        uint excludeActorGUID = 0)
    {
        Box box_src = GetBoxByGridPosition(srcGP, out WorldModule module_src, out GridPos3D localGP_src);
        if (box_src == null || !module_src.IsNotNullAndAvailable()) return true;
        foreach (GridPos3D offset in box_src.GetEntityOccupationGPs_Rotated())
        {
            GridPos3D gridGP = offset + box_src.WorldGP;
            GridPos3D gridGP_after = gridGP + direction;
            Box box_after = GetBoxByGridPosition(gridGP_after, out WorldModule module_after, out GridPos3D localGP_after);

            if (!module_after.IsNotNullAndAvailable()) return false;
            bool beBlockedByOtherBox = box_after != null && box_after != box_src && !boxes_moveable.Contains(box_after);
            if (beBlockedByOtherBox)
            {
                bool is_box_after_aboveBox = false;
                foreach (Box beneathBox in box_after.GetBeneathBoxes())
                {
                    if (beneathBox == box_src || boxes_moveable.Contains(beneathBox))
                    {
                        is_box_after_aboveBox = true;
                    }
                }

                if (is_box_after_aboveBox) beBlockedByOtherBox = false;
            }

            Actor actor = GetActorByGridPosition(gridGP_after, out WorldModule _, out GridPos3D _);
            bool beBlockedByActor = actor != null && actor.GUID != excludeActorGUID;
            bool cannotMove = module_after == null || beBlockedByOtherBox || beBlockedByActor;

            if (cannotMove)
            {
                box_src.StartTryingDropSelf();
                return false;
            }
        }

        boxes_moveable.Add(box_src);

        List<Box> aboveNearestBoxList = new List<Box>();
        foreach (GridPos3D offset in box_src.GetEntityOccupationGPs_Rotated())
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
        if (direction.x == 1) aboveNearestBoxList.Sort((a, b) => -a.EntityBoundsInt.xMax.CompareTo(b.EntityBoundsInt.xMax));
        if (direction.x == -1) aboveNearestBoxList.Sort((a, b) => a.EntityBoundsInt.xMin.CompareTo(b.EntityBoundsInt.xMin));
        if (direction.y == 1) aboveNearestBoxList.Sort((a, b) => -a.EntityBoundsInt.yMax.CompareTo(b.EntityBoundsInt.yMax));
        if (direction.y == -1) aboveNearestBoxList.Sort((a, b) => a.EntityBoundsInt.yMin.CompareTo(b.EntityBoundsInt.yMin));
        if (direction.z == 1) aboveNearestBoxList.Sort((a, b) => -a.EntityBoundsInt.zMax.CompareTo(b.EntityBoundsInt.zMax));
        if (direction.z == -1) aboveNearestBoxList.Sort((a, b) => a.EntityBoundsInt.zMin.CompareTo(b.EntityBoundsInt.zMin));

        foreach (Box aboveNearestBox in aboveNearestBoxList)
        {
            CheckCanMoveBoxColumn(aboveNearestBox.WorldGP, direction, boxes_moveable, excludeActorGUID); // 箱子的每个格子都递归竖直往上检查
        }

        return true;
    }

    public void BoxColumnTransformDOPause(GridPos3D baseBoxGP, GridPos3D moveDirection, uint excludeActorGUID = 0)
    {
        HashSet<Box> moveableBoxes = new HashSet<Box>();
        CheckCanMoveBoxColumn(baseBoxGP, moveDirection, moveableBoxes, excludeActorGUID);
        foreach (Box moveableBox in moveableBoxes)
        {
            moveableBox.transform.DOPause();
            if (Box.ENABLE_BOX_MOVE_LOG) Debug.Log($"[{Time.frameCount}] BoxTransform DOPause {moveableBox.name}");
        }
    }

    public bool MoveBoxColumn(GridPos3D srcGP, GridPos3D direction, Box.States sucState, bool needLerp = true, bool needLerpModel = false, uint excludeActorGUID = 0)
    {
        if (direction == GridPos3D.Zero) return false;
        HashSet<Box> boxes_moveable = new HashSet<Box>();
        bool valid = CheckCanMoveBoxColumn(srcGP, direction, boxes_moveable, excludeActorGUID);
        if (!valid) return false;

        // 先将矩阵对应格子清空
        foreach (Box box_moveable in boxes_moveable)
        {
            foreach (GridPos3D offset in box_moveable.GetEntityOccupationGPs_Rotated())
            {
                GridPos3D gridWorldGP_before = offset + box_moveable.WorldGP;
                GetBoxByGridPosition(gridWorldGP_before, out WorldModule module_before, out GridPos3D boxGridLocalGP_before);
                module_before[TypeDefineType.Box, boxGridLocalGP_before] = null;
            }
        }

        // 再往矩阵填入引用，不可在一个循环内完成，否则后面的Box会将前面的引用置空
        foreach (Box box_moveable in boxes_moveable)
        {
            foreach (GridPos3D offset in box_moveable.GetEntityOccupationGPs_Rotated())
            {
                GridPos3D gridWorldGP_before = offset + box_moveable.WorldGP;
                GridPos3D gridWorldGP_after = gridWorldGP_before + direction;
                GetBoxByGridPosition(gridWorldGP_after, out WorldModule module_after, out GridPos3D boxGridLocalGP_after);
                module_after[TypeDefineType.Box, boxGridLocalGP_after] = box_moveable;
            }

            box_moveable.State = sucState;
            WorldModule newModule = GetModuleByWorldGP(box_moveable.WorldGP + direction);
            Assert.IsNotNull(newModule);
            GridPos3D worldGP = box_moveable.WorldGP + direction;
            box_moveable.Initialize(worldGP, newModule, needLerp ? 0.2f : 0f, box_moveable.ArtOnly, Box.LerpType.Push, needLerpModel, false);
        }

        TryMerge(direction, boxes_moveable);
        return true;
    }

    private void TryMerge(GridPos3D direction, HashSet<Box> boxes_moveable)
    {
        HashSet<Box> mergedBoxes = new HashSet<Box>();
        List<(Box, ushort, HashSet<Box>, GridPosR.Orientation, GridPos3D)> mergeTaskList = new List<(Box, ushort, HashSet<Box>, GridPosR.Orientation, GridPos3D)>();
        foreach (Box box_moveable in boxes_moveable)
        {
            if (mergedBoxes.Contains(box_moveable)) continue;
            HashSet<Box> matchedBoxes = CheckMatchN(box_moveable, direction, mergedBoxes, box_moveable.BoxMergeConfig, out MergeOrientation mergeOrientation, out GridPosR.Orientation newBoxOrientation, out GridPos3D newBoxOffset);
            if (matchedBoxes != null && matchedBoxes.Count > 0)
            {
                ushort newBoxTypeIndex = box_moveable.BoxMergeConfig.GetMergeBoxTypeIndex(matchedBoxes.Count, mergeOrientation);
                if (newBoxTypeIndex != 0)
                {
                    mergeTaskList.Add((box_moveable, newBoxTypeIndex, matchedBoxes, newBoxOrientation, newBoxOffset));
                }
            }
        }

        foreach ((Box, ushort, HashSet<Box>, GridPosR.Orientation, GridPos3D) task in mergeTaskList)
        {
            Box oldCoreBox = task.Item1;
            ushort newBoxTypeIndex = task.Item2;
            HashSet<Box> oldBoxes = task.Item3;
            GridPosR.Orientation newBoxOrientation = task.Item4;
            GridPos3D newBoxOffset = task.Item5;
            GridPos3D mergeTargetWorldGP = oldCoreBox.WorldGP + newBoxOffset;

            List<GridPos3D> allPossibleMergeTargetWorldGP = new List<GridPos3D>();
            List<GridPos3D> newBoxOccupation_rotated = ConfigManager.GetEntityOccupationData(newBoxTypeIndex).EntityIndicatorGPs_RotatedDict[newBoxOrientation];
            foreach (GridPos3D offset in newBoxOccupation_rotated)
            {
                allPossibleMergeTargetWorldGP.Add(offset + mergeTargetWorldGP);
            }

            foreach (Box oldBox in oldBoxes)
            {
                if (oldBox == oldCoreBox) continue;
                GridPos3D nearestGP = GridPos3D.GetNearestGPFromList(oldBox.WorldGP, allPossibleMergeTargetWorldGP);
                oldBox.MergeBox(nearestGP);
            }

            // 保证目标点的箱子最后合成消失，从而生成新箱子的时候不会与任何一个老箱子冲突
            GridPos3D nearestGP_Core = GridPos3D.GetNearestGPFromList(oldCoreBox.WorldGP, allPossibleMergeTargetWorldGP);
            oldCoreBox.MergeBox(nearestGP_Core, delegate
            {
                WorldModule module = GetModuleByWorldGP(mergeTargetWorldGP);
                if (module != null)
                {
                    EntityData entityData = new EntityData(newBoxTypeIndex, newBoxOrientation);
                    Box box = (Box) module.GenerateEntity(entityData, mergeTargetWorldGP, false, false, true); // 合成生成的箱子允许往上方堆
                    if (box != null)
                    {
                        FXManager.Instance.PlayFX(box.MergedFX, box.transform.position);
                        TryMerge(direction, new HashSet<Box> {box});
                    }
                }
            });
        }
    }

    /// <summary>
    /// 检查能否多消
    /// </summary>
    /// <param name="srcBox">移动的箱子（核心）</param>
    /// <param name="srcBoxLastMoveDir">最后一次移动方向</param>
    /// <param name="mergedBoxes">此次Move所有已经标记为正在合成的箱子</param>
    /// <param name="boxMergeConfig">合并配置</param>
    /// <param name="newBoxOrientation"></param>
    /// <param name="newBoxOffset"></param>
    /// <returns>此函数计算出的需要合并的箱子集</returns>
    protected HashSet<Box> CheckMatchN(Box srcBox, GridPos3D srcBoxLastMoveDir, HashSet<Box> mergedBoxes, BoxMergeConfig boxMergeConfig, out MergeOrientation mergeOrientation, out GridPosR.Orientation newBoxOrientation, out GridPos3D newBoxOffset)
    {
        newBoxOrientation = GridPosR.Orientation.Up;
        newBoxOffset = GridPos3D.Zero;
        mergeOrientation = MergeOrientation.X;
        //if (!srcBox.IsBoxShapeCuboid()) return null; // todo 移除此限制

        BoundsInt boundsInt = srcBox.EntityBoundsInt;
        bool XZRev = srcBox.EntityOrientation == GridPosR.Orientation.Left || srcBox.EntityOrientation == GridPosR.Orientation.Right;
        if (XZRev) boundsInt = new BoundsInt(boundsInt.zMin, boundsInt.yMin, boundsInt.xMin, boundsInt.size.z, boundsInt.size.y, boundsInt.size.x);

        int maxMergeCount_x = int.MinValue;
        int maxMergeCount_z = int.MinValue;
        List<int> validMergeCounts_x = boxMergeConfig.GetAllValidMergeCounts(XZRev ? MergeOrientation.Z : MergeOrientation.X);
        List<int> validMergeCounts_z = boxMergeConfig.GetAllValidMergeCounts(XZRev ? MergeOrientation.X : MergeOrientation.Z);
        foreach (int vmc in validMergeCounts_x) maxMergeCount_x = Mathf.Max(vmc, maxMergeCount_x);
        foreach (int vmc in validMergeCounts_z) maxMergeCount_z = Mathf.Max(vmc, maxMergeCount_z);

        bool canMerge_x = maxMergeCount_x >= 3;
        bool canMerge_z = maxMergeCount_z >= 3;
        if (!canMerge_x && !canMerge_z) return null;
        int N_x = maxMergeCount_x - 1;
        int N_z = maxMergeCount_z - 1;
        bool[] x_match_matrix = null;
        bool[] z_match_matrix = null;
        if (canMerge_x) x_match_matrix = new bool[N_x * 2];
        if (canMerge_z) z_match_matrix = new bool[N_z * 2];
        if (canMerge_x)
            for (int i = 0; i < N_x * 2; i++)
                x_match_matrix[i] = true;
        if (canMerge_z)
            for (int i = 0; i < N_z * 2; i++)
                z_match_matrix[i] = true;

        foreach (GridPos3D offset in srcBox.GetEntityOccupationGPs_Rotated())
        {
            GridPos3D alignRefGP = srcBox.WorldGP + offset; // 取每个点来作为基准，都要在相隔同样距离处找到同种箱子

            // 依次找x轴左右各N格，和z轴前后各N格
            if (canMerge_x)
                for (int i = -N_x; i <= N_x; i++)
                {
                    if (i == 0) continue;
                    int matrixIndex = i > 0 ? i + N_x - 1 : i + N_x;
                    GridPos3D targetGP = alignRefGP + GridPos3D.Right * i * boundsInt.size.x;
                    Box targetBox = GetBoxByGridPosition(targetGP, out WorldModule _, out GridPos3D _);
                    if (targetBox == null || targetBox == srcBox || targetBox.EntityTypeIndex != srcBox.EntityTypeIndex)
                    {
                        x_match_matrix[matrixIndex] = false;
                    }
                }

            if (canMerge_z)
                for (int i = -N_z; i <= N_z; i++)
                {
                    if (i == 0) continue;
                    int matrixIndex = i > 0 ? i + N_z - 1 : i + N_z;
                    GridPos3D targetGP = alignRefGP + GridPos3D.Forward * i * boundsInt.size.z;
                    Box targetBox = GetBoxByGridPosition(targetGP, out WorldModule _, out GridPos3D _);
                    if (targetBox == null || targetBox == srcBox || targetBox.EntityTypeIndex != srcBox.EntityTypeIndex)
                    {
                        z_match_matrix[matrixIndex] = false;
                    }
                }
        }

        int x_connect_positive = 1;
        int x_connect_negative = 1;
        int z_connect_positive = 1;
        int z_connect_negative = 1;

        if (canMerge_x)
            for (; x_connect_positive <= N_x; x_connect_positive++)
                if (!x_match_matrix[x_connect_positive + N_x - 1])
                    break;

        if (canMerge_x)
            for (; x_connect_negative <= N_x; x_connect_negative++)
                if (!x_match_matrix[N_x - x_connect_negative])
                    break;

        if (canMerge_z)
            for (; z_connect_positive <= N_z; z_connect_positive++)
                if (!z_match_matrix[z_connect_positive + N_z - 1])
                    break;

        if (canMerge_z)
            for (; z_connect_negative <= N_z; z_connect_negative++)
                if (!z_match_matrix[N_z - z_connect_negative])
                    break;

        x_connect_positive--;
        x_connect_negative--;
        z_connect_positive--;
        z_connect_negative--;

        int matchCountOnXAxis = 1 + x_connect_positive + x_connect_negative;
        bool matchOnXAxis = false;
        foreach (int vmc in validMergeCounts_x)
        {
            if (matchCountOnXAxis == vmc)
            {
                matchOnXAxis = true;
                break;
            }

            if (matchCountOnXAxis > vmc) // 向下兼容
            {
                matchCountOnXAxis = vmc;
                matchOnXAxis = true;
            }
        }

        int matchCountOnZAxis = 1 + z_connect_positive + z_connect_negative;
        bool matchOnZAxis = false;
        foreach (int vmc in validMergeCounts_z)
        {
            if (matchCountOnZAxis == vmc)
            {
                matchOnZAxis = true;
                break;
            }

            if (matchCountOnZAxis > vmc) // 向下兼容
            {
                matchCountOnZAxis = vmc;
                matchOnZAxis = true;
            }
        }

        if (!matchOnXAxis && !matchOnZAxis) return null;

        // 若双向同时match，舍弃一向
        if (matchOnXAxis && matchOnZAxis)
        {
            if (srcBoxLastMoveDir == GridPos3D.Forward || srcBoxLastMoveDir == GridPos3D.Back) matchOnXAxis = false;
            if (srcBoxLastMoveDir == GridPos3D.Left || srcBoxLastMoveDir == GridPos3D.Right) matchOnZAxis = false;
        }

        Assert.IsFalse(matchOnXAxis && matchOnZAxis);

        // 如果connect的箱子超出了合成需要，修剪较长的一个方向
        if (matchOnXAxis)
        {
            while (matchCountOnXAxis < 1 + x_connect_positive + x_connect_negative)
            {
                if (x_connect_positive > x_connect_negative) x_connect_positive--;
                else x_connect_negative--;
            }
        }

        if (matchOnZAxis)
        {
            while (matchCountOnZAxis < 1 + z_connect_positive + z_connect_negative)
            {
                if (z_connect_positive > z_connect_negative) z_connect_positive--;
                else z_connect_negative--;
            }
        }

        // 记录合成
        HashSet<Box> matchedBoxes = new HashSet<Box>(); // 这里指本次合成的箱子
        if (matchOnXAxis)
        {
            for (int offsetUnit = -x_connect_negative; offsetUnit <= x_connect_positive; offsetUnit++)
            {
                GridPos3D targetGP = srcBox.WorldGP + GridPos3D.Right * offsetUnit * boundsInt.size.x; // 这里用Box.WorldGP是仅为了随便取一个grid
                Box targetBox = GetBoxByGridPosition(targetGP, out WorldModule _, out GridPos3D _);
                Assert.IsNotNull(targetBox);
                matchedBoxes.Add(targetBox);
                mergedBoxes.Add(targetBox); // 这是此次Move合成的所有箱子
            }

            mergeOrientation = XZRev ? MergeOrientation.Z : MergeOrientation.X;
        }

        if (matchOnZAxis)
        {
            for (int offsetUnit = -z_connect_negative; offsetUnit <= z_connect_positive; offsetUnit++)
            {
                GridPos3D targetGP = srcBox.WorldGP + GridPos3D.Forward * offsetUnit * boundsInt.size.z; // 这里用Box.WorldGP是仅为了随便取一个grid
                Box targetBox = GetBoxByGridPosition(targetGP, out WorldModule _, out GridPos3D _);
                Assert.IsNotNull(targetBox);
                matchedBoxes.Add(targetBox);
                mergedBoxes.Add(targetBox);
            }

            mergeOrientation = XZRev ? MergeOrientation.X : MergeOrientation.Z;
        }

        // 决定合并后的箱子朝向和偏移
        EntityOccupationData boxOccupationData = ConfigManager.GetEntityOccupationData(matchOnXAxis ? boxMergeConfig.GetMergeBoxTypeIndex(matchCountOnXAxis, XZRev ? MergeOrientation.Z : MergeOrientation.X) : boxMergeConfig.GetMergeBoxTypeIndex(matchCountOnZAxis, XZRev ? MergeOrientation.X : MergeOrientation.Z));

        // 默认朝向定为箱子推进的前方
        if (srcBoxLastMoveDir.x == 1) newBoxOrientation = GridPosR.Orientation.Right;
        if (srcBoxLastMoveDir.x == -1) newBoxOrientation = GridPosR.Orientation.Left;
        if (srcBoxLastMoveDir.z == 1) newBoxOrientation = GridPosR.Orientation.Up;
        if (srcBoxLastMoveDir.z == -1) newBoxOrientation = GridPosR.Orientation.Down;

        // 当产物为Mega箱子时，需要考虑占位问题
        if (boxOccupationData.EntityIndicatorGPs.Count > 1)
        {
            List<GridPos3D> availableGPs = new List<GridPos3D>(16);
            foreach (Box mergedBox in mergedBoxes)
            {
                foreach (GridPos3D gridPos in mergedBox.GetEntityOccupationGPs_Rotated())
                {
                    availableGPs.Add(mergedBox.WorldGP + gridPos);
                }
            }

            bool valid = false;
            int best_offset_x = 0;
            int best_offset_z = 0;
            int max_overlapGridCount = int.MinValue;
            GridPosR.Orientation best_ori = GridPosR.Orientation.Up;
            // 有偏移，其他朝向全部试一遍，范围正负5格
            for (int offset_x = 1; offset_x <= 10; offset_x++)
            {
                if (valid) break;
                int offset_x_signed = offset_x / 2 * (offset_x % 2 == 1 ? 1 : -1);
                for (int offset_z = 1; offset_z <= 10; offset_z++)
                {
                    if (valid) break;
                    int offset_z_signed = offset_z / 2 * (offset_z % 2 == 1 ? 1 : -1);
                    for (int i = 0; i < 4; i++)
                    {
                        GridPosR.Orientation rot = (GridPosR.Orientation) i;
                        GridPos3D offset = new GridPos3D(offset_x_signed, 0, offset_z_signed);
                        valid = CheckSpaceAvailable((GridPosR.Orientation) i, offset, out int overlapGridCount);
                        if (valid)
                        {
                            newBoxOrientation = (GridPosR.Orientation) i;
                            newBoxOffset = offset;
                            break;
                        }
                        else
                        {
                            if (overlapGridCount > max_overlapGridCount)
                            {
                                max_overlapGridCount = overlapGridCount;
                                best_offset_x = offset_x_signed;
                                best_offset_z = offset_z_signed;
                                best_ori = rot;
                            }
                        }
                    }
                }
            }

            if (!valid) // 找不到时，取重叠覆盖最多的方案
            {
                //Debug.Log($"Notfound space, use {max_overlapGridCount}");
                newBoxOrientation = best_ori;
                newBoxOffset = new GridPos3D(best_offset_x, 0, best_offset_z);
            }

            bool CheckSpaceAvailable(GridPosR.Orientation orientation, GridPos3D offset, out int overlapGridCount)
            {
                overlapGridCount = 0;
                List<GridPos3D> rotatedGPs = boxOccupationData.EntityIndicatorGPs_RotatedDict[orientation];
                foreach (GridPos3D gridPos in rotatedGPs)
                {
                    GridPos3D wGP = srcBox.WorldGP + gridPos + offset;
                    foreach (GridPos3D availableGP in availableGPs)
                    {
                        if (wGP == availableGP)
                        {
                            overlapGridCount++;
                            break;
                        }
                    }
                }

                return rotatedGPs.Count == overlapGridCount;
            }

            if (!valid)
            {
                // 则堆在最顶上
            }
        }

        return matchedBoxes;
    }

    public void RemoveBoxFromGrid(Box box, bool checkDrop = true)
    {
        if (box.WorldModule)
        {
            foreach (GridPos3D offset in box.GetEntityOccupationGPs_Rotated())
            {
                GridPos3D gridWorldGP = offset + box.WorldGP;
                if (GetBoxByGridPosition(gridWorldGP, out WorldModule module, out GridPos3D localGP) == box)
                {
                    if (module[TypeDefineType.Box, localGP] == box)
                    {
                        module[TypeDefineType.Box, localGP] = null;
                    }
                }
            }

            if (checkDrop) CheckDropAbove(box);
            box.WorldModule = null;
            box.IsInGridSystem = false;

            if (!WorldManager.Instance.OtherBoxDict.ContainsKey(box.GUID))
            {
                WorldManager.Instance.OtherBoxDict.Add(box.GUID, box);
            }
        }
    }

    public void DeleteBox(Box box, bool checkDrop = true)
    {
        RemoveBoxFromGrid(box, checkDrop);
        WorldManager.Instance.OtherBoxDict.Remove(box.GUID);
        box.PoolRecycle();
    }

    public void BoxReturnToWorldFromPhysics(Box box, bool checkMerge, GridPos3D kickDir)
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
            foreach (GridPos3D offset in box.GetEntityOccupationGPs_Rotated())
            {
                GridPos3D gridWorldGP = offset + worldGP;
                Box existBox = GetBoxByGridPosition(gridWorldGP, out WorldModule module, out GridPos3D localGP);
                if (module.IsNotNullAndAvailable())
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
                case Box.States.BeingKickedToGrind:
                {
                    lerpType = Box.LerpType.KickToGrind;
                    break;
                }
                case Box.States.DroppingFromEntity:
                {
                    lerpType = Box.LerpType.DropFromEntity;
                    break;
                }
                case Box.States.Putting:
                {
                    lerpType = Box.LerpType.Put;
                    break;
                }
            }

            WorldModule module_box_core = GetModuleByWorldGP(worldGP);
            box.Initialize(worldGP, module_box_core, 0.3f, box.ArtOnly, lerpType);

            // 时序 Initialize在登记占位之前，因为Box的WorldGP是每次Initialize时变化的
            foreach (GridPos3D offset in box.GetEntityOccupationGPs_Rotated())
            {
                GridPos3D gridWorldGP = offset + worldGP;
                GetBoxByGridPosition(gridWorldGP, out WorldModule module, out GridPos3D localGP);
                module[TypeDefineType.Box, localGP] = box;
            }

            TryMerge(kickDir, new HashSet<Box> {box});
            return true;
        }
    }

    public HashSet<Box> GetBeneathBoxes(Box box)
    {
        HashSet<Box> beneathBoxes = new HashSet<Box>();
        Queue<Box> beneathBoxesQueue = new Queue<Box>();
        beneathBoxesQueue.Enqueue(box);
        while (beneathBoxesQueue.Count > 0)
        {
            Box peakBox = beneathBoxesQueue.Dequeue();
            GetBeneathBoxesCore(peakBox, beneathBoxes, beneathBoxesQueue);
        }

        return beneathBoxes;
    }

    protected void GetBeneathBoxesCore(Box box, HashSet<Box> beneathBoxes, Queue<Box> beneathBoxesQueue)
    {
        if (box.State == Box.States.Static)
        {
            foreach (GridPos3D offset in box.GetEntityOccupationGPs_Rotated())
            {
                GridPos3D gridGP = box.WorldGP + offset;
                Box beneathBox = GetBoxByGridPosition(gridGP + GridPos3D.Down, out WorldModule _, out GridPos3D _);
                if (beneathBox != null && beneathBox != box)
                {
                    bool addSuc = beneathBoxes.Add(beneathBox);
                    if (addSuc) beneathBoxesQueue.Enqueue(beneathBox);
                }
            }
        }
    }

    public bool DropBoxOnTopLayer(ushort boxTypeIndex, GridPosR.Orientation boxOrientation, GridPos3D dir, GridPos3D origin, int maxDistance, out Box dropBox)
    {
        dropBox = null;
        if (boxTypeIndex == 0) return false;
        if (BoxProject(dir, origin, maxDistance, false, out _, out Box _))
        {
            WorldModule module = GetModuleByWorldGP(origin, true);
            if (module != null)
            {
                dropBox = GameObjectPoolManager.Instance.BoxDict[boxTypeIndex].AllocateGameObject<Box>(transform);
                EntityData entityData = new EntityData(boxTypeIndex, boxOrientation);
                dropBox.Setup(entityData, module.GUID);
                dropBox.Initialize(origin, module, 0, false, Box.LerpType.DropFromAir);
                dropBox.DropFromAir();
                return true;
            }
        }

        return false;
    }

    public void CheckDropSelf(Box box)
    {
        if (box && box.Droppable && box.State != Box.States.DroppingFromAir && !box.MarkedAsMergedSourceBox)
        {
            bool spaceAvailable = true; // logically nothing beneath
            bool spaceTempAvailable = true; // there is not any entity moving/kicking beneath
            foreach (GridPos3D offset in box.GetEntityOccupationGPs_Rotated())
            {
                GridPos3D gridWorldGP = offset + box.WorldGP;
                GridPos3D beneathBoxGridWorldGP = gridWorldGP + GridPos3D.Down;

                Box boxBeneath = GetBoxByGridPosition(beneathBoxGridWorldGP, out WorldModule module, out GridPos3D localGP);
                if (!module.IsNotNullAndAvailable() || (boxBeneath != null && boxBeneath != box))
                {
                    spaceAvailable = false;
                    break;
                }
            }

            foreach (GridPos3D offset in box.GetEntityOccupationGPs_Rotated())
            {
                Vector3 gridWorldPos = offset + box.WorldGP;
                Collider[] colliders = Physics.OverlapBox(gridWorldPos + Vector3.down * 0.5f, Vector3.one * 0.4f, Quaternion.identity, LayerManager.Instance.LayerMask_BoxIndicator | LayerManager.Instance.LayerMask_HitBox_Player | LayerManager.Instance.LayerMask_HitBox_Enemy);
                foreach (Collider collider in colliders)
                {
                    Entity entityBeneath = collider.GetComponentInParent<Entity>();
                    if (entityBeneath.IsNotNullAndAlive() && entityBeneath.GUID != box.GUID)
                    {
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
                    foreach (GridPos3D offset in box.GetEntityOccupationGPs_Rotated())
                    {
                        GridPos3D gridWorldGP = offset + box.WorldGP;
                        GridPos3D beneathBoxGridWorldGP = gridWorldGP + GridPos3D.Down;
                        GetBoxByGridPosition(gridWorldGP, out WorldModule module_before, out GridPos3D boxGridLocalGP_before);
                        GetBoxByGridPosition(beneathBoxGridWorldGP, out WorldModule module_after, out GridPos3D boxGridLocalGP_after);
                        module_before[TypeDefineType.Box, boxGridLocalGP_before] = null;
                        module_after[TypeDefineType.Box, boxGridLocalGP_after] = box;
                    }

                    CheckDropAbove(box); // 递归，检查上方箱子是否坠落
                    GridPos3D boxNewWorldGP = box.WorldGP + GridPos3D.Down;
                    GetBoxByGridPosition(boxNewWorldGP, out WorldModule newModule, out GridPos3D localGP);
                    box.Initialize(boxNewWorldGP, newModule, 0.1f, box.ArtOnly, Box.LerpType.Drop);
                }
            }
        }
    }

    public void CheckDropAbove(Box box)
    {
        HashSet<Box> boxSet = new HashSet<Box>(); // 避免上方的箱子也是Mega箱子，从而导致验算两遍
        foreach (GridPos3D offset in box.GetEntityOccupationGPs_Rotated())
        {
            GridPos3D gridWorldGP = offset + box.WorldGP + GridPos3D.Up;
            Box boxAbove = GetBoxByGridPosition(gridWorldGP, out WorldModule _, out GridPos3D _);
            if (boxAbove != null && boxAbove != box && !boxSet.Contains(boxAbove))
            {
                boxSet.Add(boxAbove);
                CheckDropSelf(boxAbove);
            }
        }
    }

    #endregion

    public bool CheckIsGroundByPos(Vector3 startPos, float checkDistance, bool ignorePassableBox, out GridPos3D nearestGroundGP)
    {
        nearestGroundGP = GridPos3D.Zero;
        GridPos3D curGrid = (startPos).ToGridPos3D();
        int startGridY = curGrid.y;
        GridPos3D lowerGrid = (startPos + Vector3.down * checkDistance).ToGridPos3D();
        int endGridY = lowerGrid.y;
        if (startGridY == endGridY) return false;
        for (int y = startGridY - 1; y >= endGridY; y--)
        {
            GridPos3D grid = new GridPos3D(curGrid.x, y, curGrid.z);
            Box lowerBox = WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(grid, out WorldModule _, out GridPos3D _, false, ignorePassableBox);
            if (lowerBox != null)
            {
                nearestGroundGP = grid;
                return true;
            }
        }

        return false;
    }

    public virtual void ShutDown()
    {
        StopAllCoroutines();
    }
}
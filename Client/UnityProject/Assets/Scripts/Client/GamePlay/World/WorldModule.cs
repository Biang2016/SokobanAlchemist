using System.Collections;
using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.ObjectPool;
using FlowCanvas;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
#if UNITY_EDITOR
using UnityEditor;

#endif

public class WorldModule : PoolObject
{
    public const int MODULE_SIZE = 16;
    public World World;

    /// <summary>
    /// 按16格为一单位的坐标
    /// </summary>
    public GridPos3D ModuleGP;

    public WorldModuleData WorldModuleData;

    public WorldZoneTrigger WorldZoneTrigger;
    public WorldDeadZoneTrigger WorldDeadZoneTrigger;
    public WorldWallCollider WorldWallCollider;
    public WorldGroundCollider WorldGroundCollider;

    [ShowInInspector]
    public Dictionary<uint, Entity> WorldModuleTriggerEntities = new Dictionary<uint, Entity>(); // 所有占位格在此模组内的TriggerEntity

    [ShowInInspector]
    public Dictionary<uint, Entity> WorldModuleTriggerEntities_CoreGridInside = new Dictionary<uint, Entity>(); // 仅记录核心格在此模组内的TriggerEntity

    public List<CollectableItem> WorldModuleCollectableItems = new List<CollectableItem>();

    public List<EntityPassiveSkill_LevelEventTriggerAppear> EventTriggerAppearEntityPassiveSkillList = new List<EntityPassiveSkill_LevelEventTriggerAppear>();

    [HideInInspector]
    private Box[,,] BoxMatrix = new Box[MODULE_SIZE, MODULE_SIZE, MODULE_SIZE]; // 箱子占位矩阵

    [HideInInspector]
    private Entity[,,] EntityMatrix_CheckOverlap_BoxAndActor = new Entity[MODULE_SIZE, MODULE_SIZE, MODULE_SIZE]; // 实体占位矩阵，包括!Passable的箱子和所有Actor

    /// <summary>
    /// 此索引仅仅用于战斗时的Set，不可用于Recycle时候置空 
    /// </summary>
    /// <param name="entityType"></param>
    /// <param name="localGP"></param>
    /// <param name="isStart">是否是初始化（初始化对WorldModuleData不做修改）</param>
    /// <param name="isPartial">针对门箱子这种，虽然Indicator占位变化，但主体未变化</param>
    /// <param name="isTriggerEntity"></param>
    /// <param name="triggerEntityGUID">针对TriggerEntity，由于其占位非一一对应，故需要一个GUID来映射到Dict</param>
    /// <returns></returns>
    public Entity this[TypeDefineType entityType, GridPos3D localGP, bool isStart = false, bool isPartial = false, bool isTriggerEntity = false, uint triggerEntityGUID = 0]
    {
        get
        {
            if (isTriggerEntity)
            {
                if (WorldModuleTriggerEntities.TryGetValue(triggerEntityGUID, out Entity triggerEntity))
                {
                    return triggerEntity;
                }
            }
            else
            {
                if (entityType == TypeDefineType.Box)
                {
                    return BoxMatrix[localGP.x, localGP.y, localGP.z];
                }
                else if (entityType == TypeDefineType.Actor)
                {
                    return EntityMatrix_CheckOverlap_BoxAndActor[localGP.x, localGP.y, localGP.z];
                }
            }

            return null;
        }
        set
        {
            if (isTriggerEntity) // TriggerEntity一定要记录坐标
            {
                if (value != null)
                {
                    if (!WorldModuleTriggerEntities.ContainsKey(value.GUID))
                    {
                        WorldModuleTriggerEntities.Add(value.GUID, value);
                        bool isCore = WorldGPToLocalGP(value.WorldGP) == localGP; // 只针对核心格纪录data信息
                        if (isCore)
                        {
                            WorldModuleTriggerEntities_CoreGridInside.Add(value.GUID, value);
                            if (!isStart)
                            {
                                EntityData triggerEntityData = value.CurrentEntityData;
                                triggerEntityData.LocalGP = localGP;
                                triggerEntityData.WorldGP = LocalGPToWorldGP(localGP);
                                WorldModuleData.TriggerEntityDataDict.Add(value.GUID, triggerEntityData);
                                WorldModuleData.TriggerEntityDataList.Add(triggerEntityData);
                            }
                            else
                            {
                                if (!WorldModuleData.TriggerEntityDataDict.ContainsKey(value.GUID))
                                {
                                    WorldModuleData.TriggerEntityDataDict.Add(value.GUID, value.CurrentEntityData);
                                }
                            }
                        }
                    }
                    else
                    {
                        // 只针对核心格更新坐标
                        bool isCore = WorldGPToLocalGP(value.WorldGP) == localGP;

                        if (!isStart && isCore)
                        {
                            EntityData triggerEntityData = WorldModuleData.TriggerEntityDataDict[value.GUID];
                            triggerEntityData.LocalGP = localGP;
                            triggerEntityData.WorldGP = LocalGPToWorldGP(localGP);
                        }
                    }
                }
                else
                {
                    WorldModuleTriggerEntities.Remove(triggerEntityGUID);
                    bool isCore = WorldModuleTriggerEntities_CoreGridInside.ContainsKey(triggerEntityGUID);
                    if (isCore)
                    {
                        WorldModuleTriggerEntities_CoreGridInside.Remove(triggerEntityGUID);
                        if (!isStart)
                        {
                            if (WorldModuleData.TriggerEntityDataDict.TryGetValue(triggerEntityGUID, out EntityData triggerEntityData))
                            {
                                WorldModuleData.TriggerEntityDataDict.Remove(triggerEntityGUID);
                                WorldModuleData.TriggerEntityDataList.Remove(triggerEntityData);
                            }
                        }
                    }
                }
            }
            else
            {
                if (entityType == TypeDefineType.Box)
                {
#if UNITY_EDITOR
                    Assert.IsTrue(value == null || value is Box);
#endif
                    Box boxValue = (Box) value;
                    BoxMatrix[localGP.x, localGP.y, localGP.z] = boxValue;
                    if (value == null)
                    {
                        if (!isPartial && !isStart) WorldModuleData[TypeDefineType.Box, localGP] = null;
                        Entity entityInOverlapMatrix = EntityMatrix_CheckOverlap_BoxAndActor[localGP.x, localGP.y, localGP.z];
                        if (entityInOverlapMatrix != null && entityInOverlapMatrix is Box box && !box.Passable)
                        {
                            EntityMatrix_CheckOverlap_BoxAndActor[localGP.x, localGP.y, localGP.z] = null;
                        }
                    }
                    else
                    {
                        if (!isPartial && !isStart)
                        {
                            if (WorldGPToLocalGP(value.WorldGP) == localGP) // 只针对核心格纪录data信息
                            {
                                EntityData _entityData = WorldModuleData[TypeDefineType.Box, localGP];
                                if (_entityData != null)
                                {
                                    _entityData.EntityTypeIndex = value.EntityTypeIndex;
                                    _entityData.EntityOrientation = value.EntityOrientation;
                                    _entityData.EntityType.RefreshGUID();
                                }
                                else
                                {
                                    WorldModuleData[TypeDefineType.Box, localGP] = value.CurrentEntityData;
                                }
                            }
                            else
                            {
                                WorldModuleData[TypeDefineType.Box, localGP] = null;
                            }
                        }

                        if (!boxValue.Passable) EntityMatrix_CheckOverlap_BoxAndActor[localGP.x, localGP.y, localGP.z] = value;
                    }

                    World.RefreshActorPathFindingSpaceAvailableCache(LocalGPToWorldGP(localGP), value);
                }
                else if (entityType == TypeDefineType.Actor)
                {
                    if (value == null)
                    {
                        if (!isPartial && !isStart) WorldModuleData[TypeDefineType.Actor, localGP] = null;
                        Entity entityInOverlapMatrix = EntityMatrix_CheckOverlap_BoxAndActor[localGP.x, localGP.y, localGP.z];
                        if (entityInOverlapMatrix != null && entityInOverlapMatrix is Actor)
                        {
                            EntityMatrix_CheckOverlap_BoxAndActor[localGP.x, localGP.y, localGP.z] = null;
                        }
                    }
                    else
                    {
                        if (!isPartial && !isStart)
                        {
                            if (WorldGPToLocalGP(value.WorldGP) == localGP) // 只针对核心格纪录data信息
                            {
                                EntityData _entityData = WorldModuleData[TypeDefineType.Actor, localGP];
                                if (_entityData != null)
                                {
                                    _entityData.EntityTypeIndex = value.EntityTypeIndex;
                                    _entityData.EntityOrientation = value.EntityOrientation;
                                    _entityData.EntityType.RefreshGUID();
                                }
                                else
                                {
                                    WorldModuleData[TypeDefineType.Actor, localGP] = value.CurrentEntityData;
                                }
                            }
                            else
                            {
                                WorldModuleData[TypeDefineType.Box, localGP] = null;
                            }
                        }

                        EntityMatrix_CheckOverlap_BoxAndActor[localGP.x, localGP.y, localGP.z] = value;
                    }

                    //World.RefreshActorPathFindingSpaceAvailableCache(LocalGPToWorldGP(localGP), value); // Actor暂时不参与寻路缓存刷新
                }
            }
        }
    }

    public Actor GetActorOccupation(GridPos3D localGP)
    {
        Entity entity = EntityMatrix_CheckOverlap_BoxAndActor[localGP.x, localGP.y, localGP.z];
        if (entity != null && entity is Actor actor)
        {
            return actor;
        }

        return null;
    }

    #region Roots

    internal Transform WorldModuleBoxRoot;
    internal Transform WorldModuleTriggerRoot;
    internal Transform WorldModuleCollectableItemRoot;

    #endregion

    [SerializeField]
    protected FlowScriptController FlowScriptController;

    void Awake()
    {
        WorldModuleBoxRoot = new GameObject("WorldModuleBoxRoot").transform;
        WorldModuleBoxRoot.parent = transform;
        WorldModuleTriggerRoot = new GameObject("WorldModuleTriggerRoot").transform;
        WorldModuleTriggerRoot.parent = transform;
        WorldModuleCollectableItemRoot = new GameObject("WorldModuleCollectableItemRoot").transform;
        WorldModuleCollectableItemRoot.parent = transform;
    }

    [HideInEditorMode]
    internal bool IsGeneratingOrRecycling;

    [HideInEditorMode]
    internal bool IsIniting = false;

    public IEnumerator Clear(bool releaseWorldModuleData, int clearEntityNumPerFrame = 256)
    {
        IsGeneratingOrRecycling = true;

        // 时序，先清理注册事件，以免回收时触发
        foreach (EntityPassiveSkill_LevelEventTriggerAppear appear in EventTriggerAppearEntityPassiveSkillList)
        {
            appear.ClearAndUnRegister();
        }

        EventTriggerAppearEntityPassiveSkillList.Clear();
        foreach (KeyValuePair<uint, Entity> kv in WorldModuleTriggerEntities_CoreGridInside)
        {
            kv.Value.DestroySelfByModuleRecycle();
        }

        WorldModuleTriggerEntities_CoreGridInside.Clear();
        WorldModuleTriggerEntities.Clear();

        foreach (CollectableItem ci in WorldModuleCollectableItems)
        {
            ci.PoolRecycle();
        }

        WorldModuleCollectableItems.Clear();

        int count = 0;
        // Clear Actor First
        for (int x = 0; x < MODULE_SIZE; x++)
        {
            for (int y = 0; y < MODULE_SIZE; y++)
            {
                for (int z = 0; z < MODULE_SIZE; z++)
                {
                    Entity entity = EntityMatrix_CheckOverlap_BoxAndActor[x, y, z];
                    if (entity != null && entity is Actor actor)
                    {
                        GridPos3D worldGP = actor.WorldGP;
                        if (actor.WorldGP == LocalGPToWorldGP(new GridPos3D(x, y, z))) // 不是核心格所在的模组无权卸载该Entity
                        {
                            foreach (GridPos3D offset in actor.GetEntityOccupationGPs_Rotated())
                            {
                                GridPos3D gridWorldGP = offset + actor.WorldGP;
                                if (World.GetActorByGridPosition(gridWorldGP, out WorldModule module, out GridPos3D localGP) == actor)
                                {
                                    if (module.EntityMatrix_CheckOverlap_BoxAndActor[localGP.x, localGP.y, localGP.z] == actor)
                                    {
                                        module.EntityMatrix_CheckOverlap_BoxAndActor[localGP.x, localGP.y, localGP.z] = null;
                                    }
                                }
                            }

                            if (actor.IsRecycled)
                            {
                                Debug.Log(actor.name + " + + Duplicated Recycle" + "   " + actor.WorldGP);
                            }

                            actor.DestroySelfByModuleRecycle();
                            count++;
                            if (count > clearEntityNumPerFrame)
                            {
                                count = 0;
                                yield return null;
                            }

                            //World.RefreshActorPathFindingSpaceAvailableCache(worldGP, null);
                        }

                        EntityMatrix_CheckOverlap_BoxAndActor[x, y, z] = null;
                        //World.RefreshActorPathFindingSpaceAvailableCache(worldGP, null);
                    }
                }
            }
        }

        // Clear Box
        for (int x = 0; x < MODULE_SIZE; x++)
        {
            for (int y = 0; y < MODULE_SIZE; y++)
            {
                for (int z = 0; z < MODULE_SIZE; z++)
                {
                    Box box = BoxMatrix[x, y, z];
                    if (box != null)
                    {
                        GridPos3D worldGP = box.WorldGP;
                        if (box.WorldGP == LocalGPToWorldGP(new GridPos3D(x, y, z))) // 不是核心格所在的模组无权卸载该Box
                        {
                            foreach (GridPos3D offset in box.GetEntityOccupationGPs_Rotated())
                            {
                                GridPos3D gridWorldGP = offset + box.WorldGP;
                                if (World.GetBoxByGridPosition(gridWorldGP, 0, out WorldModule module, out GridPos3D localGP) == box)
                                {
                                    if (module.BoxMatrix[localGP.x, localGP.y, localGP.z] == box)
                                    {
                                        module.BoxMatrix[localGP.x, localGP.y, localGP.z] = null;
                                    }

                                    if (module.EntityMatrix_CheckOverlap_BoxAndActor[localGP.x, localGP.y, localGP.z] == box)
                                    {
                                        module.EntityMatrix_CheckOverlap_BoxAndActor[localGP.x, localGP.y, localGP.z] = null;
                                    }
                                }
                            }

                            box.DestroySelfByModuleRecycle();
                            count++;
                            if (count > clearEntityNumPerFrame)
                            {
                                count = 0;
                                yield return null;
                            }

                            World.RefreshActorPathFindingSpaceAvailableCache(worldGP, null);
                        }

                        BoxMatrix[x, y, z] = null;
                        EntityMatrix_CheckOverlap_BoxAndActor[x, y, z] = null;
                        World.RefreshActorPathFindingSpaceAvailableCache(worldGP, null);
                    }
                }
            }
        }

        if (!(this is OpenWorldModule))
        {
            BattleManager.Instance.OnRecycleWorldModule(WorldModuleData.GUID); // Open World Module 不回收，长存
            World.WorldData.WorldBornPointGroupData_Runtime.UnInit_UnloadModuleData(ModuleGP); // Open World Module 不回收，长存
        }

        World = null;
        if (releaseWorldModuleData) WorldModuleData.Release();
        WorldModuleData = null;

        WorldZoneTrigger.PoolRecycle();
        WorldZoneTrigger = null;
        WorldDeadZoneTrigger?.PoolRecycle();
        WorldDeadZoneTrigger = null;
        WorldWallCollider?.PoolRecycle();
        WorldWallCollider = null;
        WorldGroundCollider?.PoolRecycle();
        WorldGroundCollider = null;

        FlowScriptController.StopBehaviour();
        FlowScriptController.graph = null;
        IsGeneratingOrRecycling = false;
    }

    public virtual IEnumerator Initialize(WorldModuleData worldModuleData, GridPos3D moduleGP, World world, int loadEntityNumPerFrame)
    {
        IsIniting = true;
        IsGeneratingOrRecycling = true;
        ModuleGP = moduleGP;
        World = world;
        WorldModuleData = worldModuleData;

        WorldZoneTrigger = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.WorldZoneTrigger].AllocateGameObject<WorldZoneTrigger>(WorldModuleTriggerRoot);
        WorldZoneTrigger.name = $"{nameof(WorldZoneTrigger)}_{ModuleGP}";
        WorldZoneTrigger.Initialize(moduleGP);
        WorldZoneTrigger.WorldModule = this;

        if (WorldModuleData.WorldModuleFeature.HasFlag(WorldModuleFeature.DeadZone))
        {
            WorldDeadZoneTrigger = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.WorldDeadZoneTrigger].AllocateGameObject<WorldDeadZoneTrigger>(WorldModuleTriggerRoot);
            WorldDeadZoneTrigger.name = $"{nameof(WorldDeadZoneTrigger)}_{ModuleGP}";
            WorldDeadZoneTrigger.Initialize(moduleGP);
        }

        if (WorldModuleData.WorldModuleFeature.HasFlag(WorldModuleFeature.Wall))
        {
            WorldWallCollider = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.WorldWallCollider].AllocateGameObject<WorldWallCollider>(WorldModuleTriggerRoot);
            WorldWallCollider.name = $"{nameof(WorldWallCollider)}_{ModuleGP}";
            WorldWallCollider.Initialize(moduleGP);
        }

        if (WorldModuleData.WorldModuleFeature.HasFlag(WorldModuleFeature.Ground))
        {
            WorldGroundCollider = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.WorldGroundCollider].AllocateGameObject<WorldGroundCollider>(WorldModuleTriggerRoot);
            WorldGroundCollider.name = $"{nameof(WorldGroundCollider)}_{ModuleGP}";
            WorldGroundCollider.Initialize(moduleGP);
        }

        foreach (EntityPassiveSkill_LevelEventTriggerAppear.Data data in worldModuleData.EventTriggerAppearEntityDataList)
        {
            EntityPassiveSkill_LevelEventTriggerAppear.Data dataClone = (EntityPassiveSkill_LevelEventTriggerAppear.Data) data.Clone();
            EntityPassiveSkill_LevelEventTriggerAppear appear = dataClone.EntityPassiveSkill_LevelEventTriggerAppear;
            GridPos3D localGP = data.LocalGP;
            appear.GenerateEntityAction = () =>
            {
                BoxMatrix[localGP.x, localGP.y, localGP.z]?.DestroySelfWithoutSideEffect(); // 强行删除该格占用Box
                EntityData entityData = dataClone.EntityData.Clone();
                entityData.RemoveAllLevelEventTriggerAppearPassiveSkill();
                Entity entity = GenerateEntity(entityData, LocalGPToWorldGP(localGP), true, false);
                entity.name = entity.name + "_Generated";

                // Entity生成后此EntityPassiveSkill及注册的事件均作废
                appear.ClearAndUnRegister();
                EventTriggerAppearEntityPassiveSkillList.Remove(appear);
            };
            appear.InitWorldModuleGUID = WorldModuleData.GUID; // 这里特例处理了这个类型的PassiveSkill，因为它没有依托的Entity，所以无法从Entity中取GUID，只能外界传进去
            appear.OnRegisterLevelEventID(); // 特例不调用OnInit()
            EventTriggerAppearEntityPassiveSkillList.Add(appear);
        }

        int loadEntityCount = 0;
        foreach (KeyValuePair<TypeDefineType, int> kv in WorldModuleData.EntityDataMatrixKeys)
        {
            for (int x = 0; x < MODULE_SIZE; x++)
            {
                for (int y = 0; y < MODULE_SIZE; y++)
                {
                    for (int z = 0; z < MODULE_SIZE; z++)
                    {
                        GridPos3D localGP = new GridPos3D(x, y, z);
                        EntityData entityData = worldModuleData[kv.Key, localGP];
                        Entity entity = GenerateEntity(entityData, LocalGPToWorldGP(localGP), false, true);
                        if (entity != null)
                        {
                            loadEntityCount++;
                            if (loadEntityCount >= loadEntityNumPerFrame)
                            {
                                loadEntityCount = 0;
                                yield return null;
                            }
                        }
                    }
                }
            }
        }

        foreach (EntityData triggerEntityData in worldModuleData.TriggerEntityDataList)
        {
            GenerateEntity(triggerEntityData, LocalGPToWorldGP(triggerEntityData.LocalGP), false, true, false);
        }

        if (!string.IsNullOrWhiteSpace(worldModuleData.WorldModuleFlowAssetPath))
        {
            FlowScript flowScriptAsset = (FlowScript) Resources.Load(worldModuleData.WorldModuleFlowAssetPath);
            FlowScript flowScript = Instantiate(flowScriptAsset, transform);
            if (flowScript)
            {
                FlowScriptController.graph = flowScript;
                FlowScriptController.StartBehaviour();
            }
        }

        IsGeneratingOrRecycling = false;
        IsIniting = false;
    }

    public Entity GenerateEntity(EntityData entityData, GridPos3D worldGP, bool isTriggerAppear = false, bool isStartedEntities = false, bool findSpaceUpward = false)
    {
        if (entityData == null) return null;
        if (entityData.EntityTypeIndex == 0) return null;
        if (entityData.InitWorldModuleGUID == "") entityData.InitWorldModuleGUID = WorldModuleData.GUID;
        EntityOccupationData occupationData = ConfigManager.GetEntityOccupationData(entityData.EntityTypeIndex);
        bool isTriggerEntity = occupationData.IsTriggerEntity;

        GridPos3D localGP = WorldGPToLocalGP(worldGP);
        bool valid = true;

        {
            // Probability Check
            {
                uint probabilityShow = 100;
                foreach (EntityPassiveSkill eps in entityData.RawEntityExtraSerializeData.EntityPassiveSkills)
                {
                    if (eps is EntityPassiveSkill_ProbablyShow ps)
                    {
                        probabilityShow = ps.ShowProbabilityPercent;
                        break;
                    }
                }

                if (!probabilityShow.ProbabilityBool()) return null;
            }

            // Occupation
            List<GridPos3D> entityOccupation_rotated = null;
            EntityData frozenActorData = entityData.RawEntityExtraSerializeData.FrozenActorData;
            if (frozenActorData != null)
            {
                entityOccupation_rotated = ConfigManager.GetEntityOccupationData(frozenActorData.EntityTypeIndex).EntityIndicatorGPs_RotatedDict[frozenActorData.EntityOrientation];
            }
            else
            {
                entityOccupation_rotated = ConfigManager.GetEntityOccupationData(entityData.EntityTypeIndex).EntityIndicatorGPs_RotatedDict[entityData.EntityOrientation];
            }

            // 空位检查，if isTriggerAppear则摧毁原先箱子
            if (!isTriggerEntity)
            {
                foreach (GridPos3D offset in entityOccupation_rotated)
                {
                    GridPos3D gridWorldGP = offset + worldGP;
                    Box box = World.GetBoxByGridPosition(gridWorldGP, 0, out WorldModule module, out GridPos3D _, true, true);
                    if (box != null)
                    {
                        bool canOverlap = entityData.EntityType.TypeDefineType == TypeDefineType.Actor && box.Passable;
                        if (isTriggerAppear)
                        {
                            valid = true;
                            if (!canOverlap && box.Destroyable) box.DestroySelfWithoutSideEffect();
                        }
                        else
                        {
                            if (canOverlap) valid = true;
                            else valid = false;
                        }
                    }
                }
            }

            if (valid)
            {
                switch (entityData.EntityType.TypeDefineType)
                {
                    case TypeDefineType.Box:
                    {
                        Box box = GameObjectPoolManager.Instance.BoxDict[entityData.EntityTypeIndex].AllocateGameObject<Box>(WorldModuleBoxRoot);
                        if (box.BoxFrozenBoxHelper != null)
                        {
                            box.BoxFrozenBoxHelper.FrozenBoxOccupation = entityOccupation_rotated;
                        }

                        box.Setup(entityData, worldGP); // 覆写优先
                        box.Initialize(worldGP, this, 0, !IsAccessible, Box.LerpType.Create, false, !isTriggerAppear && !isStartedEntities); // 如果是TriggerAppear的箱子则不需要检查坠落

                        // 到模组处登记
                        foreach (GridPos3D offset in entityOccupation_rotated)
                        {
                            GridPos3D gridPos = offset + worldGP;
                            Entity existedEntity = World.GetImpassableEntityByGridPosition(gridPos, 0, out WorldModule module, out GridPos3D gridLocalGP);
                            if (module != null && (existedEntity == null || isTriggerEntity))
                            {
                                module[TypeDefineType.Box, gridLocalGP, isStartedEntities, false, isTriggerEntity, box.GUID] = box;
                            }
                        }

                        box.ApplyEntityExtraSerializeData();
                        return box;
                    }
                    case TypeDefineType.Actor:
                    {
                        bool isPlayer = entityData.EntityTypeIndex == ConfigManager.Actor_PlayerIndex;
                        if (BattleManager.Instance.Player1 != null && isPlayer) return null;
                        Actor actor = GameObjectPoolManager.Instance.ActorDict[entityData.EntityTypeIndex].AllocateGameObject<Actor>(BattleManager.Instance.ActorContainerRoot);
                        actor.Setup(entityData, worldGP);
                        if (isStartedEntities) actor.ForbidAction = !BattleManager.Instance.IsStart;
                        BattleManager.Instance.AddActor(actor);

                        // 到模组处登记
                        foreach (GridPos3D offset in entityOccupation_rotated)
                        {
                            GridPos3D gridPos = offset + actor.WorldGP; // 此处actor.WorldGP已经根据Actor的朝向更新过，不等于上文的worldGP
                            Entity existedEntity = World.GetImpassableEntityByGridPosition(gridPos, actor.GUID, out WorldModule module, out GridPos3D gridLocalGP);
                            if (module != null && (existedEntity == null || isTriggerEntity))
                            {
                                module[TypeDefineType.Actor, gridLocalGP, isStartedEntities, false, isTriggerEntity, actor.GUID] = actor;
                            }
                        }

                        actor.ApplyEntityExtraSerializeData();
                        return actor;
                    }
                }
            }
        }
        if (findSpaceUpward)
        {
            worldGP += GridPos3D.Up;
            WorldModule module = World.GetModuleByWorldGP(worldGP);
            if (module != null)
            {
                return module.GenerateEntity(entityData, worldGP, isTriggerAppear, isStartedEntities, findSpaceUpward);
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

    public IEnumerator RecordWorldModuleData(int recordEntityNumPerFrame)
    {
        IsGeneratingOrRecycling = true;

        // 时序，先清理注册事件，以免回收时触发 // todo 触发事件的记录
        //foreach (EntityPassiveSkill_LevelEventTriggerAppear appear in EventTriggerAppearEntityPassiveSkillList)
        //{
        //    appear.ClearAndUnRegister();
        //}

        //EventTriggerAppearEntityPassiveSkillList.Clear();

        foreach (KeyValuePair<uint, Entity> kv in WorldModuleTriggerEntities_CoreGridInside)
        {
            kv.Value.RecordEntityExtraSerializeData();
        }

        int count = 0;
        // Record Actor First
        for (int x = 0; x < MODULE_SIZE; x++)
        {
            for (int y = 0; y < MODULE_SIZE; y++)
            {
                for (int z = 0; z < MODULE_SIZE; z++)
                {
                    Entity entity = EntityMatrix_CheckOverlap_BoxAndActor[x, y, z];
                    if (entity != null && entity is Actor actor)
                    {
                        GridPos3D worldGP = actor.WorldGP;
                        if (actor.WorldGP == LocalGPToWorldGP(new GridPos3D(x, y, z))) // 不是核心格所在的模组无权记录该Entity
                        {
                            actor.RecordEntityExtraSerializeData();
                            count++;
                            if (count > recordEntityNumPerFrame)
                            {
                                count = 0;
                                yield return null;
                            }
                        }
                    }
                }
            }
        }

        // Record Box
        for (int x = 0; x < MODULE_SIZE; x++)
        {
            for (int y = 0; y < MODULE_SIZE; y++)
            {
                for (int z = 0; z < MODULE_SIZE; z++)
                {
                    Box box = BoxMatrix[x, y, z];
                    if (box != null)
                    {
                        GridPos3D worldGP = box.WorldGP;
                        if (box.WorldGP == LocalGPToWorldGP(new GridPos3D(x, y, z))) // 不是核心格所在的模组无权卸载该Box
                        {
                            box.RecordEntityExtraSerializeData();
                            count++;
                            if (count > recordEntityNumPerFrame)
                            {
                                count = 0;
                                yield return null;
                            }
                        }
                    }
                }
            }
        }

        IsGeneratingOrRecycling = false;
    }

    public GridPos3D WorldGPToLocalGP(GridPos3D worldGP)
    {
        return worldGP - ModuleGP * MODULE_SIZE;
    }

    public GridPos3D LocalGPToWorldGP(GridPos3D localGP)
    {
        return localGP + ModuleGP * MODULE_SIZE;
    }

    public bool IsAccessible => !WorldModuleData.WorldModuleFeature.HasFlag(WorldModuleFeature.DeadZone)
                                && !WorldModuleData.WorldModuleFeature.HasFlag(WorldModuleFeature.Wall)
                                && !WorldModuleData.WorldModuleFeature.HasFlag(WorldModuleFeature.Ground);
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (Selection.Contains(gameObject))
        {
            if (WorldModuleData != null && WorldModuleData.WorldModuleTypeIndex == ConfigManager.WorldModule_EmptyModuleIndex)
            {
                Gizmos.color = new Color(1f, 0, 0, 0.7f);
                Gizmos.DrawSphere(transform.position + Vector3.one * (MODULE_SIZE - 1) * 0.5f, 3f);
            }
        }
    }
#endif
}

public static class WorldModuleUtils
{
    public static bool IsNotNullAndAvailable(this WorldModule module)
    {
        return module != null && !module.IsRecycled && !module.IsGeneratingOrRecycling;
    }
}
using System.Collections;
using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.Messenger;
using BiangLibrary.ObjectPool;
using FlowCanvas;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

public class WorldModule : PoolObject
{
    #region GUID

    [ReadOnly]
    [HideInEditorMode]
    public uint GUID;

    private static uint guidGenerator = (uint) ConfigManager.GUID_Separator.WorldModule;

    protected uint GetGUID()
    {
        return guidGenerator++;
    }

    #endregion

    public const int MODULE_SIZE = 16;
    public World World;

    /// <summary>
    /// 按16格为一单位的坐标
    /// </summary>
    public GridPos3D ModuleGP;

    public WorldModuleData WorldModuleData;

    public WorldDeadZoneTrigger WorldDeadZoneTrigger;
    public WorldWallCollider WorldWallCollider;
    public WorldGroundCollider WorldGroundCollider;
    protected List<LevelTriggerBase> WorldModuleLevelTriggers = new List<LevelTriggerBase>();

    public List<EntityPassiveSkill_LevelEventTriggerAppear> EventTriggerAppearEntityPassiveSkillList = new List<EntityPassiveSkill_LevelEventTriggerAppear>();

    [HideInInspector]
    private Box[,,] BoxMatrix = new Box[MODULE_SIZE, MODULE_SIZE, MODULE_SIZE]; // 箱子占位矩阵

    [HideInInspector]
    private Entity[,,] EntityMatrix_CheckOverlap_BoxAndActor = new Entity[MODULE_SIZE, MODULE_SIZE, MODULE_SIZE]; // 实体占位矩阵，包括!Passable的箱子和所有Actor

    // 此索引仅仅用于战斗时的Set，不可用于Recycle时候置空 
    public Box this[GridPos3D localGP]
    {
        get { return BoxMatrix[localGP.x, localGP.y, localGP.z]; }
        set
        {
            BoxMatrix[localGP.x, localGP.y, localGP.z] = value;
            if (value == null)
            {
                WorldModuleData[TypeDefineType.Box, localGP] = null;
                Box boxInOverlapMatrix = (Box) EntityMatrix_CheckOverlap_BoxAndActor[localGP.x, localGP.y, localGP.z];
                if (boxInOverlapMatrix != null && !boxInOverlapMatrix.Passable) EntityMatrix_CheckOverlap_BoxAndActor[localGP.x, localGP.y, localGP.z] = null;
            }
            else
            {
                EntityData entityData = WorldModuleData[TypeDefineType.Box, localGP];
                if (entityData != null)
                {
                    entityData.EntityTypeIndex = value.EntityTypeIndex;
                    entityData.EntityOrientation = value.EntityOrientation;
                }
                else
                {
                    WorldModuleData[TypeDefineType.Box, localGP] = new EntityData(value.EntityTypeIndex, value.EntityOrientation); // todo 记录箱子的extraSer
                }

                if (!value.Passable) EntityMatrix_CheckOverlap_BoxAndActor[localGP.x, localGP.y, localGP.z] = value;
            }

            World.RefreshActorPathFindingSpaceAvailableCache(LocalGPToWorldGP(localGP), value);
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

    public void SetActorOccupation(GridPos3D localGP, Actor actor)
    {
        EntityMatrix_CheckOverlap_BoxAndActor[localGP.x, localGP.y, localGP.z] = actor;
    }

    #region Roots

    protected Transform WorldModuleBoxRoot;
    protected Transform WorldModuleTriggerRoot;
    protected Transform WorldModuleLevelTriggerRoot;

    #endregion

    [SerializeField]
    protected FlowScriptController FlowScriptController;

    void Awake()
    {
        WorldModuleBoxRoot = new GameObject("WorldModuleBoxRoot").transform;
        WorldModuleBoxRoot.parent = transform;
        WorldModuleTriggerRoot = new GameObject("WorldModuleTriggerRoot").transform;
        WorldModuleTriggerRoot.parent = transform;
        WorldModuleLevelTriggerRoot = new GameObject("WorldModuleLevelTriggerRoot").transform;
        WorldModuleLevelTriggerRoot.parent = transform;
    }

    [HideInEditorMode]
    public bool IsGeneratingOrRecycling;

    public IEnumerator Clear(bool releaseWorldModuleData, int clearEntityNumPerFrame = 256)
    {
        IsGeneratingOrRecycling = true;
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

                            actor.ActorBattleHelper.DestroyActor(null, true);
                            count++;
                            if (count > clearEntityNumPerFrame)
                            {
                                count = 0;
                                yield return null;
                            }

                            World.RefreshActorPathFindingSpaceAvailableCache(worldGP, null);
                        }

                        EntityMatrix_CheckOverlap_BoxAndActor[x, y, z] = null;
                        World.RefreshActorPathFindingSpaceAvailableCache(worldGP, null);
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
                                if (World.GetBoxByGridPosition(gridWorldGP, out WorldModule module, out GridPos3D localGP) == box)
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

                            box.PoolRecycle();
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

        foreach (LevelTriggerBase trigger in WorldModuleLevelTriggers)
        {
            trigger.PoolRecycle();
        }

        WorldModuleLevelTriggers.Clear();

        foreach (EntityPassiveSkill_LevelEventTriggerAppear appear in EventTriggerAppearEntityPassiveSkillList)
        {
            appear.ClearAndUnRegister();
        }

        EventTriggerAppearEntityPassiveSkillList.Clear();
        BattleManager.Instance.OnRecycleWorldModule(GUID);

        if (!(this is OpenWorldModule)) World.WorldData.WorldBornPointGroupData_Runtime.UnInit_UnloadModuleData(ModuleGP);
        World = null;
        if (releaseWorldModuleData) WorldModuleData.Release();
        WorldModuleData = null;
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
        IsGeneratingOrRecycling = true;
        GUID = GetGUID();
        ModuleGP = moduleGP;
        World = world;
        WorldModuleData = worldModuleData;
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
                BoxMatrix[localGP.x, localGP.y, localGP.z]?.DestroyBox(null, true); // 强行删除该格占用Box
                EntityData entityData = dataClone.EntityData.Clone();
                Entity entity = GenerateEntity(entityData, LocalGPToWorldGP(localGP), true, false);
                entity.name = entity.name + "_Generated";

                // Entity生成后此EntityPassiveSkill及注册的事件均作废
                appear.ClearAndUnRegister();
                EventTriggerAppearEntityPassiveSkillList.Remove(appear);
            };
            appear.InitWorldModuleGUID = GUID; // 这里特例处理了这个类型的PassiveSkill，因为它没有依托的Entity，所以无法从Entity中取GUID，只能外界传进去
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

        foreach (LevelTriggerBase.Data triggerData in worldModuleData.WorldModuleLevelTriggerGroupData.TriggerDataList)
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
    }

    public void GenerateLevelTrigger(LevelTriggerBase.Data dataClone)
    {
        LevelTriggerBase trigger = GameObjectPoolManager.Instance.LevelTriggerDict[dataClone.LevelTriggerTypeIndex].AllocateGameObject<LevelTriggerBase>(WorldModuleLevelTriggerRoot);
        trigger.InitializeInWorldModule(dataClone, GUID);
        WorldModuleLevelTriggers.Add(trigger);
    }

    public Entity GenerateEntity(EntityData entityData, GridPos3D worldGP, bool isTriggerAppear = false, bool isStartedEntities = false, bool findSpaceUpward = false, List<GridPos3D> overrideOccupation = null)
    {
        if (entityData == null) return null;
        if (entityData.EntityTypeIndex == 0) return null;

        GridPos3D localGP = WorldGPToLocalGP(worldGP);
        bool valid = true;
        if (BoxMatrix[localGP.x, localGP.y, localGP.z] == null)
        {
            // Probability Check
            if (entityData.RawEntityExtraSerializeData != null)
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

            List<GridPos3D> boxOccupation_rotated = null;
            if (overrideOccupation != null)
            {
                boxOccupation_rotated = overrideOccupation;
            }
            else
            {
                boxOccupation_rotated = ConfigManager.GetEntityOccupationData(entityData.EntityTypeIndex).EntityIndicatorGPs_RotatedDict[entityData.EntityOrientation];
            }

            // 空位检查，if isTriggerAppear则摧毁原先箱子
            foreach (GridPos3D offset in boxOccupation_rotated)
            {
                GridPos3D gridPos = offset + localGP;
                if (gridPos.InsideModule())
                {
                    Box box = BoxMatrix[gridPos.x, gridPos.y, gridPos.z];
                    if (box != null)
                    {
                        if (isTriggerAppear)
                        {
                            valid = true;
                            box.DestroyBox(null, true);
                        }
                        else
                        {
                            valid = false;
                        }
                    }
                }
                else // 如果是大型实体则需要考虑一部分是否放到了其他模组里
                {
                    GridPos3D gridWorldGP = offset + worldGP;
                    Box boxInOtherModule = World.GetBoxByGridPosition(gridWorldGP, out WorldModule otherModule, out GridPos3D _);
                    if (boxInOtherModule != null)
                    {
                        if (isTriggerAppear)
                        {
                            valid = true;
                            boxInOtherModule.DestroyBox(null, true);
                        }
                        else
                        {
                            valid = false;
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
                            box.BoxFrozenBoxHelper.FrozenBoxOccupation = boxOccupation_rotated;
                        }

                        box.Setup(entityData, GUID);
                        box.Initialize(worldGP, this, 0, !IsAccessible, Box.LerpType.Create, false, !isTriggerAppear && !isStartedEntities); // 如果是TriggerAppear的箱子则不需要检查坠落

                        foreach (GridPos3D offset in boxOccupation_rotated)
                        {
                            GridPos3D gridPos = offset + localGP;
                            if (gridPos.InsideModule())
                            {
                                if (isStartedEntities)
                                {
                                    BoxMatrix[gridPos.x, gridPos.y, gridPos.z] = box;
                                    World.RefreshActorPathFindingSpaceAvailableCache(LocalGPToWorldGP(gridPos), box);
                                }
                                else
                                {
                                    this[gridPos] = box;
                                }
                            }
                            else // 如果合成的是异形箱子则需要考虑该箱子的一部分是否放到了其他模组里
                            {
                                GridPos3D gridWorldGP = offset + worldGP;
                                Box boxInOtherModule = World.GetBoxByGridPosition(gridWorldGP, out WorldModule otherModule, out GridPos3D otherModuleLocalGP);
                                if (otherModule != null && boxInOtherModule == null)
                                {
                                    if (isStartedEntities)
                                    {
                                        otherModule.BoxMatrix[otherModuleLocalGP.x, otherModuleLocalGP.y, otherModuleLocalGP.z] = box;
                                    }
                                    else
                                    {
                                        otherModule[otherModuleLocalGP] = box;
                                    }
                                }
                            }
                        }

                        return box;
                    }
                    case TypeDefineType.Actor:
                    {
                        Actor actor = GameObjectPoolManager.Instance.ActorDict[entityData.EntityTypeIndex].AllocateGameObject<Actor>(BattleManager.Instance.ActorContainerRoot);
                        GridPos3D.ApplyGridPosToLocalTrans(worldGP, actor.transform, 1);
                        actor.WorldGP = worldGP;
                        actor.Setup(entityData, ActorCategory.Creature, GUID);
                        BattleManager.Instance.AddActor(this, actor);
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
            if (WorldModuleData != null && WorldModuleData.WorldModuleTypeIndex == ConfigManager.WorldModule_DeadZoneIndex)
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
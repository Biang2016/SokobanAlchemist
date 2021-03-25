﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BiangLibrary.GameDataFormat;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.GamePlay.UI;
using Sirenix.OdinInspector;
using UnityEngine;

public class OpenWorld : World
{
    private LoadingMapPanel LoadingMapPanel;
    public int WorldSize_X = 2;
    public int WorldSize_Z = 2;

    public int PlayerScopeRadiusX = 2;
    public int PlayerScopeRadiusZ = 2;

    public bool UseCertainSeed = false;

    public bool TestTerrain = false;

    [ShowIf("UseCertainSeed")]
    public uint GivenSeed = 0;

    public ushort[,,] WorldMap; // 地图元素放置  Y轴缩小16
    public EntityExtraSerializeData[,,] WorldMap_EntityExtraSerializeData; // 地图元素放置  Y轴缩小16
    public GridPosR.Orientation[,,] WorldMapOrientation; // 地图元素放置朝向  Y轴缩小16
    public ushort[,,] WorldMap_Occupied; // 地图元素占位  Y轴缩小16
    public ushort[,,] WorldMap_StaticLayoutOccupied_IntactForStaticLayout; // 地图静态布局占位，禁止其他静态布局影响  Y轴缩小16
    public ushort[,,] WorldMap_StaticLayoutOccupied_IntactForBox; // 地图静态布局占位，禁止其他箱子影响  Y轴缩小16
    public TerrainType[,] WorldMap_TerrainType; // 地图地形分布

    [BoxGroup("@\"起始MicroWorld\t\"+StartMicroWorldTypeName")]
    [HideLabel]
    public TypeSelectHelper StartMicroWorldTypeName = new TypeSelectHelper {TypeDefineType = TypeDefineType.World};

    internal GridPos3D InitialPlayerBP = GridPos3D.Zero;

    #region GenerateLayerData

    [BoxGroup("地形生成配置")]
    [HideLabel]
    public GenerateTerrainData m_GenerateTerrainData = new GenerateTerrainData();

    [FoldoutGroup("地图生成")]
    [LabelText("静态布局层级配置")]
    [ListDrawerSettings(ListElementLabelName = "StaticLayoutTypeName")]
    public List<GenerateStaticLayoutLayerData> GenerateStaticLayoutLayerDataList = new List<GenerateStaticLayoutLayerData>();

    [FoldoutGroup("放置物")]
    [LabelText("Box层级配置")]
    [ListDrawerSettings(ListElementLabelName = "BoxTypeName")]
    public List<GenerateBoxLayerData> GenerateBoxLayerDataList = new List<GenerateBoxLayerData>();

    [FoldoutGroup("地图生成")]
    [LabelText("Actor层级配置")]
    [ListDrawerSettings(ListElementLabelName = "ActorTypeName")]
    public List<GenerateActorLayerData> GenerateActorLayerDataList = new List<GenerateActorLayerData>();

    #endregion

    public const string PLAYER_DEFAULT_BP = "OpenWorldPlayerBP";

    public override void OnRecycled()
    {
        base.OnRecycled();
        WorldMap = null;
        WorldMap_EntityExtraSerializeData = null;
        WorldMapOrientation = null;
        WorldMap_Occupied = null;
        WorldMap_StaticLayoutOccupied_IntactForStaticLayout = null;
        WorldMap_StaticLayoutOccupied_IntactForBox = null;
        WorldMap_TerrainType = null;
        IsInsideMicroWorld = false;
        InitialPlayerBP = GridPos3D.Zero;
        transportingPlayerToMicroWorld = false;
        returningToOpenWorldFormMicroWorld = false;
        restartingMicroWorld = false;
    }

    public override IEnumerator Initialize(WorldData worldData)
    {
        LoadingMapPanel = UIManager.Instance.GetBaseUIForm<LoadingMapPanel>();
        AudioManager.Instance.BGMFadeIn("bgm/CoolSwing", 1f, 1f, true);

        uint Seed = 0;
        if (UseCertainSeed)
        {
            Seed = GivenSeed;
        }
        else
        {
            Seed = (uint) Time.time.ToString().GetHashCode();
        }

        SRandom SRandom = new SRandom(Seed);

        WorldMap = new ushort[WorldSize_X * WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldSize_Z * WorldModule.MODULE_SIZE];
        WorldMap_EntityExtraSerializeData = new EntityExtraSerializeData[WorldSize_X * WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldSize_Z * WorldModule.MODULE_SIZE];
        WorldMapOrientation = new GridPosR.Orientation[WorldSize_X * WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldSize_Z * WorldModule.MODULE_SIZE];
        WorldMap_Occupied = new ushort[WorldSize_X * WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldSize_Z * WorldModule.MODULE_SIZE];
        WorldMap_StaticLayoutOccupied_IntactForStaticLayout = new ushort[WorldSize_X * WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldSize_Z * WorldModule.MODULE_SIZE];
        WorldMap_StaticLayoutOccupied_IntactForBox = new ushort[WorldSize_X * WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldSize_Z * WorldModule.MODULE_SIZE];
        WorldMap_TerrainType = new TerrainType[WorldSize_X * WorldModule.MODULE_SIZE, WorldSize_Z * WorldModule.MODULE_SIZE];

        WorldGUID = Seed + "_" + Guid.NewGuid().ToString("P"); // e.g: (ade24d16-db0f-40af-8794-1e08e2040df3);
        m_LevelCacheData = new LevelCacheData();
        WorldData = worldData;

        WorldData.WorldBornPointGroupData_Runtime.InitTempData();
        WorldData.DefaultWorldActorBornPointAlias = PLAYER_DEFAULT_BP;

        ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.5f, "Loading Open World");
        yield return null;

        #region GenerateTerrain

        m_LevelCacheData.TerrainGenerator = new CellularAutomataTerrainGenerator(m_GenerateTerrainData, WorldModule.MODULE_SIZE * WorldSize_X, WorldModule.MODULE_SIZE * WorldSize_Z, SRandom.Next((uint) 9999), this);
        m_LevelCacheData.TerrainGenerator.ApplyToWorldTerrainMap();

        #endregion

        #region GenerateStaticLayoutLayer

        int generatorCount_staticLayoutLayer = 0;

        foreach (GenerateStaticLayoutLayerData staticLayoutLayerData in GenerateStaticLayoutLayerDataList) // 按层生成关卡静态布局数据
        {
            if (!staticLayoutLayerData.Enable) continue;
            if (!staticLayoutLayerData.DeterminePlayerBP && TestTerrain) continue;
            staticLayoutLayerData.Init();
            MapGenerator generator = null;
            switch (staticLayoutLayerData.m_GenerateAlgorithm)
            {
                case GenerateAlgorithm.Random:
                {
                    generator = new RandomMapGenerator(staticLayoutLayerData, WorldModule.MODULE_SIZE * WorldSize_X, WorldModule.MODULE_SIZE * WorldSize_Z, SRandom.Next((uint) 9999), this);
                    break;
                }
            }

            if (generator != null)
            {
                generator.ApplyToWorldMap();
                m_LevelCacheData.CurrentGenerator_StaticLayouts.Add(generator);
                generatorCount_staticLayoutLayer++;
                ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.5f + 0.1f * generatorCount_staticLayoutLayer / GenerateStaticLayoutLayerDataList.Count, "Generating Map Static Layouts");
                yield return null;
            }
        }

        ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.6f, "Generating Map Static Layouts Completed");
        yield return null;

        #endregion

        #region GenerateBoxLayer

        int generatorCount_boxLayer = 0;
        GridPos3D playerBPWorld = new GridPos3D(-1, -1, -1);

        foreach (GenerateBoxLayerData boxLayerData in GenerateBoxLayerDataList) // 按层生成关卡Box数据
        {
            if (!boxLayerData.Enable) continue;
            if (TestTerrain) continue;
            boxLayerData.Init();
            MapGenerator generator = null;
            switch (boxLayerData.m_GenerateAlgorithm)
            {
                case GenerateAlgorithm.CellularAutomata:
                {
                    generator = new CellularAutomataMapGenerator(boxLayerData, WorldModule.MODULE_SIZE * WorldSize_X, WorldModule.MODULE_SIZE * WorldSize_Z, SRandom.Next((uint) 9999), this);
                    break;
                }
                case GenerateAlgorithm.PerlinNoise:
                {
                    break;
                }
                case GenerateAlgorithm.Random:
                {
                    generator = new RandomMapGenerator(boxLayerData, WorldModule.MODULE_SIZE * WorldSize_X, WorldModule.MODULE_SIZE * WorldSize_Z, SRandom.Next((uint) 9999), this);
                    break;
                }
            }

            if (generator != null)
            {
                generator.ApplyToWorldMap();
                m_LevelCacheData.CurrentGenerators_Boxes.Add(generator);
                generatorCount_boxLayer++;
                ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.6f + 0.2f * generatorCount_boxLayer / GenerateBoxLayerDataList.Count, "Generating Map Boxes");
                yield return null;
            }
        }

        ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.8f, "Generating Map Boxes Completed");
        yield return null;

        #endregion

        #region GenerateActorLayer

        int generatorCount_actorLayer = 0;
        foreach (GenerateActorLayerData actorLayerData in GenerateActorLayerDataList) // 按层生成关卡Actor数据
        {
            if (!actorLayerData.Enable) continue;
            if (TestTerrain) continue;
            actorLayerData.Init();
            MapGenerator generator = null;
            switch (actorLayerData.m_GenerateAlgorithm)
            {
                case GenerateAlgorithm.Random:
                {
                    generator = new RandomMapGenerator(actorLayerData, WorldModule.MODULE_SIZE * WorldSize_X, WorldModule.MODULE_SIZE * WorldSize_Z, SRandom.Next((uint) 9999), this);
                    break;
                }
            }

            if (generator != null)
            {
                generator.ApplyToWorldMap();
                m_LevelCacheData.CurrentGenerators_Actors.Add(generator);
                generatorCount_actorLayer++;
                ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.8f + 0.1f * generatorCount_actorLayer / GenerateActorLayerDataList.Count, "Generating Map Actors");
                yield return null;
            }
        }

        ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.9f, "Generating Map Actors Completed");
        yield return null;

        #endregion

        ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.91f, "Loading Maps");
        yield return null;
        BattleManager.Instance.CreateActorByBornPointData(WorldData.WorldBornPointGroupData_Runtime.PlayerBornPointDataAliasDict[WorldData.DefaultWorldActorBornPointAlias]);
        BattleManager.Instance.Player1.ForbidAction = true;
        CameraManager.Instance.FieldCamera.InitFocus();

        // 没有起始关卡就直接进入大世界，有起始关卡则避免加载了又卸载
        ushort startMicroWorldTypeIndex = ConfigManager.GetTypeIndex(TypeDefineType.World, StartMicroWorldTypeName.TypeName);
        if (startMicroWorldTypeIndex == 0)
        {
            RefreshScopeModulesCoroutine = StartCoroutine(RefreshScopeModules(playerBPWorld, PlayerScopeRadiusX, PlayerScopeRadiusZ)); // 按关卡生成器和角色位置初始化需要的模组
            while (RefreshScopeModulesCoroutine != null) yield return null;

            BattleManager.Instance.Player1.ForbidAction = false;
        }
    }

    public IEnumerator OnAfterInitialize()
    {
        ushort startMicroWorldTypeIndex = ConfigManager.GetTypeIndex(TypeDefineType.World, StartMicroWorldTypeName.TypeName);
        if (startMicroWorldTypeIndex != 0)
        {
            yield return Co_TransportPlayerToMicroWorld(startMicroWorldTypeIndex);
        }

        yield return null;
    }

    protected override IEnumerator GenerateWorldModuleByCustomizedData(WorldModuleData data, int x, int y, int z, int loadBoxNumPerFrame)
    {
        m_LevelCacheData.CurrentShowModuleGPs.Add(new GridPos3D(x, y, z));
        yield return base.GenerateWorldModuleByCustomizedData(data, x, y, z, loadBoxNumPerFrame);
    }

    #region Level Streaming

    public class LevelCacheData
    {
        public CellularAutomataTerrainGenerator TerrainGenerator;
        public List<MapGenerator> CurrentGenerator_StaticLayouts = new List<MapGenerator>(); // 按静态布局layer记录的地图生成信息，未走过的地图或离开很久之后重置的模组按这个数据加载出来
        public List<MapGenerator> CurrentGenerators_Boxes = new List<MapGenerator>(); // 按箱子layer记录的地图生成信息，未走过的地图或离开很久之后重置的模组按这个数据加载出来
        public List<MapGenerator> CurrentGenerators_Actors = new List<MapGenerator>(); // 按角色layer记录的生成信息，未走过的地图或离开很久之后重置的模组按这个数据加载出来

        public Dictionary<GridPos3D, WorldModuleData> WorldModuleDataDict = new Dictionary<GridPos3D, WorldModuleData>();

        public List<GridPos3D> CurrentShowModuleGPs = new List<GridPos3D>();
    }

    private LevelCacheData m_LevelCacheData;

    private Coroutine RefreshScopeModulesCoroutine;

    void FixedUpdate()
    {
        if (!IsRecycled)
        {
            if (GameStateManager.Instance.GetState() == GameState.Fighting && !IsInsideMicroWorld)
            {
                if (RefreshScopeModulesCoroutine == null)
                {
                    RefreshScopeModulesCoroutine = StartCoroutine(RefreshScopeModules(BattleManager.Instance.Player1.WorldGP, PlayerScopeRadiusX, PlayerScopeRadiusZ));
                }
            }
        }
        else
        {
            StopAllCoroutines();
        }
    }

    List<bool> generateModuleFinished = new List<bool>();
    List<bool> recycleModuleFinished = new List<bool>();

    Plane[] cachedPlanes = new Plane[6];
    public float ExtendScope_Load = 4f; //提前加载
    public float ExtendScope_Recycle = 20; // 推迟回收
    public float ExtendScope_Recycle_Ground = 32; // 推迟回收

    public IEnumerator RefreshScopeModules(GridPos3D playerWorldGP, int scopeX, int scopeZ)
    {
        GridPos3D playerOnModuleGP = GetModuleGPByWorldGP(playerWorldGP);

        #region Generate Modules

        bool CheckModuleCanBeSeenByCamera(GridPos3D moduleGP, bool checkBottom, bool checkTop, float extendScope)
        {
            if (IsInsideMicroWorld) return moduleGP.y >= WORLD_HEIGHT / 2; // 当角色在解谜关卡时，隐藏其它模组，且恒定显示解谜模组 todo 此处粗暴地使用了模组坐标来判断是否在解谜关卡，未来需要处理
            GeometryUtility.CalculateFrustumPlanes(CameraManager.Instance.MainCamera, cachedPlanes);
            if (checkBottom && checkTop)
            {
                Bounds bounds = new Bounds(moduleGP * WorldModule.MODULE_SIZE + (WorldModule.MODULE_SIZE - 1) / 2f * Vector3.one, Vector3.one * (WorldModule.MODULE_SIZE + extendScope));
                return GeometryUtility.TestPlanesAABB(cachedPlanes, bounds);
            }
            else
            {
                if (checkBottom)
                {
                    Bounds bounds = new Bounds(moduleGP * WorldModule.MODULE_SIZE + (WorldModule.MODULE_SIZE - 1) / 2f * new Vector3(1, 0, 1), new Vector3(WorldModule.MODULE_SIZE + extendScope, 1, WorldModule.MODULE_SIZE + extendScope));
                    return GeometryUtility.TestPlanesAABB(cachedPlanes, bounds);
                }
                else if (checkTop)
                {
                    Bounds bounds = new Bounds(moduleGP * WorldModule.MODULE_SIZE + (WorldModule.MODULE_SIZE - 1) / 2f * new Vector3(1, 2, 1) + Vector3.up, new Vector3(WorldModule.MODULE_SIZE + extendScope, 1, WorldModule.MODULE_SIZE + extendScope));
                    return GeometryUtility.TestPlanesAABB(cachedPlanes, bounds);
                }
                else
                {
                    return false;
                }
            }
        }

        generateModuleFinished.Clear();
        for (int module_x = playerOnModuleGP.x - (scopeX - 1); module_x <= playerOnModuleGP.x + (scopeX - 1); module_x++)
        for (int module_z = playerOnModuleGP.z - (scopeZ - 1); module_z <= playerOnModuleGP.z + (scopeZ - 1); module_z++)
        {
            if (module_x >= 0 && module_x < WorldSize_X && module_z >= 0 && module_z < WorldSize_Z)
            {
                if (!CheckModuleCanBeSeenByCamera(new GridPos3D(module_x, 1, module_z), true, false, ExtendScope_Load)) continue;

                // Ground Modules
                WorldModule groundModule = WorldModuleMatrix[module_x, 0, module_z];
                if (groundModule == null)
                {
                    GridPos3D targetModuleGP = new GridPos3D(module_x, 0, module_z);
                    WorldModuleData moduleData = WorldModuleData.WorldModuleDataFactory.Alloc();
                    moduleData.BoxMatrix[0, 15, 0] = ConfigManager.Box_CombinedGroundBoxIndex;
                    moduleData.RawBoxMatrix[0, 15, 0] = ConfigManager.Box_CombinedGroundBoxIndex;

                    moduleData.WorldModuleFeature = WorldModuleFeature.Ground;
                    moduleData.InitOpenWorldModuleData(true);

                    generateModuleFinished.Add(false);
                    StartCoroutine(Co_GenerateModule(moduleData, targetModuleGP, generateModuleFinished.Count - 1));
                }

                // Other Modules (以WorldMap为模板，将diff应用上去)
                WorldModule module = WorldModuleMatrix[module_x, 1, module_z];
                if (module == null)
                {
                    WorldModuleData moduleData = null;
                    GridPos3D targetModuleGP = new GridPos3D(module_x, 1, module_z);
                    if (!m_LevelCacheData.WorldModuleDataDict.TryGetValue(targetModuleGP, out moduleData))
                    {
                        // 从未加载过的模组，通过generator来计算
                        moduleData = WorldModuleData.WorldModuleDataFactory.Alloc();
                        moduleData.InitOpenWorldModuleData(true);
                        m_LevelCacheData.WorldModuleDataDict.Add(targetModuleGP, moduleData);
                        for (int world_x = targetModuleGP.x * WorldModule.MODULE_SIZE; world_x < (targetModuleGP.x + 1) * WorldModule.MODULE_SIZE; world_x++)
                        for (int world_y = targetModuleGP.y * WorldModule.MODULE_SIZE; world_y < (targetModuleGP.y + 1) * WorldModule.MODULE_SIZE; world_y++)
                        for (int world_z = targetModuleGP.z * WorldModule.MODULE_SIZE; world_z < (targetModuleGP.z + 1) * WorldModule.MODULE_SIZE; world_z++)
                        {
                            int local_x = world_x - targetModuleGP.x * WorldModule.MODULE_SIZE;
                            int local_y = world_y - targetModuleGP.y * WorldModule.MODULE_SIZE;
                            int local_z = world_z - targetModuleGP.z * WorldModule.MODULE_SIZE;
                            ushort existedIndex = WorldMap[world_x, world_y - WorldModule.MODULE_SIZE, world_z];
                            ConfigManager.TypeStartIndex indexType = existedIndex.ConvertToTypeStartIndex();
                            switch (indexType)
                            {
                                case ConfigManager.TypeStartIndex.Box:
                                {
                                    moduleData.RawBoxMatrix[local_x, local_y, local_z] = existedIndex;
                                    moduleData.BoxMatrix[local_x, local_y, local_z] = existedIndex;
                                    moduleData.RawBoxOrientationMatrix[local_x, local_y, local_z] = WorldMapOrientation[world_x, world_y - WorldModule.MODULE_SIZE, world_z];
                                    moduleData.BoxOrientationMatrix[local_x, local_y, local_z] = WorldMapOrientation[world_x, world_y - WorldModule.MODULE_SIZE, world_z];
                                    moduleData.BoxExtraSerializeDataMatrix[local_x, local_y, local_z] = WorldMap_EntityExtraSerializeData[world_x, world_y - WorldModule.MODULE_SIZE, world_z];
                                    break;
                                }
                                case ConfigManager.TypeStartIndex.Enemy:
                                {
                                    string enemyTypeName = ConfigManager.GetTypeName(TypeDefineType.Enemy, existedIndex);
                                    EntityExtraSerializeData actorExtraData = WorldMap_EntityExtraSerializeData[world_x, world_y - WorldModule.MODULE_SIZE, world_z];
                                    moduleData.WorldModuleBornPointGroupData.EnemyBornPoints.Add(
                                        new BornPointData
                                        {
                                            EnemyType = new TypeSelectHelper {TypeDefineType = TypeDefineType.Enemy, TypeSelection = enemyTypeName},
                                            LocalGP = new GridPos3D(local_x, local_y, local_z),
                                            BornPointAlias = "",
                                            WorldGP = new GridPos3D(world_x, world_y, world_z),
                                            RawEntityExtraSerializeData = actorExtraData
                                        });
                                    break;
                                }
                            }
                        }

                        WorldData.WorldBornPointGroupData_Runtime.Init_LoadModuleData(targetModuleGP, moduleData);
                    }

                    // 加载模组修改
                    WorldModuleDataModification modification = WorldModuleDataModification.LoadData(targetModuleGP);
                    if (modification != null)
                    {
                        foreach (KeyValuePair<GridPos3D, WorldModuleDataModification.BoxModification> kv in modification.BoxModificationDict)
                        {
                            // 此部分不可使用Module[x,y,z]来赋值，否则又会触发Modification
                            moduleData.BoxMatrix[kv.Key.x, kv.Key.y, kv.Key.z] = kv.Value.BoxTypeIndex;
                            moduleData.BoxOrientationMatrix[kv.Key.x, kv.Key.y, kv.Key.z] = kv.Value.EntityOrientation;
                        }

                        moduleData.Modification.Release();
                        moduleData.Modification = modification;
                    }

                    generateModuleFinished.Add(false);
                    StartCoroutine(Co_GenerateModule(moduleData, targetModuleGP, generateModuleFinished.Count - 1));
                }
            }
        }

        while (true)
        {
            bool allFinished = true;
            foreach (bool b in generateModuleFinished)
            {
                if (!b) allFinished = false;
            }

            if (allFinished) break;
            else yield return null;
        }

        generateModuleFinished.Clear();

        #endregion

        #region Recycle Modules

        recycleModuleFinished.Clear();
        List<GridPos3D> hideModuleGPs = new List<GridPos3D>();
        foreach (GridPos3D currentShowModuleGP in m_LevelCacheData.CurrentShowModuleGPs)
        {
            bool isGround = currentShowModuleGP.y == 0;
            if (CheckModuleCanBeSeenByCamera(currentShowModuleGP, !isGround, isGround, isGround ? ExtendScope_Recycle_Ground : ExtendScope_Recycle)) continue;
            WorldModule worldModule = WorldModuleMatrix[currentShowModuleGP.x, currentShowModuleGP.y, currentShowModuleGP.z];
            if (worldModule != null)
            {
                recycleModuleFinished.Add(false);
                StartCoroutine(Co_RecycleModule(worldModule, currentShowModuleGP, recycleModuleFinished.Count - 1));
            }

            hideModuleGPs.Add(currentShowModuleGP);
        }

        while (true)
        {
            bool allFinished = true;
            foreach (bool b in recycleModuleFinished)
            {
                if (!b) allFinished = false;
            }

            if (allFinished) break;
            else
            {
                LoadingMapPanel.Refresh();
                yield return null;
            }
        }

        recycleModuleFinished.Clear();

        foreach (GridPos3D hideModuleGP in hideModuleGPs)
        {
            m_LevelCacheData.CurrentShowModuleGPs.Remove(hideModuleGP);
        }

        yield return null;

        #endregion

        RefreshScopeModulesCoroutine = null;
    }

    IEnumerator Co_RecycleModule(WorldModule worldModule, GridPos3D currentShowModuleGP, int boolIndex)
    {
        WorldData.WorldBornPointGroupData_Runtime.Dynamic_UnloadModuleData(currentShowModuleGP);
        worldModule.WorldModuleData.Modification?.SaveData(worldModule.ModuleGP);
        WorldModuleMatrix[currentShowModuleGP.x, currentShowModuleGP.y, currentShowModuleGP.z] = null;
        yield return worldModule.Clear(false, 256);
        worldModule.PoolRecycle();
        //m_LevelCacheData.WorldModuleDataDict.Remove(currentShowModuleGP);
        if (boolIndex >= 0)
        {
            if (boolIndex >= recycleModuleFinished.Count)
            {
                Debug.Log(recycleModuleFinished.Count);
            }

            recycleModuleFinished[boolIndex] = true;
        }
    }

    IEnumerator Co_GenerateModule(WorldModuleData moduleData, GridPos3D targetModuleGP, int boolIndex)
    {
        yield return GenerateWorldModuleByCustomizedData(moduleData, targetModuleGP.x, targetModuleGP.y, targetModuleGP.z, 32);
        yield return WorldData.WorldBornPointGroupData_Runtime.Dynamic_LoadModuleData(targetModuleGP);
        if (boolIndex >= 0)
        {
            if (boolIndex >= generateModuleFinished.Count)
            {
                Debug.Log(generateModuleFinished.Count);
            }

            generateModuleFinished[boolIndex] = true;
        }
    }

    #endregion

    #region MicroWorld

    internal bool IsInsideMicroWorld = false;
    internal ushort CurrentMicroWorldTypeIndex = 0;
    internal GridPos3D LastLeaveOpenWorldPlayerGP = GridPos3D.Zero;
    private List<WorldModule> MicroWorldModules = new List<WorldModule>();

    public void TransportPlayerToMicroWorld(ushort worldTypeIndex)
    {
        if (transportingPlayerToMicroWorld) return;
        if (returningToOpenWorldFormMicroWorld) return;
        if (restartingMicroWorld) return;
        StartCoroutine(Co_TransportPlayerToMicroWorld(worldTypeIndex));
    }

    private bool transportingPlayerToMicroWorld = false;

    IEnumerator Co_TransportPlayerToMicroWorld(ushort worldTypeIndex)
    {
        transportingPlayerToMicroWorld = true;
        CurrentMicroWorldTypeIndex = worldTypeIndex;
        BattleManager.Instance.Player1.ForbidAction = true;
        UIManager.Instance.ShowUIForms<LoadingMapPanel>();
        LoadingMapPanel.Clear();
        LoadingMapPanel.SetMinimumLoadingDuration(2);
        LoadingMapPanel.SetBackgroundAlpha(1);
        LoadingMapPanel.SetProgress(0, "Loading Level");

        WorldData microWorldData = ConfigManager.GetWorldDataConfig(worldTypeIndex);
        GridPos3D transportPlayerBornPoint = GridPos3D.Zero;

        // Recycling Micro World Modules
        if (IsInsideMicroWorld)
        {
            int totalRecycleModuleNumber = MicroWorldModules.Count;
            int recycledModuleCount = 0;
            foreach (WorldModule microWorldModule in MicroWorldModules)
            {
                yield return Co_RecycleModule(microWorldModule, microWorldModule.ModuleGP, -1);
                recycledModuleCount++;
                LoadingMapPanel.SetProgress(20f * recycledModuleCount / totalRecycleModuleNumber, "Destroying the dungeon");
            }

            MicroWorldModules.Clear();
        }

        // Loading Micro World Modules
        int totalModuleNum = microWorldData.WorldModuleGPOrder.Count;
        int loadingModuleCount = 0;
        while (RefreshScopeModulesCoroutine != null) yield return null;
        foreach (GridPos3D worldModuleGP in microWorldData.WorldModuleGPOrder)
        {
            ushort worldModuleTypeIndex = microWorldData.ModuleMatrix[worldModuleGP.x, worldModuleGP.y, worldModuleGP.z];
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
                    yield return GenerateWorldModule(worldModuleTypeIndex, realModuleGP.x, realModuleGP.y, realModuleGP.z);
                    WorldModule module = WorldModuleMatrix[realModuleGP.x, realModuleGP.y, realModuleGP.z];
                    MicroWorldModules.Add(module);
                    WorldData.WorldBornPointGroupData_Runtime.Init_LoadModuleData(realModuleGP, module.WorldModuleData);
                    yield return WorldData.WorldBornPointGroupData_Runtime.Dynamic_LoadModuleData(realModuleGP);
                    SortedDictionary<string, BornPointData> playerBornPoints = module.WorldModuleData.WorldModuleBornPointGroupData.PlayerBornPoints;
                    if (playerBornPoints.Count > 0)
                    {
                        if (transportPlayerBornPoint == GridPos3D.Zero) transportPlayerBornPoint = module.LocalGPToWorldGP(playerBornPoints[playerBornPoints.Keys.ToList()[0]].LocalGP);
                    }

                    List<BornPointData> moduleBPData = WorldData.WorldBornPointGroupData_Runtime.TryLoadModuleBPData(realModuleGP);
                    if (moduleBPData != null)
                    {
                        foreach (BornPointData bp in moduleBPData)
                        {
                            if (bp.ActorCategory == ActorCategory.Player)
                            {
                                string playerBPAlias = module.WorldModuleData.WorldModuleTypeName;
                                if (!WorldData.WorldBornPointGroupData_Runtime.PlayerBornPointDataAliasDict.ContainsKey(playerBPAlias))
                                {
                                    WorldData.WorldBornPointGroupData_Runtime.PlayerBornPointDataAliasDict.Add(playerBPAlias, bp);
                                }
                            }
                        }
                    }
                }
            }

            loadingModuleCount++;
            LoadingMapPanel.SetProgress(20f + 60f * loadingModuleCount / totalModuleNum, "Loading Level");
        }

        if (transportPlayerBornPoint == GridPos3D.Zero)
        {
            Debug.LogWarning("传送的模组没有默认玩家出生点");
        }

        if (!IsInsideMicroWorld)
        {
            LastLeaveOpenWorldPlayerGP = BattleManager.Instance.Player1.transform.position.ToGridPos3D();
        }

        IsInsideMicroWorld = true;
        BattleManager.Instance.Player1.TransportPlayerGridPos(transportPlayerBornPoint);
        CameraManager.Instance.FieldCamera.InitFocus();

        ClientGameManager.Instance.DebugPanel.Clear();
        ClientGameManager.Instance.DebugPanel.Init();

        RefreshScopeModulesCoroutine = StartCoroutine(RefreshScopeModules(BattleManager.Instance.Player1.WorldGP, PlayerScopeRadiusX, PlayerScopeRadiusZ));
        while (RefreshScopeModulesCoroutine != null) yield return null;
        LoadingMapPanel.SetProgress(100f, "Loading Level");
        yield return new WaitForSeconds(LoadingMapPanel.GetRemainingLoadingDuration());
        LoadingMapPanel.CloseUIForm();
        BattleManager.Instance.Player1.ForbidAction = false;
        transportingPlayerToMicroWorld = false;
    }

    public void ReturnToOpenWorldFormMicroWorld(bool rebornPlayer = false)
    {
        if (transportingPlayerToMicroWorld) return;
        if (returningToOpenWorldFormMicroWorld) return;
        if (restartingMicroWorld) return;
        if (!IsInsideMicroWorld) return;
        StartCoroutine(Co_ReturnToOpenWorldFormMicroWorld(rebornPlayer));
    }

    private bool returningToOpenWorldFormMicroWorld = false;

    public IEnumerator Co_ReturnToOpenWorldFormMicroWorld(bool rebornPlayer)
    {
        returningToOpenWorldFormMicroWorld = true;
        CurrentMicroWorldTypeIndex = 0;
        AudioManager.Instance.BGMFadeIn("bgm/CoolSwing", 1f, 1f, true);
        BattleManager.Instance.Player1.ForbidAction = true;
        UIManager.Instance.ShowUIForms<LoadingMapPanel>();
        LoadingMapPanel.Clear();
        LoadingMapPanel.SetMinimumLoadingDuration(2);
        LoadingMapPanel.SetBackgroundAlpha(1);
        LoadingMapPanel.SetProgress(0, "Returning to Open World");

        // Recycling the Open World
        while (RefreshScopeModulesCoroutine != null) yield return null;
        if (rebornPlayer)
        {
            BattleManager.Instance.Player1.TransportPlayerGridPos(InitialPlayerBP);
            BattleManager.Instance.Player1.Reborn();
        }
        else
        {
            BattleManager.Instance.Player1.TransportPlayerGridPos(LastLeaveOpenWorldPlayerGP);
        }

        CameraManager.Instance.FieldCamera.InitFocus();
        IsInsideMicroWorld = false;
        RefreshScopeModulesCoroutine = StartCoroutine(RefreshScopeModules(LastLeaveOpenWorldPlayerGP, PlayerScopeRadiusX, PlayerScopeRadiusZ));
        while (RefreshScopeModulesCoroutine != null) yield return null;

        // Loading Micro World Modules
        LoadingMapPanel.SetProgress(80f, "Returning to Open World");
        int totalRecycleModuleNumber = MicroWorldModules.Count;
        int recycledModuleCount = 0;
        foreach (WorldModule microWorldModule in MicroWorldModules)
        {
            yield return Co_RecycleModule(microWorldModule, microWorldModule.ModuleGP, -1);
            recycledModuleCount++;
            LoadingMapPanel.SetProgress(80f + 20f * recycledModuleCount / totalRecycleModuleNumber, "Returning to Open World");
        }

        MicroWorldModules.Clear();
        LoadingMapPanel.SetProgress(100f, "Returning to Open World");

        yield return new WaitForSeconds(LoadingMapPanel.GetRemainingLoadingDuration());
        LoadingMapPanel.CloseUIForm();
        BattleManager.Instance.Player1.ForbidAction = false;
        returningToOpenWorldFormMicroWorld = false;
    }

    public void RestartMicroWorld(bool rebornPlayer)
    {
        if (transportingPlayerToMicroWorld) return;
        if (returningToOpenWorldFormMicroWorld) return;
        if (restartingMicroWorld) return;
        if (!IsInsideMicroWorld) return;
        StartCoroutine(Co_RestartMicroWorld(rebornPlayer));
    }

    private bool restartingMicroWorld = false;

    public IEnumerator Co_RestartMicroWorld(bool rebornPlayer)
    {
        restartingMicroWorld = true;
        BattleManager.Instance.Player1.ForbidAction = true;
        UIManager.Instance.ShowUIForms<LoadingMapPanel>();
        LoadingMapPanel.Clear();
        LoadingMapPanel.SetMinimumLoadingDuration(2);
        LoadingMapPanel.SetBackgroundAlpha(1);
        LoadingMapPanel.SetProgress(0, "Loading Level");
        WorldData microWorldData = ConfigManager.GetWorldDataConfig(CurrentMicroWorldTypeIndex);
        GridPos3D transportPlayerBornPoint = GridPos3D.Zero;

        while (RefreshScopeModulesCoroutine != null) yield return null;

        // Recycling Micro World Modules
        int totalRecycleModuleNumber = MicroWorldModules.Count;
        int recycledModuleCount = 0;
        foreach (WorldModule microWorldModule in MicroWorldModules)
        {
            yield return Co_RecycleModule(microWorldModule, microWorldModule.ModuleGP, -1);
            recycledModuleCount++;
            LoadingMapPanel.SetProgress(50f * recycledModuleCount / totalRecycleModuleNumber, "Destroying the dungeon");
        }

        MicroWorldModules.Clear();

        // Loading Micro World Modules
        int totalModuleNum = microWorldData.WorldModuleGPOrder.Count;
        int loadingModuleCount = 0;
        foreach (GridPos3D worldModuleGP in microWorldData.WorldModuleGPOrder)
        {
            ushort worldModuleTypeIndex = microWorldData.ModuleMatrix[worldModuleGP.x, worldModuleGP.y, worldModuleGP.z];
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
                    yield return GenerateWorldModule(worldModuleTypeIndex, realModuleGP.x, realModuleGP.y, realModuleGP.z);
                    WorldModule module = WorldModuleMatrix[realModuleGP.x, realModuleGP.y, realModuleGP.z];
                    MicroWorldModules.Add(module);
                    WorldData.WorldBornPointGroupData_Runtime.Init_LoadModuleData(realModuleGP, module.WorldModuleData);
                    yield return WorldData.WorldBornPointGroupData_Runtime.Dynamic_LoadModuleData(realModuleGP);
                    SortedDictionary<string, BornPointData> playerBornPoints = module.WorldModuleData.WorldModuleBornPointGroupData.PlayerBornPoints;
                    if (playerBornPoints.Count > 0)
                    {
                        if (transportPlayerBornPoint == GridPos3D.Zero) transportPlayerBornPoint = module.LocalGPToWorldGP(playerBornPoints[playerBornPoints.Keys.ToList()[0]].LocalGP);
                    }

                    List<BornPointData> moduleBPData = WorldData.WorldBornPointGroupData_Runtime.TryLoadModuleBPData(realModuleGP);
                    if (moduleBPData != null)
                    {
                        foreach (BornPointData bp in moduleBPData)
                        {
                            if (bp.ActorCategory == ActorCategory.Player)
                            {
                                string playerBPAlias = module.WorldModuleData.WorldModuleTypeName;
                                if (!WorldData.WorldBornPointGroupData_Runtime.PlayerBornPointDataAliasDict.ContainsKey(playerBPAlias))
                                {
                                    WorldData.WorldBornPointGroupData_Runtime.PlayerBornPointDataAliasDict.Add(playerBPAlias, bp);
                                }
                            }
                        }
                    }
                }
            }

            loadingModuleCount++;
            LoadingMapPanel.SetProgress(50f + 50f * loadingModuleCount / totalModuleNum, "Rebuilding the dungeon");
        }

        if (transportPlayerBornPoint == GridPos3D.Zero)
        {
            Debug.LogWarning("传送的模组没有默认玩家出生点");
        }

        BattleManager.Instance.Player1.TransportPlayerGridPos(transportPlayerBornPoint);
        if (rebornPlayer) BattleManager.Instance.Player1.Reborn();

        CameraManager.Instance.FieldCamera.InitFocus();

        ClientGameManager.Instance.DebugPanel.Clear();
        ClientGameManager.Instance.DebugPanel.Init();

        RefreshScopeModulesCoroutine = StartCoroutine(RefreshScopeModules(BattleManager.Instance.Player1.WorldGP, PlayerScopeRadiusX, PlayerScopeRadiusZ));
        while (RefreshScopeModulesCoroutine != null) yield return null;
        LoadingMapPanel.SetProgress(100f, "Loading Level");
        yield return new WaitForSeconds(LoadingMapPanel.GetRemainingLoadingDuration());
        LoadingMapPanel.CloseUIForm();
        BattleManager.Instance.Player1.ForbidAction = false;
        restartingMicroWorld = false;
    }

    #endregion
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BiangLibrary.CloneVariant;
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

    internal Dictionary<TypeDefineType, EntityData[,,]> WorldMap_EntityDataMatrix = new Dictionary<TypeDefineType, EntityData[,,]>(); // 地图元素放置, Y轴缩小16
    internal List<EntityData>[,,] WorldMap_TriggerEntityDataMatrix; // 地图Trigger实体放置, Y轴缩小16
    internal List<EntityPassiveSkill_LevelEventTriggerAppear.Data> EventTriggerAppearEntityDataList = new List<EntityPassiveSkill_LevelEventTriggerAppear.Data>();

    #region Occupy

    private ushort[,,] WorldMap_Occupied_BetweenBoxes; // Y轴缩小16
    private ushort[,,] WorldMap_Occupied_BoxAndActor; // 为了检查重叠. Box写入时只写入非Passable的，这样就允许Actor和Passable的Box初始可以重叠, Y轴缩小16
    public ushort[,,] WorldMap_StaticLayoutOccupied_IntactForStaticLayout; // 地图静态布局占位，禁止其他静态布局影响, Y轴缩小16
    public ushort[,,] WorldMap_StaticLayoutOccupied_IntactForBox; // 地图静态布局占位，禁止其他箱子影响, Y轴缩小16

    public ushort GetOccupy(TypeDefineType entityType, GridPos3D worldGP)
    {
        if (entityType == TypeDefineType.Box)
        {
            return WorldMap_Occupied_BetweenBoxes[worldGP.x, worldGP.y - WorldModule.MODULE_SIZE, worldGP.z];
        }
        else if (entityType == TypeDefineType.Actor)
        {
            return WorldMap_Occupied_BoxAndActor[worldGP.x, worldGP.y - WorldModule.MODULE_SIZE, worldGP.z];
        }

        return 0;
    }

    public bool CheckOccupied(TypeDefineType entityType, GridPos3D worldGP)
    {
        if (entityType == TypeDefineType.Box)
        {
            if (WorldMap_Occupied_BetweenBoxes[worldGP.x, worldGP.y - WorldModule.MODULE_SIZE, worldGP.z] != 0
                || WorldMap_Occupied_BoxAndActor[worldGP.x, worldGP.y - WorldModule.MODULE_SIZE, worldGP.z] != 0)
            {
                return true;
            }
        }
        else if (entityType == TypeDefineType.Actor)
        {
            if (WorldMap_Occupied_BoxAndActor[worldGP.x, worldGP.y - WorldModule.MODULE_SIZE, worldGP.z] != 0)
            {
                return true;
            }
        }

        return false;
    }

    public void SetOccupied(TypeDefineType entityType, ushort typeIndex, GridPos3D worldGP, bool occupied)
    {
        if (entityType == TypeDefineType.Box)
        {
            EntityOccupationData eod = ConfigManager.GetEntityOccupationData(typeIndex);
            bool passable = eod.Passable;
            WorldMap_Occupied_BetweenBoxes[worldGP.x, worldGP.y - WorldModule.MODULE_SIZE, worldGP.z] = (ushort) (occupied ? typeIndex : 0);
            if (!passable) WorldMap_Occupied_BoxAndActor[worldGP.x, worldGP.y - WorldModule.MODULE_SIZE, worldGP.z] = (ushort) (occupied ? typeIndex : 0);
        }
        else if (entityType == TypeDefineType.Actor)
        {
            WorldMap_Occupied_BoxAndActor[worldGP.x, worldGP.y - WorldModule.MODULE_SIZE, worldGP.z] = (ushort) (occupied ? typeIndex : 0);
        }
    }

    #endregion

    public TerrainType[,] WorldMap_TerrainType; // 地图地形分布，Y为0

    public BornPointData[,,] WorldBornPointData; // 地图出生点数据, Y轴缩小16

    [BoxGroup("@\"起始Dungeon\t\"+StartDungeonTypeName")]
    [HideLabel]
    public TypeSelectHelper StartDungeonTypeName = new TypeSelectHelper {TypeDefineType = TypeDefineType.World};

    internal GridPos3D InitialPlayerBP = GridPos3D.Zero;

    #region GenerateLayerData

    [BoxGroup("地形生成配置")]
    [HideLabel]
    public GenerateTerrainData m_GenerateTerrainData = new GenerateTerrainData();

    [FoldoutGroup("放置物")]
    [LabelText("静态布局层级配置")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<GenerateStaticLayoutLayerData> GenerateStaticLayoutLayerDataList = new List<GenerateStaticLayoutLayerData>();

    [FoldoutGroup("放置物")]
    [LabelText("Box层级配置")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<GenerateBoxLayerData> GenerateBoxLayerDataList = new List<GenerateBoxLayerData>();

    [FoldoutGroup("放置物")]
    [LabelText("Actor层级配置")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<GenerateActorLayerData> GenerateActorLayerDataList = new List<GenerateActorLayerData>();

    #endregion

    public const string PLAYER_DEFAULT_BP = "OpenWorldPlayerBP";

    public override void OnRecycled()
    {
        base.OnRecycled();
        WorldMap_EntityDataMatrix = null;
        WorldMap_TriggerEntityDataMatrix = null;
        EventTriggerAppearEntityDataList.Clear();
        WorldMap_Occupied_BetweenBoxes = null;
        WorldMap_Occupied_BoxAndActor = null;
        WorldMap_StaticLayoutOccupied_IntactForStaticLayout = null;
        WorldMap_StaticLayoutOccupied_IntactForBox = null;
        WorldMap_TerrainType = null;
        DungeonMissionState = DungeonMissionState.NotInDungeon;
        InitialPlayerBP = GridPos3D.Zero;
        transportingPlayerToDungeon = false;
        returningToOpenWorldFormDungeon = false;
        restartingDungeon = false;
    }

    public override IEnumerator Initialize(WorldData worldData)
    {
        ActorPathFinding.InitializeSpaceAvailableForActorHeight(WORLD_SIZE * WorldModule.MODULE_SIZE, WORLD_HEIGHT * WorldModule.MODULE_SIZE, WORLD_SIZE * WorldModule.MODULE_SIZE);

        LoadingMapPanel = UIManager.Instance.GetBaseUIForm<LoadingMapPanel>();
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

        WorldMap_EntityDataMatrix.Add(TypeDefineType.Box, new EntityData[WorldSize_X * WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldSize_Z * WorldModule.MODULE_SIZE]);
        WorldMap_EntityDataMatrix.Add(TypeDefineType.Actor, new EntityData[WorldSize_X * WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldSize_Z * WorldModule.MODULE_SIZE]);
        WorldMap_TriggerEntityDataMatrix = new List<EntityData>[WorldSize_X * WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldSize_Z * WorldModule.MODULE_SIZE];
        for (int x = 0; x < WorldSize_X * WorldModule.MODULE_SIZE; x++)
        for (int y = 0; y < WorldModule.MODULE_SIZE; y++)
        for (int z = 0; z < WorldSize_Z * WorldModule.MODULE_SIZE; z++)
        {
            WorldMap_TriggerEntityDataMatrix[x, y, z] = new List<EntityData>(4);
        }

        WorldMap_Occupied_BetweenBoxes = new ushort[WorldSize_X * WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldSize_Z * WorldModule.MODULE_SIZE];
        WorldMap_Occupied_BoxAndActor = new ushort[WorldSize_X * WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldSize_Z * WorldModule.MODULE_SIZE];
        WorldMap_StaticLayoutOccupied_IntactForStaticLayout = new ushort[WorldSize_X * WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldSize_Z * WorldModule.MODULE_SIZE];
        WorldMap_StaticLayoutOccupied_IntactForBox = new ushort[WorldSize_X * WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldSize_Z * WorldModule.MODULE_SIZE];
        WorldMap_TerrainType = new TerrainType[WorldSize_X * WorldModule.MODULE_SIZE, WorldSize_Z * WorldModule.MODULE_SIZE];
        WorldBornPointData = new BornPointData[WorldSize_X * WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldSize_Z * WorldModule.MODULE_SIZE];

        WorldGUID = Seed + "_" + Guid.NewGuid().ToString("P"); // e.g: (ade24d16-db0f-40af-8794-1e08e2040df3);
        m_LevelCacheData = new LevelCacheData();
        WorldData = worldData;

        WorldData.WorldBornPointGroupData_Runtime.Init();
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
                case GenerateAlgorithm.Around:
                {
                    generator = new AroundMapGenerator(boxLayerData, WorldModule.MODULE_SIZE * WorldSize_X, WorldModule.MODULE_SIZE * WorldSize_Z, SRandom.Next((uint) 9999), this);
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
        BattleManager.Instance.CreatePlayerByBornPointData(WorldData.WorldBornPointGroupData_Runtime.PlayerBornPointDataAliasDict[WorldData.DefaultWorldActorBornPointAlias]);
        BattleManager.Instance.IsStart = false;
        CameraManager.Instance.FieldCamera.InitFocus();

        // 没有起始关卡就直接进入大世界，有起始关卡则避免加载了又卸载
        ushort startDungeonTypeIndex = ConfigManager.GetTypeIndex(TypeDefineType.World, StartDungeonTypeName.TypeName);
        if (startDungeonTypeIndex == 0)
        {
            GridPos3D playerBPWorld = new GridPos3D(-1, -1, -1);
            RefreshScopeModulesCoroutine = StartCoroutine(RefreshScopeModules(playerBPWorld, PlayerScopeRadiusX, PlayerScopeRadiusZ)); // 按关卡生成器和角色位置初始化需要的模组
            while (RefreshScopeModulesCoroutine != null) yield return null;
            BattleManager.Instance.IsStart = true;
        }
        else
        {
            WaitingForLoadStartDungeon = true;
        }
    }

    private bool WaitingForLoadStartDungeon = false;

    public IEnumerator OnAfterInitialize()
    {
        ushort startDungeonTypeIndex = ConfigManager.GetTypeIndex(TypeDefineType.World, StartDungeonTypeName.TypeName);
        if (startDungeonTypeIndex != 0)
        {
            yield return Co_TransportPlayerToDungeon(startDungeonTypeIndex);
        }

        WaitingForLoadStartDungeon = false;
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
        public List<MapGenerator> CurrentGenerator_StaticLayouts = new List<MapGenerator>(); // 按静态布局layer记录的地图生成信息，未走过的地图模组按这个数据加载出来
        public List<MapGenerator> CurrentGenerators_Boxes = new List<MapGenerator>(); // 按箱子layer记录的地图生成信息，未走过的地图模组按这个数据加载出来
        public List<MapGenerator> CurrentGenerators_Actors = new List<MapGenerator>(); // 按角色layer记录的生成信息，未走过的地图模组按这个数据加载出来

        public Dictionary<GridPos3D, WorldModuleData> WorldModuleDataDict = new Dictionary<GridPos3D, WorldModuleData>();

        public List<GridPos3D> CurrentShowModuleGPs = new List<GridPos3D>();
    }

    private LevelCacheData m_LevelCacheData;

    private Coroutine RefreshScopeModulesCoroutine;

    void FixedUpdate()
    {
        if (!IsRecycled)
        {
            if (GameStateManager.Instance.GetState() == GameState.Fighting && DungeonMissionState == DungeonMissionState.NotInDungeon)
            {
                if (RefreshScopeModulesCoroutine == null && !WaitingForLoadStartDungeon)
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
            if (InsideDungeon) return moduleGP.y >= WORLD_HEIGHT / 2; // 当角色在解谜关卡时，隐藏其它模组，且恒定显示解谜模组 todo 此处粗暴地使用了模组坐标来判断是否在解谜关卡，未来需要处理
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
                    GridPos3D localGP = new GridPos3D(0, 15, 0);
                    WorldModuleData moduleData = WorldModuleData.WorldModuleDataFactory.Alloc();
                    TypeSelectHelper entityType = new TypeSelectHelper
                    {
                        TypeDefineType = TypeDefineType.Box,
                        TypeSelection = ConfigManager.GetTypeName(TypeDefineType.Box, ConfigManager.Box_CombinedGroundBoxIndex)
                    };
                    entityType.RefreshGUID();
                    moduleData[TypeDefineType.Box, localGP] =
                        new EntityData
                        {
                            EntityType = entityType,
                            EntityOrientation = GridPosR.Orientation.Up,
                            RawEntityExtraSerializeData = null,
                        };
                    moduleData.WorldModuleFeature = WorldModuleFeature.Ground;

                    generateModuleFinished.Add(false);
                    StartCoroutine(Co_GenerateModule(moduleData, targetModuleGP, generateModuleFinished.Count - 1));
                }

                // Other Modules
                WorldModule module = WorldModuleMatrix[module_x, 1, module_z];
                if (module == null)
                {
                    GridPos3D targetModuleGP = new GridPos3D(module_x, 1, module_z);
                    if (!m_LevelCacheData.WorldModuleDataDict.TryGetValue(targetModuleGP, out WorldModuleData moduleData))
                    {
                        // 从未加载过的模组，通过generator来计算
                        moduleData = WorldModuleData.WorldModuleDataFactory.Alloc();
                        moduleData.BGM_ThemeState = BGM_Theme.OpenWorld;
                        m_LevelCacheData.WorldModuleDataDict.Add(targetModuleGP, moduleData);
                        foreach (KeyValuePair<TypeDefineType, EntityData[,,]> kv in WorldMap_EntityDataMatrix)
                        {
                            for (int world_x = targetModuleGP.x * WorldModule.MODULE_SIZE; world_x < (targetModuleGP.x + 1) * WorldModule.MODULE_SIZE; world_x++)
                            for (int world_y = targetModuleGP.y * WorldModule.MODULE_SIZE; world_y < (targetModuleGP.y + 1) * WorldModule.MODULE_SIZE; world_y++)
                            for (int world_z = targetModuleGP.z * WorldModule.MODULE_SIZE; world_z < (targetModuleGP.z + 1) * WorldModule.MODULE_SIZE; world_z++)
                            {
                                GridPos3D worldGP = new GridPos3D(world_x, world_y, world_z);
                                GridPos3D localGP = worldGP - targetModuleGP * WorldModule.MODULE_SIZE;
                                moduleData[kv.Key, localGP] = WorldMap_EntityDataMatrix[kv.Key][world_x, world_y - WorldModule.MODULE_SIZE, world_z]?.Clone();
                            }
                        }

                        // TriggerEntity
                        for (int world_x = targetModuleGP.x * WorldModule.MODULE_SIZE; world_x < (targetModuleGP.x + 1) * WorldModule.MODULE_SIZE; world_x++)
                        for (int world_y = targetModuleGP.y * WorldModule.MODULE_SIZE; world_y < (targetModuleGP.y + 1) * WorldModule.MODULE_SIZE; world_y++)
                        for (int world_z = targetModuleGP.z * WorldModule.MODULE_SIZE; world_z < (targetModuleGP.z + 1) * WorldModule.MODULE_SIZE; world_z++)
                        {
                            GridPos3D worldGP = new GridPos3D(world_x, world_y, world_z);
                            GridPos3D localGP = worldGP - targetModuleGP * WorldModule.MODULE_SIZE;
                            List<EntityData> dataList = WorldMap_TriggerEntityDataMatrix[world_x, world_y - WorldModule.MODULE_SIZE, world_z].Clone<EntityData, EntityData>();
                            foreach (EntityData data in dataList)
                            {
                                moduleData.TriggerEntityDataList.Add(data);
                            }
                        }

                        foreach (EntityPassiveSkill_LevelEventTriggerAppear.Data data in EventTriggerAppearEntityDataList)
                        {
                            if (GetModuleGPByWorldGP(data.WorldGP) == targetModuleGP)
                            {
                                moduleData.EventTriggerAppearEntityDataList.Add((EntityPassiveSkill_LevelEventTriggerAppear.Data) data.Clone());
                            }
                        }

                        WorldData.WorldBornPointGroupData_Runtime.Init_LoadModuleData(targetModuleGP, moduleData);
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
        WorldModuleMatrix[currentShowModuleGP.x, currentShowModuleGP.y, currentShowModuleGP.z] = null; // 时序，先置空指针再清空
        yield return worldModule.Clear(false, 256);
        worldModule.PoolRecycle();
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

    #region Dungeon

    internal bool InsideDungeon => DungeonMissionState != DungeonMissionState.NotInDungeon;
    internal DungeonMissionState DungeonMissionState;
    internal bool IsUsingSpecialESPSInsideDungeon = false;

    internal ushort CurrentDungeonWorldTypeIndex = 0;
    internal GridPos3D LastLeaveOpenWorldPlayerGP = GridPos3D.Zero;
    private List<WorldModule> DungeonModules = new List<WorldModule>();

    private PlayerData PlayerDataSave;
    private PlayerData PlayerDataSave_DungeonTemp;

    public void TransportPlayerToDungeon(ushort worldTypeIndex)
    {
        if (transportingPlayerToDungeon) return;
        if (returningToOpenWorldFormDungeon) return;
        if (restartingDungeon) return;
        if (respawningInOpenWorld) return;
        StartCoroutine(Co_TransportPlayerToDungeon(worldTypeIndex));
    }

    private bool transportingPlayerToDungeon = false;

    IEnumerator Co_TransportPlayerToDungeon(ushort worldTypeIndex)
    {
        transportingPlayerToDungeon = true;
        CurrentDungeonWorldTypeIndex = worldTypeIndex;
        BattleManager.Instance.IsStart = false;
        WwiseAudioManager.Instance.Trigger_Teleport.Post(WwiseAudioManager.Instance.WwiseBGMConfiguration.gameObject);
        UIManager.Instance.ShowUIForms<LoadingMapPanel>();
        LoadingMapPanel.Clear();
        LoadingMapPanel.SetMinimumLoadingDuration(2);
        LoadingMapPanel.SetBackgroundAlpha(1);
        LoadingMapPanel.SetProgress(0, "Loading Level");

        WorldData dungeonData = ConfigManager.GetWorldDataConfig(worldTypeIndex);

        if (InsideDungeon)
        {
            // Recycle Current Dungeon Modules
            int totalRecycleModuleNumber = DungeonModules.Count;
            int recycledModuleCount = 0;
            foreach (WorldModule dungeonModule in DungeonModules)
            {
                yield return Co_RecycleModule(dungeonModule, dungeonModule.ModuleGP, -1);
                recycledModuleCount++;
                LoadingMapPanel.SetProgress(20f * recycledModuleCount / totalRecycleModuleNumber, "Destroying the dungeon");
            }

            DungeonModules.Clear();
            yield return RecycleEmptyModules();
            DungeonMissionState = DungeonMissionState.DungeonPartialComplete;
            SavePlayerGrowth();
        }
        else
        {
            LastLeaveOpenWorldPlayerGP = BattleManager.Instance.Player1.transform.position.ToGridPos3D();
            DungeonMissionState = DungeonMissionState.DungeonInProgress;
            SavePlayerGrowth();
        }

        // Loading New Dungeon Modules
        int totalModuleNum = dungeonData.WorldModuleGPOrder.Count;
        int loadingModuleCount = 0;
        while (RefreshScopeModulesCoroutine != null) yield return null;
        foreach (GridPos3D worldModuleGP in dungeonData.WorldModuleGPOrder)
        {
            ushort worldModuleTypeIndex = dungeonData.ModuleMatrix[worldModuleGP.x, worldModuleGP.y, worldModuleGP.z];
            GridPos3D realModuleGP = new GridPos3D(worldModuleGP.x, WORLD_HEIGHT / 2 + worldModuleGP.y, worldModuleGP.z);
            if (worldModuleTypeIndex != 0)
            {
                if (worldModuleGP.y >= WORLD_HEIGHT / 2)
                {
                    Debug.LogError($"静态世界不允许超过{WORLD_HEIGHT / 2}个模组高度");
                    continue;
                }
                else
                {
                    yield return GenerateWorldModule(worldModuleTypeIndex, realModuleGP.x, realModuleGP.y, realModuleGP.z);
                    WorldModule module = WorldModuleMatrix[realModuleGP.x, realModuleGP.y, realModuleGP.z];
                    DungeonModules.Add(module);
                    WorldData.WorldBornPointGroupData_Runtime.Init_LoadModuleData(realModuleGP, module.WorldModuleData);
                    List<BornPointData> moduleBPData = WorldData.WorldBornPointGroupData_Runtime.TryLoadModuleBPData(realModuleGP);
                    if (moduleBPData != null)
                    {
                        foreach (BornPointData bp in moduleBPData)
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

            loadingModuleCount++;
            LoadingMapPanel.SetProgress(20f + 60f * loadingModuleCount / totalModuleNum, "Loading Level");
        }

        yield return GenerateEmptyModules(dungeonData, new GridPos3D(0, WORLD_HEIGHT / 2, 0));

        GridPos3D transportPlayerBornPoint = WorldData.WorldBornPointGroupData_Runtime.PlayerBornPointDataAliasDict[dungeonData.DefaultWorldActorBornPointAlias].WorldGP;
        if (transportPlayerBornPoint == GridPos3D.Zero)
        {
            Debug.LogWarning("传送的模组没有默认玩家出生点");
        }

        // Dungeon Special Settings
        IsUsingSpecialESPSInsideDungeon = dungeonData.UseSpecialPlayerEnterESPS;
        if (IsUsingSpecialESPSInsideDungeon) BattleManager.Instance.Player1.ReloadESPS(dungeonData.Raw_PlayerEnterESPS);
        BattleManager.Instance.Player1.CanBeThreatened = true;

        ApplyWorldVisualEffectSettings(dungeonData);

        BattleManager.Instance.Player1.TransportPlayerGridPos(transportPlayerBornPoint);

        CameraManager.Instance.FieldCamera.InitFocus();

        ClientGameManager.Instance.DebugPanel.Clear();
        ClientGameManager.Instance.DebugPanel.Init();

        RefreshScopeModulesCoroutine = StartCoroutine(RefreshScopeModules(BattleManager.Instance.Player1.WorldGP, PlayerScopeRadiusX, PlayerScopeRadiusZ));
        while (RefreshScopeModulesCoroutine != null) yield return null;
        LoadingMapPanel.SetProgress(100f, "Loading Level");
        yield return new WaitForSeconds(LoadingMapPanel.GetRemainingLoadingDuration());
        LoadingMapPanel.CloseUIForm();
        BattleManager.Instance.IsStart = true;
        transportingPlayerToDungeon = false;
    }

    public void ReturnToOpenWorld(bool dungeonComplete)
    {
        if (transportingPlayerToDungeon) return;
        if (returningToOpenWorldFormDungeon) return;
        if (restartingDungeon) return;
        if (respawningInOpenWorld) return;
        StartCoroutine(Co_ReturnToOpenWorld(dungeonComplete));
    }

    private bool returningToOpenWorldFormDungeon = false;

    public IEnumerator Co_ReturnToOpenWorld(bool dungeonComplete)
    {
        returningToOpenWorldFormDungeon = true;
        CurrentDungeonWorldTypeIndex = 0;
        BattleManager.Instance.IsStart = false;
        WwiseAudioManager.Instance.Trigger_Teleport.Post(WwiseAudioManager.Instance.WwiseBGMConfiguration.gameObject);
        UIManager.Instance.ShowUIForms<LoadingMapPanel>();
        LoadingMapPanel.Clear();
        LoadingMapPanel.SetMinimumLoadingDuration(2);
        LoadingMapPanel.SetBackgroundAlpha(1);
        LoadingMapPanel.SetProgress(0, "Returning to Open World");

        while (RefreshScopeModulesCoroutine != null) yield return null;

        if (dungeonComplete) DungeonMissionState = DungeonMissionState.DungeonComplete;
        else DungeonMissionState = DungeonMissionState.DungeonFailed;
        SavePlayerGrowth();
        LoadPlayerGrowth();
        DungeonMissionState = DungeonMissionState.NotInDungeon;

        BattleManager.Instance.Player1.CanBeThreatened = true;
        BattleManager.Instance.Player1.TransportPlayerGridPos(LastLeaveOpenWorldPlayerGP); // 如果未在dungeon里面死亡，复活回进入dungeon的地方

        ApplyWorldVisualEffectSettings(WorldData);

        CameraManager.Instance.FieldCamera.InitFocus();
        DungeonMissionState = DungeonMissionState.NotInDungeon;
        RefreshScopeModulesCoroutine = StartCoroutine(RefreshScopeModules(LastLeaveOpenWorldPlayerGP, PlayerScopeRadiusX, PlayerScopeRadiusZ));
        while (RefreshScopeModulesCoroutine != null) yield return null;

        // Recycle Dungeon Modules
        LoadingMapPanel.SetProgress(80f, "Returning to Open World");
        int totalRecycleModuleNumber = DungeonModules.Count;
        int recycledModuleCount = 0;
        foreach (WorldModule dungeonModule in DungeonModules)
        {
            yield return Co_RecycleModule(dungeonModule, dungeonModule.ModuleGP, -1);
            recycledModuleCount++;
            LoadingMapPanel.SetProgress(80f + 20f * recycledModuleCount / totalRecycleModuleNumber, "Returning to Open World");
        }

        DungeonModules.Clear();
        yield return RecycleEmptyModules();
        LoadingMapPanel.SetProgress(100f, "Returning to Open World");

        yield return new WaitForSeconds(LoadingMapPanel.GetRemainingLoadingDuration());
        LoadingMapPanel.CloseUIForm();
        BattleManager.Instance.IsStart = true;
        returningToOpenWorldFormDungeon = false;
    }

    public void RestartDungeon()
    {
        if (transportingPlayerToDungeon) return;
        if (returningToOpenWorldFormDungeon) return;
        if (restartingDungeon) return;
        if (respawningInOpenWorld) return;
        if (!InsideDungeon) return;
        StartCoroutine(Co_RestartDungeon());
    }

    private bool restartingDungeon = false;

    public IEnumerator Co_RestartDungeon()
    {
        restartingDungeon = true;
        BattleManager.Instance.IsStart = false;
        WwiseAudioManager.Instance.Trigger_Teleport.Post(WwiseAudioManager.Instance.WwiseBGMConfiguration.gameObject);
        UIManager.Instance.ShowUIForms<LoadingMapPanel>();
        LoadingMapPanel.Clear();
        LoadingMapPanel.SetMinimumLoadingDuration(2);
        LoadingMapPanel.SetBackgroundAlpha(1);
        LoadingMapPanel.SetProgress(0, "Loading Level");
        WorldData dungeonData = ConfigManager.GetWorldDataConfig(CurrentDungeonWorldTypeIndex);
        GridPos3D transportPlayerBornPoint = GridPos3D.Zero;

        while (RefreshScopeModulesCoroutine != null) yield return null;

        // Recycling Dungeon Modules
        int totalRecycleModuleNumber = DungeonModules.Count;
        int recycledModuleCount = 0;
        foreach (WorldModule dungeonModule in DungeonModules)
        {
            yield return Co_RecycleModule(dungeonModule, dungeonModule.ModuleGP, -1);
            recycledModuleCount++;
            LoadingMapPanel.SetProgress(50f * recycledModuleCount / totalRecycleModuleNumber, "Destroying the dungeon");
        }

        DungeonModules.Clear();
        yield return RecycleEmptyModules();

        DungeonMissionState = DungeonMissionState.DungeonPartialFailed;
        LoadPlayerGrowth();

        // Loading Dungeon Modules
        int totalModuleNum = dungeonData.WorldModuleGPOrder.Count;
        int loadingModuleCount = 0;
        foreach (GridPos3D worldModuleGP in dungeonData.WorldModuleGPOrder)
        {
            ushort worldModuleTypeIndex = dungeonData.ModuleMatrix[worldModuleGP.x, worldModuleGP.y, worldModuleGP.z];
            GridPos3D realModuleGP = new GridPos3D(worldModuleGP.x, WORLD_HEIGHT / 2 + worldModuleGP.y, worldModuleGP.z);
            if (worldModuleTypeIndex != 0)
            {
                if (worldModuleGP.y >= WORLD_HEIGHT / 2)
                {
                    Debug.LogError($"静态世界不允许超过{WORLD_HEIGHT / 2}个模组高度");
                    continue;
                }
                else
                {
                    yield return GenerateWorldModule(worldModuleTypeIndex, realModuleGP.x, realModuleGP.y, realModuleGP.z);
                    WorldModule module = WorldModuleMatrix[realModuleGP.x, realModuleGP.y, realModuleGP.z];
                    DungeonModules.Add(module);
                    WorldData.WorldBornPointGroupData_Runtime.Init_LoadModuleData(realModuleGP, module.WorldModuleData);
                    SortedDictionary<string, BornPointData> playerBornPoints = module.WorldModuleData.WorldModuleBornPointGroupData.PlayerBornPoints;
                    if (playerBornPoints.TryGetValue(dungeonData.DefaultWorldActorBornPointAlias, out BornPointData defaultBP))
                    {
                        transportPlayerBornPoint = module.LocalGPToWorldGP(defaultBP.LocalGP);
                    }

                    List<BornPointData> moduleBPData = WorldData.WorldBornPointGroupData_Runtime.TryLoadModuleBPData(realModuleGP);
                    if (moduleBPData != null)
                    {
                        foreach (BornPointData bp in moduleBPData)
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

            loadingModuleCount++;
            LoadingMapPanel.SetProgress(50f + 50f * loadingModuleCount / totalModuleNum, "Rebuilding the dungeon");
        }

        yield return GenerateEmptyModules(dungeonData, new GridPos3D(0, WORLD_HEIGHT / 2, 0));

        if (transportPlayerBornPoint == GridPos3D.Zero)
        {
            Debug.LogWarning("传送的模组没有默认玩家出生点");
        }

        BattleManager.Instance.Player1.CanBeThreatened = true;
        BattleManager.Instance.Player1.TransportPlayerGridPos(transportPlayerBornPoint);
        BattleManager.Instance.Player1.Reborn();

        if (IsUsingSpecialESPSInsideDungeon) BattleManager.Instance.Player1.ReloadESPS(dungeonData.Raw_PlayerEnterESPS);

        CameraManager.Instance.FieldCamera.InitFocus();

        ClientGameManager.Instance.DebugPanel.Clear();
        ClientGameManager.Instance.DebugPanel.Init();

        RefreshScopeModulesCoroutine = StartCoroutine(RefreshScopeModules(BattleManager.Instance.Player1.WorldGP, PlayerScopeRadiusX, PlayerScopeRadiusZ));
        while (RefreshScopeModulesCoroutine != null) yield return null;
        LoadingMapPanel.SetProgress(100f, "Loading Level");
        yield return new WaitForSeconds(LoadingMapPanel.GetRemainingLoadingDuration());
        LoadingMapPanel.CloseUIForm();
        BattleManager.Instance.IsStart = true;
        restartingDungeon = false;
    }

    public void RespawnInOpenWorld()
    {
        if (transportingPlayerToDungeon) return;
        if (returningToOpenWorldFormDungeon) return;
        if (restartingDungeon) return;
        if (respawningInOpenWorld) return;
        StartCoroutine(Co_RespawnInOpenWorld());
    }

    private bool respawningInOpenWorld = false;

    public IEnumerator Co_RespawnInOpenWorld()
    {
        respawningInOpenWorld = true;
        BattleManager.Instance.IsStart = false;
        WwiseAudioManager.Instance.Trigger_Teleport.Post(WwiseAudioManager.Instance.WwiseBGMConfiguration.gameObject);
        UIManager.Instance.ShowUIForms<LoadingMapPanel>();
        LoadingMapPanel.Clear();
        LoadingMapPanel.SetMinimumLoadingDuration(2);
        LoadingMapPanel.SetBackgroundAlpha(1);
        LoadingMapPanel.SetProgress(0, "Respawning");

        // Recycling the Open World
        while (RefreshScopeModulesCoroutine != null) yield return null;
        SavePlayerGrowth();
        BattleManager.Instance.Player1.CanBeThreatened = true;
        BattleManager.Instance.Player1.TransportPlayerGridPos(InitialPlayerBP); // 如果在dungeon里面死亡，复活回老家
        BattleManager.Instance.Player1.Reborn();

        CameraManager.Instance.FieldCamera.InitFocus();
        RefreshScopeModulesCoroutine = StartCoroutine(RefreshScopeModules(LastLeaveOpenWorldPlayerGP, PlayerScopeRadiusX, PlayerScopeRadiusZ));
        while (RefreshScopeModulesCoroutine != null) yield return null;

        // Loading Dungeon Modules
        LoadingMapPanel.SetProgress(80f, "Returning to Open World");
        int totalRecycleModuleNumber = DungeonModules.Count;
        int recycledModuleCount = 0;
        foreach (WorldModule dungeonModule in DungeonModules)
        {
            yield return Co_RecycleModule(dungeonModule, dungeonModule.ModuleGP, -1);
            recycledModuleCount++;
            LoadingMapPanel.SetProgress(80f + 20f * recycledModuleCount / totalRecycleModuleNumber, "Returning to Open World");
        }

        DungeonModules.Clear();
        yield return RecycleEmptyModules();
        LoadingMapPanel.SetProgress(100f, "Returning to Open World");

        yield return new WaitForSeconds(LoadingMapPanel.GetRemainingLoadingDuration());
        LoadingMapPanel.CloseUIForm();
        BattleManager.Instance.IsStart = true;
        respawningInOpenWorld = false;
    }

    #endregion

    #region PlayerGrowth

    private void SavePlayerGrowth() // 将玩家成长存档
    {
        if (DungeonMissionState == DungeonMissionState.NotInDungeon)
        {
            PlayerDataSave = PlayerData.GetPlayerData();
        }
        else if (DungeonMissionState == DungeonMissionState.DungeonInProgress)
        {
            PlayerDataSave = PlayerData.GetPlayerData();
            PlayerDataSave_DungeonTemp = PlayerData.GetPlayerData();
        }
        else
        {
            if (IsUsingSpecialESPSInsideDungeon) // 此类特殊关卡默认不计入成长，如教程关、体验关等等
            {
                return;
            }
            else
            {
                if (DungeonMissionState == DungeonMissionState.DungeonPartialComplete) // Dungeon阶段性过关，记录玩家数据到DungeonTemp里
                {
                    PlayerDataSave_DungeonTemp = PlayerData.GetPlayerData();
                }
                else if (DungeonMissionState == DungeonMissionState.DungeonComplete) // Dungeon通关，获得所有奖励，保存玩家成长数值和技能
                {
                    PlayerDataSave = PlayerData.GetPlayerData();
                }
            }
        }
    }

    private void LoadPlayerGrowth() // 提取存档
    {
        if (DungeonMissionState == DungeonMissionState.NotInDungeon)
        {
            PlayerDataSave?.ApplyDataOnPlayer();
        }
        else
        {
            if (IsUsingSpecialESPSInsideDungeon) // 此类特殊关卡默认不计入成长，如教程关、体验关等等
            {
                PlayerDataSave?.ApplyDataOnPlayer();
            }
            else
            {
                if (DungeonMissionState == DungeonMissionState.DungeonPartialFailed) // Dungeon死亡，回到上一关的存档数据
                {
                    PlayerDataSave_DungeonTemp?.ApplyDataOnPlayer();
                }
                else if (DungeonMissionState == DungeonMissionState.DungeonFailed) // Dungeon放弃，回到进入Dungeon之前的大世界的存档数据
                {
                    PlayerDataSave?.ApplyDataOnPlayer();
                }
            }
        }
    }

    #endregion
}
using System;
using System.Collections;
using System.Collections.Generic;
using BiangLibrary.GameDataFormat;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class OpenWorld : World
{
    public int WorldSize_X = 2;
    public int WorldSize_Z = 2;

    public int PlayerScopeRadiusX = 2;
    public int PlayerScopeRadiusZ = 2;

    public bool UseCertainSeed = false;

    [ShowIf("UseCertainSeed")]
    public uint GivenSeed = 0;

    public ushort[,,] WorldMap; // 地图元素放置  Y轴缩小16
    public GridPosR.Orientation[,,] WorldMapOrientation; // 地图元素放置朝向  Y轴缩小16
    public ushort[,,] WorldMap_Occupied; // 地图元素占位  Y轴缩小16
    public ushort[,,] WorldMap_StaticLayoutOccupied; // 地图静态布局占位  Y轴缩小16

    #region GenerateLayerData

    [Serializable]
    public abstract class GenerateLayerData
    {
        [LabelText("生效")]
        public bool Enable = true;

        [SerializeField]
        [LabelText("允许覆盖的箱子类型")]
        [ValueDropdown("GetAllBoxTypeNames", IsUniqueList = true, DrawDropdownForListElements = true)]
        private List<string> AllowReplacedBoxTypeNames = new List<string>();

        [HideInInspector]
        public HashSet<string> AllowReplacedBoxTypeNameSet = new HashSet<string>();

        [LabelText("只允许覆盖上述箱子上")]
        public bool OnlyOverrideAnyBox = false;

        [LabelText("生成算法")]
        public GenerateAlgorithm m_GenerateAlgorithm = GenerateAlgorithm.CellularAutomata;

        [ShowIf("m_GenerateAlgorithm", GenerateAlgorithm.Random)]
        [LabelText("比率：每万格约有多少个")]
        public int CountPer10KGrid = 20;

        public void Init()
        {
            foreach (string allowReplacedBoxTypeName in AllowReplacedBoxTypeNames)
            {
                AllowReplacedBoxTypeNameSet.Add(allowReplacedBoxTypeName);
            }
        }

        private IEnumerable<string> GetAllBoxTypeNames => ConfigManager.GetAllBoxTypeNames(false);
        private IEnumerable<string> GetAllActorTypeNames => ConfigManager.GetAllActorNames(false);
        private IEnumerable<string> GetAllStaticLayoutTypeNames => ConfigManager.GetAllStaticLayoutNames(false);
    }

    [Serializable]
    public class GenerateStaticLayoutLayerData : GenerateLayerData
    {
        [BoxName]
        [LabelText("静态布局类型")]
        [ValueDropdown("GetAllStaticLayoutTypeNames")]
        public string StaticLayoutTypeName = "None";
    }

    [FoldoutGroup("地图生成")]
    [LabelText("静态布局层级配置")]
    [ListDrawerSettings(ListElementLabelName = "StaticLayoutTypeName")]
    public List<GenerateStaticLayoutLayerData> GenerateStaticLayoutLayerDataList = new List<GenerateStaticLayoutLayerData>();

    [Serializable]
    public class GenerateBoxLayerData : GenerateLayerData
    {
        [BoxName]
        [LabelText("箱子类型")]
        [ValueDropdown("GetAllBoxTypeNames")]
        public string BoxTypeName = "None";

        [ShowIf("m_GenerateAlgorithm", GenerateAlgorithm.CellularAutomata)]
        [LabelText("初始填充比率")]
        public int FillPercent = 40;

        [ShowIf("m_GenerateAlgorithm", GenerateAlgorithm.CellularAutomata)]
        [LabelText("洞穴联通率")]
        public int CaveConnectPercent = 0;

        [ShowIf("m_GenerateAlgorithm", GenerateAlgorithm.CellularAutomata)]
        [LabelText("决定玩家出生点")]
        public bool DeterminePlayerBP = false;

        [ShowIf("m_GenerateAlgorithm", GenerateAlgorithm.CellularAutomata)]
        [LabelText("迭代次数")]
        public int SmoothTimes = 4;

        [ShowIf("m_GenerateAlgorithm", GenerateAlgorithm.CellularAutomata)]
        [LabelText("空地生墙迭代次数")]
        public int SmoothTimes_GenerateWallInOpenSpace = 3;
    }

    [FoldoutGroup("地图生成")]
    [LabelText("Box层级配置")]
    [ListDrawerSettings(ListElementLabelName = "BoxTypeName")]
    public List<GenerateBoxLayerData> GenerateBoxLayerDataList = new List<GenerateBoxLayerData>();

    [Serializable]
    public class GenerateActorLayerData : GenerateLayerData
    {
        [BoxName]
        [LabelText("Actor类型")]
        [ValueDropdown("GetAllActorTypeNames")]
        public string ActorTypeName = "None";

        public ActorCategory ActorCategory
        {
            get
            {
                if (ActorTypeName.StartsWith("Player"))
                {
                    return ActorCategory.Player;
                }
                else
                {
                    return ActorCategory.Creature;
                }
            }
        }
    }

    public enum GenerateAlgorithm
    {
        CellularAutomata,
        PerlinNoise,
        Random,
    }

    [FoldoutGroup("地图生成")]
    [LabelText("Actor层级配置")]
    [ListDrawerSettings(ListElementLabelName = "ActorTypeName")]
    public List<GenerateActorLayerData> GenerateActorLayerDataList = new List<GenerateActorLayerData>();

    #endregion

    public override void OnRecycled()
    {
        base.OnRecycled();
        WorldMap = null;
        WorldMapOrientation = null;
        WorldMap_Occupied = null;
        WorldMap_StaticLayoutOccupied = null;
        IsInsideMicroModules = false;
    }

    public override IEnumerator Initialize(WorldData worldData)
    {
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
        WorldMapOrientation = new GridPosR.Orientation[WorldSize_X * WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldSize_Z * WorldModule.MODULE_SIZE];
        WorldMap_Occupied = new ushort[WorldSize_X * WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldSize_Z * WorldModule.MODULE_SIZE];
        WorldMap_StaticLayoutOccupied = new ushort[WorldSize_X * WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldSize_Z * WorldModule.MODULE_SIZE];

        WorldGUID = Seed + "_" + Guid.NewGuid().ToString("P"); // e.g: (ade24d16-db0f-40af-8794-1e08e2040df3);
        m_LevelCacheData = new LevelCacheData();
        WorldData = worldData;

        WorldData.WorldBornPointGroupData_Runtime.InitTempData();
        WorldData.DefaultWorldActorBornPointAlias = "PlayerBP";

        ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.2f, "Loading Open World");
        yield return null;

        #region GenerateStaticLayoutLayer

        int generatorCount_staticLayoutLayer = 0;

        foreach (GenerateStaticLayoutLayerData staticLayoutLayerData in GenerateStaticLayoutLayerDataList) // 按层生成关卡静态布局数据
        {
            if (!staticLayoutLayerData.Enable) continue;
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
                ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.2f + 0.1f * generatorCount_staticLayoutLayer / GenerateStaticLayoutLayerDataList.Count, "Generating Map Static Layouts");
                yield return null;
            }
        }

        ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.3f, "Generating Map Static Layouts Completed");
        yield return null;

        #endregion

        #region GenerateBoxLayer

        int generatorCount_boxLayer = 0;
        GridPos3D playerBPWorld = new GridPos3D(-1, -1, -1);

        foreach (GenerateBoxLayerData boxLayerData in GenerateBoxLayerDataList) // 按层生成关卡Box数据
        {
            if (!boxLayerData.Enable) continue;
            boxLayerData.Init();
            MapGenerator generator = null;
            switch (boxLayerData.m_GenerateAlgorithm)
            {
                case GenerateAlgorithm.CellularAutomata:
                {
                    generator = new CellularAutomataMapGenerator(boxLayerData, WorldModule.MODULE_SIZE * WorldSize_X, WorldModule.MODULE_SIZE * WorldSize_Z, SRandom.Next((uint) 9999), this);
                    if (boxLayerData.DeterminePlayerBP) // 细胞自动机的地图决定了玩家落点，位于联通的洞穴中
                    {
                        GridPos playerBP = ((CellularAutomataMapGenerator) generator).ValidPlayerPosInConnectedCaves;
                        playerBPWorld = new GridPos3D(playerBP.x, WorldModule.MODULE_SIZE, playerBP.z);
                        GridPos3D playerBPLocal = new GridPos3D(playerBP.x % WorldModule.MODULE_SIZE, 0, playerBP.z % WorldModule.MODULE_SIZE);
                        BornPointData bp = new BornPointData {ActorType = "Player1", BornPointAlias = $"PlayerBP", LocalGP = playerBPLocal, SpawnLevelEventAlias = "", WorldGP = playerBPWorld};
                        WorldData.WorldBornPointGroupData_Runtime.SetDefaultPlayerBP_OpenWorld(bp);
                        WorldMap[playerBPWorld.x, playerBPWorld.y - WorldModule.MODULE_SIZE, playerBPWorld.z] = (ushort) ConfigManager.TypeStartIndex.Player;
                    }

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
                ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.3f + 0.25f * generatorCount_boxLayer / GenerateBoxLayerDataList.Count, "Generating Map Boxes");
                yield return null;
            }
        }

        ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.55f, "Generating Map Boxes Completed");
        yield return null;

        #endregion

        #region GenerateActorLayer

        int generatorCount_actorLayer = 0;
        foreach (GenerateActorLayerData actorLayerData in GenerateActorLayerDataList) // 按层生成关卡Actor数据
        {
            if (!actorLayerData.Enable) continue;
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
                ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.55f + 0.15f * generatorCount_actorLayer / GenerateActorLayerDataList.Count, "Generating Map Actors");
                yield return null;
            }
        }

        ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.7f, "Generating Map Actors Completed");
        yield return null;

        #endregion

        ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.71f, "Loading Maps");
        yield return null;
        yield return RefreshScopeModules(playerBPWorld, 1, 1, true); // 按关卡生成器和角色位置初始化需要的模组
    }

    protected override IEnumerator GenerateWorldModuleByCustomizedData(WorldModuleData data, int x, int y, int z, int loadBoxNumPerFrame)
    {
        m_LevelCacheData.CurrentShowModuleGPs.Add(new GridPos3D(x, y, z));
        yield return base.GenerateWorldModuleByCustomizedData(data, x, y, z, loadBoxNumPerFrame);
    }

    #region Level Streaming

    public bool IsInsideMicroModules = false;

    public class LevelCacheData
    {
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
            if (GameStateManager.Instance.GetState() == GameState.Fighting)
            {
                if (RefreshScopeModulesCoroutine == null)
                {
                    RefreshScopeModulesCoroutine = StartCoroutine(RefreshScopeModules(BattleManager.Instance.Player1.WorldGP, PlayerScopeRadiusX, PlayerScopeRadiusZ, false));
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

    public IEnumerator RefreshScopeModules(GridPos3D playerWorldGP, int scopeX, int scopeZ, bool isInit)
    {
        GridPos3D playerOnModuleGP = GetModuleGPByWorldGP(playerWorldGP);

        #region Generate Modules

        bool CheckModuleCanBeSeenByCamera(GridPos3D moduleGP, bool checkBottom, bool checkTop, float extendScope)
        {
            if (IsInsideMicroModules) return moduleGP.y >= WORLD_HEIGHT/2; // 当角色在解谜关卡时，隐藏其它模组，且恒定显示解谜模组 todo 此处粗暴地使用了模组坐标来判断是否在解谜关卡，未来需要处理
            if (isInit) return true; // 初始化的时候无视相机位置，因为主角还没加载，相机位置还没确定
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
                GridPos3D diff = new GridPos3D(module_x, 1, module_z) - playerOnModuleGP;

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
                                    break;
                                }
                                case ConfigManager.TypeStartIndex.Enemy:
                                {
                                    string actorTypeName = ConfigManager.GetEnemyTypeName(existedIndex);
                                    moduleData.WorldModuleBornPointGroupData.EnemyBornPoints.Add(
                                        new BornPointData
                                        {
                                            ActorType = actorTypeName,
                                            LocalGP = new GridPos3D(local_x, local_y, local_z),
                                            BornPointAlias = "",
                                            WorldGP = new GridPos3D(world_x, world_y, world_z)
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
                        foreach (KeyValuePair<GridPos3D, WorldModuleDataModification.BoxModification> kv in modification.ModificationDict)
                        {
                            moduleData.BoxMatrix[kv.Key.x, kv.Key.y, kv.Key.z] = kv.Value.BoxTypeIndex;
                            moduleData.BoxOrientationMatrix[kv.Key.x, kv.Key.y, kv.Key.z] = kv.Value.BoxOrientation;
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

        if (isInit) BattleManager.Instance.CreateActorByBornPointData(WorldData.WorldBornPointGroupData_Runtime.PlayerBornPointDataAliasDict[WorldData.DefaultWorldActorBornPointAlias]);

        #endregion

        #region Recycle Modules

        recycleModuleFinished.Clear();
        List<GridPos3D> hideModuleGPs = new List<GridPos3D>();
        foreach (GridPos3D currentShowModuleGP in m_LevelCacheData.CurrentShowModuleGPs)
        {
            bool isGround = currentShowModuleGP.y == 0;
            if (CheckModuleCanBeSeenByCamera(currentShowModuleGP, !isGround, isGround, isGround ? ExtendScope_Recycle_Ground : ExtendScope_Recycle)) continue;
            GridPos3D diff = currentShowModuleGP - playerOnModuleGP;
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
                ClientGameManager.Instance.LoadingMapPanel.Refresh();
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
        worldModule.WorldModuleData.Modification?.SaveData(worldModule.ModuleGP);
        WorldData.WorldBornPointGroupData_Runtime.Dynamic_UnloadModuleData(currentShowModuleGP);
        WorldModuleMatrix[currentShowModuleGP.x, currentShowModuleGP.y, currentShowModuleGP.z] = null;
        yield return worldModule.Clear(false, 256);
        worldModule.PoolRecycle();
        //m_LevelCacheData.WorldModuleDataDict.Remove(currentShowModuleGP);
        recycleModuleFinished[boolIndex] = true;
    }

    IEnumerator Co_GenerateModule(WorldModuleData moduleData, GridPos3D targetModuleGP, int boolIndex)
    {
        yield return GenerateWorldModuleByCustomizedData(moduleData, targetModuleGP.x, targetModuleGP.y, targetModuleGP.z, 32);
        yield return WorldData.WorldBornPointGroupData_Runtime.Dynamic_LoadModuleData(targetModuleGP);
        generateModuleFinished[boolIndex] = true;
    }

    #endregion
}
using System;
using System.Collections;
using System.Collections.Generic;
using BiangLibrary.GameDataFormat;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

public class OpenWorld : World
{
    public int WorldSize_X = 2;
    public int WorldSize_Z = 2;

    public int PlayerScopeRadiusX = 2;
    public int PlayerScopeRadiusZ = 2;

    [Serializable]
    public class GenerateBoxLayerData
    {
        [BoxName]
        [LabelText("箱子类型")]
        [ValueDropdown("GetAllBoxTypeNames")]
        public string BoxTypeName = "None";

        [SerializeField]
        [LabelText("允许覆盖的箱子类型")]
        [ValueDropdown("GetAllBoxTypeNames", IsUniqueList = true, DrawDropdownForListElements = true)]
        private List<string> AllowReplacedBoxTypeNames = new List<string>();

        public HashSet<string> AllowReplacedBoxTypeNameSet = new HashSet<string>();

        [LabelText("只允许覆盖上述箱子上")]
        public bool OnlyOverrideAnyBox = false;

        [LabelText("生成算法")]
        public GenerateAlgorithm m_GenerateAlgorithm = GenerateAlgorithm.CellularAutomata;

        private IEnumerable<string> GetAllBoxTypeNames => ConfigManager.GetAllBoxTypeNames(false);

        [ShowIf("m_GenerateAlgorithm", GenerateAlgorithm.CellularAutomata)]
        [LabelText("初始填充比率")]
        public int FillPercent = 40;

        [ShowIf("m_GenerateAlgorithm", GenerateAlgorithm.CellularAutomata)]
        [LabelText("迭代次数")]
        public int SmoothTimes = 4;

        [ShowIf("m_GenerateAlgorithm", GenerateAlgorithm.CellularAutomata)]
        [LabelText("空地生墙迭代次数")]
        public int SmoothTimes_GenerateWallInOpenSpace = 3;

        [ShowIf("m_GenerateAlgorithm", GenerateAlgorithm.Random)]
        [LabelText("比率：每千格约有多少个")]
        public int BoxCountPerThousandGrid = 20;

        public void Init()
        {
            foreach (string allowReplacedBoxTypeName in AllowReplacedBoxTypeNames)
            {
                AllowReplacedBoxTypeNameSet.Add(allowReplacedBoxTypeName);
            }
        }
    }

    [LabelText("地图生成层级配置")]
    [ListDrawerSettings(ListElementLabelName = "BoxTypeName")]
    public List<GenerateBoxLayerData> GenerateBoxLayerDataList = new List<GenerateBoxLayerData>();

    [Serializable]
    public class GenerateActorLayerData
    {
        [BoxName]
        [LabelText("Actor类型")]
        [ValueDropdown("GetAllActorTypeNames")]
        public string ActorTypeName = "None";

        [SerializeField]
        [LabelText("允许覆盖的箱子类型")]
        [ValueDropdown("GetAllBoxTypeNames", IsUniqueList = true, DrawDropdownForListElements = true)]
        private List<string> AllowReplacedBoxTypeNames = new List<string>();

        public HashSet<string> AllowReplacedBoxTypeNameSet = new HashSet<string>();

        [LabelText("只允许覆盖上述箱子上")]
        public bool OnlyOverrideAnyBox = false;

        [LabelText("生成算法")]
        public GenerateAlgorithm m_GenerateAlgorithm = GenerateAlgorithm.Random;

        private IEnumerable<string> GetAllBoxTypeNames => ConfigManager.GetAllBoxTypeNames(false);
        private IEnumerable<string> GetAllActorTypeNames => ConfigManager.GetAllActorNames(false);

        [ShowIf("m_GenerateAlgorithm", GenerateAlgorithm.Random)]
        [LabelText("比率：每千格约有多少个")]
        public int ActorCountPerThousandGrid = 20;

        public void Init()
        {
            foreach (string allowReplacedBoxTypeName in AllowReplacedBoxTypeNames)
            {
                AllowReplacedBoxTypeNameSet.Add(allowReplacedBoxTypeName);
            }
        }
    }

    public enum GenerateAlgorithm
    {
        CellularAutomata,
        PerlinNoise,
        Random,
    }

    [LabelText("地图生成Actor配置")]
    [ListDrawerSettings(ListElementLabelName = "ActorTypeName")]
    public List<GenerateActorLayerData> GenerateActorLayerDataList = new List<GenerateActorLayerData>();

    // 生成世界的基本思路是，使用一个随机数种子，以及若干个调节参数（最初阶的如怪物密度，各个箱子密度，等），能够生成一个确定的无限世界
    // 暂时先不管数据存储问题，先能够实时生成这个世界，然后做成关卡流式加载即可
    // 关卡流式加载需要平移世界模组矩阵

    // todo 由Perlin Noise来确定每个Module的Seed，即可保证无限地图
    public override IEnumerator Initialize(WorldData worldData)
    {
        m_LevelCacheData = new LevelCacheData();
        WorldData = worldData;

        WorldData.WorldBornPointGroupData_Runtime.InitTempData();
        WorldData.DefaultWorldActorBornPointAlias = "PlayerBP";

        ushort Seed = (ushort) Time.time.ToString().GetHashCode();
        SRandom SRandom = new SRandom(Seed);

        foreach (GenerateBoxLayerData boxLayerData in GenerateBoxLayerDataList) // 初始化所有层的关卡生成器
        {
            boxLayerData.Init();
            MapGenerator generator = null;
            switch (boxLayerData.m_GenerateAlgorithm)
            {
                case GenerateAlgorithm.CellularAutomata:
                {
                    generator = new CellularAutomataMapGenerator(boxLayerData, WorldModule.MODULE_SIZE * WorldSize_X, WorldModule.MODULE_SIZE * WorldSize_Z, boxLayerData.FillPercent, boxLayerData.SmoothTimes, boxLayerData.SmoothTimes_GenerateWallInOpenSpace, SRandom.Next((uint) 9999));
                    yield return null;
                    break;
                }
                case GenerateAlgorithm.PerlinNoise:
                {
                    break;
                }
                case GenerateAlgorithm.Random:
                {
                    generator = new RandomMapGenerator(boxLayerData, WorldModule.MODULE_SIZE * WorldSize_X, WorldModule.MODULE_SIZE * WorldSize_Z, SRandom.Next((uint) 9999));
                    break;
                }
            }

            m_LevelCacheData.CurrentGenerators.Add(generator);
        }

        /*
        foreach (GenerateActorLayerData actorLayerData in GenerateActorLayerDataList) // 按层生成关卡Actor数据
        {
            for (int module_x = 0; module_x < PlayerScopeRadius; module_x++)
            for (int module_z = 0; module_z < PlayerScopeRadius; module_z++)
            {
                WorldModuleData moduleData = m_LevelCacheData.WorldModuleDataDict[new GridPos3D(module_x, 1, module_z)];
                for (int x = 0; x < WorldModule.MODULE_SIZE; x++)
                for (int z = 0; z < WorldModule.MODULE_SIZE; z++)
                {
                    bool isPlayer = actorLayerData.ActorTypeName.Equals("Player1");
                    string bornPointAlias = isPlayer ? WorldData.DefaultWorldActorBornPointAlias : "";
                    if (!isPlayer) continue; // todo 做关卡流式加载暂时关闭怪物加载
                    switch (actorLayerData.m_GenerateAlgorithm)
                    {
                        case GenerateAlgorithm.Random:
                        {
                            GenerateActor(moduleData, actorLayerData, bornPointAlias, x, z, module_x, module_z);
                            break;
                        }
                    }
                }
            }
        }

        void GenerateActor(WorldModuleData moduleData, GenerateActorLayerData actorLayerData, string bornPointAlias, int x, int z, int module_x, int module_z)
        {
            bool genSuc1 = SRandom.Range(0, 1000) < actorLayerData.ActorCountPerThousandGrid;
            if (genSuc1)
            {
                bool genSuc2 = false;
                ushort existedBoxTypeIndex = moduleData.BoxMatrix[x, 0, z];
                if (existedBoxTypeIndex != 0)
                {
                    if (actorLayerData.AllowReplacedBoxTypeNames.Contains(ConfigManager.GetBoxTypeName(existedBoxTypeIndex)))
                    {
                        genSuc2 = true;
                    }
                }
                else
                {
                    if (!actorLayerData.OnlyOverrideAnyBox)
                    {
                        genSuc2 = true;
                    }
                }

                if (genSuc2)
                {
                    moduleData.BoxMatrix[x, 0, z] = 0; // 箱子占位置空
                    GridPos3D BPLocal = new GridPos3D(x, 0, z);
                    GridPos3D BPWorld = new GridPos3D(x + WorldModule.MODULE_SIZE * module_x, WorldModule.MODULE_SIZE, z + WorldModule.MODULE_SIZE * module_z);
                    BornPointData bp = new BornPointData {ActorType = actorLayerData.ActorTypeName, BornPointAlias = bornPointAlias, LocalGP = BPLocal, SpawnLevelEventAlias = "", WorldGP = BPWorld};
                    if (actorLayerData.ActorTypeName.Equals("Player1"))
                    {
                        if (!moduleData.WorldModuleBornPointGroupData.PlayerBornPoints.ContainsKey(bp.BornPointAlias))
                        {
                            moduleData.WorldModuleBornPointGroupData.PlayerBornPoints.Add(bp.BornPointAlias, bp);
                        }
                    }
                    else
                    {
                        moduleData.WorldModuleBornPointGroupData.EnemyBornPoints.Add(bp);
                    }
                }
            }
        }
        */

        yield return RefreshScopeModules(new GridPos3D(10, WorldModule.MODULE_SIZE, 10), PlayerScopeRadiusX, PlayerScopeRadiusZ); // 按关卡生成器和角色位置初始化需要的模组

        if (WorldData.WorldBornPointGroupData_Runtime.PlayerBornPointDataAliasDict.Count == 0) // 实在没有主角出生点
        {
            //Debug.LogError("No space for Player1 to spawn");
            GridPos3D playerBPLocal = new GridPos3D(10, 0, 10);
            GridPos3D playerBPWorld = new GridPos3D(10, WorldModule.MODULE_SIZE, 10);
            BornPointData bp = new BornPointData {ActorType = "Player1", BornPointAlias = "PlayerBP", LocalGP = playerBPLocal, SpawnLevelEventAlias = "", WorldGP = playerBPWorld};
            WorldData.WorldBornPointGroupData_Runtime.PlayerBornPointDataAliasDict.Add(bp.BornPointAlias, bp);
        }

        BattleManager.Instance.CreateActorsByBornPointGroupData(WorldData.WorldBornPointGroupData_Runtime, WorldData.DefaultWorldActorBornPointAlias);
    }

    protected override IEnumerator GenerateWorldModuleByCustomizedData(WorldModuleData data, int x, int y, int z, int loadBoxNumPerFrame, GridPosR.Orientation generateOrder)
    {
        m_LevelCacheData.CurrentShowModuleGPs.Add(new GridPos3D(x, y, z));
        yield return base.GenerateWorldModuleByCustomizedData(data, x, y, z, loadBoxNumPerFrame, generateOrder);
    }

    #region Level Streaming

    public class LevelCacheData
    {
        public List<MapGenerator> CurrentGenerators = new List<MapGenerator>(); // 按箱子layer记录的地图生成信息，未走过的地图或离开很久之后重置的模组按这个数据加载出来

        public Dictionary<GridPos3D, WorldModuleData> WorldModuleDataDict = new Dictionary<GridPos3D, WorldModuleData>();

        public List<GridPos3D> CurrentShowModuleGPs = new List<GridPos3D>();
    }

    private LevelCacheData m_LevelCacheData;
    private float CheckRefreshModuleInterval = 0.1f;
    private float CheckRefreshModuleTick = 0;

    private Coroutine RefreshScopeModulesCoroutine;

    void FixedUpdate()
    {
        if (!IsRecycled)
        {
            if (GameStateManager.Instance.GetState() == GameState.Fighting)
            {
                if (RefreshScopeModulesCoroutine == null)
                {
                    CheckRefreshModuleTick += Time.fixedDeltaTime;
                    if (CheckRefreshModuleTick > CheckRefreshModuleInterval)
                    {
                        RefreshScopeModulesCoroutine = StartCoroutine(RefreshScopeModules(BattleManager.Instance.Player1.WorldGP, PlayerScopeRadiusX, PlayerScopeRadiusZ));
                        CheckRefreshModuleTick = 0;
                    }
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

    List<Vector3> cachedModuleCornerPoints = new List<Vector3>(8);
    Plane[] cachedPlanes = new Plane[6];

    public IEnumerator RefreshScopeModules(GridPos3D playerWorldGP, int scopeX, int scopeZ)
    {
        GridPos3D playerOnModuleGP = GetModuleGPByWorldGP(playerWorldGP);

        #region Generate Modules

        bool CheckModuleCanBeSeenByCamera(GridPos3D moduleGP, bool checkBottom, bool checkTop)
        {
            float extendScope = 4f;
            cachedModuleCornerPoints.Clear();
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
                    Bounds bounds = new Bounds(moduleGP * WorldModule.MODULE_SIZE + (WorldModule.MODULE_SIZE - 1) / 2f * new Vector3(1, 2, 1), new Vector3(WorldModule.MODULE_SIZE + extendScope, 1, WorldModule.MODULE_SIZE + extendScope));
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
                if (!CheckModuleCanBeSeenByCamera(new GridPos3D(module_x, 1, module_z), true, false)) continue;
                GridPos3D diff = new GridPos3D(module_x, 1, module_z) - playerOnModuleGP;
                GridPosR.Orientation generateOrientation = GridPosR.Orientation.Up;
                if (diff.z > 0) generateOrientation = GridPosR.Orientation.Up;
                else if (diff.z < 0) generateOrientation = GridPosR.Orientation.Down;
                else if (diff.x > 0) generateOrientation = GridPosR.Orientation.Right;
                else if (diff.x < 0) generateOrientation = GridPosR.Orientation.Left;

                WorldModule groundModule = WorldModuleMatrix[module_x, 0, module_z];
                if (groundModule == null)
                {
                    GridPos3D targetModuleGP = new GridPos3D(module_x, 0, module_z);
                    WorldModuleData moduleData = WorldModuleData.WorldModuleDataFactory.Alloc();
                    for (int x = 0; x < WorldModule.MODULE_SIZE; x++)
                    for (int z = 0; z < WorldModule.MODULE_SIZE; z++)
                    {
                        moduleData.BoxMatrix[x, 15, z] = ConfigManager.Box_GroundBoxIndex;
                    }

                    moduleData.WorldModuleFeature = WorldModuleFeature.Ground;

                    generateModuleFinished.Add(false);
                    StartCoroutine(Co_GenerateModule(moduleData, targetModuleGP, generateOrientation, generateModuleFinished.Count - 1));
                }

                WorldModule module = WorldModuleMatrix[module_x, 1, module_z];
                if (module == null)
                {
                    WorldModuleData moduleData = null;
                    GridPos3D targetModuleGP = new GridPos3D(module_x, 1, module_z);
                    if (!m_LevelCacheData.WorldModuleDataDict.TryGetValue(targetModuleGP, out moduleData))
                    {
                        moduleData = WorldModuleData.WorldModuleDataFactory.Alloc();
                        m_LevelCacheData.WorldModuleDataDict.Add(targetModuleGP, moduleData);
                        foreach (MapGenerator generator in m_LevelCacheData.CurrentGenerators)
                        {
                            generator.WriteMapInfoIntoWorldModuleData(moduleData, module_x, module_z);
                        }
                    }

                    generateModuleFinished.Add(false);
                    StartCoroutine(Co_GenerateModule(moduleData, targetModuleGP, generateOrientation, generateModuleFinished.Count - 1));
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
            if (CheckModuleCanBeSeenByCamera(currentShowModuleGP, currentShowModuleGP.y != 0, currentShowModuleGP.y == 0)) continue;
            GridPos3D diff = currentShowModuleGP - playerOnModuleGP;
            GridPosR.Orientation generateOrientation = GridPosR.Orientation.Up;
            if (diff.z > 0) generateOrientation = GridPosR.Orientation.Down;
            else if (diff.z < 0) generateOrientation = GridPosR.Orientation.Up;
            else if (diff.x > 0) generateOrientation = GridPosR.Orientation.Left;
            else if (diff.x < 0) generateOrientation = GridPosR.Orientation.Right;
            WorldModule worldModule = WorldModuleMatrix[currentShowModuleGP.x, currentShowModuleGP.y, currentShowModuleGP.z];
            if (worldModule != null)
            {
                recycleModuleFinished.Add(false);
                StartCoroutine(Co_RecycleModule(worldModule, currentShowModuleGP, generateOrientation, recycleModuleFinished.Count - 1));
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
            else yield return null;
        }

        recycleModuleFinished.Clear();

        foreach (GridPos3D hideModuleGP in hideModuleGPs)
        {
            m_LevelCacheData.CurrentShowModuleGPs.Remove(hideModuleGP);
        }

        #endregion

        RefreshScopeModulesCoroutine = null;
    }

    IEnumerator Co_RecycleModule(WorldModule worldModule, GridPos3D currentShowModuleGP, GridPosR.Orientation generateOrientation, int boolIndex)
    {
        WorldData.WorldBornPointGroupData_Runtime.RemoveModuleData(worldModule);
        yield return worldModule.Clear(16, generateOrientation);
        worldModule.PoolRecycle();
        WorldModuleMatrix[currentShowModuleGP.x, currentShowModuleGP.y, currentShowModuleGP.z] = null;
        m_LevelCacheData.WorldModuleDataDict.Remove(currentShowModuleGP);
        recycleModuleFinished[boolIndex] = true;
    }

    IEnumerator Co_GenerateModule(WorldModuleData moduleData, GridPos3D targetModuleGP, GridPosR.Orientation generateOrientation, int boolIndex)
    {
        yield return GenerateWorldModuleByCustomizedData(moduleData, targetModuleGP.x, targetModuleGP.y, targetModuleGP.z, 32, generateOrientation);
        WorldData.WorldBornPointGroupData_Runtime.AddModuleData(WorldModuleMatrix[targetModuleGP.x, targetModuleGP.y, targetModuleGP.z]);
        generateModuleFinished[boolIndex] = true;
    }

    #endregion
}
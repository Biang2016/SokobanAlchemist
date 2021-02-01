using System;
using System.Collections.Generic;
using BiangLibrary.GameDataFormat;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

public class OpenWorld : World
{
    public int WorldSize_X = 2;
    public int WorldSize_Z = 2;

    [Serializable]
    public class GenerateBoxLayerData
    {
        [BoxName]
        [LabelText("箱子类型")]
        [ValueDropdown("GetAllBoxTypeNames")]
        public string BoxTypeName = "None";

        [LabelText("允许覆盖的箱子类型")]
        [ValueDropdown("GetAllBoxTypeNames", IsUniqueList = true, DrawDropdownForListElements = true)]
        public List<string> AllowReplacedBoxTypeNames = new List<string>();

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
        public int SmoothTimes = 20;

        [ShowIf("m_GenerateAlgorithm", GenerateAlgorithm.Random)]
        [LabelText("比率：每千格约有多少个")]
        public int BoxCountPerThousandGrid = 20;
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

        [LabelText("允许覆盖的箱子类型")]
        [ValueDropdown("GetAllBoxTypeNames", IsUniqueList = true, DrawDropdownForListElements = true)]
        public List<string> AllowReplacedBoxTypeNames = new List<string>();

        [LabelText("只允许覆盖上述箱子上")]
        public bool OnlyOverrideAnyBox = false;

        [LabelText("生成算法")]
        public GenerateAlgorithm m_GenerateAlgorithm = GenerateAlgorithm.Random;

        private IEnumerable<string> GetAllBoxTypeNames => ConfigManager.GetAllBoxTypeNames(false);
        private IEnumerable<string> GetAllActorTypeNames => ConfigManager.GetAllActorNames(false);

        [ShowIf("m_GenerateAlgorithm", GenerateAlgorithm.Random)]
        [LabelText("比率：每千格约有多少个")]
        public int ActorCountPerThousandGrid = 20;
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
    public override void Initialize(WorldData worldData)
    {
        void TryWriteBoxIndexTypeIntoMatrix(WorldModuleData moduleData, GenerateBoxLayerData boxLayerData, ushort boxTypeIndex, ushort existedBoxTypeIndex, int x, int z)
        {
            if (existedBoxTypeIndex != 0)
            {
                if (boxLayerData.AllowReplacedBoxTypeNames.Contains(ConfigManager.GetBoxTypeName(existedBoxTypeIndex)))
                {
                    moduleData.BoxMatrix[x, 0, z] = boxTypeIndex;
                }
            }
            else
            {
                if (!boxLayerData.OnlyOverrideAnyBox)
                {
                    moduleData.BoxMatrix[x, 0, z] = boxTypeIndex;
                }
            }
        }

        WorldData = worldData;

        WorldData.WorldBornPointGroupData_Runtime.InitTempData();
        WorldData.DefaultWorldActorBornPointAlias = "PlayerBP";

        WorldModuleData[,] worldModuleDataMatrix = new WorldModuleData[WorldSize_X, WorldSize_Z];

        for (int module_x = 0; module_x < WorldSize_X; module_x++)
        for (int module_z = 0; module_z < WorldSize_Z; module_z++)
        {
            worldModuleDataMatrix[module_x, module_z] = new WorldModuleData();
        }

        ushort Seed = (ushort) Time.time.ToString().GetHashCode();
        SRandom SRandom = new SRandom(Seed);

        foreach (GenerateBoxLayerData boxLayerData in GenerateBoxLayerDataList) // 按层生成关卡Box数据
        {
            ushort boxTypeIndex = ConfigManager.GetBoxTypeIndex(boxLayerData.BoxTypeName);
            switch (boxLayerData.m_GenerateAlgorithm)
            {
                case GenerateAlgorithm.CellularAutomata:
                {
                    CellularAutomataMapGenerator MapGenerator = new CellularAutomataMapGenerator(WorldModule.MODULE_SIZE * WorldSize_X, WorldModule.MODULE_SIZE * WorldSize_Z, boxLayerData.FillPercent, boxLayerData.SmoothTimes, true, Seed);
                    for (int module_x = 0; module_x < WorldSize_X; module_x++)
                    for (int module_z = 0; module_z < WorldSize_Z; module_z++)
                    {
                        WorldModuleData moduleData = worldModuleDataMatrix[module_x, module_z];
                        for (int x = 0; x < WorldModule.MODULE_SIZE; x++)
                        for (int z = 0; z < WorldModule.MODULE_SIZE; z++)
                        {
                            bool genSuc = MapGenerator[x + WorldModule.MODULE_SIZE * module_x, z + WorldModule.MODULE_SIZE * module_z] == 1;
                            if (genSuc)
                            {
                                ushort existedBoxTypeIndex = moduleData.BoxMatrix[x, 0, z];
                                TryWriteBoxIndexTypeIntoMatrix(moduleData, boxLayerData, boxTypeIndex, existedBoxTypeIndex, x, z);
                            }
                        }
                    }

                    break;
                }
                case GenerateAlgorithm.PerlinNoise:
                {
                    // todo 
                    break;
                }
                case GenerateAlgorithm.Random:
                {
                    for (int module_x = 0; module_x < WorldSize_X; module_x++)
                    for (int module_z = 0; module_z < WorldSize_Z; module_z++)
                    {
                        WorldModuleData moduleData = worldModuleDataMatrix[module_x, module_z];
                        for (int x = 0; x < WorldModule.MODULE_SIZE; x++)
                        for (int z = 0; z < WorldModule.MODULE_SIZE; z++)
                        {
                            bool genSuc = SRandom.Range(0, 1000) < boxLayerData.BoxCountPerThousandGrid;
                            if (genSuc)
                            {
                                ushort existedBoxTypeIndex = moduleData.BoxMatrix[x, 0, z];
                                TryWriteBoxIndexTypeIntoMatrix(moduleData, boxLayerData, boxTypeIndex, existedBoxTypeIndex, x, z);
                            }
                        }
                    }

                    break;
                }
            }
        }

        foreach (GenerateActorLayerData actorLayerData in GenerateActorLayerDataList) // 按层生成关卡Actor数据
        {
            for (int module_x = 0; module_x < WorldSize_X; module_x++)
            for (int module_z = 0; module_z < WorldSize_Z; module_z++)
            {
                WorldModuleData moduleData = worldModuleDataMatrix[module_x, module_z];
                for (int x = 0; x < WorldModule.MODULE_SIZE; x++)
                for (int z = 0; z < WorldModule.MODULE_SIZE; z++)
                {
                    string bornPointAlias = actorLayerData.ActorTypeName.Equals("Player1") ? WorldData.DefaultWorldActorBornPointAlias : "";
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

        for (int module_x = 0; module_x < WorldSize_X; module_x++)
        for (int module_z = 0; module_z < WorldSize_Z; module_z++)
        {
            WorldModuleData moduleData = worldModuleDataMatrix[module_x, module_z];
            GenerateWorldModuleByCustomizedData(moduleData, module_x, 1, module_z);
            WorldData.WorldBornPointGroupData_Runtime.AddModuleData(WorldModuleMatrix[module_x, 1, module_z], new GridPos3D(0, 1, 0));
        }

        if (WorldData.WorldBornPointGroupData_Runtime.PlayerBornPointDataAliasDict.Count == 0) // 实在没有主角出生点
        {
            Debug.LogError("No space for Player1 to spawn");
            GridPos3D playerBPLocal = new GridPos3D(10, 0, 10);
            GridPos3D playerBPWorld = new GridPos3D(10, WorldModule.MODULE_SIZE, 10);
            BornPointData bp = new BornPointData {ActorType = "Player1", BornPointAlias = "PlayerBP", LocalGP = playerBPLocal, SpawnLevelEventAlias = "", WorldGP = playerBPWorld};
            WorldData.WorldBornPointGroupData_Runtime.PlayerBornPointDataAliasDict.Add(bp.BornPointAlias, bp);
        }

        // Ground
        for (int module_x = 0; module_x < WorldSize_X; module_x++)
        for (int module_z = 0; module_z < WorldSize_Z; module_z++)
        {
            GenerateWorldModule(ConfigManager.WorldModule_GroundIndex, module_x, 0, module_z);
        }

        BattleManager.Instance.CreateActorsByBornPointGroupData(WorldData.WorldBornPointGroupData_Runtime, WorldData.DefaultWorldActorBornPointAlias);
    }
}
using System;
using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;

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

        [LabelText("初始填充比率")]
        public int FillPercent = 40;

        [LabelText("迭代次数")]
        public int SmoothTimes = 20;

        [LabelText("生成算法")]
        public GenerateAlgorithm m_GenerateAlgorithm = GenerateAlgorithm.CellularAutomata;

        private IEnumerable<string> GetAllBoxTypeNames => ConfigManager.GetAllBoxTypeNames();

        public enum GenerateAlgorithm
        {
            CellularAutomata,
            PerlinNoise,
            Random,
        }
    }

    public List<GenerateBoxLayerData> GenerateBoxLayerDataList = new List<GenerateBoxLayerData>();

    internal MapGenerator MapGenerator;

    public override void OnRecycled()
    {
        base.OnRecycled();
        MapGenerator = null;
    }

    // 生成世界的基本思路是，使用一个随机数种子，以及若干个调节参数（最初阶的如怪物密度，各个箱子密度，等），能够生成一个确定的无限世界
    // 暂时先不管数据存储问题，先能够实时生成这个世界，然后做成关卡流式加载即可
    // 关卡流式加载需要平移世界模组矩阵

    public override void Initialize(WorldData worldData)
    {
        WorldData = worldData;

        WorldData.WorldBornPointGroupData_Runtime.InitTempData();
        WorldData.DefaultWorldActorBornPointAlias = "PlayerBP";

        WorldModuleData[,] worldModuleDataMatrix = new WorldModuleData[WorldSize_X, WorldSize_Z];

        for (int module_x = 0; module_x < WorldSize_X; module_x++)
        for (int module_z = 0; module_z < WorldSize_Z; module_z++)
        {
            worldModuleDataMatrix[module_x, module_z] = new WorldModuleData();
        }

        foreach (GenerateBoxLayerData boxLayerData in GenerateBoxLayerDataList) // 按层生成关卡数据
        {
            ushort boxTypeIndex = ConfigManager.GetBoxTypeIndex(boxLayerData.BoxTypeName);
            MapGenerator = new MapGenerator(WorldModule.MODULE_SIZE * WorldSize_X, WorldModule.MODULE_SIZE * WorldSize_Z, boxLayerData.FillPercent, boxLayerData.SmoothTimes, true, 0);
            for (int module_x = 0; module_x < WorldSize_X; module_x++)
            for (int module_z = 0; module_z < WorldSize_Z; module_z++)
            {
                WorldModuleData moduleData = worldModuleDataMatrix[module_x, module_z];
                for (int x = 0; x < WorldModule.MODULE_SIZE; x++)
                for (int z = 0; z < WorldModule.MODULE_SIZE; z++)
                {
                    bool isBox = MapGenerator[x + WorldModule.MODULE_SIZE * module_x, z + WorldModule.MODULE_SIZE * module_z] == 1;
                    moduleData.BoxMatrix[x, 0, z] = (moduleData.BoxMatrix[x, 0, z] == 0 && isBox) ? boxTypeIndex : moduleData.BoxMatrix[x, 0, z];
                }
            }
        }

        void FindPlayerBornPoint()
        {
            for (int module_x = 0; module_x < WorldSize_X; module_x++)
            for (int module_z = 0; module_z < WorldSize_Z; module_z++)
            {
                WorldModuleData moduleData = worldModuleDataMatrix[module_x, module_z];
                for (int x = 0; x < WorldModule.MODULE_SIZE; x++)
                for (int z = 0; z < WorldModule.MODULE_SIZE; z++)
                {
                    bool empty = moduleData.BoxMatrix[x, 0, z] == 0;
                    if (empty)
                    {
                        GridPos3D playerBPLocal = new GridPos3D(x, 0, z);
                        GridPos3D playerBPWorld = new GridPos3D(x + WorldModule.MODULE_SIZE * module_x, WorldModule.MODULE_SIZE, z + WorldModule.MODULE_SIZE * module_z);
                        BornPointData bp = new BornPointData {ActorType = "Player1", BornPointAlias = "PlayerBP", LocalGP = playerBPLocal, SpawnLevelEventAlias = "", WorldGP = playerBPWorld};
                        moduleData.WorldModuleBornPointGroupData.PlayerBornPoints.Add(bp.BornPointAlias, bp);
                        return;
                    }
                }
            }

            {
                WorldModuleData moduleData = worldModuleDataMatrix[0, 0];
                GridPos3D playerBPLocal = new GridPos3D(10, 0, 10);
                GridPos3D playerBPWorld = new GridPos3D(10, WorldModule.MODULE_SIZE, 10);
                BornPointData bp = new BornPointData {ActorType = "Player1", BornPointAlias = "PlayerBP", LocalGP = playerBPLocal, SpawnLevelEventAlias = "", WorldGP = playerBPWorld};
                moduleData.WorldModuleBornPointGroupData.PlayerBornPoints.Add(bp.BornPointAlias, bp);
            }
        }

        FindPlayerBornPoint();

        for (int module_x = 0; module_x < WorldSize_X; module_x++)
        for (int module_z = 0; module_z < WorldSize_Z; module_z++)
        {
            WorldModuleData moduleData = worldModuleDataMatrix[module_x, module_z];
            GenerateWorldModuleByCustomizedData(moduleData, module_x, 1, module_z);
            WorldData.WorldBornPointGroupData_Runtime.AddModuleData(WorldModuleMatrix[module_x, 1, module_z], new GridPos3D(0, 1, 0));
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
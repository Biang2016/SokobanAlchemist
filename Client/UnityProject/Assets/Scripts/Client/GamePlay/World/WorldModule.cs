using System.Collections.Generic;
using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ObjectPool;
using FlowCanvas;
using UnityEditor;
using UnityEngine;

public class WorldModule : PoolObject
{
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
    private List<LevelTriggerBase> WorldModuleLevelTriggers = new List<LevelTriggerBase>();

    [HideInInspector]
    public Box[,,] BoxMatrix = new Box[MODULE_SIZE, MODULE_SIZE, MODULE_SIZE];

    #region Roots

    private Transform WorldModuleBoxRoot;
    private Transform WorldModuleTriggerRoot;
    private Transform WorldModuleLevelTriggerRoot;

    #endregion

    [SerializeField]
    private FlowScriptController FlowScriptController;

    void Awake()
    {
        WorldModuleBoxRoot = new GameObject("WorldModuleBoxRoot").transform;
        WorldModuleBoxRoot.parent = transform;
        WorldModuleTriggerRoot = new GameObject("WorldModuleTriggerRoot").transform;
        WorldModuleTriggerRoot.parent = transform;
        WorldModuleLevelTriggerRoot = new GameObject("WorldModuleLevelTriggerRoot").transform;
        WorldModuleLevelTriggerRoot.parent = transform;
    }

    public void Clear()
    {
        for (int x = 0; x < BoxMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < BoxMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < BoxMatrix.GetLength(2); z++)
                {
                    BoxMatrix[x, y, z]?.PoolRecycle();
                    BoxMatrix[x, y, z] = null;
                }
            }
        }

        foreach (LevelTriggerBase trigger in WorldModuleLevelTriggers)
        {
            trigger.PoolRecycle();
        }

        WorldModuleLevelTriggers.Clear();

        World = null;
        WorldModuleData = null;
        WorldDeadZoneTrigger?.PoolRecycle();
        WorldDeadZoneTrigger = null;
        WorldWallCollider?.PoolRecycle();
        WorldWallCollider = null;
        WorldGroundCollider?.PoolRecycle();
        WorldGroundCollider = null;
        FlowScriptController.StopBehaviour();
        FlowScriptController.graph = null;
    }

    public void Initialize(WorldModuleData worldModuleData, GridPos3D moduleGP, World world, List<Box.BoxExtraSerializeData> worldBoxExtraSerializeDataList = null)
    {
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

        // Get world box extra serialize data
        Box.BoxExtraSerializeData[,,] worldExtraSerializeDataForOneModule = new Box.BoxExtraSerializeData[WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE];
        if (worldBoxExtraSerializeDataList != null)
        {
            foreach (Box.BoxExtraSerializeData worldBoxExtraData in worldBoxExtraSerializeDataList)
            {
                worldExtraSerializeDataForOneModule[worldBoxExtraData.LocalGP.x, worldBoxExtraData.LocalGP.y, worldBoxExtraData.LocalGP.z] = worldBoxExtraData;
            }
        }

        for (int x = 0; x < worldModuleData.BoxMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < worldModuleData.BoxMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < worldModuleData.BoxMatrix.GetLength(2); z++)
                {
                    ushort boxTypeIndex = worldModuleData.BoxMatrix[x, y, z];
                    if (boxTypeIndex != 0)
                    {
                        Box.BoxExtraSerializeData boxExtraSerializeDataFromModule = worldModuleData.BoxExtraSerializeDataMatrix[x, y, z];
                        Box.BoxExtraSerializeData boxExtraSerializeDataFromWorld = worldExtraSerializeDataForOneModule[x, y, z];
                        GenerateBox(boxTypeIndex, x, y, z, boxExtraSerializeDataFromModule, boxExtraSerializeDataFromWorld);
                    }
                }
            }
        }

        foreach (LevelTriggerBase.Data triggerData in worldModuleData.WorldModuleLevelTriggerGroupData.TriggerDataList)
        {
            LevelTriggerBase trigger = GameObjectPoolManager.Instance.LevelTriggerDict[triggerData.LevelTriggerTypeIndex].AllocateGameObject<LevelTriggerBase>(WorldModuleLevelTriggerRoot);
            trigger.InitializeInWorldModule((LevelTriggerBase.Data) triggerData.Clone());
            WorldModuleLevelTriggers.Add(trigger);
        }

        if (!string.IsNullOrWhiteSpace(worldModuleData.WorldModuleFlowAssetPath))
        {
            FlowScript flowScript = (FlowScript) Resources.Load(worldModuleData.WorldModuleFlowAssetPath);
            if (flowScript)
            {
                FlowScriptController.graph = flowScript;
                FlowScriptController.StartBehaviour();
            }
        }
    }

    public void CreateActorFromWorldModuleData()
    {
        BattleManager.Instance.CreateActorsByBornPointGroupData(WorldModuleData.WorldModuleBornPointGroupData, this);
    }

    public void GenerateBox(ushort boxTypeIndex, GridPos3D localGP, Box.BoxExtraSerializeData boxExtraSerializeDataFromModule = null, Box.BoxExtraSerializeData boxExtraSerializeDataFromWorld = null)
    {
        GenerateBox(boxTypeIndex, localGP.x, localGP.y, localGP.z, boxExtraSerializeDataFromModule, boxExtraSerializeDataFromWorld);
    }

    public void GenerateBox(ushort boxTypeIndex, int x, int y, int z, Box.BoxExtraSerializeData boxExtraSerializeDataFromModule = null, Box.BoxExtraSerializeData boxExtraSerializeDataFromWorld = null)
    {
        if (boxExtraSerializeDataFromWorld != null)
        {
            foreach (BoxFunctionBase bf in boxExtraSerializeDataFromWorld.BoxFunctions)
            {
                if (bf is BoxFunction_Hide)
                {
                    return;
                }
            }
        }

        if (BoxMatrix[x, y, z] != null)
        {
            Debug.LogError($"世界模组{name}的局部坐标({x},{y},{z})位置处已存在Box,请检查世界Box是否重叠放置于该模组已有的Box位置处");
            return;
        }
        else
        {
            Box box = GameObjectPoolManager.Instance.BoxDict[boxTypeIndex].AllocateGameObject<Box>(WorldModuleBoxRoot);
            string boxName = ConfigManager.GetBoxTypeName(boxTypeIndex);
            GridPos3D gp = new GridPos3D(x, y, z);
            box.ApplyBoxExtraSerializeData(boxExtraSerializeDataFromModule, boxExtraSerializeDataFromWorld);
            box.Setup(boxTypeIndex);
            box.Initialize(gp, this, 0, !IsAccessible, Box.LerpType.Create);
            box.name = $"{boxName}_{gp}";
            BoxMatrix[x, y, z] = box;
            return;
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
            if (WorldModuleData != null && WorldModuleData.WorldModuleTypeIndex == WorldManager.DeadZoneIndex)
            {
                Gizmos.color = new Color(1f, 0, 0, 0.7f);
                Gizmos.DrawSphere(transform.position + Vector3.one * (MODULE_SIZE - 1) * 0.5f, 3f);
            }
        }
    }
#endif
}
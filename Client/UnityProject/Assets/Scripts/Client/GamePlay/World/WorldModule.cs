using System.Collections;
using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.Messenger;
using BiangLibrary.ObjectPool;
using FlowCanvas;
using UnityEngine;
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

    public WorldDeadZoneTrigger WorldDeadZoneTrigger;
    public WorldWallCollider WorldWallCollider;
    public WorldGroundCollider WorldGroundCollider;
    private List<LevelTriggerBase> WorldModuleLevelTriggers = new List<LevelTriggerBase>();

    public List<BoxPassiveSkill_LevelEventTriggerAppear> EventTriggerAppearBoxPassiveSkillList = new List<BoxPassiveSkill_LevelEventTriggerAppear>();

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

        foreach (BoxPassiveSkill_LevelEventTriggerAppear bf in EventTriggerAppearBoxPassiveSkillList)
        {
            bf.ClearAndUnRegister();
        }

        EventTriggerAppearBoxPassiveSkillList.Clear();

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

    public void Initialize(WorldModuleData worldModuleData, GridPos3D moduleGP, World world, List<Box_LevelEditor.BoxExtraSerializeData> worldBoxExtraSerializeDataList = null)
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
        Box_LevelEditor.BoxExtraSerializeData[,,] worldExtraSerializeDataForOneModule = new Box_LevelEditor.BoxExtraSerializeData[WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE];
        if (worldBoxExtraSerializeDataList != null)
        {
            foreach (Box_LevelEditor.BoxExtraSerializeData worldBoxExtraData in worldBoxExtraSerializeDataList)
            {
                worldExtraSerializeDataForOneModule[worldBoxExtraData.LocalGP.x, worldBoxExtraData.LocalGP.y, worldBoxExtraData.LocalGP.z] = worldBoxExtraData;
            }
        }

        foreach (BoxPassiveSkill_LevelEventTriggerAppear.Data data in worldModuleData.EventTriggerAppearBoxDataList)
        {
            BoxPassiveSkill_LevelEventTriggerAppear.Data dataClone = (BoxPassiveSkill_LevelEventTriggerAppear.Data) data.Clone();
            BoxPassiveSkill_LevelEventTriggerAppear bf = dataClone.BoxPassiveSkill_LevelEventTriggerAppear;
            GridPos3D localGP = data.LocalGP;
            Box_LevelEditor.BoxExtraSerializeData boxExtraSerializeDataFromModule = worldModuleData.BoxExtraSerializeDataMatrix[localGP.x, localGP.y, localGP.z]; // 这里没有LevelEventTriggerBF的覆写信息
            Box_LevelEditor.BoxExtraSerializeData boxExtraSerializeDataFromWorld = worldExtraSerializeDataForOneModule[localGP.x, localGP.y, localGP.z]; // 这里可能有LevelEventTriggerBF的覆写信息
            if (boxExtraSerializeDataFromWorld != null)
            {
                foreach (BoxPassiveSkill worldBF in boxExtraSerializeDataFromWorld.BoxPassiveSkills)
                {
                    if (worldBF is BoxPassiveSkill_LevelEventTriggerAppear appear)
                    {
                        bf.CopyDataFrom(appear); // 此处仅拷贝这个BF的数据，其他BF在GenerateBox的时候再拷贝
                        break;
                    }
                }
            }

            bf.GenerateBoxAction = () =>
            {
                BoxMatrix[localGP.x, localGP.y, localGP.z]?.DestroyBox(); // 强行删除该格占用Box
                Box box = GenerateBox(dataClone.BoxTypeIndex, localGP, data.BoxOrientation, true, boxExtraSerializeDataFromModule, boxExtraSerializeDataFromWorld);
                box.name = box.name + "_Generated";

                // Box生成后此BoxPassiveSkill及注册的事件均作废
                bf.ClearAndUnRegister();
                EventTriggerAppearBoxPassiveSkillList.Remove(bf);
            };
            bf.OnRegisterLevelEventID();
            EventTriggerAppearBoxPassiveSkillList.Add(bf);
        }

        int generatedBoxCount = 0;
        for (int x = 0; x < worldModuleData.BoxMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < worldModuleData.BoxMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < worldModuleData.BoxMatrix.GetLength(2); z++)
                {
                    ushort boxTypeIndex = worldModuleData.BoxMatrix[x, y, z];
                    GridPosR.Orientation boxOrientation = worldModuleData.BoxOrientationMatrix[x, y, z];
                    if (boxTypeIndex != 0)
                    {
                        Box_LevelEditor.BoxExtraSerializeData boxExtraSerializeDataFromModule = worldModuleData.BoxExtraSerializeDataMatrix[x, y, z];
                        Box_LevelEditor.BoxExtraSerializeData boxExtraSerializeDataFromWorld = worldExtraSerializeDataForOneModule[x, y, z];
                        GenerateBox(boxTypeIndex, x, y, z, boxOrientation, false, boxExtraSerializeDataFromModule, boxExtraSerializeDataFromWorld);
                        generatedBoxCount++;
                        if (generatedBoxCount > 256)
                        {
                            generatedBoxCount -= 256;
                            //yield return null;
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
    }

    public void GenerateLevelTrigger(LevelTriggerBase.Data dataClone)
    {
        LevelTriggerBase trigger = GameObjectPoolManager.Instance.LevelTriggerDict[dataClone.LevelTriggerTypeIndex].AllocateGameObject<LevelTriggerBase>(WorldModuleLevelTriggerRoot);
        trigger.InitializeInWorldModule(dataClone);
        WorldModuleLevelTriggers.Add(trigger);
    }

    public Box GenerateBox(ushort boxTypeIndex, GridPos3D localGP, GridPosR.Orientation orientation, bool isTriggerAppear = false, Box_LevelEditor.BoxExtraSerializeData boxExtraSerializeDataFromModule = null, Box_LevelEditor.BoxExtraSerializeData boxExtraSerializeDataFromWorld = null)
    {
        return GenerateBox(boxTypeIndex, localGP.x, localGP.y, localGP.z, orientation, isTriggerAppear, boxExtraSerializeDataFromModule, boxExtraSerializeDataFromWorld);
    }

    public Box GenerateBox(ushort boxTypeIndex, int x, int y, int z, GridPosR.Orientation orientation, bool isTriggerAppear = false, Box_LevelEditor.BoxExtraSerializeData boxExtraSerializeDataFromModule = null, Box_LevelEditor.BoxExtraSerializeData boxExtraSerializeDataFromWorld = null)
    {
        if (boxExtraSerializeDataFromWorld != null)
        {
            foreach (BoxPassiveSkill bf in boxExtraSerializeDataFromWorld.BoxPassiveSkills)
            {
                if (bf is BoxPassiveSkill_Hide)
                {
                    return null;
                }
            }
        }

        if (BoxMatrix[x, y, z] != null)
        {
            //Debug.LogError($"世界模组{name}的局部坐标({x},{y},{z})位置处已存在Box,请检查世界Box是否重叠放置于该模组已有的Box位置处");
            return null;
        }
        else
        {
            List<GridPos3D> boxOccupation = ConfigManager.GetBoxOccupationData(boxTypeIndex);
            List<GridPos3D> boxOccupation_rotated = GridPos3D.TransformOccupiedPositions_XZ(orientation, boxOccupation);
            foreach (GridPos3D gridPos3D in boxOccupation_rotated)
            {
                GridPos3D gridPos = gridPos3D + new GridPos3D(x, y, z);
                if (BoxMatrix[gridPos.x, gridPos.y, gridPos.z] != null)
                {
                    //Debug.LogError($"世界模组{name}的局部坐标({gridPos})位置处已存在Box,请检查世界Box是否重叠放置于该模组已有的Box位置处");
                    return null;
                }
            }

            Box box = GameObjectPoolManager.Instance.BoxDict[boxTypeIndex].AllocateGameObject<Box>(WorldModuleBoxRoot);
            string boxName = ConfigManager.GetBoxTypeName(boxTypeIndex);
            GridPos3D gp = new GridPos3D(x, y, z);
            box.ApplyBoxExtraSerializeData(boxExtraSerializeDataFromModule, boxExtraSerializeDataFromWorld);
            box.Setup(boxTypeIndex, orientation);
            box.Initialize(gp, this, 0, !IsAccessible, Box.LerpType.Create, false, !isTriggerAppear); // 如果是TriggerAppear的箱子则不需要检查坠落
            box.name = $"{boxName}_{gp}";

            foreach (GridPos3D gridPos3D in boxOccupation_rotated)
            {
                GridPos3D gridPos = gridPos3D + new GridPos3D(x, y, z);
                BoxMatrix[gridPos.x, gridPos.y, gridPos.z] = box;
            }

            return box;
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
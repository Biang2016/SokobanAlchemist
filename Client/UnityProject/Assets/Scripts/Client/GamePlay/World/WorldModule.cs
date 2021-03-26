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

    public List<BoxPassiveSkill_LevelEventTriggerAppear> EventTriggerAppearBoxPassiveSkillList = new List<BoxPassiveSkill_LevelEventTriggerAppear>();

    [HideInInspector]
    private Box[,,] BoxMatrix = new Box[MODULE_SIZE, MODULE_SIZE, MODULE_SIZE];

    public Box this[int x, int y, int z] => BoxMatrix[x, y, z];

    public Box this[GridPos3D gp, bool isCore, GridPosR.Orientation orientation]
    {
        set => this[gp.x, gp.y, gp.z, isCore, orientation] = value;
    }

    // 此索引仅仅用于战斗时的Set，不可用于Recycle时候置空 
    public Box this[int x, int y, int z, bool isCore, GridPosR.Orientation orientation]
    {
        set
        {
            BoxMatrix[x, y, z] = value;

            if (WorldModuleData.Modification != null && WorldModuleData.Modification.Enable)
            {
                GridPos3D localGP = new GridPos3D(x, y, z);
                if (!isCore) return; // 异形箱子会对Matrix每一格设置一个引用，因此需要排除掉非核心格
                ushort boxTypeIndex = 0;
                GridPosR.Orientation boxOrientation = orientation;
                if (value != null)
                {
                    boxTypeIndex = value.EntityTypeIndex;
                }

                WorldModuleDataModification.BoxModification mod = new WorldModuleDataModification.BoxModification(boxTypeIndex, boxOrientation);
                if (WorldModuleData.RawBoxMatrix[x, y, z] == boxTypeIndex && WorldModuleData.RawBoxOrientationMatrix[x, y, z] == boxOrientation) // 和初始数据一致，则将改动抹除
                {
                    WorldModuleData.Modification.BoxModificationDict.Remove(localGP);
                }
                else
                {
                    if (WorldModuleData.Modification.BoxModificationDict.ContainsKey(localGP))
                    {
                        WorldModuleData.Modification.BoxModificationDict[localGP] = mod;
                    }
                    else
                    {
                        WorldModuleData.Modification.BoxModificationDict.Add(localGP, mod);
                    }
                }
            }
        }
    }

    public Box GetBox(GridPos3D gp)
    {
        return BoxMatrix[gp.x, gp.y, gp.z];
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

    public IEnumerator Clear(bool releaseWorldModuleData, int clearBoxNumPerFrame = 256)
    {
        IsGeneratingOrRecycling = true;
        int count = 0;

        for (int x = 0; x < MODULE_SIZE; x++)
        {
            for (int y = 0; y < MODULE_SIZE; y++)
            {
                for (int z = 0; z < MODULE_SIZE; z++)
                {
                    Box box = BoxMatrix[x, y, z];
                    if (box != null)
                    {
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
                                }
                            }

                            box.PoolRecycle();
                            count++;
                            if (count > clearBoxNumPerFrame)
                            {
                                count = 0;
                                yield return null;
                            }
                        }
                    }

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

    public virtual IEnumerator Initialize(WorldModuleData worldModuleData, GridPos3D moduleGP, World world, int loadBoxNumPerFrame)
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

        foreach (BoxPassiveSkill_LevelEventTriggerAppear.Data data in worldModuleData.EventTriggerAppearBoxDataList)
        {
            BoxPassiveSkill_LevelEventTriggerAppear.Data dataClone = (BoxPassiveSkill_LevelEventTriggerAppear.Data) data.Clone();
            BoxPassiveSkill_LevelEventTriggerAppear bf = dataClone.BoxPassiveSkill_LevelEventTriggerAppear;
            GridPos3D localGP = data.LocalGP;
            EntityExtraSerializeData boxExtraSerializeDataFromModule = worldModuleData.BoxExtraSerializeDataMatrix[localGP.x, localGP.y, localGP.z]; // 这里没有LevelEventTriggerBF的覆写信息
            bf.GenerateBoxAction = () =>
            {
                BoxMatrix[localGP.x, localGP.y, localGP.z]?.DestroyBox(null, true); // 强行删除该格占用Box
                Box box = GenerateBox(dataClone.BoxTypeIndex, LocalGPToWorldGP(localGP), data.BoxOrientation, true, false, boxExtraSerializeDataFromModule);
                box.name = box.name + "_Generated";

                // Box生成后此BoxPassiveSkill及注册的事件均作废
                bf.ClearAndUnRegister();
                EventTriggerAppearBoxPassiveSkillList.Remove(bf);
            };
            bf.InitWorldModuleGUID = GUID; // 这里特例处理了这个类型的PassiveSkill，因为它没有依托的Entity，所以无法从Entity中取GUID，只能外界传进去
            bf.OnRegisterLevelEventID(); // 特例不调用OnInit()
            EventTriggerAppearBoxPassiveSkillList.Add(bf);
        }

        int loadBoxCount = 0;
        for (int x = 0; x < MODULE_SIZE; x++)
        {
            for (int y = 0; y < MODULE_SIZE; y++)
            {
                for (int z = 0; z < MODULE_SIZE; z++)
                {
                    if (generateBox(x, y, z))
                    {
                        loadBoxCount++;
                        if (loadBoxCount >= loadBoxNumPerFrame)
                        {
                            loadBoxCount = 0;
                            yield return null;
                        }
                    }
                }
            }
        }

        bool generateBox(int x, int y, int z)
        {
            ushort boxTypeIndex = worldModuleData.BoxMatrix[x, y, z];
            GridPosR.Orientation boxOrientation = worldModuleData.BoxOrientationMatrix[x, y, z];
            if (boxTypeIndex != 0)
            {
                EntityExtraSerializeData boxExtraSerializeDataFromModule = worldModuleData.BoxExtraSerializeDataMatrix[x, y, z];
                this.GenerateBox(boxTypeIndex, LocalGPToWorldGP(new GridPos3D(x, y, z)), boxOrientation, false, true, boxExtraSerializeDataFromModule);
                return true;
            }

            return false;
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

    public Box GenerateBox(ushort boxTypeIndex, GridPos3D worldGP, GridPosR.Orientation orientation, bool isTriggerAppear = false, bool isStartedBoxes = false, EntityExtraSerializeData boxExtraSerializeDataFromModule = null, bool findSpaceUpward = false, List<GridPos3D> overrideOccupation = null)
    {
        GridPos3D localGP = WorldGPToLocalGP(worldGP);
        bool valid = true;

        if (BoxMatrix[localGP.x, localGP.y, localGP.z] == null)
        {
            // Probability Check
            if (boxExtraSerializeDataFromModule != null)
            {
                uint probabilityShow = 100;
                foreach (EntityPassiveSkill eps in boxExtraSerializeDataFromModule.EntityPassiveSkills)
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
                boxOccupation_rotated = ConfigManager.GetEntityOccupationData(boxTypeIndex).EntityIndicatorGPs_RotatedDict[orientation];
            }

            // 空位检查
            foreach (GridPos3D offset in boxOccupation_rotated)
            {
                GridPos3D gridPos = offset + localGP;
                if (gridPos.InsideModule())
                {
                    if (BoxMatrix[gridPos.x, gridPos.y, gridPos.z] != null)
                    {
                        valid = false;
                    }
                }
                else // 如果合成的是异形箱子则需要考虑该箱子的一部分是否放到了其他模组里
                {
                    GridPos3D gridWorldGP = offset + worldGP;
                    Box boxInOtherModule = World.GetBoxByGridPosition(gridWorldGP, out WorldModule otherModule, out GridPos3D _);
                    if (otherModule != null && boxInOtherModule != null)
                    {
                        valid = false;
                    }
                }
            }

            if (valid)
            {
                Box box = GameObjectPoolManager.Instance.BoxDict[boxTypeIndex].AllocateGameObject<Box>(WorldModuleBoxRoot);
                if (box.BoxFrozenBoxHelper != null)
                {
                    box.BoxFrozenBoxHelper.FrozenBoxOccupation = boxOccupation_rotated;
                }

                box.Setup(boxTypeIndex, orientation, GUID);
                box.Initialize(worldGP, this, 0, !IsAccessible, Box.LerpType.Create, false, !isTriggerAppear && !isStartedBoxes); // 如果是TriggerAppear的箱子则不需要检查坠落
                box.ApplyBoxExtraSerializeData(boxExtraSerializeDataFromModule);

                foreach (GridPos3D offset in boxOccupation_rotated)
                {
                    GridPos3D gridPos = offset + localGP;
                    if (gridPos.InsideModule())
                    {
                        if (isStartedBoxes)
                        {
                            BoxMatrix[gridPos.x, gridPos.y, gridPos.z] = box;
                        }
                        else
                        {
                            this[gridPos, offset == GridPos3D.Zero, orientation] = box;
                        }
                    }
                    else // 如果合成的是异形箱子则需要考虑该箱子的一部分是否放到了其他模组里
                    {
                        GridPos3D gridWorldGP = offset + worldGP;
                        Box boxInOtherModule = World.GetBoxByGridPosition(gridWorldGP, out WorldModule otherModule, out GridPos3D otherModuleLocalGP);
                        if (otherModule != null && boxInOtherModule == null)
                        {
                            if (isStartedBoxes)
                            {
                                otherModule.BoxMatrix[otherModuleLocalGP.x, otherModuleLocalGP.y, otherModuleLocalGP.z] = box;
                            }
                            else
                            {
                                otherModule[otherModuleLocalGP, offset == GridPos3D.Zero, orientation] = box;
                            }
                        }
                    }
                }

                return box;
            }
        }

        if (findSpaceUpward)
        {
            worldGP += GridPos3D.Up;
            WorldModule module = World.GetModuleByWorldGP(worldGP);
            if (module != null)
            {
                return module.GenerateBox(boxTypeIndex, worldGP, orientation, isTriggerAppear, isStartedBoxes, boxExtraSerializeDataFromModule, findSpaceUpward);
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
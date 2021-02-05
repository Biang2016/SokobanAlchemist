﻿using System.Collections;
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
    protected List<LevelTriggerBase> WorldModuleLevelTriggers = new List<LevelTriggerBase>();

    public List<BoxPassiveSkill_LevelEventTriggerAppear> EventTriggerAppearBoxPassiveSkillList = new List<BoxPassiveSkill_LevelEventTriggerAppear>();

    [HideInInspector]
    public Box[,,] BoxMatrix = new Box[MODULE_SIZE, MODULE_SIZE, MODULE_SIZE];

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

    public IEnumerator Clear()
    {
        int count = 0;
        for (int x = 0; x < BoxMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < BoxMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < BoxMatrix.GetLength(2); z++)
                {
                    Box box = BoxMatrix[x, y, z];
                    if (box != null)
                    {
                        box.PoolRecycle();
                        BoxMatrix[x, y, z] = null;
                        count++;
                        if (count > 64)
                        {
                            count = 0;
                            yield return null;
                        }
                    }
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

    public virtual IEnumerator Initialize(WorldModuleData worldModuleData, GridPos3D moduleGP, World world, int loadBoxNumPerFrame, GridPosR.Orientation generatorOrder = GridPosR.Orientation.Right)
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

        foreach (BoxPassiveSkill_LevelEventTriggerAppear.Data data in worldModuleData.EventTriggerAppearBoxDataList)
        {
            BoxPassiveSkill_LevelEventTriggerAppear.Data dataClone = (BoxPassiveSkill_LevelEventTriggerAppear.Data) data.Clone();
            BoxPassiveSkill_LevelEventTriggerAppear bf = dataClone.BoxPassiveSkill_LevelEventTriggerAppear;
            GridPos3D localGP = data.LocalGP;
            Box_LevelEditor.BoxExtraSerializeData boxExtraSerializeDataFromModule = worldModuleData.BoxExtraSerializeDataMatrix[localGP.x, localGP.y, localGP.z]; // 这里没有LevelEventTriggerBF的覆写信息
            bf.GenerateBoxAction = () =>
            {
                BoxMatrix[localGP.x, localGP.y, localGP.z]?.DestroyBox(); // 强行删除该格占用Box
                Box box = GenerateBox(dataClone.BoxTypeIndex, LocalGPToWorldGP(localGP), data.BoxOrientation, true, false, boxExtraSerializeDataFromModule);
                box.name = box.name + "_Generated";

                // Box生成后此BoxPassiveSkill及注册的事件均作废
                bf.ClearAndUnRegister();
                EventTriggerAppearBoxPassiveSkillList.Remove(bf);
            };
            bf.OnRegisterLevelEventID();
            EventTriggerAppearBoxPassiveSkillList.Add(bf);
        }

        int loadBoxCount = 0;
        switch (generatorOrder)
        {
            case GridPosR.Orientation.Right:
            {
                for (int x = 0; x < worldModuleData.BoxMatrix.GetLength(0); x++)
                {
                    for (int y = 0; y < worldModuleData.BoxMatrix.GetLength(1); y++)
                    {
                        for (int z = 0; z < worldModuleData.BoxMatrix.GetLength(2); z++)
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

                break;
            }
            case GridPosR.Orientation.Left:
            {
                for (int x = worldModuleData.BoxMatrix.GetLength(0) - 1; x >= 0; x--)
                {
                    for (int y = 0; y < worldModuleData.BoxMatrix.GetLength(1); y++)
                    {
                        for (int z = 0; z < worldModuleData.BoxMatrix.GetLength(2); z++)
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

                break;
            }
            case GridPosR.Orientation.Up:
            {
                for (int z = 0; z < worldModuleData.BoxMatrix.GetLength(2); z++)
                {
                    for (int x = 0; x < worldModuleData.BoxMatrix.GetLength(0); x++)
                    {
                        for (int y = 0; y < worldModuleData.BoxMatrix.GetLength(1); y++)
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

                break;
            }
            case GridPosR.Orientation.Down:
            {
                for (int z = worldModuleData.BoxMatrix.GetLength(2) - 1; z >= 0; z--)
                {
                    for (int x = 0; x < worldModuleData.BoxMatrix.GetLength(0); x++)
                    {
                        for (int y = 0; y < worldModuleData.BoxMatrix.GetLength(1); y++)
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

                break;
            }
        }

        bool generateBox(int x, int y, int z)
        {
            ushort boxTypeIndex = worldModuleData.BoxMatrix[x, y, z];
            GridPosR.Orientation boxOrientation = worldModuleData.BoxOrientationMatrix[x, y, z];
            if (boxTypeIndex != 0)
            {
                Box_LevelEditor.BoxExtraSerializeData boxExtraSerializeDataFromModule = worldModuleData.BoxExtraSerializeDataMatrix[x, y, z];
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
    }

    public void GenerateLevelTrigger(LevelTriggerBase.Data dataClone)
    {
        LevelTriggerBase trigger = GameObjectPoolManager.Instance.LevelTriggerDict[dataClone.LevelTriggerTypeIndex].AllocateGameObject<LevelTriggerBase>(WorldModuleLevelTriggerRoot);
        trigger.InitializeInWorldModule(dataClone);
        WorldModuleLevelTriggers.Add(trigger);
    }

    public Box GenerateBox(ushort boxTypeIndex, GridPos3D worldGP, GridPosR.Orientation orientation, bool isTriggerAppear = false, bool isStartedBoxes = false, Box_LevelEditor.BoxExtraSerializeData boxExtraSerializeDataFromModule = null)
    {
        GridPos3D localGP = WorldGPToLocalGP(worldGP);

        if (BoxMatrix[localGP.x, localGP.y, localGP.z] != null)
        {
            //Debug.LogError($"世界模组{name}的局部坐标({localGP})位置处已存在Box,请检查世界Box是否重叠放置于该模组已有的Box位置处");
            return null;
        }
        else
        {
            List<GridPos3D> boxOccupation = ConfigManager.GetBoxOccupationData(boxTypeIndex).BoxIndicatorGPs;
            List<GridPos3D> boxOccupation_rotated = GridPos3D.TransformOccupiedPositions_XZ(orientation, boxOccupation);

            // 空位检查
            foreach (GridPos3D offset in boxOccupation_rotated)
            {
                GridPos3D gridPos = offset + localGP;
                if (gridPos.InsideModule())
                {
                    if (BoxMatrix[gridPos.x, gridPos.y, gridPos.z] != null)
                    {
                        //Debug.LogError($"世界模组{name}的局部坐标({gridPos})位置处已存在Box,请检查世界Box是否重叠放置于该模组已有的Box位置处");
                        return null;
                    }
                }
                else // 如果合成的是异形箱子则需要考虑该箱子的一部分是否放到了其他模组里
                {
                    GridPos3D gridWorldGP = offset + worldGP;
                    Box boxInOtherModule = World.GetBoxByGridPosition(gridWorldGP, out WorldModule otherModule, out GridPos3D _);
                    if (otherModule != null && boxInOtherModule != null)
                    {
                        return null;
                    }
                }
            }

            Box box = GameObjectPoolManager.Instance.BoxDict[boxTypeIndex].AllocateGameObject<Box>(WorldModuleBoxRoot);

            box.Setup(boxTypeIndex, orientation);
            box.Initialize(worldGP, this, 0, !IsAccessible, Box.LerpType.Create, false, !isTriggerAppear && !isStartedBoxes); // 如果是TriggerAppear的箱子则不需要检查坠落
            box.ApplyBoxExtraSerializeData(boxExtraSerializeDataFromModule);

            foreach (GridPos3D offset in boxOccupation_rotated)
            {
                GridPos3D gridPos = offset + localGP;
                if (gridPos.InsideModule())
                {
                    BoxMatrix[gridPos.x, gridPos.y, gridPos.z] = box;
                }
                else // 如果合成的是异形箱子则需要考虑该箱子的一部分是否放到了其他模组里
                {
                    GridPos3D gridWorldGP = offset + worldGP;
                    Box boxInOtherModule = World.GetBoxByGridPosition(gridWorldGP, out WorldModule otherModule, out GridPos3D otherModuleLocalGP);
                    if (otherModule != null && boxInOtherModule == null)
                    {
                        otherModule.BoxMatrix[otherModuleLocalGP.x, otherModuleLocalGP.y, otherModuleLocalGP.z] = box;
                    }
                }
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
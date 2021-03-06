﻿using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat;
using BiangLibrary.GameDataFormat.Grid;

public abstract class MapGenerator
{
    protected uint Seed;
    protected SRandom SRandom;
    protected GenerateLayerData GenerateLayerData;

    protected ushort TypeIndex;

    protected OpenWorld m_OpenWorld;
    public Dictionary<TypeDefineType, EntityData[,,]> WorldMap_EntityDataMatrix => m_OpenWorld.WorldMap_EntityDataMatrix;
    protected TerrainType[,] WorldMap_TerrainType => m_OpenWorld.WorldMap_TerrainType;
    protected ushort[,,] WorldMap_StaticLayoutOccupied_IntactForStaticLayout => m_OpenWorld.WorldMap_StaticLayoutOccupied_IntactForStaticLayout;
    protected ushort[,,] WorldMap_StaticLayoutOccupied_IntactForBox => m_OpenWorld.WorldMap_StaticLayoutOccupied_IntactForBox;

    protected int Width;
    protected int Depth;
    protected int Height = WorldModule.MODULE_SIZE;

    protected MapGenerator(GenerateLayerData layerData, int width, int depth, uint seed, OpenWorld openWorld)
    {
        m_OpenWorld = openWorld;
        Seed = seed;
        SRandom = new SRandom(seed);
        GenerateLayerData = layerData;
        TypeIndex = ConfigManager.GetTypeIndex(GenerateLayerData.TypeName.TypeDefineType, GenerateLayerData.TypeName.TypeName);
        Width = width;
        Depth = depth;
    }

    public abstract void ApplyToWorldMap();

    private static List<GridPos3D> cached_IntactGPs_World = new List<GridPos3D>(1024); // 静态布局内所有的Y值为0的成功放置的箱子的LocalGP

    protected bool TryOverrideToWorldMap(GridPos3D worldGP, ushort writeEntityTypeIndex, GridPosR.Orientation writeOrientation)
    {
        bool CheckGridAvailable(GridPos3D grid_world, KeyValuePair<TypeDefineType, int> EntityDataMatrixKV, ref bool available)
        {
            if (CheckGridInsideWorldRange(grid_world, 1))
            {
                if (WorldMap_StaticLayoutOccupied_IntactForStaticLayout[grid_world.x, grid_world.y - Height, grid_world.z] != 0)
                {
                    available = false;
                    return true;
                }

                if (m_OpenWorld.CheckOccupied(EntityDataMatrixKV.Key, grid_world))
                {
                    available = false;
                    return true;
                }

                TerrainType terrainType = WorldMap_TerrainType[grid_world.x, grid_world.z];
                if (GenerateLayerData.OnlyAllowPutOnTerrain && !GenerateLayerData.AllowPlaceOnTerrainTypeSet.Contains(terrainType))
                {
                    available = false;
                    return true;
                }

                if (!GenerateLayerData.OnlyAllowPutOnTerrain && GenerateLayerData.ForbidPlaceOnTerrainTypeSet.Contains(terrainType))
                {
                    available = false;
                    return true;
                }
            }
            else
            {
                available = false;
                return true;
            }

            return false;
        }

        bool CheckGridInsideWorldRange(GridPos3D _worldGP, int borderThickness)
        {
            return _worldGP.x >= borderThickness && _worldGP.x < Width - borderThickness && _worldGP.y - Height >= 0 && _worldGP.y - Height < Height && _worldGP.z >= borderThickness && _worldGP.z < Depth - borderThickness;
        }

        bool overrideSuc = false;
        ushort existedIndex_StaticLayoutIndex_IntactForBox = WorldMap_StaticLayoutOccupied_IntactForBox[worldGP.x, worldGP.y - Height, worldGP.z];
        switch (GenerateLayerData.TypeName.TypeDefineType)
        {
            case TypeDefineType.StaticLayout:
            {
                GenerateStaticLayoutLayerData staticLayoutLayerData = (GenerateStaticLayoutLayerData) GenerateLayerData;
                WorldModuleData staticLayoutData = ConfigManager.GetStaticLayoutDataConfig(TypeIndex, false); // 不拷贝，只读数据，避免运行时动态加载GC
                GridPosR.Orientation staticLayoutOrientation = writeOrientation;

                // world: 静态布局Pivot点的世界坐标
                // sl_local: 静态布局Config中未旋转的坐标
                // rot_local: 静态布局旋转后对应的坐标
                // box_world: 静态布局旋转后对应的世界坐标
                // box_grid_world: 旋转后的静态布局中的箱子的每一格对应的世界坐标

                // 检查范围内是否允许放置静态布局
                bool allowPutStaticLayout = true;
                if (!staticLayoutLayerData.AllowFragment)
                {
                    for (int sl_local_x = 0; sl_local_x < WorldModule.MODULE_SIZE; sl_local_x++)
                    {
                        if (!allowPutStaticLayout) break;
                        for (int sl_local_y = 0; sl_local_y < WorldModule.MODULE_SIZE; sl_local_y++)
                        {
                            if (!allowPutStaticLayout) break;
                            for (int sl_local_z = 0; sl_local_z < WorldModule.MODULE_SIZE; sl_local_z++)
                            {
                                if (!allowPutStaticLayout) break;
                                GridPos3D sl_local = new GridPos3D(sl_local_x, sl_local_y, sl_local_z);
                                GridPos3D rot_local = sl_local;
                                for (int rotCount = 0; rotCount < (int) staticLayoutOrientation; rotCount++) // 旋转
                                {
                                    rot_local = new GridPos3D(rot_local.z, rot_local.y, WorldModule.MODULE_SIZE - 1 - rot_local.x);
                                }

                                foreach (KeyValuePair<TypeDefineType, int> kv in WorldModuleData.EntityDataMatrixKeys)
                                {
                                    EntityData raw_StaticLayoutEntityData = staticLayoutData[kv.Key, sl_local];
                                    if (raw_StaticLayoutEntityData != null)
                                    {
                                        GridPos3D box_world = worldGP + rot_local;
                                        EntityOccupationData occupation = ConfigManager.GetEntityOccupationData(raw_StaticLayoutEntityData.EntityTypeIndex);
                                        GridPosR.Orientation rot = GridPosR.RotateOrientationClockwise90(raw_StaticLayoutEntityData.EntityOrientation, (int) staticLayoutOrientation);
                                        foreach (GridPos3D gridPos in occupation.EntityIndicatorGPs_RotatedDict[rot])
                                        {
                                            GridPos3D box_grid_world = box_world + gridPos;
                                            if (CheckGridAvailable(box_grid_world, kv, ref allowPutStaticLayout)) break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (allowPutStaticLayout) // 对于允许布局破损的情况，默认是允许放置的，但能够成功放置多少Entity就要依据实际情况了
                {
                    string staticLayoutGUID = Guid.NewGuid().ToString("P"); // e.g: (ade24d16-db0f-40af-8794-1e08e2040df3);
                    cached_IntactGPs_World.Clear();
                    for (int sl_local_x = 0; sl_local_x < WorldModule.MODULE_SIZE; sl_local_x++)
                    for (int sl_local_y = 0; sl_local_y < WorldModule.MODULE_SIZE; sl_local_y++)
                    for (int sl_local_z = 0; sl_local_z < WorldModule.MODULE_SIZE; sl_local_z++)
                    {
                        GridPos3D sl_local = new GridPos3D(sl_local_x, sl_local_y, sl_local_z);
                        GridPos3D rot_local = sl_local;
                        for (int rotCount = 0; rotCount < (int) staticLayoutOrientation; rotCount++) // 旋转
                        {
                            rot_local = new GridPos3D(rot_local.z, rot_local.y, WorldModule.MODULE_SIZE - 1 - rot_local.x);
                        }

                        foreach (KeyValuePair<TypeDefineType, int> kv in WorldModuleData.EntityDataMatrixKeys)
                        {
                            EntityData staticLayoutEntityData = staticLayoutData[kv.Key, sl_local]?.Clone();
                            if (staticLayoutEntityData != null)
                            {
                                staticLayoutEntityData.InitStaticLayoutGUID = staticLayoutGUID;
                                GridPos3D entity_world = worldGP + rot_local;
                                EntityOccupationData occupation = ConfigManager.GetEntityOccupationData(staticLayoutEntityData.EntityTypeIndex);
                                GridPosR.Orientation rot = GridPosR.RotateOrientationClockwise90(staticLayoutEntityData.EntityOrientation, (int) staticLayoutOrientation);
                                bool allowPutEntity = true;
                                foreach (GridPos3D gridPos in occupation.EntityIndicatorGPs_RotatedDict[rot])
                                {
                                    GridPos3D entity_grid_world = entity_world + gridPos;
                                    if (CheckGridAvailable(entity_grid_world, kv, ref allowPutEntity)) break;
                                }

                                if (allowPutEntity)
                                {
                                    foreach (GridPos3D gridPos in occupation.EntityIndicatorGPs_RotatedDict[rot])
                                    {
                                        GridPos3D entity_grid_world = entity_world + gridPos;
                                        m_OpenWorld.SetOccupied(staticLayoutEntityData.EntityType.TypeDefineType, staticLayoutEntityData.EntityTypeIndex, entity_grid_world, true);

                                        // 将Layout占领区域标记为Layout已占用，避免其他布局或随机Box插进来
                                        // 允许布局破损情况下，对布局内所有放置成功的Entity及其外扩一格的占领区域标记为Layout已占用
                                        if (staticLayoutLayerData.LayoutIntactForOtherStaticLayout || staticLayoutLayerData.LayoutIntactForOtherBoxes)
                                        {
                                            if (staticLayoutLayerData.AllowFragment)
                                            {
                                                for (int delta_x = -1; delta_x <= 1; delta_x++)
                                                for (int delta_y = -1; delta_y <= 1; delta_y++)
                                                for (int delta_z = -1; delta_z <= 1; delta_z++)
                                                {
                                                    GridPos3D entity_grid_around_world = entity_grid_world + new GridPos3D(delta_x, delta_y, delta_z);
                                                    if (CheckGridInsideWorldRange(entity_grid_around_world, 1))
                                                    {
                                                        cached_IntactGPs_World.Add(entity_grid_around_world);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    staticLayoutEntityData.EntityOrientation = rot;
                                    WorldMap_EntityDataMatrix[staticLayoutEntityData.EntityType.TypeDefineType][entity_world.x, entity_world.y - Height, entity_world.z] = staticLayoutEntityData;
                                }
                            }
                        }
                    }

                    List<EntityPassiveSkill_LevelEventTriggerAppear.Data> etaList = staticLayoutData.EventTriggerAppearEntityDataList.Clone<EntityPassiveSkill_LevelEventTriggerAppear.Data, LevelComponentData>();
                    foreach (EntityPassiveSkill_LevelEventTriggerAppear.Data data in etaList)
                    {
                        GridPos3D sl_local = data.LocalGP;
                        GridPos3D rot_local = sl_local;
                        for (int rotCount = 0; rotCount < (int) staticLayoutOrientation; rotCount++) // 旋转
                        {
                            rot_local = new GridPos3D(rot_local.z, rot_local.y, WorldModule.MODULE_SIZE - 1 - rot_local.x);
                        }

                        GridPos3D appear_world = worldGP + rot_local;
                        GridPos3D appear_local = new GridPos3D(appear_world.x % WorldModule.MODULE_SIZE, 0, appear_world.z % WorldModule.MODULE_SIZE);
                        data.WorldGP = appear_world;
                        data.LocalGP = appear_local;
                        data.EntityData.EntityOrientation = GridPosR.RotateOrientationClockwise90(data.EntityData.EntityOrientation, (int) staticLayoutOrientation);
                        data.EntityData.InitStaticLayoutGUID = staticLayoutGUID;
                        data.EntityPassiveSkill_LevelEventTriggerAppear.InitStaticLayoutGUID = staticLayoutGUID;
                        m_OpenWorld.EventTriggerAppearEntityDataList.Add(data);
                    }

                    // Trigger Entity Data
                    List<EntityData> ted = staticLayoutData.TriggerEntityDataList.Clone<EntityData, EntityData>();
                    foreach (EntityData triggerEntityData in ted)
                    {
                        triggerEntityData.InitStaticLayoutGUID = staticLayoutGUID;
                        GridPos3D sl_local = triggerEntityData.LocalGP;
                        GridPos3D rot_local = sl_local;
                        for (int rotCount = 0; rotCount < (int) staticLayoutOrientation; rotCount++) // 旋转
                        {
                            rot_local = new GridPos3D(rot_local.z, rot_local.y, WorldModule.MODULE_SIZE - 1 - rot_local.x);
                        }

                        GridPos3D appear_world = worldGP + rot_local;

                        // 破损布局情况下，为了避免不占位的TriggerEntity无限放置，只有附近的有占位的Entity被允许放置了，此TriggerEntity才允许被放置（是一种假想的占位）
                        if (staticLayoutLayerData.AllowFragment)
                        {
                            bool insideValidRange = false;
                            foreach (GridPos3D validWorldGP in cached_IntactGPs_World)
                            {
                                if (appear_world == validWorldGP)
                                {
                                    insideValidRange = true;
                                    break;
                                }
                            }

                            if (!insideValidRange) continue;
                        }

                        GridPos3D appear_local = new GridPos3D(appear_world.x % WorldModule.MODULE_SIZE, 0, appear_world.z % WorldModule.MODULE_SIZE);
                        triggerEntityData.WorldGP = appear_world;
                        triggerEntityData.LocalGP = appear_local;
                        triggerEntityData.EntityOrientation = GridPosR.RotateOrientationClockwise90(triggerEntityData.EntityOrientation, (int) staticLayoutOrientation);
                        m_OpenWorld.WorldMap_TriggerEntityDataMatrix[triggerEntityData.WorldGP.x, triggerEntityData.WorldGP.y - Height, triggerEntityData.WorldGP.z].Add(triggerEntityData);
                    }

                    if (staticLayoutLayerData.DeterminePlayerBP && m_OpenWorld.InitialPlayerBP == GridPos3D.Zero)
                    {
                        if (staticLayoutData.WorldModuleBornPointGroupData.PlayerBornPoints.Count > 0)
                        {
                            foreach (KeyValuePair<string, BornPointData> kv in staticLayoutData.WorldModuleBornPointGroupData.PlayerBornPoints)
                            {
                                GridPos3D sl_local = kv.Value.LocalGP;
                                GridPos3D rot_local = sl_local;
                                for (int rotCount = 0; rotCount < (int) staticLayoutOrientation; rotCount++) // 旋转
                                {
                                    rot_local = new GridPos3D(rot_local.z, rot_local.y, WorldModule.MODULE_SIZE - 1 - rot_local.x);
                                }

                                GridPos3D entity_world = worldGP + rot_local;
                                GridPos3D entity_local = new GridPos3D(entity_world.x % WorldModule.MODULE_SIZE, 0, entity_world.z % WorldModule.MODULE_SIZE);
                                BornPointData bp = new BornPointData {BornPointAlias = OpenWorld.PLAYER_DEFAULT_BP, LocalGP = entity_local, WorldGP = entity_world};
                                bp.InitGUID();
                                m_OpenWorld.WorldData.WorldBornPointGroupData_Runtime.SetDefaultPlayerBP_OpenWorld(bp);
                                m_OpenWorld.InitialPlayerBP = entity_world;
                                //WorldMapEntityData[entity_world.x, entity_world.y - WorldModule.MODULE_SIZE, entity_world.z] = (ushort) ConfigManager.TypeStartIndex.Player; todo
                                break;
                            }
                        }
                    }

                    // 将Layout占领区域标记为Layout已占用，避免其他布局或随机Box插进来
                    if (staticLayoutLayerData.LayoutIntactForOtherStaticLayout || staticLayoutLayerData.LayoutIntactForOtherBoxes)
                    {
                        if (!staticLayoutLayerData.AllowFragment) // 不允许布局破损情况下，需对整个Layout大致占领区域都标记为已占用
                        {
                            for (int sl_local_x = staticLayoutData.BoxBounds.x_min; sl_local_x <= staticLayoutData.BoxBounds.x_max; sl_local_x++)
                            for (int sl_local_y = staticLayoutData.BoxBounds.y_min; sl_local_y <= staticLayoutData.BoxBounds.y_max; sl_local_y++)
                            for (int sl_local_z = staticLayoutData.BoxBounds.z_min; sl_local_z <= staticLayoutData.BoxBounds.z_max; sl_local_z++)
                            {
                                GridPos3D sl_local = new GridPos3D(sl_local_x, sl_local_y, sl_local_z);
                                GridPos3D rot_local = sl_local;
                                for (int rotCount = 0; rotCount < (int) staticLayoutOrientation; rotCount++) // 旋转
                                {
                                    rot_local = new GridPos3D(rot_local.z, rot_local.y, WorldModule.MODULE_SIZE - 1 - rot_local.x);
                                }

                                GridPos3D layoutOccupied_world = worldGP + rot_local;
                                if (CheckGridInsideWorldRange(layoutOccupied_world, 1))
                                {
                                    if (staticLayoutLayerData.LayoutIntactForOtherStaticLayout)
                                    {
                                        WorldMap_StaticLayoutOccupied_IntactForStaticLayout[layoutOccupied_world.x, layoutOccupied_world.y - Height, layoutOccupied_world.z] = TypeIndex;
                                    }

                                    if (staticLayoutLayerData.LayoutIntactForOtherBoxes)
                                    {
                                        WorldMap_StaticLayoutOccupied_IntactForBox[layoutOccupied_world.x, layoutOccupied_world.y - Height, layoutOccupied_world.z] = TypeIndex;
                                    }
                                }
                            }
                        }
                        else // 允许布局破损情况下，对布局内所有放置成功的Entity及其外扩一格的占领区域标记为Layout已占用
                        {
                            foreach (GridPos3D intactGP_world in cached_IntactGPs_World)
                            {
                                if (CheckGridInsideWorldRange(intactGP_world, 1))
                                {
                                    if (staticLayoutLayerData.LayoutIntactForOtherStaticLayout)
                                    {
                                        WorldMap_StaticLayoutOccupied_IntactForStaticLayout[intactGP_world.x, intactGP_world.y - Height, intactGP_world.z] = TypeIndex;
                                    }

                                    if (staticLayoutLayerData.LayoutIntactForOtherBoxes)
                                    {
                                        WorldMap_StaticLayoutOccupied_IntactForBox[intactGP_world.x, intactGP_world.y - Height, intactGP_world.z] = TypeIndex;
                                    }
                                }
                            }
                        }
                    }

                    overrideSuc = true;
                }

                break;
            }

            case TypeDefineType.Box:
            case TypeDefineType.Actor:
            {
                if (existedIndex_StaticLayoutIndex_IntactForBox != 0) return false; // 不能覆盖在静态布局上
                bool spaceAvailable = true;
                EntityOccupationData occupation = ConfigManager.GetEntityOccupationData(TypeIndex);
                foreach (GridPos3D gridPos in occupation.EntityIndicatorGPs_RotatedDict[writeOrientation])
                {
                    if (!spaceAvailable) break;
                    GridPos3D box_grid_world = worldGP + gridPos;
                    if (CheckGridInsideWorldRange(box_grid_world, 0))
                    {
                        if (WorldMap_StaticLayoutOccupied_IntactForBox[box_grid_world.x, box_grid_world.y - Height, box_grid_world.z] != 0)
                        {
                            spaceAvailable = false;
                            break;
                        }

                        ushort occupiedBoxTypeIndex = m_OpenWorld.GetOccupy(TypeDefineType.Box, box_grid_world);
                        ushort occupiedEnemyTypeIndex = m_OpenWorld.GetOccupy(TypeDefineType.Actor, box_grid_world);
                        TerrainType terrainType = WorldMap_TerrainType[box_grid_world.x, box_grid_world.z];

                        if (occupiedBoxTypeIndex != 0 && !GenerateLayerData.AllowReplacedBoxTypeNameSet.Contains(ConfigManager.GetTypeName(TypeDefineType.Box, occupiedBoxTypeIndex)))
                        {
                            spaceAvailable = false;
                            break;
                        }

                        if (occupiedBoxTypeIndex == 0 && GenerateLayerData.OnlyOverrideAnyBox)
                        {
                            spaceAvailable = false;
                            break;
                        }

                        if (occupiedEnemyTypeIndex == ConfigManager.Actor_PlayerIndex)
                        {
                            spaceAvailable = false;
                            break;
                        }

                        if (GenerateLayerData.OnlyAllowPutOnTerrain && !GenerateLayerData.AllowPlaceOnTerrainTypeSet.Contains(terrainType))
                        {
                            spaceAvailable = false;
                        }

                        if (!GenerateLayerData.OnlyAllowPutOnTerrain && GenerateLayerData.ForbidPlaceOnTerrainTypeSet.Contains(terrainType))
                        {
                            spaceAvailable = false;
                        }
                    }
                    else
                    {
                        spaceAvailable = false;
                        break;
                    }
                }

                if (spaceAvailable)
                {
                    if (!occupation.IsTriggerEntity)
                    {
                        EntityData newEntityData = new EntityData(writeEntityTypeIndex, writeOrientation);
                        WorldMap_EntityDataMatrix[newEntityData.EntityType.TypeDefineType][worldGP.x, 0, worldGP.z] = newEntityData;
                        foreach (GridPos3D gridPos in occupation.EntityIndicatorGPs_RotatedDict[writeOrientation])
                        {
                            GridPos3D box_grid_world = worldGP + gridPos;
                            m_OpenWorld.SetOccupied(newEntityData.EntityType.TypeDefineType, writeEntityTypeIndex, box_grid_world, true);
                        }

                        overrideSuc = true;
                    }
                    else
                    {
                        EntityData triggerEntityData = new EntityData();
                        triggerEntityData.WorldGP = worldGP;
                        triggerEntityData.LocalGP = m_OpenWorld.GetLocalGPByWorldGP(worldGP);
                        triggerEntityData.EntityOrientation = writeOrientation;
                        m_OpenWorld.WorldMap_TriggerEntityDataMatrix[worldGP.x, worldGP.y - Height, worldGP.z].Add(triggerEntityData);
                    }
                }

                break;
            }
        }

        cached_IntactGPs_World.Clear();
        return overrideSuc;
    }
}
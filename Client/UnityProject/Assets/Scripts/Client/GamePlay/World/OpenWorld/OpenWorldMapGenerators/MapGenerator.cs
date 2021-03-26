﻿using System.Collections.Generic;
using BiangLibrary.GameDataFormat;
using BiangLibrary.GameDataFormat.Grid;
using FlowCanvas.Nodes;
using UnityEngine;

public abstract class MapGenerator
{
    protected uint Seed;
    protected SRandom SRandom;
    protected GenerateLayerData GenerateLayerData;

    protected MapGeneratorType MapGeneratorType;

    protected ushort EntityTypeIndex;
    protected ushort StaticLayoutTypeIndex;

    protected OpenWorld m_OpenWorld;
    protected ushort[,,] WorldMap => m_OpenWorld.WorldMap;
    protected EntityExtraSerializeData[,,] WorldMap_EntityExtraSerializeData => m_OpenWorld.WorldMap_EntityExtraSerializeData; // 仅针对静态布局生效
    protected GridPosR.Orientation[,,] WorldMapOrientation => m_OpenWorld.WorldMapOrientation;
    protected ushort[,,] WorldMap_Occupied => m_OpenWorld.WorldMap_Occupied;
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
        switch (layerData)
        {
            case GenerateStaticLayoutLayerData staticLayoutLayerData:
            {
                MapGeneratorType = MapGeneratorType.StaticLayout;
                StaticLayoutTypeIndex = ConfigManager.GetTypeIndex(TypeDefineType.StaticLayout, staticLayoutLayerData.StaticLayoutTypeName.TypeName);
                break;
            }
            case GenerateBoxLayerData boxLayerData:
            {
                MapGeneratorType = MapGeneratorType.Entity;
                EntityTypeIndex = ConfigManager.GetTypeIndex(TypeDefineType.Box, boxLayerData.BoxTypeName.TypeName);
                break;
            }
            case GenerateActorLayerData actorLayerData:
            {
                MapGeneratorType = MapGeneratorType.Entity;
                EntityTypeIndex = ConfigManager.GetTypeIndex(TypeDefineType.Enemy, actorLayerData.ActorTypeName.TypeName);
                break;
            }
        }

        Width = width;
        Depth = depth;
    }

    public abstract void ApplyToWorldMap();

    protected bool TryOverrideToWorldMap(int world_x, int world_y, int world_z)
    {
        bool overrideSuc = false;
        ushort existedIndex_StaticLayoutIndex_IntactForBox = WorldMap_StaticLayoutOccupied_IntactForBox[world_x, world_y - Height, world_z];
        switch (MapGeneratorType)
        {
            case MapGeneratorType.StaticLayout:
            {
                GenerateStaticLayoutLayerData staticLayoutLayerData = (GenerateStaticLayoutLayerData) GenerateLayerData;
                ushort staticLayoutTypeIndex = ConfigManager.GetTypeIndex(TypeDefineType.StaticLayout, staticLayoutLayerData.StaticLayoutTypeName.TypeName);
                WorldModuleData staticLayoutData = ConfigManager.GetStaticLayoutDataConfig(staticLayoutTypeIndex, false); // 不拷贝，只读数据，避免运行时动态加载GC
                GridPosR.Orientation staticLayoutOrientation = (GridPosR.Orientation) SRandom.Next(4);

                // world: 静态布局Pivot点的世界坐标
                // sl_local: 静态布局Config中未旋转的坐标
                // rot_local: 静态布局旋转后对应的坐标
                // box_world: 静态布局旋转后对应的世界坐标
                // box_grid_world: 旋转后的静态布局中的箱子的每一格对应的世界坐标

                // 检查范围内是否允许放置静态布局
                bool allowPut = true;
                if (!staticLayoutLayerData.AllowFragment)
                {
                    for (int sl_local_x = 0; sl_local_x < WorldModule.MODULE_SIZE; sl_local_x++)
                    {
                        if (!allowPut) break;
                        for (int sl_local_y = 0; sl_local_y < WorldModule.MODULE_SIZE; sl_local_y++)
                        {
                            if (!allowPut) break;
                            for (int sl_local_z = 0; sl_local_z < WorldModule.MODULE_SIZE; sl_local_z++)
                            {
                                if (!allowPut) break;
                                int rot_local_x = sl_local_x;
                                int rot_local_y = sl_local_y;
                                int rot_local_z = sl_local_z;
                                for (int rotCount = 0; rotCount < (int) staticLayoutOrientation; rotCount++)
                                {
                                    int temp_x = rot_local_x;
                                    rot_local_x = rot_local_z;
                                    rot_local_z = WorldModule.MODULE_SIZE - 1 - temp_x;
                                }

                                ushort boxTypeIndex = staticLayoutData.RawBoxMatrix[sl_local_x, sl_local_y, sl_local_z];
                                GridPosR.Orientation rot = staticLayoutData.RawBoxOrientationMatrix[sl_local_x, sl_local_y, sl_local_z];
                                if (boxTypeIndex != 0)
                                {
                                    EntityOccupationData occupation = ConfigManager.GetEntityOccupationData(boxTypeIndex);
                                    rot = GridPosR.RotateOrientationClockwise90(rot, (int) staticLayoutOrientation);
                                    foreach (GridPos3D gridPos in occupation.EntityIndicatorGPs_RotatedDict[rot])
                                    {
                                        int box_grid_world_x = world_x + rot_local_x + gridPos.x;
                                        int box_grid_world_y = world_y + rot_local_y + gridPos.y;
                                        int box_grid_world_z = world_z + rot_local_z + gridPos.z;
                                        if (box_grid_world_x > 0 && box_grid_world_x < Width - 1 && box_grid_world_y - Height >= 0 && box_grid_world_y - Height < Height && box_grid_world_z > 0 && box_grid_world_z < Depth - 1) // 静态布局不要贴边
                                        {
                                            if (WorldMap_Occupied[box_grid_world_x, box_grid_world_y - Height, box_grid_world_z] != 0)
                                            {
                                                allowPut = false;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            allowPut = false;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (allowPut) // 对于允许布局破损的情况，默认是允许放置的，但能够成功放置多少Entity就要依据实际情况了
                {
                    for (int sl_local_x = 0; sl_local_x < WorldModule.MODULE_SIZE; sl_local_x++)
                    for (int sl_local_y = 0; sl_local_y < WorldModule.MODULE_SIZE; sl_local_y++)
                    for (int sl_local_z = 0; sl_local_z < WorldModule.MODULE_SIZE; sl_local_z++)
                    {
                        int rot_local_x = sl_local_x;
                        int rot_local_y = sl_local_y;
                        int rot_local_z = sl_local_z;
                        for (int rotCount = 0; rotCount < (int) staticLayoutOrientation; rotCount++)
                        {
                            int temp_x = rot_local_x;
                            rot_local_x = rot_local_z;
                            rot_local_z = WorldModule.MODULE_SIZE - 1 - temp_x;
                        }

                        ushort boxTypeIndex = staticLayoutData.RawBoxMatrix[sl_local_x, sl_local_y, sl_local_z];
                        GridPosR.Orientation rot = staticLayoutData.RawBoxOrientationMatrix[sl_local_x, sl_local_y, sl_local_z];
                        if (boxTypeIndex != 0)
                        {
                            int box_world_x = world_x + rot_local_x;
                            int box_world_y = world_y + rot_local_y;
                            int box_world_z = world_z + rot_local_z;
                            EntityOccupationData occupation = ConfigManager.GetEntityOccupationData(boxTypeIndex);
                            rot = GridPosR.RotateOrientationClockwise90(rot, (int) staticLayoutOrientation);
                            bool spaceAvailableForBox = true;
                            foreach (GridPos3D gridPos in occupation.EntityIndicatorGPs_RotatedDict[rot])
                            {
                                int box_grid_world_x = box_world_x + gridPos.x;
                                int box_grid_world_y = box_world_y + gridPos.y;
                                int box_grid_world_z = box_world_z + gridPos.z;
                                if (box_grid_world_x > 0 && box_grid_world_x < Width - 1 && box_grid_world_y - Height >= 0 && box_grid_world_y - Height < Height && box_grid_world_z > 0 && box_grid_world_z < Depth - 1) // 静态布局不要贴边
                                {
                                    if (WorldMap_StaticLayoutOccupied_IntactForStaticLayout[box_grid_world_x, box_grid_world_y - Height, box_grid_world_z] != 0)
                                    {
                                        spaceAvailableForBox = false;
                                        break;
                                    }

                                    if (WorldMap_Occupied[box_grid_world_x, box_grid_world_y - Height, box_grid_world_z] != 0)
                                    {
                                        spaceAvailableForBox = false;
                                        break;
                                    }

                                    TerrainType terrainType = WorldMap_TerrainType[box_grid_world_x, box_grid_world_z];
                                    if (GenerateLayerData.OnlyAllowPutOnTerrain && !GenerateLayerData.AllowPlaceOnTerrainTypeSet.Contains(terrainType))
                                    {
                                        spaceAvailableForBox = false;
                                        break;
                                    }

                                    if (!GenerateLayerData.OnlyAllowPutOnTerrain && GenerateLayerData.ForbidPlaceOnTerrainTypeSet.Contains(terrainType))
                                    {
                                        spaceAvailableForBox = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    spaceAvailableForBox = false;
                                    break;
                                }
                            }

                            if (spaceAvailableForBox)
                            {
                                foreach (GridPos3D gridPos in occupation.EntityIndicatorGPs_RotatedDict[rot])
                                {
                                    int box_grid_world_x = box_world_x + gridPos.x;
                                    int box_grid_world_y = box_world_y + gridPos.y;
                                    int box_grid_world_z = box_world_z + gridPos.z;
                                    WorldMap_Occupied[box_grid_world_x, box_grid_world_y - Height, box_grid_world_z] = boxTypeIndex;
                                }

                                WorldMap[box_world_x, box_world_y - Height, box_world_z] = boxTypeIndex;
                                WorldMap_EntityExtraSerializeData[box_world_x, box_world_y - Height, box_world_z] = staticLayoutData.BoxExtraSerializeDataMatrix[sl_local_x, sl_local_y, sl_local_z]?.Clone();
                                WorldMapOrientation[box_world_x, box_world_y - Height, box_world_z] = rot;
                            }
                        }
                    }

                    foreach (BornPointData enemyBornPoint in staticLayoutData.WorldModuleBornPointGroupData.EnemyBornPoints)
                    {
                        ushort enemyTypeIndex = ConfigManager.GetTypeIndex(TypeDefineType.Enemy, enemyBornPoint.EnemyType.TypeName);
                        GridPos3D enemyGP_rotated = enemyBornPoint.LocalGP;
                        int sl_local_x = enemyBornPoint.LocalGP.x;
                        int sl_local_y = enemyBornPoint.LocalGP.y;
                        int sl_local_z = enemyBornPoint.LocalGP.z;
                        int rot_local_x = enemyBornPoint.LocalGP.x;
                        int rot_local_y = enemyBornPoint.LocalGP.y;
                        int rot_local_z = enemyBornPoint.LocalGP.z;
                        for (int rotCount = 0; rotCount < (int) staticLayoutOrientation; rotCount++)
                        {
                            int temp_x = rot_local_x;
                            rot_local_x = rot_local_z;
                            rot_local_z = WorldModule.MODULE_SIZE - 1 - temp_x;
                        }

                        if (enemyTypeIndex != 0)
                        {
                            int actor_world_x = world_x + rot_local_x;
                            int actor_world_y = world_y + rot_local_y;
                            int actor_world_z = world_z + rot_local_z;

                            EntityOccupationData occupationData = ConfigManager.GetEntityOccupationData(enemyTypeIndex);
                            GridPos actorRotOffset = Actor.ActorRotateWorldGPOffset(occupationData.ActorWidth, staticLayoutOrientation); // 由于Actor放置和旋转的特殊性，此处需要加一个偏移值
                            actor_world_x -= actorRotOffset.x;
                            actor_world_z -= actorRotOffset.z;

                            bool spaceAvailableForActor = true;
                            foreach (GridPos3D gridPos in occupationData.EntityIndicatorGPs_RotatedDict[staticLayoutOrientation])
                            {
                                int actor_grid_world_x = actor_world_x + gridPos.x + actorRotOffset.x; // 由于Actor放置和旋转的特殊性，此处需要加一个偏移值
                                int actor_grid_world_y = actor_world_y + gridPos.y;
                                int actor_grid_world_z = actor_world_z + gridPos.z + actorRotOffset.z; // 由于Actor放置和旋转的特殊性，此处需要加一个偏移值
                                if (actor_grid_world_x > 0 && actor_grid_world_x < Width - 1 && actor_grid_world_y - Height >= 0 && actor_grid_world_y - Height < Height && actor_grid_world_z > 0 && actor_grid_world_z < Depth - 1) // 静态布局不要贴边
                                {
                                    if (WorldMap_StaticLayoutOccupied_IntactForStaticLayout[actor_grid_world_x, actor_grid_world_y - Height, actor_grid_world_z] != 0)
                                    {
                                        spaceAvailableForActor = false;
                                        break;
                                    }

                                    if (WorldMap_Occupied[actor_grid_world_x, actor_grid_world_y - Height, actor_grid_world_z] != 0)
                                    {
                                        spaceAvailableForActor = false;
                                        break;
                                    }

                                    TerrainType terrainType = WorldMap_TerrainType[actor_grid_world_x, actor_grid_world_z];
                                    if (GenerateLayerData.OnlyAllowPutOnTerrain && !GenerateLayerData.AllowPlaceOnTerrainTypeSet.Contains(terrainType))
                                    {
                                        spaceAvailableForActor = false;
                                        break;
                                    }

                                    if (!GenerateLayerData.OnlyAllowPutOnTerrain && GenerateLayerData.ForbidPlaceOnTerrainTypeSet.Contains(terrainType))
                                    {
                                        spaceAvailableForActor = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    spaceAvailableForActor = false;
                                    break;
                                }
                            }

                            if (spaceAvailableForActor)
                            {
                                foreach (GridPos3D gridPos in occupationData.EntityIndicatorGPs_RotatedDict[staticLayoutOrientation])
                                {
                                    int actor_grid_world_x = actor_world_x + gridPos.x + actorRotOffset.x; // 由于Actor放置和旋转的特殊性，此处需要加一个偏移值
                                    int actor_grid_world_y = actor_world_y + gridPos.y;
                                    int actor_grid_world_z = actor_world_z + gridPos.z + actorRotOffset.z; // 由于Actor放置和旋转的特殊性，此处需要加一个偏移值
                                    WorldMap_Occupied[actor_grid_world_x, actor_grid_world_y - Height, actor_grid_world_z] = enemyTypeIndex;
                                }

                                WorldMap[actor_world_x, actor_world_y - Height, actor_world_z] = enemyTypeIndex;
                                WorldMap_EntityExtraSerializeData[actor_world_x, actor_world_y - Height, actor_world_z] = enemyBornPoint.RawEntityExtraSerializeData?.Clone();
                            }
                        }
                    }

                    if (GenerateLayerData.DeterminePlayerBP)
                    {
                        if (m_OpenWorld.InitialPlayerBP == GridPos3D.Zero)
                        {
                            if (staticLayoutData.WorldModuleBornPointGroupData.PlayerBornPoints.Count > 0)
                            {
                                foreach (KeyValuePair<string, BornPointData> kv in staticLayoutData.WorldModuleBornPointGroupData.PlayerBornPoints)
                                {
                                    BornPointData playerBPD = kv.Value;
                                    int rot_local_x = playerBPD.LocalGP.x;
                                    int rot_local_z = playerBPD.LocalGP.z;
                                    for (int rotCount = 0; rotCount < (int) staticLayoutOrientation; rotCount++)
                                    {
                                        int temp_x = rot_local_x;
                                        rot_local_x = rot_local_z;
                                        rot_local_z = WorldModule.MODULE_SIZE - 1 - temp_x;
                                    }

                                    GridPos3D playerBPWorldGP = new GridPos3D(world_x + rot_local_x, WorldModule.MODULE_SIZE, world_z + rot_local_z);
                                    GridPos3D playerBPLocal = new GridPos3D(playerBPWorldGP.x % WorldModule.MODULE_SIZE, 0, playerBPWorldGP.z % WorldModule.MODULE_SIZE);
                                    BornPointData bp = new BornPointData {IsPlayer = true, EnemyType = new TypeSelectHelper {TypeDefineType = TypeDefineType.Enemy}, BornPointAlias = OpenWorld.PLAYER_DEFAULT_BP, LocalGP = playerBPLocal, SpawnLevelEventAlias = "", WorldGP = playerBPWorldGP};
                                    bp.InitGUID();
                                    m_OpenWorld.WorldData.WorldBornPointGroupData_Runtime.SetDefaultPlayerBP_OpenWorld(bp);
                                    m_OpenWorld.InitialPlayerBP = playerBPWorldGP;
                                    WorldMap[playerBPWorldGP.x, playerBPWorldGP.y - WorldModule.MODULE_SIZE, playerBPWorldGP.z] = (ushort) ConfigManager.TypeStartIndex.Player;
                                    break;
                                }
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
                                int rot_local_x = sl_local_x;
                                int rot_local_z = sl_local_z;
                                for (int rotCount = 0; rotCount < (int) staticLayoutOrientation; rotCount++)
                                {
                                    int temp_x = rot_local_x;
                                    rot_local_x = rot_local_z;
                                    rot_local_z = WorldModule.MODULE_SIZE - 1 - temp_x;
                                }

                                int layoutOccupied_world_x = world_x + rot_local_x;
                                int layoutOccupied_world_y = world_y + sl_local_y;
                                int layoutOccupied_world_z = world_z + rot_local_z;
                                if (layoutOccupied_world_x >= 0 && layoutOccupied_world_x < Width && layoutOccupied_world_y - Height >= 0 && layoutOccupied_world_y - Height < Height && layoutOccupied_world_z >= 0 && layoutOccupied_world_z < Depth)
                                {
                                    if (staticLayoutLayerData.LayoutIntactForOtherStaticLayout)
                                    {
                                        WorldMap_StaticLayoutOccupied_IntactForStaticLayout[layoutOccupied_world_x, layoutOccupied_world_y - Height, layoutOccupied_world_z] = staticLayoutTypeIndex;
                                    }

                                    if (staticLayoutLayerData.LayoutIntactForOtherBoxes)
                                    {
                                        WorldMap_StaticLayoutOccupied_IntactForBox[layoutOccupied_world_x, layoutOccupied_world_y - Height, layoutOccupied_world_z] = staticLayoutTypeIndex;
                                    }
                                }
                            }
                        }
                        else // 允许布局破损情况下，对布局内所有放置成功的Entity及其外扩一格的占领区域标记为Layout已占用
                        {
                        }
                    }

                    overrideSuc = true;
                }

                break;
            }

            case MapGeneratorType.Entity:
            {
                if (existedIndex_StaticLayoutIndex_IntactForBox != 0) return false; // 不能覆盖在静态布局上
                bool spaceAvailable = true;
                GridPosR.Orientation entityOrientation = (GridPosR.Orientation) SRandom.Range(0, 4);
                EntityOccupationData occupation = ConfigManager.GetEntityOccupationData(EntityTypeIndex);
                foreach (GridPos3D gridPos in occupation.EntityIndicatorGPs_RotatedDict[entityOrientation]) // todo Orientation Random
                {
                    if (!spaceAvailable) break;
                    int box_grid_world_x = world_x + gridPos.x;
                    int box_grid_world_y = world_y + gridPos.y;
                    int box_grid_world_z = world_z + gridPos.z;
                    if (box_grid_world_x >= 0 && box_grid_world_x < Width && box_grid_world_y - Height >= 0 && box_grid_world_y - Height < Height && box_grid_world_z >= 0 && box_grid_world_z < Depth)
                    {
                        if (WorldMap_StaticLayoutOccupied_IntactForBox[box_grid_world_x, box_grid_world_y - Height, box_grid_world_z] != 0)
                        {
                            spaceAvailable = false;
                            break;
                        }

                        ushort occupiedIndex = WorldMap_Occupied[box_grid_world_x, box_grid_world_y - Height, box_grid_world_z];
                        TerrainType terrainType = WorldMap_TerrainType[box_grid_world_x, box_grid_world_z];
                        ConfigManager.TypeStartIndex occupiedIndexType = occupiedIndex.ConvertToTypeStartIndex();
                        switch (occupiedIndexType)
                        {
                            case ConfigManager.TypeStartIndex.Box when !GenerateLayerData.AllowReplacedBoxTypeNameSet.Contains(ConfigManager.GetTypeName(TypeDefineType.Box, occupiedIndex)):
                            case ConfigManager.TypeStartIndex.None when GenerateLayerData.OnlyOverrideAnyBox:
                            case ConfigManager.TypeStartIndex.Player:
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
                    WorldMap[world_x, 0, world_z] = EntityTypeIndex;
                    WorldMapOrientation[world_x, 0, world_z] = entityOrientation;
                    foreach (GridPos3D gridPos in occupation.EntityIndicatorGPs_RotatedDict[entityOrientation]) // todo Orientation Random
                    {
                        int box_grid_world_x = world_x + gridPos.x;
                        int box_grid_world_z = world_z + gridPos.z;
                        WorldMap_Occupied[box_grid_world_x, 0, box_grid_world_z] = EntityTypeIndex;
                    }

                    overrideSuc = true;
                }

                break;
            }
        }

        return overrideSuc;
    }
}

public enum MapGeneratorType
{
    StaticLayout,
    Entity,
}
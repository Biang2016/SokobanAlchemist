using System.Collections.Generic;
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
    protected BornPointData[,,] WorldBornPointData => m_OpenWorld.WorldBornPointData;
    protected EntityExtraSerializeData[,,] WorldMap_EntityExtraSerializeData => m_OpenWorld.WorldMap_BoxExtraSerializeData; // 仅针对静态布局生效
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

    private static List<GridPos3D> cached_IntactGPs_World = new List<GridPos3D>(1024); // 静态布局内所有的Y值为0的成功放置的箱子的LocalGP

    protected bool TryOverrideToWorldMap(GridPos3D worldGP, ushort writeEntityTypeIndex, GridPosR.Orientation writeOrientation)
    {
        bool CheckGridInsideWorldRange(GridPos3D _worldGP, int borderThickness)
        {
            return _worldGP.x >= borderThickness && _worldGP.x < Width - borderThickness && _worldGP.y - Height >= 0 && _worldGP.y - Height < Height && _worldGP.z >= borderThickness && _worldGP.z < Depth - borderThickness;
        }

        bool overrideSuc = false;
        ushort existedIndex_StaticLayoutIndex_IntactForBox = WorldMap_StaticLayoutOccupied_IntactForBox[worldGP.x, worldGP.y - Height, worldGP.z];
        switch (MapGeneratorType)
        {
            case MapGeneratorType.StaticLayout:
            {
                GenerateStaticLayoutLayerData staticLayoutLayerData = (GenerateStaticLayoutLayerData) GenerateLayerData;
                ushort staticLayoutTypeIndex = ConfigManager.GetTypeIndex(TypeDefineType.StaticLayout, staticLayoutLayerData.StaticLayoutTypeName.TypeName);
                WorldModuleData staticLayoutData = ConfigManager.GetStaticLayoutDataConfig(staticLayoutTypeIndex, false); // 不拷贝，只读数据，避免运行时动态加载GC
                GridPosR.Orientation staticLayoutOrientation = writeOrientation;

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
                                GridPos3D sl_local = new GridPos3D(sl_local_x, sl_local_y, sl_local_z);
                                GridPos3D rot_local = sl_local;
                                for (int rotCount = 0; rotCount < (int) staticLayoutOrientation; rotCount++)
                                {
                                    rot_local = new GridPos3D(rot_local.z, rot_local.y, WorldModule.MODULE_SIZE - 1 - rot_local.x);
                                }

                                ushort boxTypeIndex = staticLayoutData.GetRawBoxTypeIndex(sl_local);
                                GridPosR.Orientation rot = staticLayoutData.GetRawBoxOrientation(sl_local);
                                if (boxTypeIndex != 0)
                                {
                                    GridPos3D box_world = worldGP + rot_local;
                                    EntityOccupationData occupation = ConfigManager.GetEntityOccupationData(boxTypeIndex);
                                    rot = GridPosR.RotateOrientationClockwise90(rot, (int) staticLayoutOrientation);
                                    foreach (GridPos3D gridPos in occupation.EntityIndicatorGPs_RotatedDict[rot])
                                    {
                                        GridPos3D box_grid_world = box_world + gridPos;
                                        if (CheckGridInsideWorldRange(box_grid_world, 1))
                                        {
                                            if (WorldMap_Occupied[box_grid_world.x, box_grid_world.y - Height, box_grid_world.z] != 0)
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
                    cached_IntactGPs_World.Clear();
                    for (int sl_local_x = 0; sl_local_x < WorldModule.MODULE_SIZE; sl_local_x++)
                    for (int sl_local_y = 0; sl_local_y < WorldModule.MODULE_SIZE; sl_local_y++)
                    for (int sl_local_z = 0; sl_local_z < WorldModule.MODULE_SIZE; sl_local_z++)
                    {
                        GridPos3D sl_local = new GridPos3D(sl_local_x, sl_local_y, sl_local_z);
                        GridPos3D rot_local = sl_local;
                        for (int rotCount = 0; rotCount < (int) staticLayoutOrientation; rotCount++)
                        {
                            rot_local = new GridPos3D(rot_local.z, rot_local.y, WorldModule.MODULE_SIZE - 1 - rot_local.x);
                        }

                        ushort entityTypeIndex = staticLayoutData.GetRawBoxTypeIndex(sl_local);
                        GridPosR.Orientation rot = staticLayoutData.GetRawBoxOrientation(sl_local);
                        if (entityTypeIndex != 0)
                        {
                            GridPos3D entity_world = worldGP + rot_local;
                            EntityOccupationData occupation = ConfigManager.GetEntityOccupationData(entityTypeIndex);
                            rot = GridPosR.RotateOrientationClockwise90(rot, (int) staticLayoutOrientation);
                            bool spaceAvailableForEntity = true;
                            foreach (GridPos3D gridPos in occupation.EntityIndicatorGPs_RotatedDict[rot])
                            {
                                GridPos3D entity_grid_world = entity_world + gridPos;
                                if (CheckGridInsideWorldRange(entity_grid_world, 1))
                                {
                                    if (WorldMap_StaticLayoutOccupied_IntactForStaticLayout[entity_grid_world.x, entity_grid_world.y - Height, entity_grid_world.z] != 0)
                                    {
                                        spaceAvailableForEntity = false;
                                        break;
                                    }

                                    if (WorldMap_Occupied[entity_grid_world.x, entity_grid_world.y - Height, entity_grid_world.z] != 0)
                                    {
                                        spaceAvailableForEntity = false;
                                        break;
                                    }

                                    TerrainType terrainType = WorldMap_TerrainType[entity_grid_world.x, entity_grid_world.z];
                                    if (GenerateLayerData.OnlyAllowPutOnTerrain && !GenerateLayerData.AllowPlaceOnTerrainTypeSet.Contains(terrainType))
                                    {
                                        spaceAvailableForEntity = false;
                                        break;
                                    }

                                    if (!GenerateLayerData.OnlyAllowPutOnTerrain && GenerateLayerData.ForbidPlaceOnTerrainTypeSet.Contains(terrainType))
                                    {
                                        spaceAvailableForEntity = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    spaceAvailableForEntity = false;
                                    break;
                                }
                            }

                            if (spaceAvailableForEntity)
                            {
                                foreach (GridPos3D gridPos in occupation.EntityIndicatorGPs_RotatedDict[rot])
                                {
                                    GridPos3D entity_grid_world = entity_world + gridPos;
                                    WorldMap_Occupied[entity_grid_world.x, entity_grid_world.y - Height, entity_grid_world.z] = entityTypeIndex;

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

                                WorldMap[entity_world.x, entity_world.y - Height, entity_world.z] = entityTypeIndex;
                                WorldMap_EntityExtraSerializeData[entity_world.x, entity_world.y - Height, entity_world.z] = staticLayoutData.BoxExtraSerializeDataMatrix[sl_local_x, sl_local_y, sl_local_z]?.Clone();
                                WorldMapOrientation[entity_world.x, entity_world.y - Height, entity_world.z] = rot;
                            }
                        }
                    }

                    foreach (BornPointData enemyBornPoint in staticLayoutData.WorldModuleBornPointGroupData.EnemyBornPoints)
                    {
                        ushort entityTypeIndex = ConfigManager.GetTypeIndex(TypeDefineType.Enemy, enemyBornPoint.EnemyType.TypeName);
                        GridPos3D sl_local = enemyBornPoint.LocalGP;
                        GridPos3D rot_local = sl_local;
                        for (int rotCount = 0; rotCount < (int) staticLayoutOrientation; rotCount++)
                        {
                            rot_local = new GridPos3D(rot_local.z, rot_local.y, WorldModule.MODULE_SIZE - 1 - rot_local.x);
                        }

                        if (entityTypeIndex != 0)
                        {
                            GridPos3D entity_world = worldGP + rot_local;
                            EntityOccupationData occupationData = ConfigManager.GetEntityOccupationData(entityTypeIndex);

                            bool spaceAvailableForEntity = true;
                            foreach (GridPos3D gridPos in occupationData.EntityIndicatorGPs_RotatedDict[staticLayoutOrientation])
                            {
                                GridPos3D entity_grid_world = entity_world + gridPos;
                                if (CheckGridInsideWorldRange(entity_grid_world, 1))
                                {
                                    if (WorldMap_StaticLayoutOccupied_IntactForStaticLayout[entity_grid_world.x, entity_grid_world.y - Height, entity_grid_world.z] != 0)
                                    {
                                        spaceAvailableForEntity = false;
                                        break;
                                    }

                                    if (WorldMap_Occupied[entity_grid_world.x, entity_grid_world.y - Height, entity_grid_world.z] != 0)
                                    {
                                        spaceAvailableForEntity = false;
                                        break;
                                    }

                                    TerrainType terrainType = WorldMap_TerrainType[entity_grid_world.x, entity_grid_world.z];
                                    if (GenerateLayerData.OnlyAllowPutOnTerrain && !GenerateLayerData.AllowPlaceOnTerrainTypeSet.Contains(terrainType))
                                    {
                                        spaceAvailableForEntity = false;
                                        break;
                                    }

                                    if (!GenerateLayerData.OnlyAllowPutOnTerrain && GenerateLayerData.ForbidPlaceOnTerrainTypeSet.Contains(terrainType))
                                    {
                                        spaceAvailableForEntity = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    spaceAvailableForEntity = false;
                                    break;
                                }
                            }

                            if (spaceAvailableForEntity)
                            {
                                foreach (GridPos3D gridPos in occupationData.EntityIndicatorGPs_RotatedDict[staticLayoutOrientation])
                                {
                                    GridPos3D entity_grid_world = entity_world + gridPos;
                                    WorldMap_Occupied[entity_grid_world.x, entity_grid_world.y - Height, entity_grid_world.z] = entityTypeIndex;

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

                                WorldMap[entity_world.x, entity_world.y - Height, entity_world.z] = entityTypeIndex;
                                BornPointData bp = (BornPointData) enemyBornPoint.Clone();
                                bp.ActorOrientation = staticLayoutOrientation;
                                bp.InitGUID();
                                WorldBornPointData[entity_world.x, entity_world.y - Height, entity_world.z] = bp;
                            }
                        }
                    }

                    if (staticLayoutLayerData.DeterminePlayerBP && m_OpenWorld.InitialPlayerBP == GridPos3D.Zero)
                    {
                        if (staticLayoutData.WorldModuleBornPointGroupData.PlayerBornPoints.Count > 0)
                        {
                            foreach (KeyValuePair<string, BornPointData> kv in staticLayoutData.WorldModuleBornPointGroupData.PlayerBornPoints)
                            {
                                GridPos3D sl_local = kv.Value.LocalGP;
                                GridPos3D rot_local = sl_local;
                                for (int rotCount = 0; rotCount < (int) staticLayoutOrientation; rotCount++)
                                {
                                    rot_local = new GridPos3D(rot_local.z, rot_local.y, WorldModule.MODULE_SIZE - 1 - rot_local.x);
                                }

                                GridPos3D entity_world = worldGP + rot_local;
                                GridPos3D entity_local = new GridPos3D(entity_world.x % WorldModule.MODULE_SIZE, 0, entity_world.z % WorldModule.MODULE_SIZE);
                                BornPointData bp = new BornPointData {IsPlayer = true, EnemyType = new TypeSelectHelper {TypeDefineType = TypeDefineType.Enemy}, BornPointAlias = OpenWorld.PLAYER_DEFAULT_BP, LocalGP = entity_local, SpawnLevelTriggerEventAlias = "", WorldGP = entity_world};
                                bp.InitGUID();
                                m_OpenWorld.WorldData.WorldBornPointGroupData_Runtime.SetDefaultPlayerBP_OpenWorld(bp);
                                m_OpenWorld.InitialPlayerBP = entity_world;
                                WorldMap[entity_world.x, entity_world.y - WorldModule.MODULE_SIZE, entity_world.z] = (ushort) ConfigManager.TypeStartIndex.Player;
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
                                for (int rotCount = 0; rotCount < (int) staticLayoutOrientation; rotCount++)
                                {
                                    rot_local = new GridPos3D(rot_local.z, rot_local.y, WorldModule.MODULE_SIZE - 1 - rot_local.x);
                                }

                                GridPos3D layoutOccupied_world = worldGP + rot_local;
                                if (CheckGridInsideWorldRange(layoutOccupied_world, 1))
                                {
                                    if (staticLayoutLayerData.LayoutIntactForOtherStaticLayout)
                                    {
                                        WorldMap_StaticLayoutOccupied_IntactForStaticLayout[layoutOccupied_world.x, layoutOccupied_world.y - Height, layoutOccupied_world.z] = staticLayoutTypeIndex;
                                    }

                                    if (staticLayoutLayerData.LayoutIntactForOtherBoxes)
                                    {
                                        WorldMap_StaticLayoutOccupied_IntactForBox[layoutOccupied_world.x, layoutOccupied_world.y - Height, layoutOccupied_world.z] = staticLayoutTypeIndex;
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
                                        WorldMap_StaticLayoutOccupied_IntactForStaticLayout[intactGP_world.x, intactGP_world.y - Height, intactGP_world.z] = staticLayoutTypeIndex;
                                    }

                                    if (staticLayoutLayerData.LayoutIntactForOtherBoxes)
                                    {
                                        WorldMap_StaticLayoutOccupied_IntactForBox[intactGP_world.x, intactGP_world.y - Height, intactGP_world.z] = staticLayoutTypeIndex;
                                    }
                                }
                            }
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
                EntityOccupationData occupation = ConfigManager.GetEntityOccupationData(EntityTypeIndex);
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

                        ushort occupiedIndex = WorldMap_Occupied[box_grid_world.x, box_grid_world.y - Height, box_grid_world.z];
                        TerrainType terrainType = WorldMap_TerrainType[box_grid_world.x, box_grid_world.z];
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
                    ConfigManager.TypeStartIndex typeStartIndex = writeEntityTypeIndex.ConvertToTypeStartIndex();
                    if (typeStartIndex == ConfigManager.TypeStartIndex.Enemy)
                    {
                        string enemyTypeName = ConfigManager.GetTypeName(TypeDefineType.Enemy, writeEntityTypeIndex);
                        WorldBornPointData[worldGP.x, 0, worldGP.z] = new BornPointData
                        {
                            ActorOrientation = writeOrientation,
                            BornPointAlias = "",
                            EnemyType = new TypeSelectHelper {TypeDefineType = TypeDefineType.Enemy, TypeSelection = enemyTypeName},
                            IsPlayer = false,
                            LocalGP = new GridPos3D(worldGP.x % WorldModule.MODULE_SIZE, 0, worldGP.z % WorldModule.MODULE_SIZE),
                            WorldGP = worldGP,
                            RawEntityExtraSerializeData = null,
                            TriggerSpawnMultipleTimes = 0,
                            SpawnLevelTriggerEventAlias = "",
                        };
                    }

                    WorldMap[worldGP.x, 0, worldGP.z] = writeEntityTypeIndex;
                    WorldMapOrientation[worldGP.x, 0, worldGP.z] = writeOrientation;
                    foreach (GridPos3D gridPos in occupation.EntityIndicatorGPs_RotatedDict[writeOrientation])
                    {
                        GridPos3D box_grid_world = worldGP + gridPos;
                        WorldMap_Occupied[box_grid_world.x, box_grid_world.y - Height, box_grid_world.z] = writeEntityTypeIndex;
                    }

                    overrideSuc = true;
                }

                break;
            }
        }

        cached_IntactGPs_World.Clear();
        return overrideSuc;
    }
}

public enum MapGeneratorType
{
    StaticLayout,
    Entity,
}
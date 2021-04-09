using System.Collections.Generic;
using BiangLibrary.GameDataFormat;
using BiangLibrary.GameDataFormat.Grid;
using Debug = UnityEngine.Debug;

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
                                            if (CheckGridInsideWorldRange(box_grid_world, 1))
                                            {
                                                if (m_OpenWorld.CheckOccupied(kv.Key, box_grid_world))
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
                        for (int rotCount = 0; rotCount < (int) staticLayoutOrientation; rotCount++) // 旋转
                        {
                            rot_local = new GridPos3D(rot_local.z, rot_local.y, WorldModule.MODULE_SIZE - 1 - rot_local.x);
                        }

                        foreach (KeyValuePair<TypeDefineType, int> kv in WorldModuleData.EntityDataMatrixKeys)
                        {
                            EntityData staticLayoutEntityData = staticLayoutData[kv.Key, sl_local]?.Clone();
                            if (staticLayoutEntityData != null)
                            {
                                GridPos3D entity_world = worldGP + rot_local;
                                EntityOccupationData occupation = ConfigManager.GetEntityOccupationData(staticLayoutEntityData.EntityTypeIndex);
                                GridPosR.Orientation rot = GridPosR.RotateOrientationClockwise90(staticLayoutEntityData.EntityOrientation, (int) staticLayoutOrientation);
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

                                        if (m_OpenWorld.CheckOccupied(kv.Key, entity_grid_world))
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
                    EntityData newEntityData = new EntityData(writeEntityTypeIndex, writeOrientation);
                    WorldMap_EntityDataMatrix[newEntityData.EntityType.TypeDefineType][worldGP.x, 0, worldGP.z] = newEntityData;
                    foreach (GridPos3D gridPos in occupation.EntityIndicatorGPs_RotatedDict[writeOrientation])
                    {
                        GridPos3D box_grid_world = worldGP + gridPos;
                        m_OpenWorld.SetOccupied(newEntityData.EntityType.TypeDefineType, writeEntityTypeIndex, box_grid_world, true);
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
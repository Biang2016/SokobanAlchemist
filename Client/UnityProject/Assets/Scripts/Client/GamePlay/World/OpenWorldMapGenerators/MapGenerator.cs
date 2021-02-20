using BiangLibrary.GameDataFormat;
using BiangLibrary.GameDataFormat.Grid;

public abstract class MapGenerator
{
    protected uint Seed;
    protected SRandom SRandom;
    protected OpenWorld.GenerateLayerData GenerateLayerData;

    protected MapGeneratorType MapGeneratorType;

    protected ushort BoxTypeIndex;
    protected ushort EnemyTypeIndex;
    protected ushort StaticLayoutTypeIndex;

    protected OpenWorld m_OpenWorld;
    protected ushort[,] WorldMap => m_OpenWorld.WorldMap;
    protected ushort[,] WorldMap_Occupied => m_OpenWorld.WorldMap_Occupied;
    protected ushort[,] WorldMap_StaticLayoutOccupied => m_OpenWorld.WorldMap_StaticLayoutOccupied;

    protected int Width;
    protected int Height;

    protected MapGenerator(OpenWorld.GenerateLayerData layerData, int width, int height, uint seed, OpenWorld openWorld)
    {
        m_OpenWorld = openWorld;
        Seed = seed;
        SRandom = new SRandom(seed);
        GenerateLayerData = layerData;
        switch (layerData)
        {
            case OpenWorld.GenerateStaticLayoutLayerData staticLayoutLayerData:
            {
                MapGeneratorType = MapGeneratorType.StaticLayout;
                StaticLayoutTypeIndex = ConfigManager.GetStaticLayoutTypeIndex(staticLayoutLayerData.StaticLayoutTypeName);
                break;
            }
            case OpenWorld.GenerateBoxLayerData boxLayerData:
            {
                MapGeneratorType = MapGeneratorType.Box;
                BoxTypeIndex = ConfigManager.GetBoxTypeIndex(boxLayerData.BoxTypeName);
                break;
            }
            case OpenWorld.GenerateActorLayerData actorLayerData:
            {
                MapGeneratorType = MapGeneratorType.Actor;
                EnemyTypeIndex = ConfigManager.GetEnemyTypeIndex(actorLayerData.ActorTypeName);
                break;
            }
        }

        Width = width;
        Height = height;
    }

    public abstract void ApplyToWorldMap();

    protected void TryOverrideToWorldMap(int world_x, int world_z)
    {
        ushort existedIndex = WorldMap[world_x, world_z];
        ushort existedIndex_StaticLayoutIndex = WorldMap_StaticLayoutOccupied[world_x, world_z];
        if (existedIndex_StaticLayoutIndex != 0) return; // 不能覆盖在静态布局上
        ConfigManager.TypeStartIndex existedIndexType = existedIndex.ConvertToTypeStartIndex();
        switch (MapGeneratorType)
        {
            case MapGeneratorType.StaticLayout:
            {
                OpenWorld.GenerateStaticLayoutLayerData staticLayoutLayerData = (OpenWorld.GenerateStaticLayoutLayerData) GenerateLayerData;
                ushort staticLayoutTypeIndex = ConfigManager.GetStaticLayoutTypeIndex(staticLayoutLayerData.StaticLayoutTypeName);
                WorldModuleData staticLayoutData = ConfigManager.GetStaticLayoutDataConfig(staticLayoutTypeIndex, false); // 不拷贝，只读数据，避免运行时动态加载GC

                // Check Space
                bool spaceAvailable = true;
                int sl_local_y = 0;
                for (int sl_local_x = 0; sl_local_x < WorldModule.MODULE_SIZE; sl_local_x++)
                {
                    if (!spaceAvailable) break;
                    for (int sl_local_z = 0; sl_local_z < WorldModule.MODULE_SIZE; sl_local_z++)
                    {
                        if (!spaceAvailable) break;
                        ushort boxTypeIndex = staticLayoutData.RawBoxMatrix[sl_local_x, sl_local_y, sl_local_z];
                        if (boxTypeIndex != 0)
                        {
                            BoxOccupationData occupation = ConfigManager.GetBoxOccupationData(boxTypeIndex);
                            foreach (GridPos3D gridPos in occupation.BoxIndicatorGPs_RotatedDict[staticLayoutData.RawBoxOrientationMatrix[sl_local_x, sl_local_y, sl_local_z]])
                            {
                                int box_grid_world_x = world_x + sl_local_x + gridPos.x;
                                int box_grid_world_z = world_z + sl_local_z + gridPos.z;
                                if (box_grid_world_x >= 0 && box_grid_world_x < Width && box_grid_world_z >= 0 && box_grid_world_z < Height)
                                {
                                    if (WorldMap_Occupied[box_grid_world_x, box_grid_world_z] != 0)
                                    {
                                        spaceAvailable = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    spaceAvailable = false;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (spaceAvailable)
                {
                    for (int sl_local_x = 0; sl_local_x < WorldModule.MODULE_SIZE; sl_local_x++)
                    for (int sl_local_z = 0; sl_local_z < WorldModule.MODULE_SIZE; sl_local_z++)
                    {
                        ushort boxTypeIndex = staticLayoutData.RawBoxMatrix[sl_local_x, sl_local_y, sl_local_z];
                        if (boxTypeIndex != 0)
                        {
                            WorldMap[world_x + sl_local_x, world_z + sl_local_z] = boxTypeIndex;
                            BoxOccupationData occupation = ConfigManager.GetBoxOccupationData(boxTypeIndex);
                            foreach (GridPos3D gridPos in occupation.BoxIndicatorGPs_RotatedDict[staticLayoutData.RawBoxOrientationMatrix[sl_local_x, sl_local_y, sl_local_z]])
                            {
                                int box_grid_world_x = world_x + sl_local_x + gridPos.x;
                                int box_grid_world_z = world_z + sl_local_z + gridPos.z;
                                WorldMap_Occupied[box_grid_world_x, box_grid_world_z] = boxTypeIndex;
                            }
                        }
                    }

                    for (int sl_local_x = staticLayoutData.BoxBounds.x_min; sl_local_x <= staticLayoutData.BoxBounds.x_max; sl_local_x++)
                    for (int sl_local_z = staticLayoutData.BoxBounds.z_min; sl_local_z <= staticLayoutData.BoxBounds.z_max; sl_local_z++)
                    {
                        int layoutOccupied_world_x = world_x + sl_local_x;
                        int layoutOccupied_world_z = world_z + sl_local_z;
                        if (layoutOccupied_world_x >= 0 && layoutOccupied_world_x < Width && layoutOccupied_world_z >= 0 && layoutOccupied_world_z < Height)
                        {
                            WorldMap_StaticLayoutOccupied[layoutOccupied_world_x, layoutOccupied_world_z] = staticLayoutTypeIndex;
                        }
                    }
                }

                // todo Actors
                break;
            }

            case MapGeneratorType.Box:
            {
                bool spaceAvailable = true;
                BoxOccupationData occupation = ConfigManager.GetBoxOccupationData(BoxTypeIndex);
                foreach (GridPos3D gridPos in occupation.BoxIndicatorGPs_RotatedDict[GridPosR.Orientation.Up]) // todo Orientation Random
                {
                    if (!spaceAvailable) break;
                    int box_grid_world_x = world_x + gridPos.x;
                    int box_grid_world_z = world_z + gridPos.z;
                    if (box_grid_world_x >= 0 && box_grid_world_x < Width && box_grid_world_z >= 0 && box_grid_world_z < Height)
                    {
                        if (WorldMap_StaticLayoutOccupied[box_grid_world_x, box_grid_world_z] != 0)
                        {
                            spaceAvailable = false;
                            break;
                        }

                        ushort occupiedIndex = WorldMap_Occupied[box_grid_world_x, box_grid_world_z];
                        ConfigManager.TypeStartIndex occupiedIndexType = occupiedIndex.ConvertToTypeStartIndex();
                        switch (occupiedIndexType)
                        {
                            case ConfigManager.TypeStartIndex.Box when !GenerateLayerData.AllowReplacedBoxTypeNameSet.Contains(ConfigManager.GetBoxTypeName(occupiedIndex)):
                            case ConfigManager.TypeStartIndex.None when GenerateLayerData.OnlyOverrideAnyBox:
                            {
                                spaceAvailable = false;
                                break;
                            }
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
                    WorldMap[world_x, world_z] = BoxTypeIndex;
                    foreach (GridPos3D gridPos in occupation.BoxIndicatorGPs_RotatedDict[GridPosR.Orientation.Up]) // todo Orientation Random
                    {
                        int box_grid_world_x = world_x + gridPos.x;
                        int box_grid_world_z = world_z + gridPos.z;
                        WorldMap_Occupied[box_grid_world_x, box_grid_world_z] = BoxTypeIndex;
                    }
                }

                break;
            }
            case MapGeneratorType.Actor:
            {
                if (WorldMap_StaticLayoutOccupied[world_x, world_z] != 0) break;

                bool spaceAvailable = true;
                switch (existedIndexType)
                {
                    case ConfigManager.TypeStartIndex.Box when !GenerateLayerData.AllowReplacedBoxTypeNameSet.Contains(ConfigManager.GetBoxTypeName(existedIndex)):
                    case ConfigManager.TypeStartIndex.None when GenerateLayerData.OnlyOverrideAnyBox:
                        spaceAvailable = false;
                        break;
                }

                if (spaceAvailable)
                {
                    WorldMap[world_x, world_z] = EnemyTypeIndex;
                    WorldMap_Occupied[world_x, world_z] = EnemyTypeIndex;
                }

                break;
            }
        }
    }
}

public enum MapGeneratorType
{
    StaticLayout,
    Box,
    Actor,
}
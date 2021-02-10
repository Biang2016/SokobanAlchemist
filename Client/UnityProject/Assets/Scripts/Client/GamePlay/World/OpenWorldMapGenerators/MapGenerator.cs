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

    protected int Width;
    protected int Height;
    protected GridPos LeaveSpaceForPlayerBP;

    protected MapGenerator(OpenWorld.GenerateLayerData layerData, int width, int height, uint seed, GridPos leaveSpaceForPlayerBP)
    {
        SRandom = new SRandom(seed);
        GenerateLayerData = layerData;
        switch (layerData)
        {
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
        LeaveSpaceForPlayerBP = leaveSpaceForPlayerBP;
    }

    public virtual void ApplyGeneratorToWorldModuleData(WorldModuleData moduleData, int module_x, int module_z)
    {
    }

    protected void TryOverrideBoxInfoOnMap(WorldModuleData moduleData, ushort existedBoxTypeIndex, int x, int z, int module_x, int module_z)
    {
        switch (MapGeneratorType)
        {
            case MapGeneratorType.Box:
            {
                if (existedBoxTypeIndex != 0)
                {
                    if (GenerateLayerData.AllowReplacedBoxTypeNameSet.Contains(ConfigManager.GetBoxTypeName(existedBoxTypeIndex)))
                    {
                        moduleData.RawBoxMatrix[x, 0, z] = BoxTypeIndex;
                        moduleData.BoxMatrix[x, 0, z] = BoxTypeIndex;
                    }
                }
                else
                {
                    if (!GenerateLayerData.OnlyOverrideAnyBox)
                    {
                        moduleData.RawBoxMatrix[x, 0, z] = BoxTypeIndex;
                        moduleData.BoxMatrix[x, 0, z] = BoxTypeIndex;
                    }
                }

                break;
            }
            case MapGeneratorType.Actor:
            {
                if (existedBoxTypeIndex != 0)
                {
                    if (GenerateLayerData.AllowReplacedBoxTypeNameSet.Contains(ConfigManager.GetBoxTypeName(existedBoxTypeIndex)))
                    {
                        moduleData.RawBoxMatrix[x, 0, z] = 0;
                        moduleData.BoxMatrix[x, 0, z] = 0;
                    }
                }
                else
                {
                    if (!GenerateLayerData.OnlyOverrideAnyBox)
                    {
                        moduleData.RawBoxMatrix[x, 0, z] = 0;
                        moduleData.BoxMatrix[x, 0, z] = 0;
                    }
                }

                OpenWorld.GenerateActorLayerData actorLayerData = (OpenWorld.GenerateActorLayerData) GenerateLayerData;
                if (actorLayerData.ActorCategory == ActorCategory.Player)
                {
                    //moduleData.WorldModuleBornPointGroupData.PlayerBornPoints.Add(actorLayerData);
                }
                else
                {
                    ushort enemyIndex = ConfigManager.GetEnemyTypeIndex(actorLayerData.ActorTypeName);
                    moduleData.WorldModuleBornPointGroupData.EnemyBornPoints.Add(
                        new BornPointData
                        {
                            ActorType = actorLayerData.ActorTypeName,
                            LocalGP = new GridPos3D(x, 0, z),
                            BornPointAlias = "",
                            WorldGP = new GridPos3D(x, 0, z) + new GridPos3D(module_x, 1, module_z) * WorldModule.MODULE_SIZE
                        });
                }

                break;
            }
        }
    }
}

public enum MapGeneratorType
{
    Box,
    Actor,
    StaticLayout
}
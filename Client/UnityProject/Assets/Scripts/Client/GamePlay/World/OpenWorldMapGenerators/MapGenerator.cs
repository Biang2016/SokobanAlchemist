using BiangLibrary.GameDataFormat;

public abstract class MapGenerator
{
    protected uint Seed;
    protected SRandom SRandom;
    protected OpenWorld.GenerateBoxLayerData GenerateBoxLayerData;
    protected ushort BoxTypeIndex;

    protected int Width;
    protected int Height;

    protected MapGenerator(OpenWorld.GenerateBoxLayerData boxLayerData, int width, int height, uint seed)
    {
        SRandom = new SRandom(seed);
        GenerateBoxLayerData = boxLayerData;
        BoxTypeIndex = ConfigManager.GetBoxTypeIndex(boxLayerData.BoxTypeName);
        Width = width;
        Height = height;
    }

    public virtual void WriteMapInfoIntoWorldModuleData(WorldModuleData moduleData, int module_x, int module_z)
    {
    }

    protected void TryOverrideBoxInfoOnMap(WorldModuleData moduleData, ushort existedBoxTypeIndex, int x, int z)
    {
        if (existedBoxTypeIndex != 0)
        {
            if (GenerateBoxLayerData.AllowReplacedBoxTypeNameSet.Contains(ConfigManager.GetBoxTypeName(existedBoxTypeIndex)))
            {
                moduleData.BoxMatrix[x, 0, z] = BoxTypeIndex;
            }
        }
        else
        {
            if (!GenerateBoxLayerData.OnlyOverrideAnyBox)
            {
                moduleData.BoxMatrix[x, 0, z] = BoxTypeIndex;
            }
        }
    }
}
using BiangLibrary.GameDataFormat.Grid;

public sealed class AroundMapGenerator : MapGenerator
{
    public AroundMapGenerator(GenerateLayerData layerData, int width, int depth, uint seed, OpenWorld openWorld) : base(layerData, width, depth, seed, openWorld)
    {
    }

    public override void ApplyToWorldMap()
    {
        for (int world_x = 0; world_x < Width; world_x++)
        {
            TryOverrideToWorldMap(new GridPos3D(world_x, Height, 0), TypeIndex, (GridPosR.Orientation) SRandom.Range(0, 4));
            TryOverrideToWorldMap(new GridPos3D(world_x, Height, Depth - 1), TypeIndex, (GridPosR.Orientation) SRandom.Range(0, 4));
        }

        for (int world_z = 0; world_z < Depth; world_z++)
        {
            TryOverrideToWorldMap(new GridPos3D(0, Height, world_z), TypeIndex, (GridPosR.Orientation) SRandom.Range(0, 4));
            TryOverrideToWorldMap(new GridPos3D(Width - 1, Height, world_z), TypeIndex, (GridPosR.Orientation) SRandom.Range(0, 4));
        }
    }
}
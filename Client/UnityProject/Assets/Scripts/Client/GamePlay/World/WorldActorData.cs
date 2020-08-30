using BiangStudio.CloneVariant;
using BiangStudio.GameDataFormat.Grid;

public class WorldActorData : IClone<WorldActorData>
{
    public GridPos3D PlayerBornPoint;

    public WorldActorData Clone()
    {
        WorldActorData newData = new WorldActorData();
        newData.PlayerBornPoint = PlayerBornPoint;
        return newData;
    }
}
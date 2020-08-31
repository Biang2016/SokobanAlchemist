using BiangStudio.CloneVariant;
using BiangStudio.GameDataFormat.Grid;

public class WorldActorData : IClone<WorldActorData>
{
    public GridPos3D Player1BornPoint;
    public GridPos3D Player2BornPoint;

    public WorldActorData Clone()
    {
        WorldActorData newData = new WorldActorData();
        newData.Player1BornPoint = Player1BornPoint;
        newData.Player2BornPoint = Player2BornPoint;
        return newData;
    }
}
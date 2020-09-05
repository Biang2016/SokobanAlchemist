using System.Collections.Generic;
using BiangStudio.CloneVariant;

public class WorldActorData : IClone<WorldActorData>
{
    public List<BornPointData> BornPoints = new List<BornPointData>();

    public WorldActorData Clone()
    {
        WorldActorData newData = new WorldActorData();
        newData.BornPoints = BornPoints.Clone();
        return newData;
    }

    private BornPointData cachedPlayer1BornPoint;

    private BornPointData cachedPlayer2BornPoint;

    public BornPointData GetPlayerData(PlayerNumber playerNumber)
    {
        switch (playerNumber)
        {
            case PlayerNumber.Player1:
            {
                if (cachedPlayer1BornPoint == null)
                {
                    foreach (BornPointData bp in BornPoints)
                    {
                        if (bp.BornPointType == BornPointType.Player && bp.PlayerNumber == PlayerNumber.Player1)
                        {
                            cachedPlayer1BornPoint = bp;
                        }
                    }
                }

                return cachedPlayer1BornPoint;
            }
            case PlayerNumber.Player2:
            {
                if (cachedPlayer2BornPoint == null)
                {
                    foreach (BornPointData bp in BornPoints)
                    {
                        if (bp.BornPointType == BornPointType.Player && bp.PlayerNumber == PlayerNumber.Player2)
                        {
                            cachedPlayer2BornPoint = bp;
                        }
                    }
                }

                return cachedPlayer2BornPoint;
            }
        }

        return null;
    }
}
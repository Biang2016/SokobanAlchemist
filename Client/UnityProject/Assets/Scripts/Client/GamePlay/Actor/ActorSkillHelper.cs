using System.Collections.Generic;

public class ActorSkillHelper : ActorHelper
{
    public override void OnRecycled()
    {
        base.OnRecycled();
    }

    public void Initialize()
    {
        foreach (string boxName in Actor.PushableBoxList)
        {
            byte boxIndex = ConfigManager.GetBoxTypeIndex(boxName);
            PushableBoxSet.Add(boxIndex);
        }

        foreach (string boxName in Actor.KickableBoxList)
        {
            byte boxIndex = ConfigManager.GetBoxTypeIndex(boxName);
            KickableBoxSet.Add(boxIndex);
        }

        foreach (string boxName in Actor.LiftableBoxList)
        {
            byte boxIndex = ConfigManager.GetBoxTypeIndex(boxName);
            LiftableBoxSet.Add(boxIndex);
        }
    }

    public byte CurrentGetKickAbility = 0;

    public HashSet<byte> PushableBoxSet = new HashSet<byte>();
    public HashSet<byte> KickableBoxSet = new HashSet<byte>();
    public HashSet<byte> LiftableBoxSet = new HashSet<byte>();
}
using System;
using System.Collections.Generic;
using UnityEngine.Events;

public class ActorSkillHelper : ActorMonoHelper
{
    public override void OnRecycled()
    {
        InteractSkillDict.Clear();
        base.OnRecycled();
    }

    private SortedDictionary<byte, InteractSkillType> InteractSkillDict = new SortedDictionary<byte, InteractSkillType>();

    public byte PlayerCurrentGetKickAbility = 0;

    public void Initialize()
    {
        foreach (KeyValuePair<byte, string> kv in ConfigManager.BoxTypeDefineDict.TypeNameDict)
        {
            InteractSkillDict.Add(kv.Key, 0);
        }

        foreach (string boxName in Actor.PushableBoxList)
        {
            byte boxTypeIndex = ConfigManager.GetBoxTypeIndex(boxName);
            InteractSkillDict[boxTypeIndex] |= InteractSkillType.Push;
            if (Actor.IsPlayer) ClientGameManager.Instance.BattleMessenger.Broadcast((uint) Enum_Events.OnPlayerInteractSkillChanged, InteractSkillDict[boxTypeIndex], boxTypeIndex);
        }

        foreach (string boxName in Actor.KickableBoxList)
        {
            byte boxTypeIndex = ConfigManager.GetBoxTypeIndex(boxName);
            InteractSkillDict[boxTypeIndex] |= InteractSkillType.Kick;
            if (Actor.IsPlayer) ClientGameManager.Instance.BattleMessenger.Broadcast((uint) Enum_Events.OnPlayerInteractSkillChanged, InteractSkillDict[boxTypeIndex], boxTypeIndex);
        }

        foreach (string boxName in Actor.LiftableBoxList)
        {
            byte boxTypeIndex = ConfigManager.GetBoxTypeIndex(boxName);
            InteractSkillDict[boxTypeIndex] |= InteractSkillType.Lift;
            if (Actor.IsPlayer) ClientGameManager.Instance.BattleMessenger.Broadcast((uint) Enum_Events.OnPlayerInteractSkillChanged, InteractSkillDict[boxTypeIndex], boxTypeIndex);
        }

        foreach (string boxName in Actor.ThrowableBoxList)
        {
            byte boxTypeIndex = ConfigManager.GetBoxTypeIndex(boxName);
            InteractSkillDict[boxTypeIndex] |= InteractSkillType.Throw;
            if (Actor.IsPlayer) ClientGameManager.Instance.BattleMessenger.Broadcast((uint) Enum_Events.OnPlayerInteractSkillChanged, InteractSkillDict[boxTypeIndex], boxTypeIndex);
        }
    }

    public InteractSkillType GetInteractSkillType(byte boxTypeIndex)
    {
        return InteractSkillDict[boxTypeIndex];
    }

    public bool CanInteract(InteractSkillType interactType, byte boxTypeIndex)
    {
        if (!InteractSkillDict.ContainsKey(boxTypeIndex)) return false;
        return InteractSkillDict[boxTypeIndex].HasFlag(interactType);
    }

    public void EnableInteract(InteractSkillType interactType, byte boxTypeIndex)
    {
        if (!InteractSkillDict.ContainsKey(boxTypeIndex)) return;
        if (!InteractSkillDict[boxTypeIndex].HasFlag(interactType))
        {
            InteractSkillDict[boxTypeIndex] |= interactType;
            if (Actor.IsPlayer) ClientGameManager.Instance.BattleMessenger.Broadcast((uint) Enum_Events.OnPlayerInteractSkillChanged, InteractSkillDict[boxTypeIndex], boxTypeIndex);
        }
    }

    public void DisableInteract(InteractSkillType interactType, byte boxTypeIndex)
    {
        if (!InteractSkillDict.ContainsKey(boxTypeIndex)) return;
        if (InteractSkillDict[boxTypeIndex].HasFlag(interactType))
        {
            InteractSkillDict[boxTypeIndex] -= interactType;
            if (Actor.IsPlayer) ClientGameManager.Instance.BattleMessenger.Broadcast((uint) Enum_Events.OnPlayerInteractSkillChanged, InteractSkillDict[boxTypeIndex], boxTypeIndex);
        }
    }
}

[Flags]
public enum InteractSkillType
{
    None = 0,
    Push = 1 << 0,
    Kick = 1 << 1,
    Lift = 1 << 2,
    Throw = 1 << 3,
}

public static class InteractSkillTypeExtension
{
    public static BoxFeature ConvertToBoxFeature(this InteractSkillType interactSkillType)
    {
        switch (interactSkillType)
        {
            case InteractSkillType.Push:
            {
                return BoxFeature.Pushable;
            }
            case InteractSkillType.Kick:
            {
                return BoxFeature.Kickable;
            }
            case InteractSkillType.Lift:
            {
                return BoxFeature.Liftable;
            }
            case InteractSkillType.Throw:
            {
                return BoxFeature.Throwable;
            }
        }

        return 0;
    }

    public static BoxSkinHelper.BoxModelType ConvertToBoxModelType(this InteractSkillType interactSkillType)
    {
        if (interactSkillType.HasFlag(InteractSkillType.Kick))
        {
            return BoxSkinHelper.BoxModelType.Rounded;
        }
        else
        {
            return BoxSkinHelper.BoxModelType.Box;
        }
    }
}
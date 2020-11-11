using System;
using System.Collections.Generic;
using UnityEngine.Events;

public class ActorSkillHelper : ActorMonoHelper
{
    public override void OnHelperRecycled()
    {
        InteractSkillDict.Clear();
        base.OnHelperRecycled();
    }

    private SortedDictionary<ushort, InteractSkillType> InteractSkillDict = new SortedDictionary<ushort, InteractSkillType>();

    public ushort PlayerCurrentGetKickAbility = 0;

    public void Initialize()
    {
        foreach (KeyValuePair<ushort, string> kv in ConfigManager.BoxTypeDefineDict.TypeNameDict)
        {
            InteractSkillDict.Add(kv.Key, 0);
        }

        foreach (string boxName in Actor.PushableBoxList)
        {
            ushort boxTypeIndex = ConfigManager.GetBoxTypeIndex(boxName);
            InteractSkillDict[boxTypeIndex] |= InteractSkillType.Push;
            if (Actor.IsPlayer) ClientGameManager.Instance.BattleMessenger.Broadcast((uint) Enum_Events.OnPlayerInteractSkillChanged, InteractSkillDict[boxTypeIndex], boxTypeIndex);
        }

        foreach (string boxName in Actor.KickableBoxList)
        {
            ushort boxTypeIndex = ConfigManager.GetBoxTypeIndex(boxName);
            InteractSkillDict[boxTypeIndex] |= InteractSkillType.Kick;
            if (Actor.IsPlayer) ClientGameManager.Instance.BattleMessenger.Broadcast((uint) Enum_Events.OnPlayerInteractSkillChanged, InteractSkillDict[boxTypeIndex], boxTypeIndex);
        }

        foreach (string boxName in Actor.LiftableBoxList)
        {
            ushort boxTypeIndex = ConfigManager.GetBoxTypeIndex(boxName);
            InteractSkillDict[boxTypeIndex] |= InteractSkillType.Lift;
            if (Actor.IsPlayer) ClientGameManager.Instance.BattleMessenger.Broadcast((uint) Enum_Events.OnPlayerInteractSkillChanged, InteractSkillDict[boxTypeIndex], boxTypeIndex);
        }

        foreach (string boxName in Actor.ThrowableBoxList)
        {
            ushort boxTypeIndex = ConfigManager.GetBoxTypeIndex(boxName);
            InteractSkillDict[boxTypeIndex] |= InteractSkillType.Throw;
            if (Actor.IsPlayer) ClientGameManager.Instance.BattleMessenger.Broadcast((uint) Enum_Events.OnPlayerInteractSkillChanged, InteractSkillDict[boxTypeIndex], boxTypeIndex);
        }
    }

    public InteractSkillType GetInteractSkillType(ushort boxTypeIndex)
    {
        return InteractSkillDict[boxTypeIndex];
    }

    public bool CanInteract(InteractSkillType interactType, ushort boxTypeIndex)
    {
        if (!InteractSkillDict.ContainsKey(boxTypeIndex)) return false;
        return InteractSkillDict[boxTypeIndex].HasFlag(interactType);
    }

    public void EnableInteract(InteractSkillType interactType, ushort boxTypeIndex)
    {
        if (!InteractSkillDict.ContainsKey(boxTypeIndex)) return;
        if (!InteractSkillDict[boxTypeIndex].HasFlag(interactType))
        {
            InteractSkillDict[boxTypeIndex] |= interactType;
            if (Actor.IsPlayer) ClientGameManager.Instance.BattleMessenger.Broadcast((uint) Enum_Events.OnPlayerInteractSkillChanged, InteractSkillDict[boxTypeIndex], boxTypeIndex);
        }
    }

    public void DisableInteract(InteractSkillType interactType, ushort boxTypeIndex)
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
}
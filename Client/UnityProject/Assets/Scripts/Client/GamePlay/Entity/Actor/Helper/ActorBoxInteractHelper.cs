using System;
using System.Collections.Generic;

public class ActorBoxInteractHelper : ActorMonoHelper
{
    public override void OnHelperRecycled()
    {
        InteractSkillDict.Clear();
        base.OnHelperRecycled();
    }

    private SortedDictionary<ushort, InteractSkillType> InteractSkillDict = new SortedDictionary<ushort, InteractSkillType>();

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
        }

        foreach (string boxName in Actor.KickableBoxList)
        {
            ushort boxTypeIndex = ConfigManager.GetBoxTypeIndex(boxName);
            InteractSkillDict[boxTypeIndex] |= InteractSkillType.Kick;
        }

        foreach (string boxName in Actor.LiftableBoxList)
        {
            ushort boxTypeIndex = ConfigManager.GetBoxTypeIndex(boxName);
            InteractSkillDict[boxTypeIndex] |= InteractSkillType.Lift;
        }

        foreach (string boxName in Actor.ThrowableBoxList)
        {
            ushort boxTypeIndex = ConfigManager.GetBoxTypeIndex(boxName);
            InteractSkillDict[boxTypeIndex] |= InteractSkillType.Throw;
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

    // 废弃原因：所有箱子初始化时注册事件太耗了，如果还想启用，未来可以主动刷新所有箱子，或用从游戏设计上避免技能实时变化
    //public void EnableInteract(InteractSkillType interactType, ushort boxTypeIndex)
    //{
    //    if (!InteractSkillDict.ContainsKey(boxTypeIndex)) return;
    //    if (!InteractSkillDict[boxTypeIndex].HasFlag(interactType))
    //    {
    //        InteractSkillDict[boxTypeIndex] |= interactType;
    //    }
    //}

    //public void DisableInteract(InteractSkillType interactType, ushort boxTypeIndex)
    //{
    //    if (!InteractSkillDict.ContainsKey(boxTypeIndex)) return;
    //    if (InteractSkillDict[boxTypeIndex].HasFlag(interactType))
    //    {
    //        InteractSkillDict[boxTypeIndex] -= interactType;
    //    }
    //}
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
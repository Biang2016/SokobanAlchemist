using BiangLibrary.CloneVariant;
using UnityEngine;

public class UIBattleTipInfo : IClone<UIBattleTipInfo>
{
    public uint HitMCB_GUID;
    public BattleTipType BattleTipType;
    public Camp ReceiverCamp;
    public int DiffValue;
    public string ExtraStr_Before;
    public string ExtraStr_After;
    public float Scale;
    public string SpriteImagePath;
    public Vector3 StartPos;
    public Vector2 Offset;
    public Vector2 RandomRange;
    public float DisappearTime = 1.5f;

    public UIBattleTipInfo(uint hitMcbGuid, BattleTipType battleTipType, Camp receiverCamp, int diffValue, string extraStr_Before, string extraStr_After, float scale,
        string spriteImagePath, Vector3 startPos, Vector2 offset, Vector2 randomRange, float disappearTime)
    {
        HitMCB_GUID = hitMcbGuid;
        BattleTipType = battleTipType;
        ReceiverCamp = receiverCamp;
        DiffValue = diffValue;
        ExtraStr_Before = extraStr_Before;
        ExtraStr_After = extraStr_After;
        Scale = scale;
        SpriteImagePath = spriteImagePath;
        StartPos = startPos;
        Offset = offset;
        RandomRange = randomRange;
        DisappearTime = disappearTime;
    }

    public UIBattleTipInfo Clone()
    {
        return new UIBattleTipInfo(HitMCB_GUID, BattleTipType, ReceiverCamp, DiffValue, ExtraStr_Before, ExtraStr_After, Scale, SpriteImagePath, StartPos, Offset, RandomRange, DisappearTime);
    }
}